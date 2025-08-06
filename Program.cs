using System.IO.Compression;
using System.Text;

namespace SASTester;

partial class SASTester
{
    public const string RscPath = @"Resources";

    public const string FilePrompt = "Enter a filename, or press Enter to continue:";
    public const string FilePromptWarn = "[Warning: There may be false positives.]";
    public const string Divider = "---------------------------------------";

    public const string Apple2Abb   = "AII";
    const string Apple2Type         = "Apple II";
    public const string AtariStAbb  = "AST";
    const string AtariStType        = "Atari ST";
    public const string CommodoreAbb = "C64";
    const string CommodoreType      = "Commodore 64";
    public const string IbmAbb      = "IBM";
    const string IbmType            = "IBM PC (default)";
    public const string MacAbb      = "MAC";
    const string MacType            = "Macintosh";
    public const string MsxAbb      = "MSX";
    const string MsxType            = "MSX";

    const string MainTitle   = "SPINNAKER ADVENTURE LANGUAGE TESTER";
    const string TitleDecor  = "**";
    const string Goodbye     = "Game over.";

    const string ExpTag      = " [experimental]";
    const string SwitchMenu  = "Switch computer type";
    const string ExtractPdsMenu = "Extract all container files";
    const string ExportPngMenu = "Export all pictures to PNG";
    const string ExportMidMenu = "Export all audio to MIDI";
    const string BackMenu    = "Back to Main Menu";
    const string QuitMenu    = "Quit";
    const string Prompt      = "Press number/letter of an option to proceed:";
    const string KeyPress    = "Press Esc or B for back, Q to quit, or any key when ready for more.";
    const string KeyError    = "Error: Bad entry.";
    const string FileError   = "Error: File not found:";

    const string PicMenu     = "View pictures";
    const string SndMenu     = "Play audio";
    const string StrMenu     = "Strings";
    const string StrExpMenu  = "Strings (expanded)";
    const string PdsMenu     = "Examine containers";

    public const string AllGamesAbb = "ALL";

    // Amazon
    const string AmazonName         = "Amazon";
    public const string AmazonAbb   = "AMZ";
    const string AmazonMenu1        = "AMZ Executable";
    const string AmazonMenu2        = "AMZ.V: Vocabulary";
    const string AmazonMenu3        = "DIR: Location directory";
    readonly string amazonTitle     = AmazonName.ToUpperInvariant();

    // Dragonworld
    const string DragonName         = "Dragonworld";
    public const string DragonAbb   = "DGW";
    const string DragonMenu1        = "DGW Executable";
    const string DragonMenu2        = "DIR: Location directory";
    readonly string dragonTitle     = DragonName.ToUpperInvariant();

    // Fahrenheit 451
    const string F451Name           = "Fahrenheit 451";
    public const string F451Abb     = "F451";
    const string F451Menu1          = "DIR: Location directory";
    const string F451Menu2          = "F451 Executable";
    const string F451Menu3          = "F451.V: Vocabulary";
    readonly string f451Title       = F451Name.ToUpperInvariant();

    // Nine Princes in Amber
    const string AmberName          = "Nine Princes in Amber";
    public const string AmberAbb    = "AMB";
    const string AmberMenu1         = "AMB.DIB: Location directory";
    const string AmberMenu2         = "AMB Executable";
    const string AmberMenu3         = "AMB.T: Functions";
    const string AmberMenu4         = "AMB.TOK: String tokens";
    const string AmberMenu5         = "AMB.V: Vocabulary";
    readonly string amberTitle      = AmberName.ToUpperInvariant();

    // Perry Mason: The Case of the Mandarin Murder
    const string PerryName          = "Perry Mason";
    public const string PerryAbb    = "PMN";
    const string PerryMenu1         = "PMN.DIB: Location directory";
    const string PerryMenu2         = "PMN Executable";
    const string PerryMenu3         = "PMN.T: Functions";
    const string PerryMenu4         = "PMN.V: Vocabulary";
    readonly string perryTitle      = PerryName.ToUpperInvariant();

    // Rendezvous with Rama
    const string RamaName           = "Rendezvous with Rama";
    public const string RamaAbb     = "RDV";
    const string RamaMenu1          = "DIR: Location directory";
    const string RamaMenu2          = "RDV Executable";
    readonly string ramaTitle       = RamaName.ToUpperInvariant();

    // Treasure Island
    const string IslandName         = "Treasure Island";
    public const string IslandAbb   = "TRI";
    const string IslandMenu1        = "DIR: Location directory";
    const string IslandMenu2        = "TRI Executable";
    const string IslandMenu3        = "TRI.V: Vocabulary";
    readonly string islandTitle     = IslandName.ToUpperInvariant();

    // The Wizard of Oz
    const string OzName             = "The Wizard of Oz";
    public const string OzAbb       = "WOZ";
    const string OzMenu1            = "DIR: Location directory";
    const string OzMenu2            = "WOZ Executable";
    const string OzMenu3            = "WOZ.T: Functions";
    const string OzMenu4            = "WOZ.V: Vocabulary";
    readonly string ozTitle         = OzName.ToUpperInvariant();

