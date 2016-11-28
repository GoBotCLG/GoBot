// GLOBAL FUNCTIONS
$(function () {
    readyResize();
});

$(window).resize(function () {
    readyResize();

    if ($("#loading").length > 0)
        v_align($(".loading"), $("#loading"), $(".loading").height());
});

function readyResize() {
    setErrorMessage();
    setPopUp();
}

function setErrorMessage() {
    if ($("#error_overlay").length > 0) {
        var y = $(window).height() / 2 - $("#error_overlay > div").height() / 2;

        $("#error_overlay > div").css("top", (y > 0 ? y : 0));
        $("#error_overlay").css("background-color", "rgba(0,0,0,0.85)");
    }
}

function setPopUp() {
    if ($(".popUp").length > 0) {
        h_align_window($(".popUp > .prompt"));
        h_align($(".popUp .lastMatch"), $(".popUp > .prompt"), $(".popUp .lastMatch").width());
    }
}

$(document).on("click", "#close_errorOverlay", function () {
    $("#error_overlay > div").animate({
        left: '-100%'
    }, 300, function () {
        $("#error_overlay").animate({
            padding: 0,
            height: 0
        }, 300, function () {
            $("#error_overlay").remove();
        });
    });
});

function h_align_window(e) {
    var windowWidth = $("#navBig").length == 0 ? $(window).width() : $(window).width() - $("#navBig").width();
    var pos = windowWidth / 2 - e.outerWidth() / 2;
    e.css("left", pos > 0 ? pos : 0);
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

    var numE = e.length;
    var eRow = windowWidth / e.outerWidth() > maxE ? maxE : Math.floor(windowWidth / e.outerWidth());

    if ((e.parent().width() > eRow * e.outerWidth()) || (e.parent().width() < eRow * e.outerWidth())) {
        var maxWidth = 0;
        var i = 0;
        e.each(function () {
            maxWidth += i < eRow ? $(this).outerWidth() : 0;
            $(this).css({
                top: -(Math.floor(i / eRow)),
                left: -(i % eRow)
            });
            i++;

        });
        e.parent().css("width", maxWidth);
    }

    h_align_window(e.parent());
}

function popUp(text, yesNo, custom, prioritize, buttonText) {
    var buttons = buttonText == undefined ? ["Oui", "Non"] : buttonText;

    if (prioritize || ($("#error_overlay").length == 0 && $(".popUp").length == 0)) {
        $(".popUp").remove();

        $("body").prepend('<div class="popUp ' + (yesNo ? 'yesNo' : '') + '"><div class="prompt">' + (!custom ? '<span>' + text + '</span>' : custom) + '</div></div>');

        if (yesNo)
            $(".popUp > .prompt").append('<div><button id="closePrompt">' + buttons[1] + '</button><button id="confirmPrompt">' + buttons[0] + '</button></div>');
        else
            $(".popUp > .prompt").append('<div><button id="closePrompt">Ok</button></div>');

        setPopUp();
        v_align($(".popUp .nextMatch .imgTeam"), $(".popUp .nextMatch .imgTeam").parent(), $(".popUp .nextMatch .imgTeam").height());

        $(".popUp").animate({
            height: '100%'
        }, 100, function () {
            $(".prompt").animate({
                padding: '25px 0',
                opacity: '1'
            }, 200);
        });

        return 1;
    }
    else
        return 0;
}

$(document).on("click", ".popUp > .prompt > div > button", function () {
    $(".prompt").animate({
        padding: '0',
        opacity: '0'
    }, 100);
    setTimeout(function () {
        $(".popUp").animate({
            height: '0'
        }, 100, function () {
            $(".popUp").remove();
        });
    }, 100);
});

function input_error_show(input, text, onTop) {
    if (input.next().hasClass("inputError"))
        input.next().remove();

    var id = "error_" + input.attr('name');
    $('<div class="inputError" id="' + id + '"><div class="popUp_triangle ' + (onTop ? 'below' : 'top') + '"></div><h3>' + text + '</h3></div>').insertAfter(input);

    var top = onTop ? input.position().top - input.outerHeight() - $("#" + id).height() + 6 : input.position().top + input.outerHeight();
    var margin = input.css("margin-left").replace("px", "");
    var left = input.position().left;
    $("#" + id).css({
        width: input.width() - 2,
        top: top,
        left: left + parseInt(margin)
    });
}

function input_error_hide(input) {
    if (input.next().hasClass("inputError"))
        input.next().remove();
}

function loading_create() {
    if ($("#loading").length > 0)
        $("#loading").remove();

    $("body").prepend('<div id="loading"><div class="loading"><div></div></div></div>');
    v_align($(".loading"), $("#loading"), $(".loading").height());
}

function loading_remove(success, redirect) {
    if ($("#loading").length > 0) {
        $(".loading").addClass(success ? "success" : "error").removeClass("loading");
        $("." + (success ? "success" : "error") + " > div").animate({
            "background-size": 20,
        }, 100, function () {
            $("." + (success ? "success" : "error") + " > div").animate({
                "background-size": 26,
            }, 50);
        });

        setTimeout(function () {
            $("#loading").remove();

            if (redirect !== undefined)
                window.location.href = redirect;
        }, 600);
    }
}