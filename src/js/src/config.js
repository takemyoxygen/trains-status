requirejs.config({
  baseUrl: "js/build",
  paths: {
    bootstrap: "../../bower_components/bootstrap/dist/js/bootstrap",
    jquery: "../../bower_components/jquery/dist/jquery",
    react: "../../bower_components/react/react",
    requirejs: "../../bower_components/requirejs/require",
    rxjs: "../../bower_components/rxjs/dist/rx.all",
    "react-autocomplete": "../../bower_components/react-autocomplete/dist/react-autocomplete",
    "react-dom": "../../bower_components/react/react-dom",
    "react-bootstrap": "../../bower_components/react-bootstrap/react-bootstrap.min"
  },
  map: {
      "react-autocomplete": {
          "React": "react"
      }
  },
  packages: [

  ]
});

require(["app"], function(App){
  App.start();
});
