goto start

:start
@echo off
title Creating archive file...
@echo on
%$dsk%
cd %$cd%
tar --exclude-from=%$exfe% -czvf %$arc% *
@echo off
title Uploading archive file...
@echo on
gdrive files upload --parent %$fdid% %$arc%
@echo off
title Archive uploaded!
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