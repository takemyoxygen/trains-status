import Auth from "auth"
import Rx from "rxjs"
import Stations from "stations"

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

const sortedFavourites = Rx.Observable
    .combineLatest(
        replay,
        Stations.origin,
        (favs, origin) => {
            console.log("Sorting fav directions");
            for (var i = 0; i < favs.length; i++){
                if (favs[i].name.toLowerCase() === origin.name.toLowerCase()){
                    const clone = favs.slice(0);
                    clone.splice(i, 1);
                    clone.push(favs[i]);
                    return clone;
                }
            }
            return favs;
        });

function update(favourites){
    $.ajax({
        method: "PUT",
        url: `/api/user/${user.id}/favourite`,
        data: JSON.stringify(favourites)
    }).done(_ => subject.onNext(favourites));
}

export default class Directions {

    static favourites = sortedFavourites

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
