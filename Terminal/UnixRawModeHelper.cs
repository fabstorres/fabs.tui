
using System.Runtime.InteropServices;

namespace Fabs.Tui.Terminal;

#if OSX
using cc_t = byte;
using tc_flagt = ulong;
using tc_speedt = ulong;
#else
using cc_t = uint;
using tc_flagt = uint;
using tc_speedt = uint;
#endif

internal sealed unsafe partial class UnixRawModeHelper : IDisposable
{
    private Termios _originalTermios;
    private int _fd = -1;

    public bool IsRawModeEnabled { get; private set; }
    public int TermiosFD => _fd;
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
        if (!IsRawModeEnabled) return;

        tcsetattr(_fd, TCSANOW, ref _originalTermios);
        IsRawModeEnabled = false;
    }



    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Termios
    {
        public tc_flagt c_iflag;
        public tc_flagt c_oflag;
        public tc_flagt c_cflag;
        public tc_flagt c_lflag;
#if OSX
        public fixed cc_t c_cc[20];
#else
        public fixed cc_t c_cc[32];
#endif
        public tc_speedt c_ispeed;
        public tc_speedt c_ospeed;
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

}
