@echo off

cls
pushd %~dp0

if not exist .paket (
  @echo "Installing Paket"
  mkdir .paket
  curl https://github.com/fsprojects/Paket/releases/download/2.21.0/paket.bootstrapper.exe -L --insecure -o .paket\paket.bootstrapper.exe

  .paket\paket.bootstrapper.exe prerelease
  if errorlevel 1 (
    exit /b %errorlevel%
  )
)

if not exist paket.lock (
  @echo "Installing dependencies"
  .paket\paket.exe install
) else (
  @echo "Restoring dependencies"
  .paket\paket.exe restore
)

packages\FAKE\tools\FAKE.exe build.fsx Build output-dir="%DEPLOYMENT_TARGET%"

popd