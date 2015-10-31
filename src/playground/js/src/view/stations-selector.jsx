import React from "react";
import Autocomplete from "react-autocomplete";
import {Stations} from "stations";

export default class StationsSelector extends React.Component{
    constructor(){
        super();
        this.state = {stations: [], valid: true, text: ""};
    }

    static propTypes = {
        onStationSelected: React.PropTypes.func.isRequired,
        canSelectStation: React.PropTypes.func.isRequired
    }

    componentDidMount(){
        Stations
            .loadAll()
            .done(stations => this.setState({stations: stations}));
    }

    onSelect = (_, item) => {
        if (this.props.canSelectStation(item)){
            this.props.onStationSelected(item);
        } else {
            this.setState({valid: false});
        }
    }

    forceAdd(){
        const current =
            this.state.text &&
            this.state.valid &&
            this.state.stations.find(station => station.name.toLowerCase() == this.state.text.toLowerCase())

         if (current) this.onSelect(null, current)
         else this.setState({valid: false});
    }

    onChange = (_, text) => this.setState({valid: true, text: text});

    render(){
        return (
            <div className={`stations-selector ${this.state.valid ? "valid" : "invalid"}`}>
                <Autocomplete
                    items={this.state.stations}
                    getItemValue={x => x.name}
                    onSelect={this.onSelect}
                    onChange={this.onChange}
                    shouldItemRender={(st, val) => st.name.toLowerCase().indexOf(val.toLowerCase()) >= 0 }
                    renderItem={(item, highlighted, style) =>
                        <div className={`station ${highlighted ? "highlighted" : ""}`}>
                            {item.name}
                        </div>}/>
            </div>);
    }
}
