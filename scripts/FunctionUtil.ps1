<#
Functions for easier usage and cleaner code
#>

function Get-DeveloperSettings() {
    param(
        [Parameter(Mandatory)][string]$SettingsFile
    )

    Write-Host "Reading settings from file $SettingsFile"
    $settingsJson = Get-Content -Raw -Path $SettingsFile | ConvertFrom-Json
    
    $tagsHashtable = @{ }
    if ($settingsJson.Tags) {
        $settingsJson.Tags.psobject.properties | ForEach-Object { $tagsHashtable[$_.Name] = $_.Value }
    }

    return $settingsJson
}

function Get-BlobServiceUriForConnection() {
    param(
        [Parameter(Mandatory)][Microsoft.Azure.Commands.WebApps.Models.PSSite]$WebApp,
        [Parameter(Mandatory)][string]$Connection
    )

    return $WebApp.SiteConfig.AppSettings | `
        Where-Object { $_.Name -Like "AzureWebJobs$($Connection)__blobServiceUri" } | `
        Select-Object -First 1 | `
        ForEach-Object -Process { $_.Value }
}

function Get-WebSitePackage() {
    param(
        [Parameter(ValueFromPipeline = $true, Position = 0, Mandatory)][Microsoft.Azure.Commands.WebApps.Models.PSSite]$WebApp
    )

    return $WebApp.SiteConfig.AppSettings | `
        Where-Object { $_.Name -Like 'WEBSITE_RUN_FROM_PACKAGE' } | `
        Select-Object -First 1 | `
        ForEach-Object -Process { $_.Value }
}

function Get-FunctionAppStorageAccountName() {
    param(
        [Parameter(Mandatory)][Microsoft.Azure.Commands.WebApps.Models.PSSite]$WebApp
    )
    $accountName = $WebApp.SiteConfig.AppSettings | `
        Where-Object { $_.Name -Like 'AzureWebJobsStorage__accountName' } | `
        Select-Object -First 1 | `
        ForEach-Object -Process { $_.Value }

    if ($accountName) {
        return $accountName
    }

    $accountName = $WebApp.SiteConfig.AppSettings | `
        Where-Object { $_.Name -Like 'AzureWebJobsStorage' } | `
        Select-Object -First 1 | `
        ForEach-Object -Process { $_.Value.Split(';') } | `
        Where-Object { $_ -Like 'AccountName=*' } | `
        ForEach-Object -Process { $_.Split('=') } | `
        Select-Object -Last 1

    return $accountName
}

function Get-FunctionCode() {
    param(
        [Parameter(Mandatory)][Microsoft.Azure.Commands.WebApps.Models.PSSite]$FunctionApp
    )
    $path = "$($FunctionApp.Id)/host/default/listkeys?api-version=2021-02-01"

    $result = Invoke-AzRestMethod -Path $path -Method POST
    if ($result -and $result.StatusCode -eq 200) {
        $contentBody = $result.Content | ConvertFrom-Json

        return $contentBody.functionKeys.default
    }
    return ''
}

function Get-FunctionBaseUrl() {
    param(
        [Parameter(Mandatory)][Microsoft.Azure.Commands.WebApps.Models.PSSite]$FunctionApp
    )
    return "https://$($FunctionApp.HostNames[0])/api/"
}
