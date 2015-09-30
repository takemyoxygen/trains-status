define(["react", "rxjs", "jquery", "stations"], function(React, Rx, $, Stations){

  class Origin extends React.Component{
      constructor(){
          super();
          this.state = {station: "loading closest station"};
      }

    componentDidMount = () => Stations
        .loadNearby()
        .done(stations => {
          if (stations.length && stations.length > 0){
            var origin = stations[0];
            this.setState({station: origin.name});
            if (typeof this.props.onStationSelected === "function"){
              this.props.onStationSelected(origin);
            }
          }
        })
    render = () => (
          <h3 className="origin">
            Closest station:
            <span className="station-name">{this.state.station}</span>
            <button className="btn btn-default btn-sm edit-icon" type="button" title="Change">
                <span className="glyphicon glyphicon-edit"></span>
            </button>
          </h3>
      )
  };

  class StopStatus extends React.Component{
      formatDate(s){
          var date = new Date(s);
          return date.getHours() + ":" + date.getMinutes();
      }
      render(){
          var delayed = this.props.stop.delay != null;
          return (
              <span>
                  <span>{this.props.stop.station}</span>
                  <span className={delayed ? "delayed" : ""}>
                      {" (" + this.formatDate(this.props.stop.time) + (delayed ? (" " + this.props.stop.delay) : "") + ")"}
                  </span>
              </span>
          );
      }
  };

  class Transfers extends React.Component{
      render(){
          if (this.props.transfers == null || this.props.transfers.length == 0) {
              return null;
          }

          return (
              <span>
                  via {this.props.transfers.map(function(s){
                      return <StopStatus stop={s}/>;
                  })}
              </span>
          );
      }
  };

  class TravelOptionStatus extends React.Component{
      render(){
          var iconType;
          switch (this.props.status) {
              case "ok":
                iconType = "glyphicon-ok";
                break;
              case "warning":
                iconType = "glyphicon-warning-sign";
                break;
              case "cancelled":
                iconType = "glyphicon-remove";
                break;
              default:
                iconType = "glyphicon-question-sign";
                break
          }

          return <span className={"glyphicon " + iconType}></span>
      }
  };

  class TravelOption extends React.Component{
      render = () => (
          <div className="row travel-option">
              <div className="travel-option-description col-md-11">
                  <span>from <StopStatus stop={this.props.option.from}/></span>
                  <span>to <StopStatus stop={this.props.option.to}/></span>
                  <Transfers transfers={this.props.option.via} />
              </div>
              <div className="status col-md-1">
                  <TravelOptionStatus status={this.props.option.status}/>
              </div>
          </div>
        );
  };

  class TravelOptions extends React.Component{
      render = () => (
        <div className="travel-options">
            {this.props.travelOptions.map(function(option){
                return <TravelOption option={option} />
            })}
        </div>
      )
  };

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
      $.getJSON("/api/status/" + origin.name + "/" + this.props.destination.name)
        .done(response => this.setState({status: response.status, travelOptions: response.options}));
    }

    render = () => (
        <li className={"list-group-item" + " " + this.state.status + (this.state.disabled ? " hidden": "")}>
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

    componentDidMount = () => Stations
        .loadFavourites()
        .done(stations => this.setState({stations: stations}))

    render = () => (
      <div className="row favourites">
          <ul className="list-group">
            {this.state.stations.map(s => <Direction origin={this.props.origin} destination={s} />)}
          </ul>
      </div>)
  };

  class DirectionsStatus extends React.Component{
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

  class App extends React.Component{
    render = () => <DirectionsStatus />
  };

  return {
    start: function(){
      React.render(<App />, document.getElementById("app"));
    }
  };
});
