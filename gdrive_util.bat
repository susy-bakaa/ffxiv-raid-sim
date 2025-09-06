@echo off
title Choose an operation
echo What would you like to do?
echo.
echo [1] Upload new archive to Google Drive
echo [2] Update archive on Google Drive
echo [3] Download archive from Google Drive
echo [4] Exit
echo.
echo Pick one by typing out the number.
echo.

set "_var=$dsk,$cd,$arc,$feid,$fdid,$exfe"
(for %%i in (%_var%)do set/p %%~i=)<.\.gdrive\gdrive_config.txt

choice /c 1234 /d 4 /n /t 30

if errorlevel 4 goto end
if errorlevel 3 goto dow
if errorlevel 2 goto upd
if errorlevel 1 goto upl

:upl
cls
.gdrive/gdrive_upload.bat

:upd
cls
.gdrive/gdrive_update.bat

:dow
cls
.gdrive/gdrive_download.bat

:end
timeout /t 1
exit