(function(Stations){
  "use strict";

  var endpoint = "/api/stations/nearby?count=5"

  Stations.load = function(){
    return $.Deferred(function(deferred){
      if (navigator.geolocation) {
          navigator.geolocation.getCurrentPosition(function(location){
            var url = endpoint + "&lat=" + location.coords.latitude + "&lon=" + location.coords.longitude;
            $.getJSON(url).then(deferred.resolve, deferred.reject)
          });
      } else {
          deferred.reject("Geolocation is not supported by this browser.");
      }
    });
  }
})(window.Stations = window.Stations || {});
