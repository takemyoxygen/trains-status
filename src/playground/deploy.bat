@echo off

cls
pushd %~dp0

if not exist .paket (
  @echo "Installing Paket"
  mkdir .paket
  curl https://github.com/fsprojects/Paket/releases/download/1.4.0/paket.bootstrapper.exe -L --insecure -o .paket\paket.bootstrapper.exe

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

@echo "Preparing web.config"

if exist web.azure.config (
  @echo "Using web.azure.config"
  rm web.config
  rename web.azure.config web.config
)

if errorlevel 1 (
  exit /b %errorlevel%
)

@echo "Copying files to web root"
xcopy /s /y .  "%DEPLOYMENT_TARGET%"

cd "%DEPLOYMENT_TARGET%"

@echo "Installing npm packages"
call npm install --production

@echo "Installing bower packages"
call node_modules\.bin\bower.cmd install --production

@echo "Compiling JSX and JS files"
start node_modules\.bin\babel.cmd js/src --out-dir js/build --modules amd

@echo "Compiling LESS files"
call node_modules\.bin\autoless.cmd --no-watch styles styles

popd
