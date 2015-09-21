(function(){

    $(function () {
        console.log('Starting');
        window.rpc.run(onStarted, onResponse, onStopped);


        function onStarted() {
            console.log('RPC started');
        }
        function onResponse() {
            console.log('RPC got response');
        }
        function onStopped() {
            console.log('RPC stopped');
        }
    });

    // This optional function html-encodes messages for display in the page.
    function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }

}());
