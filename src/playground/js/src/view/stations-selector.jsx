import React from "react";
import Autocomplete from "react-autocomplete";
import {Stations} from "stations";

export default class StationsSelector extends React.Component{
    constructor(){
        super();
        this.state = {stations: [], valid: true};
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

    onChange = () => this.setState({valid: true});

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
