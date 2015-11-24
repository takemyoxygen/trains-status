import React from "react";
import ReactDOM from "react-dom";
import Autocomplete from "react-autocomplete";
import Stations from "stations";
import Select from "react-select";

export default class StationsSelector extends React.Component{
    constructor(props){
        super();
        this.state = {stations: [], valid: true, currentStation: props.currentStation};
    }

    static propTypes = {
        onStationSelected: React.PropTypes.func.isRequired,
        canSelectStation: React.PropTypes.func.isRequired,
        onCancel: React.PropTypes.func,
        currentStation: React.PropTypes.object,
    }

    componentDidMount(){
        Stations
            .loadAll()
            .done(stations => this.setState({stations: stations}));
    }

    onChange = station => {
        console.log("Current station changed to:" + station.name);
        let valid = this.props.canSelectStation(station);
        if (valid){
            this.props.onStationSelected(station);
        }
        this.setState({valid: valid, currentStation: station});
    }

    render(){
        return (
            <div className={`stations-selector ${this.state.valid ? "valid" : "invalid"}`}>
                <div className="stations-dropdown">
                    <Select
                        name="some-select"
                        options={this.state.stations}
                        value={this.state.currentStation}
                        autofocus
                        searchable={true}
                        clearable={false}
                        labelKey="name"
                        onChange={this.onChange} />
                </div>
                <a className="btn btn-default" onClick={this.props.onCancel && this.props.onCancel}>Cancel</a>
            </div>);
    }
}
