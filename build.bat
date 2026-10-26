@echo off
rem Dino Joe - lokalni build (bez instalacija, koristi ugradjeni .NET Framework 4.x csc.exe)
setlocal
cd /d "%~dp0"
if not exist build mkdir build
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
"%CSC%" /nologo /target:winexe /optimize+ /out:build\DinoJoe.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Security.dll /r:System.Web.Extensions.dll src\*.cs
if errorlevel 1 (
  echo BUILD GRESKA
  if not defined CI pause
  exit /b 1
)
xcopy /e /i /y /q packs-builtin build\packs-builtin >nul
echo OK  build\DinoJoe.exe
if not defined CI pause
