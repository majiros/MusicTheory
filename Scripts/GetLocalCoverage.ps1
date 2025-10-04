param(
    [string]$SummaryPath = "Tests/MusicTheory.Tests/TestResults/coverage-report/Summary.xml"
)

$ErrorActionPreference = 'Stop'

function Get-PercentAttr {
    param(
        [xml]$Xml,
        [string]$AttrName
    )
    $node = Select-Xml -Xml $Xml -XPath "//*[@$AttrName]" | Select-Object -First 1
    if ($node -and $node.Node -and $node.Node.Attributes[$AttrName]) {
        $raw = $node.Node.Attributes[$AttrName].Value
        if ([string]::IsNullOrWhiteSpace($raw)) { return $null }
        try { return [double]::Parse($raw, [System.Globalization.CultureInfo]::InvariantCulture) } catch { return $null }
    }
    return $null
}

function Get-AttrValue {
    param(
        [xml]$Xml,
        [string]$AttrName
    )
    $node = Select-Xml -Xml $Xml -XPath "//*[@$AttrName]" | Select-Object -First 1
    if ($node -and $node.Node -and $node.Node.Attributes[$AttrName]) {
        return $node.Node.Attributes[$AttrName].Value
    }
    return $null
}

if (-not (Test-Path -LiteralPath $SummaryPath)) {
    Write-Error "Summary.xml not found: $SummaryPath"
    exit 1
}

[xml]$xml = Get-Content -LiteralPath $SummaryPath -Raw

$line   = Get-PercentAttr -Xml $xml -AttrName 'linecoverage'
$branch = Get-PercentAttr -Xml $xml -AttrName 'branchcoverage'
$method = Get-PercentAttr -Xml $xml -AttrName 'methodcoverage'
$genOn  = Get-AttrValue   -Xml $xml -AttrName 'generatedon'

# If values are in 0..1, convert to percentage
if ($line -le 1)   { $line   = $line   * 100 }
if ($branch -le 1) { $branch = $branch * 100 }
if ($method -le 1) { $method = $method * 100 }

# Round to 2 decimals
if ($null -ne $line)   { $line   = [Math]::Round($line, 2) }
if ($null -ne $branch) { $branch = [Math]::Round($branch, 2) }
if ($null -ne $method) { $method = [Math]::Round($method, 2) }

# Simple output
$parts = @()
if ($null -ne $line)   { $parts += "Line $line%" }
if ($null -ne $branch) { $parts += "Branch $branch%" }
if ($null -ne $method) { $parts += "Method $method%" }
$metrics = ($parts -join ' • ')

if ($metrics) {
    if ($genOn) {
        Write-Host "Local coverage: $metrics (Generated on: $genOn)"
    }
    else {
        Write-Host "Local coverage: $metrics"
    }
}
else {
    Write-Warning "Could not parse coverage metrics from: $SummaryPath"
    exit 2
}
