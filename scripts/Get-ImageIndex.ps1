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

.$PSScriptRoot/FunctionUtil.ps1

$settingsJson = Get-DeveloperSettings -SettingsFile $SettingsFile
$appName = "func-$($settingsJson.ResourceGroup)"

$webApp = Get-AzWebApp -ResourceGroupName $settingsJson.ResourceGroup -Name $appName
$url = Get-FunctionBaseUrl -FunctionApp $webApp
$code = Get-FunctionCode -FunctionApp $webApp

$functionUrl = "$($url)GetImageIndex?code=$code"
Invoke-RestMethod -Method Get -Uri $functionUrl -ContentType 'application/json'
