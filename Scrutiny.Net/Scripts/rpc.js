(function () {
    "use strict";
    var io = window.io = {};

    //window.console.clear = function () { console.log('Console clear is DEBUG suppressed.');}

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
            var manager = this;
            if (manager.socket !== null) {
                //console.warn("RPC disconnecting.");
                manager._autoRestartPolling = false;
                if (manager.socket) {
                    if (manager.socket.id)
                        sendEvent.call(manager, 'disconnect');
                    else {
                        //TODO: Connect failed event?
                    }
                    manager.socket = null;
                }
            }
        }

        this.on = function (eventName, callback) {
            if (!isFunction(callback)) {
                throw "Listener for '" + eventName + "' must be a function!";
            }

            //console.log('RPC Manager registers listener for: ' + eventName);
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

        this.emitQueue = [];
        this.emitIsRunning = false;
        this.emit = function (message, args) {
            var manager = this;

            if (manager.socket === null)
                throw "No socket is currently connected!";

            var data = { id: manager.socket.id, message: message, args: args };
            manager.emitQueue.push(data);
            //console.log('Emit queue has length: ' + manager.emitQueue.length);
            tryRunEmitter();

            function tryRunEmitter() {
                if (!manager.emitIsRunning) {
                    manager.emitIsRunning = true;
                    runEmitter();
                }
            }

            function runEmitter() {
                if (manager.emitQueue.length === 0) {
                    manager.emitIsRunning = false;
                    return;
                }

                var data = manager.emitQueue.shift();
                console.log((new Date).toLocaleTimeString() + ' emitting "' + data.message + '":');
                console.log(data.args);
                $.ajax({
                    url: manager._url,
                    type: 'post',
                    data: data,
                    dataType: 'json',
                    success: onEmitSuccess,
                    error: onEmitError,
                    complete: function () {
                        runEmitter();
                    }
                });
            }

            function onEmitSuccess(response, status, xhr) {
                //console.log((new Date).toLocaleTimeString() + ' emit succeeded');
                //console.log(response);
            }

            function onEmitError(xhr, status, error) {
                console.error(xhr.responseText);

                console.warn("Disconnecting due to unhandled emit error.");
                manager.disconnect();
            }
        };













        function poll() {
            var manager = this;

            var data = null;
            if (manager.socket !== null) {
                //console.log((new Date).toLocaleTimeString() + ' poll continued for ' + manager.socket.id);
                data = { id: manager.socket.id };
            }
            //else
            //{
            //    console.log((new Date).toLocaleTimeString() + ' poll initiated');
            //}

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
                //console.error((new Date).toLocaleTimeString() + ' poll returned ' + error);
                //console.error('Server responded with status ' + status);
                //console.error(xhr.responseText);
                console.warn("Disconnected since there was an unhandled poll error.");
                manager.disconnect();
            }
        }

        function sendEvent(eventName, args) {
            console.log((new Date).toLocaleTimeString() + ' throwing event "' + eventName + '":');
            if (args)
                console.log(args);

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