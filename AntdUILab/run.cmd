@echo off
chcp 65001 >nul
setlocal
cd /d "%~dp0"

if exist "bin\Debug\AntdUILab.exe" goto run

echo [AntdUI Lab] 还没有生成，正在编译...
set "MSBUILD=C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
if exist "%MSBUILD%" goto build

set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" goto nomsbuild
for /f "delims=" %%i in ('"%VSWHERE%" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe 2^>nul') do set "MSBUILD=%%i"
if not defined MSBUILD goto nomsbuild

:build
"%MSBUILD%" "AntdUILab.sln" /t:Build /p:Configuration=Debug /v:minimal /nologo
if errorlevel 1 (
  echo.
  echo 编译失败，请用 Visual Studio 打开 AntdUILab.sln 查看错误。
  pause
  exit /b 1
)

:run
start "" "bin\Debug\AntdUILab.exe"
exit /b 0

:nomsbuild
echo 找不到 MSBuild，请用 Visual Studio 打开 AntdUILab.sln 生成一次。
pause
exit /b 1
