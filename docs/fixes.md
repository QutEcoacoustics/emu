# Fixes

Sometimes sensors produce faulty audio files. These faults can be introduced
via firmware bugs, power failures, or through intentional design.

There are two broad categories of problems:

- problems that can be fixed
- problems that can be detected but not fixed

Any problems that can be fixed will be fixed in-place; that is,
the file with the problem will be updated so it no longer has a problem.

Any problems that cannot be fixed will be renamed with an new extension.
This is useful because:

- a new extension means the file will not registered as an audio file anymore
- such files are automatically filtered out of searches. e.g. searching for 
  FLAC (`*.flac`) or WAVE (`*.wav`) files will not return the error files
- no information is destroyed or changed (it is safe!)
- you can easily delete or filter the error files later

## Listing fixes and checks

To see what problems EMU knows to check for, use the `emu fix list` command:

```bash
$ emu fix list
EMU can fix these problems:
┌───────┬───────────────────────────┬─────────┬──────┬─────────────────────────────────────────────────────────────────────────────────┐
│ ID    │ Description               │ Fixable │ Safe │ URL                                                                             │
├───────┼───────────────────────────┼─────────┼──────┼─────────────────────────────────────────────────────────────────────────────────┤
│ FL001 │ Preallocated header       │ ✗       │ ✓    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL001.md │
│ FL008 │ Invalid datestamp (space) │ ✓       │ ✓    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL008.md │
│ FL010 │ Metadata Duration Bug     │ ✓       │ ✓    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL010.md │
│ FL005 │ Incorrect SubChunk2 size  │ ✓       │ ✓    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL005.md │
└───────┴───────────────────────────┴─────────┴──────┴─────────────────────────────────────────────────────────────────────────────────┘

Use `emu fix apply` to apply a fix to target files:

emu fix apply --fix XX001 *.wav

Or use `--fix-all` to apply all known fixes:

emu fix apply --fix-all XX001 *.wav
```

## Checking files for problems

You can check for one or more problems with the `emu fix check` command. 
The following checks any file with the `.flac` extension for the FL001
or FL010 problems:

```bash
$ fix check -f FL001 -f FL010 "F:\tmp\fixes\*.flac"
File F:\tmp\fixes\20181101_060000_REC [-27.3866 152.8761].flac:
        - FL001: Affected. The file is a stub and has no usable data
        - FL010: Unaffected. Audio recording is not a FLAC file

File F:\tmp\fixes\20191125T000000+1000_REC.flac:
        - FL001: Affected. The file is a stub and has no usable data
        - FL010: Unaffected. Audio recording is not a FLAC file
```

- Use the `--help` option to find out more about the command command
  (`emu fix check --help`)
- You can check for more than one problem at a time by supplying
   additional `-f` (or `--fix`) arguments.
- Refer to our section on [Wildcards](./wildcards.md) to learn how to 
  target more than one file at a time

## Repairing or renaming problem files

You can repair or rename files with the `emu fix apply` command.
The following applies fixes to file with the `.flac` extension for the FL001
or FL010 problems:

``` bash
$ emu fix apply -f FL001 -f FL010 --dry-run "F:\tmp\fixes\*.flac"
DRY RUN would rename file to F:\tmp\fixes\20181101_060000_REC [-27.3866 152.8761].flac.error_FL001
File F:\tmp\fixes\20181101_060000_REC [-27.3866 152.8761].flac.error_FL001:
        - FL010: Unaffected Audio recording is not a FLAC file.
                Action taken: NoOperation.
        - FL001: Affected The file is a stub and has no usable data.
                Action taken: Renamed. Renamed to: F:\tmp\fixes\20181101_060000_REC [-27.3866 152.8761].flac.error_FL001

DRY RUN would rename file to F:\tmp\fixes\20191125T000000+1000_REC.flac.error_FL001
File F:\tmp\fixes\20191125T000000+1000_REC.flac.error_FL001:
        - FL010: Unaffected Audio recording is not a FLAC file.
                Action taken: NoOperation.
        - FL001: Affected The file is a stub and has no usable data.
                Action taken: Renamed. Renamed to: F:\tmp\fixes\20191125T000000+1000_REC.flac.error_FL001

DRY RUN This was a dry run, no changes were made
```

- Use the `--help` option to find out more about the command command
  (`emu fix apply --help`)
- **INote the use of `--dry-run`**. When dry run is used nothing is changed!
  This allows you to safely check that the behavior of EMU is as you expect.
- EMU has a `--backup` option that will create a copy of the original file
  before making changes.
- EMU has a `--no-rename` option that prevents renaming unfixable files

To actually make changes, remove the `--dry-run` option and run again:

