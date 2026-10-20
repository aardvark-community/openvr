#!/bin/sh

CONFIG="${1:-Debug}"

dotnet tool restore
dotnet paket restore
dotnet build --configuration "${CONFIG}" src/OpenVR.sln