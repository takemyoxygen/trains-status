import React from "react";
import Autocomplete from "react-autocomplete";
import {Stations} from "stations";

export default class StationsSelector extends React.Component{
    constructor(){
        super();
        this.state = {stations: []};
        this.props = {onStationSelected: () => {}};
    }

    static propTypes = {
        onStationSelected: React.PropTypes.func
    }

    componentDidMount(){
        Stations
            .loadAll()
            .done(stations => this.setState({stations: stations}));
    }

    onSelect = (item) => this.props.onStationSelected(item)

    render(){
        return (
            <div className="stations-selector">
                <Autocomplete
                    items={this.state.stations}
                    getItemValue={x => x}
                    onSelect={this.onSelect}
                    shouldItemRender={(st, val) => st.toLowerCase().indexOf(val.toLowerCase()) >= 0 }
                    renderItem={(item, highlighted, style) =>
                        <div className={`station ${highlighted ? "highlighted" : ""}`}>
                            {item}
                        </div>}/>
            </div>);
    }
}
