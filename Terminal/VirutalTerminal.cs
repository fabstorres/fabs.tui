using System.Text;
using System.Text.Unicode;
using System.Threading.Channels;

namespace Fabs.Tui.Terminal;

public interface IVirtualTerminal
{
    public void Start(Action<string> onData);
    public void Stop();

    public void HideCursor();
    public void ShowCursor();

    public void Write(string data);

    public int Columns { get; }
    public int Rows { get; }
}

public class ProcessTerminal : IVirtualTerminal
{
    private ITerminalDriver? _driver;
    private CancellationTokenSource? _cts;
    private readonly Channel<string> _inputChannel = Channel.CreateUnbounded<string>();
    private readonly Decoder _decoder = Encoding.UTF8.GetDecoder();
    public void Start(Action<string> onData)
    {
        if (!(OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()))
        {
            throw new PlatformNotSupportedException("Windows is not supported");
        }
        else
        {
            _driver = new UnixTerminalDriver();
        }
        _driver.EnableRawMode();
        _driver.Write(Encoding.UTF8.GetBytes("\x1b[?2004h"));
        HideCursor();

        _cts = new CancellationTokenSource();
        Task.Run(() => ReadLoop(_cts.Token));
        Task.Run(() => DispatchLoop(onData, _cts.Token));
    }

    private void ReadLoop(CancellationToken ct)
    {
        var buffer = new byte[1024];
        while (!ct.IsCancellationRequested)
        {
            var n = _driver!.Read(buffer);
            if (n <= 0) break;

            var chunk = buffer[0..n];
            var charCount = _decoder.GetCharCount(chunk, false);
            var chars = new char[charCount];
            _decoder.GetChars(chunk, chars, false);
            _inputChannel.Writer.TryWrite(new string(chars));
        }
    }

    private async Task DispatchLoop(Action<string> onData, CancellationToken ct)
    {
        await foreach (var chunk in _inputChannel.Reader.ReadAllAsync(ct))
            onData(chunk);
    }

    public void Write(string data)
    {
        _driver?.Write(Encoding.UTF8.GetBytes(data));
    }

    public void Stop()
    {
        _cts?.Cancel();
        _driver?.DisableRawMode();
        _driver?.Write(Encoding.UTF8.GetBytes("\x1b[?2004l")); // disable bracketed paste
        ShowCursor();
    }

    public void HideCursor()
    {
        _driver?.Write(Encoding.UTF8.GetBytes("\x1b[?25l"));
    }

    public void ShowCursor()
    {
        _driver?.Write(Encoding.UTF8.GetBytes("\x1b[?25h"));
    }

    public int Columns => Console.WindowWidth;
    public int Rows => Console.WindowHeight;
}
