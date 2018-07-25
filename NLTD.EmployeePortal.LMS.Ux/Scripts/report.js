function loadLateAndEarlyRpt() {

    if ($("#OnlyReportedToMe").val() == undefined) {
        var showTeam = false;
    }
    else {
        var showTeam = $("#OnlyReportedToMe").prop('checked');
    }
    if ($("#IsLeaveOnly").val() == undefined) {
        var leaveOnly = false;
    }
    else {
        var leaveOnly = $("#IsLeaveOnly").prop('checked');
    }
    if ($("#DonotShowRejected").val() == undefined) {
        var donotshowRejected = false;
    }
    else {
        var donotshowRejected = $("#DonotShowRejected").prop('checked');
    }
    if ($("#Name").val() == undefined) {
        var name = "";
    }
    else {
        if ($("#Name").val() != "") {
            var name = $("#Name").val().replace(/ /g, "|");
        }
        else {
            name = "";
        }
    }


    $("#divLoading").show();
    $("#divForLateandEarlyRpt")
        .load('/Report/loadLateAndEarlyRpt?Name=' + name + '&FromDate=' + $("#FromDate").val() + '&ToDate=' + $("#ToDate").val() + '&IsLeaveOnly=' + leaveOnly + '&OnlyReportedToMe=' + showTeam + '&reqUsr=' + $("#RequestLevelPerson").val() + '&DonotShowRejected=' + donotshowRejected,
        function () {
            $("#LateEarlyrpt").DataTable({ order: [[1, "asc"]]});
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 230  // Means Less header height
            }, 400);
        });

}

function LoadNoOfLateReport() {

    if ($("#OnlyReportedToMe").val() == undefined) {
        var showTeam = false;
    }
    else {
        var showTeam = $("#OnlyReportedToMe").prop('checked');
    }
    if ($("#IsLeaveOnly").val() == undefined) {
        var leaveOnly = false;
    }
    else {
        var leaveOnly = $("#IsLeaveOnly").prop('checked');
    }
    if ($("#DonotShowRejected").val() == undefined) {
        var donotshowRejected = false;
    }
    else {
        var donotshowRejected = $("#DonotShowRejected").prop('checked');
    }
    if ($("#Name").val() == undefined) {
        var name = "";
    }
    else {
        if ($("#Name").val() != "") {
            var name = $("#Name").val().replace(/ /g, "|");
        }
        else {
            name = "";
        }
    }

    $("#divLoading").show();
    $("#divForNoOfLateRpt")
        .load('/Report/LoadNoOfLateReport?Name=' + name + '&FromDate=' + $("#FromDate").val() + '&ToDate=' + $("#ToDate").val() + '&IsLeaveOnly=' + leaveOnly + '&OnlyReportedToMe=' + showTeam + '&reqUsr=' + $("#RequestLevelPerson").val() + '&DonotShowRejected=' + donotshowRejected,
        function () {
            $("#NoOfLate").DataTable({ order: [[1, "asc"]] });
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 230  // Means Less header height
            }, 400);
        });
}