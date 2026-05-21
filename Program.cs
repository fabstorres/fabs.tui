
using Fabs.Tui.Terminal;

var terminal = new ProcessTerminal();
terminal.Start(terminal.Write);

terminal.Write("Hello, World!\r\n");

terminal.Stop();
