(function (global) {
    "use strict";
    global.wwp = global.wwp || {};
    var exports = global.wwp.home_index = {};

    exports.initDrawingArea = function (drawingArea) {
        console.log('initializing drawing area here in wwp');
        drawingArea.setAttribute("foo", "bar");
    }

    exports.initialize = function () {
        console.log('Window loading');
        var drawingArea = $('#wwp-drawingArea');
        exports.initDrawingArea(drawingArea);
    	console.log('Window loaded');
    };


}(this));