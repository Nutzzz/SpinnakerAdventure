using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO.Compression;
using System.Text;

const string divider     = "---------------------------------------";
const string mainTitle   = "SPINNAKER ADVENTURE LANGUAGE TESTER";
const string titleDecor  = "**";
const string goodbye     = "Game over.";

const string exportMenu  = "Export all pictures to PNG";
const string backMenu    = "Back to Main Menu";
const string quitMenu    = "Quit";
const string prompt      = "Press number/letter of an option to proceed:";
const string keypress    = "Press Esc or B for back, Q to quit, or any key when ready for more.";
const string picWarn     = "[Warning: There may be false positives.]";
const string fileprompt  = "Enter a filename, or press Enter to continue:";
const string keyError    = "Error: Bad entry.";

const string picMenu     = "View pictures";
const string sndMenu     = "Audio filenames";
const string strMenu     = "Strings";
const string strExpMenu  = "Strings (expanded)";

// Amazon
const string amazonName  = "Amazon";
const string amazonAbb   = "AMZ";
const string amazonMenu1 = "AMZ.EXE: Misc";
const string amazonMenu2 = "AMZ.V: Vocabulary";
const string amazonMenu3 = "DIR: Location directory";
var amazonTitle = amazonName.ToUpperInvariant();

// Dragonworld
const string dragonName  = "Dragonworld";
const string dragonAbb   = "DGW";
const string dragonMenu1 = "DGW.EXE: Misc";
const string dragonMenu2 = "DIR: Location directory";
var dragonTitle = dragonName.ToUpperInvariant();

// Fahrenheit 451
const string f451Name    = "Fahrenheit 451";
const string f451Abb     = "F451";
const string f451Menu1   = "DIR: Location directory";
const string f451Menu2   = "F451.EXE: Misc";
const string f451Menu3   = "F451.V: Vocabulary";
var f451Title = f451Name.ToUpperInvariant();

// Nine Princes in Amber
const string amberName   = "Nine Princes in Amber";
const string amberAbb    = "AMB";
const string amberMenu1  = "AMB.DIB: Location directory";
const string amberMenu2  = "AMB.EXE: Misc";
const string amberMenu3  = "AMB.T: Functions";
const string amberMenu4  = "AMB.TOK: String tokens";
const string amberMenu5  = "AMB.V: Vocabulary";
var amberTitle = amberName.ToUpperInvariant();

// Perry Mason: The Case of the Mandarin Murder
const string perryName   = "Perry Mason";
const string perryAbb    = "PMN";
const string perryMenu1  = "PMN.DIB: Location directory";
const string perryMenu2  = "PMN.EXE: Misc";
const string perryMenu3  = "PMN.T: Functions";
const string perryMenu4  = "PMN.V: Vocabulary";
var perryTitle = perryName.ToUpperInvariant();

// Rendezvous with Rama
const string ramaName    = "Rendezvous with Rama";
const string ramaAbb     = "RDV";
const string ramaMenu1   = "DIR: Location directory";
const string ramaMenu2   = "RDV.EXE: Misc";
var ramaTitle = ramaName.ToUpperInvariant();

// Treasure Island
const string islandName  = "Treasure Island";
const string islandAbb   = "TRI";
const string islandMenu1 = "DIR: Location directory";
const string islandMenu2 = "TRI.EXE: Misc";
const string islandMenu3 = "TRI.V: Vocabulary";
var islandTitle = islandName.ToUpperInvariant();

// The Wizard of Oz
const string ozName      = "The Wizard of Oz";
const string ozAbb       = "WOZ";
const string ozMenu1     = "DIR: Location directory";
const string ozMenu2     = "WOZ.EXE: Misc";
const string ozMenu3     = "WOZ.T: Functions";
const string ozMenu4     = "WOZ.V: Vocabulary";
var ozTitle = ozName.ToUpperInvariant();

const string rscPath     = @"Resources\";

const byte DELIM = 0x00;

const ConsoleKey key1 = ConsoleKey.D1, key2 = ConsoleKey.D2, key3 = ConsoleKey.D3, key4 = ConsoleKey.D4,
                 key5 = ConsoleKey.D5, key6 = ConsoleKey.D6, key7 = ConsoleKey.D7, key8 = ConsoleKey.D8,
                 key9 = ConsoleKey.D9, keyExport = ConsoleKey.X, keyBack = ConsoleKey.B, keyQuit = ConsoleKey.Q,
                 keyEsc = ConsoleKey.Escape;

