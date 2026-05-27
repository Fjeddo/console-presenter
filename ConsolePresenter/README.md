# ConsolePresenter

A terminal-based slide presentation tool that renders `.txt` slide files in the console. Supports a synchronized notes window, a countdown timer, and keyboard-driven navigation.

## Slide file convention

Slides are plain text files placed in a directory and must follow the naming convention:

```
01. Title of slide.txt
02. Another slide.txt
...
```

Notes for a slide are stored in a companion file with the same name but a `.notes.txt` suffix, e.g. `01. Title of slide.notes.txt`.

## Usage

```
ConsolePresenter [directory] [options]
```

### Arguments

| Argument | Description |
|---|---|
| `directory` | Path to the directory containing the slide files. Defaults to the current working directory if omitted. |

### Options

| Option | Value | Description |
|---|---|---|
| `--max-minutes` | `<minutes>` | Sets a countdown timer for the presentation (e.g. `--max-minutes 30`). |
| `--notes` | _(flag)_ | Launches the presenter in notes mode. This is normally handled automatically by the main presenter window and does not need to be passed manually. |
| `--parent` | `<pid>` | PID of the parent presenter process. Used internally to keep the notes window in sync. |

## Keyboard shortcuts

| Key | Action |
|---|---|
| `→` / `↓` | Next slide |
| `←` / `↑` | Previous slide |
| `1`–`9` + `Enter` | Jump to a specific slide number |
| `T` | Toggle the presentation timer |
| `R` | Reset the presentation timer |
| `Q` | Quit |

## Example

```powershell
# Run a presentation from a specific folder with a 45-minute timer
ConsolePresenter C:\Presentations\MyTalk --max-minutes 45
```
