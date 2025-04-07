## Spinnaker Adventure System

![logo](images/telarium-logo.jpg "Telarium logo") ![logo](images/windham-logo.png "Windham Classics logo")

#### Used by Telarium and Windham Classics adventure games

## Introduction

I wasn't able to find any technical information on the web pertaining to the [Spinnaker Software](https://en.wikipedia.org/wiki/Spinnaker_Software)'s \"Spinnaker Adventure System\" or its SAL, the Spinnaker Adventure Language, used in creating adventure games published by Spinnaker's imprints Trillium/Telarium and Windham Classics, though some interesting historical background [can](https://www.filfre.net/2013/09/rendezvous-with-rama/) be [found](https://www.filfre.net/2022/09/byron-preisss-games-or-the-perils-of-the-electronic-book/).

![screenshot](images/f451-ibm-screenshot.png "Fahrenheit 451 for IBM PC screenshot") ![screenshot](images/tri-ibm-screenshot.png "Treasure Island for IBM PC screenshot")

Because I feel nostalgic about some of these games, but at the same time find their input text parser to be extraordinarily frustrating, I wanted to update one or more of them to a choice-based format that would enable modern players to better enjoy them.  As a first step along this path, I'm documenting here my discoveries from examining the binaries for the PC versions and comparing the various games to one another, and for what little it might be worth, sharing the Tester Tool I created in C# to assist in my analysis.  I have no particular experience in reverse engineering, but hopefully this will inspire and assist the beginning of such an effort.  For instance, it'd be great to eventually have these games added to [Gargoyle](https://ccxvii.net/gargoyle/) and/or [ScummVM](https://www.scummvm.org/)'s Glk engine, which could provide an enhanced experience compared to DOSBox.

---

### NOTE: To analyze a game, the Tester Tool expects to find it in a subdirectory of .\\Resources\\, named using one of the below abbreviations.

These are the games created with the Spinnaker Advanture System:


| abbrev | game                                         | year | imprint           | Apple | Atari | Commodore | PC  | Mac | MSX* |
| -------- | ---------------------------------------------- | ------ | ------------------- | ------- | ------- | ----------- | ----- | ----- | ------ |
| AMB    | Nice Princes in Amber                        | 1985 | Telarium          | II    | ST    | 64        | DOS |     | MSX2 |
| AMZ    | Amazon                                       | 1984 | Trillium/Telarium | II    | ST    | 64        | DOS | Mac | MSX  |
| DGW    | Dragonworld                                  | 1984 | Trillium/Telarium | II    |       | 64        | DOS | Mac | MSX  |
| F451   | Fahrenheit 451                               | 1984 | Trillium/Telarium | II    | ST    | 64        | DOS | Mac | MSX  |
| PMN    | Perry Mason: The Case of the Mandarin Murder | 1985 | Telarium          | II    | ST    | 64, Amiga | DOS |     | MSX  |
| RDV    | Rendezvous with Rama                         | 1984 | Trillium/Telarium | II    |       | 64        | DOS | Mac | MSX2 |
| TRI    | Treasure Island                              | 1985 | Windham Classics  | II    | ST    | 64        | DOS |     | MSX  |
| WOZ    | The Wizard of Oz                             | 1984 | Windham Classics  | II    |       | 64        | DOS |     | MSX  |

- * = The MSX ports were published by Idealogic, in Spanish only.

## File types

Some observations about the files used by these games: Thankfully, game strings are ASCII-encoded (though AMB is partially tokenized).


| filename               | game                 | description                                                                                                                        |
| ------------------------ | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| \<abbrev\>             | all                  | Strings and data used globally                                                                                                     |
| **AMBGLOB**            | AMB only             | For AMB, additional strings and data used globally                                                                                 |
| **0** \| **1**         | DGW & RDV            | I'm guessing these are strings and data used globally across a specific disk                                                       |
| **A** \| **B**         | AMB & F451 & PMN     | I'm guessing these are strings and data used globally across a specific disk                                                       |
| \<abbrev\>**.DIB**     | AMB & PMN only       | Directory of locations with disk numbers ("a" or "b") for AMB & PMN                                                                |
| **DIR**                | all except AMB & PMN | Directory of locations with disk numbers ("a" or "b")                                                                              |
| \<abbrev\>**.EXE**     | all                  | The game executable. Note a few game strings are found here, though most strings here are applicable to the game engine generally. |
| \<abbrev\>**.T**       | AMB, PMN & WOZ only  | List of game functions?                                                                                                            |
| \<abbrev\>**.TOK**     | AMB only             | [Token file](#tokenization-in-nine-princes)                                                                                        |
| \<abbrev\>**.V**       | all except DGW & RDV | [Vocabulary file](#vocabularies)                                                                                                   |
| **NEWDATA**            | all except TRI       | Additional help particular to this game                                                                                            |
| **SAVED**              | all                  | Saved game file                                                                                                                    |
| **VOLT**               | all                  | Identifies the current disk                                                                                                        |
| **\*.IB** \| **\*.JR** | all                  | Sound files in IBM PC and PCjr formats                                                                                             |
| **\*.FEN**             | AMB only             | For AMB, data specific to the fencing (swordfighting) events                                                                       |
| **\*.STR**             | PMN only             | For PMN, some game strings have been separated into separate files (especially for cross-examinations?)                            |
| **\*.** (no extension) | all                  | Mostly location or[graphics](#picture-format) files                                                                                |

Game strings and other data is found in the appropriate location files.

## Vocabularies

The vocabulary files list all of the words the parser understands.  Note that nearly all words are truncated, but the game can be played this way, e.g. "EXAM CHAL" will examine the chalice.

## Tokenization in "Nine Princes"

To save disk space, AMB (only) uses a tokenizer of its 256 most common words to shrink the text strings a bit.  Starting at offset 0x102 of AMB.TOK is a list of words, from which can be created a dictionary with a serialized index.  If a char is 0x80 or greater within any of the string lists from the Amber location files, then that represents the number of the token word--just subtract 0x80.  The Tester Tool expands strings for "Nine Princes" automatically.

## Picture Format

The Tester Tool permits you to export all pictures from all games to .PNG.  You can also get a preview of an individual file with ANSI block characters.  Note that the Tester's list of pictures shows files with no extension that weren't found in the location dir file (other than \<abbrev\>,1,2,A,B,DIR,NEWDATA,SAVED,VOLT), but there may be false positives.

For the IBM versions, SAS uses 320x200 medium-resolution CGA, which supports three 4-color palettes and 2 intensity levels; these games only use low intensity and the first 2 palettes.  Note that the Atari ST and Commodore 64 versions use the same resolution, but with 16 color support.  The Apple II versions use the 280x192 resolution, with 6 "fringed" colors.

Pictures are either placed at the top in landscape orientation, fullscreen width with (typically) 40% of the screen height, or on one side in portrait orientation, with 45% of the screen width.  Note that AMZ was ported to SAS from Apple II, and it uses most of the screen for its pictures (`0xA0` for both height and width, or 320x160) [plus the text is in all-caps, ungh].

As mentioned, my initial analysis describes the PC versions, and the format used for pictures for other ports does differ.

### Header

The first 6 bytes are used as a header with the following layout:


| offset | use          | description                                                                                                                                                                                                            |
| -------- | -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 00     | Palette      | For PC,`0x00`=GRY (Green/Red/Yellow) or `0x01`=CMW (Cyan/Magenta/White)                                                                                                                                                |
| 01     | Intensity/Bg | For PC, 1st hex nibble is intensity (`0`=low; `1`=bright), 2nd is background color (0-F) corresponding to PC color codes*                                                                                              |
| 02     | Unknown      | Lots of variance. Maybe an identifier of some kind? Differs between ports.                                                                                                                                             |
| 03     | Unknown      | Small variance, i.e.`0x00` - `0x10`?; the game freezes after drawing is complete when values are too large, or the drawing does not complete when values are too small. Buffer size, maybe? Same values in PC and C64. |
| 04     | Height       | For PC and C64, typically either`0xB0` (176px) = 88% height, or `0x50` (80px) = 40%-height                                                                                                                             |
| 05     | Width / 2    | For PC and C64, typically either`0xA0` (160=>320px) = 100% width, or `0x48` (72=>144px) = 45%-width; though this field seems to be ignored                                                                             |

- \* = For PC: 0=black, 1=dk.blue, 2=dk.green, 3=dk.cyan, 4=dk.red, 5=dk.magenta, 6=dk.yellow, 7=br.gray, 8=dk.gray, 9=br.blue, 10=br.green, 11=br.cyan, 12=br.red, 13=br.magenta, 14=br.yellow, 15=white

### Pixel data

The rest of the file is pixel data.  Though I don't have much experience with image formats, it seems a bit odd.  It's similar to sixel (which I gather is odd enough), except this is "fourxel" and it's rotated 90 degrees.  Four-pixel wide blocks are laid out top to bottom, with each block being from 0-15 pixels high.  A set of three bytes represents two of these blocks, with the first byte's color map given by the 1st nibble (hexadecimal digit) of the second byte, and the 2nd nibble of the second byte gives the height of the color map in the third byte, i.e.:


| byte1 (00-FF) | byte2 nibble1 (0-F) | byte2 nibble2 (0-F) | byte3 (00-FF) |
| --------------- | --------------------- | --------------------- | --------------- |
| color map     | height for byte1    | height for byte3    | color map     |

The following 3 bytes will place the next set of 2 blocks below the previous ones, until the height from the header offset=04 is met, then further pixels are moved back to the top and shifted right by 4 pixels.  The width in header offset=05 appears to be ignored.

Color Maps:
The color maps are base-4 bitmasks for the color of each of the 4 pixels.  See the table below for an excerpt (the palette columns assume the background color is black):


| hex    | color map | palette 0 | palette 1 |
| -------- | ----------- | ----------- | ----------- |
| `0x00` | 0 0 0 0   | K K K K   | K K K K   |
| `0x01` | 0 0 0 1   | K K K G   | K K K C   |
| `0x02` | 0 0 0 2   | K K K R   | K K K M   |
| `0x03` | 0 0 0 3   | K K K Y   | K K K W   |
| `0x04` | 0 0 1 0   | K K G K   | K K C K   |
| `0x05` | 0 0 1 1   | K K G G   | K K C C   |
| ...    |           |           |           |
| `0x1B` | 0 1 2 3   | K G R Y   | K C M W   |
| ...    |           |           |           |
| `0x6C` | 1 2 3 0   | G R W K   | C M W K   |
| ...    |           |           |           |
| `0XB1` | 2 3 0 1   | R Y K G   | M W K C   |
| ...    |           |           |           |
| `0XC6` | 3 0 1 2   | Y K G R   | W K C M   |
| ...    |           |           |           |
| `0xFA` | 3 3 2 2   | Y Y R R   | W W M M   |
| `0xFB` | 3 3 2 3   | Y Y R Y   | W W M W   |
| `0xFC` | 3 3 3 0   | Y Y Y K   | W W W K   |
| `0xFD` | 3 3 3 1   | Y Y Y G   | W W W C   |
| `0xFE` | 3 3 3 2   | Y Y Y R   | W W W M   |
| `0xFF` | 3 3 3 3   | Y Y Y Y   | W W W W   |

- key: K=black, B=blue, G=green, C=cyan, R=red, M=magenta, Y=yellow, W=white

### Decoding the Color Map

It took me an embarrassingly long time to figure out the appropriate bitwise operation to read out a base-4 bitmask, so if I might save you the trouble:

```
colorMap[0] = (byteArray >> 6) & 0x3;
colorMap[1] = (byteArray >> 4) & 0x3;
colorMap[2] = (byteArray >> 2) & 0x3;
colorMap[3] = byteArray & 0x3;
```

### Examples

So, take an example 3 bytes: `0x1BF7C6`.  You'll get a 4x15-pixel block above a 4x7-pixel block, with sets of 4-color stripes based on the color map.  If it's palette 0, low-intensity and a black background, you'll get the following (8x zoom for clarity):

![example](images/gfx-example1.png "example")


| `1B`        | `F`       |   | `7`      | `C6`        |
| ------------- | ----------- | --- | ---------- | ------------- |
| colors 0123 | 15px high |   | 7px high | colors 3012 |

#### AMB\\HOSPITL

So, taking the first location of "Nine Princes in Amber" as an example:

![screenshot](images/amb-ibm-screenshot.png "Nine Princes in Amber for IBM PC screenshot")


| offset | `00 01 02 03 04 05` |
| -------- | --------------------- |
| 00     | `01 00 C9 03 50 A0` |

Looking at the header, we see:

- `0x0100` = palette 1 (KCMW), low-intensity and black background.
- `0xC903` = unknown
- `0x50A0` = 320x80 (fullscreen width, top 40% of screen)


| offset | `00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F` |
| -------- | --------------------------------------------------- |
| 00     | `-- -- -- -- -- -- FF FF FF FF FF FF FF 11 00 FF` |
| 10     | `71 FC F0 11 01 05 11 15 54 11 51 11 13 55 FF 61` |
| ...    |                                                   |

Looking at the pixel data, we see that it's pretty boring at first: 61 pixels down of all white. Then 1 of all black, and seven more all white.  Finally, a set of 1 pixel-high mixed black and white, and then some black and cyan until the bottom of the picture height, and back to the top of the screen (shifted by 4 pixels to the right) for the next block of white.

![example](images/gfx-example2.png "pixel layout")


| offset | colors       | height  |   | offset | height  | colors       |
| -------- | -------------- | --------- | --- | -------- | --------- | -------------- |
| 06     | `FF`=W W W W | `F`=x15 |   | 08     | `F`=15x | `FF`=W W W W |
| 09     | `FF`=W W W W | `F`=x15 |   | 0B     | `F`=15x | `FF`=W W W W |
| 0C     | `FF`=W W W W | `F`=x1  |   | 0E     | `1`=1x  | `00`=K K K K |
| 0F     | `FF`=W W W W | `7`=x7  |   | 11     | `1`=1x  | `FC`=W W W K |
| 12     | `F0`=W W K K | `1`=x1  |   | 14     | `1`=1x  | `01`=K K K C |
| 15     | `05`=K K C C | `1`=x1  |   | 17     | `1`=1x  | `15`=K C C C |
| 18     | `54`=C C C K | `1`=x1  |   | 1A     | `1`=1x  | `51`=C C K C |
| 1B     | `11`=K C K C | `1`=x1  |   | 1D     | `3`=3x  | `55`=C C C C |
| 1E     | `FF`=W W W W | `6`=x6  |   | ...    | `1`=1x  | ...          |

### Animations

Some of the graphic files invoke simple animations, but I haven't yet done much analysis.

### Other ports

I have no experience with the Commodore 64 or Atari ST, but I figured it'd be nice to have the 16-color version of the images, and potentially some better sound files (then there's the Spanish-only remakes for MSX with totally redrawn art, but I'm sure that's a different engine altogether).

#### Atari ST

So I pulled the files from the Atari ST port of "Nine Princes," and discovered the graphics had been packed into a container file called GRAPHPDS (which, based on the ASCII text in the header, apparently collects files that originally had a .GST extension).  I guess PDS probably refers to the "Programmers Development System" which was used as a cross-compiler for C64 and Atari ST.  It would be nice not to have to figure out a container format as well, and it looks like C64 does not have that issue.  And even though the Atari had a more flexible color system than the Commodore, based on the screenshots online it doesn't look like Telarium really leveraged it very well.

#### Commodore 64

OK, so I've just started this analysis, but here's what I've got so far.

It's clearly a different format from IBM, but the header is similar, and there appeared to be the tantalizing similarity of three byte sequences starting at offset 0x65 (after the third reference to 0x50A0, the resolution).  Twiddling bits showed me that I was sort of correct; that there was indeed something similar going on here with a pattern of blocks with colors being placed in the first and third byte, and a size in each of the 2 nibbles of the second byte.  However, here it was instead doing color fills.  So the colors are also split into nibbles, with each one representing one of 16 colors in the current palette:


| 0        | 1        | 2        |   | 3        | 4        | 5        |
| ---------- | ---------- | ---------- | --- | ---------- | ---------- | ---------- |
| color A1 | color A2 | number A |   | number B | color B1 | color B2 |

So, returning to "Nine Princes" for our example:

![screenshot](images/amb-c64-screenshot.png "Nine Princes in Amber for Commodore 64 screenshot")


| 0x65     | 0x68     | 0x6B     | 0x6E     |
| ---------- | ---------- | ---------- | ---------- |
| F0 71 FC | 6C 11 60 | 1F 12 10 | 1F 13 0F |

After experimenting, it looks like the palette is slightly different from the default C64 palette.  So that means there's probably going to be another section of the file that assigns colors.  Looking at "F0," I discover that Color F is light gray on both this and the default palette; same for Color 0: black; but here it looks like black is a no-op because there are no dividing lines in the top-left 4x8 pixel block.  Nor are there any for the next 6 blocks.  And so the 7 in what I'm calling "number A" says to use the same fill colors for seven 4x8 blocks.  The next nibble, "number B," is 1, saying that the third byte's colors are only going to apply to the one block.  And this block occurs on both sides of the dividing line between the wall and the floor; the wall being color F again, and the floor being color C, the medium gray.

Looking to the next three bytes, 6 is the blue used on the bed.  But why is 6 listed before C?  It appears that the first byte fills from the bottom first, unlike the third byte; so perhaps what happens with the first byte in 0x65 is that the black *isn't* a no-op; it just fills it with black first and then gray goes on top because there's no dividing line here?  Could be...

In any case, the three-byte pattern looks like it changes again around 0x1D6, a couple bytes before repeating the resolution (this time including the prior two bytes of the header; the ones I'm unclear on, after giving the signal 0x1010).  So what's next?  ...Or should I return to the top of the file and see if that's where the palette is being set?
