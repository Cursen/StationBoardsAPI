var boardRows = ko.observableArray();
var updateBoardTimerId = 0;
var getCount = 0;

function FlatDepartureBoard(boardRow) {
    this.stationName = boardRow.StationName;
    this.destination = boardRow.Destination;
    this.scheduledTime = new Date(boardRow.ScheduledTime).toTimeString().substring(0, 5);
    this.expected = boardRow.Expected;
    this.platform = boardRow.Platform;
    this.via = boardRow.Via;
}

var getDepartures = function () {
    console.log(`getting departure info, pass ${getCount++}`);

    var urlParams = new URLSearchParams(window.location.search);
    console.log("url params:" + urlParams);

    // change the url here to use your service or my externally hosted one!
    var webServiceUrl = "http://resteasysolutions.co.uk:8081/api/stations/GetDepartureBoard?" + urlParams;

    var request = $.get(
        webServiceUrl,
        function (data) {
            boardRows.removeAll();
            data.forEach(element => {
                var boardRow = new FlatDepartureBoard(element);
                boardRows.push(boardRow);
            })
        })
        .fail(function () {
            boardRows.removeAll();
        });
    
}

$(document).ready(function() {
    getDepartures();
    ko.applyBindings(boardRows);
	setInterval(getDepartures, 30*1000);
})