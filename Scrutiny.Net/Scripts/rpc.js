(function () {
    "use strict";
    var io = window.io = {};

    io.connect = function (url, options) {
        var manager = new Manager(url, options);
        manager.connect();
        return manager;
    };


    function Manager(url, options) {

        this._options = options || {};
        this._url = url + '/' + (options.resource || ''); //TODO: This is probably NOT accoring to Socket.IO, where I think resource is an internal namespace for multiplexing

        this._autoRestartPolling = false;
        this._callbacks = [];

        this.socket = null;





        this.connect = function () {
            this._autoRestartPolling = true;
            poll.call(this);
        };

        this.disconnect = function () {
            if (this.socket !== null) {
                this.socket._autoRestartPolling = false;
            }
        }

        this.on = function (eventName, callback) {
            if (!isFunction(callback)) {
                debugger;
                throw "Listener for '" + eventName + "' must be a function!";
            }

            console.log('RPC Manager registers listener for: ' + eventName);
            var callbacksForEvent = this._callbacks.filter(function (event) { return event.name === eventName; });
            var list;
            if (callbacksForEvent.length === 0)
            {
                list = [];
                this._callbacks.push({ name: eventName, callbacks: list });
            }
            else {
                list = callbacksForEvent[0].callbacks;
            }
            list.push(callback);
            return this;
        }

        this.emit = function (message, args) {
            var manager = this;

            if (manager.socket === null)
                throw "No socket is currently connected!";

            var data = { id: manager.socket.id, message: message, args: args };
            console.log('Emitting:');
            console.log(data);
            $.ajax({
                url: manager._url,
                type: 'post',
                data: data,
                dataType: 'json',
                success: onEmitSuccess,
                error: onEmitError
            });

            function onEmitSuccess(response, status, xhr) {
                //console.log((new Date).toLocaleTimeString() + ' emit succeeded');
                //console.log(response);
            }

            function onEmitError(xhr, status, error) {
                debugger;
                console.warn((new Date).toLocaleTimeString() + ' emit error');
                console.warn('Server responded with status ' + status);
                console.warn(error);

                manager.disconnect();
                console.warn("Disabled automatic restarting polling since there was a real server error.");
            }
        };













        function poll() {
            var manager = this;

            var data = null;
            if (manager.socket === null){
                console.log((new Date).toLocaleTimeString() + ' poll initiated');
            }
            else
            {
                console.log((new Date).toLocaleTimeString() + ' poll continued for ' + manager.socket.id);
                data = { id: manager.socket.id };
            }

            $.ajax({
                url: manager._url,
                type: 'post',
                data: data,
                dataType: 'json',
                success: onPollSuccess,
                error: onPollError,
                complete: function () {
                    if (manager._autoRestartPolling === true) {
                        poll.call(manager);
                    }
                }
            });

            function onPollSuccess(response, status, xhr) {
                console.log(response);

                if (manager.socket === null) {
                    manager.socket = { id: response.id, transport: { name: 'http' } };
                    console.log((new Date).toLocaleTimeString() + ' new id = ' + manager.socket.id);
                    sendEvent.call(manager, 'connect', null);
                }

                if (response.commands) {
                    $.each(response.commands, function (i, command) {
                        sendEvent.call(manager, command.Name, command.Args);
                    });
                }
            }

            function onPollError(xhr, status, error) {
                console.warn((new Date).toLocaleTimeString() + ' poll error for ' + manager.socket.id);
                console.warn('Server responded with status ' + status);
                console.warn(error);
                manager._autoRestartPolling = false;
                console.warn("Disabled automatic restarting polling since there was a real server error.");

                debugger;
                if (manager.socket.id) {
                    this.socket = null;
                    sendEvent.call(manager, 'disconnect');
                }
            }
        }

        function sendEvent(eventName, args) {
            var eventCallbacks = this._callbacks
                .filter(function (event) { return event.name === eventName; })
                .map(function (event) { return event.callbacks; });
            var eventCallbacks = [].concat.apply([], eventCallbacks); //Flattens array of arrays
            $.each(eventCallbacks, function (j, callback) {
                callback(args);
            });
        }
    };


    function isFunction(f) { return typeof (f) === 'function'; }

}());