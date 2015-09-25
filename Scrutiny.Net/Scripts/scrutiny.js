(function(){

    $(function () {
        window.rpc.run(onConnected, onResponse, onDisconnected);

        function onConnected() {
            $('#banner').removeClass('offline').addClass('online');
            $('#title').text('Scrutiny - started');
        }

        function onDisconnected() {
            $('#banner').removeClass('online').addClass('offline');
            $('#title').text('Scrutiny - stopped');
        }

        function onResponse(commands) {
            $.each(commands, function () {
                switch (this.Name) {
                    case 'Clients': onClientsUpdate(this.Data); break;
                    default:
                        console.error('Unsupported command: ' + this.Name);
                        console.log(this.Data);
                        break;
                }
            });
        }

        function onClientsUpdate(data) {
            var html = data.map(function (browser) {
                return '<li>' + browser + '</li>';
            });
            $('#browsers').html(html);
        }
    });

    // This optional function html-encodes messages for display in the page.
    function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }

}());
