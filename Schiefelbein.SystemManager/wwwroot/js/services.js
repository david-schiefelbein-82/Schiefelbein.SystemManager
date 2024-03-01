function createServiceManager(urlInfo) {

    var _refreshUrl = urlInfo['refresh-url'];
    var _startUrl = urlInfo['start-url'];
    var _stopUrl = urlInfo['stop-url'];
    var _favouriteUrl = urlInfo['favourite-url'];
    var _waitForStatusUrl = urlInfo['wait-for-status-url'];

    function _startService(serverName, serviceName) {
        return $.ajax({
            type: "POST",
            url: _startUrl,
            data: { "serverName": serverName, "serviceName": serviceName },
            dataType: "json",
        });
    }

    function _stopService(serverName, serviceName) {
        return $.ajax({
            type: "POST",
            url: _stopUrl,
            data: { "serverName": serverName, "serviceName": serviceName },
            dataType: "json",
        });
    }

    function _favourite(serverName, serviceName, isFavourite) {
        return $.ajax({
            type: "POST",
            url: _favouriteUrl,
            data: { "serverName": serverName, "serviceName": serviceName, "isFavourite": isFavourite },
            dataType: "json",
        });
    }

    function _waitForServiceStatus(serverName, serviceName, serviceStatusus, timeout) {
        return $.ajax({
            type: "POST",
            url: _waitForStatusUrl,
            data: { "serverName": serverName, "serviceName": serviceName, "serviceStatusus": serviceStatusus, "timeout": timeout },
            dataType: "json",
        });
    }

    function _refreshServices() {
        return $.ajax({
            type: "GET",
            url: _refreshUrl,
            contentType: 'application/json',
            dataType: "json",
        });
    }

    return {
        "startService": _startService,
        "stopService": _stopService,
        "favourite": _favourite,
        "waitForServiceStatus": _waitForServiceStatus,
        "refresh": _refreshServices,
    };
}