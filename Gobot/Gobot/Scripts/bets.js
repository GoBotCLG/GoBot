$(function () {
    onBetResize();
});

$(window).resize(function () {
    onBetResize();
});

function onBetResize() {
    h_align_window($(".info_container"));
    posTeamBets();

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

        if (matchId != undefined && matchId != "") {
            loading_create();
            $.ajax({
                type: "POST",
                url: "/Bet/GetNextDay",
                data: JSON.stringify({ lastMatchId: matchId }),
                dataType: "json",
                success: function (data) {
                    loading_remove(true);
                    appendNextDayBets(data);
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
                    <div id="bets_users"><form action="/Bet/GetBetUser" method="post"></form></div>\
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

    posTeamBets();
}

function appendNextDayBets(data) {
    var days = ["Dimanche", "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi"];
    var date = new Date(data.date); // data.date == milliseconds
    var day = '<div class="info day"><h2>' + days[date.getDay()] + '<grey>' + zeros(date.getDate()) + '-' + zeros(date.getMonth()) + '-' + zeros(date.getFullYear()) + '</grey></h2></div>';
    $(day).insertBefore("#showNextDay");

    $.each(data.matches, function (i, match) {
        var mDate = new Date(match.date); // match.date == milliseconds

        var start = '<div class="info">';
        var time = '<div class="time sqr"><h3>' + mDate.getHours() + ':' + zeros(mDate.getMinutes()) + '</h3></div>';
        var team1 = getTeamText(match.teams[0]);
        var vs = '<div class="vs"><h1>VS</h1></div>';
        var manage = getManageText(match);
        var team2 = getTeamText(match.teams[1]);
        var matchId = '<input name="matchId" type="hidden" value="' + match.id + '" />';
        var end = '</div>';

        $(start + time + team1 + vs + manage + team2 + matchId + end).insertBefore("#showNextDay");
    });
}

function zeros(n) {
    return n > 9 ? "" + n: "0" + n;
}

function getManageText(match) {
    var start = '<div class="manageBet">';

    var bet;
    if (match.teams[0].bet.user != undefined && match.teams[0].bet.user != "")
        bet = 0;
    else if (match.teams[1].bet.user != undefined && match.teams[1].bet.user != "")
        bet = 1;

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