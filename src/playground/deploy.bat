@echo off

cls
pushd %~dp0

if not exist .paket (
  mkdir .paket
  curl https://github.com/fsprojects/Paket/releases/download/2.21.0/paket.bootstrapper.exe -L --insecure -o .paket\paket.bootstrapper.exe

  .paket\paket.bootstrapper.exe prerelease
  if errorlevel 1 (
    exit /b %errorlevel%
  )
)

@echo Restoring Paket dependencies
if not exist paket.lock (
  .paket\paket.exe install
) else (
  .paket\paket.exe restore
)

@echo Restoring Node dependencies
call npm install --production

packages\FAKE\tools\FAKE.exe build.fsx Build output-dir=%DEPLOYMENT_TARGET% env=azure

popd