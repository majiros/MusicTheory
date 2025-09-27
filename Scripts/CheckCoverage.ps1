param(
    [double]$Threshold = 70.0,
    [string]$SummaryPath = 'Tests/MusicTheory.Tests/TestResults/coverage-report/Summary.xml',
    [string]$CoberturaRoot = 'Tests/MusicTheory.Tests/TestResults'
)

$ErrorActionPreference = 'Stop'

function Get-LineCoverageFromSummary {
    param([string]$Path)
    if (Test-Path $Path) {
        try {
            [xml]$xml = Get-Content -Path $Path
            $line = $xml.Summary.Overall.Linecoverage
            if (-not $line) { $line = $xml.summary.overall.linecoverage }
            if ($line) { return [double]$line }
        } catch { }
    }
    return $null
}

function Get-LineCoverageFromCobertura {
    param([string]$Root)
    $cov = Get-ChildItem -Path $Root -Recurse -Filter 'coverage.cobertura.xml' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($cov) {
        try {
            [xml]$cxml = Get-Content -Path $cov.FullName
            $lr = $cxml.coverage.GetAttribute('line-rate')
            if ($lr) { return ([double]$lr * 100.0) }
            $covered = $cxml.coverage.GetAttribute('lines-covered')
            $valid = $cxml.coverage.GetAttribute('lines-valid')
            if ($covered -and $valid) {
                $coveredD = [double]$covered
                $validD = [double]$valid
                if ($validD -gt 0) { return ($coveredD / $validD) * 100.0 }
            }
        } catch { }
    }
    return $null
}

$pct = Get-LineCoverageFromSummary -Path $SummaryPath
if (-not $pct) { $pct = Get-LineCoverageFromCobertura -Root $CoberturaRoot }

if (-not $pct) {
    Write-Error "Unable to determine line coverage. Looked at '$SummaryPath' and under '$CoberturaRoot'."
    exit 1
}

Write-Host ("Line coverage: {0:N2}% (threshold: {1:N2}%)" -f $pct, $Threshold)
if ($pct -lt $Threshold) {
    Write-Error "Coverage below threshold."
    exit 1
}
Write-Host "Coverage OK"
