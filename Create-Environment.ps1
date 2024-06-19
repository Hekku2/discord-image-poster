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

.$PSScriptRoot/scripts/FunctionUtil.ps1

Write-Host "Reading settings from file $SettingsFile"
$settingsJson = Get-Content -Raw -Path $SettingsFile | ConvertFrom-Json

Write-Host 'Checking if there is an existing installation...'
$webSitePackageLocation = Get-AzWebApp -ResourceGroupName $settingsJson.ResourceGroup -Name "func-$($settingsJson.ApplicationName)" -ErrorAction SilentlyContinue | Get-WebSitePackage
if ($webSitePackageLocation) {
    Write-Host "Function app already exist with website package, using it..."
}
else {
    $webSitePackageLocation = ''
}

Write-Host 'Creating resource group if it doesn''t exist...'
New-AzResourceGroup -Name $settingsJson.ResourceGroup -Location $settingsJson.Location -Force

Write-Host 'Deploying template...'
$parameters = @{
    baseName               = $settingsJson.ApplicationName
    webSitePackageLocation = $webSitePackageLocation
    discordSettings        = @{
        token     = $settingsJson.DiscordToken
        channelId = $settingsJson.DiscordChannelId
        guildId   = $settingsJson.DiscordGuildId
    }
}
New-AzResourceGroupDeployment `
    -ResourceGroupName $settingsJson.ResourceGroup `
    -TemplateFile 'infra/main.bicep' `
    -TemplateParameterObject $parameters
