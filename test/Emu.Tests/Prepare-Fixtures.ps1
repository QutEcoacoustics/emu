param(
    [Parameter(Mandatory = $true)]
    [string]$FixturesRoot,

    [int]$ThrottleLimit = [Environment]::ProcessorCount
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$fixturesRootResolved = [System.IO.Path]::GetFullPath($FixturesRoot)

if (-not [System.IO.Directory]::Exists($fixturesRootResolved)) {
    throw "Fixtures root folder not found: $fixturesRootResolved"
}

$archives = [System.IO.Directory]::GetFiles($fixturesRootResolved, '*.zip', [System.IO.SearchOption]::AllDirectories)

$work = New-Object System.Collections.Generic.List[psobject]
foreach ($archivePath in $archives) {
    $expectedPath = $archivePath.Substring(0, $archivePath.Length - 4)

    $needsExtract = $true
    if ([System.IO.File]::Exists($expectedPath)) {
        $expectedInfo = [System.IO.FileInfo]::new($expectedPath)
        $archiveInfo = [System.IO.FileInfo]::new($archivePath)
        $needsExtract = $expectedInfo.LastWriteTimeUtc -lt $archiveInfo.LastWriteTimeUtc
    }

    if ($needsExtract) {
        [void]$work.Add([pscustomobject]@{
                ExpectedPath = $expectedPath
                ArchivePath  = $archivePath
            })
    }
}

if ($work.Count -eq 0) {
    Write-Output "[Prepare-Fixtures] No compressed fixtures need extraction."
    return
}

Write-Output "[Prepare-Fixtures] Preparing $($work.Count) compressed fixtures from '$fixturesRootResolved' with throttle $ThrottleLimit"

$work | ForEach-Object -ThrottleLimit $ThrottleLimit -Parallel {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = 'Stop'

    $expectedPath = $_.ExpectedPath
    $archivePath = $_.ArchivePath
    $expectedName = [System.IO.Path]::GetFileName($expectedPath)

    $archive = [System.IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        $fileEntries = @($archive.Entries | Where-Object { -not [string]::IsNullOrEmpty($_.Name) })

        $targetEntry = $fileEntries | Where-Object { $_.Name.Equals($expectedName, [System.StringComparison]::OrdinalIgnoreCase) } | Select-Object -First 1
        if (-not $targetEntry) {
            if ($fileEntries.Count -eq 1) {
                $targetEntry = $fileEntries[0]
            }
            else {
                throw "Archive '$archivePath' does not contain '$expectedName' and has $($fileEntries.Count) file entries"
            }
        }

        $directory = [System.IO.Path]::GetDirectoryName($expectedPath)
        if (-not [string]::IsNullOrEmpty($directory)) {
            [System.IO.Directory]::CreateDirectory($directory) | Out-Null
        }

        $tempPath = "$expectedPath.__tmp__$([Guid]::NewGuid().ToString('N'))"

        $source = $targetEntry.Open()
        try {
            $destination = [System.IO.File]::Open($tempPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
            try {
                $source.CopyTo($destination)
            }
            finally {
                $destination.Dispose()
            }
        }
        finally {
            $source.Dispose()
        }

        [System.IO.File]::Move($tempPath, $expectedPath, $true)
        Write-Output "[Prepare-Fixtures] Extracted '$archivePath' -> '$expectedPath'"
    }
    finally {
        $archive.Dispose()
    }
}

Write-Output "[Prepare-Fixtures] Completed fixture preparation."
