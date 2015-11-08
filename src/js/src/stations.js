import $ from "jquery";
import Rx from "rxjs";

const loadNearbyStations = () => $.Deferred(deferred => {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function(location){
            var url = `/api/stations/nearby?count=5&lat=${location.coords.latitude}&lon=${location.coords.longitude}`;
            $.getJSON(url).then(deferred.resolve, deferred.reject)
        });
    } else {
        deferred.reject("Geolocation is not supported by this browser.");
    }
});

const originSubject = new Rx.Subject();
const replay = originSubject.replay(null, 1);
const connection = replay.connect();
let pendingLoadingClosest = true;

const originsStream = Rx.Observable
    .create(
        observer => {
            if (pendingLoadingClosest){
                pendingLoadingClosest = false;

                let originOverridden = false;
                const overridesWatcher = originSubject
                    .subscribe(() => {
                        originOverridden = true;
                        overridesWatcher.dispose();
                    });

                loadNearbyStations()
                    .then(
                        nearby => {
                            if (nearby.length > 0 && !originOverridden){
                                originSubject.onNext(nearby[0]);
                            }
                            overridesWatcher.dispose();

                        },
                        error => {
                            originSubject.onError(error);
                            overridesWatcher.dispose();
                        });
            }

            const subjectSubscription = replay.subscribe(observer);

            return () => {
                subjectSubscription.dispose();
                overridesWatcher.dispose();
            };
        }
    );

var cachedStations = null;

export default class Stations {

    static origin = originsStream

    static setOrigin = station => originSubject.onNext(station)

    static loadFavourites = id => $.getJSON(`/api/user/${id}/favourite`)

    static loadAll = () => cachedStations || (cachedStations =  $.getJSON("/api/stations/all"))
};
