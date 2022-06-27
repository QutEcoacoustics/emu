# Contributing 

## Contributions

Contributions are welcome!

- Find an issue you want to work on (or create a new issue if you want a new feature)
- Discuss the issue with the maintainers to get guidance on implementation details
- Fork this repo
- Make your changes on a branch
- Create a pull request when you're ready for a review

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
    - `emu` (with no extension) on Linux and MaxOS

Other notes:

-   You can build for development with `dotnet build`
-   You can test in development with `dotnet test`
-   You can run EMU dev builds from the `src/MetadataUtility` folder with `dotnet run -- `
    -   Arguments after the `--` are passed to EMU as if you had run EMU directly
-   You can build all releases for all platforms with the command:

    ```powershell
    $rids = ("win-x64", "linux-x64", "osx-x64", "osx-arm64", "linux-arm", "linux-arm64")
    $rids | ForEach-Object { dotnet publish .\src\MetadataUtility\ -c Release -o ./publish/$_ --self-contained -r $_  }
    ```

-   Release a new version with:

    ```powershell
    dotnet test
    git tag -a -m "Version x.x.x" x.x.x
    $rids = ("win-x64", "linux-x64", "osx-x64", "osx-arm64", "linux-arm", "linux-arm64")
    $rids | ForEach-Object { dotnet publish .\src\MetadataUtility\ -c Release -o ./publish/$_ --self-contained -r $_  }
    Get-ChildItem ./publish/ -Directory | % { Compress-Archive -Path $_ -DestinationPath ('./publish/' + $_.Name + ".zip") }
    git push --tags
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