import React from "react";
import {Stations} from "stations";

export default class Origin extends React.Component{
    constructor(){
        super();
        this.state = {station: "loading closest station"};
    }

    componentDidMount = () => Stations
        .loadNearby()
        .done(stations => {
            if (stations.length && stations.length > 0){
                var origin = stations[0];
                this.setState({station: origin.name});
                if (typeof this.props.onStationSelected === "function"){
                    this.props.onStationSelected(origin);
                }
            }
        })

    render = () => (
        <h3 className="origin">
            Closest station:
            <span className="station-name">{this.state.station}</span>
            <button className="btn btn-default btn-sm edit-icon" type="button" title="Change">
                <span className="glyphicon glyphicon-edit"></span>
            </button>
        </h3>
    )
};
