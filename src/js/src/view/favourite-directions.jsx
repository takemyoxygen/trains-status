import React from "react";
import Stations from "stations";
import $ from "jquery";
import Rx from "rxjs";
import Origin from "view/origin";
import TravelOptions from "view/travel-options";
import Auth from "auth";
import StationsSelector from "view/stations-selector";
import Directions from "directions"

class Direction extends React.Component{
    constructor(){
        super();
        this.state = {status: "loading", travelOptions: [], expanded: false};
    }

    componentDidMount(){
        this.originSubscription = Stations
            .origin
            .do(station => console.log("Origin has changed to " + station.name))
            .subscribe(origin => {
                if (origin.name == this.props.destination.name){
                    this.setState({status: "same-as-origin"});
                } else {
                    this.checkStatus(origin);
                }
            });
    }

    componentWillUnmount() {
        this.originSubscription.dispose();
    }

    toggleExpanded = () => this.setState({expanded: !this.state.expanded})

    checkStatus(origin){
        $.getJSON(`/api/status/${origin.name}/${this.props.destination.name}`)
            .done(response => this.setState({
                status: response.status.toLowerCase(),
                travelOptions: response.options
            }));
    }

    removeDirection = () => Directions.removeFavourite(this.props.destination)

    render = () => (
        <li className={`list-group-item ${this.state.status} ${this.state.disabled ? "hidden": ""}`}>
            <div className="direction row">
                <div className="col-md-11 direction-name" onClick={this.toggleExpanded}>
                    {this.props.destination.name}
                </div>
                <div className="col-md-1 remove-direction">
                    <a title="Remove direction" onClick={this.removeDirection}>
                        <span className="glyphicon glyphicon-remove"/>
                    </a>
                </div>
            </div>
            {this.state.expanded ? <TravelOptions travelOptions={this.state.travelOptions} /> : null}
        </li>
    )
};

class AddFavouriteStation extends React.Component{
    constructor(){
        super();
        this.state = {inEditMode: false};
    }

    static propTypes = {
        onStationAdded: React.PropTypes.func.isRequired,
        canAddStation: React.PropTypes.func.isRequired
    }

    onAddStation = () => this.setState({inEditMode: true})

    onStationSelected = station => {
        this.props.onStationAdded(station);
        this.setState({inEditMode: false});
    }

    onCancel = () => this.setState({inEditMode: false});

    render(){
        return this.state.inEditMode
            ? (
                <div className="add-favourite-station">
                    <span>Pick a station:</span>
                    <StationsSelector
                        ref="stationSelector"
                        valid={this.state.valid}
                        autofocus={true}
                        canSelectStation={this.props.canAddStation}
                        onStationSelected={this.onStationSelected}
                        onCancel={this.onCancel} />
                </div>
            )
            : <a className="btn btn-primary" onClick={this.onAddStation}>Add station</a>;
    }
}

class FavouritesStatus extends React.Component{
    constructor(){
      super();
      this.state = {stations: [], userLoggedIn: false};
    }

    componentDidMount() {
        const directions = Directions
            .favourites
            .subscribe(favs => this.setState({stations: favs}));
        const users = Auth
            .currentUser
            .select(user => user.loggedIn)
            .subscribe(loggedIn => this.setState({userLoggedIn: loggedIn}));

        this.subscription = new Rx.CompositeDisposable(directions, users);
    }

    componentWillUnmount = () => this.subscription.dispose()
    onStationAdded = station => Directions.addFavourite(station)
    canAddStation = station => !this.state.stations.find(element => element.name.toLowerCase() == station.name.toLowerCase())

    render = () => (
        <div className="row favourites">
            <ul className="list-group">
                {this.state.stations.map((s, i) => <Direction key={`${i}-${s.name}`} destination={s} />)}
            </ul>
            {this.state.userLoggedIn && <AddFavouriteStation onStationAdded={this.onStationAdded} canAddStation={this.canAddStation}/>}
        </div>)
};

export default class DirectionsStatus extends React.Component{
    constructor(){
        super();
        this.state = {origin: Stations.origin};
    }

    render = () => (
        <div>
          <Origin />
          <FavouritesStatus origin={this.state.origin}/>
        </div>);
 };
