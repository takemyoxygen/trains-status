var Station = React.createClass({
    render: function(){
      return <div>{this.props.name}</div>
    }
  }
);

var Stations = React.createClass({
  render: function(){
    return (
      <div>
        {this.props.stations.map(function(s){
          return <Station name={s.name} />;
        })}
      </div>
    );
  }
});

var App = React.createClass({
  getInitialState: function() {
    var that = this;
    $.getJSON("/api/stations").done(function(stations){
      return that.setState({stations: stations});
    });
    return {stations: []};
  },
  render: function(){
    return <Stations stations={this.state.stations} />;
  }
});

React.render(<App />, document.getElementById("app"));
