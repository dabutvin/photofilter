var nextMarker = null;

function fetch() {
    $.getJSON("/home/LoaderApi/",
    {
        "count": 12,
        "nextMarker": nextMarker
    })
    .success(function (data) {
        var numImages = data.images.images.length;
        for (var i = 0; i < numImages; i++) {
            $("#image_" + i).attr("src", data.images.images[i].id);
        }
        nextMarker = data.images.continuationToken.nextMarker;
    })
    .fail(function (a, b, c) {
        alert("whoops");
        console.error(a);
        console.error(b);
        console.error(c);

        return undefined;
    });
}

$(function () {
    var subjects = $(".subject");

    fetch();
    $("body").on("click", "#next", function () {
        subjects.hide();
        fetch();
        subjects.show();
    });
});;