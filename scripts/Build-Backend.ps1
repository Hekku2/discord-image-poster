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
$publishFolder = 'bin/publish'

dotnet build --output $publishFolder $project --configuration Release

$fullSourcePath = (Resolve-Path "$publishFolder").Path

#Compress-Archive not used because it doesn't include hidden files (. -prefix)
Remove-Item -Path $ZipPath -ErrorAction SilentlyContinue
[System.IO.Compression.ZipFile]::CreateFromDirectory($fullSourcePath, $ZipPath) 
