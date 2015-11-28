import React from "react";
import DirectionsStatus from "view/favourite-directions"
import User from "view/user"
import Footer from "view/footer";

export default class Main extends React.Component{
    render = () => (
        <div className="content-wrapper">
            <div className="main-content">
                <div className="header">
                    <User />
                    <img src="/img/train-white.png" className="logo"></img>
                    <h1>Trains status</h1>
                </div>
                <DirectionsStatus />
            </div>
            <Footer/>
        </div>)
};
