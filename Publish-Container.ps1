<#
    .SYNOPSIS
    This script environment to azure

    .DESCRIPTION
    This assumes that dotnet sdk and az client are installed

    .PARAMETER SettinsFile
    Settings file that contains environment settings. Defaults to 'developer-settings.json'
#>
param(
    [Parameter()][string]$SettingsFile = 'developer-settings.json'
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Write-Host "Reading settings from file $SettingsFile"
$settingsJson = Get-Content -Raw -Path $SettingsFile | ConvertFrom-Json

$acrName = "cr$($settingsJson.ApplicationName)" -replace '-', ''
Write-Host "Logging in to Azure Container Registry $acrName..."

$version = Get-Date -Format "yyyy-MM-ddTHHmm"
$imageName = "$acrName.azurecr.io/discord-image-poster:$version"
docker build -t $imageName -f ./src/FunctionApp/Dockerfile ./src/FunctionApp/.

az acr login -n $acrName
docker push $imageName
