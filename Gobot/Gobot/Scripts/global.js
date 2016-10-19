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

$(document).on("click", "#close_errorOverlay", function () {
    $("#error_overlay").remove();
});

function h_align_window(elem) {
    var pos = ($(window).width() - $("#navBig").width()) / 2 - elem.width() / 2;
    elem.css("left", pos);
}
function v_align_window(elem, parent) {
    var pos = $(window).height() / 2 - elem.height() / 2;
    elem.css("top", pos);
}
function v_align(e, p, eH) {
    var pos = p.height() / 2 - eH / 2;
    e.css("top", pos > 0 ? pos : 0);
}
function v_align_margin(e, p, eH) {
    var pos = p.height() / 2 - eH / 2;
    e.css("margin-top", pos > 0 ? pos : 0);
}
function h_align(e, p) {
    var pos = p.width() / 2 - e.width() / 2;
    e.css("left", pos > 0 ? pos : 0);
}
function h_align(e, p, eW) {
    var pos = p.width() / 2 - eW / 2;
    e.css("left", pos > 0 ? pos : 0);
}

function getHeightOfChilds(e) {
    var totalHeight = 0;

    e.children().each(function () {
        totalHeight += $(this).outerHeight(true);
    });

    return totalHeight;
}