import React from "react";
import ReactDOM from "react-dom";
import Main from "view/main"

export default class App {
    static start(){
        ReactDOM.render(<Main />, document.getElementById("app"))
    }
}