List<string> addlStrFiles =
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

var zipfile = Path.Combine(rscPath, "Resources.zip");
try
{
    if (File.Exists(zipfile))
    {
        Console.WriteLine("Extracting Resources.zip ...");
        ZipFile.ExtractToDirectory(zipfile, rscPath, overwriteFiles: true);
    }
}
catch (Exception) { }

DoMainMenu();
DoExit();

void ShowLocs(string abbrev)
{
    foreach (var loc in GetLocs(abbrev))
    {
        Console.WriteLine($"{loc.Key:x2}: disk {loc.Value.Item1} : [{loc.Value.Item2}]");
    }
}

void ShowExe(string abbrev, byte[]? start, byte[]? end)
{
    start ??= Encoding.ASCII.GetBytes(abbrev + ".   ");
    end ??= [DELIM, DELIM];
    foreach (var entry in GetExe(abbrev, start, end))
    {
        Console.WriteLine($"[{entry}]");
    }
}

void ShowFunctions(string abbrev)
{
    const byte OFFSET = 0x06;
    byte[] funcEnd = [0x00, 0x00, 0x00];
    ushort i = 0;
    var funcPath = Path.Combine(rscPath, abbrev, abbrev + ".T");
    try
    {
        if (!File.Exists(funcPath))
        {
            Console.Error.WriteLine("Error: File not found!");
            return;
        }
        var t = File.ReadAllBytes(funcPath);
        if (t.Length < (OFFSET + 1))
            return;
        foreach (var func in Split(t[OFFSET..], [DELIM]))
        {
            if (func.Equals(funcEnd))
                break;
            Console.WriteLine($"{i:x3}: [{Encoding.ASCII.GetString(func)}]");
            i++;
        }
    }
    catch (Exception) { }
}

void ShowTokens(string abbrev)
{
    foreach (var token in GetTokens(abbrev))
    {
        Console.WriteLine($"{token.Key:x2}: [{token.Value}]");
    }
}

void ShowVocab(string abbrev)
{
    const byte OFFSET = 0x3C;
    byte[] vDelim = [0x80, 0x8C]; // This represents the part of speech, but acts as delimiter as well here
    ushort i = 0;
    var vocabPath = Path.Combine(rscPath, abbrev, abbrev + ".V");
    try
    {
        if (!File.Exists(vocabPath))
        {
            Console.Error.WriteLine("Error: File not found!");
            return;
        }
        var vocab = File.ReadAllBytes(vocabPath);
        foreach (var word in Split(vocab[OFFSET..], vDelim, range: true))
        {
            if (word[0] > 0 && word[0] < word.Count)
            {
                var num = word[^1] - 0x80;
                var pos = LookupPartOfSpeech(abbrev, num);
                Console.WriteLine($"{i:x3}: char[{word[0]}] = \"{Encoding.ASCII.GetString(word[1..(word[0] + 1)])}\" : 0x{SegmentToHex(word[(word[0] + 1)..^1])} ({pos})");
            }
            i++;
        }
    }
    catch (Exception) { }
}

void ShowPicFiles(string abbrev)
{
    Console.WriteLine(picWarn);
    foreach (var file in GetPicFiles(abbrev))
    {
        Console.WriteLine(file);
    }
    var input = "";
    do
    {
        Console.Write(fileprompt);
        input = Console.ReadLine();
        if (!string.IsNullOrEmpty(input))
        {
            var filePath = Path.Combine(rscPath, abbrev, input);
            try
            {
                if (!File.Exists(filePath))
                    Console.WriteLine("Error: File not found!");
                else
                    Draw(abbrev, filePath, toFile: false);
            }
            catch (Exception) { }
        }
        else
            Console.WriteLine(divider);
    }
    while (!string.IsNullOrEmpty(input));
}

void ShowSoundFiles(string abbrev)
{
    foreach (var file in GetSoundFiles(abbrev))
    {
        Console.WriteLine(file);
    }
}

