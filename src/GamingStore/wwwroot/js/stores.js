const iconBase =
    "https://maps.google.com/mapfiles/kml/pushpin/";
const icons = {
    regularStore: {
        icon: iconBase + "red-pushpin.png",
    },
    websiteStore: {
        icon: iconBase + "ylw-pushpin.png",
    }
};

var storeIndex = 0;
var map;
var geocoder;

// Initialize and add the map
function initMap() {
    // The location of Uluru
    const dizi = { lat: 32.0750224, lng: 34.7749395};
    // The map, centered at Uluru
    const map = new google.maps.Map(document.getElementById("map"), {
        zoom: 4,
        center: dizi,
    });
    // The marker, positioned at Uluru
    const marker = new google.maps.Marker({
        position: dizi,
        map: map,
    });
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}
