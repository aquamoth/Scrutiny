describe('Index html', function () {
    var frame;

    beforeEach(function (done) {
        frame = quixote.createFrame({ src: '/' }, done);
    });


    afterEach(function () {
        frame.remove();
    });

    it('looks ok', function () {
        var logo = frame.get('.jumbotron h1');
        logo.assert({ left: 490 });

    });
});

describe('DrawingArea', function () {
    it('initializes', function () {

        var drawingArea = document.createElement("div");
        //drawingArea.setAttribute("id", "wwp-drawingArea");
        document.body.appendChild(drawingArea);

        wwp.home_index.initDrawingArea(drawingArea);

        //var extractedDiv = document.getElementById("maas");
        expect(drawingArea.getAttribute("foo")).toEqual("bar");

    });
});