``` bash
$ emu fix apply -f FL001 -f FL010 --dry-run "F:\tmp\fixes\*.flac"
File F:\tmp\fixes\20181101_060000_REC [-27.3866 152.8761].flac.error_FL001:
        - FL010 is Unaffected Audio recording is not a FLAC file.
          Action taken: NoOperation.
        - FL001 is Affected The file is a stub and has no usable data.
          Action taken: Renamed. Renamed to: F:\tmp\fixes\20181101_060000_REC [-27.3866 152.8761].flac.error_FL001

File F:\tmp\fixes\20191125T000000+1000_REC.flac.error_FL001:
        - FL010 is Unaffected Audio recording is not a FLAC file.
          Action taken: NoOperation.
        - FL001 is Affected The file is a stub and has no usable data.
          Action taken: Renamed. Renamed to: F:\tmp\fixes\20191125T000000+1000_REC.flac.error_FL001
```



## Examples

### Rename stub files (Fix FL001)

Frontier Labs sensors often produce small but invalid files when a sensor
fault occurs. This is problem is known as [FL001](https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL001.md).

There is no data in these files and since there is nothing to repair EMU can't repair these files.
Instead EMU will rename them to get them out of the way.

Here we can see some examples of these files:

```bash
$ ls -l
-rwxr--r-- 1 anthony anthony   44 Feb 10 21:12 '20181101_060000_REC [-27.3866 152.8761].flac'
-rwxr--r-- 1 anthony anthony  153 Feb 10 21:12  20191125T000000+1000_REC.flac
```

If we use EMU to fix these files:

``` bash
$ emu fix apply -f FL001 -f FL010 --dry-run "F:\tmp\fixes\*.flac"
File F:\tmp\fixes\20181101_060000_REC [-27.3866 152.8761].flac.error_FL001:
        - FL010 is Unaffected Audio recording is not a FLAC file.
          Action taken: NoOperation.
        - FL001 is Affected The file is a stub and has no usable data.
          Action taken: Renamed. Renamed to: F:\tmp\fixes\20181101_060000_REC [-27.3866 152.8761].flac.error_FL001

File F:\tmp\fixes\20191125T000000+1000_REC.flac.error_FL001:
        - FL010 is Unaffected Audio recording is not a FLAC file.
          Action taken: NoOperation.
        - FL001 is Affected The file is a stub and has no usable data.
          Action taken: Renamed. Renamed to: F:\tmp\fixes\20191125T000000+1000_REC.flac.error_FL001
```

Then we can see that those files no longer have the `.flac` extension:

```bash
$ ls -l
-rwxr--r-- 1 anthony anthony   44 Feb 10 21:12 '20181101_060000_REC [-27.3866 152.8761].flac.error_FL001'
-rwxr--r-- 1 anthony anthony  153 Feb 10 21:12  20191125T000000+1000_REC.flac.error_FL001
```



### Check if a file is affected by the FL010 metadata bug

```bash
$ emu fix check --fix FL010 20211004T200000+0000_Rec2_-18.1883+144.5414.flac
Looking for targets...
File 20211004T200000+0000_Rec2_-18.1883+144.5414.flac:
        - FL010: Affected. File's duration is wrong
```

### Fix the FL010 metadata duration bug

We recommend doing a ""dry run"" (a practice run) before doing any command that can modify a file.

```bash
$ emu fix apply --fix FL010 20211004T200000+0000_Rec2_-18.1883+144.5414.flac --dry-run
Looking for targets...
2021-11-29T12:27:14.6945889+10:00 [INFO] <1> Emu.Utilities.DryRun ["dry run would"] write total samples 158646272
2021-11-29T12:27:14.7064992+10:00 [INFO] <1> Emu.Utilities.DryRun ["dry run would"] update firmware tag with EMU+FL010
File 20210617T080000_Rec2_-18.2656+144.5564.flac:
        - FL010: Fixed. Old total samples was 317292544, new total samples is: 158646272

2021-11-29T12:27:14.7077852+10:00 [INFO] <1> Emu.Utilities.DryRun  This was a dry run, no changes were made
```

Then if it looks like it will work, the "real run" (remove the `--dry-run` flag)

```bash
$ emu fix apply --fix FL010 20211004T200000+0000_Rec2_-18.1883+144.5414.flac
Looking for targets...
File 20211004T200000+0000_Rec2_-18.1883+144.5414.flac:
        - FL010: Fixed. Old total samples was 317292544, new total samples is: 158646272
```

### Fix the FL008 spaces in datestamp bug

Sometimes we find spaces in datestamps due to [FL008](https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL008.md).

For example, a problem file, a fix with EMU, and the result:

```bash
$ ls -l
-rwxr--r-- 1 anthony anthony 622592 Jul 13 23:25 '201906 7T095935+1000_REC [19.2144 152.8811].flac'

$ emu fix apply -f FL008 "201906 7T095935+1000_REC [19.2144 152.8811].flac"
Looking for targets...
File F:\tmp\fixes\20190607T095935+1000_REC [19.2144 152.8811].flac:
        - FL008 is Affected Space in datestamp detected.
          Action taken: Fixed. Inserted `0` into datestamp

$ ls -l
-rwxr--r-- 1 anthony anthony 622592 Jul 13 23:25 '20190607T095935+1000_REC [19.2144 152.8811].flac'
```

