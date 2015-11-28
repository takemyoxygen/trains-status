import React from "react";
import DirectionsStatus from "view/favourite-directions"
import User from "view/user"

export default class Main extends React.Component{
    render = () => (
        <div>
            <div className="header">
                <User />
                <img src="/img/train-black.png" className="logo"></img>
                <h1>Trains status</h1>
            </div>
            <DirectionsStatus />
        </div>)
};
