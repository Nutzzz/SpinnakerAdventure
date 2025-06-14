using System.IO.Compression;
using System.Text;
using NAudio.Midi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

const string divider     = "---------------------------------------";
const string mainTitle   = "SPINNAKER ADVENTURE LANGUAGE TESTER";
const string titleDecor  = "**";
const string goodbye     = "Game over.";

const string expTag      = " [experimental]";
const string switchMenu  = "Switch computer type";
const string exportPngMenu = "Export all pictures to PNG";
const string exportMidMenu = "Export all audio to MIDI";
const string backMenu    = "Back to Main Menu";
const string quitMenu    = "Quit";
const string prompt      = "Press number/letter of an option to proceed:";
const string keypress    = "Press Esc or B for back, Q to quit, or any key when ready for more.";
const string fpWarn      = "[Warning: There may be false positives.]";
const string fileprompt  = "Enter a filename, or press Enter to continue:";
const string keyError    = "Error: Bad entry.";

const string picMenu     = "View pictures";
const string sndMenu     = "Play audio";
const string strMenu     = "Strings";
const string strExpMenu  = "Strings (expanded)";

const string apple2Abb   = "AII";
const string apple2Type  = "Apple II";
const string atariStAbb  = "AST";
const string atariStType = "Atari ST";
const string commodoreAbb = "C64";
const string commodoreType = "Commodore 64";
const string ibmAbb      = "IBM";
const string ibmType     = "IBM PC (default)";
const string macAbb      = "MAC";
const string macType     = "Macintosh";
const string msxAbb      = "MSX";
const string msxType     = "MSX";

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

const string rscPath     = @"Resources";

const byte DELIM = 0x00;
const byte FIRST_PITCH_BYTE = 0xC2;
const int FIRST_MIDI = 45;  // A2
const int LAST_MIDI = 127;  // FF=106=Bb7
const int FIRST_OCTAVE = 3; // to nearest C
const int PATCH = 80;       // "Lead 1 (square)"

const ConsoleKey key0 = ConsoleKey.D0, key1 = ConsoleKey.D1, key2 = ConsoleKey.D2, key3 = ConsoleKey.D3,
                 key4 = ConsoleKey.D4, key5 = ConsoleKey.D5, key6 = ConsoleKey.D6, key7 = ConsoleKey.D7,
                 key8 = ConsoleKey.D8, key9 = ConsoleKey.D9, keySwitch = ConsoleKey.S, keyExpPng = ConsoleKey.P,
                 keyExpMid = ConsoleKey.M, keyBack = ConsoleKey.B, keyQuit = ConsoleKey.Q, keyEsc = ConsoleKey.Escape;

var pcType = "IBM";

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
        ZipFile.ExtractToDirectory(zipfile, rscPath, overwriteFiles: false);
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
    var funcPath = Path.Combine(rscPath, abbrev + pcType, abbrev + ".T");
    try
    {
        if (!File.Exists(funcPath))
        {
            Console.Error.WriteLine($"Error: File not found: {funcPath}");
            funcPath = Path.Combine(rscPath, abbrev, abbrev + ".T");
            if (!File.Exists(funcPath))
            {
                return;
            }
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
    var vocabPath = Path.Combine(rscPath, abbrev + pcType, abbrev + ".V");
    try
    {
        if (!File.Exists(vocabPath))
        {
            Console.Error.WriteLine($"Error: File not found: {vocabPath}");
            vocabPath = Path.Combine(rscPath, abbrev, abbrev + ".V");
            if (!File.Exists(vocabPath))
            {
                return;
            }
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
    var fileFound = false;
    Console.WriteLine(fpWarn);
    foreach (var file in GetPicFiles(abbrev))
    {
        Console.WriteLine(file);
        fileFound = true;
    }
    if (!fileFound)
        return;
    var input = "";
    do
    {
        Console.Write(fileprompt);
        input = Console.ReadLine();
        if (!string.IsNullOrEmpty(input))
        {
            var filePath = Path.Combine(rscPath, abbrev + pcType, input);
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(rscPath, abbrev, input);
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: File not found: {filePath}");
                    break;
                }
            }
            DrawPic(abbrev, filePath, toFile: false);
        }
        else
            Console.WriteLine(divider);
    }
    while (!string.IsNullOrEmpty(input));
}

void ShowSoundFiles(string abbrev)
{
    var fileFound = false;
    if (pcType != "IBM")
        Console.WriteLine(fpWarn);
    foreach (var file in GetSoundFiles(abbrev))
    {
        Console.WriteLine(file);
        fileFound = true;
    }
    if (!fileFound)
        return;
    var input = "";
    do
    {
        Console.Write(fileprompt);
        input = Console.ReadLine();
        if (!string.IsNullOrEmpty(input))
        {
            var filePath = Path.Combine(rscPath, abbrev + pcType, input);
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(rscPath, abbrev, input);
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: File not found: {filePath}");
                    break;
                }
            }
            PlaySound(abbrev, filePath);
        }
        else
            Console.WriteLine(divider);
    }
    while (!string.IsNullOrEmpty(input));
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

        var filePath = Path.Combine(rscPath, abbrev + pcType, locName);
        try
        {
            if (!File.Exists(filePath))
            {
                if (pcType.Equals("MSX") && File.Exists(filePath + ".STR"))
                    filePath += ".STR";
                else
                {
                    Console.WriteLine($"Error: File not found: {filePath}");
                    filePath = Path.Combine(rscPath, abbrev, locName);
                    if (!File.Exists(filePath))
                    {
                        continue;
                    }
                }
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
                        Console.WriteLine($"{i:x3}: {entry.ToUpperInvariant()} [gfx]");
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
                            Console.WriteLine($"{i:x3}: {entry.ToUpperInvariant()} [sfx]");
                            break;
                        }
                    }
                }
                if (!found)
                    Console.WriteLine($"{i:x3}: \"{entry}\"");
                i++;
            }
            Console.WriteLine(divider);
            Console.WriteLine(keypress);
            var k = Console.ReadKey(intercept: true);
            if (k.Key.Equals(keyEsc) ||
                k.Key.Equals(keyBack))
            {
                break;
            }
            else if (k.Key.Equals(keyQuit))
                DoExit();

        }
        catch (Exception) { }
    }
}

