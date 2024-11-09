@echo off

:: Navigate to the server directory
cd /d "C:\path\to\nattbakka-server"
echo Starting .NET server...
dotnet run

:: Navigate to the client directory
cd /d "C:\path\to\nattbakka-client"
echo Starting client with npm...
npm start

pause