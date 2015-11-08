import React from "react";
import Stations from "stations";
import StationsSelector from "view/stations-selector";

export default class Origin extends React.Component{
    constructor(){
        super();
        this.state = {station: "loading closest station...", available: false, inEditMode: false};
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

    toEditMode = () => this.setState({inEditMode: true})

    onStationSelected = (station) => {
        console.log("New origin selected: " + station.name);
        Stations.setOrigin(station);
        this.setState({inEditMode: false});
    }

    render(){
        var disabled = this.state.available ? "" : "disabled";
        return (
            <h3 className="origin">
                {this.state.inEditMode
                    ? (
                        <StationsSelector canSelectStation={() => true} onStationSelected={this.onStationSelected} />
                    )
                    : (
                        <div>
                            Departure:
                            <span className="station-name">{this.state.station}</span>
                            {this.state.available && (
                                <div className="origin-buttons">
                                    <a className={`btn btn-default btn-sm edit-icon ${disabled}`} title="Change" onClick={this.toEditMode}>
                                        <span className="glyphicon glyphicon-edit"></span>
                                    </a>
                                    <a target="_blank" href={this.state.liveDepartures} title="See all departures" className={`btn btn-default btn-sm ${disabled}`}>
                                        <span className="glyphicon glyphicon-eye-open"></span>
                                    </a>
                                </div>)}
                        </div>)}
            </h3>
    );}
};
