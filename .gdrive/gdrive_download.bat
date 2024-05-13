goto start

:start
@echo off
title Downloading archive file...
@echo on
%$dsk%
cd %$cd%
gdrive files download %$feid%
@echo off
title Extracting archive file...
@echo on
tar -xzvf %$arc% -C %$cd%
@echo off
title Archive extracted!
echo.
echo What would you like to do?
echo.
echo [1] Delete local archive and exit
echo [2] Exit
echo.

choice /c 12 /d 2 /n /t 60

if errorlevel 2 goto end
if errorlevel 1 goto delarc

:delarc
del %$arc%
goto end

:end
pause
exit