@echo off
cls

@echo Restoring Paket dependencies
.paket\paket.bootstrapper.exe
.paket\paket.exe restore group AzureFunctions

"packages\azurefunctions\FAKE\tools\Fake.exe" build.fsx %*
exit /b %errorlevel%
