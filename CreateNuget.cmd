set stateprinter_version=1.0.5
if not exist .\nuget_packages mkdir nuget_packages
del /Q .\nuget_packages\*.*
.nuget\NuGet.exe pack StatePrinter\StatePrinter.csproj -OutputDirectory .\nuget_packages -Version %stateprinter_version% -symbols -Prop Platform=AnyCPU
pause 