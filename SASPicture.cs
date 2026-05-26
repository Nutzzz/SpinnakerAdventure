using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixPix;

namespace SASTester;

partial class SASTester
{
    private const int AII_INTIAL_OFFSET = 3;    // file size?
    private const int AST_INITIAL_OFFSET = 0;
    private const int AST_PALETTE_OFFSET = 6;
    private const int AST_DECODE_OFFSET = 54;
    private const int C64_INITIAL_OFFSET = 2;   // SAL version?
    private const int C64_DECODE_OFFSET = 10;
    private const int IBM_INITIAL_OFFSET = 0;
    private const int IBM_DECODE_OFFSET = 6;

    private const int AST_FAIL_COLOR = 9; // purple, usually
    private const int C64_FAIL_COLOR = 4; // purple
    private const int SIXEL_ZOOM = 3;

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
    enum CGAPalette1
    {
        Black,
        Cyan,
        Magenta,
        White,
    };
    enum CGAPalette2
    {
        Black,
        Green,
        Red,
        Yellow
    };
    // Standard Commodore 64 16-color palette in Rgb24 format
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
        new Rgb24(170, 170, 0),   // 3: Yellow
    ];
    private static readonly Rgb24[] CGAPalette1Rgb =
    [
        new Rgb24(0, 0, 0),       // 0: Black
        new Rgb24(0, 170, 170),   // 1: Cyan
        new Rgb24(170, 0, 170),   // 2: Magenta
        new Rgb24(170, 170, 170)  // 3: White
    ];

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
            Console.WriteLine();
            Console.Write(FilePrompt);
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                string filePath;
                var gameDir = abbrev + pcType;
                if (pcType == AtariAbb)
                {
                    filePath = Path.Combine(RscPath, gameDir, abbrev, "PDS", input);
                    if (!File.Exists(filePath))
                        filePath = Path.Combine(RscPath, gameDir, abbrev, input);
                }
                else
                    filePath = Path.Combine(RscPath, gameDir, input);
                if (!File.Exists(filePath))
                {
                    Console.Error.WriteLine($"{FileError} {filePath}");
                    gameDir = abbrev;
                    filePath = Path.Combine(RscPath, gameDir, input);
                    if (!File.Exists(filePath))
                    {
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
            var filePath = Path.Combine(RscPath, abbrev + pcType);
            if (Directory.Exists(filePath))
            {
                if (pcType == AtariAbb) // sometimes uses extension + deeper folder structure
                {
                    filePath = Path.Combine(RscPath, abbrev + pcType, abbrev);
                    var pdsFilePath = Path.Combine(RscPath, abbrev + pcType, abbrev, "PDS");
                    var fileExt = "*.GST";

                    if (Directory.Exists(filePath))
                    {
                        if (Directory.Exists(pdsFilePath))
                            filePath = pdsFilePath;

                        for (var i = 0; i < 2; i++)
                        {
                            foreach (var file in Directory.EnumerateFiles(filePath, fileExt).ToList())
                            {
                                var filename = Path.GetFileName(file);
                                if (!locNames.Contains(filename, StringComparer.OrdinalIgnoreCase) &&
                                    !addlStrFiles.Contains(filename, StringComparer.OrdinalIgnoreCase) &&
                                    !abbrev.Equals(filename, StringComparison.OrdinalIgnoreCase))
                                {
                                    picFiles.Add(filename);
                                }
                            }
                            fileExt = "*.";
                        }
                    }
                }
                else
                {
                    var fileExt = "*.";

                    foreach (var file in Directory.EnumerateFiles(filePath, fileExt).ToList())
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
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        return picFiles;
    }

    private void ExportPicFiles()
    {
        if (pcType != CommodoreAbb && pcType != IbmAbb) // (pcType != AtariAbb &&
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

    private static (byte, byte, byte) CGAtoRGB(ushort c, ushort palette)
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
                    color = (0xAA, 0xAA, 0x00); // amber
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
                    color = (0xAA, 0xAA, 0xAA); // lt.gray
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
                color = ConsoleColor.DarkYellow;
                break;
            case 9:
                color = ConsoleColor.DarkYellow;
                break;
            case 10:
                color = ConsoleColor.Red;
                break;
            case 11:
                color = ConsoleColor.DarkGray;
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

    // IBM or C64 only for now:
    //   C64 uses Multicolor 160x200 x 16-color mode
    //   IBM uses CGA graphics mode 320x200 x 4-color x 2-palettes in mode 4; pictures are drawn doubled in width
    // TODO: Figure out format for other ports
    //   AST uses 320x200 x 16-color; pictures are drawn doubled in width
    private void DrawPic(string abbrev, string filePath = "", bool toFile = false)
    {
        var doSixel = false;
        if (Sixel.IsSupported())
            doSixel = true;
        
        var offset = IBM_DECODE_OFFSET; // For IBM, the first 6 bytes have palette colors and height/width
        if (pcType == AtariAbb)
            offset = AST_PALETTE_OFFSET; // For AST, the section from 0x06 to 0x53 is a ramped palette
        else if (pcType == CommodoreAbb)
            offset = C64_DECODE_OFFSET; // For C64, there's a section at 0x0a after the height/width again
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
            if (pcType == IbmAbb)
            {
                if (i == 0)
                    palette = b;
                else if (i == 1)
                {
                    intensity = (byte)(b >> 4);
                    bgColor = (byte)(b & 0x0F);
                }
            }
            else if (i == 4 && b < 0xC9)
                height = b;
            else if (i == 5 && b < 0xA1)
                width = b;
            else if (pcType == CommodoreAbb && i == 6)
                c64Unknown = b;
            if (!toFile)
            {
                if (i == offset)
                {
                    if (pcType == IbmAbb)
                    {
                        Console.WriteLine($"\n [{pcType} CGA Palette: {(palette == 0 ? "GRY" : (palette == 1 ? "CMW" : palette))}, " +
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
                if (pcType == CommodoreAbb && lastB == height && b == width)
                {
                    if (section > 0)
                    {
                        if (section == 1)
                            Console.Write($"\n [{pcType} WxH: {width}x{height}]"); //$", Unknown: {c64Unknown}]");
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
                else if (pcType == AtariAbb && i == AST_DECODE_OFFSET - 1)
                {
                    offset = i % 16 + 1;
                    Console.Write($"\nPalette: ");
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
            if (pcType == AtariAbb)
            {
                //image = DecodeASTPic(fileArray, toFile, doSixel);
                //image.SaveAsPng(Path.GetFileName(filePath) + ".png");
            }
            else if (pcType == CommodoreAbb)
                image = DecodeC64Pic(fileArray, toFile, doSixel);
            else if (pcType == IbmAbb)
                image = DecodeIBMPic(fileArray, toFile, doSixel);

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

    public static ushort Make4ColorMap(byte[] data, int section, ref List<(int, int, int, int)> map)
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

            rc = AddRunToColorMap(section, colors, run, ref map, ref blockIdx);
            if (rc != 0) return rc;

            if (i <= dataLength - 3)
            {
                // Run-Length #2 (Byte2 Low Nibble) for Byte3 Colors #2
                run = b & 0x0F;

                // Color 2A (Byte3 High Nibble) and Color 2B (Byte3 Low Nibble)
                b = data[i + 2];
                colors = (b >> 4, b & 0x0F);

                rc = AddRunToColorMap(section, colors, run, ref map, ref blockIdx);
                if (rc != 0) return rc;
            }
        }

        return 0;
    }

    public static ushort AddRunToColorMap(int section, (int colorA, int colorB) colors, int run, ref List<(int color1, int color2, int color3, int color4)> map, ref int blockIdx)
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

    // "Ramped" color palette where the components are separated (R0, R1, R2... G0, G1, G2... B0, B1, B2...)
    public Rgb24[] GetASTPalette(byte[] paletteData)
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
    
    public Image<Rgb24> DecodeC64Pic(byte[] data, bool toFile, bool doSixel)
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

        if (width < 1 || width > 160 || height < 1 || height > 200)
        {
            if (!toFile)
                Console.WriteLine($"\nERROR: This is not a {CommodoreName} image: height or width is out of range.");
            return new(1, 1);
        }

        var sectionNum = 1;
        var dataLength = data.Length;

        List<byte> section = [];

        var offset = C64_DECODE_OFFSET;
        for (; offset < dataLength - 1; offset++)
        {
            // If the next two bytes are a header (hh ww), this section is done.
            if (offset + 1 < dataLength && data[offset] == height && data[offset + 1] == width)
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
                Console.WriteLine($"\nERROR: This is not a {CommodoreName} image: 4 sections required, only {sectionData.Count} found.");
            return new(1, 1);
        }

        var numBlocks = width / 4 * height / 8;

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
        ushort rc = Make4ColorMap(sectionData[1], 1, ref colorMap);
        if (rc == 0)
            rc = Make4ColorMap(sectionData[2], 2, ref colorMap);
        if (rc != 0)
        {
            if (!toFile)
                Console.WriteLine($"\nERROR: This is not a {CommodoreName} image: run-length overflow.");
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

    public Image<Rgb24> DecodeIBMPic(byte[] data, bool toFile, bool doSixel)
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
                Console.WriteLine($"\nERROR: This is not an {IbmName} image: palette is {palette} (should be zero or one).");
            return new(1, 1);
        }
        if (width < 1 || width > 160 || height < 1 || height > 200)
        {
            if (!toFile)
                Console.WriteLine($"\nERROR: This is not an {IbmName} image: height or width is out of bounds: {width}x{height}.");
            return new(1, 1);
        }
        if (!toFile && !doSixel)
        {
            Console.WriteLine();
            Console.WriteLine(Divider);
            Console.Clear();
        }

        i = IBM_DECODE_OFFSET;
        Dictionary<(int, int), (byte, byte, byte)> pixmap = [];
        ushort[][] cols = [[0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0]];
        ushort[] hs = [1, 1, 1, 1];

        foreach (var b in data[IBM_DECODE_OFFSET..])
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
                    pixmap.TryAdd(new(x + 0, y), CGAtoRGB(cols[0][0], palette));
                    pixmap.TryAdd(new(x + 1, y), CGAtoRGB(cols[0][1], palette));
                    pixmap.TryAdd(new(x + 2, y), CGAtoRGB(cols[0][2], palette));
                    pixmap.TryAdd(new(x + 3, y), CGAtoRGB(cols[0][3], palette));

                    if (!toFile && !doSixel && (y + 3) < Console.WindowHeight && (x + 3) < Console.WindowWidth)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = CGAToConsoleColor(cols[0][0], palette);
                        Console.Write("█");
                        Console.ForegroundColor = CGAToConsoleColor(cols[0][1], palette);
                        Console.Write("█");
                        Console.ForegroundColor = CGAToConsoleColor(cols[0][2], palette);
                        Console.Write("█");
                        Console.ForegroundColor = CGAToConsoleColor(cols[0][3], palette);
                        Console.Write("█");
                    }
                    y++;
                    if (y >= height)
                    {
                        y = 0;
                        x += 4;
                    }
                }
                for (var j = 0; j < hs[1]; j++)
                {
                    pixmap.TryAdd(new(x + 0, y), CGAtoRGB(cols[1][0], palette));
                    pixmap.TryAdd(new(x + 1, y), CGAtoRGB(cols[1][1], palette));
                    pixmap.TryAdd(new(x + 2, y), CGAtoRGB(cols[1][2], palette));
                    pixmap.TryAdd(new(x + 3, y), CGAtoRGB(cols[1][3], palette));

                    if (!toFile && !doSixel && (y + 3) < Console.WindowHeight && (x + 3) < Console.WindowWidth)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = CGAToConsoleColor(cols[1][0], palette);
                        Console.Write("█");
                        Console.ForegroundColor = CGAToConsoleColor(cols[1][1], palette);
                        Console.Write("█");
                        Console.ForegroundColor = CGAToConsoleColor(cols[1][2], palette);
                        Console.Write("█");
                        Console.ForegroundColor = CGAToConsoleColor(cols[1][3], palette);
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
                        row[x] = new Rgb24(color.Item1, color.Item2, color.Item3);
                    else
                        row[x] = new Rgb24(0, 0, 0);
                }
            }
        });

        return img;
    }
}