    const byte Delim = 0x00;

    const ConsoleKey key0 = ConsoleKey.D0, key1 = ConsoleKey.D1, key2 = ConsoleKey.D2, key3 = ConsoleKey.D3,
                     key4 = ConsoleKey.D4, key5 = ConsoleKey.D5, key6 = ConsoleKey.D6, key7 = ConsoleKey.D7,
                     key8 = ConsoleKey.D8, key9 = ConsoleKey.D9, keyExtPds = ConsoleKey.X, keyExpPng = ConsoleKey.P,
                     keyExpMid = ConsoleKey.M, keySwitch = ConsoleKey.S, keyBack = ConsoleKey.B,
                     keyQuit = ConsoleKey.Q, keyEsc = ConsoleKey.Escape;

    // These enums are used by LookupPartOfSpeech()

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

    public static string pcType = IbmAbb;

    static void Main()
    {
        string zipfile = Path.Combine(RscPath, "Resources.zip");
        try
        {
            if (File.Exists(zipfile))
            {
                Console.WriteLine("Extracting Resources.zip ...");
                ZipFile.ExtractToDirectory(zipfile, RscPath, overwriteFiles: false);
            }
        }
        catch (IOException) {}
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        var tester = new SASTester();
        tester.DoMainMenu();
        DoExit();
    }

    static void ShowLocs(string abbrev)
    {
        foreach (var loc in GetLocs(abbrev))
        {
            Console.WriteLine($"{loc.Key:x2}: disk {loc.Value.Item1} : [{loc.Value.Item2}]");
        }
    }

    static void ShowExe(string abbrev, byte[]? start, byte[]? end)
    {
        start ??= Encoding.ASCII.GetBytes(abbrev + ".   ");
        end ??= [Delim, Delim];
        foreach (var entry in GetExe(abbrev, start, end))
        {
            Console.WriteLine($"[{entry}]");
        }
    }

