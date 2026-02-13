#!/bin/bash
# Script for running the LFC Compiler on Linux/Mac

COMPILER_PATH="bin/Debug/LFC-Tema2.exe"
INPUT_FILE="${1:-input.txt}"

echo "================================"
echo "  LFC Compiler - Test Runner"
echo "================================"
echo ""

if [ ! -f "$INPUT_FILE" ]; then
    echo "ERROR: Input file '$INPUT_FILE' not found!"
    echo "Please create the file or specify a valid path."
    exit 1
fi

if [ ! -f "$COMPILER_PATH" ]; then
    echo "ERROR: Compiler not found at $COMPILER_PATH"
    echo "Please build the project first using 'dotnet build'"
    exit 1
fi

echo "Copying input file to compiler directory..."
cp "$INPUT_FILE" "bin/Debug/input.txt"

echo ""
echo "================================"
echo "  Running Compiler..."
echo "================================"
echo ""

cd bin/Debug
mono LFC-Tema2.exe
EXIT_CODE=$?
cd ../..

echo ""
echo "================================"
echo "  Output Files Generated:"
echo "================================"
[ -f "bin/Debug/tokens.txt" ] && echo "[CREATED] tokens.txt"
[ -f "bin/Debug/global_variables.txt" ] && echo "[CREATED] global_variables.txt"
[ -f "bin/Debug/functions.txt" ] && echo "[CREATED] functions.txt"
[ -f "bin/Debug/errors.txt" ] && echo "[CREATED] errors.txt"

echo ""
echo "Compiler exit code: $EXIT_CODE"
