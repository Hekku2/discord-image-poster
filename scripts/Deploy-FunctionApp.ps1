<#
    .SYNOPSIS
    Deploys zip while to function app with WEBSITE_RUN_FROM_PACKAGE method
#>
param(
    [Parameter(Mandatory)][string]$ResourceGroup,
    [Parameter()][string]$AppName = "func-$ResourceGroup",
    [Parameter(Mandatory)][string]$ZipFile
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

.$PSScriptRoot/FunctionUtil.ps1

$webApp = Get-AzWebApp -ResourceGroupName $ResourceGroup -Name $AppName
$config = $webApp.SiteConfig.AppSettings
$accountName = Get-FunctionAppStorageAccountName -WebApp $webApp

$storageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroup -Name $accountName
$context = $storageAccount.Context

try {
    Get-AzStorageContainer -Name 'function-releases' -Context $context | Out-Null
}
catch {
    New-AzStorageContainer -Name 'function-releases' -Context $context -Permission Off | Out-Null
}

$blobName = $(get-date -f yyyyMMddHHmmss) + '-' + [guid]::NewGuid().ToString() + '.zip'

$blobParameters = @{
    File             = $ZipFile
    Container        = 'function-releases'
    Blob             = $blobName
    Context          = $context
    StandardBlobTier = 'Hot'
}
Set-AzStorageBlobContent @blobParameters | Out-Null

$newAppSettings = @{}
ForEach ($item in $config) {
    $newAppSettings[$item.Name] = $item.Value
}

$newAppSettings['WEBSITE_RUN_FROM_PACKAGE'] = "https://$accountName.blob.core.windows.net/function-releases/$blobName"

Set-AzWebApp -ResourceGroupName $ResourceGroup -Name $AppName -AppSettings $newAppSettings | Out-Null
#func azure functionapp publish func-hjni-discord-image-poster
