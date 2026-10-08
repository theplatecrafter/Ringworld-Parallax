param([string]$UnityEditor='C:\Program Files\Unity\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$taskRoot=$PSScriptRoot
$taskProject=Join-Path $taskRoot 'tools\VisualShaders'
$taskLog=Join-Path $taskRoot 'artifacts\visual-shader-build.log'
New-Item -ItemType Directory -Path (Split-Path -Parent $taskLog) -Force | Out-Null
$taskUnity=Start-Process -FilePath $UnityEditor -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+$taskProject+'"'),'-executeMethod','BuildVisuals.Run','-logFile',('"'+$taskLog+'"')) -WindowStyle Hidden -PassThru
$taskUnity.WaitForExit()
$taskText=Get-Content -LiteralPath $taskLog -Raw
if ($taskUnity.ExitCode -ne 0 -or $taskText -notmatch 'RINGWORLD PARALLAX BUNDLE BUILT' -or $taskText -match 'Shader error|error CS\d+') { throw "Visual bundle build failed. See $taskLog" }
Write-Host "Visual shader bundle compiled successfully. Log: $taskLog"
