# **EMU**: <small>**E**coacoustics **M**etadata **U**tility</small> <img align="right" width="100" height="100" alt="A surprised Emu." src="docs/media/emu-small.png"/>

A generic tool for metadata manipulation of ecoacoustic audio recordings


## But why though?

Currently every environmental sensor captures audio recordings and recording metadata in a differant way.

There are efforts underway to standardize this process, but even in a perfect world, there are still plenty of problems to  deal with:

- standards adoption takes time
- there a millions of recording made using older sensors
- there a many problems and quirks with existing sensors

_EMU_ aims to be a babelfish—an adapter—between these formats. _EMU_ can:

- extract metadata from audio recordings
- recognise and parse different datestamp formats
- rename files so that they have a consistent format
- fix problems in recordings so you can recover and use the data (idempotently)
- do this in various formats (human friendly, compact, json, json-lines, and csv)


## Status

It's still early days. _EMU_ is an _alpha-level_ product and we have a lot more fixes and utilities we want to add to it.
However, _EMU_ is being actively used in large-scale automated ecoacoustics pipelines to validate and repair faulty audio recordings.

- EMU runs on Windows, Linux, and Mac*
- A docker container is provided (see [Docker](#docker))
- The metadata extraction and date recognition featues are currently a work in progress
- There is one Fix that works well: `FL010` - the Frontier Labs metadata duration bug can be repaired automatically


*EMU needs to be compiled from source on Macs

## Build notes

- You'll need a .NET 6 SDK installed
- You can build for development with `dotnet build`
- You can test in development with `dotnet test`
- You can run EMU dev builds from the `src/MetadataUtility` folder with `dotnet run  -- ` 
    - Arguments after the `--` are passed to EMU as if you had run EMU directly
- You can build all releases for all platforms with the command:

    ```powershell
    $rids = ("win-x64", "linux-x64", "osx-x64", "osx-arm64", "linux-arm", "linux-arm64")
    $rids | ForEach-Object { dotnet publish .\src\MetadataUtility\ -c Release -o ./publish/$_ --self-contained -r $_ }
    ```

    or any single one:

    ``` bash
    dotnet publish -r linux-x64 --self-contained -o ./publish/linux-x64 ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r win-x64 --self-contained -o ./publish/win-x64 ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r osx-x64 --self-contained -o ./publish/osx-x64 ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r osx-arm64 --self-contained -o ./publish/osx-arm64 ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r linux-arm --self-contained -o ./publish/linux-arm ./src/MetadataUtility/MetadataUtility.csproj
    dotnet publish -r linux-arm64 --self-contained -o ./publish/linux-arm64 ./src/MetadataUtility/MetadataUtility.csproj
    ```

## Docker

[![Docker Image Version (latest semver)](https://img.shields.io/docker/v/qutecoacoustics/emu)](https://hub.docker.com/repository/docker/qutecoacoustics/emu)

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

[![open ecoacoustics logo](./docs/media/OpenEcoAcoustics_horizontal_rgb.png)](https://openecoacoustics.org/)