var nextMarker = null;

function fetch() {
    $.getJSON("/home/LoaderApi/",
    {
        "count": 12,
        "nextMarker": nextMarker
    })
    .success(function (data) {
        $(".subject").removeClass("active");
        for (var i = 0; i < data.images.images.length; i++) {
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
            isPhoto: $(subjects[i]).hasClass("active")
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

    $("body").on("click", ".subject", function () {
        $(this).toggleClass("active");
    });
});;