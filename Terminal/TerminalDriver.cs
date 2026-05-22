
using System.Runtime.InteropServices;
using System.Text;

namespace Fabs.Tui.Terminal;

public interface ITerminalDriver
{
    bool EnableRawMode();
    void DisableRawMode();
    int Read(Span<byte> buffer);
    void Write(ReadOnlySpan<byte> buffer);
    (int rows, int columns) GetWindowSize();
}


public unsafe partial class UnixTerminalDriver : ITerminalDriver
{
    private readonly UnixRawModeHelper _rawModeHelper = new();
    public bool EnableRawMode()
    {
        return _rawModeHelper.EnableRawMode();
    }

    public void DisableRawMode()
    {
        _rawModeHelper.Dispose();
    }

    public int Read(Span<byte> buffer)
    {

        fixed (byte* ptr = buffer)
        {
            return read(_rawModeHelper.TermiosFD, ptr, (uint)buffer.Length);
        }
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        fixed (byte* ptr = buffer)
        {
            nint written = write(1, ptr, (uint)buffer.Length);
            if (written < 0)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
            }
        }
    }

    [LibraryImport("libc", SetLastError = true)]
    private static partial int write(int fd, byte* buffer, uint count);

    [LibraryImport("libc", SetLastError = true)]
    private static partial int read(int fd, byte* buffer, uint count);

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WinSize
    {
        public ushort ws_row;
        public ushort ws_col;
        public ushort ws_xpixel;
        public ushort ws_ypixel;
    }

    [LibraryImport("libc", SetLastError = true)]
    private static partial int ioctl(
        int fd,
        ulong request,
        out WinSize ws);

    /// <remarks>
    /// Currently unstable do not rely on this method.
    /// </remarks>
    public (int rows, int columns) GetWindowSize()
    {
        var result = ioctl(_rawModeHelper.TermiosFD, 0x5413, out var ws);
        if (result < 0)
        {
            Write(Encoding.UTF8.GetBytes($"ioctl failed: {Marshal.GetLastPInvokeError()}\r\n"));
            //throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
            return (24, 80);
        }
        return (ws.ws_row, ws.ws_col);
    }
}
