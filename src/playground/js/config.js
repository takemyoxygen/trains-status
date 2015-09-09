requirejs.config({
  baseUrl: "js",
  paths: {
    bootstrap: "../bower_components/bootstrap/dist/js/bootstrap",
    jquery: "../bower_components/jquery/dist/jquery",
    react: "../bower_components/react/react",
    requirejs: "../bower_components/requirejs/require",
    rxjs: "../bower_components/rxjs/dist/rx.all"
  },
  packages: [

  ]
});

require(["app"], function(foo){
  foo.start();
});
