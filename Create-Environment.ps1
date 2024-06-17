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

Write-Host 'Creating resource group if it doesn''t exist...'
New-AzResourceGroup -Name $settingsJson.ResourceGroup -Location $settingsJson.Location -Force

Write-Host 'Deploying template...'
$parameters = @{
    baseName = $settingsJson.ApplicationName
}
$result = New-AzResourceGroupDeployment `
    -ResourceGroupName $settingsJson.ResourceGroup `
    -TemplateFile 'infra/main.bicep' `
    -TemplateParameterObject $parameters `
    -Verbose

$storageAccountName = $result.Outputs.storageAccountName.Value
$applicationInsightsName = $result.Outputs.applicationInsightsName.Value
$functionAppIdentity = $result.Outputs.functionAppIdentity.Value
$containerAppEnvironmentName = "cae-$($settingsJson.ApplicationName)"
$functionAppName = "func-$($settingsJson.ApplicationName)"

Write-Host "Creating environment $containerAppEnvironmentName..."
az containerapp env create `
    --name $containerAppEnvironmentName `
    --enable-workload-profiles `
    --resource-group $settingsJson.ResourceGroup `
    --location $settingsJson.Location `
    --logs-destination none

Write-Host "Creating function app $functionAppName with storage $storageAccountName..."
az functionapp create `
    --name $functionAppName `
    --storage-account $storageAccountName `
    --environment $containerAppEnvironmentName `
    --workload-profile-name "Consumption" `
    --resource-group $settingsJson.ResourceGroup `
    --functions-version 4 `
    --runtime dotnet-isolated `
    --image mcr.microsoft.com/azure-functions/dotnet8-quickstart-demo:1.0 `
    --app-insights $applicationInsightsName `
    --assign-identity $functionAppIdentity `
    --debug
