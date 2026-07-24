# Provenance

These files was sourced by Doug Hynes from ECCC and are licensed to us under a
Creative Commons By Attribution v4.0 license.

https://github.com/QutEcoacoustics/emu/issues/440

Contact https://github.com/DougPHynes

## Fault information

No known faults.

## Modifications

We nulled out the bulk of the content in this files to make it easier to compress and distribute.

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
