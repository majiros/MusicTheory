param(
    [double]$Threshold = 75.0,
    [string]$SummaryPath = 'Tests/MusicTheory.Tests/TestResults/coverage-report/Summary.xml',
    [string]$CoberturaRoot = 'Tests/MusicTheory.Tests/TestResults'
)

$ErrorActionPreference = 'Stop'

function Get-LineCoverageFromSummary {
    param([string]$Path)
    if (Test-Path $Path) {
        try {
            [xml]$xml = Get-Content -Path $Path

            # ReportGenerator XmlSummary (new): <CoverageReport><Summary><Linecoverage>
            $line = $null
            if ($xml.CoverageReport -and $xml.CoverageReport.Summary -and $xml.CoverageReport.Summary.Linecoverage) {
                $line = $xml.CoverageReport.Summary.Linecoverage
            }

            # ReportGenerator XmlSummary (older Overall node)
            if (-not $line -and $xml.Summary -and $xml.Summary.Overall -and $xml.Summary.Overall.Linecoverage) {
                $line = $xml.Summary.Overall.Linecoverage
            }
            if (-not $line -and $xml.summary -and $xml.summary.overall -and $xml.summary.overall.linecoverage) {
                $line = $xml.summary.overall.linecoverage
            }

            # ReportGenerator XmlSummary (flat <Summary><Linecoverage>)
            if (-not $line -and $xml.Summary -and $xml.Summary.Linecoverage) {
                $line = $xml.Summary.Linecoverage
            }
            if (-not $line -and $xml.summary -and $xml.summary.linecoverage) {
                $line = $xml.summary.linecoverage
            }

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
