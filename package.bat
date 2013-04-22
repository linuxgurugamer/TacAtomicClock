@echo off

set DIR=TacAtomicClock_v%1

mkdir Release\%DIR%

xcopy /s /f /y Parts Release\%DIR%\Parts\
xcopy /s /f /y Plugins Release\%DIR%\Plugins\
copy /y LICENSE.txt Release\%DIR%\
copy /y Readme.txt Release\%DIR%\

cd Release
7z a -tzip %DIR%.zip %DIR%
cd ..