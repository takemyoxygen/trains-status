import React from "react";
import $ from "jquery";
import Rx from "rxjs";

export default class User extends React.Component{

    constructor(){
        super();
        this.state = {loggedIn: false};
    }

    componentWillMount(){
        $('head').append(`<meta name="google-signin-client_id" content="1070118148604-54uf2ocdsog6qsbigomu22v42aujahht.apps.googleusercontent.com">`);
        $('head').append(`<script src="https://apis.google.com/js/platform.js" async defer></script>`);
    }

    componentDidMount(){
        Rx.Observable
            .interval(100)
            .skipWhile (() => typeof gapi === "undefined")
            .take(1)
            .subscribe(() => this.renderSignIn())
    }

    renderSignIn(){
        gapi.signin2.render('sign-in', {
            'scope': 'https://www.googleapis.com/auth/plus.login',
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
                <div id="sign-in" className={this.state.loggedIn ? "hidden": ""}></div>
                <div className={`username ${this.state.loggedIn ? "": "hidden"}`}>
                    <span>Hello {this.state.username}</span>
                    <a href="#" onClick={this.signOut}>Sign out</a>
                </div>
            </div>
        );
    }
}
