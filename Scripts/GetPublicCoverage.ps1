param(
  [string]$Url = "https://majiros.github.io/MusicTheory/coverage/Summary.xml",
  [int]$Retries = 5,
  [int]$DelaySeconds = 5
)

$ErrorActionPreference = 'Stop'

function Get-Summary {
  param([string]$Url)
  # Cache-busting to avoid CDN caches (use UriBuilder for safety)
  $ts = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
  try {
    $builder = [System.UriBuilder]::new($Url)
  }
  catch {
    throw "Invalid Url: '$Url' — $($_.Exception.Message)"
  }
  $q = $builder.Query.TrimStart('?')
  if ([string]::IsNullOrWhiteSpace($q)) { $builder.Query = "v=$ts" }
  else { $builder.Query = "$q&v=$ts" }
  $u = $builder.Uri.AbsoluteUri
  Write-Host "Fetching: $u" -ForegroundColor DarkCyan
  $resp = Invoke-WebRequest -Uri ([Uri]$u) -UseBasicParsing -Headers @{ 'Cache-Control'='no-cache'; 'Pragma'='no-cache' }
  if ($resp.StatusCode -ge 400) { throw "HTTP $($resp.StatusCode)" }
  [xml]$xml = $resp.Content
  $sum = $xml.CoverageReport.Summary
  if (-not $sum) { throw 'No <CoverageReport><Summary> node' }
  $line = [double]$sum.Linecoverage
  $branch = [double]$sum.Branchcoverage
  $method = [double]$sum.Methodcoverage
  $gen = [string]$sum.Generatedon
  return [pscustomobject]@{
    Line        = [math]::Round($line, 1)
    Branch      = [math]::Round($branch, 1)
    Method      = [math]::Round($method, 1)
    GeneratedOn = $gen
  }
}

$attempt = 0
while ($attempt -lt $Retries) {
  try {
    $summary = Get-Summary -Url $Url
    Write-Host ("Public coverage: Line {0}% • Branch {1}% • Method {2}% (Generated on: {3} UTC)" -f $summary.Line, $summary.Branch, $summary.Method, $summary.GeneratedOn)
    exit 0
  }
  catch {
    $attempt++
    if ($attempt -ge $Retries) {
      Write-Error "Failed to fetch/parse public coverage after $Retries attempts. Error: $($_.Exception.Message)"
      exit 1
    }
    Start-Sleep -Seconds $DelaySeconds
  }
}
