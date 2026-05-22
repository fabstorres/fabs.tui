
using Fabs.Tui.Terminal;

var terminal = new ProcessTerminal();
terminal.Start(terminal.Write);
Thread.Sleep(1000);
terminal.Write("hello, world! " + terminal.Columns + "x" + terminal.Rows + "\r\n");

terminal.Stop();
