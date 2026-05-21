using Fabs.Tui.Terminal;
Console.WriteLine("Before enabling raw mode.");
var unixHelper = new UnixRawModeHelper();

var result = unixHelper.EnableRawMode();
// Remove the usage of Console
if (!result) Console.Out.Write("Failed to enable raw mode.\r\n");
else Console.Out.Write("Raw mode enabled.\r\n");

Thread.Sleep(5000);
