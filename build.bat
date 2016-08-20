@echo off
cls

@echo Restoring Paket dependencies
.paket\paket.bootstrapper.exe
.paket\paket.exe restore

"packages\FAKE\tools\Fake.exe" build.fsx %*
exit /b %errorlevel%
