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

function update(favourites){
    $.ajax({
        method: "PUT",
        url: `/api/user/${user.id}/favourite`,
        data: JSON.stringify(favourites)
    }).done(_ => subject.onNext(favourites));
}

export default class Directions {

    static favourites = replay;

    static addFavourite(direction){
        favourites = favourites.concat([direction]);
        update(favourites);
    }

    static removeFavourite(direction){
        const name = direction.name.toLowerCase();
        favourites = favourites.filter(f => f.name.toLowerCase() != name);
        update(favourites);
    }
}
