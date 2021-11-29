# **EMU**: <small>**E**coacoustics **M**etadata **U**tility</small> <img align="right" width="100" height="100" alt="A surprised Emu." src="docs/media/emu-small.png"/>

A generic tool for metadata manipulation of ecoacoustic audio recordings

![main help screenshot](docs/media/main_help.png)

## But why though?

 <img align="right"  height="163" alt="A surprised Emu." src="docs/media/but-why.gif"/>

Currently every environmental sensor captures audio recordings and recording metadata in a differant way.

There are efforts underway to standardize this process, but even in a perfect world, there are still plenty of problems to  deal with:

- standards adoption takes time
- there a millions of recording made using older sensors
- there a many problems and quirks with existing sensors

## Features

_EMU_ aims to be a babelfish—an adapter—between these formats. _EMU_ can:

- extract metadata from audio recordings
- recognize and parse different date stamp formats
- rename files so that they have a consistent format
- fix problems in recordings so you can recover and use the data (idempotently)
- do this in various formats (human friendly, compact, json, json-lines, and csv)

## Output

Emu is designed to be used as part of an automated process - that is why it is a command line tool.

Emu can output results from must of its commands in a format that best suits you:

- default - a format meant to be easy to read. Will include extra comments, headers, and footers.
- compact
- JSON
- [JSON Lines](https://jsonlines.org/) (easier to parse line by line records)
- CSV (a great choice for R's data frames)

![example format output](docs/media/formats.png)

## Status

It's still early days. _EMU_ is an _alpha-level_ product and we have a lot more fixes and utilities we want to add to it.
However, _EMU_ is being actively used in large-scale automated ecoacoustics pipelines to validate and repair faulty audio recordings.

- EMU runs on Windows, Linux, and Mac*
- A docker container is provided (see [Docker](#docker))
- The metadata extraction and date recognition features are currently a work in progress
- There is one Fix that works well: `FL010` - the Frontier Labs metadata duration bug can be repaired automatically

<small>*EMU needs to be compiled from source on Macs</small>

## Examples

### Learn about the options

Append `--help` after any command:

```sh
> emu --help
...
> emu fix --help
...
> emu fix check --help
...
> emu fix list --help
...
> emu fix apply --help
...
> emu rename --help
...
```

### Check if a file is affected by the FL010 metadata bug

```sh
> emu fix check --fix FL010 20211004T200000+0000_Rec2_-18.1883+144.5414.flac
Looking for targets...
File 20211004T200000+0000_Rec2_-18.1883+144.5414.flac:
        - FL010: Affected. File's duration is wrong
```
### Fix the FL010 metadata duration bug

We recommend doing a ""dry run"" (a practice run) before doing any command that can modify a file.

```sh
> emu fix apply --fix FL010 20211004T200000+0000_Rec2_-18.1883+144.5414.flac --dry-run
Looking for targets...
2021-11-29T12:27:14.6945889+10:00 [INFO] <1> MetadataUtility.Utilities.DryRun ["dry run would"] write total samples 158646272
2021-11-29T12:27:14.7064992+10:00 [INFO] <1> MetadataUtility.Utilities.DryRun ["dry run would"] update firmware tag with EMU+FL010
File 20210617T080000_Rec2_-18.2656+144.5564.flac:
        - FL010: Fixed. Old total samples was 317292544, new total samples is: 158646272

2021-11-29T12:27:14.7077852+10:00 [INFO] <1> MetadataUtility.Utilities.DryRun  This was a dry run, no changes were made
```

Then if it looks like it will work, the "real run" (remove the `--dry-run` flag)

```sh
> emu fix apply --fix FL010 20211004T200000+0000_Rec2_-18.1883+144.5414.flac
Looking for targets...
File 20211004T200000+0000_Rec2_-18.1883+144.5414.flac:
        - FL010: Fixed. Old total samples was 317292544, new total samples is: 158646272
```

### Rename your files (AudioMoth)

Emu will change dates in files to a common and recommended date format where possible.

To rename all AudioMoth files in the current folder and all sub-folders:

```sh
> emu rename **/*.WAV
Looking for targets...
-   Renamed 5B07FAC0.WAV
         to 20180525T120000Z.WAV
1 files, 1 renamed, 0 unchanged, 0 failed
```

### Rename: add an offset to a local datestamp

Most acoustic monitors record only a local datestamp - this means the date stamp has no timezone information.
For example a classic SM4 datestamp looks like this: `PILLIGA_20121204_234600.wav`. We can see it was 
recorded at 11:46 PM... but is that 11:46 PM UTC time? Eastern Australian Standard Time? Eastern Australian Daylight Savings Time? Soemthing else!?

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

### Rename: Changing the offset

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

### Rename: safety

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

### Rename: flatten

Got too many folders? Flattern the folder hierarchy with:

```sh
> emu rename **/*.wav --flatten
Looking for targets...
-   Renamed WAV\20180525T220000+1000.WAV
         to 20180525T220000+1000.WAV
-   Renamed WAV\PILLIGA_20121204T224600+1000.wav
         to PILLIGA_20121204T224600+1000.wav
4 files, 2 renamed, 2 unchanged, 0 failed
...



## Build notes

If you want to run a copy of `emu`, you'll need to build it yourself (we'll have an installer soon but it is not yet ready).

1. You'll need a .NET 6 SDK installed
    - From <https://dotnet.microsoft.com/download/dotnet/6.0> choose a download from the SDK section, for your OS, and CPU architecture
2. Next clone this repo to your computer
3. Open a Terminal prompt (in `pwsh` (Windows/MacOs/Linux) or `bash` (Linux/MacOS)) and navigate to the repo folder
4. Then choose **one** of publish commands from below that matches your OS and CPU architecture:

    ```shell
    dotnet publish -r linux-x64 --self-contained -o ./publish/linux-x64 ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r win-x64 --self-contained -o ./publish/win-x64 ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r osx-x64 --self-contained -o ./publish/osx-x64 ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r osx-arm64 --self-contained -o ./publish/osx-arm64 ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r linux-arm --self-contained -o ./publish/linux-arm ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r linux-arm64 --self-contained -o ./publish/linux-arm64 ./src/MetadataUtility/MetadataUtility.csproj
    ```

5. Copy the resulting folder from the `publish` directory to somewhere on your computer
6. You can then use `emu` by referring to the full path of the `emu` binary.
    - It will be `emu.exe` on Windows
    - `emu` (with no extension)  on Linux and MaxOS

Other notes:

- You can build for development with `dotnet build`
- You can test in development with `dotnet test`
- You can run EMU dev builds from the `src/MetadataUtility` folder with `dotnet run  -- `
  - Arguments after the `--` are passed to EMU as if you had run EMU directly
- You can build all releases for all platforms with the command:

    ```powershell
    $rids = ("win-x64", "linux-x64", "osx-x64", "osx-arm64", "linux-arm", "linux-arm64")
    $rids | ForEach-Object { dotnet publish .\src\MetadataUtility\ -c Release -o ./publish/$_ --self-contained -r $_ }
    ```

- Release a new version with:

  ```shell
  dotnet test
  git tag -a -m "Version x.x" x.x
  ./docker_build_and_push.ps1
  ```

## Docker

[![Docker Image Version (latest semver)](https://img.shields.io/docker/v/qutecoacoustics/emu)](https://hub.docker.com/repository/docker/qutecoacoustics/emu)

Using Docker is considered an advanced workflow - most users should not try to use docker if they want to use `emu`.

EMU uses a multistage build dockerfile. In your dockerfile you should be able to:

```dockerfile

ENV DOTNET_RUNNING_IN_CONTAINER=true 
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

COPY --from=qutecoacoustics/emu:latest /emu /emu

```

This will copy a linux-x64 build of EMU to the `/emu` folder in your container. It should be executable and need no further dependencies:

```bash
/emu/emu --version
```

The environment variables are recommended as well.

## Acknowledgements

This project is funded through QUT Ecoacoustics and the [Open Ecoacoustics](https://openecoacoustics.org/) projects

[![open ecoacoustics logo](./docs/media/OpenEcoAcoustics_horizontal_rgb_white_bg.png)](https://openecoacoustics.org/)