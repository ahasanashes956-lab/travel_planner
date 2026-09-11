@echo off
cd /d "%~dp0"
echo Starting Travel Planner...
echo.
echo Opening http://localhost:8000 in your browser...
timeout /t 2 /nobreak
start http://localhost:8000
python -m http.server 8000
pause
