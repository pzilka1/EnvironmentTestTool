
echo off
title RNAS Testing Tool

WHERE dotnet
IF %ERRORLEVEL% NEQ 0 goto install
goto run

:install
cls
echo Installing .NET Core Runtime v2.0.0
echo Please wait for wizard to open
dotnet-runtime-2.0.0-win-x64.exe
goto commonexit

:run
cls
echo Starting test client
dotnet RNASTestingTool.dll
