(function () {
    "use strict";
    var exports = window.rpc = {};

    var _connectionId = null;
    var _onConnected, _onResponse, _onDisconnected;
    var _autoRestartPolling = false;

    exports.run = function (onConnected, onResponse, onDisconnected) {
        _onConnected = isFunction(onConnected) ? onConnected : null;
        _onResponse = isFunction(onResponse) ? onResponse : null;
        _onDisconnected = isFunction(onDisconnected) ? onDisconnected : null;

        _autoRestartPolling = true;
        poll();
    };

    function poll() {
        console.log((new Date).toLocaleTimeString() + ' poll started for ' + _connectionId);
        $.ajax({
            url: '/scrutiny/rpc/poll',
            type: 'post',
            data: { id: _connectionId },
            dataType: 'json',
            success: onPollSuccess,
            error: onPollError,
            complete: function () {
                if (_autoRestartPolling === true) {
                    poll();
                }
            }
        });
    }

    function onPollSuccess(response, status, xhr) {
        console.log(response);

        if (_connectionId === null) {
            _connectionId = response.id;
            console.log((new Date).toLocaleTimeString() + ' new id = ' + _connectionId);
            if (_onConnected !== null)
                _onConnected(_connectionId);
        }

        if (response.commands && _onResponse !== null)
            _onResponse(response.commands);
    }

    function onPollError(xhr, status, error) {
        console.warn((new Date).toLocaleTimeString() + ' poll error for ' + _connectionId);
        console.warn('Server responded with status ' + status);
        console.warn(error);
        _autoRestartPolling = false;
        console.warn("Disabled automatic restarting polling since there was a real server error.");
        if (_onDisconnected !== null)
            _onDisconnected();
    }

    function isFunction(f) { return typeof (f) === 'function'; }

}());