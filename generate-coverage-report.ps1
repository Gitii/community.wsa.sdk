Remove-Item -Force -Recurse .\CoverageReport -ErrorAction SilentlyContinue

$Coverage = Get-ChildItem -Path .\CoverageResults -Filter "*net6*"
$fn = $Coverage.FullName

reportgenerator -reports:"$fn" -targetdir:CoverageReport -reporttypes:"Html;HtmlSummary"

Remove-Item -Force -Recurse .\CoverageResults -ErrorAction SilentlyContinue
