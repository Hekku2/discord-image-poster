<#
    .SYNOPSIS
        Helper script for testing trigger interactions.
#>
param(
    [Parameter()][string]$JsonFile
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$content = Get-Content -Raw -Path $JsonFile

Invoke-RestMethod -Method Post -Uri 'http://localhost:8080/api/HandleDiscordWebHook?code=mock-secret-for-local-testing' -Body $content -ContentType 'application/json'
