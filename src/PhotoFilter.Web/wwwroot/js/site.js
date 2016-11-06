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
        $(".subject").show();
    })
    .fail(function (a, b, c) {
        alert("whoops");
        console.error(a);
        console.error(b);
        console.error(c);

        return undefined;
    });
}

function post(callback) {
    var imageData = [],
        subjects = $('.subject');

    for (var i = 0; i < subjects.length; i++) {
        imageData.push({
            id: $(subjects[i]).attr("src"),
            isPhoto: $(subjects[i]).data("isphoto")
        });
    }

    $.post("/home/PosterApi/", {
        data: {
            Images: imageData
        }
    })
    .success(callback)
    .fail(function (a, b, c) {
        alert("whoops");
        console.error(a);
        console.error(b);
        console.error(c);

        return undefined;
    });
}

$(function () {
    fetch();
    $("body").on("click", "#next", function () {
        post(function (response) {
            $(".subject").hide();
            fetch();
        });
    });
});;