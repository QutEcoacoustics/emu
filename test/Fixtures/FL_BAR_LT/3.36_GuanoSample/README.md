# Provenance

https://drive.google.com/drive/folders/1iifEJZoj-I3Ejwf5hwBuLJWMKyEammzd?usp=sharing

**Description**

- File name: 33901_BigHarbourIslandA1_20250710T130000-0300.wav
- Location: Big Harbour Island, Nova Scotia
- Recording date: 2025-07-10
- Start time: 13:00:00 AST (-0300)
- Format: WAV
- Size: approximately 604 MB

## License

Copyright © 2025 Doug Hynes

This recording is licensed under the Creative Commons Attribution 4.0
International (CC BY 4.0) License.

You are free to:

- Share — copy and redistribute the material in any medium or format.
- Adapt — remix, transform, and build upon the material for any purpose,
  even commercially.

Under the following terms:

- Attribution — You must give appropriate credit, provide a link to the
  license, and indicate if changes were made.

Full license:

https://creativecommons.org/licenses/by/4.0/
https://github.com/QutEcoacoustics/emu/issues/440

Contact https://github.com/DougPHynes

## Fault information

No known faults.

## Modifications

We nulled out the bulk of the content in this file to make it easier to compress and distribute.

We keep the samples from approximately the first and last 30 seconds of the file, as well as all the metadata,
so that it can be used for testing and validation.

The nulling of samples reduces the initial file size from 604MB to 7.52MB when compressed.

```pwsh
$p = "33901_BigHarbourIslandA1_20250710T130000-0300.wav"

# Computed from metadata:
# bytesPerSecond = 44100 * 2 * (16/8) = 176400
# start = 44 + (30 * 176400) = 5292044
# endExclusive = (44 + (158535669 * 4)) - (30 * 176400) = 628850720
$start = 5292044L
$endExclusive = 628850720L

$fs = [System.IO.File]::Open(
    $p,
    [System.IO.FileMode]::Open,
    [System.IO.FileAccess]::ReadWrite,
    [System.IO.FileShare]::None
)

try {
    $fs.Position = $start
    $zeros = New-Object byte[] (1MB)
    $remaining = $endExclusive - $start

    while ($remaining -gt 0) {
        $write = [Math]::Min($zeros.Length, [int]$remaining)
        $fs.Write($zeros, 0, $write)
        $remaining -= $write
    }
}
finally {
    $fs.Dispose()
}
```