    static void ShowFunctions(string abbrev)
    {
        const byte Offset = 0x06;
        byte[] funcEnd = [0x00, 0x00, 0x00];
        ushort i = 0;
        var funcPath = "";
        if (pcType == AtariStType)
            funcPath = Path.Combine(RscPath, abbrev + pcType, abbrev, abbrev + ".T");
        else
            funcPath = Path.Combine(RscPath, abbrev + pcType, abbrev + ".T");
        try
        {
            if (!File.Exists(funcPath))
            {
                Console.Error.WriteLine($"{FileError} {funcPath}");
                funcPath = Path.Combine(RscPath, abbrev, abbrev + ".T");
                if (!File.Exists(funcPath))
                {
                    return;
                }
            }
            var t = File.ReadAllBytes(funcPath);
            if (t.Length < (Offset + 1))
                return;
            foreach (var func in Split(t[Offset..], [Delim]))
            {
                if (func.Equals(funcEnd))
                    break;
                Console.WriteLine($"{i:x3}: [{Encoding.ASCII.GetString(func)}]");
                i++;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
    }

    static void ShowTokens(string abbrev)
    {
        foreach (var token in GetTokens(abbrev))
        {
            Console.WriteLine($"{token.Key:x2}: [{token.Value}]");
        }
    }

    static void ShowVocab(string abbrev)
    {
        const byte Offset = 0x3C;
        byte[] vDelim = [0x80, 0x8C]; // This represents the part of speech, but acts as delimiter as well here
        ushort i = 0;
        var vocabPath = "";
        if (pcType == AtariStType)
            vocabPath = Path.Combine(RscPath, abbrev + pcType, abbrev, abbrev + ".V");
        else
            vocabPath = Path.Combine(RscPath, abbrev + pcType, abbrev + ".V");
        try
        {
            if (!File.Exists(vocabPath))
            {
                Console.Error.WriteLine($"{FileError} {vocabPath}");
                vocabPath = Path.Combine(RscPath, abbrev, abbrev + ".V");
                if (!File.Exists(vocabPath))
                {
                    return;
                }
            }
            var vocab = File.ReadAllBytes(vocabPath);
            foreach (var word in Split(vocab[Offset..], vDelim, range: true))
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
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
    }

    void ShowStrings(string abbrev, byte start, bool expand = false)
    {
        var picFiles = GetPicFiles(abbrev);
        var sndFiles = GetSoundFiles(abbrev);
        foreach (var loc in GetLocs(abbrev))
        {
            var locName = loc.Value.Item2;
            Console.WriteLine(Divider);
            Console.WriteLine(locName.ToUpperInvariant());

            var filePath = "";
            if (pcType == AtariStAbb)
                filePath = Path.Combine(RscPath, abbrev + pcType, abbrev, locName);
            else
                filePath = Path.Combine(RscPath, abbrev + pcType, locName);
            try
            {
                if (!File.Exists(filePath))
                {
                    if (pcType == MsxAbb && File.Exists(filePath + ".STR"))
                        filePath += ".STR";
                    else
                    {
                        Console.WriteLine($"{FileError} {filePath}");
                        filePath = Path.Combine(RscPath, abbrev, locName);
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
                    tokens = GetTokens(abbrev);
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
                Console.WriteLine(Divider);
                Console.WriteLine(KeyPress);
                var k = Console.ReadKey(intercept: true);
                if (k.Key.Equals(keyEsc) ||
                    k.Key.Equals(keyBack))
                {
                    break;
                }
                else if (k.Key.Equals(keyQuit))
                    DoExit();

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }

    static Dictionary<ushort, string> GetExe(string abbrev, byte[] start, byte[] end)
    {
        Dictionary<ushort, string> exeDict = [];
        var exePath = "";
        if (pcType == AtariStAbb)
            exePath = Path.Combine(RscPath, abbrev + pcType, abbrev, abbrev);
        else if (pcType == IbmAbb)
            exePath = Path.Combine(RscPath, abbrev + pcType, abbrev + ".EXE");
        else if (pcType == MsxAbb)
            exePath = Path.Combine(RscPath, abbrev + pcType, "AVENTURA.COM");
        else
            exePath = Path.Combine(RscPath, abbrev + pcType, abbrev);

        try
        {
            if (!File.Exists(exePath))
            {
                Console.Error.WriteLine($"{FileError} {exePath}");
                exePath = Path.Combine(RscPath, abbrev, abbrev + ".EXE");
                if (!File.Exists(exePath))
                {
                    return exeDict;
                }
            }
            var exe = File.ReadAllBytes(exePath);
            var begin = false;
            ushort i = 0;
            foreach (var entry in Split(exe, [Delim]))
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
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        return exeDict;
    }

    static Dictionary<ushort, (char, string)> GetLocs(string abbrev)
    {
        const byte LocEnd = 0x1A;
        byte[] locDelim = [0x0D, 0x0A];
        Dictionary<ushort, (char, string)> locs = [];
        ushort i = 0;
        if (pcType == MsxAbb)
        {
            Console.WriteLine("GetLocs(): AVENTURA.COM parsing not yet implemented for MSX");
            return locs;
        }
        var locPath = "";
        if (abbrev == AmberAbb || abbrev == PerryAbb)
        {
            if (pcType == AtariStType)
                locPath = Path.Combine(RscPath, abbrev + pcType, abbrev, abbrev + ".DIB");
            else
                locPath = Path.Combine(RscPath, abbrev + pcType, abbrev + ".DIB");
        }
        else
        {
            if (pcType == AtariStType)
                locPath = Path.Combine(RscPath, abbrev + pcType, abbrev, "DIR");
            else
                locPath = Path.Combine(RscPath, abbrev + pcType, "DIR");
        }
        try
        {
            if (!File.Exists(locPath))
            {
                Console.Error.WriteLine($"{FileError} {locPath}");
                locPath = Path.Combine(RscPath, abbrev, abbrev + ".DIB");
                if (abbrev == AmberAbb || abbrev == PerryAbb)
                {
                    locPath = Path.Combine(RscPath, abbrev, abbrev + ".DIB");
                }
                if (!File.Exists(locPath))
                {
                    return locs;
                }
            }
            var dir = File.ReadAllBytes(locPath);
            foreach (var loc in Split(dir, locDelim))
            {
                if (loc[0].Equals(LocEnd))
                    break;
                var str = Encoding.ASCII.GetString(loc);
                locs.Add(i, (str[0], str[2..])); // format is "<disk>:<locname>" where <disk> = a-d
                i++;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        return locs;
    }

    static Dictionary<byte, string> GetTokens(string abbrev) // Only necessary for Amber
    {
        const int Offset = 0x102;
        Dictionary<byte, string> tokens = [];
        byte b = 0x80;
        var tokenPath = "";
        if (pcType == AtariStType)
            tokenPath = Path.Combine(RscPath, abbrev + pcType, abbrev, abbrev + ".TOK");
        else
            tokenPath = Path.Combine(RscPath, abbrev + pcType, abbrev + ".TOK");
        try
        {
            if (!File.Exists(tokenPath))
            {
                Console.Error.WriteLine($"{FileError} {tokenPath}");
                tokenPath = Path.Combine(RscPath, abbrev, abbrev + ".TOK");
                if (!File.Exists(tokenPath))
                {
                    return tokens;
                }
            }
            var tok = File.ReadAllBytes(tokenPath);
            if (tok.Length < (Offset + 1))
                return tokens;
            foreach (var token in Split(tok[Offset..], [Delim]))
            {
                tokens.Add(b, Encoding.ASCII.GetString(token));
                b++;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        return tokens;
    }

    // TODO: Use .exe to generate this rather than using enums
    static string LookupPartOfSpeech(string abbrev, int val)
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

    static string SegmentToHex(ArraySegment<byte> segment)
    {
        StringBuilder hexString = new();
        for (int i = segment.Offset; i < segment.Offset + segment.Count; i++)
        {
            hexString.Append(segment.Array?[i].ToString("x2"));
        }
        return hexString.ToString();
    }

    static IEnumerable<string> ProcessStrings(byte[] array, Dictionary<byte, string> tokens)
    {
        const byte First = 0x0B;
        const byte Min = 0x7F;
        bool accent = false;
        MemoryStream stream = new();
        BinaryWriter result = new(stream);
        ushort i = 0;
        ushort n = 0;
        foreach (var b in array)
        {
            if (i >= First)
            {
                if (b.Equals(Delim))
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
                else if (b > Min) // char > 'z'
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

    static void ExaminePdsFiles(string abbrev = AllGamesAbb)
    {
        List<string> pdsFiles = [];
        if (pcType == Apple2Abb && (abbrev == AmberAbb || abbrev == AllGamesAbb))
        {
            pdsFiles = [Path.Combine(RscPath, "AMBAII", "GRAPHPDS.a"), Path.Combine(RscPath, "AMBAII", "GRAPHPDS.b"),
                        Path.Combine(RscPath, "AMBAII", "GRAPHPDS.c"), Path.Combine(RscPath, "AMBAII", "GRAPHPDS.d"),
                        Path.Combine(RscPath, "AMBAII", "MUSICPDS.a"), Path.Combine(RscPath, "AMBAII", "MUSICPDS.b"),
                        Path.Combine(RscPath, "AMBAII", "MUSICPDS.c"), Path.Combine(RscPath, "AMBAII", "MUSICPDS.d")];
            //Directory.CreateDirectory(Path.Combine(RscPath, "AMBAII", "PDS"));
        }
        else if (pcType == AtariStAbb)
        {
            if (abbrev == AmberAbb || abbrev == AllGamesAbb)
            {
                pdsFiles.AddRange([Path.Combine(RscPath, "AMBAST", "AMB", "GRAPHPDS"), Path.Combine(RscPath, "AMBAST", "AMB", "MUSICPDS")]);
                //Directory.CreateDirectory(Path.Combine(RscPath, "AMBAST", "AMB", "PDS"));
            }
            if (abbrev == AmazonAbb || abbrev == AllGamesAbb)
            {
                pdsFiles.AddRange([Path.Combine(RscPath, "AMZAST", "AMZ", "GRAPHPDS"), Path.Combine(RscPath, "AMZAST", "AMZ", "GRAPHPDS.B"),
                                Path.Combine(RscPath, "AMZAST", "AMZ", "MUSICPDS"), Path.Combine(RscPath, "AMZAST", "AMZ", "MUSICPDS.B")]);
                //Directory.CreateDirectory(Path.Combine(RscPath, "AMBAST", "AMZ", "PDS"));
            }
        }
        // TODO: For Mac, we might use the names.pds files to get the list of pds files
        else if (pcType == MacAbb)
        {
            if (abbrev == AmazonAbb || abbrev == AllGamesAbb)
            {
                pdsFiles.AddRange([Path.Combine(RscPath, "AMZMAC", "ctxa1.pds"), Path.Combine(RscPath, "AMZMAC", "musa1.pds"),
                                Path.Combine(RscPath, "AMZMAC", "pixa1.pds"), Path.Combine(RscPath, "AMZMAC", "pixa2.pds")]);
            }
            if (abbrev == DragonAbb || abbrev == AllGamesAbb)
            {
                pdsFiles.AddRange([Path.Combine(RscPath, "DGWMAC", "ctxa1.pds"), Path.Combine(RscPath, "DGWMAC", "ctxb1.pds"),
                                Path.Combine(RscPath, "DGWMAC", "ctxb2.pds"), Path.Combine(RscPath, "DGWMAC", "musa.pds"),
                                Path.Combine(RscPath, "DGWMAC", "musb.pds"), Path.Combine(RscPath, "DGWMAC", "pixa1.pds"),
                                Path.Combine(RscPath, "DGWMAC", "pixa2.pds"), Path.Combine(RscPath, "DGWMAC", "pixb1.pds"),
                                Path.Combine(RscPath, "DGWMAC", "pixb2.pds")]);
            }
            if (abbrev == F451Abb || abbrev == AllGamesAbb)
            {
                pdsFiles.AddRange([Path.Combine(RscPath, "F451MAC", "ctxa.pds"), Path.Combine(RscPath, "F451MAC", "ctxb1.pds"),
                                Path.Combine(RscPath, "F451MAC", "ctxb2.pds"), Path.Combine(RscPath, "F451MAC", "musa.pds"),
                                Path.Combine(RscPath, "F451MAC", "musb.pds"), Path.Combine(RscPath, "F451MAC", "pixa1.pds"),
                                Path.Combine(RscPath, "F451MAC", "pixa2.pds"), Path.Combine(RscPath, "F451MAC", "pixb1.pds"),
                                Path.Combine(RscPath, "F451MAC", "pixb2.pds"), Path.Combine(RscPath, "F451MAC", "pixb3.pds")]);
            }
        }

        OpenPds(pdsFiles, abbrev == AllGamesAbb);
    }

    static void OpenPds(List<string> pdsFiles, bool extract = false)
    {
        try
        {
            foreach (var pdsFile in pdsFiles)
            {
                if (!File.Exists(pdsFile))
                {
                    Console.Error.WriteLine($"{FileError} {pdsFile}");
                }

                List<(string Name, string InName, int InBeginAddress, int InEndAddress)> inFiles = [];
                var numFileAddr = 0x00;
                var fileListAddr = 0x02;
                var fileAddrAddr = 0x10;
                var firstFileAddr = 0x00;
                var fileNameLength = 12;
                var fileRefLength = 18;
                var offset = 0;

                // Apple II PDS have an extra three bytes at the beginning (2-byte full file length + separator)
                if (pcType == Apple2Abb)
                {
                    offset = 3;
                    numFileAddr += offset;
                    fileListAddr += offset;
                    fileAddrAddr += offset;
                }
                // Mac PDS files specify the first file address (twice for some reason) rather than a file count
                else if (pcType == MacAbb)
                {
                    offset = 8;
                    fileListAddr += offset;
                    fileAddrAddr += offset - 4;
                    fileNameLength = 8;
                    fileRefLength = 12;
                }

                var numFiles = 0;
                var inPrevFilename = "";
                var inCurFilename = "";
                var inListNum = 0;
                if (pcType == MacAbb)
                    inListNum = 1;
                var inFileNum = 0;
                byte inByte1 = 0x00;
                byte inByte2 = 0x00;
                var inPrevAddress = 0x000000;
                var inBeginAddress = 0x000000;
                var inEndAddress = 0x000000;
                var allBytes = File.ReadAllBytes(pdsFile);
                var eof = allBytes.Length - 1;
                var nameDone = false;
                List<byte> fileBytes = [];

                int i = 0;
                foreach (var b in allBytes)
                {
                    if (i == numFileAddr)
                    {
                        if (pcType == MacAbb)
                            firstFileAddr = b * 0x100;
                        else
                        {
                            numFiles = b;
                            Console.WriteLine();
                            Console.WriteLine($"{pdsFile} [0x{allBytes.Length:x6}] contains {numFiles} files:");
                        }
                    }
                    else if ((i == numFileAddr + 1) && pcType == MacAbb)
                    {
                        firstFileAddr += b;
                        numFiles = (firstFileAddr - offset - 2) / fileRefLength;
                        Console.WriteLine();
                        Console.WriteLine($"{pdsFile} [0x{allBytes.Length:x6}] contains {numFiles} files, beginning at 0x{firstFileAddr:x4}:");
                    }

                    if (i < fileListAddr)
                    {
                        // do nothing
                    }
                    else if (inListNum <= numFiles)
                    {
                        if ((i - fileListAddr) % fileRefLength == 0)       // new filename
                        {
                            inPrevFilename = inCurFilename;
                            inCurFilename = "";
                            inCurFilename += (char)b;
                            inByte1 = 0x00;
                            inByte2 = 0x00;
                            if (pcType == MacAbb)
                                inPrevAddress = inEndAddress;
                            else
                                inPrevAddress = inBeginAddress;
                            inBeginAddress = 0x000000;
                            nameDone = false;
                        }
                        else if (((i - fileAddrAddr) % fileRefLength == 0) && pcType != MacAbb)     // little byte
                            inByte1 = b;
                        else if ((i - fileAddrAddr) % fileRefLength == (pcType == MacAbb ? 0 : 1))  // middle byte (or big for Mac)
                            inByte2 = b;
                        else if ((i - fileAddrAddr) % fileRefLength == (pcType == MacAbb ? 1 : 2))  // big byte (or little for Mac)
                        {
                            // For Mac, the specified value is the length rather than the file address (and again twice for some reason),
                            // and so we're only looking at the last two bytes
                            if (pcType == MacAbb)
                            {
                                if (inListNum == 1)
                                    inBeginAddress = firstFileAddr;
                                else
                                    inBeginAddress = inPrevAddress + 1;
                                inEndAddress = inBeginAddress + (inByte2 * 0x100) + b - 1;
                                if (inEndAddress > eof)
                                    inEndAddress = eof;
                                if (inListNum > 0)
                                    inFiles.Add((pdsFile, inCurFilename, inBeginAddress, inEndAddress));

                                inListNum++;
                            }
                            else
                            {
                                // Other than Mac, the 3-byte specified value is the starting address of the file
                                inBeginAddress = (b * 0x10000) + (inByte2 * 0x100) + inByte1;
                                if (inBeginAddress > eof)
                                    inBeginAddress = eof + 1;
                                if (inListNum == 0)
                                    firstFileAddr = inBeginAddress;
                                else if (inListNum > 0)
                                    inFiles.Add((pdsFile, inPrevFilename, inPrevAddress, inBeginAddress - 1));

                                inListNum++;
                            }
                        }
                        else if (b == 0x00 || b == 0x20)
                            nameDone = true;
                        else if (!nameDone)
                        {
                            inCurFilename += (char)b;

                            if (inCurFilename.Length >= fileNameLength)
                                nameDone = true;
                        }
                    }

                    if (firstFileAddr > 0 && i >= firstFileAddr) // file data
                    {
                        if (inFiles.Count == 0)
                            continue;

//#if DEBUG
//                        if (i == inFiles[inFileNum].InBeginAddress)
//                            Console.WriteLine($"\n{pdsFile}: {inFileNum + 1,2}.{inFiles[inFileNum].InName} (0x{inFiles[inFileNum].InBeginAddress:x6} to 0x{inFiles[inFileNum].InEndAddress:x6})");
//#endif

                        if (i == inFiles[inFileNum].InEndAddress || i == eof)
                        {
                            fileBytes.Add(b);
                            if (extract)
                                File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(pdsFile) ?? Directory.GetCurrentDirectory(), inFiles[inFileNum].InName), [.. fileBytes]);
                            else
                            {
                                Console.WriteLine("\nOff | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
                                Console.Write      ("----+------------------------------------------------");
                                var j = 0;
                                foreach (var fb in fileBytes)
                                {
                                    if (j % 16 == 0)
                                    {
                                        Console.Write($"\n{j / 16:x3} | ");
                                    }
                                    Console.Write($"{fb:x2} ");
                                    j++;
                                }
                                Console.WriteLine($"\n\nEND {pdsFile}: {inFileNum + 1,2}.{inFiles[inFileNum].InName} (0x{inFiles[inFileNum].InBeginAddress:x6} to 0x{inFiles[inFileNum].InEndAddress:x6})");
                                Console.Write(KeyPress);
                                var k = Console.ReadKey(intercept: true);
                                if (k.Key.Equals(keyEsc) ||
                                    k.Key.Equals(keyBack))
                                {
                                    break;
                                }
                                else if (k.Key.Equals(keyQuit))
                                    DoExit();
                                Console.WriteLine();
                            }
                            inFileNum++;
                            if (inFileNum >= inFiles.Count)
                                break;
                            fileBytes = [];
                        }
                        else
                            fileBytes.Add(b);
                    }
                    i++;
                }
#if DEBUG
                foreach (var file in inFiles)
                {
                    Console.WriteLine($"{file.Name}: {file.InName} at 0x{file.InBeginAddress:x6}");
                }
#endif
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        Console.WriteLine();
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

    static void DoExit(string error = "")
    {
        if (!string.IsNullOrEmpty(error))
        {
            Console.Error.WriteLine(error);
            Environment.Exit(-1);
        }
        Console.WriteLine(Goodbye);
        Environment.Exit(0);
    }

    static ConsoleKey? ShowMenu(bool main, string title, params string[] options)
    {
        ConsoleKeyInfo? key = null;
        while (key?.Key != keyEsc && key?.Key != keyBack && key?.Key != keyQuit && key?.Key != keySwitch &&
               key?.Key != keyExpPng && key?.Key != keyExpMid && key?.Key != keyExtPds &&
               key?.Key != key0 && key?.Key != key1 && key?.Key != key2 && key?.Key != key3 &&
               key?.Key != key4 && key?.Key != key5 && key?.Key != key6 && key?.Key != key7 &&
               key?.Key != key8 && key?.Key != key9)
        {
            if (key is not null)
                Console.Error.WriteLine(KeyError);

            Console.WriteLine(Divider);
            Console.WriteLine();
            Console.WriteLine($"{TitleDecor}{title}{TitleDecor}");
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
                if (pcType == Apple2Abb || pcType == AtariStAbb || pcType == MacAbb)
                    Console.WriteLine($" X. {ExtractPdsMenu}");
                if (pcType == IbmAbb)
                    Console.WriteLine($" P. {ExportPngMenu}");
                if (pcType == Apple2Abb || pcType == AtariStAbb || pcType == CommodoreAbb || pcType == IbmAbb || pcType == MacAbb)
                    Console.WriteLine($" M. {ExportMidMenu}{ExpTag}");
                Console.WriteLine($" S. {SwitchMenu} ({pcType})");
            }
            else
                Console.WriteLine($" B. {BackMenu}");

            Console.WriteLine($" Q. {QuitMenu}");
            Console.WriteLine();
            Console.Write(Prompt);
            key = Console.ReadKey(intercept: true);

            Console.WriteLine();
            Console.WriteLine(Divider);
        }
        return key?.Key;
    }

    void DoMainMenu()
    {
        while (true)
        {
            var key = ShowMenu(main: true, title: MainTitle, AmazonName, DragonName, F451Name, AmberName, PerryName, RamaName, IslandName, OzName);
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
                case keyExtPds:
                    ExaminePdsFiles();
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
            var key = ShowMenu(main: false, title: amazonTitle, AmazonMenu1, AmazonMenu2, AmazonMenu3,
                PicMenu, SndMenu, StrMenu, PdsMenu);
            switch (key)
            {
                case key0:
                    Console.WriteLine(AmazonMenu1);
                    ShowExe(AmazonAbb, null, null);
                    break;
                case key1:
                    Console.WriteLine(AmazonMenu2);
                    ShowVocab(AmazonAbb);
                    break;
                case key2:
                    Console.WriteLine(AmazonMenu3);
                    ShowLocs(AmazonAbb);
                    break;
                /*
                case key2:
                    Console.WriteLine(AmazonMenu3);
                    ShowFunctions(AmazonAbb);
                    break;
                */
                case key3:
                    Console.WriteLine(PicMenu);
                    ShowPicFiles(AmazonAbb);
                    break;
                case key4:
                    Console.WriteLine(SndMenu);
                    ShowSoundFiles(AmazonAbb);
                    break;
                case key5:
                    Console.WriteLine(StrMenu);
                    ShowStrings(AmazonAbb, 0x03, expand: false);
                    break;
                case key6:
                    if (pcType == AtariStAbb || pcType == MacAbb)
                    {
                        Console.WriteLine(PdsMenu);
                        ExaminePdsFiles(AmazonAbb);
                    }
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
            var key = ShowMenu(main: false, title: dragonTitle, DragonMenu1, DragonMenu2,
                PicMenu, SndMenu, StrMenu, PdsMenu);
            switch (key)
            {
                case key0:
                    Console.WriteLine(DragonMenu1);
                    ShowExe(DragonAbb,
                        [0x50, 0x57, 0xE8, 0xC9, 0xD2, 0x83, 0xC4, 0x06, 0x57, 0xE8, 0x3C, 0xDB, 0x59, 0x8B, 0xC7, 0xC3,
                        0x64, 0x67, 0x77], // "dgw"
                        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
                    break;
                case key1:
                    Console.WriteLine(DragonMenu2);
                    ShowLocs(DragonAbb);
                    break;
                /*
                case key2:
                    Console.WriteLine(DragonMenu3);
                    ShowFunctions(DragonAbb);
                    break;
                case key3:
                    Console.WriteLine(DragonMenu5);
                    ShowVocab(DragonAbb);
                    break;
                */
                case key2:
                    Console.WriteLine(PicMenu);
                    ShowPicFiles(DragonAbb);
                    break;
                case key3:
                    Console.WriteLine(SndMenu);
                    ShowSoundFiles(DragonAbb);
                    break;
                case key4:
                    Console.WriteLine(StrMenu);
                    ShowStrings(DragonAbb, 0x03, expand: false);
                    break;
                case key5:
                    if (pcType == MacAbb)
                    {
                        Console.WriteLine(ExtractPdsMenu);
                        ExaminePdsFiles(DragonAbb);
                    }
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
            var key = ShowMenu(main: false, title: f451Title, F451Menu1, F451Menu2, F451Menu3,
                PicMenu, SndMenu, StrMenu, PdsMenu);
            switch (key)
            {
                case key0:
                    Console.WriteLine(F451Menu1);
                    ShowLocs(F451Abb);
                    break;
                case key1:
                    Console.WriteLine(F451Menu2);
                    ShowExe(F451Abb, null, null);
                    break;
                case key2:
                    Console.WriteLine(F451Menu3);
                    ShowVocab(F451Abb);
                    break;
                /*
                case key2:
                    Console.WriteLine(F451Menu3);
                    ShowFunctions(F451Abb);
                    break;
                */
                case key3:
                    Console.WriteLine(PicMenu);
                    ShowPicFiles(F451Abb);
                    break;
                case key4:
                    Console.WriteLine(SndMenu);
                    ShowSoundFiles(F451Abb);
                    break;
                case key5:
                    Console.WriteLine(StrMenu);
                    ShowStrings(F451Abb, 0x03, expand: false);
                    break;
                case key6:
                    if (pcType == MacAbb)
                    {
                        Console.WriteLine(ExtractPdsMenu);
                        ExaminePdsFiles(F451Abb);
                    }
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
            var key = ShowMenu(main: false, title: amberTitle, AmberMenu1, AmberMenu2, AmberMenu3, AmberMenu4, AmberMenu5,
                PicMenu, SndMenu, StrExpMenu, PdsMenu);
            switch (key)
            {
                case key0:
                    Console.WriteLine(AmberMenu1);
                    ShowLocs(AmberAbb);
                    break;
                case key1:
                    Console.WriteLine(AmberMenu2);
                    ShowExe(AmberAbb,
                        [0x61, 0x6D, 0x62, 0x2E, 0x20, 0x20, 0x20],                     // start: "amb.   "
                        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
                    break;
                case key2:
                    Console.WriteLine(AmberMenu3);
                    ShowFunctions(AmberAbb);
                    break;
                case key3:
                    Console.WriteLine(AmberMenu4);
                    ShowTokens(AmberAbb);
                    break;
                case key4:
                    Console.WriteLine(AmberMenu5);
                    ShowVocab(AmberAbb);
                    break;
                case key5:
                    Console.WriteLine(PicMenu);
                    ShowPicFiles(AmberAbb);
                    break;
                case key6:
                    Console.WriteLine(SndMenu);
                    ShowSoundFiles(AmberAbb);
                    break;
                case key7:
                    Console.WriteLine(StrExpMenu);
                    ShowStrings(AmberAbb, 0x02, expand: true);
                    break;
                case key8:
                    if (pcType == Apple2Abb || pcType == AtariStAbb)
                    {
                        Console.WriteLine(ExtractPdsMenu);
                        ExaminePdsFiles(AmberAbb);
                    }
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
            var key = ShowMenu(main: false, title: perryTitle, PerryMenu1, PerryMenu2, PerryMenu3, PerryMenu4,
                PicMenu, SndMenu, StrMenu);
            switch (key)
            {
                case key0:
                    Console.WriteLine(PerryMenu1);
                    ShowLocs(PerryAbb);
                    break;
                case key1:
                    Console.WriteLine(PerryMenu2);
                    ShowExe(PerryAbb, null, null);
                    break;
                case key2:
                    Console.WriteLine(PerryMenu3);
                    ShowFunctions(PerryAbb);
                    break;
                case key3:
                    Console.WriteLine(PerryMenu4);
                    ShowVocab(PerryAbb);
                    break;
                case key4:
                    Console.WriteLine(PicMenu);
                    ShowPicFiles(PerryAbb);
                    break;
                case key5:
                    Console.WriteLine(SndMenu);
                    ShowSoundFiles(PerryAbb);
                    break;
                case key6:
                    Console.WriteLine(StrMenu);
                    ShowStrings(PerryAbb, 0x03, expand: false);
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
            var key = ShowMenu(main: false, title: ramaTitle, RamaMenu1, RamaMenu2,
                PicMenu, SndMenu, StrMenu);
            switch (key)
            {
                case key0:
                    Console.WriteLine(RamaMenu1);
                    ShowLocs(RamaAbb);
                    break;
                case key1:
                    Console.WriteLine(RamaMenu2);
                    ShowExe(RamaAbb, null, null);
                    break;
                /*
                case key2:
                    Console.WriteLine(RamaMenu3);
                    ShowFunctions(RamaAbb);
                    break;
                case key3:
                    Console.WriteLine(RamaMenu5);
                    ShowVocab(RamaAbb);
                    break;
                */
                case key2:
                    Console.WriteLine(PicMenu);
                    ShowPicFiles(RamaAbb);
                    break;
                case key3:
                    Console.WriteLine(SndMenu);
                    ShowSoundFiles(RamaAbb);
                    break;
                case key4:
                    Console.WriteLine(StrMenu);
                    ShowStrings(RamaAbb, 0x03, expand: false);
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
            var key = ShowMenu(main: false, title: islandTitle, IslandMenu1, IslandMenu2, IslandMenu3,
                PicMenu, SndMenu, StrMenu);
            switch (key)
            {
                case key0:
                    Console.WriteLine(IslandMenu1);
                    ShowLocs(IslandAbb);
                    break;
                case key1:
                    Console.WriteLine(IslandMenu2);
                    ShowExe(IslandAbb, null, null);
                    break;
                case key2:
                    Console.WriteLine(IslandMenu3);
                    ShowVocab(IslandAbb);
                    break;
                /*
                case key2:
                    Console.WriteLine(IslandMenu3);
                    ShowFunctions(IslandAbb);
                    break;
                */
                case key3:
                    Console.WriteLine(PicMenu);
                    ShowPicFiles(IslandAbb);
                    break;
                case key4:
                    Console.WriteLine(SndMenu);
                    ShowSoundFiles(IslandAbb);
                    break;
                case key5:
                    Console.WriteLine(StrMenu);
                    ShowStrings(IslandAbb, 0x03, expand: false);
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
            var key = ShowMenu(main: false, title: ozTitle, OzMenu1, OzMenu2, OzMenu3, OzMenu4,
                PicMenu, SndMenu, StrMenu);
            switch (key)
            {
                case key0:
                    Console.WriteLine(OzMenu1);
                    ShowLocs(OzAbb);
                    break;
                case key1:
                    Console.WriteLine(OzMenu2);
                    ShowExe(OzAbb, null, null);
                    break;
                case key2:
                    Console.WriteLine(OzMenu3);
                    ShowFunctions(OzAbb);
                    break;
                case key3:
                    Console.WriteLine(OzMenu4);
                    ShowVocab(OzAbb);
                    break;
                case key4:
                    Console.WriteLine(PicMenu);
                    ShowPicFiles(OzAbb);
                    break;
                case key5:
                    Console.WriteLine(SndMenu);
                    ShowSoundFiles(OzAbb);
                    break;
                case key6:
                    Console.WriteLine(StrMenu);
                    ShowStrings(OzAbb, 0x03, expand: false);
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
            var key = ShowMenu(main: false, title: SwitchMenu, Apple2Type, AtariStType, CommodoreType, IbmType, MacType, MsxType);

            switch (key)
            {
                case key0:
                    Console.WriteLine(Apple2Type);
                    pcType = Apple2Abb;
                    break;
                case key1:
                    Console.WriteLine(AtariStType);
                    pcType = AtariStAbb;
                    break;
                case key2:
                    Console.WriteLine(CommodoreType);
                    pcType = CommodoreAbb;
                    break;
                case key3:
                    Console.WriteLine(IbmType);
                    pcType = IbmAbb;
                    break;
                case key4:
                    Console.WriteLine(MacType);
                    pcType = MacAbb;
                    break;
                case key5:
                    Console.WriteLine(MsxType);
                    pcType = MsxAbb;
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
}