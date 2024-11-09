@echo off

cd nattbakka-server
echo Starting .NET server...
start "" dotnet run --launch-profile https

timeout /t 3 /nobreak
cd ..

cd nattbakka-client
echo Starting client with npm...
npm start

pause
