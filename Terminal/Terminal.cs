
using System.Runtime.InteropServices;

namespace Fabs.Tui.Terminal;

public interface ITerminal
{
    bool EnableRawMode();
    void DisableRawMode();
    int Read(Span<byte> buffer);
    void Write(ReadOnlySpan<byte> buffer);
}


public unsafe partial class UnixTerminal : ITerminal
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
}
