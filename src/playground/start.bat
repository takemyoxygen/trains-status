start fsi app.fsx loglevel=verbose
start node_modules\.bin\babel.cmd js/src --watch --out-dir js/build --modules amd
start node_modules\.bin\autoless.cmd styles styles
