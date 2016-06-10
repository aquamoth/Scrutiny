require=(function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({"./client.js":[function(require,module,exports){
// Copyright (c) 2012 Titanium I.T. LLC. All rights reserved. See LICENSE.txt for details.
/*global Raphael, $ */

(function() {
	"use strict";

	dump("Dump at start of tests");

	var SvgCanvas = require("./svg_canvas.js");
	var HtmlElement = require("./html_element.js");
	var browser = require("./browser.js");
	var failFast = require("./fail_fast.js");

	var svgCanvas = null;
	var start = null;
	var lineDrawn = false;
	var drawingArea;
	var clearScreenButton;
	var documentBody;
	var windowElement;
	var useSetCaptureApi = false;

	exports.initializeDrawingArea = function(elements) {
		if (svgCanvas !== null) throw new Error("Client.js is not re-entrant");

		drawingArea = elements.drawingAreaDiv;
		clearScreenButton = elements.clearScreenButton;

		failFast.unlessDefined(drawingArea, "elements.drawingArea");
		failFast.unlessDefined(clearScreenButton, "elements.clearScreenButton");

		documentBody = new HtmlElement(document.body);
		windowElement = new HtmlElement(window);

		svgCanvas = new SvgCanvas(drawingArea);

		drawingArea.preventBrowserDragDefaults();
		handleClearScreenClick();
		handleMouseDragEvents();
		handleTouchDragEvents();

		return svgCanvas;
	};

	exports.drawingAreaHasBeenRemovedFromDom = function() {
		svgCanvas = null;
	};

	function handleClearScreenClick() {
		clearScreenButton.onMouseClick(function() {
			svgCanvas.clear();
		});
	}

	function handleMouseDragEvents() {
		drawingArea.onMouseDown(startDrag);
		documentBody.onMouseMove(continueDrag);
		windowElement.onMouseUp(endDrag);

		if (browser.doesNotHandlesUserEventsOnWindow()) {
			drawingArea.onMouseUp(endDrag);
			useSetCaptureApi = true;
		}
	}

	function handleTouchDragEvents() {
		drawingArea.onSingleTouchStart(startDrag);
		drawingArea.onSingleTouchMove(continueDrag);
		drawingArea.onTouchEnd(endDrag);
		drawingArea.onTouchCancel(endDrag);

		drawingArea.onMultiTouchStart(endDrag);
	}

	function startDrag(pageOffset) {
		start = drawingArea.relativeOffset(pageOffset);
    if (useSetCaptureApi) drawingArea.setCapture();
	}

	function continueDrag(pageOffset) {
		if (!isCurrentlyDrawing()) return;

		var end = drawingArea.relativeOffset(pageOffset);
		if (start.x !== end.x || start.y !== end.y) {
			svgCanvas.drawLine(start.x, start.y, end.x, end.y);
			start = end;
			lineDrawn = true;
		}
	}

	function endDrag() {
		if (!isCurrentlyDrawing()) return;

		if (!lineDrawn) svgCanvas.drawDot(start.x, start.y);

		if (useSetCaptureApi) drawingArea.releaseCapture();
		start = null;
		lineDrawn = false;
	}

	function isCurrentlyDrawing() {
		return start !== null;
	}

}());
},{"./browser.js":1,"./fail_fast.js":2,"./html_element.js":undefined,"./svg_canvas.js":3}],"./html_element.js":[function(require,module,exports){
// Copyright (c) 2013 Titanium I.T. LLC. All rights reserved. See LICENSE.txt for details.
/*global $, jQuery, TouchList, Touch */

(function() {
	"use strict";

	var browser = require("./browser.js");
	var failFast = require("./fail_fast.js");

	var capturedElement = null;


	/* Constructors */

	var HtmlElement = module.exports = function(domElement) {
		var self = this;

		self._domElement = domElement;
		self._element = $(domElement);
		self._dragDefaultsPrevented = false;
	};

	HtmlElement.fromHtml = function(html) {
		return new HtmlElement($(html)[0]);
	};

	HtmlElement.fromId = function(id) {
		var domElement = document.getElementById(id);
		failFast.unlessTrue(domElement !== null, "could not find element with id '" + id + "'");
		return new HtmlElement(domElement);
	};

	/* Capture API */

	HtmlElement.prototype.setCapture = function() {
		capturedElement = this;
		this._domElement.setCapture();
	};

	HtmlElement.prototype.releaseCapture = function() {
		capturedElement = null;
		this._domElement.releaseCapture();
	};


	/* General event handling */

	HtmlElement.prototype.removeAllEventHandlers = function() {
		this._element.off();
	};

	HtmlElement.prototype.preventBrowserDragDefaults = function() {
		this._element.on("selectstart", preventDefaults);
		this._element.on("mousedown", preventDefaults);
		this._element.on("touchstart", preventDefaults);

		this._dragDefaultsPrevented = true;

		function preventDefaults(event) {
			event.preventDefault();
		}
	};

	HtmlElement.prototype.isBrowserDragDefaultsPrevented = function() {
		return this._dragDefaultsPrevented;
	};

	/* Mouse events */
	HtmlElement.prototype.triggerMouseClick = triggerMouseEventFn("click");
	HtmlElement.prototype.triggerMouseDown = triggerMouseEventFn("mousedown");
	HtmlElement.prototype.triggerMouseMove = triggerMouseEventFn("mousemove");
	HtmlElement.prototype.triggerMouseLeave = triggerMouseEventFn("mouseleave");
	HtmlElement.prototype.triggerMouseUp = triggerMouseEventFn("mouseup");

	HtmlElement.prototype.onMouseClick = onMouseEventFn("click");
	HtmlElement.prototype.onMouseDown = onMouseEventFn("mousedown");
	HtmlElement.prototype.onMouseMove = onMouseEventFn("mousemove");
	HtmlElement.prototype.onMouseLeave = onMouseEventFn("mouseleave");
	HtmlElement.prototype.onMouseUp = onMouseEventFn("mouseup");

	function triggerMouseEventFn(event) {
		return function(relativeX, relativeY) {
			var targetElement = capturedElement || this;

			var pageCoords;
			if (relativeX === undefined || relativeY === undefined) {
				pageCoords = { x: 0, y: 0 };
			}
			else {
				pageCoords = pageOffset(this, relativeX, relativeY);
			}

			sendMouseEvent(targetElement, event, pageCoords);
		};
	}

	function onMouseEventFn(event) {
		return function(callback) {
			if (browser.doesNotHandlesUserEventsOnWindow() && this._domElement === window) return;

			this._element.on(event, function(event) {
				var pageOffset = { x: event.pageX, y: event.pageY };
				callback(pageOffset);
			});
		};
	}

	function sendMouseEvent(self, event, pageCoords) {
		var jqElement = self._element;

		var eventData = new jQuery.Event();
		eventData.pageX = pageCoords.x;
		eventData.pageY = pageCoords.y;
		eventData.type = event;
		jqElement.trigger(eventData);
	}


	/* Touch events */

	HtmlElement.prototype.triggerTouchEnd = triggerZeroTouchEventFn("touchend");
	HtmlElement.prototype.triggerTouchCancel = triggerZeroTouchEventFn("touchcancel");
	HtmlElement.prototype.triggerSingleTouchStart = triggerSingleTouchEventFn("touchstart");
	HtmlElement.prototype.triggerSingleTouchMove = triggerSingleTouchEventFn("touchmove");
	HtmlElement.prototype.triggerMultiTouchStart = triggerMultiTouchEventFn("touchstart");

	HtmlElement.prototype.onTouchEnd = onZeroTouchEventFn("touchend");
	HtmlElement.prototype.onTouchCancel = onZeroTouchEventFn("touchcancel");
	HtmlElement.prototype.onSingleTouchStart = onSingleTouchEventFn("touchstart");
	HtmlElement.prototype.onSingleTouchMove = onSingleTouchEventFn("touchmove");
	HtmlElement.prototype.onMultiTouchStart = onMultiTouchEventFn("touchstart");

	function triggerZeroTouchEventFn(event) {
		return function() {
			sendTouchEvent(this, event, new TouchList());
		};
	}

	function triggerSingleTouchEventFn(event) {
		return function(relativeX, relativeY) {
			var touch = createTouch(this, relativeX, relativeY);
			sendTouchEvent(this, event, new TouchList(touch));
		};
	}

	function triggerMultiTouchEventFn(event) {
		return function(relative1X, relative1Y, relative2X, relative2Y) {
			var touch1 = createTouch(this, relative1X, relative1Y);
			var touch2 = createTouch(this, relative2X, relative2Y);
			sendTouchEvent(this, event, new TouchList(touch1, touch2));
		};
	}


	function onZeroTouchEventFn(event) {
		return function(callback) {
			this._element.on(event, function() {
				callback();
			});
		};
	}

	function onSingleTouchEventFn(eventName) {
		return function(callback) {
			this._element.on(eventName, function(event) {
				var originalEvent = event.originalEvent;
				if (originalEvent.touches.length !== 1) return;

				var pageX = originalEvent.touches[0].pageX;
				var pageY = originalEvent.touches[0].pageY;
				var offset = { x: pageX, y: pageY };

				callback(offset);
			});
		};
	}

	function onMultiTouchEventFn(event) {
		return function(callback) {
			var self = this;
			this._element.on(event, function(event) {
				var originalEvent = event.originalEvent;
				if (originalEvent.touches.length !== 1) callback();
			});
		};
	}

	function sendTouchEvent(self, event, touchList) {
		var touchEvent = document.createEvent("TouchEvent");
		touchEvent.initTouchEvent(
			event, // event type
			true, // canBubble
			true, // cancelable
			window, // DOM window
			null, // detail (not sure what this is)
			0, 0, // screenX/Y
			0, 0, // clientX/Y
			false, false, false, false, // meta keys (shift etc.)
			touchList, touchList, touchList
		);

		var eventData = new jQuery.Event("event");
		eventData.type = event;
		eventData.originalEvent = touchEvent;
		self._element.trigger(eventData);
	}

	function createTouch(self, relativeX, relativeY) {
		var offset = pageOffset(self, relativeX, relativeY);

		var target = self._element[0];
		var identifier = 0;
		var pageX = offset.x;
		var pageY = offset.y;
		var screenX = 0;
		var screenY = 0;

		return new Touch(undefined, target, identifier, pageX, pageY, screenX, screenY);
	}


	/* Dimensions, offsets, and positioning */

	HtmlElement.prototype.getDimensions = function() {
		return {
			width: this._element.width(),
			height: this._element.height()
		};
	};

	HtmlElement.prototype.relativeOffset = function(pageOffset) {
		return relativeOffset(this, pageOffset.x, pageOffset.y);
	};

	HtmlElement.prototype.pageOffset = function(relativeOffset) {
		return pageOffset(this, relativeOffset.x, relativeOffset.y);
	};

	function relativeOffset(self, pageX, pageY) {
		failFastIfStylingPresent(self);

		var pageOffset = self._element.offset();
		return {
			x: pageX - pageOffset.left,
			y: pageY - pageOffset.top
		};
	}

	function pageOffset(self, relativeX, relativeY) {
		failFastIfStylingPresent(self);

		var topLeftOfDrawingArea = self._element.offset();
		return {
			x: relativeX + topLeftOfDrawingArea.left,
			y: relativeY + topLeftOfDrawingArea.top
		};
	}

	function failFastIfStylingPresent(self) {
		failFastIfPaddingPresent("top");
		failFastIfPaddingPresent("left");
		failFastIfBorderPresent("top");
		failFastIfBorderPresent("left");

		function failFastIfPaddingPresent(side) {
			var css = self._element.css("padding-" + side);
			if (css !== "0px") throw new Error("Do not apply padding to elements used with relativeOffset() (expected 0px but was " + css + ")");
		}

		function failFastIfBorderPresent(side) {
			var css = self._element.css("border-" + side + "-width");
			if (browser.doesNotComputeStyles()) {
				if (self._element.css("border-" + side + "-style") === "none") css = "0px";
			}

			if (css !== "0px") throw new Error("Do not apply border to elements used with relativeOffset() (expected 0px but was " + css + ")");
		}
	}

	/* DOM Manipulation */

	HtmlElement.prototype.append = function(elementToAppend) {
		this._element.append(elementToAppend._element);
	};

	HtmlElement.prototype.appendSelfToBody = function() {
		$(document.body).append(this._element);
	};

	HtmlElement.prototype.remove = function() {
		this._element.remove();
	};

	HtmlElement.prototype.toDomElement = function() {
		return this._element[0];
	};

}());
},{"./browser.js":1,"./fail_fast.js":2}],1:[function(require,module,exports){
// Copyright (c) 2013 Titanium I.T. LLC. All rights reserved. See LICENSE.TXT for details.
/*global Modernizr, $ */

(function() {
	"use strict";

	exports.supportsTouchEvents = function() {
		return Modernizr.touch;
	};

	exports.supportsCaptureApi = function() {
		return document.body.setCapture && document.body.releaseCapture;
	};

	exports.reportsElementPositionOffByOneSometimes = function() {
		return isIe8();
	};

	exports.doesNotHandlesUserEventsOnWindow = function() {
		return isIe8();
	};

	exports.doesNotComputeStyles = function() {
		return isIe8();
	};

	function isIe8() {
		return $.browser.msie && $.browser.version === "8.0";
	}

}());
},{}],2:[function(require,module,exports){
// Copyright (c) 2013 Titanium I.T. LLC. All rights reserved. See LICENSE.TXT for details.
(function() {
	"use strict";

	exports.unlessDefined = function(variable, variableName) {
		variableName = variableName ? " [" + variableName + "] " : " ";
		if (variable === undefined) throw new FailFastException(exports.unlessDefined, "Required variable" + variableName + "was not defined");
	};

	exports.unlessTrue = function(variable, message) {
		if (message === undefined) message = "Expected condition to be true";

		if (variable === false) throw new FailFastException(exports.unlessTrue, message);
		if (variable !== true) throw new FailFastException(exports.unlessTrue, "Expected condition to be true or false");
	};

	exports.unreachable = function(message) {
		if (!message) message = "Unreachable code executed";

		throw new FailFastException(exports.unreachable, message);
	};

	var FailFastException = exports.FailFastException = function(fnToRemoveFromStackTrace, message) {
		if (Error.captureStackTrace) Error.captureStackTrace(this, fnToRemoveFromStackTrace);    // only works on Chrome/V8
		this.message = message;
	};
	FailFastException.prototype = new Error();
	FailFastException.prototype.constructor = FailFastException;
	FailFastException.prototype.name = "FailFastException";

}());
},{}],3:[function(require,module,exports){
// Copyright (c) 2013 Titanium I.T. LLC. All rights reserved. See LICENSE.TXT for details.
/*global Raphael */

(function() {
	"use strict";

	var SvgCanvas = module.exports = function(htmlElement) {
		var dimensions = htmlElement.getDimensions();
		this._paper = new Raphael(htmlElement.toDomElement(), dimensions.width, dimensions.height);
	};

	SvgCanvas.LINE_COLOR = "black";
	SvgCanvas.STROKE_WIDTH = 2;
	SvgCanvas.LINE_CAP = "round";

	SvgCanvas.prototype.clear = function() {
		this._paper.clear();
	};

	SvgCanvas.prototype.drawLine = function(startX, startY, endX, endY) {
		this._paper.path("M" + startX + "," + startY + "L" + endX + "," + endY)
			.attr({
				"stroke": SvgCanvas.LINE_COLOR,
				"stroke-width": SvgCanvas.STROKE_WIDTH,
				"stroke-linecap": SvgCanvas.LINE_CAP
			});
	};

	SvgCanvas.prototype.drawDot = function(x, y) {
		this._paper.circle(x, y, SvgCanvas.STROKE_WIDTH / 2)
			.attr({
				"stroke": SvgCanvas.LINE_COLOR,
				"fill": SvgCanvas.LINE_COLOR
			});
	};

	SvgCanvas.prototype.lineSegments = function() {
		var result = [];
		this._paper.forEach(function(element) {
			result.push(normalizeToLineSegment(element));
		});
		return result;
	};

	SvgCanvas.prototype.elementsForTestingOnly = function() {
		var result = [];
		this._paper.forEach(function(element) {
			result.push(element);
		});
		return result;
	};

	function normalizeToLineSegment(element) {
		switch (element.type) {
			case "path":
				return normalizePath(element);
			case "circle":
				return normalizeCircle(element);
			default:
				throw new Error("Unknown element type: " + element.type);
		}
	}

	function normalizeCircle(element) {
		return [
			element.attrs.cx,
			element.attrs.cy
		];
	}

	function normalizePath(element) {
		if (Raphael.svg) {
			return normalizeSvgPath(element);
		}
		else if (Raphael.vml) {
			return normalizeVmlPath(element);
		}
		else {
			throw new Error("Unknown Raphael rendering engine");
		}
	}

	function normalizeSvgPath(element) {
		var pathRegex;

		var path = element.node.attributes.d.value;
		if (path.indexOf(",") !== -1)
		// We're in Firefox, Safari, Chrome, which uses format "M20,30L30,300"
		{
			pathRegex = /M(\d+),(\d+)L(\d+),(\d+)/;
		}
		else {
			// We're in IE9, which uses format "M 20 30 L 30 300"
			pathRegex = /M (\d+) (\d+) L (\d+) (\d+)/;
		}
		var pathComponents = path.match(pathRegex);

		return [
			pathComponents[1],
			pathComponents[2],
			pathComponents[3],
			pathComponents[4]
		];
	}

	function normalizeVmlPath(element) {
		// We're in IE 8, which uses format "m432000,648000 l648000,67456800 e"
		var VML_MAGIC_NUMBER = 21600;

		var path = element.node.path.value;

		var ie8PathRegex = /m(\d+),(\d+) l(\d+),(\d+) e/;
		var ie8 = path.match(ie8PathRegex);

		var startX = ie8[1] / VML_MAGIC_NUMBER;
		var startY = ie8[2] / VML_MAGIC_NUMBER;
		var endX = ie8[3] / VML_MAGIC_NUMBER;
		var endY = ie8[4] / VML_MAGIC_NUMBER;

		return [
			startX,
			startY,
			endX,
			endY
		];
	}

}());
},{}]},{},[])
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIkM6XFxVc2Vyc1xcbWFhc1xcV29ya3NwYWNlXFxHaXRcXGxldHNfY29kZV9qYXZhc2NyaXB0XFxub2RlX21vZHVsZXNcXGJyb3dzZXJpZnlcXG5vZGVfbW9kdWxlc1xcYnJvd3Nlci1wYWNrXFxfcHJlbHVkZS5qcyIsIi4vc3JjL2NsaWVudC9jbGllbnQuanMiLCIuL3NyYy9jbGllbnQvaHRtbF9lbGVtZW50LmpzIiwiQzovVXNlcnMvbWFhcy9Xb3Jrc3BhY2UvR2l0L2xldHNfY29kZV9qYXZhc2NyaXB0L3NyYy9jbGllbnQvYnJvd3Nlci5qcyIsIkM6L1VzZXJzL21hYXMvV29ya3NwYWNlL0dpdC9sZXRzX2NvZGVfamF2YXNjcmlwdC9zcmMvY2xpZW50L2ZhaWxfZmFzdC5qcyIsIkM6L1VzZXJzL21hYXMvV29ya3NwYWNlL0dpdC9sZXRzX2NvZGVfamF2YXNjcmlwdC9zcmMvY2xpZW50L3N2Z19jYW52YXMuanMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7QUNBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUN0R0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDelNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQzlCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUM5QkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSIsImZpbGUiOiJnZW5lcmF0ZWQuanMiLCJzb3VyY2VSb290IjoiIiwic291cmNlc0NvbnRlbnQiOlsiKGZ1bmN0aW9uIGUodCxuLHIpe2Z1bmN0aW9uIHMobyx1KXtpZighbltvXSl7aWYoIXRbb10pe3ZhciBhPXR5cGVvZiByZXF1aXJlPT1cImZ1bmN0aW9uXCImJnJlcXVpcmU7aWYoIXUmJmEpcmV0dXJuIGEobywhMCk7aWYoaSlyZXR1cm4gaShvLCEwKTt2YXIgZj1uZXcgRXJyb3IoXCJDYW5ub3QgZmluZCBtb2R1bGUgJ1wiK28rXCInXCIpO3Rocm93IGYuY29kZT1cIk1PRFVMRV9OT1RfRk9VTkRcIixmfXZhciBsPW5bb109e2V4cG9ydHM6e319O3Rbb11bMF0uY2FsbChsLmV4cG9ydHMsZnVuY3Rpb24oZSl7dmFyIG49dFtvXVsxXVtlXTtyZXR1cm4gcyhuP246ZSl9LGwsbC5leHBvcnRzLGUsdCxuLHIpfXJldHVybiBuW29dLmV4cG9ydHN9dmFyIGk9dHlwZW9mIHJlcXVpcmU9PVwiZnVuY3Rpb25cIiYmcmVxdWlyZTtmb3IodmFyIG89MDtvPHIubGVuZ3RoO28rKylzKHJbb10pO3JldHVybiBzfSkiLCIvLyBDb3B5cmlnaHQgKGMpIDIwMTIgVGl0YW5pdW0gSS5ULiBMTEMuIEFsbCByaWdodHMgcmVzZXJ2ZWQuIFNlZSBMSUNFTlNFLnR4dCBmb3IgZGV0YWlscy5cclxuLypnbG9iYWwgUmFwaGFlbCwgJCAqL1xyXG5cclxuKGZ1bmN0aW9uKCkge1xyXG5cdFwidXNlIHN0cmljdFwiO1xyXG5cclxuXHR2YXIgU3ZnQ2FudmFzID0gcmVxdWlyZShcIi4vc3ZnX2NhbnZhcy5qc1wiKTtcclxuXHR2YXIgSHRtbEVsZW1lbnQgPSByZXF1aXJlKFwiLi9odG1sX2VsZW1lbnQuanNcIik7XHJcblx0dmFyIGJyb3dzZXIgPSByZXF1aXJlKFwiLi9icm93c2VyLmpzXCIpO1xyXG5cdHZhciBmYWlsRmFzdCA9IHJlcXVpcmUoXCIuL2ZhaWxfZmFzdC5qc1wiKTtcclxuXHJcblx0dmFyIHN2Z0NhbnZhcyA9IG51bGw7XHJcblx0dmFyIHN0YXJ0ID0gbnVsbDtcclxuXHR2YXIgbGluZURyYXduID0gZmFsc2U7XHJcblx0dmFyIGRyYXdpbmdBcmVhO1xyXG5cdHZhciBjbGVhclNjcmVlbkJ1dHRvbjtcclxuXHR2YXIgZG9jdW1lbnRCb2R5O1xyXG5cdHZhciB3aW5kb3dFbGVtZW50O1xyXG5cdHZhciB1c2VTZXRDYXB0dXJlQXBpID0gZmFsc2U7XHJcblxyXG5cdGV4cG9ydHMuaW5pdGlhbGl6ZURyYXdpbmdBcmVhID0gZnVuY3Rpb24oZWxlbWVudHMpIHtcclxuXHRcdGlmIChzdmdDYW52YXMgIT09IG51bGwpIHRocm93IG5ldyBFcnJvcihcIkNsaWVudC5qcyBpcyBub3QgcmUtZW50cmFudFwiKTtcclxuXHJcblx0XHRkcmF3aW5nQXJlYSA9IGVsZW1lbnRzLmRyYXdpbmdBcmVhRGl2O1xyXG5cdFx0Y2xlYXJTY3JlZW5CdXR0b24gPSBlbGVtZW50cy5jbGVhclNjcmVlbkJ1dHRvbjtcclxuXHJcblx0XHRmYWlsRmFzdC51bmxlc3NEZWZpbmVkKGRyYXdpbmdBcmVhLCBcImVsZW1lbnRzLmRyYXdpbmdBcmVhXCIpO1xyXG5cdFx0ZmFpbEZhc3QudW5sZXNzRGVmaW5lZChjbGVhclNjcmVlbkJ1dHRvbiwgXCJlbGVtZW50cy5jbGVhclNjcmVlbkJ1dHRvblwiKTtcclxuXHJcblx0XHRkb2N1bWVudEJvZHkgPSBuZXcgSHRtbEVsZW1lbnQoZG9jdW1lbnQuYm9keSk7XHJcblx0XHR3aW5kb3dFbGVtZW50ID0gbmV3IEh0bWxFbGVtZW50KHdpbmRvdyk7XHJcblxyXG5cdFx0c3ZnQ2FudmFzID0gbmV3IFN2Z0NhbnZhcyhkcmF3aW5nQXJlYSk7XHJcblxyXG5cdFx0ZHJhd2luZ0FyZWEucHJldmVudEJyb3dzZXJEcmFnRGVmYXVsdHMoKTtcclxuXHRcdGhhbmRsZUNsZWFyU2NyZWVuQ2xpY2soKTtcclxuXHRcdGhhbmRsZU1vdXNlRHJhZ0V2ZW50cygpO1xyXG5cdFx0aGFuZGxlVG91Y2hEcmFnRXZlbnRzKCk7XHJcblxyXG5cdFx0cmV0dXJuIHN2Z0NhbnZhcztcclxuXHR9O1xyXG5cclxuXHRleHBvcnRzLmRyYXdpbmdBcmVhSGFzQmVlblJlbW92ZWRGcm9tRG9tID0gZnVuY3Rpb24oKSB7XHJcblx0XHRzdmdDYW52YXMgPSBudWxsO1xyXG5cdH07XHJcblxyXG5cdGZ1bmN0aW9uIGhhbmRsZUNsZWFyU2NyZWVuQ2xpY2soKSB7XHJcblx0XHRjbGVhclNjcmVlbkJ1dHRvbi5vbk1vdXNlQ2xpY2soZnVuY3Rpb24oKSB7XHJcblx0XHRcdHN2Z0NhbnZhcy5jbGVhcigpO1xyXG5cdFx0fSk7XHJcblx0fVxyXG5cclxuXHRmdW5jdGlvbiBoYW5kbGVNb3VzZURyYWdFdmVudHMoKSB7XHJcblx0XHRkcmF3aW5nQXJlYS5vbk1vdXNlRG93bihzdGFydERyYWcpO1xyXG5cdFx0ZG9jdW1lbnRCb2R5Lm9uTW91c2VNb3ZlKGNvbnRpbnVlRHJhZyk7XHJcblx0XHR3aW5kb3dFbGVtZW50Lm9uTW91c2VVcChlbmREcmFnKTtcclxuXHJcblx0XHRpZiAoYnJvd3Nlci5kb2VzTm90SGFuZGxlc1VzZXJFdmVudHNPbldpbmRvdygpKSB7XHJcblx0XHRcdGRyYXdpbmdBcmVhLm9uTW91c2VVcChlbmREcmFnKTtcclxuXHRcdFx0dXNlU2V0Q2FwdHVyZUFwaSA9IHRydWU7XHJcblx0XHR9XHJcblx0fVxyXG5cclxuXHRmdW5jdGlvbiBoYW5kbGVUb3VjaERyYWdFdmVudHMoKSB7XHJcblx0XHRkcmF3aW5nQXJlYS5vblNpbmdsZVRvdWNoU3RhcnQoc3RhcnREcmFnKTtcclxuXHRcdGRyYXdpbmdBcmVhLm9uU2luZ2xlVG91Y2hNb3ZlKGNvbnRpbnVlRHJhZyk7XHJcblx0XHRkcmF3aW5nQXJlYS5vblRvdWNoRW5kKGVuZERyYWcpO1xyXG5cdFx0ZHJhd2luZ0FyZWEub25Ub3VjaENhbmNlbChlbmREcmFnKTtcclxuXHJcblx0XHRkcmF3aW5nQXJlYS5vbk11bHRpVG91Y2hTdGFydChlbmREcmFnKTtcclxuXHR9XHJcblxyXG5cdGZ1bmN0aW9uIHN0YXJ0RHJhZyhwYWdlT2Zmc2V0KSB7XHJcblx0XHRzdGFydCA9IGRyYXdpbmdBcmVhLnJlbGF0aXZlT2Zmc2V0KHBhZ2VPZmZzZXQpO1xyXG4gICAgaWYgKHVzZVNldENhcHR1cmVBcGkpIGRyYXdpbmdBcmVhLnNldENhcHR1cmUoKTtcclxuXHR9XHJcblxyXG5cdGZ1bmN0aW9uIGNvbnRpbnVlRHJhZyhwYWdlT2Zmc2V0KSB7XHJcblx0XHRpZiAoIWlzQ3VycmVudGx5RHJhd2luZygpKSByZXR1cm47XHJcblxyXG5cdFx0dmFyIGVuZCA9IGRyYXdpbmdBcmVhLnJlbGF0aXZlT2Zmc2V0KHBhZ2VPZmZzZXQpO1xyXG5cdFx0aWYgKHN0YXJ0LnggIT09IGVuZC54IHx8IHN0YXJ0LnkgIT09IGVuZC55KSB7XHJcblx0XHRcdHN2Z0NhbnZhcy5kcmF3TGluZShzdGFydC54LCBzdGFydC55LCBlbmQueCwgZW5kLnkpO1xyXG5cdFx0XHRzdGFydCA9IGVuZDtcclxuXHRcdFx0bGluZURyYXduID0gdHJ1ZTtcclxuXHRcdH1cclxuXHR9XHJcblxyXG5cdGZ1bmN0aW9uIGVuZERyYWcoKSB7XHJcblx0XHRpZiAoIWlzQ3VycmVudGx5RHJhd2luZygpKSByZXR1cm47XHJcblxyXG5cdFx0aWYgKCFsaW5lRHJhd24pIHN2Z0NhbnZhcy5kcmF3RG90KHN0YXJ0LngsIHN0YXJ0LnkpO1xyXG5cclxuXHRcdGlmICh1c2VTZXRDYXB0dXJlQXBpKSBkcmF3aW5nQXJlYS5yZWxlYXNlQ2FwdHVyZSgpO1xyXG5cdFx0c3RhcnQgPSBudWxsO1xyXG5cdFx0bGluZURyYXduID0gZmFsc2U7XHJcblx0fVxyXG5cclxuXHRmdW5jdGlvbiBpc0N1cnJlbnRseURyYXdpbmcoKSB7XHJcblx0XHRyZXR1cm4gc3RhcnQgIT09IG51bGw7XHJcblx0fVxyXG5cclxufSgpKTsiLCIvLyBDb3B5cmlnaHQgKGMpIDIwMTMgVGl0YW5pdW0gSS5ULiBMTEMuIEFsbCByaWdodHMgcmVzZXJ2ZWQuIFNlZSBMSUNFTlNFLnR4dCBmb3IgZGV0YWlscy5cclxuLypnbG9iYWwgJCwgalF1ZXJ5LCBUb3VjaExpc3QsIFRvdWNoICovXHJcblxyXG4oZnVuY3Rpb24oKSB7XHJcblx0XCJ1c2Ugc3RyaWN0XCI7XHJcblxyXG5cdHZhciBicm93c2VyID0gcmVxdWlyZShcIi4vYnJvd3Nlci5qc1wiKTtcclxuXHR2YXIgZmFpbEZhc3QgPSByZXF1aXJlKFwiLi9mYWlsX2Zhc3QuanNcIik7XHJcblxyXG5cdHZhciBjYXB0dXJlZEVsZW1lbnQgPSBudWxsO1xyXG5cclxuXHJcblx0LyogQ29uc3RydWN0b3JzICovXHJcblxyXG5cdHZhciBIdG1sRWxlbWVudCA9IG1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oZG9tRWxlbWVudCkge1xyXG5cdFx0dmFyIHNlbGYgPSB0aGlzO1xyXG5cclxuXHRcdHNlbGYuX2RvbUVsZW1lbnQgPSBkb21FbGVtZW50O1xyXG5cdFx0c2VsZi5fZWxlbWVudCA9ICQoZG9tRWxlbWVudCk7XHJcblx0XHRzZWxmLl9kcmFnRGVmYXVsdHNQcmV2ZW50ZWQgPSBmYWxzZTtcclxuXHR9O1xyXG5cclxuXHRIdG1sRWxlbWVudC5mcm9tSHRtbCA9IGZ1bmN0aW9uKGh0bWwpIHtcclxuXHRcdHJldHVybiBuZXcgSHRtbEVsZW1lbnQoJChodG1sKVswXSk7XHJcblx0fTtcclxuXHJcblx0SHRtbEVsZW1lbnQuZnJvbUlkID0gZnVuY3Rpb24oaWQpIHtcclxuXHRcdHZhciBkb21FbGVtZW50ID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoaWQpO1xyXG5cdFx0ZmFpbEZhc3QudW5sZXNzVHJ1ZShkb21FbGVtZW50ICE9PSBudWxsLCBcImNvdWxkIG5vdCBmaW5kIGVsZW1lbnQgd2l0aCBpZCAnXCIgKyBpZCArIFwiJ1wiKTtcclxuXHRcdHJldHVybiBuZXcgSHRtbEVsZW1lbnQoZG9tRWxlbWVudCk7XHJcblx0fTtcclxuXHJcblx0LyogQ2FwdHVyZSBBUEkgKi9cclxuXHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnNldENhcHR1cmUgPSBmdW5jdGlvbigpIHtcclxuXHRcdGNhcHR1cmVkRWxlbWVudCA9IHRoaXM7XHJcblx0XHR0aGlzLl9kb21FbGVtZW50LnNldENhcHR1cmUoKTtcclxuXHR9O1xyXG5cclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUucmVsZWFzZUNhcHR1cmUgPSBmdW5jdGlvbigpIHtcclxuXHRcdGNhcHR1cmVkRWxlbWVudCA9IG51bGw7XHJcblx0XHR0aGlzLl9kb21FbGVtZW50LnJlbGVhc2VDYXB0dXJlKCk7XHJcblx0fTtcclxuXHJcblxyXG5cdC8qIEdlbmVyYWwgZXZlbnQgaGFuZGxpbmcgKi9cclxuXHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnJlbW92ZUFsbEV2ZW50SGFuZGxlcnMgPSBmdW5jdGlvbigpIHtcclxuXHRcdHRoaXMuX2VsZW1lbnQub2ZmKCk7XHJcblx0fTtcclxuXHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnByZXZlbnRCcm93c2VyRHJhZ0RlZmF1bHRzID0gZnVuY3Rpb24oKSB7XHJcblx0XHR0aGlzLl9lbGVtZW50Lm9uKFwic2VsZWN0c3RhcnRcIiwgcHJldmVudERlZmF1bHRzKTtcclxuXHRcdHRoaXMuX2VsZW1lbnQub24oXCJtb3VzZWRvd25cIiwgcHJldmVudERlZmF1bHRzKTtcclxuXHRcdHRoaXMuX2VsZW1lbnQub24oXCJ0b3VjaHN0YXJ0XCIsIHByZXZlbnREZWZhdWx0cyk7XHJcblxyXG5cdFx0dGhpcy5fZHJhZ0RlZmF1bHRzUHJldmVudGVkID0gdHJ1ZTtcclxuXHJcblx0XHRmdW5jdGlvbiBwcmV2ZW50RGVmYXVsdHMoZXZlbnQpIHtcclxuXHRcdFx0ZXZlbnQucHJldmVudERlZmF1bHQoKTtcclxuXHRcdH1cclxuXHR9O1xyXG5cclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUuaXNCcm93c2VyRHJhZ0RlZmF1bHRzUHJldmVudGVkID0gZnVuY3Rpb24oKSB7XHJcblx0XHRyZXR1cm4gdGhpcy5fZHJhZ0RlZmF1bHRzUHJldmVudGVkO1xyXG5cdH07XHJcblxyXG5cdC8qIE1vdXNlIGV2ZW50cyAqL1xyXG5cdEh0bWxFbGVtZW50LnByb3RvdHlwZS50cmlnZ2VyTW91c2VDbGljayA9IHRyaWdnZXJNb3VzZUV2ZW50Rm4oXCJjbGlja1wiKTtcclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUudHJpZ2dlck1vdXNlRG93biA9IHRyaWdnZXJNb3VzZUV2ZW50Rm4oXCJtb3VzZWRvd25cIik7XHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnRyaWdnZXJNb3VzZU1vdmUgPSB0cmlnZ2VyTW91c2VFdmVudEZuKFwibW91c2Vtb3ZlXCIpO1xyXG5cdEh0bWxFbGVtZW50LnByb3RvdHlwZS50cmlnZ2VyTW91c2VMZWF2ZSA9IHRyaWdnZXJNb3VzZUV2ZW50Rm4oXCJtb3VzZWxlYXZlXCIpO1xyXG5cdEh0bWxFbGVtZW50LnByb3RvdHlwZS50cmlnZ2VyTW91c2VVcCA9IHRyaWdnZXJNb3VzZUV2ZW50Rm4oXCJtb3VzZXVwXCIpO1xyXG5cclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUub25Nb3VzZUNsaWNrID0gb25Nb3VzZUV2ZW50Rm4oXCJjbGlja1wiKTtcclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUub25Nb3VzZURvd24gPSBvbk1vdXNlRXZlbnRGbihcIm1vdXNlZG93blwiKTtcclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUub25Nb3VzZU1vdmUgPSBvbk1vdXNlRXZlbnRGbihcIm1vdXNlbW92ZVwiKTtcclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUub25Nb3VzZUxlYXZlID0gb25Nb3VzZUV2ZW50Rm4oXCJtb3VzZWxlYXZlXCIpO1xyXG5cdEh0bWxFbGVtZW50LnByb3RvdHlwZS5vbk1vdXNlVXAgPSBvbk1vdXNlRXZlbnRGbihcIm1vdXNldXBcIik7XHJcblxyXG5cdGZ1bmN0aW9uIHRyaWdnZXJNb3VzZUV2ZW50Rm4oZXZlbnQpIHtcclxuXHRcdHJldHVybiBmdW5jdGlvbihyZWxhdGl2ZVgsIHJlbGF0aXZlWSkge1xyXG5cdFx0XHR2YXIgdGFyZ2V0RWxlbWVudCA9IGNhcHR1cmVkRWxlbWVudCB8fCB0aGlzO1xyXG5cclxuXHRcdFx0dmFyIHBhZ2VDb29yZHM7XHJcblx0XHRcdGlmIChyZWxhdGl2ZVggPT09IHVuZGVmaW5lZCB8fCByZWxhdGl2ZVkgPT09IHVuZGVmaW5lZCkge1xyXG5cdFx0XHRcdHBhZ2VDb29yZHMgPSB7IHg6IDAsIHk6IDAgfTtcclxuXHRcdFx0fVxyXG5cdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRwYWdlQ29vcmRzID0gcGFnZU9mZnNldCh0aGlzLCByZWxhdGl2ZVgsIHJlbGF0aXZlWSk7XHJcblx0XHRcdH1cclxuXHJcblx0XHRcdHNlbmRNb3VzZUV2ZW50KHRhcmdldEVsZW1lbnQsIGV2ZW50LCBwYWdlQ29vcmRzKTtcclxuXHRcdH07XHJcblx0fVxyXG5cclxuXHRmdW5jdGlvbiBvbk1vdXNlRXZlbnRGbihldmVudCkge1xyXG5cdFx0cmV0dXJuIGZ1bmN0aW9uKGNhbGxiYWNrKSB7XHJcblx0XHRcdGlmIChicm93c2VyLmRvZXNOb3RIYW5kbGVzVXNlckV2ZW50c09uV2luZG93KCkgJiYgdGhpcy5fZG9tRWxlbWVudCA9PT0gd2luZG93KSByZXR1cm47XHJcblxyXG5cdFx0XHR0aGlzLl9lbGVtZW50Lm9uKGV2ZW50LCBmdW5jdGlvbihldmVudCkge1xyXG5cdFx0XHRcdHZhciBwYWdlT2Zmc2V0ID0geyB4OiBldmVudC5wYWdlWCwgeTogZXZlbnQucGFnZVkgfTtcclxuXHRcdFx0XHRjYWxsYmFjayhwYWdlT2Zmc2V0KTtcclxuXHRcdFx0fSk7XHJcblx0XHR9O1xyXG5cdH1cclxuXHJcblx0ZnVuY3Rpb24gc2VuZE1vdXNlRXZlbnQoc2VsZiwgZXZlbnQsIHBhZ2VDb29yZHMpIHtcclxuXHRcdHZhciBqcUVsZW1lbnQgPSBzZWxmLl9lbGVtZW50O1xyXG5cclxuXHRcdHZhciBldmVudERhdGEgPSBuZXcgalF1ZXJ5LkV2ZW50KCk7XHJcblx0XHRldmVudERhdGEucGFnZVggPSBwYWdlQ29vcmRzLng7XHJcblx0XHRldmVudERhdGEucGFnZVkgPSBwYWdlQ29vcmRzLnk7XHJcblx0XHRldmVudERhdGEudHlwZSA9IGV2ZW50O1xyXG5cdFx0anFFbGVtZW50LnRyaWdnZXIoZXZlbnREYXRhKTtcclxuXHR9XHJcblxyXG5cclxuXHQvKiBUb3VjaCBldmVudHMgKi9cclxuXHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnRyaWdnZXJUb3VjaEVuZCA9IHRyaWdnZXJaZXJvVG91Y2hFdmVudEZuKFwidG91Y2hlbmRcIik7XHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnRyaWdnZXJUb3VjaENhbmNlbCA9IHRyaWdnZXJaZXJvVG91Y2hFdmVudEZuKFwidG91Y2hjYW5jZWxcIik7XHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnRyaWdnZXJTaW5nbGVUb3VjaFN0YXJ0ID0gdHJpZ2dlclNpbmdsZVRvdWNoRXZlbnRGbihcInRvdWNoc3RhcnRcIik7XHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnRyaWdnZXJTaW5nbGVUb3VjaE1vdmUgPSB0cmlnZ2VyU2luZ2xlVG91Y2hFdmVudEZuKFwidG91Y2htb3ZlXCIpO1xyXG5cdEh0bWxFbGVtZW50LnByb3RvdHlwZS50cmlnZ2VyTXVsdGlUb3VjaFN0YXJ0ID0gdHJpZ2dlck11bHRpVG91Y2hFdmVudEZuKFwidG91Y2hzdGFydFwiKTtcclxuXHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLm9uVG91Y2hFbmQgPSBvblplcm9Ub3VjaEV2ZW50Rm4oXCJ0b3VjaGVuZFwiKTtcclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUub25Ub3VjaENhbmNlbCA9IG9uWmVyb1RvdWNoRXZlbnRGbihcInRvdWNoY2FuY2VsXCIpO1xyXG5cdEh0bWxFbGVtZW50LnByb3RvdHlwZS5vblNpbmdsZVRvdWNoU3RhcnQgPSBvblNpbmdsZVRvdWNoRXZlbnRGbihcInRvdWNoc3RhcnRcIik7XHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLm9uU2luZ2xlVG91Y2hNb3ZlID0gb25TaW5nbGVUb3VjaEV2ZW50Rm4oXCJ0b3VjaG1vdmVcIik7XHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLm9uTXVsdGlUb3VjaFN0YXJ0ID0gb25NdWx0aVRvdWNoRXZlbnRGbihcInRvdWNoc3RhcnRcIik7XHJcblxyXG5cdGZ1bmN0aW9uIHRyaWdnZXJaZXJvVG91Y2hFdmVudEZuKGV2ZW50KSB7XHJcblx0XHRyZXR1cm4gZnVuY3Rpb24oKSB7XHJcblx0XHRcdHNlbmRUb3VjaEV2ZW50KHRoaXMsIGV2ZW50LCBuZXcgVG91Y2hMaXN0KCkpO1xyXG5cdFx0fTtcclxuXHR9XHJcblxyXG5cdGZ1bmN0aW9uIHRyaWdnZXJTaW5nbGVUb3VjaEV2ZW50Rm4oZXZlbnQpIHtcclxuXHRcdHJldHVybiBmdW5jdGlvbihyZWxhdGl2ZVgsIHJlbGF0aXZlWSkge1xyXG5cdFx0XHR2YXIgdG91Y2ggPSBjcmVhdGVUb3VjaCh0aGlzLCByZWxhdGl2ZVgsIHJlbGF0aXZlWSk7XHJcblx0XHRcdHNlbmRUb3VjaEV2ZW50KHRoaXMsIGV2ZW50LCBuZXcgVG91Y2hMaXN0KHRvdWNoKSk7XHJcblx0XHR9O1xyXG5cdH1cclxuXHJcblx0ZnVuY3Rpb24gdHJpZ2dlck11bHRpVG91Y2hFdmVudEZuKGV2ZW50KSB7XHJcblx0XHRyZXR1cm4gZnVuY3Rpb24ocmVsYXRpdmUxWCwgcmVsYXRpdmUxWSwgcmVsYXRpdmUyWCwgcmVsYXRpdmUyWSkge1xyXG5cdFx0XHR2YXIgdG91Y2gxID0gY3JlYXRlVG91Y2godGhpcywgcmVsYXRpdmUxWCwgcmVsYXRpdmUxWSk7XHJcblx0XHRcdHZhciB0b3VjaDIgPSBjcmVhdGVUb3VjaCh0aGlzLCByZWxhdGl2ZTJYLCByZWxhdGl2ZTJZKTtcclxuXHRcdFx0c2VuZFRvdWNoRXZlbnQodGhpcywgZXZlbnQsIG5ldyBUb3VjaExpc3QodG91Y2gxLCB0b3VjaDIpKTtcclxuXHRcdH07XHJcblx0fVxyXG5cclxuXHJcblx0ZnVuY3Rpb24gb25aZXJvVG91Y2hFdmVudEZuKGV2ZW50KSB7XHJcblx0XHRyZXR1cm4gZnVuY3Rpb24oY2FsbGJhY2spIHtcclxuXHRcdFx0dGhpcy5fZWxlbWVudC5vbihldmVudCwgZnVuY3Rpb24oKSB7XHJcblx0XHRcdFx0Y2FsbGJhY2soKTtcclxuXHRcdFx0fSk7XHJcblx0XHR9O1xyXG5cdH1cclxuXHJcblx0ZnVuY3Rpb24gb25TaW5nbGVUb3VjaEV2ZW50Rm4oZXZlbnROYW1lKSB7XHJcblx0XHRyZXR1cm4gZnVuY3Rpb24oY2FsbGJhY2spIHtcclxuXHRcdFx0dGhpcy5fZWxlbWVudC5vbihldmVudE5hbWUsIGZ1bmN0aW9uKGV2ZW50KSB7XHJcblx0XHRcdFx0dmFyIG9yaWdpbmFsRXZlbnQgPSBldmVudC5vcmlnaW5hbEV2ZW50O1xyXG5cdFx0XHRcdGlmIChvcmlnaW5hbEV2ZW50LnRvdWNoZXMubGVuZ3RoICE9PSAxKSByZXR1cm47XHJcblxyXG5cdFx0XHRcdHZhciBwYWdlWCA9IG9yaWdpbmFsRXZlbnQudG91Y2hlc1swXS5wYWdlWDtcclxuXHRcdFx0XHR2YXIgcGFnZVkgPSBvcmlnaW5hbEV2ZW50LnRvdWNoZXNbMF0ucGFnZVk7XHJcblx0XHRcdFx0dmFyIG9mZnNldCA9IHsgeDogcGFnZVgsIHk6IHBhZ2VZIH07XHJcblxyXG5cdFx0XHRcdGNhbGxiYWNrKG9mZnNldCk7XHJcblx0XHRcdH0pO1xyXG5cdFx0fTtcclxuXHR9XHJcblxyXG5cdGZ1bmN0aW9uIG9uTXVsdGlUb3VjaEV2ZW50Rm4oZXZlbnQpIHtcclxuXHRcdHJldHVybiBmdW5jdGlvbihjYWxsYmFjaykge1xyXG5cdFx0XHR2YXIgc2VsZiA9IHRoaXM7XHJcblx0XHRcdHRoaXMuX2VsZW1lbnQub24oZXZlbnQsIGZ1bmN0aW9uKGV2ZW50KSB7XHJcblx0XHRcdFx0dmFyIG9yaWdpbmFsRXZlbnQgPSBldmVudC5vcmlnaW5hbEV2ZW50O1xyXG5cdFx0XHRcdGlmIChvcmlnaW5hbEV2ZW50LnRvdWNoZXMubGVuZ3RoICE9PSAxKSBjYWxsYmFjaygpO1xyXG5cdFx0XHR9KTtcclxuXHRcdH07XHJcblx0fVxyXG5cclxuXHRmdW5jdGlvbiBzZW5kVG91Y2hFdmVudChzZWxmLCBldmVudCwgdG91Y2hMaXN0KSB7XHJcblx0XHR2YXIgdG91Y2hFdmVudCA9IGRvY3VtZW50LmNyZWF0ZUV2ZW50KFwiVG91Y2hFdmVudFwiKTtcclxuXHRcdHRvdWNoRXZlbnQuaW5pdFRvdWNoRXZlbnQoXHJcblx0XHRcdGV2ZW50LCAvLyBldmVudCB0eXBlXHJcblx0XHRcdHRydWUsIC8vIGNhbkJ1YmJsZVxyXG5cdFx0XHR0cnVlLCAvLyBjYW5jZWxhYmxlXHJcblx0XHRcdHdpbmRvdywgLy8gRE9NIHdpbmRvd1xyXG5cdFx0XHRudWxsLCAvLyBkZXRhaWwgKG5vdCBzdXJlIHdoYXQgdGhpcyBpcylcclxuXHRcdFx0MCwgMCwgLy8gc2NyZWVuWC9ZXHJcblx0XHRcdDAsIDAsIC8vIGNsaWVudFgvWVxyXG5cdFx0XHRmYWxzZSwgZmFsc2UsIGZhbHNlLCBmYWxzZSwgLy8gbWV0YSBrZXlzIChzaGlmdCBldGMuKVxyXG5cdFx0XHR0b3VjaExpc3QsIHRvdWNoTGlzdCwgdG91Y2hMaXN0XHJcblx0XHQpO1xyXG5cclxuXHRcdHZhciBldmVudERhdGEgPSBuZXcgalF1ZXJ5LkV2ZW50KFwiZXZlbnRcIik7XHJcblx0XHRldmVudERhdGEudHlwZSA9IGV2ZW50O1xyXG5cdFx0ZXZlbnREYXRhLm9yaWdpbmFsRXZlbnQgPSB0b3VjaEV2ZW50O1xyXG5cdFx0c2VsZi5fZWxlbWVudC50cmlnZ2VyKGV2ZW50RGF0YSk7XHJcblx0fVxyXG5cclxuXHRmdW5jdGlvbiBjcmVhdGVUb3VjaChzZWxmLCByZWxhdGl2ZVgsIHJlbGF0aXZlWSkge1xyXG5cdFx0dmFyIG9mZnNldCA9IHBhZ2VPZmZzZXQoc2VsZiwgcmVsYXRpdmVYLCByZWxhdGl2ZVkpO1xyXG5cclxuXHRcdHZhciB0YXJnZXQgPSBzZWxmLl9lbGVtZW50WzBdO1xyXG5cdFx0dmFyIGlkZW50aWZpZXIgPSAwO1xyXG5cdFx0dmFyIHBhZ2VYID0gb2Zmc2V0Lng7XHJcblx0XHR2YXIgcGFnZVkgPSBvZmZzZXQueTtcclxuXHRcdHZhciBzY3JlZW5YID0gMDtcclxuXHRcdHZhciBzY3JlZW5ZID0gMDtcclxuXHJcblx0XHRyZXR1cm4gbmV3IFRvdWNoKHVuZGVmaW5lZCwgdGFyZ2V0LCBpZGVudGlmaWVyLCBwYWdlWCwgcGFnZVksIHNjcmVlblgsIHNjcmVlblkpO1xyXG5cdH1cclxuXHJcblxyXG5cdC8qIERpbWVuc2lvbnMsIG9mZnNldHMsIGFuZCBwb3NpdGlvbmluZyAqL1xyXG5cclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUuZ2V0RGltZW5zaW9ucyA9IGZ1bmN0aW9uKCkge1xyXG5cdFx0cmV0dXJuIHtcclxuXHRcdFx0d2lkdGg6IHRoaXMuX2VsZW1lbnQud2lkdGgoKSxcclxuXHRcdFx0aGVpZ2h0OiB0aGlzLl9lbGVtZW50LmhlaWdodCgpXHJcblx0XHR9O1xyXG5cdH07XHJcblxyXG5cdEh0bWxFbGVtZW50LnByb3RvdHlwZS5yZWxhdGl2ZU9mZnNldCA9IGZ1bmN0aW9uKHBhZ2VPZmZzZXQpIHtcclxuXHRcdHJldHVybiByZWxhdGl2ZU9mZnNldCh0aGlzLCBwYWdlT2Zmc2V0LngsIHBhZ2VPZmZzZXQueSk7XHJcblx0fTtcclxuXHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnBhZ2VPZmZzZXQgPSBmdW5jdGlvbihyZWxhdGl2ZU9mZnNldCkge1xyXG5cdFx0cmV0dXJuIHBhZ2VPZmZzZXQodGhpcywgcmVsYXRpdmVPZmZzZXQueCwgcmVsYXRpdmVPZmZzZXQueSk7XHJcblx0fTtcclxuXHJcblx0ZnVuY3Rpb24gcmVsYXRpdmVPZmZzZXQoc2VsZiwgcGFnZVgsIHBhZ2VZKSB7XHJcblx0XHRmYWlsRmFzdElmU3R5bGluZ1ByZXNlbnQoc2VsZik7XHJcblxyXG5cdFx0dmFyIHBhZ2VPZmZzZXQgPSBzZWxmLl9lbGVtZW50Lm9mZnNldCgpO1xyXG5cdFx0cmV0dXJuIHtcclxuXHRcdFx0eDogcGFnZVggLSBwYWdlT2Zmc2V0LmxlZnQsXHJcblx0XHRcdHk6IHBhZ2VZIC0gcGFnZU9mZnNldC50b3BcclxuXHRcdH07XHJcblx0fVxyXG5cclxuXHRmdW5jdGlvbiBwYWdlT2Zmc2V0KHNlbGYsIHJlbGF0aXZlWCwgcmVsYXRpdmVZKSB7XHJcblx0XHRmYWlsRmFzdElmU3R5bGluZ1ByZXNlbnQoc2VsZik7XHJcblxyXG5cdFx0dmFyIHRvcExlZnRPZkRyYXdpbmdBcmVhID0gc2VsZi5fZWxlbWVudC5vZmZzZXQoKTtcclxuXHRcdHJldHVybiB7XHJcblx0XHRcdHg6IHJlbGF0aXZlWCArIHRvcExlZnRPZkRyYXdpbmdBcmVhLmxlZnQsXHJcblx0XHRcdHk6IHJlbGF0aXZlWSArIHRvcExlZnRPZkRyYXdpbmdBcmVhLnRvcFxyXG5cdFx0fTtcclxuXHR9XHJcblxyXG5cdGZ1bmN0aW9uIGZhaWxGYXN0SWZTdHlsaW5nUHJlc2VudChzZWxmKSB7XHJcblx0XHRmYWlsRmFzdElmUGFkZGluZ1ByZXNlbnQoXCJ0b3BcIik7XHJcblx0XHRmYWlsRmFzdElmUGFkZGluZ1ByZXNlbnQoXCJsZWZ0XCIpO1xyXG5cdFx0ZmFpbEZhc3RJZkJvcmRlclByZXNlbnQoXCJ0b3BcIik7XHJcblx0XHRmYWlsRmFzdElmQm9yZGVyUHJlc2VudChcImxlZnRcIik7XHJcblxyXG5cdFx0ZnVuY3Rpb24gZmFpbEZhc3RJZlBhZGRpbmdQcmVzZW50KHNpZGUpIHtcclxuXHRcdFx0dmFyIGNzcyA9IHNlbGYuX2VsZW1lbnQuY3NzKFwicGFkZGluZy1cIiArIHNpZGUpO1xyXG5cdFx0XHRpZiAoY3NzICE9PSBcIjBweFwiKSB0aHJvdyBuZXcgRXJyb3IoXCJEbyBub3QgYXBwbHkgcGFkZGluZyB0byBlbGVtZW50cyB1c2VkIHdpdGggcmVsYXRpdmVPZmZzZXQoKSAoZXhwZWN0ZWQgMHB4IGJ1dCB3YXMgXCIgKyBjc3MgKyBcIilcIik7XHJcblx0XHR9XHJcblxyXG5cdFx0ZnVuY3Rpb24gZmFpbEZhc3RJZkJvcmRlclByZXNlbnQoc2lkZSkge1xyXG5cdFx0XHR2YXIgY3NzID0gc2VsZi5fZWxlbWVudC5jc3MoXCJib3JkZXItXCIgKyBzaWRlICsgXCItd2lkdGhcIik7XHJcblx0XHRcdGlmIChicm93c2VyLmRvZXNOb3RDb21wdXRlU3R5bGVzKCkpIHtcclxuXHRcdFx0XHRpZiAoc2VsZi5fZWxlbWVudC5jc3MoXCJib3JkZXItXCIgKyBzaWRlICsgXCItc3R5bGVcIikgPT09IFwibm9uZVwiKSBjc3MgPSBcIjBweFwiO1xyXG5cdFx0XHR9XHJcblxyXG5cdFx0XHRpZiAoY3NzICE9PSBcIjBweFwiKSB0aHJvdyBuZXcgRXJyb3IoXCJEbyBub3QgYXBwbHkgYm9yZGVyIHRvIGVsZW1lbnRzIHVzZWQgd2l0aCByZWxhdGl2ZU9mZnNldCgpIChleHBlY3RlZCAwcHggYnV0IHdhcyBcIiArIGNzcyArIFwiKVwiKTtcclxuXHRcdH1cclxuXHR9XHJcblxyXG5cdC8qIERPTSBNYW5pcHVsYXRpb24gKi9cclxuXHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLmFwcGVuZCA9IGZ1bmN0aW9uKGVsZW1lbnRUb0FwcGVuZCkge1xyXG5cdFx0dGhpcy5fZWxlbWVudC5hcHBlbmQoZWxlbWVudFRvQXBwZW5kLl9lbGVtZW50KTtcclxuXHR9O1xyXG5cclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUuYXBwZW5kU2VsZlRvQm9keSA9IGZ1bmN0aW9uKCkge1xyXG5cdFx0JChkb2N1bWVudC5ib2R5KS5hcHBlbmQodGhpcy5fZWxlbWVudCk7XHJcblx0fTtcclxuXHJcblx0SHRtbEVsZW1lbnQucHJvdG90eXBlLnJlbW92ZSA9IGZ1bmN0aW9uKCkge1xyXG5cdFx0dGhpcy5fZWxlbWVudC5yZW1vdmUoKTtcclxuXHR9O1xyXG5cclxuXHRIdG1sRWxlbWVudC5wcm90b3R5cGUudG9Eb21FbGVtZW50ID0gZnVuY3Rpb24oKSB7XHJcblx0XHRyZXR1cm4gdGhpcy5fZWxlbWVudFswXTtcclxuXHR9O1xyXG5cclxufSgpKTsiLCIvLyBDb3B5cmlnaHQgKGMpIDIwMTMgVGl0YW5pdW0gSS5ULiBMTEMuIEFsbCByaWdodHMgcmVzZXJ2ZWQuIFNlZSBMSUNFTlNFLlRYVCBmb3IgZGV0YWlscy5cclxuLypnbG9iYWwgTW9kZXJuaXpyLCAkICovXHJcblxyXG4oZnVuY3Rpb24oKSB7XHJcblx0XCJ1c2Ugc3RyaWN0XCI7XHJcblxyXG5cdGV4cG9ydHMuc3VwcG9ydHNUb3VjaEV2ZW50cyA9IGZ1bmN0aW9uKCkge1xyXG5cdFx0cmV0dXJuIE1vZGVybml6ci50b3VjaDtcclxuXHR9O1xyXG5cclxuXHRleHBvcnRzLnN1cHBvcnRzQ2FwdHVyZUFwaSA9IGZ1bmN0aW9uKCkge1xyXG5cdFx0cmV0dXJuIGRvY3VtZW50LmJvZHkuc2V0Q2FwdHVyZSAmJiBkb2N1bWVudC5ib2R5LnJlbGVhc2VDYXB0dXJlO1xyXG5cdH07XHJcblxyXG5cdGV4cG9ydHMucmVwb3J0c0VsZW1lbnRQb3NpdGlvbk9mZkJ5T25lU29tZXRpbWVzID0gZnVuY3Rpb24oKSB7XHJcblx0XHRyZXR1cm4gaXNJZTgoKTtcclxuXHR9O1xyXG5cclxuXHRleHBvcnRzLmRvZXNOb3RIYW5kbGVzVXNlckV2ZW50c09uV2luZG93ID0gZnVuY3Rpb24oKSB7XHJcblx0XHRyZXR1cm4gaXNJZTgoKTtcclxuXHR9O1xyXG5cclxuXHRleHBvcnRzLmRvZXNOb3RDb21wdXRlU3R5bGVzID0gZnVuY3Rpb24oKSB7XHJcblx0XHRyZXR1cm4gaXNJZTgoKTtcclxuXHR9O1xyXG5cclxuXHRmdW5jdGlvbiBpc0llOCgpIHtcclxuXHRcdHJldHVybiAkLmJyb3dzZXIubXNpZSAmJiAkLmJyb3dzZXIudmVyc2lvbiA9PT0gXCI4LjBcIjtcclxuXHR9XHJcblxyXG59KCkpOyIsIi8vIENvcHlyaWdodCAoYykgMjAxMyBUaXRhbml1bSBJLlQuIExMQy4gQWxsIHJpZ2h0cyByZXNlcnZlZC4gU2VlIExJQ0VOU0UuVFhUIGZvciBkZXRhaWxzLlxyXG4oZnVuY3Rpb24oKSB7XHJcblx0XCJ1c2Ugc3RyaWN0XCI7XHJcblxyXG5cdGV4cG9ydHMudW5sZXNzRGVmaW5lZCA9IGZ1bmN0aW9uKHZhcmlhYmxlLCB2YXJpYWJsZU5hbWUpIHtcclxuXHRcdHZhcmlhYmxlTmFtZSA9IHZhcmlhYmxlTmFtZSA/IFwiIFtcIiArIHZhcmlhYmxlTmFtZSArIFwiXSBcIiA6IFwiIFwiO1xyXG5cdFx0aWYgKHZhcmlhYmxlID09PSB1bmRlZmluZWQpIHRocm93IG5ldyBGYWlsRmFzdEV4Y2VwdGlvbihleHBvcnRzLnVubGVzc0RlZmluZWQsIFwiUmVxdWlyZWQgdmFyaWFibGVcIiArIHZhcmlhYmxlTmFtZSArIFwid2FzIG5vdCBkZWZpbmVkXCIpO1xyXG5cdH07XHJcblxyXG5cdGV4cG9ydHMudW5sZXNzVHJ1ZSA9IGZ1bmN0aW9uKHZhcmlhYmxlLCBtZXNzYWdlKSB7XHJcblx0XHRpZiAobWVzc2FnZSA9PT0gdW5kZWZpbmVkKSBtZXNzYWdlID0gXCJFeHBlY3RlZCBjb25kaXRpb24gdG8gYmUgdHJ1ZVwiO1xyXG5cclxuXHRcdGlmICh2YXJpYWJsZSA9PT0gZmFsc2UpIHRocm93IG5ldyBGYWlsRmFzdEV4Y2VwdGlvbihleHBvcnRzLnVubGVzc1RydWUsIG1lc3NhZ2UpO1xyXG5cdFx0aWYgKHZhcmlhYmxlICE9PSB0cnVlKSB0aHJvdyBuZXcgRmFpbEZhc3RFeGNlcHRpb24oZXhwb3J0cy51bmxlc3NUcnVlLCBcIkV4cGVjdGVkIGNvbmRpdGlvbiB0byBiZSB0cnVlIG9yIGZhbHNlXCIpO1xyXG5cdH07XHJcblxyXG5cdGV4cG9ydHMudW5yZWFjaGFibGUgPSBmdW5jdGlvbihtZXNzYWdlKSB7XHJcblx0XHRpZiAoIW1lc3NhZ2UpIG1lc3NhZ2UgPSBcIlVucmVhY2hhYmxlIGNvZGUgZXhlY3V0ZWRcIjtcclxuXHJcblx0XHR0aHJvdyBuZXcgRmFpbEZhc3RFeGNlcHRpb24oZXhwb3J0cy51bnJlYWNoYWJsZSwgbWVzc2FnZSk7XHJcblx0fTtcclxuXHJcblx0dmFyIEZhaWxGYXN0RXhjZXB0aW9uID0gZXhwb3J0cy5GYWlsRmFzdEV4Y2VwdGlvbiA9IGZ1bmN0aW9uKGZuVG9SZW1vdmVGcm9tU3RhY2tUcmFjZSwgbWVzc2FnZSkge1xyXG5cdFx0aWYgKEVycm9yLmNhcHR1cmVTdGFja1RyYWNlKSBFcnJvci5jYXB0dXJlU3RhY2tUcmFjZSh0aGlzLCBmblRvUmVtb3ZlRnJvbVN0YWNrVHJhY2UpOyAgICAvLyBvbmx5IHdvcmtzIG9uIENocm9tZS9WOFxyXG5cdFx0dGhpcy5tZXNzYWdlID0gbWVzc2FnZTtcclxuXHR9O1xyXG5cdEZhaWxGYXN0RXhjZXB0aW9uLnByb3RvdHlwZSA9IG5ldyBFcnJvcigpO1xyXG5cdEZhaWxGYXN0RXhjZXB0aW9uLnByb3RvdHlwZS5jb25zdHJ1Y3RvciA9IEZhaWxGYXN0RXhjZXB0aW9uO1xyXG5cdEZhaWxGYXN0RXhjZXB0aW9uLnByb3RvdHlwZS5uYW1lID0gXCJGYWlsRmFzdEV4Y2VwdGlvblwiO1xyXG5cclxufSgpKTsiLCIvLyBDb3B5cmlnaHQgKGMpIDIwMTMgVGl0YW5pdW0gSS5ULiBMTEMuIEFsbCByaWdodHMgcmVzZXJ2ZWQuIFNlZSBMSUNFTlNFLlRYVCBmb3IgZGV0YWlscy5cclxuLypnbG9iYWwgUmFwaGFlbCAqL1xyXG5cclxuKGZ1bmN0aW9uKCkge1xyXG5cdFwidXNlIHN0cmljdFwiO1xyXG5cclxuXHR2YXIgU3ZnQ2FudmFzID0gbW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihodG1sRWxlbWVudCkge1xyXG5cdFx0dmFyIGRpbWVuc2lvbnMgPSBodG1sRWxlbWVudC5nZXREaW1lbnNpb25zKCk7XHJcblx0XHR0aGlzLl9wYXBlciA9IG5ldyBSYXBoYWVsKGh0bWxFbGVtZW50LnRvRG9tRWxlbWVudCgpLCBkaW1lbnNpb25zLndpZHRoLCBkaW1lbnNpb25zLmhlaWdodCk7XHJcblx0fTtcclxuXHJcblx0U3ZnQ2FudmFzLkxJTkVfQ09MT1IgPSBcImJsYWNrXCI7XHJcblx0U3ZnQ2FudmFzLlNUUk9LRV9XSURUSCA9IDI7XHJcblx0U3ZnQ2FudmFzLkxJTkVfQ0FQID0gXCJyb3VuZFwiO1xyXG5cclxuXHRTdmdDYW52YXMucHJvdG90eXBlLmNsZWFyID0gZnVuY3Rpb24oKSB7XHJcblx0XHR0aGlzLl9wYXBlci5jbGVhcigpO1xyXG5cdH07XHJcblxyXG5cdFN2Z0NhbnZhcy5wcm90b3R5cGUuZHJhd0xpbmUgPSBmdW5jdGlvbihzdGFydFgsIHN0YXJ0WSwgZW5kWCwgZW5kWSkge1xyXG5cdFx0dGhpcy5fcGFwZXIucGF0aChcIk1cIiArIHN0YXJ0WCArIFwiLFwiICsgc3RhcnRZICsgXCJMXCIgKyBlbmRYICsgXCIsXCIgKyBlbmRZKVxyXG5cdFx0XHQuYXR0cih7XHJcblx0XHRcdFx0XCJzdHJva2VcIjogU3ZnQ2FudmFzLkxJTkVfQ09MT1IsXHJcblx0XHRcdFx0XCJzdHJva2Utd2lkdGhcIjogU3ZnQ2FudmFzLlNUUk9LRV9XSURUSCxcclxuXHRcdFx0XHRcInN0cm9rZS1saW5lY2FwXCI6IFN2Z0NhbnZhcy5MSU5FX0NBUFxyXG5cdFx0XHR9KTtcclxuXHR9O1xyXG5cclxuXHRTdmdDYW52YXMucHJvdG90eXBlLmRyYXdEb3QgPSBmdW5jdGlvbih4LCB5KSB7XHJcblx0XHR0aGlzLl9wYXBlci5jaXJjbGUoeCwgeSwgU3ZnQ2FudmFzLlNUUk9LRV9XSURUSCAvIDIpXHJcblx0XHRcdC5hdHRyKHtcclxuXHRcdFx0XHRcInN0cm9rZVwiOiBTdmdDYW52YXMuTElORV9DT0xPUixcclxuXHRcdFx0XHRcImZpbGxcIjogU3ZnQ2FudmFzLkxJTkVfQ09MT1JcclxuXHRcdFx0fSk7XHJcblx0fTtcclxuXHJcblx0U3ZnQ2FudmFzLnByb3RvdHlwZS5saW5lU2VnbWVudHMgPSBmdW5jdGlvbigpIHtcclxuXHRcdHZhciByZXN1bHQgPSBbXTtcclxuXHRcdHRoaXMuX3BhcGVyLmZvckVhY2goZnVuY3Rpb24oZWxlbWVudCkge1xyXG5cdFx0XHRyZXN1bHQucHVzaChub3JtYWxpemVUb0xpbmVTZWdtZW50KGVsZW1lbnQpKTtcclxuXHRcdH0pO1xyXG5cdFx0cmV0dXJuIHJlc3VsdDtcclxuXHR9O1xyXG5cclxuXHRTdmdDYW52YXMucHJvdG90eXBlLmVsZW1lbnRzRm9yVGVzdGluZ09ubHkgPSBmdW5jdGlvbigpIHtcclxuXHRcdHZhciByZXN1bHQgPSBbXTtcclxuXHRcdHRoaXMuX3BhcGVyLmZvckVhY2goZnVuY3Rpb24oZWxlbWVudCkge1xyXG5cdFx0XHRyZXN1bHQucHVzaChlbGVtZW50KTtcclxuXHRcdH0pO1xyXG5cdFx0cmV0dXJuIHJlc3VsdDtcclxuXHR9O1xyXG5cclxuXHRmdW5jdGlvbiBub3JtYWxpemVUb0xpbmVTZWdtZW50KGVsZW1lbnQpIHtcclxuXHRcdHN3aXRjaCAoZWxlbWVudC50eXBlKSB7XHJcblx0XHRcdGNhc2UgXCJwYXRoXCI6XHJcblx0XHRcdFx0cmV0dXJuIG5vcm1hbGl6ZVBhdGgoZWxlbWVudCk7XHJcblx0XHRcdGNhc2UgXCJjaXJjbGVcIjpcclxuXHRcdFx0XHRyZXR1cm4gbm9ybWFsaXplQ2lyY2xlKGVsZW1lbnQpO1xyXG5cdFx0XHRkZWZhdWx0OlxyXG5cdFx0XHRcdHRocm93IG5ldyBFcnJvcihcIlVua25vd24gZWxlbWVudCB0eXBlOiBcIiArIGVsZW1lbnQudHlwZSk7XHJcblx0XHR9XHJcblx0fVxyXG5cclxuXHRmdW5jdGlvbiBub3JtYWxpemVDaXJjbGUoZWxlbWVudCkge1xyXG5cdFx0cmV0dXJuIFtcclxuXHRcdFx0ZWxlbWVudC5hdHRycy5jeCxcclxuXHRcdFx0ZWxlbWVudC5hdHRycy5jeVxyXG5cdFx0XTtcclxuXHR9XHJcblxyXG5cdGZ1bmN0aW9uIG5vcm1hbGl6ZVBhdGgoZWxlbWVudCkge1xyXG5cdFx0aWYgKFJhcGhhZWwuc3ZnKSB7XHJcblx0XHRcdHJldHVybiBub3JtYWxpemVTdmdQYXRoKGVsZW1lbnQpO1xyXG5cdFx0fVxyXG5cdFx0ZWxzZSBpZiAoUmFwaGFlbC52bWwpIHtcclxuXHRcdFx0cmV0dXJuIG5vcm1hbGl6ZVZtbFBhdGgoZWxlbWVudCk7XHJcblx0XHR9XHJcblx0XHRlbHNlIHtcclxuXHRcdFx0dGhyb3cgbmV3IEVycm9yKFwiVW5rbm93biBSYXBoYWVsIHJlbmRlcmluZyBlbmdpbmVcIik7XHJcblx0XHR9XHJcblx0fVxyXG5cclxuXHRmdW5jdGlvbiBub3JtYWxpemVTdmdQYXRoKGVsZW1lbnQpIHtcclxuXHRcdHZhciBwYXRoUmVnZXg7XHJcblxyXG5cdFx0dmFyIHBhdGggPSBlbGVtZW50Lm5vZGUuYXR0cmlidXRlcy5kLnZhbHVlO1xyXG5cdFx0aWYgKHBhdGguaW5kZXhPZihcIixcIikgIT09IC0xKVxyXG5cdFx0Ly8gV2UncmUgaW4gRmlyZWZveCwgU2FmYXJpLCBDaHJvbWUsIHdoaWNoIHVzZXMgZm9ybWF0IFwiTTIwLDMwTDMwLDMwMFwiXHJcblx0XHR7XHJcblx0XHRcdHBhdGhSZWdleCA9IC9NKFxcZCspLChcXGQrKUwoXFxkKyksKFxcZCspLztcclxuXHRcdH1cclxuXHRcdGVsc2Uge1xyXG5cdFx0XHQvLyBXZSdyZSBpbiBJRTksIHdoaWNoIHVzZXMgZm9ybWF0IFwiTSAyMCAzMCBMIDMwIDMwMFwiXHJcblx0XHRcdHBhdGhSZWdleCA9IC9NIChcXGQrKSAoXFxkKykgTCAoXFxkKykgKFxcZCspLztcclxuXHRcdH1cclxuXHRcdHZhciBwYXRoQ29tcG9uZW50cyA9IHBhdGgubWF0Y2gocGF0aFJlZ2V4KTtcclxuXHJcblx0XHRyZXR1cm4gW1xyXG5cdFx0XHRwYXRoQ29tcG9uZW50c1sxXSxcclxuXHRcdFx0cGF0aENvbXBvbmVudHNbMl0sXHJcblx0XHRcdHBhdGhDb21wb25lbnRzWzNdLFxyXG5cdFx0XHRwYXRoQ29tcG9uZW50c1s0XVxyXG5cdFx0XTtcclxuXHR9XHJcblxyXG5cdGZ1bmN0aW9uIG5vcm1hbGl6ZVZtbFBhdGgoZWxlbWVudCkge1xyXG5cdFx0Ly8gV2UncmUgaW4gSUUgOCwgd2hpY2ggdXNlcyBmb3JtYXQgXCJtNDMyMDAwLDY0ODAwMCBsNjQ4MDAwLDY3NDU2ODAwIGVcIlxyXG5cdFx0dmFyIFZNTF9NQUdJQ19OVU1CRVIgPSAyMTYwMDtcclxuXHJcblx0XHR2YXIgcGF0aCA9IGVsZW1lbnQubm9kZS5wYXRoLnZhbHVlO1xyXG5cclxuXHRcdHZhciBpZThQYXRoUmVnZXggPSAvbShcXGQrKSwoXFxkKykgbChcXGQrKSwoXFxkKykgZS87XHJcblx0XHR2YXIgaWU4ID0gcGF0aC5tYXRjaChpZThQYXRoUmVnZXgpO1xyXG5cclxuXHRcdHZhciBzdGFydFggPSBpZThbMV0gLyBWTUxfTUFHSUNfTlVNQkVSO1xyXG5cdFx0dmFyIHN0YXJ0WSA9IGllOFsyXSAvIFZNTF9NQUdJQ19OVU1CRVI7XHJcblx0XHR2YXIgZW5kWCA9IGllOFszXSAvIFZNTF9NQUdJQ19OVU1CRVI7XHJcblx0XHR2YXIgZW5kWSA9IGllOFs0XSAvIFZNTF9NQUdJQ19OVU1CRVI7XHJcblxyXG5cdFx0cmV0dXJuIFtcclxuXHRcdFx0c3RhcnRYLFxyXG5cdFx0XHRzdGFydFksXHJcblx0XHRcdGVuZFgsXHJcblx0XHRcdGVuZFlcclxuXHRcdF07XHJcblx0fVxyXG5cclxufSgpKTsiXX0=
