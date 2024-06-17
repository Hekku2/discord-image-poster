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

$functionAppName = "func-$($settingsJson.ApplicationName)"

$acrName = "cr$($settingsJson.ApplicationName)" -replace '-', ''
$latest = (az acr repository show-tags --name $acrName --repository discord-image-poster --orderby time_desc --top 1 | ConvertFrom-Json)
Write-host "Latest container is $latest"

$credentials = (az acr credential show --name $acrName | ConvertFrom-Json)

$imageName = "$acrName.azurecr.io/discord-image-poster:$latest"
Write-Host "Updating container app environment with image $imageName..."

az functionapp config container set `
    --registry-server "$acrName.azurecr.io" `
    --registry-username $credentials.username `
    --registry-password $credentials.passwords[0].value `
    --image $imageName `
    --name $functionAppName `
    --resource-group $settingsJson.ResourceGroup
    
