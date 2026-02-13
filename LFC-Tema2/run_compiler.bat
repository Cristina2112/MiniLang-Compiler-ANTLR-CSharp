@echo off
REM Script pentru rularea compilatorului LFC

setlocal enabledelayedexpansion

echo ================================
echo  LFC Compiler - Test Runner
echo ================================
echo.

REM Set paths
set COMPILER_PATH=bin\Debug\LFC-Tema2.exe
set INPUT_FILE=%1

if "%INPUT_FILE%"=="" (
    set INPUT_FILE=input.txt
    echo Using default input file: input.txt
) else (
    echo Using specified input file: %INPUT_FILE%
)

echo.

REM Check if input file exists
if not exist "%INPUT_FILE%" (
    echo ERROR: Input file '%INPUT_FILE%' not found!
    echo Please create the file or specify a valid path.
    pause
    exit /b 1
)

REM Check if compiler exists
if not exist "%COMPILER_PATH%" (
    echo ERROR: Compiler not found at %COMPILER_PATH%
    echo Please build the project first using Ctrl+Shift+B
    pause
    exit /b 1
)

REM Copy input file to bin\Debug
echo Copying input file to compiler directory...
copy "%INPUT_FILE%" "bin\Debug\input.txt" >nul

REM Run compiler
echo.
echo ================================
echo  Running Compiler...
echo ================================
echo.

cd bin\Debug
%COMPILER_PATH%
set EXIT_CODE=%ERRORLEVEL%
cd ..\..

echo.
echo ================================
echo  Output Files Generated:
echo ================================
if exist "bin\Debug\tokens.txt" (
    echo [CREATED] tokens.txt - %CD%\bin\Debug\tokens.txt
)
if exist "bin\Debug\global_variables.txt" (
    echo [CREATED] global_variables.txt - %CD%\bin\Debug\global_variables.txt
)
if exist "bin\Debug\functions.txt" (
    echo [CREATED] functions.txt - %CD%\bin\Debug\functions.txt
)
if exist "bin\Debug\errors.txt" (
    echo [CREATED] errors.txt - %CD%\bin\Debug\errors.txt
)

echo.
echo Compiler exit code: %EXIT_CODE%
pause
