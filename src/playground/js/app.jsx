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

  var TrainsList = React.createClass({
      formatDate: function(s){
          var date = new Date(s);
          return date.getHours() + ":" + date.getMinutes();
      },
      render: function(){
          return (
              <div className="row">
                  <ul>
                    {this.props.trains.map(function(t){
                        return <li>{this.formatDate(t.time) + ":" + t.legs.join(" -> ")}</li>
                    }.bind(this))}
                  </ul>
              </div>
          );
      }
  });

  var Direction = React.createClass({
    getInitialState: function() {return {status: "loading", trains: []};},
    labelType: function(status) {
      switch (status){
        case "ok": return "success";
        case "delayed": return "warning";
        case "notfound": return "danger";
        default: return "info";
      }
    },
    iconType: function(status) {
      switch (status){
        case "ok": return "ok-sign";
        case "delayed": return "alert";
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
    checkStatus: function(origin){
      console.log("Checkig status...");
      $.getJSON("/api/status/" + origin.name + "/" + this.props.destination.name)
        .done(function(response){
          if (this.isMounted()){
              this.setState({status: response.status.toLowerCase(), trains: response.trains});
          }
        }.bind(this));
    },
    render: function(){
      return (
        <li className={"list-group-item" + (this.state.disabled ? " disabled": "")}>
          <div className="row">
            <div className="col-md-10">
              {this.props.destination.name}
            </div>
            <div className={"col-md-1" + (this.state.disabled ? " hidden" : "")}>
              <span className={"label label-" + this.labelType(this.state.status)}>
                <span className={"glyphicon glyphicon-" + this.iconType(this.state.status)} aria-hidden="true"></span>
              </span>
            </div>
          </div>
          <TrainsList trains={this.state.trains} />
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
            <div className="col-md-5">
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
