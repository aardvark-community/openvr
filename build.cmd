@echo off
setlocal

set "CONFIG=%~1"
if "%CONFIG%"=="" set "CONFIG=Debug"

dotnet tool restore
dotnet paket restore
dotnet build --configuration %CONFIG% src\OpenVR.sln

endlocal