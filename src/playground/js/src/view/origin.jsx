import React from "react";
import {Stations} from "stations";

export default class Origin extends React.Component{
    constructor(){
        super();
        this.state = {station: "loading closest station...", available: false};
    }

    componentDidMount = () => Stations
        .loadNearby()
        .done(stations => {
            if (stations.length && stations.length > 0){
                var origin = stations[0];
                this.setState({
                    station: origin.name,
                    liveDepartures: origin.liveDeparturesUrl,
                    available: true
                });

                if (typeof this.props.onStationSelected === "function"){
                    this.props.onStationSelected(origin);
                }
            }
        })

    render(){
        var disabled = this.state.available ? "" : "disabled";
        return (
            <h3 className="origin">
                Closest station:
                <span className="station-name">{this.state.station}</span>
                {this.state.available ? (
                    <div className="origin-buttons">
                        <button className={`btn btn-default btn-sm edit-icon ${disabled}`} type="button" title="Change">
                            <span className="glyphicon glyphicon-edit"></span>
                        </button>
                        <a target="_blank" href={this.state.liveDepartures} title="See all departures" className={`btn btn-default btn-sm ${disabled}`}>
                            <span className="glyphicon glyphicon-eye-open"></span>
                        </a>
                    </div>) : false}
            </h3>
    );}
};
