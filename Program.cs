using System.Text;
using Fabs.Tui.Terminal;
// Console.WriteLine("Before enabling raw mode.");
var unixHelper = new UnixRawModeHelper();
var terminal = new UnixTerminal();

var result = unixHelper.EnableRawMode();
// Remove the usage of Console
// if (!result) Console.Out.Write("Failed to enable raw mode.\r\n");
// else Console.Out.Write("Raw mode enabled.\r\n");

var buffer = new byte[1024];

terminal.Write(Encoding.UTF8.GetBytes("Press 'q' to quit.\r\n"));

while (true)
{
    var n = terminal.Read(buffer);
    if (n <= 0) break;
    if (buffer[0] == 'q') break;
}
