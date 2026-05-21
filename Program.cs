using System.Text;
using Fabs.Tui.Terminal;

var terminal = new UnixTerminal();
terminal.EnableRawMode();

var buffer = new byte[1024];

terminal.Write(Encoding.UTF8.GetBytes("Press 'q' to quit.\r\n"));

while (true)
{
    var n = terminal.Read(buffer);
    if (n <= 0) break;
    if (buffer[0] == 'q') break;
    terminal.Write(buffer);
}

terminal.DisableRawMode();
