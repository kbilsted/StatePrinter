if not exist .\nuget_packages mkdir nuget_packages
if not exist .\distro mkdir distro
xcopy StatePrinter\*.cs distro\src\ /Y /Q /E
xcopy StatePrinter\bin\Debug\*.dll  distro\lib\net35\ /Q
xcopy StatePrinter.nuspec           distro\ /Q
cd distro

..\.nuget\nuget.exe pack StatePrinter.nuspec -symbols -Prop Platform=AnyCPU

xcopy *.nupkg ..\nuget_packages\ /Y /Q

pause
cd ..
rmdir distro  /s /q
