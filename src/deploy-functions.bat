@echo off

echo Deploying functions...
xcopy src\functions %DEPLOYMENT_TARGET% /s /f