Dictionary<ushort, string> GetExe(string abbrev, byte[] start, byte[] end)
{
    Dictionary<ushort, string> exeDict = [];
    var exePath = Path.Combine(rscPath, abbrev + pcType, abbrev + ".EXE");
    if (pcType.Equals("MSX"))
        exePath = Path.Combine(rscPath, abbrev + pcType, "AVENTURA.COM");
    try
    {
        if (!File.Exists(exePath))
        {
            Console.Error.WriteLine($"Error: File not found: {exePath}");
            exePath = Path.Combine(rscPath, abbrev, abbrev + ".EXE");
            if (!File.Exists(exePath))
            {
                return exeDict;
            }
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
            {
                Console.WriteLine($"{e}");
            }
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
    if (pcType.Equals("MSX"))
    {
        Console.WriteLine("GetLocs(): AVENTURA.COM parsing not yet implemented for MSX");
        return locs;
    }
    var locPath = Path.Combine(rscPath, abbrev + pcType, "DIR");
    if (abbrev.Equals("AMB") ||
        abbrev.Equals("PMN"))
    {
        locPath = Path.Combine(rscPath, abbrev + pcType, abbrev + ".DIB");
    }
    try
    {
        if (!File.Exists(locPath))
        {
            Console.Error.WriteLine($"Error: File not found: {locPath}");
            locPath = Path.Combine(rscPath, abbrev, abbrev + ".DIB");
            if (abbrev.Equals("AMB") ||
                abbrev.Equals("PMN"))
            {
                locPath = Path.Combine(rscPath, abbrev, abbrev + ".DIB");
            }
            if (!File.Exists(locPath))
            {
                return locs;
            }
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
    byte b = 0x80;
    var tokenPath = Path.Combine(rscPath, abbrev + pcType, abbrev + ".TOK");
    try
    {
        if (!File.Exists(tokenPath))
        {
            Console.Error.WriteLine($"Error: File not found: {tokenPath}");
            tokenPath = Path.Combine(rscPath, abbrev, abbrev + ".TOK");
            if (!File.Exists(tokenPath))
            {
                return tokens;
            }
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
        locNames.Add(loc.Value.Item2);
    }
    try
    {
        foreach (var file in Directory.EnumerateFiles(Path.Combine(rscPath, abbrev + pcType), "*.").ToList())
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
    catch (Exception) { }
    return picFiles;
}

List<string> GetSoundFiles(string abbrev)
{
    List<string> sndFiles = [];
    try
    {
        if (pcType == "IBM")
        {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(rscPath, abbrev + pcType), "*.IB").ToList())
            {
                var filename = Path.GetFileName(file);
                sndFiles.Add(filename);
            }
            foreach (var file in Directory.EnumerateFiles(Path.Combine(rscPath, abbrev + pcType), "*.JR").ToList())
            {
                var filename = Path.GetFileName(file);
                sndFiles.Add(filename);
            }
            return sndFiles;
        }
        if (pcType == "AST")
        {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(rscPath, abbrev + pcType), "*.MST").ToList())
            {
                var filename = Path.GetFileName(file);
                sndFiles.Add(filename);
            }
        }
        if (abbrev == "F451")
            abbrev = "F4";
        foreach (var file in Directory.EnumerateFiles(Path.Combine(rscPath, abbrev + pcType), abbrev + "*.").ToList())
        {
            var filename = Path.GetFileName(file);
            if (filename.Equals(abbrev, StringComparison.OrdinalIgnoreCase) ||
                (abbrev == "F4" && (filename.Equals("F451", StringComparison.OrdinalIgnoreCase) ||
                    filename.Length < 3 || !char.IsAsciiDigit(filename[2]))))
                continue;
            sndFiles.Add(filename);
        }
    }
    catch (Exception) { }
    return sndFiles;
}

void ExportPicFiles()
{
    if (pcType != "IBM")
        return;
    Console.WriteLine("Please wait ...");
    foreach (var abbrev in new[] { amazonAbb, dragonAbb, f451Abb, amberAbb, perryAbb, ramaAbb, islandAbb, ozAbb })
    {
        var picFiles = GetPicFiles(abbrev);
        foreach (var file in picFiles)
        {
            DrawPic(abbrev, Path.Combine(rscPath, abbrev + pcType, file), toFile: true);
        }
    }
}

void ExportSndFiles()
{
    if (pcType != "AST" && pcType != "C64" && pcType != "IBM")
        return;
    Console.WriteLine("Please wait ...");
    foreach (var abbrev in new[] { amazonAbb, dragonAbb, f451Abb, amberAbb, perryAbb, ramaAbb, islandAbb, ozAbb })
    {
        var sndFiles = GetSoundFiles(abbrev);
        foreach (var file in sndFiles)
        {
            PlaySound(abbrev, Path.Combine(rscPath, abbrev + pcType, file), toFile: true);
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
    const byte MIN = 0x7F;
    bool accent = false;
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
            else if (b == 0x22) // escape quotation marks
            {
                result.Write(Encoding.ASCII.GetBytes("\\\""));
            }
            else if (b == 0x7E) // tilde
                accent = true;
            else if (accent)
                accent = false;

            n = 0;
            var output = b;
            switch (b)
            {
                case 0x61: // a
                    output = 0xE1;
                    break;
                case 0x65: // e
                    output = 0xE9;
                    break;
                case 0x69: // i
                    output = 0xED;
                    break;
                case 0x6E: // n
                    output = 0xF1;
                    break;
                case 0x6F: // o
                    output = 0xF3;
                    break;
                case 0x75: // u
                    output = 0xFA;
                    break;
            }
            result.Write(output);
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

// This implementation is monophonic only, plays each channel one at a time
// (This one sounds more authentic than MIDI, but the timing doesn't work as well)
void WaveOut(int beatLen, List<(int Ch, int Pos, byte B, int Midi, double Freq, int Length)> notes)
{
    var ch = 0;
    var square = new SignalGenerator()
    {
        Gain = 0.1,
        Type = SignalGeneratorType.Square,
    };
    using var wave = new WaveOutEvent();
    
    foreach (var note in notes)
    {
        var nibble1 = (byte)((note.B & 0xF0) >> 4);
        var nibble2 = (byte)((note.B & 0x0F) - 1);
        if (note.B == 0x80)
            continue;

        var rest = false;
        var freq = note.Freq;
        var midi = note.Midi;
        var len = note.Length;
        if (nibble1 == 0x8 && nibble2 != 0x0)
        {
            rest = true;
            freq = 0; // set frequency to 0 if this is a rest
            midi = 0; // set MIDI note to 0 if this is a rest
        }
        var lenStr = GetNoteLengthName(len);
        Console.WriteLine($"{note.Pos:x3} : {note.B:x2} {(len > 0 ? (rest ? "r" : midi > 0 ? GetNoteName(midi) : "") : midi > 0 ? "*" : "")}\t{lenStr}\tFreq:{Math.Round(freq, 2),7}\tMIDI:{midi,3}");
        // Not polyphonic, play next track separately
        if (note.Ch != ch)
        {
            ch = note.Ch;
            if (ch > 1)
                Thread.Sleep(1000);
            Console.WriteLine("Channel " + ch);
        }
        square.Frequency = freq;
        wave.Init(square.Take(TimeSpan.FromSeconds(2)));
        wave.Play();
        while (wave.PlaybackState == PlaybackState.Playing)
        {
            Thread.Sleep(note.Length * beatLen);
            wave.Stop();
            // TODO: Stop on keyboard input
        }
    }
}

// This implementation is monophonic only, plays each channel one at a time
void MidiOut(int beatLen, List<(int Ch, int Pos, byte B, int Midi, double Freq, int Length)> notes)
{
    var ch = 0;
    using var midiOut = new MidiOut(0);
    for (int i = 1; i < 17; i++)
    {
        midiOut.Send(MidiMessage.ChangePatch(PATCH, i).RawData);
    }

    foreach (var note in notes)
    {
        var nibble1 = (byte)((note.B & 0xF0) >> 4);
        var nibble2 = (byte)((note.B & 0x0F) - 1);

        var rest = false;
        var freq = note.Freq;
        var midi = note.Midi;
        var len = note.Length;
        if (nibble1 == 0x8 && nibble2 != 0x0)
        {
            rest = true;
            freq = 0; // set frequency to 0 if this is a rest
            midi = 0; // set MIDI note to 0 if this is a rest
        }
        var lenStr = GetNoteLengthName(len);
        Console.WriteLine($"{note.Pos:x3} : {note.B:x2} {(len > 0 ? (rest ? "r" : midi > 0 ? GetNoteName(midi) : "") : midi > 0 ? "*" : "")}\t{lenStr}\tMIDI:{midi,3}\tFreq:{Math.Round(freq, 2),7}");
        // Not polyphonic, play next track separately
        if (note.Ch != ch)
        {
            ch = note.Ch;
            if (ch > 1)
                Thread.Sleep(1000);
            Console.WriteLine("Channel " + ch);
        }
        if (!rest && note.Midi > 0 && note.Ch > 0)
            midiOut.Send(MidiMessage.StartNote(note.Midi, 127, note.Ch).RawData);
        Thread.Sleep(note.Length * beatLen);
        if (!rest && note.Midi > 0 && note.Ch > 0)
            midiOut.Send(MidiMessage.StopNote(note.Midi, 0, note.Ch).RawData);
    }
}

void WriteMidi(int beatLen, List<(int Ch, int Pos, byte B, int Midi, double Freq, int Length)> notes, string filePath = "")
{
    var multiTrack = 0;
    if (Path.GetExtension(filePath).Equals(".jr", StringComparison.OrdinalIgnoreCase))
        multiTrack = 1;
    IList<MidiEvent> track = [];

    const int TICKS_PER_QTR = 120;  // ticks per quarter note
    const int DEF_VELOC = 127;      // 127=maximum velocity
    var ch = 0;
    long time = 0;

    MidiEventCollection events = new(multiTrack, TICKS_PER_QTR);

    foreach (var note in notes)
    {
        var nibble1 = (byte)((note.B & 0xF0) >> 4);
        var nibble2 = (byte)((note.B & 0x0F) - 1);

        var midi = note.Midi;
        if (note.Ch != ch)
        {
            time = 0;
            if (multiTrack == 1 && note.Ch < 17)
            {
                if (ch > 0)
                    track.Add(new MetaEvent(MetaEventType.EndTrack, 0, time)); // end prior track
                ch = note.Ch;
            }
            else
                ch = 1;

            track = events.AddTrack([new PatchChangeEvent(0, ch, PATCH)]);

            if (ch == 1)
            {
                var tempo = beatLen * TICKS_PER_QTR * 18;
                Console.WriteLine($"Beat Length: {beatLen} / Tempo: {tempo * 4} ms \u00bc={Math.Round(60000 / (double)(beatLen * 32), 2)} bpm");
                track.Add(new TempoEvent(tempo, 0)); // in ms per quarter note
            }
        }
        if (ch > 0)
        {
            if (nibble1 == 0x8)
                time += note.Length * beatLen;
            else if (midi > 0)
            {
                var duration = note.Length * beatLen;
                track.Add(new NoteOnEvent(time, ch, midi, DEF_VELOC, duration));
                time += duration;
                track.Add(new NoteEvent(time, ch, MidiCommandCode.NoteOff, midi, 0));
            }
        }
    }
    track.Add(new MetaEvent(MetaEventType.EndTrack, 0, time));
    if (ch == 1)
        events.MidiFileType = 0;
    events.PrepareForExport();
    MidiFile.Export($"{filePath}.mid", events);
}

double GetFreqOffset(int n)
{
    return (n % 12) switch
    {
        -3 => 27.5000,   // A0
        -2 => 29.1352,   // Bb0
        -1 => 30.8677,   // B0
        0 => 32.7032,   // C1
        1 => 34.6478,   // C#1
        2 => 36.7081,   // D1
        3 => 38.8909,   // Eb1
        4 => 41.2034,   // E1
        5 => 43.6535,   // F1
        6 => 46.2493,   // F#1
        7 => 48.9994,   // G1
        8 => 51.9131,   // G#1
        9 => 55.0000,   // A1
        10 => 58.2705,  // Bb1
        11 => 61.7354,  // B1
        //9 => 65.4064,   // C2
        //10 => 69.2957,  // C#2
        //11 => 73.4162,  // D2
        _ => 0,
    };
}

int GetMidiNote(byte b) // where e.g., 0xC2 -> 57 (A3)
{
    if (b >= FIRST_PITCH_BYTE)
        return b - FIRST_PITCH_BYTE + FIRST_MIDI;
    return FIRST_MIDI;
}

double GetFreq(int midi)
{
    var offset = midi - FIRST_MIDI - 3;  // Need to go to the previous A note
    var oct = offset / 12 + FIRST_OCTAVE;
    return GetFreqOffset(offset) * Math.Pow(2, oct);
}

string GetNoteName(int midi) // where 57 -> A3 (0xC2)
{
    var offset = midi - FIRST_MIDI - 3;  // Need to go to the previous A note
    var oct = offset / 12 + FIRST_OCTAVE;
    if (oct == FIRST_OCTAVE && offset < 0)
        oct--;
    return (offset % 12) switch
    {
        -3 => "A" + oct,
        -2 => "Bb" + oct,
        -1 => "B" + oct,
        0 => "C" + oct,
        1 => "C#" + oct,
        2 => "D" + oct,
        3 => "Eb" + oct,
        4 => "E" + oct,
        5 => "F" + oct,
        6 => "F#" + oct,
        7 => "G" + oct,
        8 => "G#" + oct,
        9 => "A" + oct,
        10 => "Bb" + oct,
        11 => "B" + oct,
        _ => "-" + oct,
    };
}

string GetNoteLengthName(int len, string last = "")
{
    return (len) switch
    {
        <1 => "",
        2 => last != " 1/8" ? " 1/8" : "",
        4 => last != " 1/4" ? " 1/4" : "",
        6 => last != " 3/8" ? " 3/8" : "",
        8 => last != " 1/2" ? " 1/2" : "",
        10 => last != " 5/8" ? " 5/8" : "",
        12 => last != " 3/4" ? " 3/4" : "",
        14 => last != " 7/8" ? " 7/8" : "",
        16 => last != " 1" ? " 1" : "",
        32 => last != " 2" ? " 2" : "",
        48 => last != " 3" ? " 3" : "",
        64 => last != " 4" ? " 4" : "",
        80 => last != " 5" ? " 5" : "",
        96 => last != " 6" ? " 6" : "",
        112 => last != " 7" ? " 7" : "",
        128 => last != " 8" ? " 8" : "",
        _ => last != $" {len}/16" ? $" {len}/16" : "",
    };
}

// The PC Speaker was monophonic, and the PCjr was quadraphonic (though these games only uses the 3 waveform channels; the fourth noise channel isn't used)
void PlaySound(string abbrev, string filePath = "", bool toFile = false)
{
    bool control = true;
    bool pitch = false;
    bool rest = false;
    byte[] array = [];
    //byte[] ibmNewCh = [0x50, 0x00, 0x08, 0x40, 0x00, 0x80];
    var midiNote = 0;
    var beatLen = 48;
    var channel = 0;
    var numCtrl = 0;
    double freq = 0f;
    List<int> lengths = [];
    List<(int Ch, int Pos, byte B, int Midi, double Freq, int Length)> notes = [];
    try
    {
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Error: File not found: {filePath}");
            return;
        }
        array = File.ReadAllBytes(filePath);
    }
    catch (Exception) { }
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
        pitch = false;
        rest = false;
        if ((pcType == "IBM" && i == 0x02) ||
            (pcType == "C64" && i == 0x04))             // beat time length (gives tempo)
            beatLen = b * 16;
        else if ((pcType == "IBM" && i > 0x0A && i < 0x1A) ||
            pcType == "C64" && i > 0x0C && i < 0x1C)    // note lengths
            lengths.Add(b);
        else if ((pcType == "IBM" && i > 0x19) ||
            (pcType == "C64" && i > 0x1B))              // note data
        {
            var len = 0;
            var nibble1 = (byte)((b & 0xF0) >> 4);
            var nibble2 = (byte)(b & 0x0F);

            if (b == 0x50 || b == 0x00 || b == 0x40 || b == 0x20)
            {
                if (!control)
                    numCtrl = 0;
                numCtrl++;
                control = true;
            }
            else if (control == true && numCtrl > 1 &&
                (b == 0x08 || b == 0x05 || b == 0x0F)) // these could be control codes or note data
            {
                // ignore these
            }
            else if (control == true && numCtrl > 5)
            {
                if (b == 0x80 || b == 0xC5) // new channel
                {
                    channel++;
                    // we shouldn't have more than 3 channels, but just in case don't use the percussion channel
                    if (channel == 10)
                        channel++;
                    if (channel > 16)
                        channel = 16;
                }
                else if (control == true && numCtrl > 5)
                {
                    if (b >= FIRST_PITCH_BYTE)
                    {
                        control = false;
                        pitch = true;
                        midiNote = GetMidiNote(b);
                    }
                    // Sometimes a rest occurs before first pitch is set
                    else if (nibble1 == 0x8 && nibble2 > 0 && nibble2 <= lengths.Count)
                    {
                        rest = true;
                        len = lengths[nibble2 - 1];
                    }
                    // I don't think we should get here with properly formatted files
                    else
                        control = false;
                }
            }
            // TODO: What about the weird 0x10s and 0x20s?
            else if (numCtrl == 1)
            {
                control = false;
                pitch = true;
                numCtrl = 0;
                if ((nibble1 & 0x08) == 0)  // positive nibble (< 0x8)
                {
                    // 0:+0 octaves, 1:+1 octaves, etc.?
                    midiNote += nibble1 * 12 + nibble2;
                }
                else if (nibble1 > 0x8)     // negative nibble (> 0x8)
                {
                    // F:-0 octaves, E:-1 octaves, etc.?
                    midiNote -= ((0xF - nibble1) * 12) + (0x10 - nibble2);
                }
            }
            else
            {
                control = false;
                numCtrl = 0;
                if ((nibble1 & 0x08) == 0)  // positive nibble (< 0x8)
                    midiNote += nibble1;
                else if (nibble1 > 0x8)     // negative nibble (> 0x8)
                    midiNote -= 0xF - nibble1 + 1;  // F:-1, E:-2, D:-3, C:-4, B:-5, A:-6, 9:-7
                else //if (nibble1 == 0x8)  // rest
                    rest = true;

                if (nibble2 > 0 && nibble2 <= lengths.Count)
                    len = lengths[nibble2 - 1];
                else
                    len = 0;
            }

            if (midiNote < FIRST_MIDI)
                midiNote = FIRST_MIDI;
            if (midiNote > LAST_MIDI)
                midiNote = LAST_MIDI;
            freq = GetFreq(midiNote);

            if (pitch)
                notes.Add((channel, i, b, midiNote, freq, 0));
            else if (rest) // rests might occur before pitch is set (while control=true)
                notes.Add((channel, i, b, 0, 0, len));
            else if (control)
                notes.Add((channel, i, b, 0, 0, 0));
            else
                notes.Add((channel, i, b, midiNote, freq, len));
        }

        if (!toFile)
        {
            if (i % 16 == 0)
            {
                Console.WriteLine();
                Console.Write($"{i:x3} | ");
            }
            Console.Write($"{b:x2} ");
#if DEBUG
            if (i == 0x02)
            {
                Console.WriteLine();
                Console.WriteLine($"      [beatLen: {beatLen}]");
                Console.Write("000 | -- -- -- ");
            }
            else if (i == 0x19)
            {
                Console.WriteLine();
                Console.Write($"      [lengths: ");
                foreach (var len in lengths)
                {
                    Console.Write(len + ",");
                }
                Console.WriteLine("]");
                Console.Write("010 | -- -- -- -- -- -- -- -- -- -- ");
            }
#endif
        }
        i++;
    }

    if (notes.Count == 0)
        return;

    if (toFile)
    {
        WriteMidi(beatLen, notes, filePath);
        return;
    }

    Console.WriteLine();
    Console.WriteLine(divider);
    MidiOut(beatLen, notes);
    Console.WriteLine(divider);
    return;
}

// The CGA medium-resolution graphics mode used here for PC is 320x200 x 4-color, with 2 possible palettes in mode 4
void DrawPic(string abbrev, string filePath = "", bool toFile = false)
{
    byte OFFSET = 0x06; // The first 6 bytes have palette colors and height/width
    ushort x = 0, y = 0, w = 0, h = 0, palette = 0;
    byte[] array = [];
    if (!toFile)
        Console.Clear();

    try
    {
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Error: File not found: {filePath}");
            return;
        }
        array = File.ReadAllBytes(filePath);
    }
    catch (Exception) { }
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
            if (i == OFFSET)
            {
                Console.WriteLine($" [Palette: {(palette == 0 ? "GRY" : (palette == 1 ? "CMW" : palette))}, WxH: {w * 2}x{h}]");
                Console.Write    ("000 | ");
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
    while (key?.Key != keyEsc && key?.Key != keyBack && key?.Key != keyQuit &&
           key?.Key != keySwitch && key?.Key != keyExpPng && key?.Key != keyExpMid &&
           key?.Key != key0 && key?.Key != key1 && key?.Key != key2 && key?.Key != key3 &&
           key?.Key != key4 && key?.Key != key5 && key?.Key != key6 && key?.Key != key7 &&
           key?.Key != key8 && key?.Key != key9)
    {
        if (key is not null)
            Console.Error.WriteLine(keyError);

        Console.WriteLine(divider);
        Console.WriteLine();
        Console.WriteLine($"{titleDecor}{title}{titleDecor}");
        var i = 0;
        foreach (var option in options)
        {
            if (i < 10)
                Console.WriteLine($" {i}. {option}");
            i++;
        }
        Console.WriteLine();
        if (main)
        {
            if (pcType == "IBM")
                Console.WriteLine($" P. {exportPngMenu}");
            if (pcType == "AST" || pcType == "C64" || pcType == "IBM")
                Console.WriteLine($" M. {exportMidMenu}{expTag}");
            Console.WriteLine($" S. {switchMenu} ({pcType}){expTag}");
        }
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
            case key0:
                DoAmazonMenu();
                break;
            case key1:
                DoDragonMenu();
                break;
            case key2:
                DoF451Menu();
                break;
            case key3:
                DoAmberMenu();
                break;
            case key4:
                DoPerryMenu();
                break;
            case key5:
                DoRamaMenu();
                break;
            case key6:
                DoIslandMenu();
                break;
            case key7:
                DoOzMenu();
                break;
            case keySwitch:
                DoSwitchMenu();
                break;
            case keyExpPng:
                ExportPicFiles();
                break;
            case keyExpMid:
                ExportSndFiles();
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
            case key0:
                Console.WriteLine(amazonMenu1);
                ShowExe(amazonAbb, null, null);
                break;
            case key1:
                Console.WriteLine(amazonMenu2);
                ShowVocab(amazonAbb);
                break;
            case key2:
                Console.WriteLine(amazonMenu3);
                ShowLocs(amazonAbb);
                break;
            /*
            case key2:
                Console.WriteLine(amazonMenu3);
                ShowFunctions(amazonAbb);
                break;
            */
            case key3:
                Console.WriteLine(picMenu);
                ShowPicFiles(amazonAbb);
                break;
            case key4:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(amazonAbb);
                break;
            case key5:
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
            case key0:
                Console.WriteLine(dragonMenu1);
                ShowExe(dragonAbb,
                    [0x50, 0x57, 0xE8, 0xC9, 0xD2, 0x83, 0xC4, 0x06, 0x57, 0xE8, 0x3C, 0xDB, 0x59, 0x8B, 0xC7, 0xC3,
                    0x64, 0x67, 0x77], // "dgw"
                    [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
                break;
            case key1:
                Console.WriteLine(dragonMenu2);
                ShowLocs(dragonAbb);
                break;
            /*
            case key2:
                Console.WriteLine(dragonMenu3);
                ShowFunctions(dragonAbb);
                break;
            case key3:
                Console.WriteLine(dragonMenu5);
                ShowVocab(dragonAbb);
                break;
            */
            case key2:
                Console.WriteLine(picMenu);
                ShowPicFiles(dragonAbb);
                break;
            case key3:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(dragonAbb);
                break;
            case key4:
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
            case key0:
                Console.WriteLine(f451Menu1);
                ShowLocs(f451Abb);
                break;
            case key1:
                Console.WriteLine(f451Menu2);
                ShowExe(f451Abb, null, null);
                break;
            case key2:
                Console.WriteLine(f451Menu3);
                ShowVocab(f451Abb);
                break;
            /*
            case key2:
                Console.WriteLine(f451Menu3);
                ShowFunctions(f451Abb);
                break;
            */
            case key3:
                Console.WriteLine(picMenu);
                ShowPicFiles(f451Abb);
                break;
            case key4:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(f451Abb);
                break;
            case key5:
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
            case key0:
                Console.WriteLine(amberMenu1);
                ShowLocs(amberAbb);
                break;
            case key1:
                Console.WriteLine(amberMenu2);
                ShowExe(amberAbb,
                    [0x61, 0x6D, 0x62, 0x2E, 0x20, 0x20, 0x20],                     // start: "amb.   "
                    [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
                break;
            case key2:
                Console.WriteLine(amberMenu3);
                ShowFunctions(amberAbb);
                break;
            case key3:
                Console.WriteLine(amberMenu4);
                ShowTokens(amberAbb);
                break;
            case key4:
                Console.WriteLine(amberMenu5);
                ShowVocab(amberAbb);
                break;
            case key5:
                Console.WriteLine(picMenu);
                ShowPicFiles(amberAbb);
                break;
            case key6:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(amberAbb);
                break;
            case key7:
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
            case key0:
                Console.WriteLine(perryMenu1);
                ShowLocs(perryAbb);
                break;
            case key1:
                Console.WriteLine(perryMenu2);
                ShowExe(perryAbb, null, null);
                break;
            case key2:
                Console.WriteLine(perryMenu3);
                ShowFunctions(perryAbb);
                break;
            case key3:
                Console.WriteLine(perryMenu4);
                ShowVocab(perryAbb);
                break;
            case key4:
                Console.WriteLine(picMenu);
                ShowPicFiles(perryAbb);
                break;
            case key5:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(perryAbb);
                break;
            case key6:
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
            case key0:
                Console.WriteLine(ramaMenu1);
                ShowLocs(ramaAbb);
                break;
            case key1:
                Console.WriteLine(ramaMenu2);
                ShowExe(ramaAbb, null, null);
                break;
            /*
            case key2:
                Console.WriteLine(ramaMenu3);
                ShowFunctions(ramaAbb);
                break;
            case key3:
                Console.WriteLine(ramaMenu5);
                ShowVocab(ramaAbb);
                break;
            */
            case key2:
                Console.WriteLine(picMenu);
                ShowPicFiles(ramaAbb);
                break;
            case key3:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(ramaAbb);
                break;
            case key4:
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
            case key0:
                Console.WriteLine(islandMenu1);
                ShowLocs(islandAbb);
                break;
            case key1:
                Console.WriteLine(islandMenu2);
                ShowExe(islandAbb, null, null);
                break;
            case key2:
                Console.WriteLine(islandMenu3);
                ShowVocab(islandAbb);
                break;
            /*
            case key2:
                Console.WriteLine(islandMenu3);
                ShowFunctions(islandAbb);
                break;
            */
            case key3:
                Console.WriteLine(picMenu);
                ShowPicFiles(islandAbb);
                break;
            case key4:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(islandAbb);
                break;
            case key5:
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
            case key0:
                Console.WriteLine(ozMenu1);
                ShowLocs(ozAbb);
                break;
            case key1:
                Console.WriteLine(ozMenu2);
                ShowExe(ozAbb, null, null);
                break;
            case key2:
                Console.WriteLine(ozMenu3);
                ShowFunctions(ozAbb);
                break;
            case key3:
                Console.WriteLine(ozMenu4);
                ShowVocab(ozAbb);
                break;
            case key4:
                Console.WriteLine(picMenu);
                ShowPicFiles(ozAbb);
                break;
            case key5:
                Console.WriteLine(sndMenu);
                ShowSoundFiles(ozAbb);
                break;
            case key6:
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

void DoSwitchMenu()
{
    while (true)
    {
        var key = ShowMenu(main: false, title: switchMenu, apple2Type, atariStType, commodoreType, ibmType, macType, msxType);

        switch (key)
        {
            case key0:
                Console.WriteLine(apple2Type);
                pcType = apple2Abb;
                break;
            case key1:
                Console.WriteLine(atariStType);
                pcType = atariStAbb;
                break;
            case key2:
                Console.WriteLine(commodoreType);
                pcType = commodoreAbb;
                break;
            case key3:
                Console.WriteLine(ibmType);
                pcType = ibmAbb;
                break;
            case key4:
                Console.WriteLine(macType);
                pcType = macAbb;
                break;
            case key5:
                Console.WriteLine(msxType);
                pcType = msxAbb;
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
        if (key == key0 || key == key1 || key == key2 || key == key3 || key == key4 || key == key5 ||
            //key == key6 || key == key7 || key == key8 || key == key9
            key == keyBack || key == keyEsc)
        {
            break;
        }
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