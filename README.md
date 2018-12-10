# metadata-extractor
A generic tool for metadata extraction of ecoacoustic audio recordings



# Build notes

```powershell
("win-x64", "linux-x64", "osx-x64") | % { dotnet publish .\src\MetadataUtility\ -c Release -o ../../publish/$_ --self-contained -r $_ }
```