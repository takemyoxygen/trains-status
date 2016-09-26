@echo off
cls

if "%FUNCTIONS_EXTENSION_VERSION%"=="" (
    @echo Restoring Paket dependencies
    .paket\paket.bootstrapper.exe
    .paket\paket.exe restore group AzureFunctions

    "packages\azurefunctions\FAKE\tools\Fake.exe" build.fsx %*
    exit /b %errorlevel%
) else (
    echo Deploying functions.
    xcopy src\functions %DEPLOYMENT_TARGET% /s /f
)

