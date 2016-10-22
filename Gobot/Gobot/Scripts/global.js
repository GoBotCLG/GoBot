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

function h_align_window(e) {
    var pos = ($(window).width() - $("#navBig").width()) / 2 - e.width() / 2;
    e.css("left", pos);
}
function v_align_window(e) {
    var pos = $(window).height() / 2 - e.height() / 2;
    e.css("top", pos);
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
function h_align_margin(e, p, eW) {
    var pos = p.width() / 2 - eW / 2;
    e.css("margin-left", pos > 0 ? pos : 0);
}

function getHeightOfChilds(e) {
    var totalHeight = 0;

    e.children().each(function () {
        totalHeight += $(this).outerHeight(true);
    });

    return totalHeight;
}

function getWidthOfChilds(e) {
    var totalWidth = 0;

    e.children().each(function () {
        totalWidth += $(this).outerWidth(true);
    });

    return totalWidth;
}

function setWidthFromChilds(e, maxE) {
    var padding = e.parent().css("padding-left").replace("px", "");
    var windowWidth = $("#navBig").length == 0 ? $(window).width() : $(window).width() - $("#navBig").width();
    windowWidth -= padding * 2; // padding on each side
    //var maxWidth = e.parent().find(e).length * e.width() * maxE;

    var numE = e.parent().find("> " + e.attr("class")).length;
    var eRow = windowWidth / maxE > maxE ? maxE : Math.floor(windowWidth / maxE);
    var maxWidth = eRow * e.outerWidth();

    
}
