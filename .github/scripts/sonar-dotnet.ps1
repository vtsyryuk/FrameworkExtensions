param([switch]$Analyze)
$ErrorActionPreference = 'Stop'
function Invoke-Checked([scriptblock]$Command) {
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "Command failed with exit code $LASTEXITCODE" }
}
New-Item -ItemType Directory -Force build | Out-Null
Invoke-Checked { nuget restore System.FrameworkExtensions.sln -NonInteractive -Source https://api.nuget.org/v3/index.json }
Invoke-Checked { nuget install NUnit.Runners -Version 2.6.4 -OutputDirectory build/tools -NonInteractive -Source https://api.nuget.org/v3/index.json }
Invoke-Checked { nuget install OpenCover -Version 4.7.1221 -OutputDirectory build/tools -NonInteractive -Source https://api.nuget.org/v3/index.json }
if ($Analyze) {
    Invoke-Checked { dotnet tool install --global dotnet-sonarscanner --version 12.1.0 }
    Invoke-Checked { dotnet sonarscanner begin /k:"$env:SONAR_PROJECT_KEY" /o:"$env:SONAR_ORGANIZATION" /d:sonar.token="$env:SONAR_TOKEN" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.cs.opencover.reportsPaths="build/coverage.xml" /d:sonar.qualitygate.wait=true /d:sonar.qualitygate.timeout=300 }
}
# Use the installed reference pack for CI without changing the library's project files.
Invoke-Checked { msbuild System.FrameworkExtensions.sln /t:Rebuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.8 /p:RestorePackages=false /nodeReuse:false }
$runner = (Resolve-Path 'build/tools/NUnit.Runners.2.6.4/tools/nunit-console.exe').Path
$tests = 'System.FrameworkExtensions.Tests/bin/Release/System.FrameworkExtensions.Tests.dll'
Invoke-Checked { & build/tools/OpenCover.4.7.1221/tools/OpenCover.Console.exe -register:user "-target:$runner" "-targetargs:$tests /noshadow /framework:net-4.0 /xml:build/test-results.xml" '-filter:+[System.FrameworkExtensions]*' '-output:build/coverage.xml' -returntargetcode }
if ($Analyze) {
    Invoke-Checked { dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN" }
}
