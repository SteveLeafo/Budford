@echo off
set /p Build=<ver.txt
set /a Build=%Build%+1
echo %Build%>ver.txt

fart ..\Properties\AssemblyInfo.cs 1.0.0.0  1.0.%Build%
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\DEVENV"   ..\..\Budford.sln /Rebuild Release
git commit -m "New release" ver.txt
git status
git push origin master
git checkout ..\..
call "c:\Program Files (x86)\NSIS\Bin\makensis.exe" Budford.nsi
rem call "c:\Program Files (x86)\NSIS\Bin\makensis.exe" "Budford - Full.nsi"

ren "Budford - Setup.exe" "Budford - Setup - V1.0.%Build%.exe" 
rem ren "Budford - Full Setup.exe" "Budford - Full Setup - V1.0.%Build%.exe" 
