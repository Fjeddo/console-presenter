# QrCode.Ascii.Generator

A simple interactive console tool that generates an ASCII/Unicode QR code from a string and renders it directly in the terminal using Spectre.Console.

## Usage

```
QrCode.Ascii.Generator
```

The tool has no command-line arguments. When started it prompts you to enter the string you want to encode:

```
Ange sträng att qr-koda, avsluta med <ENTER>
```

Type the text you want to encode and press **Enter**. The QR code is rendered in the terminal using block characters (`█`, `▀`, `▄`) with a black-on-white colour scheme.

## Example

```powershell
# Start the generator
QrCode.Ascii.Generator

# When prompted, enter the text to encode, e.g.:
# https://github.com/Fjeddo/console-presenter
```

The resulting QR code is printed to the terminal and can be scanned with any standard QR code reader.

## Error correction level

The generator uses ECC level **L** (Low, ~7% error correction), which produces a smaller/denser QR code suited for clean terminal output.
