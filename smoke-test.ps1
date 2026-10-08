param([int]$TimeoutSeconds=600)
$ErrorActionPreference='Stop'
$taskBase=Join-Path (Split-Path -Parent $PSScriptRoot) 'Ring World KSP'
$taskGame=Join-Path (Split-Path -Parent $PSScriptRoot) 'template_instance'
$taskProcess=$null
$taskLog='RingworldParallaxSmoke-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.log'
try {
 & (Join-Path $taskBase 'build.ps1') -Install -SmokeTest
 & (Join-Path $PSScriptRoot 'build.ps1') -Install
 $taskProcess=Start-Process -FilePath (Join-Path $taskGame 'KSP_x64.exe') -WorkingDirectory $taskGame -ArgumentList @('-ringworld-smoketest','-ringworld-parallax-only','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-popupwindow','-logFile',$taskLog) -WindowStyle Hidden -PassThru
 $taskDeadline=(Get-Date).AddSeconds($TimeoutSeconds)
 while(-not $taskProcess.WaitForExit(1000)){if((Get-Date) -gt $taskDeadline){throw 'Parallax smoke timed out.'}}
 $taskText=Get-Content -LiteralPath (Join-Path $taskGame $taskLog) -Raw
 if($taskText -notmatch 'PARALLAX installed assets, GPU LOD output, map isolation and disable fallback passed' -or $taskText -match '\[RingworldSmoke\] FAIL'){throw "Parallax smoke failed: $taskLog"}
 $taskValidation=Join-Path $PSScriptRoot 'artifacts/validation';New-Item -ItemType Directory -Path $taskValidation -Force | Out-Null
 $taskText -split '\r?\n' | Where-Object {$_ -match '\[RingworldSmoke\]|\[RingworldParallax\]'} | Set-Content -LiteralPath (Join-Path $taskValidation 'installed-parallax-smoke.txt')
 Write-Host "PASS: $taskLog"
} finally {
 if($taskProcess -and -not $taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}
 & (Join-Path $taskBase 'build.ps1') -Install
}