void ShowStrings(string abbrev, byte start, bool expand = false)
{
    var picFiles = GetPicFiles(abbrev);
    var sndFiles = GetSoundFiles(abbrev);
    foreach (var loc in GetLocs(abbrev))
    {
        var locName = loc.Value.Item2;
        Console.WriteLine(divider);
        Console.WriteLine(locName.ToUpperInvariant());
        var filePath = Path.Combine(rscPath, abbrev, locName).ToUpperInvariant();
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                continue;
            }
            var i = 0;
            var array = File.ReadAllBytes(filePath);
            Dictionary<byte, string> tokens = [];
            if (expand)
                tokens = GetTokens(abbrev, start);
            foreach (var entry in ProcessStrings(array, tokens))
            {
                var found = false;
                foreach (var file in picFiles)
                {
                    if (entry.Equals(file, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        Console.WriteLine($"{i:x3}: {entry.ToUpperInvariant()} (gfx)");
                        break;
                    }
                }
                if (!found)
                {
                    foreach (var file in sndFiles)
                    {
                        if (entry.Equals(Path.GetFileNameWithoutExtension(file), StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            Console.WriteLine($"{i:x3}: {entry.ToUpperInvariant()} (sfx)");
                            break;
                        }
                    }
                }
                if (!found)
                    Console.WriteLine($"{i:x3}: [{entry}]");
                i++;
            }
            Console.WriteLine(divider);
            Console.WriteLine(keypress);
            var k = Console.ReadKey(intercept: true);
            if (k.Key.Equals(keyEsc) ||
                k.Key.Equals(keyBack))
                break;
            else if (k.Key.Equals(keyQuit))
                DoExit();

        }
        catch (Exception) { }
    }
}

Dictionary<ushort, string> GetExe(string abbrev, byte[] start, byte[] end)
{
    Dictionary<ushort, string> exeDict = [];
    var exePath = Path.Combine(rscPath, abbrev, abbrev + ".EXE");
    try
    {
        if (!File.Exists(exePath))
        {
            Console.Error.WriteLine("Error: File not found!");
            return exeDict;
        }
        var exe = File.ReadAllBytes(exePath);
        var begin = false;
        ushort i = 0;
        foreach (var entry in Split(exe, [DELIM]))
        {
            if (begin)
            {
                if (entry.Equals(end))
                    break;
                exeDict.Add(i, Encoding.ASCII.GetString(entry));
                i++;
            }
            else if (entry.Equals(start))
                begin = true;
            var e = Encoding.ASCII.GetString(entry);
            if (!string.IsNullOrEmpty(e) &&
                !e.Equals("?") &&
                !e.Contains("??"))
                Console.WriteLine($"{e}");
        }
    }
    catch (Exception) { }

    return exeDict;
}

Dictionary<ushort, (char, string)> GetLocs(string abbrev)
{
    const byte LOC_END = 0x1A;
    byte[] locDelim = [0x0D, 0x0A];
    Dictionary<ushort, (char, string)> locs = [];
    ushort i = 0;
    var locPath = Path.Combine(rscPath, abbrev, "DIR");
    if (abbrev.Equals("AMB", StringComparison.OrdinalIgnoreCase) ||
        abbrev.Equals("PMN", StringComparison.OrdinalIgnoreCase))
        locPath = Path.Combine(rscPath, abbrev, abbrev + ".DIB");
    try
    {
        if (!File.Exists(locPath))
        {
            Console.Error.WriteLine("Error: File not found!");
            return locs;
        }
        var dir = File.ReadAllBytes(locPath);
        foreach (var loc in Split(dir, locDelim))
        {
            if (loc[0].Equals(LOC_END))
                break;
            var str = Encoding.ASCII.GetString(loc);
            locs.Add(i, (str[0], str[2..])); // format is "<disk>:<locname>" where <disk> = a-d
            i++;
        }
    }
    catch (Exception) { }

    return locs;
}

