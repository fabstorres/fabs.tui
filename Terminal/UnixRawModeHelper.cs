
using System.Runtime.InteropServices;

namespace Fabs.Tui.Terminal;

internal sealed unsafe partial class UnixRawModeHelper : IDisposable
{
    private Termios _originalTermios;
    private int _fd = -1;

    public bool IsRawModeEnabled { get; private set; }

    public bool EnableRawMode()
    {
        if (IsRawModeEnabled) return IsRawModeEnabled;
        if (OperatingSystem.IsWindows()) return false;

        try
        {
            int fd = open("/dev/tty", O_RDWR);
            if (fd < 0)
            {
                Console.WriteLine("No Terminal Device Detected. Failed to enable raw mode.");
                return false;
            }
            _fd = fd;
            int result = tcgetattr(fd, out _originalTermios);

            if (result != 0)
            {
                int errno = Marshal.GetLastWin32Error();
                Console.WriteLine($"Failed to get terminal attributes: {errno}");
                return false;
            }

            Termios raw = _originalTermios;

            try
            {
                // Try using cfmakeraw if available (cleaner, platform-specific implementation)
                cfmakeraw_ref(ref raw);
            }
            catch (EntryPointNotFoundException)
            {
                // Manually configure raw mode if cfmakeraw not available
                // This is equivalent to cfmakeraw's behavior
                raw.c_iflag &= ~(BRKINT | ICRNL | INPCK | ISTRIP | IXON);
                raw.c_oflag &= ~OPOST;
                raw.c_cflag |= CS8;
                raw.c_lflag &= ~(ECHO | ICANON | IEXTEN | ISIG);
            }

            // Apply raw mode settings
            result = tcsetattr(_fd, TCSANOW, ref raw);

            if (result != 0)
            {
                int errno = Marshal.GetLastWin32Error();
                Console.WriteLine($"tcsetattr failed (errno={errno}). Cannot enable raw mode.");

                return false;
            }

            IsRawModeEnabled = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }

        return true;
    }

    public void Dispose()
    {

    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Termios
    {
        public ulong c_iflag;
        public ulong c_oflag;
        public ulong c_cflag;
        public ulong c_lflag;
        public fixed byte c_cc[20];
        public ulong c_ispeed;
        public ulong c_ospeed;
    }

    private const int STDIN_FILENO = 0;
    private const int TCSANOW = 0;
    private const uint BRKINT = 0x00000002;
    private const uint ICRNL = 0x00000100;
    private const uint INPCK = 0x00000010;
    private const uint ISTRIP = 0x00000020;
    private const uint IXON = 0x00000400;
    private const uint OPOST = 0x00000001;
    private const uint CS8 = 0x00000030;
    private const uint ECHO = 0x00000008;
    private const uint ICANON = 0x00000100;
    private const uint IEXTEN = 0x00008000;
    private const uint ISIG = 0x00000001;

    [LibraryImport("libc", SetLastError = true)]
    private static partial int tcgetattr(int fd, out Termios termios);

    [LibraryImport("libc", SetLastError = true)]
    private static partial int tcsetattr(int fd, int optional_actions, ref Termios termios);

    [LibraryImport("libc", EntryPoint = "cfmakeraw", SetLastError = false)]
    private static partial void cfmakeraw_ref(ref Termios termios);

    private const int O_RDWR = 2;
    private const int O_NOCTTY = 0x100;

    [LibraryImport("libc", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int open(string path, int oflag);

    [LibraryImport("libc", SetLastError = true)]
    private static partial int close(int fd);
}
