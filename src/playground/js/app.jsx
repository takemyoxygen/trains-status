var Origin = React.createClass({
  getInitialState: function(){
    return {station: "..."};
  },
  componentDidMount: function(){
    Stations.load()
      .done(function(stations){
        if (this.isMounted() && stations.length && stations.length > 0){
          var station = stations[0].name;
          this.setState({station: station});
          if (typeof this.props.onStationSelected === "function"){
            this.props.onStationSelected(station);
          }
        }
      }.bind(this));
  },
  render: function(){
    return <h3>Closest station: <span className="label label-primary">{this.state.station}</span></h3>;
  }
});

var DirectionsStatus = React.createClass({
  render: function(){
    return <Origin onStationSelected={function(s){console.log(s);}}/>;
  }
});

var App = React.createClass({
  render: function(){
    return <DirectionsStatus />;
  }
});

React.render(<App />, document.getElementById("app"));
