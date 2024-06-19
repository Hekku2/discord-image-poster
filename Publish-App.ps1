<#
    .SYNOPSIS
    This script builds and publishes measurement listener to Azure

    .DESCRIPTION
    This assumes that dotnet sdk and Azure Powershell are installed

    .PARAMETER SettinsFile
    Settings file that contains environment settings. Defaults to 'developer-settings.json'
#>
param(
    [Parameter()][string]$SettingsFile = 'developer-settings.json'
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

.$PSScriptRoot/scripts/FunctionUtil.ps1

$settingsJson = Get-DeveloperSettings -SettingsFile $SettingsFile

Write-Host 'Deploying function app...'
$archivePath = Join-Path -Path (Resolve-Path ".\").Path -ChildPath 'publish.zip'
.$PSScriptRoot/scripts/Build-Backend.ps1 -ZipPath $archivePath
.$PSScriptRoot/scripts/Deploy-FunctionApp.ps1 -ResourceGroup $settingsJson.ResourceGroup -AppName "func-$($settingsJson.ResourceGroup)" -ZipFile $archivePath
