using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixPix;

namespace SASTester;

partial class SASTester
{
    private const int MAX_WIDTH             = 160;
    private const int MAX_HEIGHT            = 200;
    private const int AII_MAX_WIDTH         = 140;
    private const int AII_MAX_HEIGHT        = 192;

    private const int AII_INTIAL_OFFSET     = 0x02; // file size?
    private const int AII_DECODE_OFFSET     = 0x07; // ???
    private const int AST_INITIAL_OFFSET    = 0x00;
    private const int AST_PALETTE_OFFSET    = 0x06;
    private const int AST_DECODE_OFFSET     = 0x36;
    private const int C64_INITIAL_OFFSET    = 0x02; // SAL version?
    private const int C64_DECODE_OFFSET     = 0x0A;
    private const int IBM_INITIAL_OFFSET    = 0x00;
    private const int IBM_DECODE_OFFSET     = 0x06;
    private const int MAC_INITIAL_OFFSET    = 0x00; // ???
    private const int MAC_DECODE_OFFSET     = 0x06; // ???

    private const int AII_FAIL_COLOR        = 2; // purple
    private const int AST_FAIL_COLOR        = 9; // purple, usually
    private const int C64_FAIL_COLOR        = 4; // purple
    private const int IBM_FAIL_COLOR        = 3; // red (palette 0) or magenta (palette 1)
    private const int SIXEL_ZOOM            = 3;
    
    private const string NotAPic            = ErrorPrefix + "This is not a";  // might need an 'n' for "...not an"
    private const string OutOfRange         = " image: height or width is out of bounds: ";

    public static bool doSixel = Sixel.IsSupported();

    enum AIIPalette
    {
        Black,
        White,
        Purple,
        Blue,
        Green,
        Orange,
    }

    enum C64Palette
    {
        Black,
        White,
        Red,
        Cyan,
        Purple,
        Green,
        Blue,
        Yellow,
        Orange,
        Brown,
        LtRed,
        DkGray,
        Gray,
        LtGreen,
        LtBlue,
        LtGray,
    };
    enum CGAPalette0
    {
        Black,
        Green,
        Red,
        Amber,
    };
    enum CGAPalette1
    {
        Black,
        Cyan,
        Magenta,
        White,
    };
    // Apple II NTSC artifact colors
    private static readonly Rgb24[] AIIPaletteRgb =
    [
        new Rgb24(0, 0, 0),       // 0: Black
        new Rgb24(255, 255, 255), // 1: White
        new Rgb24(197, 77, 193),  // 2: Purple
        new Rgb24(45, 152, 203),  // 3: Blue
        new Rgb24(56, 176, 43),   // 4: Green
        new Rgb24(209, 101, 50),  // 5: Orange
    ];
    // Commodore 64 16-color palette in Rgb24 format
    private static readonly Rgb24[] C64PaletteRgb =
    [
        new Rgb24(0, 0, 0),       // 0: Black
        new Rgb24(255, 255, 255), // 1: White
        new Rgb24(136, 0, 0),     // 2: Red
        new Rgb24(170, 255, 238), // 3: Cyan
        new Rgb24(204, 68, 204),  // 4: Purple
        new Rgb24(0, 204, 85),    // 5: Green
        new Rgb24(0, 0, 170),     // 6: Blue
        new Rgb24(238, 238, 119), // 7: Yellow
        new Rgb24(221, 136, 85),  // 8: Orange
        new Rgb24(102, 68, 0),    // 9: Brown
        new Rgb24(255, 119, 119), // 10: Lt. Red
        new Rgb24(51, 51, 51),    // 11: Dk. Gray
        new Rgb24(119, 119, 119), // 12: Gray
        new Rgb24(170, 255, 102), // 13: Lt. Green
        new Rgb24(0, 136, 255),   // 14: Lt. Blue
        new Rgb24(187, 187, 187)  // 15: Lt. Gray
    ];
    private static readonly Rgb24[] CGAPalette0Rgb =
    [
        new Rgb24(0, 0, 0),       // 0: Black
        new Rgb24(0, 170, 0),     // 1: Green
        new Rgb24(170, 0, 0),     // 2: Red
        new Rgb24(170, 170, 0),   // 3: Amber
    ];
    private static readonly Rgb24[] CGAPalette1Rgb =
    [
        new Rgb24(0, 0, 0),       // 0: Black
        new Rgb24(0, 170, 170),   // 1: Cyan
        new Rgb24(170, 0, 170),   // 2: Magenta
        new Rgb24(170, 170, 170)  // 3: White
    ];

