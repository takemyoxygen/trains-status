define(["react", "rxjs", "jquery", "stations"], function(React, Rx, $, Stations){

  var Origin = React.createClass({displayName: "Origin",
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
      return React.createElement("h3", null, "Closest station: ", React.createElement("span", {className: "label label-primary"}, this.state.station));
    }
  });

  var TrainsList = React.createClass({displayName: "TrainsList",
      formatDate: function(s){
          var date = new Date(s);
          return date.getHours() + ":" + date.getMinutes();
      },
      render: function(){
          return (
              React.createElement("div", {className: "row"}, 
                  React.createElement("ul", null, 
                    this.props.trains.map(function(t){
                        return React.createElement("li", null, this.formatDate(t.time) + ":" + t.legs.join(" -> "))
                    }.bind(this))
                  )
              )
          );
      }
  });

  var Direction = React.createClass({displayName: "Direction",
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
        React.createElement("li", {className: "list-group-item" + (this.state.disabled ? " disabled": "")}, 
          React.createElement("div", {className: "row"}, 
            React.createElement("div", {className: "col-md-10"}, 
              this.props.destination.name
            ), 
            React.createElement("div", {className: "col-md-1" + (this.state.disabled ? " hidden" : "")}, 
              React.createElement("span", {className: "label label-" + this.labelType(this.state.status)}, 
                React.createElement("span", {className: "glyphicon glyphicon-" + this.iconType(this.state.status), "aria-hidden": "true"})
              )
            )
          ), 
          React.createElement(TrainsList, {trains: this.state.trains})
        )
      );
    }
  })

  var FavouritesStatus = React.createClass({displayName: "FavouritesStatus",
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
          React.createElement("div", {className: "row"}, 
            React.createElement("div", {className: "col-md-5"}, 
              React.createElement("ul", {className: "list-group"}, 
                this.state.stations.map(function(s){
                  return React.createElement(Direction, {origin: this.props.origin, destination: s});
                }.bind(this))
              )
            )
          ));
    }
  });

  var DirectionsStatus = React.createClass({displayName: "DirectionsStatus",
    onOriginChanged: function(station){
      this.props.origin.onNext(station);
    },
    getDefaultProps: function(){
      return {origin: new Rx.BehaviorSubject(null)};
    },
    render: function(){
      return (
        React.createElement("div", null, 
          React.createElement(Origin, {onStationSelected: this.onOriginChanged}), 
          React.createElement(FavouritesStatus, {origin: this.props.origin})
        ));
    }
  });

  var App = React.createClass({displayName: "App",
    render: function(){
      return React.createElement(DirectionsStatus, null);
    }
  });

  return {
    start: function(){
      React.render(React.createElement(App, null), document.getElementById("app"));
    }
  };
});