Dictionary<byte, string> GetTokens(string abbrev, int start = 0) // Only necessary for Amber
{
    const int OFFSET = 0x102;
    Dictionary<byte, string> tokens = [];
    /*
    Dictionary<byte, string> tokens = new()
    {
        { 0x80, "you" } // This is unfortunate because 0x80 is also 'z'.
    };
    */
    var begin = false;
    byte b = 0x80;
    var tokenPath = Path.Combine(rscPath, abbrev, abbrev + ".TOK");
    try
    {
        if (!File.Exists(tokenPath))
        {
            Console.Error.WriteLine("Error: File not found!");
            return tokens;
        }
        var tok = File.ReadAllBytes(tokenPath);
        if (tok.Length < (OFFSET + 1))
            return tokens;
        foreach (var token in Split(tok[OFFSET..], [DELIM]))
        {
            tokens.Add(b, Encoding.ASCII.GetString(token));
            b++;
        }
    }
    catch (Exception) { }

    return tokens;
}

List<string> GetPicFiles(string abbrev)
{
    List<string> picFiles = [];
    List<string> locNames = [];
    foreach (var loc in GetLocs(abbrev))
    {
        locNames.Add(loc.Value.Item2.ToLowerInvariant());
    }
    foreach (var file in Directory.EnumerateFiles(Path.Combine(rscPath, abbrev), "*.").ToList())
    {
        var filename = Path.GetFileName(file);
        if (!locNames.Contains(filename.ToLowerInvariant()) &&
            !addlStrFiles.Contains(filename.ToUpperInvariant()) &&
            !abbrev.Equals(filename.ToUpperInvariant()))
            picFiles.Add(filename);
    }
    return picFiles;
}

List<string> GetSoundFiles(string abbrev)
{
    List<string> sndFiles = [];
    foreach (var file in Directory.EnumerateFiles(Path.Combine(rscPath, abbrev), "*.IB").ToList())
    {
        var filename = Path.GetFileName(file);
        sndFiles.Add(filename);
    }
    foreach (var file in Directory.EnumerateFiles(Path.Combine(rscPath, abbrev), "*.JR").ToList())
    {
        var filename = Path.GetFileName(file);
        sndFiles.Add(filename);
    }
    return sndFiles;
}

void ExportPicFiles()
{
    Console.WriteLine("Please wait...");
    foreach (var abbrev in new[] { amazonAbb, dragonAbb, f451Abb, amberAbb, perryAbb, ramaAbb, islandAbb, ozAbb })
    {
        var picFiles = GetPicFiles(abbrev);
        foreach (var file in picFiles)
        {
            Draw(abbrev, Path.Combine(rscPath, abbrev, file), toFile: true);
        }
    }
}

// TODO: Use .exe to generate this rather than using enums
string LookupPartOfSpeech(string abbrev, int val)
{
    if (val > -1 && val < 13)
    {
        return abbrev switch
        {
            "AMZ" => Enum.GetNames<PartSpeechAmz>()[val],
            "DGW" or "AMB" => Enum.GetNames<PartSpeechDgwAmb>()[val],
            "F451" or "TRI" => Enum.GetNames<PartSpeechF451Tri>()[val],
            "PMN" => Enum.GetNames<PartSpeechPmn>()[val],
            "RDV" => Enum.GetNames<PartSpeechRdv>()[val],
            "WOZ" => Enum.GetNames<PartSpeechWoz>()[val],
            _ => "",
        };
    }
    return val.ToString();
}

string SegmentToHex(ArraySegment<byte> segment)
{
    StringBuilder hexString = new();
    for (int i = segment.Offset; i < segment.Offset + segment.Count; i++)
    {
        hexString.Append(segment.Array?[i].ToString("x2"));
    }
    return hexString.ToString();
}

IEnumerable<string> ProcessStrings(byte[] array, Dictionary<byte, string> tokens)
{
    const byte FIRST = 0x0B;
    const byte MIN = 0x79;
    MemoryStream stream = new();
    BinaryWriter result = new(stream);
    ushort i = 0;
    ushort n = 0;
    foreach (var b in array)
    {
        if (i >= FIRST)
        {
            if (b.Equals(DELIM))
            {
                if (n > 1) // third 0x00, where 0x000000 = end of string portion]
                    break;
                else if (n < 1)
                {
                    if (stream.Length > 1)
                        yield return Encoding.ASCII.GetString(stream.ToArray());
                    stream.Close();
                    stream = new();
                    result = new(stream);
                }
                n++;
                continue;
            }
            else if (b > MIN) // char > 'z'
            {
                n = 0;
                if (tokens.TryGetValue(b, out var str))
                    result.Write(Encoding.ASCII.GetBytes(" " + str));
                continue;
            }
            n = 0;
            result.Write(b);
        }
        else
            i++;
    }
    stream.Close();
}

