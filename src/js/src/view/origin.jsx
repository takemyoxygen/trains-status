import React from "react";
import Stations from "stations";
import StationsSelector from "view/stations-selector";
import $ from "jquery";

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
                    station: station,
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

    onCancel = () => this.setState({inEditMode: false});

    render(){
        var disabled = this.state.available ? "" : "disabled";
        return (
            <h3>
                {this.state.inEditMode
                    ? (
                        <div className="change-origin">
                            <span>New origin:</span>
                            <StationsSelector
                                currentStation={this.state.station}
                                canSelectStation={() => true}
                                onStationSelected={this.onStationSelected}
                                onCancel={this.onCancel}/>
                        </div>
                    )
                    : (
                        <div className="origin">
                            From    :
                            <span className="station-name">{this.state.station.name}</span>
                            {this.state.available && (
                                <div className="origin-buttons">
                                    <a className={`btn btn-default edit-icon ${disabled}`} onClick={this.toEditMode}>
                                        Change
                                    </a>
                                    <a target="_blank" href={this.state.liveDepartures} className={`btn btn-default ${disabled}`}>
                                        Open on NS.nl
                                    </a>
                                </div>)}
                        </div>)}
            </h3>
    );}
};
