param(
    [string]$Owner,
    [string]$Repo,
    [string]$Url
)

# If explicit URL not provided, compose from Owner/Repo
if (-not $Url -or [string]::IsNullOrWhiteSpace($Url)) {
    if (-not $Owner -or -not $Repo) {
        Write-Host "Usage: -Owner <OWNER> -Repo <REPO> or -Url <https://owner.github.io/repo/badge_linecoverage.svg>" -ForegroundColor Yellow
        exit 1
    }
    $Url = "https://$Owner.github.io/$Repo/badge_linecoverage.svg"
}

$readmePath = Join-Path $PSScriptRoot '..\README.md'
if (-not (Test-Path $readmePath)) {
    Write-Error "README.md not found at '$readmePath'"
    exit 1
}

$content = Get-Content -LiteralPath $readmePath -Raw

# Prefer replacing the known local badge path; fallback to first generic coverage image
$localPathPattern = 'Tests/MusicTheory\.Tests/TestResults/coverage-report/badge_linecoverage\.svg'
if ($content -match $localPathPattern) {
    $newContent = [regex]::Replace($content, $localPathPattern, [System.Text.RegularExpressions.Regex]::Escape($Url), 1)
    # Note: Using Escape in replacement ensures any special chars are treated literally in regex replacement
} else {
    $pattern = '!\[coverage\]\(([^)]+)\)'
    $m = [regex]::Match($content, $pattern)
    if ($m.Success) {
        $prefix = $content.Substring(0, $m.Index)
        $suffix = $content.Substring($m.Index + $m.Length)
        $replacement = "![coverage]($Url)"
        $newContent = $prefix + $replacement + $suffix
    } else {
        Write-Error "No coverage badge image found in README.md"
        exit 1
    }
}

Set-Content -LiteralPath $readmePath -Value $newContent -Encoding UTF8
Write-Host "Updated README coverage badge to: $Url" -ForegroundColor Green
