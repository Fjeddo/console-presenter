using QRCoder;
using Spectre.Console;
using System.Text;

Console.WriteLine("Ange sträng att qr-koda, avsluta med <ENTER>");
var s = Console.ReadLine();

Console.OutputEncoding = Encoding.UTF8;

var generator = new QRCodeGenerator();

var data = generator.CreateQrCode(s!, QRCodeGenerator.ECCLevel.L);

var modules = data.ModuleMatrix;

var sb = new StringBuilder();

for (var y = 0; y < modules.Count; y += 2)
{
    for (var x = 0; x < modules.Count; x++)
    {
        var top = modules[y][x];

        var bottom =
            y + 1 < modules.Count &&
            modules[y + 1][x];

        // QR: svart = fylld modul
        sb.Append((top, bottom) switch
        {
            (true, true) =>
                "[black on white]█[/]",

            (true, false) =>
                "[black on white]▀[/]",

            (false, true) =>
                "[black on white]▄[/]",

            _ =>
                "[white on white] [/]"

        });
    }

    sb.AppendLine();
}

AnsiConsole.Write(new Markup(sb.ToString()));

Console.WriteLine("---------------");
Console.WriteLine("Klart!");
