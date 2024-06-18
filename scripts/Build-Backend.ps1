<#
    .SYNOPSIS
    This script builds backend and packages it
#>
param(
    [Parameter(Mandatory)][string]$ZipPath
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$project = 'src/FunctionApp.Isolated'
$publishFolder = 'src/output'

dotnet publish -o $publishFolder $project --version-suffix 'DEV'

$fullSourcePath = (Resolve-Path "$publishFolder").Path

Compress-Archive -DestinationPath $ZipPath -Path "$fullSourcePath/*" -Force
