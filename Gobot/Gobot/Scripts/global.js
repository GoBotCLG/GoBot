// GLOBAL FUNCTIONS
$(function () {
    readyResize();
});

$(window).resize(function () {
    readyResize();
});

function readyResize() {
    setErrorMessage();
}


// Error message
function setErrorMessage() {
    if ($("#error_overlay").length > 0) {
        var y = $(window).height() / 2 - $("#error_overlay > div").height() / 2;

        $("#error_overlay > div").css("top", (y > 0 ? y : 0));
        $("#error_overlay").css("background-color", "rgba(0,0,0,0.85)");
    }
}

$("#close_errorOverlay").click(function () {
    $("#error_overlay").remove();
});