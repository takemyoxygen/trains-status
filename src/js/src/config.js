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
    "react-bootstrap": "../../bower_components/react-bootstrap/react-bootstrap.min",
    "react-select": "../../bower_components/react-select/dist/react-select",
    //classNames: "../../bower_components/classnames/index",
    classnames: "../../bower_components/classnames/index",
    "react-input-autosize": "../../bower_components/react-input-autosize/dist/react-input-autosize.min"
  },
  map: {
      "react-autocomplete": {
          "React": "react"
      },
  },
  shim: {
      "react-input-autosize": {
          deps: ["global-react"]
      },
      "react-select": {
          deps:["global-modules"]
      }
  }
});

define("global-react", ["react"], function(React){
    window.React = React;
})

define("global-modules", ["classnames", "react-input-autosize"], function(classNames, input){
    window.classNames = classNames;
    window.AutosizeInput = input;
});

require(["app"], function(App){
    App.start();
});
