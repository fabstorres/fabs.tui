
using System.Runtime.InteropServices;

namespace Fabs.Tui.Terminal;

public interface ITerminal
{
    int Read(Span<byte> buffer);
    void Write(ReadOnlySpan<byte> buffer);
}


public unsafe partial class UnixTerminal : ITerminal
{
    public int Read(Span<byte> buffer)
    {
        var fd = open("/dev/tty", 0);
        if (fd < 0) return 0;

        fixed (byte* ptr = buffer)
        {
            return read(fd, ptr, (uint)buffer.Length);
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

    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int open(string path, int oflag);

    [LibraryImport("libc", SetLastError = true)]
    private static partial int write(int fd, byte* buffer, uint count);

    [LibraryImport("libc", SetLastError = true)]
    private static partial int read(int fd, byte* buffer, uint count);
}
