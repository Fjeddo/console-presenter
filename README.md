# console-presenter

A terminal-based presentation suite built with .NET 10. The repository contains two tools:

- **[ConsolePresenter](ConsolePresenter/README.md)** – a keyboard-driven slide presenter that renders `.txt` files in the terminal, with a synchronised notes window and an optional countdown timer.
- **[QrCode.Ascii.Generator](QrCode.Ascii.Generator/README.md)** – an interactive tool that encodes a string as a QR code and renders it in the terminal using Unicode block characters.

## Repository structure

```
console-presenter/
├── ConsolePresenter/          # Slide presentation tool
│   └── README.md
├── QrCode.Ascii.Generator/    # ASCII QR code generator
│   └── README.md
└── README.md                  # This file
```

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Build

```powershell
dotnet build
```

### Run ConsolePresenter

```powershell
dotnet run --project ConsolePresenter -- C:\Path\To\Slides --max-minutes 45
```

See [ConsolePresenter/README.md](ConsolePresenter/README.md) for full usage and keyboard shortcuts.

### Run QrCode.Ascii.Generator

```powershell
dotnet run --project QrCode.Ascii.Generator
```

See [QrCode.Ascii.Generator/README.md](QrCode.Ascii.Generator/README.md) for full usage.
