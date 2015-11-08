import React from "react";
import Stations from "stations";

export default class Origin extends React.Component{
    constructor(){
        super();
        this.state = {station: "loading closest station...", available: false};
    }

    componentDidMount(){
        this.subscription = Stations
            .origin
            .subscribe(station => {
                this.setState({
                    station: station.name,
                    liveDepartures: station.liveDeparturesUrl,
                    available: true
                });
            });
    }

    componentWillUnmount(){
        this.subscription.dispose();
    }

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
