# fabs.tui

> An experiment TUI for the microcode project.

This is a small, experimental terminal user interface library built in C# with raw Unix terminal control via `libc` P/Invoke. It's part of the broader **fabs.stack** / microcode ecosystem.

## Status

**Experimental.** This is a playground for low-level terminal manipulation — raw mode, termios, non-canonical input, and direct output. Not intended for production use.

## What's inside

- **`Terminal/`** — Unix raw-mode helpers and an `ITerminal` abstraction
  - `UnixTerminal` — wraps `read`/`write` syscalls on `/dev/tty`
  - `UnixRawModeHelper` — enables/disables raw mode via `tcgetattr`/`tcsetattr`/`cfmakeraw`
- **`Program.cs`** — a minimal echo loop that exits on `q`

## Requirements

- .NET 10.0
- macOS (currently targets OSX; Linux support is planned)

## Usage

```bash
dotnet run
```

Press `q` to quit. All other input is echoed back.

## License

MIT — see [LICENSE](LICENSE).
