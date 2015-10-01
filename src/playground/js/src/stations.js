import $ from "jquery";

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
    loadFavourites: () => $.getJSON("/api/stations/favourite")
};

export { Stations };