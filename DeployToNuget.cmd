REM ON FIRST RUN, RUN THIS (change the key to whatever is found on your profile on www.nuget.org ->   
REM .nuget\NuGet.exe setapikey e39ea-get-the-full-key-on-nuget.org

CreateNuget.cmd
.nuget\NuGet.exe push nuget_packages\StatePrinter.1.0.?.nupkg

pause 