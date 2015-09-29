define(["react", "rxjs", "jquery", "stations"], function(React, Rx, $, Stations){

  var Origin = React.createClass({
    getInitialState: function(){
      return {station: "..."};
    },
    componentDidMount: function(){
      Stations.loadNearby()
        .done(function(stations){
          if (this.isMounted() && stations.length && stations.length > 0){
            var origin = stations[0];
            this.setState({station: origin.name});
            if (typeof this.props.onStationSelected === "function"){
              this.props.onStationSelected(origin);
            }
          }
        }.bind(this));
    },
    render: function(){
      return (
          <h3 className="origin">
            Closest station:
            <span className="station-name">{this.state.station}</span>
            <button className="btn btn-default btn-sm edit-icon" type="button" title="Change">
                <span className="glyphicon glyphicon-edit"></span>
            </button>
          </h3>
      );
    }
  });

  var StopStatus = React.createClass({
      formatDate: function(s){
          var date = new Date(s);
          return date.getHours() + ":" + date.getMinutes();
      },
      render: function(){
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
  });

  var Transfers = React.createClass({
      render: function(){
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
  });

  var TravelOption = React.createClass({
      render: function(){
          return (
              <div className="row travel-option">
                  <span>from <StopStatus stop={this.props.option.from}/>,</span>
                  <span>to <StopStatus stop={this.props.option.to}/></span>
                  <Transfers transfers={this.props.option.via} />
              </div>
        );
    }
  });

  var TravelOptions = React.createClass({
      render: function(){
          return (
            <div className="travel-options">
                {this.props.travelOptions.map(function(option){
                    return <TravelOption option={option} />
                })}
            </div>
          );
      }
  });

  var Direction = React.createClass({
    getInitialState: function() {
        return {status: "loading", travelOptions: [], expanded: false};
    },
    componentDidMount: function(){
      this.originSubscription = this.props.origin
        .filter(function(s){return s != null;})
        .subscribe(function(origin){
            if (origin.name == this.props.destination.name){
                this.setState({status: "disabled"});
            } else {
                this.checkStatus(origin);
            }
      }.bind(this));
    },
    toggleExpanded: function(){
        this.setState({expanded: !this.state.expanded});
    },
    checkStatus: function(origin){
      $.getJSON("/api/status/" + origin.name + "/" + this.props.destination.name)
        .done(function(response){
          if (this.isMounted()){
              this.setState({status: response.status, travelOptions: response.options});
          }
        }.bind(this));
    },
    render: function(){
      return (
        <li className={"list-group-item" + " " + this.state.status + (this.state.disabled ? " hidden": "")}>
          <div className="direction" onClick={this.toggleExpanded}>
              {this.props.destination.name}
          </div>
          {this.state.expanded ? <TravelOptions travelOptions={this.state.travelOptions} /> : null}
        </li>
      );
    }
  })

  var FavouritesStatus = React.createClass({
    componentDidMount: function(){
      Stations
        .loadFavourites()
        .done(function(stations){
          this.setState({stations: stations});
        }.bind(this));
    },
    getInitialState: function(){
      return {stations: []};
    },
    getDefaultProps: function(){
      return {};
    },
    render: function(){
        return (
          <div className="row favourites">
              <ul className="list-group">
                {this.state.stations.map(function(s){
                  return <Direction origin={this.props.origin} destination={s} />;
                }.bind(this))}
              </ul>
          </div>);
    }
  });

  var DirectionsStatus = React.createClass({
    onOriginChanged: function(station){
      this.props.origin.onNext(station);
    },
    getDefaultProps: function(){
      return {origin: new Rx.BehaviorSubject(null)};
    },
    render: function(){
      return (
        <div>
          <Origin onStationSelected={this.onOriginChanged}/>
          <FavouritesStatus origin={this.props.origin}/>
        </div>);
    }
  });

  var App = React.createClass({
    render: function(){
      return <DirectionsStatus />;
    }
  });

  return {
    start: function(){
      React.render(<App />, document.getElementById("app"));
    }
  };
});
