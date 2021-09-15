#!/usr/bin/env pwsh

function script:exec {
    [CmdletBinding()]

    param(
        [Parameter(Position = 0, Mandatory = 1)][scriptblock]$cmd,
        [Parameter(Position = 1, Mandatory = 0)][string]$errorMessage,

        [switch]$WhatIf = $false
    )
    if ($WhatIf) {
        $InformationPreference = 'Continue'
        Write-Information "Would execute `"$cmd`""
        return;
    }

    & $cmd
    if ($LASTEXITCODE -ne 0) {
        throw ("Error ($LASTEXITCODE) executing command: {0}" -f $cmd) + ($errorMessage ?? "")
    }
}

$short = exec { git describe }
$long =  exec { git describe --long }

Write-Output "Building docker file"
exec {
  docker build --build-arg version=$short --build-arg trimmed=true `
   --label version=$long `
   --tag qutecoacoustics/emu:latest --tag qutecoacoustics/emu:$long `
   .
}

Write-Output "Pushing dockerfile"
exec {
  docker push -a qutecoacoustics/emu
}
