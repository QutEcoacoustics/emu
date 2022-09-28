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
┌───────┬───────────────────────────┬─────────┬──────┬──────────────────────────────────────────────────────────────────────────────────────┐
│ ID    │ Description               │ Fixable │ Safe │ URL                                                                                  │
├───────┼───────────────────────────┼─────────┼──────┼──────────────────────────────────────────────────────────────────────────────────────┤
│ OE004 │ Empty file                │ ✗       │ ✓    │ https://github.com/ecoacoustics/known-problems/blob/main/open_ecoacoustics/OE004.md  │
│ FL001 │ Preallocated header       │ ✗       │ ✓    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL001.md      │
│ FL005 │ Incorrect SubChunk2 size  │ ✓       │ ✗    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL005.md      │
│ FL008 │ Invalid datestamp (space) │ ✓       │ ✓    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL008.md      │
│ FL010 │ Metadata Duration Bug     │ ✓       │ ✓    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL010.md      │
│ FL011 │ Partial file named data   │ ✓       │ ✗    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL011.md      │
│ FL012 │ Data chunk size is 0      │ ✓       │ ✗    │ https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL012.md      │
│ WA002 │ No data in file           │ ✗       │ ✓    │ https://github.com/ecoacoustics/known-problems/blob/main/wildlife_acoustics/WA002.md │
└───────┴───────────────────────────┴─────────┴──────┴──────────────────────────────────────────────────────────────────────────────────────┘

Use `emu fix check` or `emu fix check --all` to check all known fixes:

emu fix check --fix XX001 *.wav

Use `emu fix apply` to apply a fix to target files:

emu fix apply --fix XX001 *.wav
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
- **Note the use of `--dry-run`**. When dry run is used nothing is changed!
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

---

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

### Rename empty files (Fix OE004)

Sensors often produce empty audio files. 
This is problem is known as [OE004](https://github.com/ecoacoustics/known-problems/blob/main/open_ecoacoustics/OE004.md).

To rename any detected files (so they are no longer recognized as audio files) use 
`fix apply` with fix `OE004`:

``` bash
$ emu fix apply -f OE004 "**/*.flac"
```

That command renames (by adding the suffix `.error_empty`) to any FLAC file in 
any sub-folder of your present working folder. 

You can do it for WAVE and FLAC files at the same time:

``` bash
$ emu fix apply -f OE004 "**/*.flac" "**/*.wav"
```

😎

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

### Fix the FL005 incorrect data size bug

In firmwares before 3.0, FL sensors sometimes recorded the wrong duration for their files.
This is the [FL005](https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL005.md) problem.

For example, a problem file, a fix with EMU, and the result:

```bash
$ ls -l
-rwxr--r-- 1 anthony anthony 157610028 Jul 15 17:33 '20160809_063108_Dawn _1.4647 116.9136_.wav'

$ emu fix apply -f FL005 "20160809_063108_Dawn _1.4647 116.9136_.wav"
Looking for targets...
File F:\tmp\fixes\20160809_063108_Dawn _1.4647 116.9136_.wav:
        - FL005 is Affected RIFF length and data length are incorrect.
          Action taken: Fixed. RIFF length set to 157610020 (was 157610064). data length set to 157609984 (was 157610028)
```

### Fixing partial `data` files (FL011)

When FL sensors have trouble writing files they will abandon writing the file
and instead just leave a partial file behind named only `data` (with no extension).

This is [FL011](https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL011.md).

This is a very complex fix; there are lots of different problems that have to be dealt with, but generally these are the following steps that occur:

1. Each `data` file is inspected
2. If it has no size (is `0` bytes in length) it will be renamed to `data.error_empty` and the process ends
3. If the file is not a FLAC file an error is raised
4. If the file has a valid FLAC header, then that information is extracted
5. The vendor information included by Frontier Labs is extracted
6. We then scan the file for valid data
7. The file is split into two parts
   1. All the valid data
   2. And everything afterwards
8. The vendor data is used to rename segments and those segments are saved to separate files
   1. The valid part will have a name like `<date>_recovered.flac`
   2. The invalid part will have a name like `<date>_recovered.flac.truncated_part`

We keep the truncated part so that no data is deleted. You can remove the truncated parts after
a successful operation.

Let's see an example:

```bash
$ ls -lAR */
20200426_STUDY/:
total 212072
-rwxr--r-- 1 anthony anthony     26852 Sep 14 19:02 20200426T020000+0000_REC.flac
-rwxr--r-- 1 anthony anthony 217129512 May 14  2021 data

20210416_STUDY/:
total 0
-rwxr--r-- 1 anthony anthony 0 May 14  2021 data
```

This is a subset of files from a deployment. There are two `data` partial files that were produced.
Note one of the files is empty (has `0` bytes) and the other is not.

Let's fix them:

```bash
$ emu fix apply -f FL011 

Looking for targets...
2022-09-28T01:55:05.8702803+10:00 [INFO] <1> Emu.Utilities.FileMatcher  No wild card was provided, using the default **/*.flac **/*.wav **/*.mp3 **/data
File F:\tmp\fixes\3.17_PartialDataFiles\Robson-Creek-Dry-A_201\20200426_STUDY\20200426T020000+0000_REC.flac:
        - FL011 is NotApplicable: File is not named `data`.
          Action taken: NoOperation.

File F:\tmp\fixes\3.17_PartialDataFiles\Robson-Creek-Dry-A_201\20200426_STUDY\20200426T020000Z_recovered.flac:
        - FL011 is Affected: Partial file detected.
          Action taken: Fixed. Partial file repaired. New name is 20200426T020000Z_recovered.flac. Samples count was 317292544, new samples count is: 73035776. File truncated at 99824893.

File F:\tmp\fixes\3.17_PartialDataFiles\Robson-Creek-Dry-A_201\20210416_STUDY\data.error_empty:
        - FL011 is Affected: Partial file detected.
          Action taken: Renamed. Partial file was empty
```

And the resulting files afterwards:

```bash
ls -lAR */
20200426_STUDY/:
total 212072
-rwxr--r-- 1 anthony anthony     26852 Sep 14 19:02 20200426T020000+0000_REC.flac
-rwxr--r-- 1 anthony anthony  99824893 Sep 28 01:55 20200426T020000Z_recovered.flac
-rwxr--r-- 1 anthony anthony 117304619 Sep 28 01:55 20200426T020000Z_recovered.flac.truncated_part

20210416_STUDY/:
total 0
-rwxr--r-- 1 anthony anthony 0 May 14  2021 data.error_empty
```

Note:

1. The truncated file part that was removed from the first `data` file
2. The proper datestamp in the first repaired file
3. The empty file was renamed to `data.error_empty`

This fix is new and complicated. The use of the `--backup` option is strongly recommended!