static List<ArraySegment<byte>> Split(byte[] array, byte[] delim, bool range = false)
{
    var segList = new List<ArraySegment<byte>>();
    var segStart = 0;
    for (ushort i = 0, j = 0; i < array.Length; i++)
    {
        if (range && delim.Length > 1)
        {
            if (array[i] >= delim[0] && array[i] <= delim[1])
            {
                var segLen = i - segStart + 1;
                if (segLen > 0)
                    segList.Add(new ArraySegment<byte>(array, segStart, segLen));
                segStart = i + 1;
            }
        }
        else
        {
            if (array[i] != delim[j])
            {
                if (j == 0)
                    continue;
                j = 0;
            }
            if (array[i] == delim[j])
                j++;

            if (j == delim.Length)
            {
                var segLen = (i + 1) - segStart - delim.Length;
                if (segLen > 0)
                    segList.Add(new ArraySegment<byte>(array, segStart, segLen));
                segStart = i + 1;
                j = 0;
            }
        }
    }

    if (segStart < array.Length)
        segList.Add(new ArraySegment<byte>(array, segStart, array.Length - segStart));

    return segList;
}

ConsoleColor GetCColor(ushort c, ushort palette)
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

(byte, byte, byte) GetRGBColor(ushort c, ushort palette)
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

