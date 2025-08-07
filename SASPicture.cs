using NAudio.Midi;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SASTester;

partial class SASTester
{
    private readonly List<string> addlStrFiles =
    [
        "0",
        "1",
        "A",
        "B",
        "DIR",
        "NEWDATA",
        "SAVED",
        "VOLT"
    ];

    public void ShowPicFiles(string abbrev)
    {
        var fileFound = false;
        Console.WriteLine(FilePromptWarn);
        var files = GetPicFiles(abbrev);
        if (files.Count == 0)
        {
            Console.WriteLine("No picture files found.");
            Console.WriteLine(Divider);
            return;
        }
        foreach (var file in files)
        {
            Console.WriteLine(file);
            fileFound = true;
        }
        if (!fileFound)
            return;
        string? input;
        do
        {
            Console.Write(FilePrompt);
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                var filePath = "";
                if (pcType == AtariStAbb)
                    filePath = Path.Combine(RscPath, abbrev + pcType, abbrev, input);
                else
                    filePath = Path.Combine(RscPath, abbrev + pcType, input);
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(RscPath, abbrev, input);
                    if (!File.Exists(filePath))
                    {
                        Console.Error.WriteLine($"{FileError} {filePath}");
                        break;
                    }
                }
                DrawPic(abbrev, filePath, toFile: false);
            }
            else
                Console.WriteLine(Divider);
        }
        while (!string.IsNullOrEmpty(input));
    }

    public List<string> GetPicFiles(string abbrev)
    {
        List<string> picFiles = [];
        List<string> locNames = [];
        foreach (var loc in GetLocs(abbrev))
        {
            locNames.Add(loc.Value.Item2);
        }
        try
        {
            if (Directory.Exists(Path.Combine(RscPath, abbrev + pcType)))
            {
                foreach (var file in Directory.EnumerateFiles(Path.Combine(RscPath, abbrev + pcType), "*.").ToList())
                {
                    var filename = Path.GetFileName(file);
                    if (!locNames.Contains(filename, StringComparer.OrdinalIgnoreCase) &&
                        !addlStrFiles.Contains(filename, StringComparer.OrdinalIgnoreCase) &&
                        !abbrev.Equals(filename, StringComparison.OrdinalIgnoreCase))
                    {
                        picFiles.Add(filename);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        return picFiles;
    }

    private void ExportPicFiles()
    {
        if (pcType != IbmAbb)
            return;
        Console.WriteLine("Please wait ...");
        foreach (var abbrev in new[] { AmazonAbb, DragonAbb, F451Abb, AmberAbb, PerryAbb, RamaAbb, IslandAbb, OzAbb })
        {
            var picFiles = GetPicFiles(abbrev);
            foreach (var file in picFiles)
            {
                DrawPic(abbrev, Path.Combine(RscPath, abbrev + pcType, file), toFile: true);
            }
        }
    }

    private static ConsoleColor GetCColor(ushort c, ushort palette)
    {
        var color = ConsoleColor.Black;
        if (palette == 0)
        {
            switch (c)
            {
                case 0:
                    color = ConsoleColor.Black;
                    break;
                case 1:
                    color = ConsoleColor.DarkGreen;
                    break;
                case 2:
                    color = ConsoleColor.DarkRed;
                    break;
                case 3:
                    color = ConsoleColor.DarkYellow;
                    break;
            }
        }
        else if (palette == 1)
        {
            switch (c)
            {
                case 0:
                    color = ConsoleColor.Black;
                    break;
                case 1:
                    color = ConsoleColor.DarkCyan;
                    break;
                case 2:
                    color = ConsoleColor.DarkMagenta;
                    break;
                case 3:
                    color = ConsoleColor.Gray;
                    break;
            }
        }
        return color;
    }

    private static (byte, byte, byte) GetRGBColor(ushort c, ushort palette)
    {
        (byte, byte, byte) color = (0x00, 0x00, 0x00);
        if (palette == 0)
        {
            switch (c)
            {
                case 0:
                    color = (0x00, 0x00, 0x00); // black
                    break;
                case 1:
                    color = (0x00, 0xAA, 0x00); // green
                    break;
                case 2:
                    color = (0xAA, 0x00, 0x00); // red
                    break;
                case 3:
                    color = (0xAA, 0xAA, 0x00); // yellow
                    break;
            }
        }
        else if (palette == 1)
        {
            switch (c)
            {
                case 0:
                    color = (0x00, 0x00, 0x00); // black
                    break;
                case 1:
                    color = (0x00, 0xAA, 0xAA); // cyan
                    break;
                case 2:
                    color = (0xAA, 0x00, 0xAA); // magenta
                    break;
                case 3:
                    color = (0xAA, 0xAA, 0xAA); // white
                    break;
            }
        }
        return color;
    }

    // IBM only for now
    // The CGA medium-resolution graphics mode used here for PC is 320x200 x 4-color, with 2 possible palettes in mode 4
    // TODO: Figure out format for other ports
    private static void DrawPic(string abbrev, string filePath = "", bool toFile = false)
    {
        const byte Offset = 0x06; // The first 6 bytes have palette colors and height/width
        ushort x = 0, y = 0, w = 0, h = 0, palette = 0;
        byte[] array = [];
        var fg = ConsoleColor.Gray;
        if (!toFile)
        {
            fg = Console.ForegroundColor;
            Console.Clear();
        }

        try
        {
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"{FileError} {filePath}");
                return;
            }
            array = File.ReadAllBytes(filePath);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        if (toFile)
            Console.WriteLine(filePath);

        ushort i = 0;
        if (!toFile)
        {
            Console.WriteLine("Off | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
            Console.Write    ("----+------------------------------------------------");
        }
        foreach (var b in array)
        {
            if (i == 0 && b < 2)
                palette = b;
            else if (i == 4 && b < 0xC9)
                h = b;
            else if (i == 5 && b < 0xA1)
                w = b;
            if (!toFile)
            {
                if (i == Offset)
                {
                    Console.WriteLine($" [Palette: {(palette == 0 ? "GRY" : (palette == 1 ? "CMW" : palette))}, WxH: {w * 2}x{h}]");
                    Console.Write("000 | ");
                    for (ushort j = 0; j < Offset; j++)
                    {
                        Console.Write("-- ");
                    }
                }
                else if (i % 16 == 0)
                {
                    Console.WriteLine();
                    Console.Write($"{i:x3} | ");
                }
                Console.Write($"{b:x2} ");
            }
            i++;
        }
        if (!toFile)
        {
            Console.WriteLine();
            Console.WriteLine(Divider);
            Console.Clear();
        }

        i = Offset;
        Dictionary<(int, int), (byte, byte, byte)> pixmap = [];
        ushort[][] cols = [[0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0]];
        ushort[] hs = [1, 1, 1, 1];
        foreach (var b in array[Offset..])
        {
            if (i % 3 == 0)
            {
                cols = [[0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0]];
                hs = [1, 1, 1, 1];
                cols[0][0] = (ushort)((b >> 6) & 0x3);
                cols[0][1] = (ushort)((b >> 4) & 0x3);
                cols[0][2] = (ushort)((b >> 2) & 0x3);
                cols[0][3] = (ushort)(b & 0x3);
            }
            else if (i % 3 == 1)
            {
                hs[0] = (ushort)((b >> 4) & 0xF);
                hs[1] = (ushort)(b & 0xF);
            }
            else if (i % 3 == 2)
            {
                cols[1][0] = (ushort)((b >> 6) & 0x3);
                cols[1][1] = (ushort)((b >> 4) & 0x3);
                cols[1][2] = (ushort)((b >> 2) & 0x3);
                cols[1][3] = (ushort)(b & 0x3);

                for (var j = 0; j < hs[0]; j++)
                {
                    pixmap.TryAdd(new(x + 0, y), GetRGBColor(cols[0][0], palette));
                    pixmap.TryAdd(new(x + 1, y), GetRGBColor(cols[0][1], palette));
                    pixmap.TryAdd(new(x + 2, y), GetRGBColor(cols[0][2], palette));
                    pixmap.TryAdd(new(x + 3, y), GetRGBColor(cols[0][3], palette));
                    if (!toFile && (y + 3) < Console.WindowHeight && (x + 3) < Console.WindowWidth)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = GetCColor(cols[0][0], palette);
                        Console.Write("█");
                        Console.ForegroundColor = GetCColor(cols[0][1], palette);
                        Console.Write("█");
                        Console.ForegroundColor = GetCColor(cols[0][2], palette);
                        Console.Write("█");
                        Console.ForegroundColor = GetCColor(cols[0][3], palette);
                        Console.Write("█");
                    }
                    y++;
                    if (y >= h)
                    {
                        y = 0;
                        x += 4;
                    }
                }
                for (var j = 0; j < hs[1]; j++)
                {
                    pixmap.TryAdd(new(x + 0, y), GetRGBColor(cols[1][0], palette));
                    pixmap.TryAdd(new(x + 1, y), GetRGBColor(cols[1][1], palette));
                    pixmap.TryAdd(new(x + 2, y), GetRGBColor(cols[1][2], palette));
                    pixmap.TryAdd(new(x + 3, y), GetRGBColor(cols[1][3], palette));
                    if (!toFile && (y + 3) < Console.WindowHeight && (x + 3) < Console.WindowWidth)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = GetCColor(cols[1][0], palette);
                        Console.Write("█");
                        Console.ForegroundColor = GetCColor(cols[1][1], palette);
                        Console.Write("█");
                        Console.ForegroundColor = GetCColor(cols[1][2], palette);
                        Console.Write("█");
                        Console.ForegroundColor = GetCColor(cols[1][3], palette);
                        Console.Write("█");
                    }
                    y++;
                    if (y >= h)
                    {
                        y = 0;
                        x += 4;
                    }
                }
            }
            i++;
        }
        Console.ForegroundColor = fg;
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        if (!toFile || w < 1 || h < 1)
            return;

        using var img = new Image<Rgb24>(w, h);
        img.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    if (pixmap.TryGetValue(new(x, y), out var color))
                        row[x] = new Rgb24(color.Item1, color.Item2, color.Item3);
                    else
                        row[x] = new Rgb24(0, 0, 0);
                }
            }
        });
        var dir = Path.Combine(Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory(), "PNG");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        img.SaveAsPng(Path.Combine(dir, Path.GetFileName(filePath) + ".png"));
    }
}