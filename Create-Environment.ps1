<#
    .SYNOPSIS
    This script environment to azure

    .DESCRIPTION
    This assumes that dotnet sdk and az client are installed

    .PARAMETER SettinsFile
    Settings file that contains environment settings. Defaults to 'developer-settings.json'

    .PARAMETER NoDiscord
    If set, discord sending is disabled. Useful for CI/CD and testing.
#>
param(
    [Parameter()][string]$SettingsFile = 'developer-settings.json',
    [Parameter()][switch]$NoDiscord,
    [Parameter()][switch]$DeleteOldRoleAssingments
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

.$PSScriptRoot/scripts/FunctionUtil.ps1

Write-Host "Reading settings from file $SettingsFile"
$settingsJson = Get-Content -Raw -Path $SettingsFile | ConvertFrom-Json

Write-Host 'Checking if there is an existing installation...'
$webSitePackageLocation = Get-AzWebApp -ResourceGroupName $settingsJson.ResourceGroup -Name "func-$($settingsJson.ApplicationName)" -ErrorAction SilentlyContinue | Get-WebSitePackage -ErrorAction SilentlyContinue
if ($webSitePackageLocation) {
    Write-Host "Function app already exist with website package, using it..."
}
else {
    $webSitePackageLocation = ''
}

Write-Host 'Creating resource group if it doesn''t exist...'
New-AzResourceGroup -Name $settingsJson.ResourceGroup -Location $settingsJson.Location -Force

if ($DeleteOldRoleAssingments -and -not [string]::IsNullOrEmpty($settingsJson.ExistingCognitiveServicesAccountName)) {
    Write-Host 'Removing unknown ''Cognitive Services User'' role assignments...'
    
    $rg = $settingsJson.ExistingCognitiveServicesResourceGroup ?? $settingsJson.ResourceGroup
    Remove-UnknownRoleAssingments -ResourceName $settingsJson.ExistingCognitiveServicesAccountName -ResourceGroupName $rg
}

Write-Host 'Deploying template...'
$parameters = @{
    baseName               = $settingsJson.ApplicationName
    webSitePackageLocation = $webSitePackageLocation
    disableDiscordSending  = $NoDiscord ? $true : $false
    discordSettings        = @{
        token     = $settingsJson.DiscordToken
        channelId = $settingsJson.DiscordChannelId
        guildId   = $settingsJson.DiscordGuildId
        publicKey = $settingsJson.DiscordPublicKey
    }
    cognitiveService       = @{
        existingServiceName          = $settingsJson.ExistingCognitiveServicesAccountName
        existingServiceResourceGroup = $settingsJson.ExistingCognitiveServicesResourceGroup
    }
}
New-AzResourceGroupDeployment `
    -ResourceGroupName $settingsJson.ResourceGroup `
    -TemplateFile 'infra/main.bicep' `
    -TemplateParameterObject $parameters
