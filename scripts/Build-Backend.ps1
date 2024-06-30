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

Write-Host "Building backend and packing it to $ZipPath..."

dotnet build --output $publishFolder $project --configuration Release

$fullSourcePath = (Resolve-Path "$publishFolder").Path

# Remove local.settings.json
Remove-Item -Path "$fullSourcePath/local.settings.json" -ErrorAction SilentlyContinue

#Compress-Archive not used because it doesn't include hidden files (. -prefix)
Remove-Item -Path $ZipPath -ErrorAction SilentlyContinue
[System.IO.Compression.ZipFile]::CreateFromDirectory($fullSourcePath, $ZipPath) 
