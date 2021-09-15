# **EMU**: <small>**E**coacoustics **M**etadata **U**tility</small> <img align="right" width="100" height="100" alt="A surprised Emu." src="docs/media/emu-small.png"/>

A generic tool for metadata extraction of ecoacoustic audio recordings

## Build notes

```powershell
("win-x64", "linux-x64", "osx-x64") | % { dotnet publish .\src\MetadataUtility\ -c Release -o ../../publish/$_ --self-contained -r $_ }
```