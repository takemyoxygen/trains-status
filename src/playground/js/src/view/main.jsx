import React from "react";
import DirectionsStatus from "view/favourite-directions"
import User from "view/user"

export default class Main extends React.Component{
    render = () => (
        <div>
            <User />
            <DirectionsStatus />
        </div>)
};
