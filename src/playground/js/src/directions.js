import Auth from "auth"
import Rx from "rxjs"

let user = null;
let favourites = [];

const subject = new Rx.Subject();
const replay = subject.replay(null, 1);
const connection = replay.connect();
const sync = Auth
    .currentUser
    .do(u => user = u)
    .selectMany(user => user.loggedIn
        ? $.getJSON(`/api/user/${user.id}/favourite`)
        : Promise.resolve([]))
    .subscribe(favs => {
        favourites = favs;
        subject.onNext(favourites);
    });

export default class Directions {

    static favourites = replay;

    static addFavourite(direction){
        favourites = favourites.concat([direction]);
        subject.onNext(favourites);
    }

    static remoteFavourite(direction){
        const name = direction.name.toLowerCase();
        favourites = favourites.map(f => f.name.toLowerCase() != name);
        subject.onNext(favourites);
    }
}
