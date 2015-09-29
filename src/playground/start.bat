start fsi app.fsx loglevel=verbose
start node_modules\.bin\jsx.cmd --watch -x jsx js/ js/
start node_modules\.bin\autoless.cmd styles styles
