$(function () {
    onBetResize();
});

$(window).resize(function () {
    onBetResize();
});

function onBetResize() {
    h_align_window($(".info_container"));

    if ($("#teamBets").length > 0) {
        h_align_window($("#teamBets"));
        v_align_window($("#teamBets"));
        v_align($(".link"), $(".user"), $(".link").height());
        v_align($(".user > h2"), $(".user"), $(".user > h2").height());
        $("#bets_users").css("height", $("#teamBets").height() - $("#bets_header").outerHeight());
    }

    $(".info > div").each(function () {
        if ($(this).children().children().length > 0 && $(this).children().height() == $(this).height())
            v_align($(this).children().children(), $(this), getHeightOfChilds($(this).children()));
        else
            v_align($(this).children(), $(this), getHeightOfChilds($(this)))
    });
}

$(document).on("click", "#teamBets_close", function () {
    if ($("#teamBets").length > 0)
        $("#teamBets_container").remove();
});

$(document).on("click", ":not(#teamBets)", function (e) {
    var container = $("#teamBets");
    if (!container.is(e.target) && container.has(e.target).length === 0)
        $("#teamBets_container").remove();
});

$(document).on("click", ".team", function () {
    var team = $(this).find("> input").val();
    var match = $(this).closest(".info").find("> input").val();

    if (team != undefined && team != "" && match != undefined && match != "") {
        $.ajax({
            type: "GET",
            url: "/Bet/GetBetUsers",
            data: JSON.stringify({ TeamId: team, MatchId: match }),
            dataType: "json",
            success: function (data) {
                createTeamBets(data);
            },
            contentType: "application/json"
        });
    }
});

$(document).on("click", ".user", function () {
    var username = $(this).attr("id");

    if (username != undefined && username != "") {
        $("#bets_users > form > input").val(username);
        $("#bets_users > form").submit();
    }
});

$(document).on("click", "#showNextDay", function () {
    if ($(".team").length > 0) {
        var matchId = $(".team").last().closest(".info").find("> input").val();

        if (matchId != undefined && matchId != "") {
            $.ajax({
                type: "POST",
                url: "/Bet/GetNextDay",
                data: JSON.stringify({ lastMatchId: matchId }),
                dataType: "json",
                success: function (data) {
                    appendNextDayBets(data);
                },
                error: function (data) {
                    $("#showNextDay").remove();
                },
                contentType: "application/json"
            });
        }
        else
            $("#showNextDay").remove();
    }
});

function createTeamBets(data) {
    if ($("#teamBets").length > 0)
        $("#teamBets_container").remove();

    var text =
        '<div id="teamBets_container">\
                <div id="teamBets">\
                    <div id="bets_header">\
                        <h1>' + data.teamName + '</h1>\
                        <h3 class="grey">JOUEURS AYANT MISÉ SUR CETTE ÉQUIPE:</h3>\
                        <div id="teamBets_close"></div>\
                    </div>\
                    <div id="bets_users"></div>\
                </div>\
            </div>';

    $(text).prepend("body");

    data.users.each(function (i, user) {
        var userDiv =
            '<div class="user" id="' + user.username + '">\
                    <div class="bet_img"><img src="' + user.img + '"/></div>\
                    <h2>' + user.name + '</h2><div class="link"></div>\
                </div>';
        $("#bets_users").append(userDiv);
    });
}

function appendNextDayBets(data) {
    data.matches.each(function (i, match) {
        var match = '';

        $(match).insertBefore();
    });
}