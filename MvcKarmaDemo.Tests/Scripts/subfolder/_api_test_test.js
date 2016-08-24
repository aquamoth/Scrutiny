(function() {
	"use strict";

    describe("API", function () {

        it("defines its base url before test scripts run", function () {
            expect(window.__SCRUTINY.API_BASEURL).to.not.be(undefined);
        });

        it("returns JSON when queried", function (done) {
            ajaxGetJSON(window.__SCRUTINY.API_BASEURL + 'test/673821', function (response) {
                expect(response).to.eql({ id: '673821' });
                done();
            });
        });
    });

    function ajaxGetJSON(url, successCallback) {
        var xmlhttp = new XMLHttpRequest();

        xmlhttp.onreadystatechange = function () {
            if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
                var myArr = JSON.parse(xmlhttp.responseText);
                successCallback(myArr);
            }
        };

        xmlhttp.open("GET", url, true);
        xmlhttp.send();
    }
}());