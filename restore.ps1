Set-Location 'C:\Users\AMD\source\repos\MediCore'
if (-Not (Test-Path .\nuget.exe)) { Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile .\nuget.exe }
.\nuget.exe locals all -clear
.\nuget.exe restore .\MediCore.slnx

