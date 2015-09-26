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
      return <h3>Closest station: <span className="label label-primary">{this.state.station}</span></h3>;
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

  var TravelOptions = React.createClass({

      render: function(){
          return (
              <div className="row travel-options">
                  <div className="col-md-12">
                    {this.props.travelOptions.map(function(option, i, options){
                        return (
                            <div>
                                <div>
                                    from <StopStatus stop={option.from}/>,
                                    to <StopStatus stop={option.to}/>
                                </div>
                                {option.via.length > 0
                                    ? (
                                        <div>
                                            via {option.via.map(function(s){return s.station;}).join(", ")}
                                        </div>)
                                    : false}
                                {i < options.length - 1 ? <hr/>: false}
                            </div>
                        )
                    }.bind(this))}
                  </div>
              </div>
          );
      }
  });

  var Direction = React.createClass({
    getInitialState: function() {return {status: "loading", travelOptions: [], expanded: false};},
    labelType: function(status) {
      switch (status){
        case "ok": return "success";
        case "warning": return "warning";
        case "notfound": return "danger";
        default: return "info";
      }
    },
    iconType: function(status) {
      switch (status){
        case "ok": return "ok-sign";
        case "warning": return "alert";
        case "notfound": return "remove-sign";
        default: return "question-sign";
      }
    },
    componentDidMount: function(){
      this.originSubscription = this.props.origin
        .filter(function(s){return s != null;})
        .do(function(s){
          this.setState({disabled: s.name == this.props.destination.name});
        }.bind(this))
        .filter(function(s){
          return s.name != this.props.destination.name;
        }.bind(this))
        .subscribe(function(origin){
            this.checkStatus(origin);
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
        <li className={"list-group-item" + (this.state.disabled ? " hidden": "")}>
          <div className="row" style={{cursor: "pointer"}} onClick={this.toggleExpanded}>
            <div className="col-md-10">
              {this.props.destination.name}
            </div>
            <div className="col-md-1">
              <span className={"label label-" + this.labelType(this.state.status)}>
                <span className={"glyphicon glyphicon-" + this.iconType(this.state.status)} aria-hidden="true"></span>
              </span>
            </div>
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
          <div className="row">
            <div className="col-md-6">
              <ul className="list-group">
                {this.state.stations.map(function(s){
                  return <Direction origin={this.props.origin} destination={s} />;
                }.bind(this))}
              </ul>
            </div>
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
