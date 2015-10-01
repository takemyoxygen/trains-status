import React from "react";

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
                <span className={`departure-time ${delayed ? "delayed" : ""}`}>
                    {`(${this.formatDate(this.props.stop.time)}${delayed ? (" " + this.props.stop.delay) : ""})`}
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
                break;
        }

        return <span className={`glyphicon ${iconType}`}></span>
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

export default class TravelOptions extends React.Component{
    render = () => (
        <div className="travel-options">
            {this.props.travelOptions.map(option => <TravelOption option={option} />)}
        </div>
    )
};
