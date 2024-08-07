<#
    .SYNOPSIS
    This script generates envvironmetn variable files based on developer-settings.json

    .DESCRIPTION
    Purpose of this is to make starting the development more convenient by generating environment files.

    .PARAMETER SettinsFile
    Settings file that contains environment settings. Defaults to 'developer-settings.json'
#>
param(
    [Parameter()][string]$SettingsFile = 'developer-settings.json'
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

.$PSScriptRoot/scripts/FunctionUtil.ps1

Write-Host "Reading settings from file $SettingsFile"
$settingsJson = Get-Content -Raw -Path $SettingsFile | ConvertFrom-Json

$cognitiveServicesEndpoint = ''
if (-not [string]::IsNullOrEmpty($settingsJson.ExistingCognitiveServicesAccountName) -and -not [string]::IsNullOrEmpty($settingsJson.ExistingCognitiveServicesResourceGroup)) {
    Write-Host 'Retrieving cognitive services endpoint from existing account'
    $service = Get-AzCognitiveServicesAccount -ResourceGroupName hjni-discord-image-poster -AccountName aisa-hjni-discord-image-poster
    $cognitiveServicesEndpoint = $service.Endpoint
}

# Docker compose support
$dockerEnvFile = "$PSScriptRoot/.env"
$dockerEnvFileContent = "
# This file was generated by Create-Settings.ps1. Don't commit this to version control.
DISCORD_TOKEN=$($settingsJson.DiscordToken)
DISCORD_GUILDID=$($settingsJson.DiscordGuildId)
DISCORD_CHANNELID=$($settingsJson.DiscordChannelId)
DISCORD_PUBLICKEY=$($settingsJson.DiscordPublicKey)
COGNITIVESERVICES_ENDPOINT=$cognitiveServicesEndpoint
"
Write-Host "Writing Docker ENV $dockerEnvFile"
$dockerEnvFileContent | Out-File -FilePath $dockerEnvFile -Encoding utf8

# Function Core Tools support
$funcSettingsFile = "$PSScriptRoot/src/FunctionApp.Isolated/local.settings.json"
Write-Host "Writing Function Core Toole support $funcSettingsFile"
$localSettings = @{
    IsEncrypted = $false
    Values      = @{
        AzureWebJobsStorage                             = 'UseDevelopmentStorage=true'
        FUNCTIONS_WORKER_RUNTIME                        = 'dotnet-isolated'
        DiscordConfiguration__Token                     = $settingsJson.DiscordToken
        DiscordConfiguration__GuildId                   = $settingsJson.DiscordGuildId
        DiscordConfiguration__ChannelId                 = $settingsJson.DiscordChannelId
        DiscordConfiguration__PublicKey                 = $settingsJson.DiscordPublicKey
        ImageAnalysisConfiguration__Endpoint            = $cognitiveServicesEndpoint
        BlobStorageImageSourceOptions__ConnectionString = 'UseDevelopmentStorage=true'
        BlobStorageImageSourceOptions__ContainerName    = 'images'
        BlobStorageImageSourceOptions__FolderPath       = 'root'
        ImageIndexOptions__ConnectionString             = 'UseDevelopmentStorage=true'
        ImageIndexOptions__ContainerName                = 'images'
        ImageIndexOptions__IndexFileName                = 'index.json'
    }
}

$localSettings | ConvertTo-Json | Out-File -FilePath $funcSettingsFile -Encoding utf8

Write-Host "Writing user-secrets for console tester."

# NOTE: For some reason __ didn't work in the key names, so I had to use : instead.
dotnet user-secrets --project src/ConsoleTester set "DiscordConfiguration:Token" "$($settingsJson.DiscordToken)"
dotnet user-secrets --project src/ConsoleTester set "DiscordConfiguration:GuildId" "$($settingsJson.DiscordGuildId)"
dotnet user-secrets --project src/ConsoleTester set "DiscordConfiguration:ChannelId" "$($settingsJson.DiscordChannelId)"
dotnet user-secrets --project src/ConsoleTester set "DiscordConfiguration:PublicKey" "$($settingsJson.DiscordPublicKey)"
dotnet user-secrets --project src/ConsoleTester set "ImageAnalysisConfiguration:Endpoint" "$($cognitiveServicesEndpoint)"
