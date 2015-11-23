import React from "react";
import ReactDOM from "react-dom";
import Autocomplete from "react-autocomplete";
import Stations from "stations";
import Select from "react-select";

export default class StationsSelector extends React.Component{
    constructor(){
        super();
        this.state = {stations: [], valid: true, text: ""};
    }

    static propTypes = {
        onStationSelected: React.PropTypes.func.isRequired,
        canSelectStation: React.PropTypes.func.isRequired,
        onCancel: React.PropTypes.func,
        currentStation: React.PropTypes.object,
        autofocus: React.PropTypes.bool
    }

    componentDidMount(){
        Stations
            .loadAll()
            .done(stations => this.setState({stations: stations}));
    }

    componentDidUpdate(){
        if (this.props.autofocus){
            $(ReactDOM.findDOMNode(this))
                .find("input[role=combobox]")
                .focus();
        }
    }

    onSelect = (_, item) => {
        if (this.props.canSelectStation(item)){
            this.props.onStationSelected(item);
        } else {
            this.setState({valid: false});
        }
    }

    onOk = () => {
        const current =
            this.state.text &&
            this.state.valid &&
            this.state.stations.find(station => station.name.toLowerCase() == this.state.text.toLowerCase())

         if (current) this.onSelect(null, current)
         else this.setState({valid: false});
    }

    onChange = (_, text) => this.setState({valid: true, text: text});

    render(){
        var items = [
            {key: "k1", value: "v1"},
            {key: "k2", value: "v2"}
        ];
        return (
            <div className={`stations-selector ${this.state.valid ? "valid" : "invalid"}`}>
                <Select name="some-select" options={items} value="please select something..." autofocus clearable simpleValue />
                <a className="btn btn-primary" onClick={this.onOk}>Ok</a>
                <a className="btn btn-default" onClick={this.props.onCancel && this.props.onCancel}>Cancel</a>
            </div>);
    }
}
