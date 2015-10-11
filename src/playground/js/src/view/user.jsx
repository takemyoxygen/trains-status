import React from "react";
import $ from "jquery";
import Rx from "rxjs";
import Auth from "auth";

export default class User extends React.Component{

    constructor(){
        super();
        this.state = {user: {}, initialized: false};
        Auth.currentUser.subscribe(user => this.setState({user: user, initialized: true}));
    }

    componentWillMount(){
        $('head').append(`<script src="https://apis.google.com/js/platform.js" async defer></script>`);
    }

    componentDidMount(){
        Auth.initialize().then(() => this.renderSignIn());
    }

    renderSignIn(){
        gapi.signin2.render('sign-in', {
            'scope': 'profile',
            'width': 100,
            'height': 30,
            'longtitle': false,
            'theme': 'dark',
            'onsuccess': googleUser => {
                if(!this.state.user.loggedIn){
                    Auth.signIn(googleUser);
                }
            },
        });
    }

    signOut = () => Auth.signOut();

    render(){
        return (
            <div className="user">
                <div id="sign-in" className={`sign-in ${(!this.state.user.loggedIn && this.state.initialized) ? "": "hidden"}`}></div>
                <div className={`username ${this.state.user.loggedIn && this.state.initialized ? "": "hidden"}`}>
                    <span>Hello, {this.state.user.name}</span>
                    <a href="#" onClick={this.signOut}>Sign out</a>
                </div>
            </div>
        );
    }
}
