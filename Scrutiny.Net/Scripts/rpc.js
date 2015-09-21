(function () {
    "use strict";
    var exports = window.rpc = {};

    var _connectionId;
    var _onResponse, _onStopped;
    var _autoRestartPolling = false;

    exports.run = function (onStarted, onResponse, onStopped) {
        connect(function (response) {
            _connectionId = response;
            _onResponse = isFunction(onResponse) ? onResponse : null;
            _onStopped = isFunction(onStopped) ? onStopped : null;

            _autoRestartPolling = true;
            poll();

            if (isFunction(onStarted))
                onStarted(_connectionId);
        });
    };

    function connect(onSuccess) {
        $.ajax({
            url: '/scrutiny/rpc/register',
            type: 'post',
            success: function (response, status, xhr) {
                onSuccess(response);
            },
            error: function (xhr, status, error) {
                console.error("Failed to register with rpc server!");
            }
        })
    }

    function poll() {
        console.log((new Date).toLocaleTimeString() + ' poll started for ' + _connectionId);
        $.ajax({
            url: '/scrutiny/rpc/poll',
            type: 'post',
            data: { id: _connectionId },
            success: function (response, status, xhr) {
                console.log((new Date).toLocaleTimeString() + ' poll success for ' + _connectionId);
                if (_onResponse !== null)
                    _onResponse(response);
            },
            error: function (xhr, status, error) {
                console.warn((new Date).toLocaleTimeString() + 'poll error for ' + _connectionId);
                console.warn('Server responded with status ' + status);
                console.warn(error);
                _autoRestartPolling = false;
                console.warn("Disabled automatic restarting polling since there was a real server error.");
            },
            complete: function () {
                if (_autoRestartPolling === true) {
                    poll();
                }
            }
        });
    }

    function isFunction(f) { return typeof (f) === 'function'; }

}());