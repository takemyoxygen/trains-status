import Rx from "rxjs";
import $ from "jquery";

const userSubject = new Rx.Subject();
const replay = userSubject.replay(null, 1);
const connection = replay.connect();
const anonymousUser = {loggedIn: false};

function userFromGoogleUser(googleUser){
    const profile = googleUser.getBasicProfile();
    return {
        name: profile.getName(),
        id: profile.getId(),
        email: profile.getEmail(),
        loggedIn: true
    };
}

export default class Auth {

    static clientId = "1070118148604-54uf2ocdsog6qsbigomu22v42aujahht.apps.googleusercontent.com"

    static initialize(){
        return new Promise((resolve, reject) => Rx.Observable
            .interval(100)
            .skipWhile (() => typeof gapi === "undefined")
            .take(1)
            .toPromise()
            .then(() => {
                gapi.load("auth2", () => {
                    var auth2 = gapi.auth2.init({
                        client_id: this.clientId,
                        scope: "profile"
                    });

                    auth2.then(() => {
                        const signedIn = auth2.isSignedIn.get();
                        console.log("[Initialized]: logged in - " + signedIn);
                        if (signedIn){
                            const googleUser = auth2.currentUser.get();
                            const user = userFromGoogleUser(googleUser);
                            this.setCurrentUser(user);
                        } else {
                            this.setCurrentUser(anonymousUser);
                        }
                        resolve();
                    }, reject);
                });
            }, reject));
    }

    static currentUser = replay
        .do(user => {
            console.log("New user:");
            console.dir(user);
        });

    static setCurrentUser(user){
        userSubject.onNext(user);
    }

    static signIn(googleUser){
        const user = userFromGoogleUser(googleUser);
        var token = googleUser.getAuthResponse().id_token;
        $.getJSON("/api/user/info?token=" + token)
            .then(response => {
                user.id = response.id;
                this.setCurrentUser(user);
            });
    }

    static signOut(){
        gapi.auth2
            .getAuthInstance()
            .signOut()
            .then(() => {
                console.log('User signed out.');
                this.setCurrentUser(anonymousUser);
            });
    }
}
