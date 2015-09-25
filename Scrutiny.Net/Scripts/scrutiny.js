(function(){

    $(function () {
        window.rpc.run(onConnected, onResponse, onDisconnected);

        function onConnected() {
            $('#banner').removeClass('offline').addClass('online');
            $('#title').text('Scrutiny - started');
        }

        function onResponse(commands) {
            console.log('RPC got response');
            debugger;
            console.log(commands);
        }

        function onDisconnected() {
            $('#banner').removeClass('online').addClass('offline');
            $('#title').text('Scrutiny - stopped');
        }
    });

    // This optional function html-encodes messages for display in the page.
    function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }

}());
