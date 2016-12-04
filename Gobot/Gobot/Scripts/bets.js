$(function () {
    onBetResize();
});

$(window).resize(function () {
    onBetResize();
});

function onBetResize() {
    h_align_window($(".info_container"));
    posTeamBets();

    alignDivsElem();
}

function alignDivsElem() {
    $(".info > div").each(function () {
        if ($(this).children().children().length > 0 && $(this).children().height() == $(this).height())
            v_align($(this).children().children(), $(this), getHeightOfChilds($(this).children()));
        else
            v_align($(this).children(), $(this), getHeightOfChilds($(this)))
    });
}

function posTeamBets() {
    if ($("#teamBets").length > 0) {
        h_align_window($("#teamBets"));
        v_align_window($("#teamBets"));
        v_align($(".link"), $(".user"), $(".link").height());
        v_align($(".user > h2"), $(".user"), $(".user > h2").height());
        $("#bets_users").css("height", $("#teamBets").height() - $("#bets_header").outerHeight());
    }
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
    var teamName = $(this).find("> .text > h1").text();

    if (team != undefined && team != "" && match != undefined && match != "") {
        loading_create();
        $.ajax({
            type: "POST",
            url: "/Bet/GetBetUsers",
            data: JSON.stringify({ TeamId: team, MatchId: match }),
            dataType: "json",
            success: function (data) {
                loading_remove(true);
                createTeamBets(data, teamName);
            },
            error: function() {
                loading_remove(false);
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
        var href = window.location.href;
        href = href.substring(href.lastIndexOf("/") + 1).toLowerCase();
        var method = href.indexOf("history") > -1 ? "GetPreviousDay" : "GetNextDay";

        if (matchId != undefined && matchId != "") {
            loading_create();
            $.ajax({
                type: "POST",
                url: "/Bet/" + method,
                data: JSON.stringify({ lastMatchId: matchId }),
                dataType: "json",
                success: function (data) {
                    loading_remove(true);
                    appendNextDayBets((href.indexOf("bet") > -1 ? true : false), data);
                },
                error: function (data) {
                    $("#showNextDay").remove();
                    loading_remove(false);
                },
                contentType: "application/json"
            });
        }
        else
            $("#showNextDay").remove();
    }
});

function createTeamBets(data, teamName) {
    if ($("#teamBets").length > 0)
        $("#teamBets_container").remove();

    var text =
        '<div id="teamBets_container">\
                <div id="teamBets">\
                    <div id="bets_header">\
                        <h1>' + teamName + '</h1>\
                        <h3 class="grey">JOUEURS AYANT MISÉ SUR CETTE ÉQUIPE:</h3>\
                        <div id="teamBets_close"></div>\
                    </div>\
                    <div id="bets_users"><form action="/Account/Index" method="post"></form></div>\
                </div>\
            </div>';

    $("body").prepend(text);

    $.each(data.users, function (i, user) {
        var userDiv =
            '<div class="user" id="' + user.username + '">\
                    <img src="' + user.img + '"/>\
                    <h2>' + user.username + '</h2><div class="link"></div>\
                </div>';
        $("#bets_users > form").append(userDiv);
    });

    $("#bets_users > form").append('<input type="hidden" name="username" value=""/>');
    posTeamBets();
}

function appendNextDayBets(bet, data) {
    var day = '<div class="info day"><h2>' + data.date_day + '<grey>&nbsp;' + data.date_complete + '</grey></h2></div>';
    $(day).insertBefore("#showNextDay");

    $.each(data.matches, function (i, match) {
        var start = '<div class="info">';
        var time = '<div class="time sqr"><h3 ' + (getTeamBet(match) != undefined ? 'class="menuColor"' : '') + '>' + match.date + '</h3></div>';
        var team1 = getTeamText(match.teams[0]);
        var vs = '<div class="vs"><h1>VS</h1></div>';
        var manage = bet == true ? getManageText(match) : "";
        var team2 = getTeamText(match.teams[1]);
        var matchId = '<input name="matchId" type="hidden" value="' + match.id + '" />';
        var end = '</div>';

        $(start + time + team1 + vs + manage + team2 + matchId + end).insertBefore("#showNextDay");
    });

    var divs = $(".day").last().nextAll().not("#showNextDay");
    $.each(divs, function () {
        if ($(this).children().children().length > 0 && $(this).children().height() == $(this).height())
            v_align($(this).children().children(), $(this), getHeightOfChilds($(this).children()));
        else
            v_align($(this).children(), $(this), getHeightOfChilds($(this)))
    });
}

function zeros(n) {
    return n > 9 ? "" + n: "0" + n;
}

function getTeamBet(match) {
    var bet;
    if (match.teams[0].bet.user != undefined && match.teams[0].bet.user != "")
        bet = 0;
    else if (match.teams[1].bet.user != undefined && match.teams[1].bet.user != "")
        bet = 1;

    return bet;
}

function getManageText(match) {
    var start = '<div class="manageBet">';
    var bet = getTeamBet(match);

    if (bet != undefined) {
        return start + '<div class="remove sqr"><a href="/Bet/Remove?tId=' + match.teams[bet].id + '&mId=' + match.id + '"></a><div><h2>Retirer</h2></div></div>\
                <div class="edit sqr"><div><h2>Modifier</h2></div></div></div>';
    }
    else
        return start + '<div class="create sqr"><div><h2>Miser</h2></div></div></div>';
}

function getTeamText(team) {
    var start = '<div class="team team' + team.num + '"><div class="img" style="background-image: url(/Images/' + team.img + ');"></div><div class="text">';
    var text;
    if (team.bet.user != undefined && team.bet.user != "") {
        text = '<h1><green>' + team.name.toString().toUpperCase() + '</green></h1>\
                <h4 class="grey">Mise Totale<green> ' + team.bet.total + '</green>GC</h4>\
                <h4 class="grey">Votre mise<green> ' + team.bet.user + '</green>GC</h4>';
    }
    else {
        text = '<h1>' + team.name.toString().toUpperCase() + '</h1>\
                <h4 class="grey">Mise Totale<offw> ' + team.bet.total + '</offw>GC</h4>';
    }
    var end = '</div><input type="hidden" name="teamId" value="' + team.id + '" /></div>';

    return start + text + end;
}