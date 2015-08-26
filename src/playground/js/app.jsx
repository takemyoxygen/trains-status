var StationsList = React.createClass({
    render: function(){
      return <div>
              <select>
                {this.props.stations.map(function(station){
                  return <option value={station.name}>{station.name}</option>;
                })}
              </select>
            </div>;
    }
});

var StatusCheck = React.createClass({
    getInitialState: function(){
      return {stations: []};
    },
    componentDidMount: function(){
      $.getJSON("/api/stations").done(function(stations){
        if (this.isMounted()){
            this.setState({stations: stations});
        }
      }.bind(this));
    },
    render: function(){
      return <div>
              <StationsList stations={this.state.stations}/>
              <StationsList stations={this.state.stations}/>
              <div>
                <input type="button" className="btn" value="Check status"/>
              </div>
            </div>;
    }
});

var App = React.createClass({
  render: function(){
    return <StatusCheck />;
  }
});

React.render(<App />, document.getElementById("app"));
