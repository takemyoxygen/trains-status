@echo off
cls

if "%FUNCTIONS_EXTENSION_VERSION%"=="" (
    @echo Restoring Paket dependencies
    .paket\paket.bootstrapper.exe
    .paket\paket.exe restore group AzureFunctions

    "packages\azurefunctions\FAKE\tools\Fake.exe" build.fsx %*
    exit /b %errorlevel%
) else (
    echo Deploying functions

    echo Deleting old stuff...
    for /f %%i in ('dir /b /a:d %DEPLOYMENT_TARGET%') do (
        echo Deleting %DEPLOYMENT_TARGET%\%%i...
        rd %DEPLOYMENT_TARGET%\%%i /s/q
    )

    echo Copying new stuff...
    xcopy src\functions %DEPLOYMENT_TARGET% /s /f /Y
)

