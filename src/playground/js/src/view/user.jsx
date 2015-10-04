import React from "react";
import $ from "jquery";
import Rx from "rxjs";

export default class User extends React.Component{

    constructor(){
        super();
        this.state = {loggedIn: false, initialized: false};
    }

    componentWillMount(){
        $('head').append(`<script src="https://apis.google.com/js/platform.js" async defer></script>`);
    }

    componentDidMount(){
        Rx.Observable
            .interval(100)
            .skipWhile (() => typeof gapi === "undefined")
            .take(1)
            .subscribe(() => this.initializeAuthentication());
    }

    initializeAuthentication(){
        gapi.load("auth2", () => {
            var auth2 = gapi.auth2.init({
                client_id: '1070118148604-54uf2ocdsog6qsbigomu22v42aujahht.apps.googleusercontent.com',
                scope: 'profile'
            });

            auth2.then(() => {
                console.log("[Initialized]: logged in - " + auth2.isSignedIn.get());
                this.setState({initialized: true, loggedIn: auth2.isSignedIn.get()});
            });
            this.renderSignIn();
        });
    }

    renderSignIn(){
        gapi.signin2.render('sign-in', {
            'scope': 'profile',
            'width': 100,
            'height': 30,
            'longtitle': false,
            'theme': 'dark',
            'onsuccess': this.signIn,
        });
    }

    signIn = googleUser => {
        var profile = googleUser.getBasicProfile();
        var token = googleUser.getAuthResponse().id_token;
        var name = profile.getName();
        $.get("/api/user/info?token=" + token)
            .then(() => this.setState({loggedIn: true, username: name}));
    }

    signOut = () => {
        gapi.auth2
            .getAuthInstance()
            .signOut()
            .then(() => {
                console.log('User signed out.');
                this.setState({loggedIn: false});
            });
    }

    render(){
        return (
            <div className="user">
                <div id="sign-in" className={`sign-in ${(!this.state.loggedIn && this.state.initialized) ? "": "hidden"}`}></div>
                <div className={`username ${this.state.loggedIn && this.state.initialized ? "": "hidden"}`}>
                    <span>Hello, {this.state.username}</span>
                    <a href="#" onClick={this.signOut}>Sign out</a>
                </div>
            </div>
        );
    }
}
