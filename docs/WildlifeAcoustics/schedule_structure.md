# Wildlife Acoustics SM3 and SM4 schedule structure

Ok here's my best reverse engineered guess at the layout of the `SM4P` and `SM3P` chunks.

Note: this document took significant time and effort to produce. If you use it or find
it useful please acknowledge [@atruskie](https://github.com/atruskie)/EMU in your work.
Also let us know! We'd love to know it helped someone.


## Prelude

Notes: The ones I have seem all be of size 1124 bytes (for SM4 variants)
or 500 bytes (for SM3 variants). 
The sizes are consistent when embedded in a `wamd` block or in a standalone file.

Notation note:
  -   `b<digit>`: the byte at the zero based index `<digit>`
  -   `b0 + b3 + b2`: is the concatenation of bytes with the most significant byte first
      -   e.g. `b0 + b3 + b2` is equivalent too `bytes[0] << 16 & bytes[3] << 8 & bytes[2]`
  -   `b<digit>[<bit>]`: the bit the zero based index `<bit>`. 0 is the lowest bit.
  -   `b<digit>[<bit1>..<bit2>]`: a range of bits,
                               starting from and including `<bit1>`
                               and ending at but excluding `<bit2>`.

## SM3P

SM3s have only advanced schedules.

```
Offset  Size  Description
--------------------------------------------------------------------------------------------------------------
0       4     Chunk ID (`SM3P`)
4       2     The one-based index of the first schedule, almost always `1` (16-bit le).
6       2     Some unknown value
8       2     The one-based index of the last schedule e.g `1` or `10` (16-bit le).
10      2     Some unknown value
16      396   Steps in the advanced schedule.
              Array of 99 4-byte records.
              Unused records are null bytes.
              Each record consists of:
              b0: the payload overflow byte
              b1: schedule step type
              b2..3: the payload.
              Use the constant lookup table below to decode step types.
412     24    The prefix given to files recorded by this program. UTF-16.
444     2     Prefix enabled. 0 = false, 1 = true.
446     2     The UTC offset hours component (16-bit le signed short)
448     2     The UTC offset minutes component (16-bit le signed short)(seems to be negative as well?)
450     2     Timezone enabled. 0 = false, 1 = true.
452     4     The latitude and longitude as integers. Each value a 16-bit le signed short.
              Divide by 100 to get actual value?
456     2     Position enabled. 0 = false, 1 = true.
458     2     Solar mode (twilight type).
              0xFFFF = sunrise/sunset
              0x0000 = civil
              0x0001 = nautical
              0x0002 = astronomical
460     2     Solar mode enabled. 0 = false, 1 = true.
462     2     Cut off voltage. 16-bit le, divide by 10 to get value in volts.
464     2     Cut off voltage enabled. 0 = false, 1 = true.
466     2     Sensitivity channel 0 (left). 16-bit signed le, divide by 10 to get value in decibels.
              Sometimes is 0xFF. Interpreting 0xFF as -1 (not set or N/A).
468     2     Sensitivity channel 1 (right). 16-bit signed le, divide by 10 to get value in decibels.
              Sometimes is 0xFF. Interpreting 0xFF as -1 (not set or N/A).
470     2     Sensitivity enabled. 0 = false, 1 = true.
472     2     Model. 0 = SM3, 1 = SM3BAT/SM3M
474     2     Scenario: memory Card A. 16-bit le. 0 = empty, otherwise value is 2^value = size in GB.
476     2     Scenario: memory Card B. As per memory Card A.
478     2     Scenario: memory Card C. As per memory Card A.
480     2     Scenario: memory Card D. As per memory Card A.
482     2     Scenario: Mic 0. 16-bit le.
              0 = internal
              1 = SMM-A1/SM3-A1
              2 = SMM-A2
              3 = SMM-U1/SM3-U1
              4 = SMM-U2
              5 = SMM-H1
              6 = SMM-H2
484     2     Scenario: Mic 1. As per Mic 0.
486     2     Scenario: Trig ratio. 16-bit le. Whole integer percentage points.
              Is 0xFF when read from a recording. Interpreting 0xFF as -1 (not set or N/A).
488     2     Scenario: Battery energy. 16-bit le. Integer value in watt hours.
              Is 0xFF when read from a recording. Interpreting 0xFF as -1 (not set or N/A).
492     4     Scenario: start. Two consecutive 16-bit le interpreted as one 32-bit le.
              The value is the number of seconds from 2000-01-01.
498     2     Unknown
```

## SM4P

SM4s can have either simple schedules or advanced schedules
(boolean or: both are possible within the one file).

```
Offset Size  Description
-------------------------------------------------------------------------------------------------------------
0      4    Chunk ID (`SM4P`)
4      2    The one-based index of the first schedule `1` (16-bit le).
6      2    Some unknown value
8      2    The one-based index of the last schedule e.g `1` or `10` (16-bit le).
10     2    Some unknown value
12     8    Unknown 8 bytes, always observed to be null
20     80     The schedule.
            - an array of 10 8-byte basic schedules: 80 bytes
              - unused schedules are null bytes
              - the byte layout for each schedule is:
                 - 4 two-byte little endian values
                 - 0..2: `start time` in minutes from midnight
                     - high bits 14..15 are unused
                     - bit 13 is a flag for a `set` relative time
                     - bit 12 is a flag for a `rise` relative time
                     - low bits 0..12 are minutes from midnight 
                        (11-bit le, **signed magnitude** int)
                        This is not a two's complement!
                 - 2..4: `end time` in minutes from midnight (16-bit le)
                     - follows the same spec as the `start time`
                 - 4..8: are null IF duty cycle is `always`
                 - ELSE if a duty cycle is set to `cycle`:
                     - the `on` and `off` values are encoded in two 11-bit numbers
                     - 4: split into two parts:
                         - the highest 2 bits are ignored
                         - the lowest 6 bits are highest bits of the `on` value
                     - 5: unknown, always null
                     - 6: the lower 8 bits of the `off` value
                     - 7: split into two parts:
                         - the higher 5 bits are the 5 lowest bits of the `on` value
                         - the lowest 3 bits are the 3 highest bits of the `off` value
               - if advanced schedules are used, there is the possibility of the simple
                 schedules still being encoded in the file. They just seem to be ignored.
100    360  Then two recording bitmaps that 360 bytes wide: 2 days?
                - 2 sections each 180 bytes wide: 1 day
                - First and last days? First and normal days?
                - 22 inner sections each 15 bytes wide: 2 hours?
                - 15 bytes is 120 bits = 1 bits/min
                - Byte order: lowest byte is the first 8 minutes of the day,
                      and the highest byte is the last 8 minutes of the day.
                - Bit order: the lowest bit is the first minute of byte/block,
                      and the highest bit is the last minute of byte/block.
                - (it seems obvious when written out but the order is actually a little confusing
                    because the bytes are read in order left-to-right but the bits are read 
                    in right-left order).
                While the pattern for these bits is fairly easy to guess at, I have
                no idea what the semantics are. The start and end ranges inside the bitmaps
                are *close* to the start and end times of some schedules, part way through
                deployments, but not always - especially for schedules that are rise/set based.
                For example, for one bitmap I found, the records most closely matched the 34th
                and 35th days of a 56 day predicted deployment. I also have no idea how 
                advanced schedules are encoded in these bitmaps.
460    24   The prefix given to files recorded by this program. UTF-16.
484    8    Unknown 8 bytes, observed to be null
492    2    Prefix enabled. The value `1`, previous field enabled
494    2    The UTC offset hours component (16-bit le signed short)
496    2    The UTC offset minutes component (16-bit le unsigned short)
498    2    Timezone enabled. The value `1`, previous field enabled
500    4    The latitude and longitude as integers. Each value a 16-bit le signed short.
            Divide by 100 to get actual value?
504    2    Position enabled. The value `1`, previous field enabled
506    2    Solar mode (twilight type).
            0xFFFF = sunrise/sunset
            0x0000 = civil
            0x0001 = nautical
            0x0002 = astronomical
512    4    Delay start. Seconds from 2000-01-01.
            Encoded as two consecutive 16-bit le unsigned shorts, byte order
            (high to low) is: 2 1 4 3.
516    2    Delay start enabled. 0 = false, 1 = true.
524    2    Battery cutoff voltage. Divide by 10 to get actual value. (16-bit le)
526    2    Battery Cutoff Enabled. 0 = off, 1 = on.
528    2    Led settings. 0 = "LED 5 minutes only", 1 = "LED always"
532    2    Sensitivity Left. 16-bit le signed short. Divide by 10 to get actual value.
            Sometimes is 0xFF. Interpreting 0xFF as -1 (not set or N/A).
534    2    Sensitivity Right. As above.
536    2    Sensitivity Enabled. 0 = off, 1 = on.
539    1    Trigger window. 1 byte unsigned. Divide by 10 to get actual value in seconds.
            Sometimes is 0xFF. Interpreting 0xFF as -1 (not set or N/A).
540    2    The channel assignment: 0 = Left, 1 = Right, 2 = Both
542    2    Unknown 2 bytes, observed to be null
544    2    Gain left channel (16-bit le short).
            Integer, divide by 2 to get actual value (graduated 0.5 dB steps)
546    2    Gain right channel (16-bit le short). Divide by 2 as above.
548    2    High pass filter left (16-bit-le short). 0 = off, 1 = 220Hz, 2 = 1000 Hz
            On SM4BAT-FS this is the _16k High Filter_: 0 = off, 1 = on
550    2    High pass filter right (as per HPF left, but no provision for SM4BAT-FS).
552    4    Sample rate. Byte layout as follows:
            For values less than ushort.max: bytes 3..4  are sample rate (16-bit le)
            For higher values: byte 1 is extra bits.
            Final value is: 1,3,4... 20-bit le?
            Byte 2 is always observed to be null.
556    2    Division ratio (SM4BAT-ZC): 0 = 8, 1 = 16.
558    2    Preamp flag:
                Bit 14: right channel 0 = preamp off, 1 = preamp on 26 dB
                Bit 15: left channel 0 = preamp off, 1 = preamp on 26 dB
560    2    Min Duration (16-bit le).
            Stored as an integer of seconds a 1x10^-4.
            Divide by 10 to get value in milliseconds.
562    2    Unknown. Sometimes observed to be not null (15/0x000f).
564    2    Max duration (16-bit le). `0` means _none_.
            Stored as an integer of milliseconds.
568    2    Unknown, observed to be null.
570    2    Min Trig Frequency. Might only be one byte?
            Integer in kHz (16-bit le).
572    2    Unknown, observed to be null
574    2    Unknown, mostly observed to be `16` but was found to be `15` on 
            a SM4BAT-ZC schedule (16-bit le).
576    2    Unknown, observed to be null.
578    2    Unknown, sometimes observed to be 130/0x8200 for SM4BAT-ZC configurations.
580    2    Unknown, observed to be null.
582    2    Unknown, sometimes observed to be 130/0x8200 for SM4BAT-ZC configurations.
584    2    Trigger level (SM4BAT-FS) (signed 16-bit le)
586    2    Trigger level? Unknown, sometimes observed to be `12`/0x000c 
            for SM4BAT-FS otherwise null.
592    2    SM4BAT-FS: Max Length in seconds
            SM4BAT-ZC: Max Trig Time in seconds
596    2    Max (recording) Length in minutes (16-bit le).
600    1    Compression: 0 = None, 8 = W4V-8, 10 = W4V-6, 12 = W4V-4
604    1    Model
            0x00 = SM4BAT
            0x01 = SM4BAT-FS
            0x02 = SM4BAT-ZC
608     2   Scenario: memory Card A. 16-bit le. 0 = empty, otherwise value is 2^value = size in GB.
610     2   Scenario: memory Card B. As per memory Card A.
612     2   Scenario: Mic 0. 16-bit le.
            0 = internal
            1 = SMM-A1
            2 = SMM-A2
            3 = ??? U1
            4 = ??? U2
            5 = SMM-H1
            6 = SMM-H2
            7 = Internal
614     2   Scenario: Mic 1. As per Mic 0.
616     2   Scenario: Trig ratio. 16-bit le. Whole integer percentage points.
            Is 0xFF when read from a recording. Interpreting 0xFF as -1 (not set or N/A).
618     2   Scenario: Battery energy. 16-bit le. Integer value in watt hours.
            Is 0xFF when read from a recording. Interpreting 0xFF as -1 (not set or N/A).
620     4   Scenario: start. Two consecutive 16-bit le interpreted as one 32-bit le.
            The value is the number of seconds from 2000-01-01.
640    1    Schedule mode.
            0x00 = simple
            0x01 = advanced
644    4    High precision latitude. Encoded as two consecutive 16-bit le bytes.
            It is a signed number. Divide by 100,000 to get value in degrees.
            Byte order is: 1 0 3 2
648    4    High precision longitude. Encoded as two consecutive 16-bit le bytes.
            It is a signed number. Divide by 100,000 to get value in degrees.
            For some reason E (east) is negative and W (west) is positive.
            Byte order is: 1 0 3 2
716    1    Unknown. Number of first step in an advanced schedule?
720    1    Number of last step in an advanced schedule
728    396  Steps in the advanced schedule.
            Array of 99 4-byte records.
            Unused records are null bytes.
            Each record consists of:
            b0: the payload overflow byte
            b1: schedule step type
            b2..3: the payload.
            Use the constant lookup table below to decode step types.
```

## Step types

Each step consists of a 4 byte payload:

```
b0        b1              b2       b3
overflow  type+overflow   payload  payload
```

Payload bytes are filled first. If they are not enough content flows up to higher order bytes,
like b0, and from what I've rarely observed, the b1[0..2] bits.

This isn't as weird as it seems: these are two 16-bit little endian numbers encoded sequentially
(rather than an four byte int32). This explains why the byte ordering is so often 
`b1 b0 b3 b2` (first number high byte, then low byte, second number high byte, then low byte).

The type seems to be a 6-bit integer (`b0 >> 2`) that can encode up to 64 unique commands.
I've only counted 26 commands exposed in the GUI.

`ID` in the table below refers to the value as a whole byte (not bit-shifted) because it is
easier to look at example schedule files and visually match bytes when reverse engineering
schedules.

```
ID    ID>>2 Name      SM3 SM4 Payload     Description
-----------------------------------------------------------------------------------------------------
0x08  0x02  HPF       T   F   enum        High pass filter: frequency of each channel in kilohertz
                                          enum:
                                             0x00 = Off
                                             0x01 = 220 Hz
                                             0x02 = 1 kHz
                                             0x03 = 16 kHz
                                          b2[0..4]: channel 1 (right)
                                          b2[4..8]: channel 0 (left)
0x0C  0x03  GAIN      T   F   integer      Gain: value is integer divided by 10 and in dB.
                                          0xFF = Automatic.
                                          b2: channel 1 (right)
                                          b3: channel 0 (left)
0x10  0x04  FS        T   F   compound    Full spectrum mode.
                                          b0[3]: 0 = normal, 1 = Auto rate
                                          b0[4..7]: 3-bit number enum for channel mode:
                                             0 = CH 0 (left only)
                                             1 = CH 1 (right only)
                                             2 = CH 0+1 (stereo)
                                             6 = off
                                             7 = auto
                                          b0[7]: 0 = WAVE, 1 = WAC
                                          b0[0..3] + b3 + b2: the sample rate in hertz (19-bit le)
0x14  0x05  ZC        T   F   compound    Zero crossing mode.
                                          Channel layout is the same as FS.
                                          b2: is enum of mode. 0 = DIV 4, 1 = DIV 8, 2 = DIV 16
0x18  0x06  FREQMIN   T   F   compound    Minimum frequency. Two values, integers in kilohertz, 
                                          one for each channel. 0 = off.
                                          b2: CH 1 (right) 
                                          b3: CH 0 (left)
0x1C  0x07  FREQMAX   T   F   compound    Maximum frequency. Two values, integers in kilohertz, 
                                          As per FREQMIN.
0x20  0x08  DMIN      T   F   compound    Duration minimum. Two values, each 13-bit integers, 1 per channel, 0 = off.
                                          Divide by 10 to get the value in milliseconds
                                          b3[0..5] + b2: CH 1 (right)
                                          b1[0..2] + b0 + b3[5..8]: CH 0 (left)
0x24  0x09  DMAX      T   F   compound    Duration maximum.
                                          As per DMIN.
0x28  0x0A  TRGLVL    T   F   compound    Trigger level. Two values, 1 per channel in decibels, 
                                          0x7F = off, 0xFF = Automatic.
                                          Each value is one byte. Positive values have high bit 7 set to 1 (weird). 
                                          When bit 7 is 1, the value is bits 0..7 + 1
                                          When bit 7 is 0, the value is (127 - bits 0..7) * -1
                                          b2: CH 1 (right)
                                          b3: CH 0 (left)
0x2C  0x0B  TRGWIN    T   F   compound    Trigger window. Two values, each 10-bit unsigned integers, 
                                          1 per channel. Divide value by 10 to get seconds.
                                          0x00 = off
                                          b3[0..2] + b2 : CH 1 (right)
                                          b0[0..4] + b3[2..8]: CH 0 (left)
0x30  0x0c  TRGMAX    T   F   compound    Trigger maximum.
                                          as per TRGWIN.
0x34  0x0D  NAP       T   F   minutes     NAP duration. 0 = off.
                                          b2: minutes to nap 
0x38  0x0E  AT DATE   -   T   date        partitioned in 3 parts:
                                          b2[0..5]: day of month as 5-bit unsigned int
                                          b3[0] + b2[5..8]: month as 4-bit unsigned int
                                          b3[1..8]: 7-bit year calculated as 2000 + <year>
0x3C  0x0F  AT TIME   -   T   time        17-bit le, seconds from midnight
                                          b0[0] + b3 + b2: is the number
0x40  0x10  AT SRIS   -   T   flag+time   unsigned 18-bit le, seconds from event (-1)
                                          b0[2]: is a flag: 0 = before event/negative, 
                                                            1 = after event/positive
                                          b0[0..1] + b3 + b2: value + 1 is the number
0x44  0x11  AT SSET   -   T   time        as per AT SRIS
0x48  0x12  REPEAT    -   T   <empty>
0x4C  0x13  UNTDATE   -   T   date        as per AT DATE
0x50  0x14  UNTTIME   -   T   time        as per AT TIME
0x54  0x15  UNTSRIS   -   T   flag+time   as per AT SRIS
0x58  0x16  UNTSSET   -   T   flag+time   as per AT SRIS
0x5C  0x17  UNTCOUNT  -   T   integer     Number of counts to reach. `0` indicates Forever.
                                          b[2] as integer.
0x60  0x18  RECORD    -   T   time        17-bit le, seconds to record for
                                          b0[0] + b3 + b2: is the number
0x64  0x19  PAUSE     T   T   time        17-bit le, seconds to record for
                                          b0[0] + b3 + b2: is the number
0x68  0x1A  PLAY      T   F   time        Play a recording. Play the file with the name CALL<value>.WAV
                                          b0[3..7]: 4-bit number index to play.
0x6C  0x1B  FEATURE   T   F   compound    Activate a feature.
                                          Feature 0 = 01 - LED DISABLE
                                          FEATURE 1 = 02 - 32BIT ENABLE
                                          14 additional unlabeled features.
                                          b2[0..4]: feature number, 4-bit integer
                                          b2[4..8]: feature status. 0 = off, 1 = on

```
