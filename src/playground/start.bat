REM start fsi app.fsx loglevel=verbose
REM start node_modules\.bin\babel.cmd js/src --watch --out-dir js/build --modules amd --stage 0
REM start node_modules\.bin\autoless.cmd styles styles
packages\FAKE\tools\FAKE.exe build.fsx Run 