    public void ShowPicFiles(string abbrev)
    {
        var fileFound = false;
        Console.WriteLine(FileWarn);
        var picFiles = GetMediaFileList(abbrev, false);
        if (picFiles.Count == 0)
        {
            Console.WriteLine("No picture files found.");
            Console.WriteLine(Divider);
            return;
        }
        foreach (var file in picFiles)
        {
            Console.WriteLine(file);
            fileFound = true;
        }
        if (!fileFound)
            return;
        string? input;
        do
        {
            Console.WriteLine();
            Console.Write(FilePrompt);
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                string filePath;
                var gameDir = abbrev + pcType;

                if (pcType == AtariAbb)
                    filePath = Path.Combine(RscPath, gameDir, abbrev, input);
                else
                    filePath = Path.Combine(RscPath, gameDir, input);

                if ((pcType == AppleAbb || pcType == AtariAbb) && (abbrev == AmberAbb || abbrev == AmazonAbb))
                {
                    string testFilePath;
                    if (pcType == AtariAbb)
                        testFilePath = Path.Combine(RscPath, gameDir, abbrev, "PDS", input);
                    else
                        testFilePath = Path.Combine(RscPath, gameDir, "PDS", input);

                    if (File.Exists(testFilePath))
                        filePath = testFilePath;
                }

                if (!File.Exists(filePath))
                {
                    Console.Error.WriteLine($"{FileError} {filePath}");
                    gameDir = abbrev;
                    filePath = Path.Combine(RscPath, gameDir, input);
                    if (!File.Exists(filePath))
                        break;
                }
                DrawPic(abbrev, filePath, toFile: false);
            }
            else
                Console.WriteLine(Divider);
        }
        while (!string.IsNullOrEmpty(input));
    }

    private void ExportPicFiles()
    {
        if (pcType == MacAbb || pcType == MsxAbb)
            return;
        Console.WriteLine(Wait);
        foreach (var abbrev in allGames)
        {
            if (pcType == AppleAbb & abbrev == AmazonAbb)
                continue;
            if (pcType == AtariAbb && !atariGames.Contains(abbrev))
                continue;
            if (pcType == MacAbb && !macGames.Contains(abbrev))
                continue;
            var picFiles = GetMediaFileList(abbrev, false);
            foreach (var file in picFiles)
            {
                var sub = "";
                if (pcType == MacAbb || (pcType == AppleAbb && abbrev == AmberAbb))
                    sub = "PDS";
                else if (pcType == AtariAbb)
                {
                    if (abbrev == AmazonAbb || abbrev == AmberAbb)
                        sub = Path.Combine(abbrev, "PDS");
                    else
                        sub = abbrev;
                }
                DrawPic(abbrev, Path.Combine(RscPath, abbrev + pcType, sub, file), toFile: true);
            }
        }
    }

    private static ConsoleColor CGAToConsoleColor(ushort c, ushort palette)
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

    private static Rgb24 CGAtoRGB(ushort c, ushort palette)
    {
        Rgb24 color = new(0x00, 0x00, 0x00);
        if (palette == 0)
        {
            switch (c)
            {
                case 0:
                    color = new(0x00, 0x00, 0x00); // black
                    break;
                case 1:
                    color = new(0x00, 0xAA, 0x00); // green
                    break;
                case 2:
                    color = new(0xAA, 0x00, 0x00); // red
                    break;
                case 3:
                    color = new(0xAA, 0xAA, 0x00); // amber
                    break;
            }
        }
        else if (palette == 1)
        {
            switch (c)
            {
                case 0:
                    color = new(0x00, 0x00, 0x00); // black
                    break;
                case 1:
                    color = new(0x00, 0xAA, 0xAA); // cyan
                    break;
                case 2:
                    color = new(0xAA, 0x00, 0xAA); // magenta
                    break;
                case 3:
                    color = new(0xAA, 0xAA, 0xAA); // lt.gray
                    break;
            }
        }
        return color;
    }

    private static ConsoleColor C64ToConsoleColor(int c)
    {
        var color = ConsoleColor.Black;
        switch (c)
        {
            case 0:
                color = ConsoleColor.Black;
                break;
            case 1:
                color = ConsoleColor.White;
                break;
            case 2:
                color = ConsoleColor.DarkRed;
                break;
            case 3:
                color = ConsoleColor.Cyan;
                break;
            case 4:
                color = ConsoleColor.Magenta;
                break;
            case 5:
                color = ConsoleColor.DarkGreen;
                break;
            case 6:
                color = ConsoleColor.DarkBlue;
                break;
            case 7:
                color = ConsoleColor.Yellow;
                break;
            case 8:
                color = ConsoleColor.Red;       // don't have a better choice for orange
                break;
            case 9:
                color = ConsoleColor.DarkYellow;
                break;
            case 10:
                color = ConsoleColor.Red;
                break;
            case 11:
                color = ConsoleColor.DarkGray; // this color is actually closest to the C64's md.gray, but there isn't a third option in any case
                break;
            case 12:
                color = ConsoleColor.DarkGray;
                break;
            case 13:
                color = ConsoleColor.Green;
                break;
            case 14:
                color = ConsoleColor.DarkCyan;
                break;
            case 15:
                color = ConsoleColor.Gray;
                break;
        }
        return color;
    }

    private static (byte, byte, byte) C64toRGB(ushort c)
    {
        // C64 Multicolor Palette (Approximate RGB)

        (byte, byte, byte) color = (0x00, 0x00, 0x00);
        switch (c)
        {
            case 0:
                color = (0x00, 0x00, 0x00); // black
                break;
            case 1:
                color = (0xFF, 0xFF, 0xFF); // white
                break;
            case 2:
                color = (0x88, 0x00, 0x00); // red
                break;
            case 3:
                color = (0xAA, 0xFF, 0xEE); // cyan
                break;
            case 4:
                color = (0xCC, 0x44, 0xCC); // purple
                break;
            case 5:
                color = (0x00, 0xCC, 0x55); // green
                break;
            case 6:
                color = (0x00, 0x00, 0xAA); // blue
                break;
            case 7:
                color = (0xEE, 0xEE, 0x77); // yellow
                break;
            case 8:
                color = (0xDD, 0x88, 0x55); // orange
                break;
            case 9:
                color = (0x66, 0x44, 0x00); // brown
                break;
            case 10:
                color = (0xFF, 0x77, 0x77); // pink
                break;
            case 11:
                color = (0x33, 0x33, 0x33); // dk.gray
                break;
            case 12:
                color = (0x77, 0x77, 0x77); // gray
                break;
            case 13:
                color = (0xAA, 0xFF, 0x66); // lt.green
                break;
            case 14:
                color = (0x00, 0x88, 0xFF); // lt.blue
                break;
            case 15:
                color = (0xBB, 0xBB, 0xBB); // lt.gray
                break;
        }
        return color;
    }

    private static ConsoleColor AIIToConsoleColor((bool parity, bool palette) m, (bool parity, bool palette) n)
    {
        ConsoleColor color = ConsoleColor.Magenta;

        switch (GetAIIColors(m, n))
        {
            case 0:
            case 4:
                color = ConsoleColor.Black;
                break;
            case 1:
                color = ConsoleColor.DarkGreen;
                break;
            case 2:
                color = ConsoleColor.DarkMagenta;
                break;
            case 3:
            case 7:
                color = ConsoleColor.White;
                break;
            case 5:
                color = ConsoleColor.Red;   // close as I can get to orange
                break;
            case 6:
                color = ConsoleColor.Blue;
                break;
        }

        return color;
    }

    private static Rgb24 AIItoRgb((bool parity, bool palette) m, (bool parity, bool palette) n)
    {
        Rgb24 color = new(0xFF, 0x00, 0xFF);

        switch (GetAIIColors(m, n))
        {
            case 0:
            case 4:
                color = new(0x00, 0x00, 0x00);  // black
                break;
            case 1:
                color = new(0x38, 0xB0, 0x2B);  // green
                break;
            case 2:
                color = new(0xC5, 0x4D, 0xC1);  // purple
                break;
            case 3:
            case 7:
                color = new(0xFF, 0xFF, 0xFF);  // white
                break;
            case 5:
                color = new(0xD1, 0x65, 0x32);  // orange
                break;
            case 6:
                color = new(0x2D, 0x98, 0xCB);  // blue
                break;
        }

        return color;
    }

    private static ushort GetAIIColors((bool parity, bool palette) m, (bool parity, bool palette) n)
    {
        ushort color1 = 0, color2 = 0;
        if (m.palette) color1 += 4;
        if (n.palette) color2 += 4;

        if (!m.parity && n.parity)
            return (ushort)(color2 + 1);    // green or orange

        if (m.parity && !n.parity)
            return (ushort)(color1 + 2);    // purple or blue

        if (m.parity && n.parity)
            return (ushort)(color2 + 3);    // white

        // if (!m.parity && !n.parity)
        return color1;                      // black
    }

    // AII, AST, C64, IBM for now:
    //   AII uses HIRES 280x192 x 6-color mode (pictures are doubled in width)
    //   AST uses 320x200 x 16-color (pictures are doubled in width)
    //   C64 uses Multicolor 160x200 x 16-color mode
    //   IBM uses CGA graphics mode 320x200 x 4-color x 2-palettes in mode 4 (pictures are doubled in width)
    // TODO: Figure out format for other MAC and MSX
    private static void DrawPic(string abbrev, string filePath = "", bool toFile = false)
    {
        var offset = 6;
        if (pcType == AppleAbb)
            offset = AII_DECODE_OFFSET;  // For AII, the first 7 bytes have height/width and... unknown
        else if (pcType == AtariAbb)
            offset = AST_PALETTE_OFFSET; // For AST, the section from 0x06 to 0x53 is a ramped palette
        else if (pcType == CommodoreAbb)
            offset = C64_DECODE_OFFSET;  // For C64, there's multiple sections after the height/width again
        else if (pcType == IbmAbb)
            offset = IBM_DECODE_OFFSET;  // For IBM, the first 6 bytes have palette colors and height/width
        else if (pcType == MacAbb)
            offset = MAC_DECODE_OFFSET;
        ushort width = 0, height = 0, palette = 0xFF, intensity = 0xFF, bgColor = 0xFF, c64Unknown = 0xFF;
        byte[] fileArray = [];
        var fg = ConsoleColor.Gray;
        if (!toFile && !doSixel)
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
            fileArray = File.ReadAllBytes(filePath);
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
        byte lastB = 0;
        var section = 0;
        foreach (var b in fileArray)
        {
            if (pcType == AppleAbb)
            {
                if (i == 5)
                    height = b;
                else if (i == 6)
                    width = b;
            }
            else if (pcType == AtariAbb)
            {
                if (i == 3)
                    width = b;
                else if (i == 5)
                    height = b;
            }
            else if (pcType == CommodoreAbb)
            {
                if (i == 4 && b < 0xC9)
                    height = b;
                else if (i == 5 && b < 0xA1)
                    width = b;
                else if (i == 6)
                    c64Unknown = b;
            }
            else if (pcType == IbmAbb)
            {
                if (i == 0)
                    palette = b;
                else if (i == 1)
                {
                    intensity = (byte)(b >> 4);
                    bgColor = (byte)(b & 0x0F);
                }
            }
            if (!toFile)
            {
                if (i == offset)
                {
                    if (pcType == AppleAbb)
                    {
                        Console.WriteLine($"\n [WxH: {width}x{height}]");
                        Console.Write("000 | ");
                        for (ushort j = 0; j < offset; j++)
                        {
                            Console.Write("-- ");
                        }
                    }
                    else if (pcType == AtariAbb)
                    {
                        Console.WriteLine($"\n [WxH: {width}x{height}]");
                        Console.Write("000 | ");
                        for (ushort j = 0; j < offset; j++)
                        {
                            Console.Write("-- ");
                        }
                    }
                    else if (pcType == IbmAbb)
                    {
                        Console.WriteLine($"\n [Palette: {(palette == 0 ? "GRY" : (palette == 1 ? "CMW" : palette))}, " +
                            $"Intensity: {(intensity == 0 ? "Low" : (intensity == 1 ? "High" : intensity))}, " +
                            $"BgColor: {(bgColor == 0 ? "Black" : bgColor)}, " +
                            $"WxH: {width}x{height}]");
                        Console.Write("000 | ");
                        for (ushort j = 0; j < offset; j++)
                        {
                            Console.Write("-- ");
                        }
                    }
                }
                else if (i % 16 == 0)
                {
                    Console.Write($"\n{i:x3} | ");
                }
                Console.Write($"{b:x2} ");
                if (pcType == AtariAbb && i == AST_DECODE_OFFSET - 1)
                {
                    offset = i % 16 + 1;
                    Console.Write($"\nPalette:");
                    foreach (var c in GetASTPalette(fileArray[AST_PALETTE_OFFSET..AST_DECODE_OFFSET]))
                    {
                        Console.Write($"{c.R:x2}{c.G:x2}{c.B:x2} ");
                    }
                    Console.Write($"\n{(i - offset + 1):x3} | ");
                    for (ushort j = 0; j < offset; j++)
                    {
                        Console.Write("-- ");
                    }
                }
                else if (pcType == CommodoreAbb && lastB == height && b == width) // Section breaks
                {
                    if (section > 0)
                    {
                        if (section == 1)
                            Console.Write($"\n [WxH: {width}x{height}, Unknown: {c64Unknown}]");
                        offset = i % 16 + 1;
                        Console.WriteLine($"\nSection {section}:");
                        Console.Write($"\n{(i - offset + 1):x3} | ");
                        for (ushort j = 0; j < offset; j++)
                        {
                            Console.Write("-- ");
                        }
                    }
                    section++;
                }
            }
            i++;
            lastB = b;
        }
        if (!toFile && !doSixel)
        {
            Console.WriteLine();
            Console.WriteLine(Divider);
            Console.Clear();
        }

        if (fileArray.Length > offset)
        {
            var image = new Image<Rgb24>(1, 1);
            if (pcType == AppleAbb)
                image = DecodeAIIPic(fileArray, toFile);
            else if (pcType == AtariAbb)
                image = DecodeASTPic(fileArray, toFile);
            else if (pcType == CommodoreAbb)
                image = DecodeC64Pic(fileArray, toFile);
            else if (pcType == IbmAbb)
                image = DecodeIBMPic(fileArray, toFile);

            if (image.Width > 1)
            {
                if (toFile)
                {
                    var dir = Path.Combine(Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory(), "PNG");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    image.SaveAsPng(Path.Combine(dir, Path.GetFileName(filePath) + ".png"));
                }
                else if (doSixel)
                {
                    Console.WriteLine();
                    Console.WriteLine(Sixel.Encode(image.CloneAs<Rgba32>(), new Size(width * 2 * SIXEL_ZOOM, height * SIXEL_ZOOM)).ToArray());
                    Console.WriteLine();
                }
                else // ANSI art
                {
                    Console.ForegroundColor = fg;
                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                }
            }
        }
    }

    private static ushort MakeC64FourColorMap(byte[] data, int section, ref List<(int, int, int, int)> map)
    {
        ushort rc;
        var dataLength = data.Length;
        var blockIdx = 0;
        
        // Process in groups of 3 bytes (6 nibbles: Two Colors #1, Two Run-Lengths #1 & #2, Two Colors #2)
        for (var i = 0; i <= dataLength - 2; i += 3)
        {
            // Color 1A (Byte1 High Nibble) and Color 1B (Byte1 Low Nibble)
            var b = data[i];
            var colors = (b >> 4, b & 0x0F);

            // Run-Length #1 (Byte2 High Nibble) for Byte1 Colors #1
            b = data[i + 1];
            var run = b >> 4;

            rc = AddRunToC64ColorMap(section, colors, run, ref map, ref blockIdx);
            if (rc != 0) return rc;

            if (i <= dataLength - 3)
            {
                // Run-Length #2 (Byte2 Low Nibble) for Byte3 Colors #2
                run = b & 0x0F;

                // Color 2A (Byte3 High Nibble) and Color 2B (Byte3 Low Nibble)
                b = data[i + 2];
                colors = (b >> 4, b & 0x0F);

                rc = AddRunToC64ColorMap(section, colors, run, ref map, ref blockIdx);
                if (rc != 0) return rc;
            }
        }

        return 0;
    }

    private static ushort AddRunToC64ColorMap(int section, (int colorA, int colorB) colors, int run, ref List<(int color1, int color2, int color3, int color4)> map, ref int blockIdx)
    {
        for (var i = 0; i < run; i++)
        {
            if (blockIdx >= map.Count)
                return 1;

            if (section == 1)
                map[blockIdx] = (map[blockIdx].color1, map[blockIdx].color2, colors.colorA, colors.colorB);
            else
                map[blockIdx] = (colors.colorA, colors.colorB, map[blockIdx].color3, map[blockIdx].color4);

            blockIdx++;
        }

        return 0;
    }

    // These colors seem to be automatically switched to brighter variants
    private static int C64PaletteRemap(int color, bool remap = false)
    {
        if (remap)
        {
            return color switch
            {
                2 => 10,    // Red -> Lt.Red
                6 => 14,    // Blue -> Lt.Blue
                _ => color
            };
        }
        return color;
    }

    private static Image<Rgb24> DecodeAIIPic(byte[] data, bool toFile)
    {
        int mx = 0, my = 0, width = 0, height = 0;
        var fg = ConsoleColor.Gray;
        if (!toFile && !doSixel)
        {
            fg = Console.ForegroundColor;
            Console.Clear();
        }

        ushort i = 0;
        height = data[5];
        width = data[6];

        if (width < 1 || width > AII_MAX_WIDTH || height < 1 || height > AII_MAX_HEIGHT)
        {
            if (!toFile)
                Console.WriteLine($"\n{NotAPic} {AppleName}{OutOfRange}{width}x{height}.");
            return new(1, 1);
        }
        if (!toFile && !doSixel)
        {
            Console.WriteLine();
            Console.WriteLine(Divider);
            Console.Clear();
        }
        //Console.WriteLine();
        i = AII_DECODE_OFFSET;
        Dictionary<(int, int), (bool parity, bool palette)> monomap = [];
        Dictionary<(int, int), (byte, byte, byte)> pixmap = [];
        var run1 = (ushort)1;
        var run2 = (ushort)1;
        byte b1 = 0x0;

        foreach (var b in data[AII_DECODE_OFFSET..])
        {
            bool palette = false;

            // the first byte in a three-byte sequence is decoded into a 7-pixel pattern
            if ((i - 1) % 3 == 0)
            {
                // reset for new 3-byte sequence
                run1 = 1;
                run2 = 1;

                b1 = b;
            }
            // the second byte is the run-lengths for the first and third byte, split between nibbles
            else if ((i - 1) % 3 == 1)
            {
                palette = (b1 & 0x80) != 0; //(b1 & 0x1) != 0;
                //if (!toFile) Console.Write($"{b1:b8}");

                // second byte, first nibble = run-length for first byte pattern
                run1 = (ushort)((b >> 4) & 0xF);
                for (var run = 0; run < run1; run++)
                {
                    //if (!toFile) Console.Write($"({mx}, {my}): ");
                    for (var p = 0; p < 7; p++)
                    {
                        var pixel = ((b1 >> p) & 0x1) == 1;
                        //if (!toFile) Console.Write(pixel ? 1 : 0);
                        monomap.Add((mx + p, my), (pixel, palette));
                    }
                    //if (!toFile) Console.WriteLine();
                    my++;
                    if (my >= height)
                    {
                        //if (!toFile) Console.WriteLine();
                        my = 0;
                        mx += 7;
                    }
                }

                // second byte, second nibble = run-length for third byte pattern
                run2 = (ushort)(b & 0xF);
                //if (!toFile) Console.Write($" x {runs[0]:d2} | {runs[1]:d2} x ");
            }
            // the third byte is another 7-pixel pattern
            else if ((i - 1) % 3 == 2)
            {
                palette = (b & 0x80) != 0; //(b & 0x1) != 0;
                //if (!toFile) Console.Write($"{b:b8}");

                for (var run = 0; run < run2; run++)
                {
                    //if (!toFile) Console.Write($"({mx}, {my}): ");
                    for (var p = 0; p < 7; p++)
                    {
                        var pixel = ((b >> p) & 0x1) == 1;
                        //if (!toFile) Console.Write(pixel ? 1 : 0);
                        monomap.Add((mx + p, my), (pixel, palette));
                    }
                    //if (!toFile) Console.WriteLine();
                    my++;
                    if (my >= height)
                    {
                        //if (!toFile) Console.WriteLine();
                        my = 0;
                        mx += 7;
                    }
                }
            }
            //if (!toFile) Console.WriteLine();
            i++;
        }

        //if (!toFile) Console.WriteLine();

        var img = new Image<Rgb24>(width, height);
        //var img = new Image<Rgb24>(width * 2, height);
        img.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                //for (var x = 0; x < row.Length - 1; x += 2)
                {
                    monomap.TryGetValue((x * 2, y), out var m);
                    monomap.TryGetValue((x * 2 + 1, y), out var n);
                    //monomap.TryGetValue((x, y), out var m);
                    //monomap.TryGetValue((x + 1, y), out var n);
                    row[x] = AIItoRgb(m, n);
                    //row[x] = m.parity ? new Rgb24(0xFF, 0xFF, 0xFF) : new Rgb24(0x00, 0x00, 0x00);
                    //row[x + 1] = n.parity ? new Rgb24(0xFF, 0xFF, 0xFF) : new Rgb24(0x00, 0x00, 0x00);
                    if (!toFile && !doSixel && y < Console.WindowHeight && x < Console.WindowWidth)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = AIIToConsoleColor(m, n);
                        Console.Write("█");
                    }
                }
            }
        });

        return img;
    }

    // "Ramped" color palette where the components are separated (R0, R1, R2... G0, G1, G2... B0, B1, B2...)
    private static Rgb24[] GetASTPalette(byte[] paletteData)
    {
        var RampedPaletteRgb = new Rgb24[16];

        for (var i = 0; i < 16; i++)
        {
            // Extract 4-bit nibbles from the three separate 16-byte ramps
            var rVal = paletteData[i] & 0x0F;          // First 16 bytes are Red
            var gVal = paletteData[i + 16] & 0x0F;     // Next 16 bytes are Green
            var bVal = paletteData[i + 32] & 0x0F;     // Final 16 bytes are Blue

            // Scale 0-15 to 0-255
            var r = (byte)(rVal * 17);
            var g = (byte)(gVal * 17);
            var b = (byte)(bVal * 17);

            RampedPaletteRgb[i] = new Rgb24(r, g, b);
        }
        return RampedPaletteRgb;
    }

    private static Image<Rgb24> DecodeASTPic(byte[] fileData, bool toFile)
    {
        // 1. Read Header Dimensions
        var width = (fileData[2] << 8) | fileData[3];
        var height = (fileData[4] << 8) | fileData[5];
        var maxBlockX = width / 4;
        var maxBlockY = height / 2;

        // 2. Parse Palette
        var palette = GetASTPalette(fileData[AST_PALETTE_OFFSET..AST_DECODE_OFFSET]);

        // 3. Setup Separate Bitplane Workspaces (Width x Height)
        var bitplanes = new byte[4][];
        for (var i = 0; i < 4; i++)
        {
            bitplanes[i] = new byte[width * height];
        }

        int filePtr = AST_DECODE_OFFSET;
        var blockX = 0;
        var blockY = 0;
        var extraBytes = 0;

        if (!toFile) Console.WriteLine($"\n\n{maxBlockX}x{maxBlockY} Blocks");

        // 4. Parse the Sequential Bitplane Streams
        for (var plane = 0; plane < 4; plane++)
        {
            if (blockY > maxBlockY)
                blockY -= maxBlockY + 1;

            if (!toFile) Console.WriteLine($"\n\n===Bitplane #{plane}=================");

            while (filePtr < fileData.Length - 2)
            {
                var b = fileData[filePtr++];

                if (b == 0x00) // empty block takes a run-length
                {
                    int skipValue = fileData[filePtr++];
                    if (skipValue == 0x00) // a double zero adds 256 to the next skip (and can be cumulative)
                    {
                        extraBytes++;
                        if (!toFile) Console.WriteLine($"    00 00 [byte count: {extraBytes}]");
                        continue;
                    }

                    if (!toFile) Console.WriteLine($" skip {skipValue:x2} ({skipValue}) {(extraBytes > 0 ? $"[ x {extraBytes} ]" : "")}");
                    
                    skipValue += extraBytes * 0x100;
                    
                    if (extraBytes > 0)
                        extraBytes = 0;

                    for (var s = 0; s < skipValue; s++)
                    {
                        blockX++;
                        if (blockX > maxBlockX)
                        {
                            blockX -= maxBlockX + 1;
                            blockY++;
                            if (blockY > maxBlockY)
                                break;
                        }
                    }
                    if (blockY > maxBlockY)
                        break;

                    continue;
                }
                if (b == 0xFF) // full block takes a run-length
                {
                    int fillValue = fileData[filePtr++];
                    //if (!toFile) Console.WriteLine($" fill {fillValue:x2} ({fillValue})");
                    for (var f = 0; f < fillValue; f++)
                    {
                        if (!toFile) Console.WriteLine($"    ({blockX:d2}, {blockY:d2}): ff (1111_1111)");

                        var fX = blockX * 4;
                        var fY = blockY * 2;

                        for (var p = 0; p < 4; p++)
                        {
                            var targetX = fX + p;

                            if (targetX < width && (fY + 1) < height)
                            {
                                bitplanes[plane][fY * width + targetX] = 1;
                                bitplanes[plane][(fY + 1) * width + targetX] = 1;
                            }
                        }

                        blockX++;
                        if (blockX > maxBlockX)
                        {
                            blockX -= maxBlockX + 1;
                            blockY++;
                            if (blockY > maxBlockY)
                                break;
                        }
                    }
                    if (blockY > maxBlockY)
                        break;

                    continue;
                }

                if (blockX >= maxBlockX) // to the right of visible area, shouldn't happen
                {
                    if (!toFile) Console.WriteLine($"   *({blockX:d2}, {blockY:d2}): {b:x2} ({b})");
                }
                else if (blockY >= maxBlockY) // below visible area, shouldn't happen
                {
                    if (!toFile) Console.WriteLine($"  **({blockX:d2}, {blockY:d2}): {b:x2} ({b})");
                }
                else
                {
                    // Extract high and low nibbles as direct independent row values
                    var topRowBits = (byte)((b >> 4) & 0x0F);
                    var bottomRowBits = (byte)(b & 0x0F);

                    if (!toFile) Console.WriteLine($"    ({blockX:d2}, {blockY:d2}): {b:x2} ({topRowBits:b4}_{bottomRowBits:b4})");

                    var x = blockX * 4;
                    var y = blockY * 2;

                    for (var p = 0; p < 4; p++)
                    {
                        var targetX = x + p;
                        var topBitState = (topRowBits >> (3 - p)) & 0x01;
                        var bottomBitState = (bottomRowBits >> (3 - p)) & 0x01;

                        if (targetX < width && (y + 1) < height)
                        {
                            bitplanes[plane][y * width + targetX] = (byte)topBitState;
                            bitplanes[plane][(y + 1) * width + targetX] = (byte)bottomBitState;
                        }
                    }
                }

                blockX++;
                if (blockX > maxBlockX)
                {
                    blockX -= maxBlockX + 1;
                    blockY++;
                    if (blockY > maxBlockY)
                        break;
                }
            }
        }
        if (!toFile) Console.WriteLine($"\nend ({blockX:d2}, {blockY:d2})");

        // 5. Composite the 4 Bitplanes into Finished Hardware Color Indices
        var finalPixelBuffer = new byte[width * height];
        for (var i = 0; i < finalPixelBuffer.Length; i++)
        {
            var b0 = bitplanes[0][i];
            var b1 = bitplanes[1][i];
            var b2 = bitplanes[2][i];
            var b3 = bitplanes[3][i];

            // Merge the bitplanes to find the palette index
            finalPixelBuffer[i] = (byte)((b3 << 3) | (b2 << 2) | (b1 << 1) | b0);
        }

        if (!toFile)
        {
            for (var plane = 0; plane < 4; plane++)
            {
                // 6. Output to Canvas Image
                var planeImage = new Image<Rgb24>(width, height);
                planeImage.ProcessPixelRows(accessor =>
                {
                    for (var currY = 0; currY < accessor.Height; currY++)
                    {
                        var row = accessor.GetRowSpan(currY);
                        for (var currX = 0; currX < accessor.Width; currX++)
                        {
                            byte colorIndex = bitplanes[plane][currY * width + currX];
                            row[currX] = palette[colorIndex];
                        }
                    }
                });
                Console.WriteLine();
                Console.WriteLine(Sixel.Encode(planeImage.CloneAs<Rgba32>(), new Size(width * 2 * SIXEL_ZOOM, height * SIXEL_ZOOM)).ToArray());
                //planeImage.SaveAsPng(Path.Combine("ast-test" + plane + ".png"));
            }
        }

        // 6. Output to Canvas Image
        var img = new Image<Rgb24>(width, height);
        img.ProcessPixelRows(accessor =>
        {
            for (var currY = 0; currY < accessor.Height; currY++)
            {
                var row = accessor.GetRowSpan(currY);
                for (var currX = 0; currX < accessor.Width; currX++)
                {
                    byte colorIndex = finalPixelBuffer[currY * width + currX];
                    row[currX] = palette[colorIndex];
                }
            }
        });
        
        return img;
    }
   
    private static Image<Rgb24> DecodeC64Pic(byte[] data, bool toFile)
    {
        List<byte[]> sectionData = [];
        sectionData.Add(data[C64_INITIAL_OFFSET..(C64_DECODE_OFFSET - 1)]);

        // For C64 only, every file for each game starts with the same 2 bytes. Since AMZ is 0x0010, and was the first to be
        // released, and AMB and PMN are both 0x2041, and were the last to be released, I suspect it's a SAL version ID.

        // Skip initial header, starting with two bytes described above until the *second* instance of two bytes picture size (hh ww),
        // followed by an unknown byte, then 00; next is the picture size again, then almost always 00 (contra AMB "feet"), which
        // sets off the next section. The "hh ww" header also sets off the next two sections.

        var height = data[4];
        var width = data[5];

        if (width < 1 || width > MAX_WIDTH || height < 1 || height > MAX_HEIGHT)
        {
            if (!toFile)
                Console.WriteLine($"\n{NotAPic} {CommodoreName}{OutOfRange}{width}x{height}");
            return new(1, 1);
        }

        var sectionNum = 1;
        var dataLength = data.Length;

        List<byte> section = [];

        var offset = C64_DECODE_OFFSET;
        for (; offset < dataLength - 1; offset++)
        {
            // If the next two bytes are a header (hh ww), this section is done.
            if (offset + 2 < dataLength && data[offset] == height && data[offset + 1] == width)
            {
                sectionData.Add([.. section]);
                sectionNum++;
                section = [];
                offset += 2;         // Move past the header
            }
            section.Add(data[offset]);
        }
        if (offset < dataLength)
            section.Add(data[offset]);
        sectionData.Add([.. section]);

        if (sectionData.Count < 4)
        {
            if (!toFile)
                Console.WriteLine($"\n{NotAPic} {CommodoreName} image: 4 sections required, only {sectionData.Count} found.");
            return new(1, 1);
        }

        var numBlocks = width / 4 * (height / 8);

        // 1. TODO: Find if a background color is specified in header
        var globBgColor = 0;

        var img = new Image<Rgb24>(width, height);

        List<(int color1, int color2, int color3, int color4)> colorMap = [];
        // Fill with Color 4 (Purple) to see if we have any failures
        for (var i = 0; i < numBlocks; i++)
        {
            colorMap.Add((C64_FAIL_COLOR, C64_FAIL_COLOR, C64_FAIL_COLOR, C64_FAIL_COLOR));
        }

        // 2. Parse Sections 1 and 2 into a List of color data, one entry for each 4x8 block
        ushort rc = MakeC64FourColorMap(sectionData[1], 1, ref colorMap);
        if (rc == 0)
            rc = MakeC64FourColorMap(sectionData[2], 2, ref colorMap);
        if (rc != 0)
        {
            if (!toFile)
                Console.WriteLine($"\n{NotAPic} {CommodoreName} image: run-length overflow.");
            return new(1, 1);
        }

        // 3. Parse Section 3 for bitmask data while iterating the Image
        var s3Idx = 0;

        // Stream State (Persists across column boundaries)
        var runRemaining = 0;
        byte currentMask = 0;
        byte pendingMaskB = 0;
        var pendingRunB = 0;
        var maskBActive = false;

        var blocksHigh = height / 8;

        // Process column-by-column (4-pixel wide vertical strips)
        for (var x = 0; x < width; x += 4)
        {
            for (var y = 0; y < height; y++)
            {
                // Bitmask
                if (runRemaining <= 0)
                {
                    if (maskBActive && pendingRunB > 0)
                    {
                        currentMask = pendingMaskB;
                        runRemaining = pendingRunB;
                        maskBActive = false;
                    }
                    else if (s3Idx + 2 < sectionData[3].Length)
                    {
                        byte maskA = sectionData[3][s3Idx++];
                        byte lengths = sectionData[3][s3Idx++];
                        byte maskB = sectionData[3][s3Idx++];

                        currentMask = maskA;
                        runRemaining = (lengths >> 4) & 0x0F;

                        pendingMaskB = maskB;
                        pendingRunB = lengths & 0x0F;
                        maskBActive = true;

                        if (runRemaining == 0) // Immediate skip to B
                        {
                            currentMask = pendingMaskB;
                            runRemaining = pendingRunB;
                            maskBActive = false;
                        }
                    }
                }

                // Color lookup
                var bx = x / 4;
                var by = y / 8;
                var blockIdx = (bx * blocksHigh) + by;

                if (blockIdx < numBlocks)
                {
                    for (var xOff = 0; xOff < 4; xOff++)
                    {
                        var px = x + xOff;
                        if (px < width)
                        {
                            var shift = (3 - xOff) * 2;
                            var pair = (currentMask >> shift) & 0x03;

                            var color = pair switch
                            {
                                0b00 => globBgColor,
                                0b01 => colorMap[blockIdx].color1,
                                0b10 => colorMap[blockIdx].color2,
                                0b11 => colorMap[blockIdx].color4,
                                _ => colorMap[blockIdx].color3,
                            };

                            color = C64PaletteRemap(color, true);
                            img[px, y] = C64PaletteRgb[color];
                            if (!toFile && !doSixel && y < Console.WindowHeight && px < Console.WindowWidth)
                            {
                                Console.SetCursorPosition(px, y);
                                Console.ForegroundColor = C64ToConsoleColor(color);
                                Console.Write("█");
                            }
                        }
                    }
                }

                runRemaining--;
            }
        }

        return img;
    }

    private static Image<Rgb24> DecodeIBMPic(byte[] data, bool toFile)
    {
        ushort x = 0, y = 0, width = 0, height = 0, palette = 0;
        var fg = ConsoleColor.Gray;
        if (!toFile && !doSixel)
        {
            fg = Console.ForegroundColor;
            Console.Clear();
        }

        ushort i = 0;
        palette = data[0];
        height = data[4];
        width = data[5];

        if (palette > 1)
        {
            if (!toFile)
                Console.WriteLine($"\n{NotAPic}n {IbmName} image: palette is {palette} (should be zero or one).");
            return new(1, 1);
        }
        if (width < 1 || width > MAX_WIDTH || height < 1 || height > MAX_HEIGHT)
        {
            if (!toFile)
                Console.WriteLine($"\n{NotAPic}n {IbmName}{OutOfRange}{width}x{height}.");
            return new(1, 1);
        }
        if (!toFile && !doSixel)
        {
            Console.WriteLine();
            Console.WriteLine(Divider);
            Console.Clear();
        }

        i = IBM_DECODE_OFFSET;
        bool done = false;
        Dictionary<(int, int), Rgb24> pixmap = [];
        ushort[][] cols = [
            [IBM_FAIL_COLOR, IBM_FAIL_COLOR, IBM_FAIL_COLOR, IBM_FAIL_COLOR,],
            [IBM_FAIL_COLOR, IBM_FAIL_COLOR, IBM_FAIL_COLOR, IBM_FAIL_COLOR,]];
        ushort[] runs = [1, 1];

        foreach (var b in data[IBM_DECODE_OFFSET..])
        {
            // the first byte in a three-byte sequence is decoded into a 4-pixel bitmask
            if (i % 3 == 0)
            {
                // reset for new 3-byte sequence
                done = false;
                for (var j = 0; j < 2; j++)
                {
                    Array.Fill<ushort>(cols[j], IBM_FAIL_COLOR);
                }
                runs = [1, 1];
                cols[0][0] = (ushort)((b >> 6) & 0x3);
                cols[0][1] = (ushort)((b >> 4) & 0x3);
                cols[0][2] = (ushort)((b >> 2) & 0x3);
                cols[0][3] = (ushort)(b & 0x3);
            }
            // the run-lengths for the first and third byte are split in each nibble of the second byte
            else if (i % 3 == 1)
            {
                runs[0] = (ushort)((b >> 4) & 0xF); // first nibble = run-length for first byte pattern
                runs[1] = (ushort)(b & 0xF);        // second nibble = run-length for third byte pattern
                if (runs[1] == 0)
                    done = true;
            }
            // the third byte is another 4-pixel bitmask
            else if (i % 3 == 2)
            {
                cols[1][0] = (ushort)((b >> 6) & 0x3);
                cols[1][1] = (ushort)((b >> 4) & 0x3);
                cols[1][2] = (ushort)((b >> 2) & 0x3);
                cols[1][3] = (ushort)(b & 0x3);
                done = true;
            }
            if (done)
            {
                for (var j = 0; j < 2; j++)
                {
                    for (var k = 0; k < runs[j]; k++)
                    {
                        pixmap.TryAdd(new(x + 0, y), CGAtoRGB(cols[j][0], palette));
                        pixmap.TryAdd(new(x + 1, y), CGAtoRGB(cols[j][1], palette));
                        pixmap.TryAdd(new(x + 2, y), CGAtoRGB(cols[j][2], palette));
                        pixmap.TryAdd(new(x + 3, y), CGAtoRGB(cols[j][3], palette));

                        if (!toFile && !doSixel && (y + 3) < Console.WindowHeight && (x + 3) < Console.WindowWidth)
                        {
                            Console.SetCursorPosition(x, y);
                            Console.ForegroundColor = CGAToConsoleColor(cols[j][0], palette);
                            Console.Write("█");
                            Console.ForegroundColor = CGAToConsoleColor(cols[j][1], palette);
                            Console.Write("█");
                            Console.ForegroundColor = CGAToConsoleColor(cols[j][2], palette);
                            Console.Write("█");
                            Console.ForegroundColor = CGAToConsoleColor(cols[j][3], palette);
                            Console.Write("█");
                        }
                        y++;
                        if (y >= height)
                        {
                            y = 0;
                            x += 4;
                        }
                    }
                }
            }
            i++;
        }

        var img = new Image<Rgb24>(width, height);
        img.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    if (pixmap.TryGetValue(new(x, y), out var color))
                        row[x] = color;
                    else
                        row[x] = CGAtoRGB(IBM_FAIL_COLOR, 1);
                }
            }
        });

        return img;
    }
}