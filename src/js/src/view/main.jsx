import React from "react";
import DirectionsStatus from "view/favourite-directions"
import User from "view/user"

export default class Main extends React.Component{
    render = () => (
        <div className="content-wrapper">
            <div className="main-content">
                <div className="header">
                    <User />
                    <img src="/img/train-black.png" className="logo"></img>
                    <h1>Trains status</h1>
                </div>
                <DirectionsStatus />
            </div>
            <div className="footer">
                <span className="reference">
                    Powered by <a target="_blank" href="http://www.ns.nl/">NS.nl</a> API
                </span>

                <span className="copyright">
                    <a target="_blank" href="http://takemyoxygen.github.io/">takemyoxygen</a> &copy; 2015
                </span>
            </div>
        </div>)
};
