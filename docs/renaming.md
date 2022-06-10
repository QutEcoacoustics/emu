# Renaming

## Examples

### Rename your files: AudioMoth

Emu will change dates in files to a common and recommended date format where possible.

To rename all AudioMoth files in the current folder and all sub-folders:

```sh
> emu rename **/*.WAV
Looking for targets...
-   Renamed 5B07FAC0.WAV
         to 20180525T120000Z.WAV
1 files, 1 renamed, 0 unchanged, 0 failed
```

### Rename your files: add an offset to a local datestamp

Most acoustic monitors record only a local datestamp - this means the date stamp has no timezone information.
For example a classic SM4 datestamp looks like this: `PILLIGA_20121204_234600.wav`. We can see it was
recorded at 11:46 PM... but is that 11:46 PM UTC time? Eastern Australian Standard Time? Eastern Australian Daylight Savings Time? Something else!?

For small projects adding timezone information won't help a lot. But for any project which spans multiple timezones (
including daylight savings time) we recommend you add this information if your files after you pull them off of your
SD cards:

```sh
> emu rename **/*.wav --offset "+11:00"
Looking for targets...
-   Renamed PILLIGA_20121204_234600.wav
         to PILLIGA_20121204T234600+1100.wav
1 files, 1 renamed, 0 unchanged, 0 failed
```

### Rename your files: Changing the offset

The two dates <date>2021-10-04T20:00:00+00:00</date> and <date>2021-10-05T06:00:00+10:00</date> represent the exact
same instant in time! They're the same... just interpreted differently.

For someone in the UK they experienced that instant as 8 PM. Meanwhile, for someone in Australia, it was 6 AM the next day.

Even though the dates aren't "wrong", it's not useful to try and interpret natural activities in another timezone.
An Australian dawn chorus shouldn't look like it happened at 8 PM (even though it did for that UK observer).

To move all dates to a new UTC offset, use the `--new-offset` argument:

```sh
> emu rename **/*.wav **/*.flac --new-offset "+10:00"
Looking for targets...
-   Renamed 20210617T080000+0000_Rec2_-18.2656+144.5564.flac
         to 20210617T180000+1000_Rec2_-18.2656+144.5564.flac
-   Renamed 20211004T200000+0000_Rec2_-18.1883+144.5414.flac
         to 20211005T060000+1000_Rec2_-18.1883+144.5414.flac
-   Renamed 5B07FAC0.WAV
         to 20180525T220000+1000.WAV
-   Renamed PILLIGA_20121204T234600+1100.wav
         to PILLIGA_20121204T224600+1000.wav
4 files, 4 renamed, 0 unchanged, 0 failed
```


### Rename your files: safety

If you're worried about a rename you can instead create a copy of your files:

```sh
> emu rename **/*.wav --copy-to "G:\RenamedFiles"
...
```

Remember to do a dry-run before operations that modify data!

```sh
> emu rename **/*.wav --copy-to "G:\RenamedFiles" --dry-run
...
```

### Rename your files: flatten

Got too many folders? Flattern the folder hierarchy with:

```sh
> emu rename **/*.wav --flatten
Looking for targets...
-   Renamed WAV\20180525T220000+1000.WAV
         to 20180525T220000+1000.WAV
-   Renamed WAV\PILLIGA_20121204T224600+1000.wav
         to PILLIGA_20121204T224600+1000.wav
4 files, 2 renamed, 2 unchanged, 0 failed
```