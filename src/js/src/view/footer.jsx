import React from "react";

export default class Footer extends React.Component{
    render(){
        return (
            <div className="footer">
                <span className="reference">
                    Powered by <a target="_blank" href="http://www.ns.nl/">NS.nl</a> API
                </span>

                <span className="copyright">
                    <a target="_blank" href="http://takemyoxygen.github.io/">takemyoxygen</a> &copy; 2015
                </span>
            </div>
        );
    }
}
