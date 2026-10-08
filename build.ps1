param([string]$KspRoot=(Join-Path (Split-Path -Parent $PSScriptRoot) 'template_instance'),[string]$RingworldRoot=(Join-Path (Split-Path -Parent $PSScriptRoot) 'Ring World KSP'),[switch]$Install,[switch]$Package)
$ErrorActionPreference='Stop'
& dotnet run --project (Join-Path $PSScriptRoot 'tests/Ringworld.Parallax.Tests.csproj') -c Release
if($LASTEXITCODE -ne 0){throw 'Placement tests failed.'}
& dotnet build (Join-Path $PSScriptRoot 'src/Ringworld.Parallax.csproj') -c Release --nologo "-p:KspRoot=$KspRoot" "-p:RingworldRoot=$RingworldRoot"
if($LASTEXITCODE -ne 0){throw 'Bridge build failed; build the matching base branch first.'}
$taskStage=Join-Path $PSScriptRoot ('artifacts/stage-'+[Guid]::NewGuid().ToString('N'))
$taskMod=Join-Path $taskStage 'GameData/RingworldParallax'
New-Item -ItemType Directory -Path $taskMod -Force | Out-Null
Copy-Item -Path (Join-Path $PSScriptRoot 'GameData/RingworldParallax/*') -Destination $taskMod -Recurse -Force
New-Item -ItemType Directory -Path (Join-Path $taskMod 'Plugins') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'src/bin/Release/net48/Ringworld.Parallax.dll') -Destination (Join-Path $taskMod 'Plugins') -Force
if(-not(Test-Path -LiteralPath (Join-Path $taskMod 'Assets/ringworldparallax'))){throw 'Build the original bridge evaluator with build-visuals.ps1 first.'}
if($Install){$taskInstall=Join-Path $KspRoot 'GameData/RingworldParallax';New-Item -ItemType Directory -Path $taskInstall -Force | Out-Null;Copy-Item -Path (Join-Path $taskMod '*') -Destination $taskInstall -Recurse -Force}
if($Package){
 $taskDocs=Join-Path $taskMod 'Documentation';New-Item -ItemType Directory -Path $taskDocs -Force | Out-Null
 foreach($doc in @('README.md','RELEASE-NOTES.md','LICENSE','CREDITS.md','THIRD-PARTY-NOTICES.md','docs')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $doc) -Destination $taskDocs -Recurse -Force}
 $taskVersion=([xml](Get-Content -LiteralPath (Join-Path $PSScriptRoot 'src/Ringworld.Parallax.csproj') -Raw)).Project.PropertyGroup.Version
 $taskZip=Join-Path $PSScriptRoot ("artifacts/RingworldParallax-$taskVersion.zip")
 $taskUnexpected=Get-ChildItem -LiteralPath (Join-Path $taskStage 'GameData') | Where-Object Name -ne 'RingworldParallax'
 if($taskUnexpected){throw 'Unexpected bundled dependency in stage.'}
 Compress-Archive -Path (Join-Path $taskStage '*') -DestinationPath $taskZip -Force
 ((Get-FileHash -LiteralPath $taskZip -Algorithm SHA256).Hash.ToLowerInvariant()+'  '+[IO.Path]::GetFileName($taskZip)) | Set-Content -LiteralPath ($taskZip+'.sha256')
}
Write-Host "Bridge staged: $taskMod"
