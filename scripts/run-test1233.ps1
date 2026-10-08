$projectPath = (Resolve-Path (Join-Path $PSScriptRoot '..\test1233\test1233.csproj')).Path
$runningAppProcesses = Get-CimInstance Win32_Process | Where-Object {
    $_.Name -eq 'dotnet.exe' -and $_.CommandLine -like '*\test1233\*'
}

foreach ($runningAppProcess in $runningAppProcesses) {
    Stop-Process -Id $runningAppProcess.ProcessId -Force
}

Start-Job -ScriptBlock {
    for ($attempt = 0; $attempt -lt 20; $attempt++) {
        try {
            Invoke-WebRequest -Uri 'http://localhost:5228' -UseBasicParsing -TimeoutSec 1 | Out-Null
            Start-Process 'http://localhost:5228'
            break
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }
} | Out-Null

& dotnet run --project $projectPath --launch-profile http
