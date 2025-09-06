@echo off
title Choose an operation
echo What would you like to do?
echo.
echo [1] Create a new archive and update an archive on Google Drive
echo [2] Update an archive on Google Drive with an existing local archive
echo [3] Back
echo.
echo Pick one by typing out the number.
echo.

choice /c 1234 /d 3 /n /t 30

if errorlevel 3 goto bak
if errorlevel 2 goto old
if errorlevel 1 goto new

:new
@echo off
title Creating archive file...
@echo on
%$dsk%
cd %$cd%
tar --exclude-from=%$exfe% -czvf %$arc% *
@echo off
title Updating archive file...
@echo on
gdrive files update %$feid% %$arc%
@echo off
title Archive updated!
goto quearc

:old
@echo off
title Updating archive file...
@echo on
gdrive files update %$feid% %$arc%
@echo off
title Archive updated!
goto quearc

:bak
cls
gdrive_util.bat

:quearc
echo.
echo What would you like to do now?
echo.
echo [1] Delete the local archive and exit
echo [2] Exit
echo.

choice /c 12 /d 2 /n /t 300

if errorlevel 2 goto end
if errorlevel 1 goto delarc

:delarc
del %$arc%
goto end

:end
pause
exit