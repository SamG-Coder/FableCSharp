$ErrorActionPreference = "Stop"
Set-Location "C:\FableCSharp"
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) { throw "dotnet missing" }

# Prefer dotnet-script if present; else compile a one-off.
$script = "C:\FableCSharp\proofs\newgame-plus545\Dump.csx"
$out = "C:\FableCSharp\proofs\newgame-plus545\dump-out.txt"
try {
    & dotnet script $script | Tee-Object -FilePath $out
} catch {
    $exe = "C:\FableCSharp\tools\_frontend\bin\Debug\net10.0\Fable.FrontendDump.exe"
    if (Test-Path $exe) {
        & $exe UI_FRONTEND_BUTTON_NEW_GAME UI_ACCEPT_NEW_PROFILE UI_FRONTEND_BUTTON_INVISIBLE |
            Tee-Object -FilePath $out
    } else {
        throw $_
    }
}
