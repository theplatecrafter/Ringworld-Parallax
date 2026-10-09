param([string]$Run='1',[switch]$SkipBuild)
$ErrorActionPreference='Stop'
$photoBridgeRoot=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$photoBaseRoot=(Resolve-Path (Join-Path $photoBridgeRoot '../Ring World KSP')).Path
$photoGameRoot=(Resolve-Path (Join-Path $photoBridgeRoot '../template_instance')).Path
if(Get-Process -Name KSP_x64 -ErrorAction SilentlyContinue){throw 'Close KSP before running the photo regression.'}
$photoValidation=Join-Path $photoBridgeRoot 'artifacts/validation'
New-Item -ItemType Directory -Path $photoValidation -Force | Out-Null
$photoSettingsBackup=Join-Path $photoValidation ('photo-settings-'+[Guid]::NewGuid().ToString('N')+'.cfg')
Copy-Item -LiteralPath (Join-Path $photoGameRoot 'settings.cfg') -Destination $photoSettingsBackup
$photoProcess=$null
try{
 if(-not $SkipBuild){
  & (Join-Path $photoBaseRoot 'build.ps1') -Install -SmokeTest
  if($LASTEXITCODE -ne 0){throw 'Photo base build failed.'}
  & (Join-Path $photoBridgeRoot 'build.ps1') -Install -SmokeTest
  if($LASTEXITCODE -ne 0){throw 'Photo bridge build failed.'}
 }
 $photoArguments=@('-ringworld-smoketest','-ringworld-parallax-only','-ringworld-parallax-photo','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-popupwindow','-logFile',"ParallaxPhoto-$Run.log")
 $photoProcess=Start-Process -FilePath (Join-Path $photoGameRoot 'KSP_x64.exe') -WorkingDirectory $photoGameRoot -WindowStyle Hidden -ArgumentList $photoArguments -PassThru
 Write-Output "Photo regression PID $($photoProcess.Id)"
 $photoDeadline=[DateTime]::UtcNow.AddMinutes(15)
 while(-not $photoProcess.HasExited){
  if([DateTime]::UtcNow -gt $photoDeadline){Stop-Process -Id $photoProcess.Id;throw 'Photo regression timed out.'}
  Start-Sleep -Seconds 2;$photoProcess.Refresh()
 }
 $photoLog=Get-Content -LiteralPath (Join-Path $photoGameRoot "ParallaxPhoto-$Run.log")
 $photoLog | Select-String '\[RingworldSmoke\] (PASS|FAIL)' | ForEach-Object {$_.Line}
 if(($photoLog | Select-String '\[RingworldSmoke\] FAIL') -or -not($photoLog | Select-String '\[RingworldSmoke\] PASS Parallax photo resume/cancel')){throw 'Photo regression did not pass its completion marker.'}
}finally{
 if($photoProcess -and -not $photoProcess.HasExited){Stop-Process -Id $photoProcess.Id}
 & (Join-Path $photoBaseRoot 'build.ps1') -Install
 & (Join-Path $photoBridgeRoot 'build.ps1') -Install
 Copy-Item -LiteralPath $photoSettingsBackup -Destination (Join-Path $photoGameRoot 'settings.cfg') -Force
 Write-Output 'Normal development base and original settings restored.'
}
