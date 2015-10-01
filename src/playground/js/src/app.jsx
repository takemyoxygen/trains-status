import React from "react";
import Main from "view/main"

export default class App {
    static start(){
        React.render(<Main />, document.getElementById("app"))
    }
}