// The CGA medium-resolution graphics mode is 320x200 x 4-color, with 2 possible palettes in mode 4;
void Draw(string abbrev, string filePath = "", bool toFile = false)
{
    byte OFFSET = 0x06; // The first 6 bytes have palette colors, some unknown stuff, and height/width
    ushort x = 0, y = 0, w = 0, h = 0, palette = 0;
    byte[] array = [];
    Console.Clear();
    try
    {
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine("Error: File not found!");
            return;
        }
        array = File.ReadAllBytes(filePath);
    }
    catch (Exception) { }
    ushort i = 0;
    if (!toFile)
    {
        Console.WriteLine("Off | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
        Console.Write("----+------------------------------------------------");
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
            if (i == OFFSET)
            {
                Console.WriteLine($" [Palette: {(palette == 0 ? "GRY" : (palette == 1 ? "CMW" : palette))}, WxH: {w * 2}x{h}]");
                Console.Write("000 | ");
                for (ushort j = 0; j < OFFSET; j++)
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
        Console.WriteLine(divider);
        Console.Clear();
    }

    i = OFFSET;
    Dictionary<(int, int), (byte, byte, byte)> pixmap = [];
    ushort[][] cols = [[0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0], [0, 0, 0, 0]];
    ushort[] hs = [1, 1, 1, 1];
    foreach (var b in array[OFFSET..])
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
    Console.ForegroundColor = ConsoleColor.Gray;
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
    img.SaveAsPng(filePath + ".png");
}
void DoExit(string error = "")
{
    if (!string.IsNullOrEmpty(error))
    {
        Console.Error.WriteLine(error);
        Environment.Exit(-1);
    }
    Console.WriteLine(goodbye);
    Environment.Exit(0);
}

ConsoleKey? ShowMenu(bool main, string title, params string[] options)
{
    ConsoleKeyInfo? key = null;
    while (key?.Key != keyEsc && key?.Key != keyBack && key?.Key != keyQuit && key?.Key != keyExport &&
           key?.Key != key1 && key?.Key != key2 && key?.Key != key3 &&
           key?.Key != key4 && key?.Key != key5 && key?.Key != key6 &&
           key?.Key != key7 && key?.Key != key8 && key?.Key != key9)
    {
        if (key is not null)
            Console.Error.WriteLine(keyError);

        Console.WriteLine(divider);
        Console.WriteLine();
        Console.WriteLine($"{titleDecor}{title}{titleDecor}");
        var i = 1;
        foreach (var option in options)
        {
            if (i < 10)
                Console.WriteLine($" {i}. {option}");
            i++;
        }
        Console.WriteLine();
        if (main)
            Console.WriteLine($" X. {exportMenu}");
        else
            Console.WriteLine($" B. {backMenu}");
        Console.WriteLine($" Q. {quitMenu}");
        Console.WriteLine();
        Console.Write(prompt);
        key = Console.ReadKey(intercept: true);

        Console.WriteLine();
        Console.WriteLine(divider);
    }
    return key?.Key;
}

void DoMainMenu()
{
    while (true)
    {
    var key = ShowMenu(main: true, title: mainTitle, amazonName, dragonName, f451Name, amberName, perryName, ramaName, islandName, ozName);

        switch (key)
        {
            case key1:
                DoAmazonMenu();
                break;
            case key2:
                DoDragonMenu();
                break;
            case key3:
                DoF451Menu();
                break;
            case key4:
                DoAmberMenu();
                break;
            case key5:
                DoPerryMenu();
                break;
            case key6:
                DoRamaMenu();
                break;
            case key7:
                DoIslandMenu();
                break;
            case key8:
                DoOzMenu();
                break;
            case keyExport:
                ExportPicFiles();
                break;
            case keyBack:
            case keyQuit:
            case keyEsc:
                break;
            default:
                break;
        }
        if (key == keyBack || key == keyQuit || key == keyEsc)
            break;
    }
}

void DoAmazonMenu()
{
    while (true)
    {
        var key = ShowMenu(main: false, title: amazonTitle, amazonMenu1, amazonMenu2, amazonMenu3, picMenu, sndMenu, strMenu);

        switch (key)
        {
            case key1:
                Console.WriteLine(amazonMenu1);
                ShowExe(amazonAbb, null, null);
                break;
            case key2:
                Console.WriteLine(amazonMenu2);
                ShowVocab(amazonAbb);
                break;
            case key3:
                Console.WriteLine(amazonMenu3);
                ShowLocs(amazonAbb);
                break;
            /*
            case key3:
                Console.WriteLine(amazonMenu3);
                ShowFunctions(amazonAbb);
                break;
            */
            case key4:
                Console.WriteLine(picMenu);
                ShowPicFiles(amazonAbb);
                break;
            case key5:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(amazonAbb);
                break;
            case key6:
                Console.WriteLine(strMenu);
                ShowStrings(amazonAbb, 0x03, expand: false);
                break;
            case keyBack:
            case keyEsc:
                break;
            case keyQuit:
                DoExit();
                break;
            default:
                break;
        }
        if (key == keyBack || key == keyEsc)
            break;
    }
}

void DoDragonMenu()
{
    while (true)
    {
        var key = ShowMenu(main: false, title: dragonTitle, dragonMenu1, dragonMenu2, picMenu, sndMenu, strMenu);
        switch (key)
        {
            case key1:
                Console.WriteLine(dragonMenu1);
                ShowExe(dragonAbb,
                    [0x50, 0x57, 0xE8, 0xC9, 0xD2, 0x83, 0xC4, 0x06, 0x57, 0xE8, 0x3C, 0xDB, 0x59, 0x8B, 0xC7, 0xC3,
                    0x64, 0x67, 0x77], // "dgw"
                    [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
                break;
            case key2:
                Console.WriteLine(dragonMenu2);
                ShowLocs(dragonAbb);
                break;
            /*
            case key3:
                Console.WriteLine(dragonMenu3);
                ShowFunctions(dragonAbb);
                break;
            case key4:
                Console.WriteLine(dragonMenu5);
                ShowVocab(dragonAbb);
                break;
            */
            case key3:
                Console.WriteLine(picMenu);
                ShowPicFiles(dragonAbb);
                break;
            case key4:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(dragonAbb);
                break;
            case key5:
                Console.WriteLine(strMenu);
                ShowStrings(dragonAbb, 0x03, expand: false);
                break;
            case keyBack:
            case keyEsc:
                break;
            case keyQuit:
                DoExit();
                break;
            default:
                break;
        }
        if (key == keyBack || key == keyEsc)
            break;
    }
}

void DoF451Menu()
{
    while (true)
    {
        var key = ShowMenu(main: false, title: f451Title, f451Menu1, f451Menu2, f451Menu3, picMenu, sndMenu, strMenu);
        switch (key)
        {
            case key1:
                Console.WriteLine(f451Menu1);
                ShowLocs(f451Abb);
                break;
            case key2:
                Console.WriteLine(f451Menu2);
                ShowExe(f451Abb, null, null);
                break;
            case key3:
                Console.WriteLine(f451Menu3);
                ShowVocab(f451Abb);
                break;
            /*
            case key3:
                Console.WriteLine(f451Menu3);
                ShowFunctions(f451Abb);
                break;
            */
            case key4:
                Console.WriteLine(picMenu);
                ShowPicFiles(f451Abb);
                break;
            case key5:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(f451Abb);
                break;
            case key6:
                Console.WriteLine(strMenu);
                ShowStrings(f451Abb, 0x03, expand: false);
                break;
            case keyBack:
            case keyEsc:
                break;
            case keyQuit:
                DoExit();
                break;
            default:
                break;
        }
        if (key == keyBack || key == keyEsc)
            break;
    }
}

void DoAmberMenu()
{
    while (true)
    {
        var key = ShowMenu(main: false, title: amberTitle, amberMenu1, amberMenu2, amberMenu3, amberMenu4, amberMenu5, picMenu, sndMenu, strExpMenu);
        switch (key)
        {
            case key1:
                Console.WriteLine(amberMenu1);
                ShowLocs(amberAbb);
                break;
            case key2:
                Console.WriteLine(amberMenu2);
                ShowExe(amberAbb,
                    [0x61, 0x6D, 0x62, 0x2E, 0x20, 0x20, 0x20],                     // start: "amb.   "
                    [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
                break;
            case key3:
                Console.WriteLine(amberMenu3);
                ShowFunctions(amberAbb);
                break;
            case key4:
                Console.WriteLine(amberMenu4);
                ShowTokens(amberAbb);
                break;
            case key5:
                Console.WriteLine(amberMenu5);
                ShowVocab(amberAbb);
                break;
            case key6:
                Console.WriteLine(picMenu);
                ShowPicFiles(amberAbb);
                break;
            case key7:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(amberAbb);
                break;
            case key8:
                Console.WriteLine(strExpMenu);
                ShowStrings(amberAbb, 0x02, expand: true);
                break;
            case keyBack:
            case keyEsc:
                break;
            case keyQuit:
                DoExit();
                break;
            default:
                break;
        }
        if (key == keyBack || key == keyEsc)
            break;
    }
}

void DoPerryMenu()
{
    while (true)
    {
        var key = ShowMenu(main: false, title: perryTitle, perryMenu1, perryMenu2, perryMenu3, perryMenu4, picMenu, sndMenu, strMenu);
        switch (key)
        {
            case key1:
                Console.WriteLine(perryMenu1);
                ShowLocs(perryAbb);
                break;
            case key2:
                Console.WriteLine(perryMenu2);
                ShowExe(perryAbb, null, null);
                break;
            case key3:
                Console.WriteLine(perryMenu3);
                ShowFunctions(perryAbb);
                break;
            case key4:
                Console.WriteLine(perryMenu4);
                ShowVocab(perryAbb);
                break;
            case key5:
                Console.WriteLine(picMenu);
                ShowPicFiles(perryAbb);
                break;
            case key6:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(perryAbb);
                break;
            case key7:
                Console.WriteLine(strMenu);
                ShowStrings(perryAbb, 0x03, expand: false);
                break;
            case keyBack:
            case keyEsc:
                break;
            case keyQuit:
                DoExit();
                break;
            default:
                break;
        }
        if (key == keyBack || key == keyEsc)
            break;
    }
}

void DoRamaMenu()
{
    while (true)
    {
        var key = ShowMenu(main: false, title: ramaTitle, ramaMenu1, ramaMenu2, picMenu, sndMenu, strMenu);
        switch (key)
        {
            case key1:
                Console.WriteLine(ramaMenu1);
                ShowLocs(ramaAbb);
                break;
            case key2:
                Console.WriteLine(ramaMenu2);
                ShowExe(ramaAbb, null, null);
                break;
            /*
            case key3:
                Console.WriteLine(ramaMenu3);
                ShowFunctions(ramaAbb);
                break;
            case key4:
                Console.WriteLine(ramaMenu5);
                ShowVocab(ramaAbb);
                break;
            */
            case key3:
                Console.WriteLine(picMenu);
                ShowPicFiles(ramaAbb);
                break;
            case key4:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(ramaAbb);
                break;
            case key5:
                Console.WriteLine(strMenu);
                ShowStrings(ramaAbb, 0x03, expand: false);
                break;
            case keyBack:
            case keyEsc:
                break;
            case keyQuit:
                DoExit();
                break;
            default:
                break;
        }
        if (key == keyBack || key == keyEsc)
            break;
    }
}

void DoIslandMenu()
{
    while (true)
    {
        var key = ShowMenu(main: false, title: islandTitle, islandMenu1, islandMenu2, islandMenu3, picMenu, sndMenu, strMenu);
        switch (key)
        {
            case key1:
                Console.WriteLine(islandMenu1);
                ShowLocs(islandAbb);
                break;
            case key2:
                Console.WriteLine(islandMenu2);
                ShowExe(islandAbb, null, null);
                break;
            case key3:
                Console.WriteLine(islandMenu3);
                ShowVocab(islandAbb);
                break;
            /*
            case key3:
                Console.WriteLine(islandMenu3);
                ShowFunctions(islandAbb);
                break;
            */
            case key4:
                Console.WriteLine(picMenu);
                ShowPicFiles(islandAbb);
                break;
            case key5:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(islandAbb);
                break;
            case key6:
                Console.WriteLine(strMenu);
                ShowStrings(islandAbb, 0x03, expand: false);
                break;
            case keyBack:
            case keyEsc:
                break;
            case keyQuit:
                DoExit();
                break;
            default:
                break;
        }
        if (key == keyBack || key == keyEsc)
            break;
    }
}

void DoOzMenu()
{
    while (true)
    {
        var key = ShowMenu(main: false, title: ozTitle, ozMenu1, ozMenu2, ozMenu3, ozMenu4, picMenu, sndMenu, strMenu);
        switch (key)
        {
            case key1:
                Console.WriteLine(ozMenu1);
                ShowLocs(ozAbb);
                break;
            case key2:
                Console.WriteLine(ozMenu2);
                ShowExe(ozAbb, null, null);
                break;
            case key3:
                Console.WriteLine(ozMenu3);
                ShowFunctions(ozAbb);
                break;
            case key4:
                Console.WriteLine(ozMenu4);
                ShowVocab(ozAbb);
                break;
            case key5:
                Console.WriteLine(picMenu);
                ShowPicFiles(ozAbb);
                break;
            case key6:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(ozAbb);
                break;
            case key7:
                Console.WriteLine(strMenu);
                ShowStrings(ozAbb, 0x03, expand: false);
                break;
            case keyBack:
            case keyEsc:
                break;
            case keyQuit:
                DoExit();
                break;
            default:
                break;
        }
        if (key == keyBack || key == keyEsc)
            break;
    }
}

// Amazon
enum PartSpeechAmz
{
    ppronoun,
    listsep,
    verb,
    noun,
    loneword,
    article,
    adjective,
    adverb,
    prep,
    pronoun,
    delim,
    dansep,
    conj
}

// Dragonworld, Nine Princes in Amber
enum PartSpeechDgwAmb
{
    noun,
    pronoun,
    conj,
    dansep,
    ppronoun,
    listsep,
    delim,
    loneword,
    verb,
    adjective,
    prep,
    adverb,
    article
}

// Fahrenheit 451, Treasure Island
enum PartSpeechF451Tri
{
    article,
    noun,
    verb,
    adverb,
    prep,
    adjective,
    pronoun,
    loneword,
    conj,
    dansep,
    delim,
    listsep,
    ppronoun
}

// Perry Mason
enum PartSpeechPmn
{
    article,
    noun,
    verb,
    adverb,
    prep,
    intpnoun,
    intverb,
    delim,
    adjective,
    pronoun,
    loneword,
    conj,
    listsep,
    dansep,
    ppronoun
}

// Rendezvous with Rama
enum PartSpeechRdv
{
    article,
    noun,
    verb,
    adjective,
    adverb,
    prep,
    pronoun,
    ppronoun,
    loneword,
    delim,
    dansep,
    listsep,
    conj
}

// The Wizard of Oz
enum PartSpeechWoz
{
    loneword,
    noun,
    verb,
    prep,
    adverb,
    adjective,
    article,
    pronoun,
    conj,
    dansep,
    ppronoun,
    listsep,
    delim
}