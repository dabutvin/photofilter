var nextMarker = null;
var email = "";

function fetch() {
    $.getJSON("/home/LoaderApi/",
    {
        "count": 12,
        "nextMarker": nextMarker
    })
    .success(function (data) {
        $(".subject").removeClass("active");

        if (data.images.images.length < 1) {
            $("#loader").hide();
            $("#nodata").hide();
            $("#nodata").show();
        }

        for (var i = 0; i < data.images.images.length; i++) {
            $("#image_" + i)
                .attr("src", data.images.images[i].id)
                .attr("data-blobname", data.images.images[i].blobName)
                .attr("data-leaseid", data.images.images[i].leaseId);
        }
        
        if (data.images.continuationToken) {
            nextMarker = data.images.continuationToken.nextMarker;
        } else {
            nextMarker = null;
        }
        $("#loader").hide();
        $("#nodata").hide();
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
            isPhoto: $(subjects[i]).hasClass("active"),
            blobName: $(subjects[i]).attr("data-blobname"),
            leaseId: $(subjects[i]).attr("data-leaseid"),
            email: email
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

function countit() {
    $.getJSON("home/CountApi/")
    .success(function (data) {
        $("#numtotal").text(data.numTotal);
        $("#numphoto").text(data.numPhoto);
        $("#numnonphoto").text(data.numNonPhoto);
    });
}

$(function () {
    fetch();
    $("body").on("click", ".next", function () {
        $(".next").text("No photos");
        post(function (response) {
            $(".subject").hide();
            $("#nodata").hide();
            $("#loader").show();
            fetch();
        });
    });

    $("body").on("click", ".subject", function (e) {
        e.preventDefault();
        $(this).toggleClass("active");
        if ($(".active").length > 0) {
            $(".next").text("Mark these as photos");
        } else {
            $(".next").text("No photos");
        }
    });

    $.getJSON("/.auth/me")
        .success(function(userdata) {
            email = userdata[0].user_id;
        });
});