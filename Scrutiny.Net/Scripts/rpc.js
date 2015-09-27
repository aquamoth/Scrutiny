(function () {
    "use strict";
    var exports = window.rpc = {};

    var _connectionId = null;
    var _onConnected, _onDisconnected;
    var _autoRestartPolling = false;

    var callbacks = [];


    exports.run = function (onConnected, onDisconnected) {
        _onConnected = isFunction(onConnected) ? onConnected : null;
        _onDisconnected = isFunction(onDisconnected) ? onDisconnected : null;

        _autoRestartPolling = true;
        poll();
    };

    exports.on = function (name, callback) {
        var itemsForName = callbacks.filter(function (item) { return item.name === name; });
        var list;
        if (itemsForName.length === 0)
        {
            list = [];
            callbacks.push({ name: name, items: list });
        }
        else {
            debugger;
            list = itemsForName[0].items;
        }
        list.push(callback);
        return this;
    }

    exports.emit = function (message, args) {
        //TODO: Implement emit()!
        console.error("RPC does not yet support emitting data.");
        console.log(message);
        console.log(args);
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

        if (response.commands) {
            onResponse(response.commands);
        }
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

    function onResponse(commands) {
        $.each(commands, function (i, command) {
            var fn = callbacks
                .filter(function (item) { return item.name === command.Name; })
                .map(function (item) { return item.items; });
            var fn = [].concat.apply([], fn); //Flattens array of arrays
            $.each(fn, function (j, callback) {
                callback(command.Data);
            });
        });
    }

    function isFunction(f) { return typeof (f) === 'function'; }

}());