import $ from "jquery";

var cachedStations = null;

var Stations = {
    loadNearby: () => $.Deferred(deferred => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function(location){
                var url = `/api/stations/nearby?count=5&lat=${location.coords.latitude}&lon=${location.coords.longitude}`;
                $.getJSON(url).then(deferred.resolve, deferred.reject)
            });
        } else {
            deferred.reject("Geolocation is not supported by this browser.");
        }
    }),

    loadFavourites: (id) => $.getJSON(`/api/user/${id}/favourite`),

    loadAll: () => {
        if (!cachedStations) {
            cachedStations = $.getJSON("/api/stations/all")
        };

        return cachedStations;
    }
};

export { Stations };
