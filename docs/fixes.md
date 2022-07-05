# Fixes

## Examples

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
2021-11-29T12:27:14.6945889+10:00 [INFO] <1> Emu.Utilities.DryRun ["dry run would"] write total samples 158646272
2021-11-29T12:27:14.7064992+10:00 [INFO] <1> Emu.Utilities.DryRun ["dry run would"] update firmware tag with EMU+FL010
File 20210617T080000_Rec2_-18.2656+144.5564.flac:
        - FL010: Fixed. Old total samples was 317292544, new total samples is: 158646272

2021-11-29T12:27:14.7077852+10:00 [INFO] <1> Emu.Utilities.DryRun  This was a dry run, no changes were made
```

Then if it looks like it will work, the "real run" (remove the `--dry-run` flag)

```sh
> emu fix apply --fix FL010 20211004T200000+0000_Rec2_-18.1883+144.5414.flac
Looking for targets...
File 20211004T200000+0000_Rec2_-18.1883+144.5414.flac:
        - FL010: Fixed. Old total samples was 317292544, new total samples is: 158646272
```