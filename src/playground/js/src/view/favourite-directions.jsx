import React from "react";
import {Stations} from "stations";
import $ from "jquery";
import Rx from "rxjs";
import Origin from "view/origin"
import TravelOptions from "view/travel-options"
import Auth from "auth";

class Direction extends React.Component{
    constructor(){
        super();
        this.state = {status: "loading", travelOptions: [], expanded: false};
    }

    componentDidMount(){
        this.originSubscription = this.props.origin
            .filter(s => s != null)
            .subscribe(origin => {
                if (origin.name == this.props.destination.name){
                    this.setState({status: "disabled"});
                } else {
                    this.checkStatus(origin);
                }
            });
    }

    toggleExpanded = () => this.setState({expanded: !this.state.expanded})

    checkStatus(origin){
        $.getJSON(`/api/status/${origin.name}/${this.props.destination.name}`)
            .done(response => this.setState({
                status: response.status,
                travelOptions: response.options
            }));
    }

    render = () => (
        <li className={`list-group-item ${this.state.status} ${this.state.disabled ? "hidden": ""}`}>
            <div className="direction" onClick={this.toggleExpanded}>
                {this.props.destination.name}
            </div>
            {this.state.expanded ? <TravelOptions travelOptions={this.state.travelOptions} /> : null}
        </li>
    )
};

class FavouritesStatus extends React.Component{
    constructor(){
      super();
      this.state = {stations: []};
    }

    componentDidMount = () =>
        Auth.currentUser
            .subscribe(user => {
                if (user.loggedIn){
                    Stations
                        .loadFavourites(user.id)
                        .done(stations => this.setState({stations: stations}));
                } else {
                    this.setState({stations: []});
                }
            })

    render = () => (
        <div className="row favourites">
            <ul className="list-group">
                {this.state.stations.map(s => <Direction origin={this.props.origin} destination={s} />)}
            </ul>
        </div>)
};

export default class DirectionsStatus extends React.Component{
    constructor(){
        super();
        this.state = {origin: new Rx.BehaviorSubject(null)}
    }

    onOriginChanged = origin => this.state.origin.onNext(origin)

    render = () => (
        <div>
          <Origin onStationSelected={this.onOriginChanged}/>
          <FavouritesStatus origin={this.state.origin}/>
        </div>);
 };
