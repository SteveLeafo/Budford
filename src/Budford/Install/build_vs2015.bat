@echo off
set /p Build=<ver.txt
set /a Build=%Build%+1
echo %Build%>ver.txt

fart ..\Properties\AssemblyInfo.cs 1.0.0.0  0.1.0.%Build%
"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\DEVENV"   ..\..\Budford.sln /Rebuild Release
svn revert ..\Properties\AssemblyInfo.cs
"c:\Program Files (x86)\NSIS\Bin\makensis.exe" Budford.nsi
"c:\Program Files (x86)\NSIS\Bin\makensis.exe" "Budford - Full.nsi"

ren "Budford - Setup.exe" "Budford - Setup - V0.1.%Build%.exe" 
ren "Budford - Full Setup.exe" "Budford - Full Setup - V0.1.%Build%.exe" 
