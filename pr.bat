@echo off
setlocal

set "COMMENT=%~1"

if "%COMMENT%"=="" (
    echo Comment required.
    exit /b 1
)

:: Get current datetime in yyyyMMddHHmmss format
for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMddHHmmss"') do set "DATETIME=%%i"

set "BRANCH=%DATETIME%-Sanjay"

echo git status
git status || goto :error

echo git checkout -b %BRANCH%
git checkout -b "%BRANCH%" || goto :error

echo git add .
git add . || goto :error

echo git commit -m "%COMMENT%"
git commit -m "%COMMENT%" || goto :error

echo git push -u origin %BRANCH%
git push -u origin "%BRANCH%" || goto :error

echo git checkout master
git checkout master || goto :error

echo git pull origin master
git pull origin master || goto :error

exit /b 0

:error
exit /b 1