#if DEBUG
//#define _DEBUG
#endif

using NAudio.Midi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SASTester;

partial class SASTester
{
    private const byte FirstPitchByte = 0x9E; //0xC2;
    private const int FirstMidi = 9; //45;  // 0xC2 = #45 = A2; Weird behaviors seen when going below A2 then back up again
                                            // But lowest actual note seems to be 0xA1 = #12 = C0
    private const int LastMidi = 127;       // Though the highest absolute pitch is 0xFF = #105 = Bb7, relative pitches might go higher
    private const int FirstOctave = 0; //3; // to nearest C
    private const int PatchSquare = 80;     // "Lead 1 (square)"    // or maybe 13=Xylophone
    private const int PatchSaw = 81;        // "Lead 2 (sawtooth)"  // or maybe 7=Harpsichord
    private const int PatchTriangle = 122;  // "Ocarina"            // triangle approx.
    private const int PatchNoise = 126;     // "Applause"           // white noise approx. or maybe 122=Seashore
    private const int PatchDouble = 87;     // "Lead 8 (bass+lead)" // at this point a stand-in, for (I think) waveform doubling

    private const string KeyStop = "Press any key to stop playback.";

    public void ShowSoundFiles(string abbrev)
    {
        var fileFound = false;
        if (pcType != IbmAbb)
            Console.WriteLine(FilePromptWarn);
        var files = GetSoundFiles(abbrev);
        if (files.Count == 0)
        {
            Console.WriteLine("No sound files found.");
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
                        Console.WriteLine($"{FileError} {filePath}");
                        break;
                    }
                }
                PlaySound(abbrev, filePath);
            }
            else
                Console.WriteLine(Divider);
        }
        while (!string.IsNullOrEmpty(input));
    }

    // IBM PC/PCjr formats are close to complete; AII, AST, and C64 are still works in progress
    public List<string> GetSoundFiles(string abbrev)
    {
        List<string> sndFiles = [];
        try
        {
            if (pcType == IbmAbb) // can rely on extensions
            {
                foreach (var file in Directory.EnumerateFiles(Path.Combine(RscPath, abbrev + pcType), "*.IB").ToList())
                {
                    var filename = Path.GetFileName(file);
                    sndFiles.Add(filename);
                }
                foreach (var file in Directory.EnumerateFiles(Path.Combine(RscPath, abbrev + pcType), "*.JR").ToList())
                {
                    var filename = Path.GetFileName(file);
                    sndFiles.Add(filename);
                }
                return sndFiles;
            }
            else if (pcType == AtariStAbb) // doesn't always use extension + deeper folder structure
            {
                foreach (var file in Directory.EnumerateFiles(Path.Combine(RscPath, abbrev + pcType, abbrev), "*.MST").ToList())
                {
                    var filename = Path.GetFileName(file);
                    sndFiles.Add(filename);
                }
                foreach (var file in Directory.EnumerateFiles(Path.Combine(RscPath, abbrev + pcType, abbrev), "*.").ToList())
                {
                    if (abbrev == F451Abb)
                        abbrev = "F4";
                    else if (abbrev == OzAbb)  // no WOZAST port?
                        abbrev = "WM";
                    var filename = Path.GetFileName(file);
                    if (!filename.StartsWith(abbrev, StringComparison.OrdinalIgnoreCase) &&
                        !filename.Equals("TELARIUM", StringComparison.OrdinalIgnoreCase) &&
                        !filename.Equals("TRILLIUM", StringComparison.OrdinalIgnoreCase) &&
                        !filename.Equals("WINDHAM", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (filename.Equals(abbrev, StringComparison.OrdinalIgnoreCase) ||
                        (abbrev == "F4" && (filename.Equals(F451Abb, StringComparison.OrdinalIgnoreCase) ||
                        filename.Length < 5)))
                        continue;
                    sndFiles.Add(filename);
                }
            }
            else // if (pcType == apple2Abb || pcType == commodoreAbb)
            {
                foreach (var file in Directory.EnumerateFiles(Path.Combine(RscPath, abbrev + pcType), "*.").ToList())
                {
                    if (abbrev == F451Abb)
                        abbrev = "F4";
                    else if (abbrev == OzAbb)
                        abbrev = "WM";

                    var filename = Path.GetFileName(file);
                    //if (!filename.StartsWith("MUSICPDS") && !filename.StartsWith(abbrev))
                    if (!filename.StartsWith(abbrev, StringComparison.OrdinalIgnoreCase) &&
                        !filename.Equals("TELARIUM", StringComparison.OrdinalIgnoreCase) &&
                        !filename.Equals("TRILLIUM", StringComparison.OrdinalIgnoreCase) &&
                        !filename.Equals("WINDHAM", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (filename.Equals(abbrev, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (abbrev == AmberAbb && (filename.Equals("AMBER", StringComparison.OrdinalIgnoreCase) ||
                        filename.EndsWith("AMBGLOB", StringComparison.OrdinalIgnoreCase) ||
                        filename.EndsWith("AMBINIT", StringComparison.OrdinalIgnoreCase)))
                        continue;
                    if (abbrev == "F4" && ((filename.StartsWith(F451Abb, StringComparison.OrdinalIgnoreCase) &&
                        !filename.EndsWith("OPEN", StringComparison.OrdinalIgnoreCase)) || filename.Length < 5))
                        continue;
                    if (abbrev == IslandAbb && filename.Equals("TRILL", StringComparison.OrdinalIgnoreCase))
                        continue;
                    sndFiles.Add(filename);
                }
            }
        }
        catch (Exception) { }
        return sndFiles;
    }

    private void ExportSndFiles()
    {
        if (pcType != Apple2Abb && pcType != AtariStAbb &&
            pcType != CommodoreAbb && pcType != IbmAbb)
            return;
        Console.WriteLine("Please wait ...");
        foreach (var abbrev in new[] { AmazonAbb, DragonAbb, F451Abb, AmberAbb,
            PerryAbb, RamaAbb, IslandAbb, OzAbb })
        {
            var sndFiles = GetSoundFiles(abbrev);
            foreach (var file in sndFiles)
            {
                var sub = "";
                if (pcType == AtariStAbb)
                    sub = abbrev;
                PlaySound(abbrev, Path.Combine(RscPath, abbrev + pcType, sub, file), toFile: true);
            }
        }
    }

    // This implementation is monophonic only, plays each channel one at a time
    // It sounds more authentic to PC speaker (Apple II and IBM *.IB), but the timing doesn't work very well yet
    // TODO: Fix timing
    private static void WaveOut(int beatLen, List<(int Pos, byte B, int Ch, int Midi, double Freq, int Length, int Patch)> notes)
    {
        Console.WriteLine(KeyStop);

        var ch = 0;
        var patch = PatchSquare;
        var square = new SignalGenerator()
        {
            Gain = 0.1,
            Type = SignalGeneratorType.Square,
        };
        var saw = new SignalGenerator()
        {
            Gain = 0.1,
            Type = SignalGeneratorType.SawTooth,
        };
        var tri = new SignalGenerator()
        {
            Gain = 0.1,
            Type = SignalGeneratorType.Triangle,
        };
        var noise = new SignalGenerator()
        {
            Gain = 0.1,
            Type = SignalGeneratorType.White,
        };
        using var wave = new WaveOutEvent();

        foreach (var (Pos, B, Ch, Midi, Freq, Length, Patch) in notes)
        {
            if (B == 0x80)
                continue;

            var rest = false;
            var freq = Freq;
            var midi = Midi;
            var len = Length;
            var nibble1 = (byte)((B & 0xF0) >> 4);
            var nibble2 = (byte)(B & 0x0F);

            if (nibble1 == 0x8 && nibble2 != 0x0)
            {
                rest = true;
                freq = 0; // set frequency to 0 if this is a rest
                midi = 0; // set MIDI note to 0 if this is a rest
            }
            var lenStr = GetNoteLengthName(len);
            Console.WriteLine($"{Pos:x3} : {B:x2} {(len > 0 ? (rest ? "r" : midi > 0 ? GetNoteName(midi) : "") : midi > 0 ? "*" : Patch > 0 ? "P" + Patch : "")}\t{lenStr}\tFreq:{Math.Round(freq, 2),7}\tMIDI:{midi,3}");

            // Not polyphonic, play next track separately
            if (Ch != ch)
            {
                ch = Ch;
                if (ch > 1)
                    Thread.Sleep(1000);
                Console.WriteLine("Channel " + ch);
            }
            if (Patch != 0)
                patch = Patch;

            if (len <= 0)
                continue;

            switch (patch)
            {
                case PatchSquare:
                    square.Frequency = freq;
                    wave.Init(square.Take(TimeSpan.FromMilliseconds(Length * beatLen / 2)));
                    break;
                case PatchSaw:
                    saw.Frequency = freq;
                    wave.Init(saw.Take(TimeSpan.FromMilliseconds(Length * beatLen / 2)));
                    break;
                case PatchTriangle:
                    tri.Frequency = freq;
                    wave.Init(tri.Take(TimeSpan.FromMilliseconds(Length * beatLen / 2)));
                    break;
                case PatchNoise:
                    noise.Frequency = freq;
                    wave.Init(noise.Take(TimeSpan.FromMilliseconds(Length * beatLen / 2)));
                    break;
            }

            wave.Play();
            var stop = false;
            while (true)
            {
                if (wave.PlaybackState == PlaybackState.Stopped)
                    break;

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (!key.Equals(ConsoleKey.None))
                    {
                        wave.Stop();
                        stop = true;
                        break;
                    }
                }
            }
            if (stop)
                break;
        }
    }

    // This implementation is monophonic only, plays each channel one at a time
    private void MidiOut(int beatLen, List<(int Pos, byte B, int Ch, int Midi, double Freq, int Length, int Patch)> notes)
    {
        Console.WriteLine(KeyStop);

        var ch = 0;
        var patch = PatchSquare;
        using var midiOut = new MidiOut(0);

        foreach (var (Pos, B, Ch, Midi, Freq, Length, Patch) in notes)
        {
            if (B == 0x80)
                continue;

            var rest = false;
            var freq = Freq;
            var midi = Midi;
            var len = Length;
            var nibble1 = (byte)((B & 0xF0) >> 4);
            var nibble2 = (byte)(B & 0x0F);

            if (nibble1 == 0x8 && nibble2 != 0x0)
            {
                rest = true;
                freq = 0; // set frequency to 0 if this is a rest
                midi = 0; // set MIDI note to 0 if this is a rest
            }
            var lenStr = GetNoteLengthName(len);
            Console.WriteLine($"{Pos:x3} : {B:x2} {(len > 0 ? (rest ? "r" : midi > 0 ? GetNoteName(midi) : "") : midi > 0 ? "*" : Patch > 0 ? "P" + Patch : "")}\t{lenStr}\tMIDI:{midi,3}\tFreq:{Math.Round(freq, 2),7}");

            // Not polyphonic, play next track separately
            if (Ch != ch)
            {
                ch = Ch;
                if (ch > 1)
                    Thread.Sleep(1000);
                Console.WriteLine("Channel " + ch);
            }
            if (Patch > 0)
                midiOut.Send(MidiMessage.ChangePatch(patch, ch).RawData);

            if (len > 0)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (!key.Equals(ConsoleKey.None))
                        break;
                }
                midiOut.Send(MidiMessage.StartNote(midi, 127, ch).RawData);
                Thread.Sleep(Length * beatLen);
                midiOut.Send(MidiMessage.StopNote(midi, 0, ch).RawData);
            }
        }
    }

    private static void WriteMidi(int beatLen, List<(int Pos, byte B, int Ch, int Midi, double Freq, int Length, int Patch)> notes, string filePath = "")
    {
        var multiTrack = 1;
        //if (pcType == IbmAbb && Path.GetExtension(filePath).Equals(".IB", StringComparison.OrdinalIgnoreCase))
        //    multiTrack = 0;
        IList<MidiEvent> track = [];

        const int TicksPerQtrNote = 480;  // ticks per quarter note
        const int DefVelocity = 127;      // 127=maximum velocity
        var ch = 0;
        long time = 0;

        MidiEventCollection events = new(multiTrack, TicksPerQtrNote);

        foreach (var (Pos, B, Ch, Midi, Freq, Length, Patch) in notes)
        {
            var nibble1 = (byte)((B & 0xF0) >> 4);
            var nibble2 = (byte)(B & 0x0F);

            var midi = Midi;
            if (Ch != ch)
            {
                time = 0;
                if (multiTrack == 1 && Ch < 17)
                {
                    if (ch > 0)
                        track.Add(new MetaEvent(MetaEventType.EndTrack, 0, time)); // end prior track
                    ch = Ch;
                }
                else
                    ch = 1;

                if (Patch != 0)
                    track = events.AddTrack([new PatchChangeEvent(0, ch, Patch)]);

                if (ch == 1 && beatLen > 0)
                {
                    int tempo = (int.MaxValue / 2) / (beatLen * TicksPerQtrNote / 20); //beatLen * TicksPerQtrNote * 18;
                    //Console.WriteLine($"Beat Length: {beatLen} / Tempo: {tempo * 4} ms \u00bc={Math.Round(60000 / (double)(beatLen * 32), 2)} bpm");
                    track.Add(new TempoEvent(tempo, 0)); // in ms per quarter note
                }
            }
            if (ch > 0 && beatLen > 0)
            {
                if (midi == 0)
                    time += Length * beatLen;
                else
                {
                    var duration = Length * beatLen;
                    track.Add(new NoteOnEvent(time, ch, midi, DefVelocity, duration));
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

    private static double GetFreqOffset(int n)
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

    private static int GetMidiNote(byte b) // where e.g., 0xC2 -> 45 (A2)
    {
        if (b >= FirstPitchByte)
            return b - FirstPitchByte + FirstMidi;
        return FirstMidi;
    }

    private static double GetFreq(int midi)
    {
        var offset = midi - FirstMidi - 3;  // Need to go to the previous A note
        var oct = offset / 12 + FirstOctave;
        return GetFreqOffset(offset) * Math.Pow(2, oct);
    }

    private static string GetNoteName(int midi) // where 45 -> A2 (0xC2)
    {
        var offset = midi - FirstMidi - 3;  // Need to go to the previous A note
        var oct = offset / 12 + FirstOctave;
        if (oct == FirstOctave && offset < 0)
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

    private static string GetNoteLengthName(int len, string last = "")
    {
        return (len) switch
        {
            < 1 => "",
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

    // The IBM PC Speaker was monophonic, but the PCjr was quadraphonic (though these games only use the 3 waveform channels; the fourth noise channel isn't used)
    // TODO: Figure out how to extract from MUSICPDS*.* for AMBAII, AMZAST, AMBAST
    private void PlaySound(string abbrev, string filePath = "", bool toFile = false)
    {
        byte timeOffset = 0x02;
        byte lensOffset = 0x0B;
        byte noteOffset = 0x1A;
        bool control = false;
        bool keyChg = false;
        bool dbl = false;
        bool newCh = false;
        bool pitch = false;
        bool rest = false;
        byte[] array = [];
        var midiNote = 0;
        var beatLen = 48;
        var channel = 0;
        var patch = 0;
        var numCtrl = 0;
        double freq = 0f;
        List<int> lengths = [];
        List<(int Pos, byte B, int Ch, int Midi, double Freq, int Length, int Patch)> notes = [];
        try
        {
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"{FileError} {filePath}");
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
            Console.Write("----+------------------------------------------------");
        }
        foreach (var b in array)
        {
            patch = 0;
            pitch = false;
            rest = false;
            if (pcType == Apple2Abb)
            {
                timeOffset += 3;
                lensOffset += 3;
                noteOffset += 3;
            }
            if (pcType == CommodoreAbb)
            {
                timeOffset += 2;
                lensOffset += 2;
                noteOffset += 2;
            }
            if (i == timeOffset)
                beatLen = b * 16;           // Beat time length (gives tempo)
            else if (i >= lensOffset && i < noteOffset)
                lengths.Add(b);             // Note lengths
            else if (i >= noteOffset)
            {
                // Note data
                var len = 0;
                var nibble1 = (byte)((b & 0xF0) >> 4);
                var nibble2 = (byte)(b & 0x0F);

                if (!control && b == 0x50)  // New channel
                {
                    newCh = true;
                    keyChg = false;
                    control = true;
                    numCtrl = 1;
                    channel++;
                    // we shouldn't have more than 3 or 4 channels, but just in case don't use the percussion channel
                    if (channel == 10)
                        channel++;
                    if (channel > 16)
                        channel = 16;
#if _DEBUG
                    Console.Write(    "\n\t" + $"{b:x2} A.new channel " + channel + " / " + numCtrl + ": ");
                    Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                }
                else if (!control && b == 0x80)
                {
                    newCh = true;
                    control = false;
                    numCtrl = 0;
#if _DEBUG
                    Console.Write(    "\n\t" + $"{b:x2} B.end channel " + channel + " / " + numCtrl + ": ");
                    Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                }
                else if (!newCh && b == 0x00) // Key change
                {
                    if (!keyChg)
                        numCtrl = 0;
                    keyChg = true;
                    control = true;
                    numCtrl++;
#if _DEBUG
                    Console.Write(    "\n\t" + $"{b:x2} C.keyChg on " + numCtrl + ": ");
                    Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                }
                else if (newCh && numCtrl == 2) // Doubled waveform? (ST only)
                {
                    // almost always 0x08
                    // TODO: Is 0x0F the only other value?
                    if (b == 0x0F)              // (This is always combined with 0x40 in the next byte)
                    {
                        dbl = true;
                        patch = PatchDouble;    // placeholder for now
                    }
                    else
                        dbl = false;
                    numCtrl++;
#if _DEBUG
                    Console.Write(    "\n\t" + $"{b:x2} D.control [on] patch " + patch + " / " + numCtrl + ": ");
                    Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                }
                else if (newCh && numCtrl == 3) // Change waveform
                {
                    // PCjr and Atari ST only have square waveforms [and unused? noise channels], but the ST can adjust envelope
                    if (!dbl)
                    {
                        if (b == 0x10)
                            patch = PatchTriangle;
                        else if (b == 0x20)
                            patch = PatchSaw;
                        else if (b == 0x40)
                            patch = PatchSquare;
                        else if (b == 0x80)
                            patch = PatchNoise;
                    }

                    numCtrl++;
#if _DEBUG
                    Console.Write(    "\n\t" + $"{b:x2} E.control [on] patch " + patch + " / " + numCtrl + ": ");
                    Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                }
                else if (newCh && numCtrl > 5)
                {
                    numCtrl++;
                    if (b >= FirstPitchByte) // Absolute pitch, first byte after a new channel
                    {
                        newCh = false;
                        pitch = true;
                        control = false;
                        midiNote = GetMidiNote(b);
#if _DEBUG
                        Console.Write(    "\n\t" + $"{b:x2} F.control off [pitch] " + numCtrl + ": ");
                        Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                    }
                    // Sometimes one or more rests occur before absolute pitch is set, we'll consider it a control
                    else if (nibble1 == 0x8 && nibble2 != 0x0 && nibble2 <= lengths.Count)
                    {
                        rest = true;
                        len = lengths[nibble2 - 1];
#if _DEBUG
                        Console.Write(    "\n\t" + $"{b:x2} G.tacit " + numCtrl + ": ");
                        Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                    }
                    else if (b == 0x00)
                    {
                        // ignore this
#if _DEBUG
                        Console.Write(    "\n\t" + $"{b:x2} H.control [on] 0x00 " + numCtrl + ": ");
                        Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                    }
                    else  // I don't think we should get here with a properly formatted file
                    {
                        newCh = false;
                        control = false;
#if _DEBUG
                        Console.Write(    "\n\t" + $"{b:x2} I.control off ??? " + numCtrl + ": ");
                        Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                        numCtrl = 0;
                    }
                }
                else if (newCh && (numCtrl < 2 || numCtrl > 3))
                {
                    // not sure what these do, do nothing for now
                    numCtrl++;
#if _DEBUG
                    Console.Write(    "\n\t" + $"{b:x2} J.control [on] ??? " + numCtrl + ": ");
                    Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                }
                // TODO: Does a double 0x00 mean something else?
                else if (keyChg) // Key change, first byte after one or more 0x00
                {
                    newCh = false;
                    pitch = true;
                    numCtrl++;
#if _DEBUG
                    Console.Write(    "\n\t" + $"{b:x2} K.keyChg [on] [pitch] " + numCtrl + ": ");
                    Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                    if ((nibble1 & 0x08) == 0)  // positive nibble (< 0x8)
                    {
                        keyChg = false;
                        control = false;
                        numCtrl = 0;
                        // 0:+0 octaves, 1:+1 octaves, etc.?
                        midiNote += nibble1 * 12 + nibble2;
                    }
                    else if (nibble1 > 0x8)     // negative nibble (> 0x8)
                    {
                        keyChg = false;
                        control = false;
                        numCtrl = 0;
                        // F:-0 octaves, E:-1 octaves, etc.?
                        midiNote -= ((0xF - nibble1) * 12) + (0x10 - nibble2);
                    }
                    else // (nibble1 == 0x8)  // I don't think we should get here with a properly formatted file
                    {
                        pitch = false;
                        if (nibble2 != 0x0)
                            rest = true;
                    }
                }
                // TODO: What about the weird 0x10s and 0x20s?
                else
                {
                    newCh = false;
                    keyChg = false;
                    control = false;
                    if (nibble1 == 0x8)
                    {
                        if (nibble2 == 0x0)
                        {
                            control = true;
                            numCtrl++;
                        }
                        else
                            rest = true;
                    }
                    else if ((nibble1 & 0x08) == 0)  // positive nibble (< 0x8)
                        midiNote += nibble1;
                    else if (nibble1 > 0x8)     // negative nibble (> 0x8)
                        midiNote -= 0xF - nibble1 + 1;  // F:-1, E:-2, D:-3, C:-4, B:-5, A:-6, 9:-7

                    if (nibble2 > 0x0 && nibble2 <= lengths.Count)
                        len = lengths[nibble2 - 1];
                    else
                        len = 0;
#if _DEBUG
                    Console.Write(    "\n\t" + $"{b:x2} L.note data " + (rest ? "[rest] " : "") + numCtrl + ": ");
                    Console.WriteLine(pitch ? "P" : keyChg ? "K" : newCh ? "N" : rest ? "R" : control ? "C" : "n/a");
#endif
                }

                if (midiNote < 0)
                    midiNote = 0;
                if (midiNote > LastMidi)
                    midiNote = LastMidi;
                freq = GetFreq(midiNote);

                if (pitch)
                    notes.Add((i, b, channel, midiNote, freq, 0, 0));
                else if (keyChg)    // non-pitch = 0x00
                    notes.Add((i, b, channel, 0, freq, 0, 0));
                else if (rest)      // rests might occur before pitch is set (while control=true)
                    notes.Add((i, b, channel, 0, 0, len, 0));
                else if (newCh)
                    notes.Add((i, b, channel, 0, 0, 0, patch));
                else if (control)
                    notes.Add((i, b, channel, 0, 0, 0, 0));
                else
                    notes.Add((i, b, channel, midiNote, freq, len, 0));
            }

            if (!toFile)
            {
                if (i % 16 == 0)
                {
                    Console.WriteLine();
                    Console.Write($"{i:x3} | ");
                }
                Console.Write($"{b:x2} ");
#if _DEBUG
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
        Console.WriteLine(Divider);
        /*
        if (pcType == Apple2Abb || (pcType == IbmAbb &&
            Path.GetExtension(filePath).Equals(".IB", StringComparison.OrdinalIgnoreCase)))
            WaveOut(beatLen, notes);
        else
        */
            MidiOut(beatLen, notes);
        Console.WriteLine(Divider);
        return;
    }
}