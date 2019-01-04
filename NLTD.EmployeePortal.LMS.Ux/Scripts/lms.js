$.ajaxSetup({ cache: false });
function LoadDashboardCalender() {
    $("#fullcalender").fullCalendar({
        selectable: true,
        selectHelper: true,
        firstDay: 0
    });
    $(".fc-sat").css("background-color", "#F4F3EA");
    $(".fc-sun").css("background-color", "#F4F3EA")
}

function InitDateTimeDropdown(varLeaveFromTime, varLeaveUptoTime) {
    if (varLeaveFromTime != "") {
        $("#LeaveFromTime").val(varLeaveFromTime);
    }
    if (varLeaveUptoTime != "") {
        $("#LeaveUptoTime").val(varLeaveUptoTime);
    }
}
function SubmitForm(e) {
    $('.close').click()
    if ($("#Reason").val() == "") {
        $('#alert_placeholder').append('<div id="alertDivId" class="alert alert alert-danger"><a class="close" data-dismiss="alert">×</a><span>' + "Please provide the reason for this request." + '</span></div>')
        return;
    }

    if (isTimeBasedLayout() == false) {
        $("#IsTimeBased").val("Day");
        var msg = "Are you sure you want to apply '" + $("#LeaveType option:selected").text() + "' for " + $("#NumberOfDays").val() + " day(s)?."
    }
    else {
        if ($("#PermissionTimeFrom").val() == "" || $("#PermissionTimeTo").val() == "") {
            $('#alert_placeholder').append('<div id="alertDivId" class="alert alert alert-danger"><a class="close" data-dismiss="alert">×</a><span>' + "Please select the correct time duration for the request." + '</span></div>')
            return;
        }
        $("#IsTimeBased").val("Time");
        var msg = "Are you sure you want to apply '" + $("#LeaveType option:selected").text() + "' from " + $("#PermissionTimeFrom").val() + " to " + $("#PermissionTimeTo").val() + " ?."
    }
    var msg =
        bootbox.confirm({
            title: "LMS Request Confirm",
            message: msg,
            buttons: {
                cancel: {
                    label: '<i class="fa fa-times"></i> Cancel'
                },
                confirm: {
                    label: '<i class="fa fa-check"></i> Confirm'
                }
            },
            callback: function (result) {
                if (result == true) {
                    $("#frmSubmitLeave").submit();
                }
            }
        });
}
function SubmitEmpForm(e) {
    $("#errorMessageArea").css("display", "none");
    $('.close').click();
    $("#alert_placeholder").empty();
    var corpId = $("#LogonId").val();
    var iserror = false;
    var errorMessage = "";

    if ($.trim($("#FirstName").val()) == "") {
        $("span[data-valmsg-for='FirstName']").text("Please enter first name.");
        iserror = true;
    }
    else {
        $("span[data-valmsg-for='FirstName']").text("");
    }

    if ($.trim($("#LastName").val()) == "") {
        $("span[data-valmsg-for='LastName']").text("Please enter last name.");
        iserror = true;
    } else {
        $("span[data-valmsg-for='LastName']").text("");
    }

    if ($.trim($("#EmployeeId").val()) == "") {
        $("span[data-valmsg-for='EmployeeId']").text("Please enter employee Id.");
        iserror = true;
    } else {
        $("span[data-valmsg-for='EmployeeId']").text("");
    }
    if ($("#Gender option:selected").index() < 1) {
        $("span[data-valmsg-for='Gender']").text("Please select employee gender.");
        iserror = true;
    } else {
        $("span[data-valmsg-for='Gender']").text("");
    }
    if ($("#RoleId option:selected").index() < 0) {
        $("span[data-valmsg-for='RoleId']").text("Please select LMS role.");
        iserror = true;
    } else {
        $("span[data-valmsg-for='RoleId']").text("");
    }

    if ($("#ReportedToId option:selected").index() < 1) {
        $("span[data-valmsg-for='ReportedToId']").text("Please select reporting to person name.");
        iserror = true;
    } else {
        $("span[data-valmsg-for='ReportedToId']").text("");
    }
    if ($("#OfficeId option:selected").index() < 0) {
        $("span[data-valmsg-for='OfficeId']").text("Please select office.");
        iserror = true;
    } else {
        $("span[data-valmsg-for='OfficeId']").text("");
    }
    if ($("#OfficeHolidayId option:selected").index() < 1) {
        $("span[data-valmsg-for='OfficeHolidayId']").text("Please select holiday office.");
        iserror = true;
    } else {
        $("span[data-valmsg-for='OfficeHolidayId']").text("");
    }
    if ($.trim($("#DOJ").val()) == "") {
        $("span[data-valmsg-for='DOJ']").text("Please enter joining date.");
        iserror = true;
    } else {
        $("span[data-valmsg-for='DOJ']").text("");
    }

    if (iserror == true) {
        $("#errorMessageArea").css("display", "block");
        showalert("", "Fix the error messages shown and try to save again.", "alert alert-danger");
        return;
    }

    if ($("#Mode").val() == "Add") {
        var msg = "Are you sure you want to add new employee profile?";
    }
    else {
        var msg = "Are you sure you want to update employee profile?";
    }
    bootbox.confirm({
        title: "Employee Profile Confirm",
        message: msg,
        buttons: {
            cancel: {
                label: '<i class="fa fa-times"></i> Cancel'
            },
            confirm: {
                label: '<i class="fa fa-check"></i> Confirm'
            }
        },
        callback: function (result) {
            if (result == true) {
                $("#frmEmployee").submit();
            }
        }
    });
}
function applyComplete(data) {
    if (data.responseJSON == "Saved") {
        resMessage = "Request submitted Successfully.";
        showalert("", resMessage, "alert alert-success");
        loadApplyLeaveSummary();
    }
    else if (data.responseJSON == "EmailFailed") {
        resMessage = "Request submitted Successfully,but sending email failed. Please inform your Reporting Person.";
        showalert("", resMessage, "alert alert-success");
        loadApplyLeaveSummary();
    }
    else {
        resMessage = data.responseJSON;
        showalert("", resMessage, "alert alert-danger")
    }
}

function toggleIcon(e) {
    $(e.target)
        .prev('.panel-heading')
        .find(".more-less")
        .toggleClass('glyphicon-plus glyphicon-minus');
}
$('.panel-group').on('hidden.bs.collapse', toggleIcon);
$('.panel-group').on('shown.bs.collapse', toggleIcon);

$(function () {
    $('#accordion').on('shown.bs.collapse', function (e) {
        var offset = $(this).find('.collapse.in').prev('.panel-heading');
        if (offset) {
            $('html,body').animate({
                scrollTop: $(offset).offset().top - 10
            }, 200);
        }
    });
});

function returnArray() {
    var offStr = $("#hdnWeekOff").val();
    if (offStr.length > 0) {
        var arr = offStr.split(',');
    }
    else
        return [];
    return arr;
}
function returnHolidayArray() {
    var offStr = $("#hdnHolidays").val();

    if (offStr.length > 0) {
        var arr = offStr.split(',');
    }
    else
        return [];
    return arr;
}

//function returnHolidayArray() {
//    var offStr = $("#hdnHolidays").val();
//    if (offStr.length > 0) {
//        var arr = offStr.split(',');
//    }
//    else
//        return [];
//    return arr;
//}
function loadLeaveSummary(userId) {
    $.ajax({
        type: 'GET',
        cache: false,
        url: "/Leaves/LoadLeaveSummaryFull",
        data: { "userId": userId },
        success: function (data) {
            $('#divForCreate' + userId).html(data);
        }
    });
}
function loadApplyLeaveSummary() {
    $.ajax({
        type: 'GET',
        cache: false,
        url: "/Leaves/LoadApplyLeaveSummary",
        success: function (data) {
            $('#divLeaveSummaryInApply').html(data);
        }
    });
}

function loadPendingLeaves() {
    if ($("#btnSearchPending").length == 1) {
        $.ajax({
            type: 'GET',
            cache: false,

            beforeSend: function () {
                $("#divLoading").show();
            },
            url: "/Leaves/LoadManageLeavePartial",
            data: { "ShowOnlyReportedToMe": $("#OnlyReportedToMe").prop('checked'), "ShowApprovedLeaves": $("#ShowApprovedLeaves").prop('checked'), "FromDate": $('#FromDate').val(), "ToDate": $('#ToDate').val() },
            success: function (data) {
                $('#divForPendingLeave').html(data);
            },
            complete: function () {
                $("#divLoading").hide();
            },
            error: function () {
                $("#divLoading").hide();
            }
        });
    }
}
function loadViewHistoryLeaves() {
    $("#alert_placeholder").empty();
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

    if ($("#Name").val() != undefined) {
        if (!ValidateAutocompleteName($("#Name").val(), $("#SearchUserID").val())) {
            Clearshowalert("Please Choose a valid Username from the List.", "alert alert-danger");
            return;
        }
    }

    $.ajax({
        type: 'GET',
        cache: false,
        beforeSend: function () {
            $("#divLoading").show();
        },
        url: "/Leaves/ViewLeaveHistory",
        data: {
            "OnlyReportedToMe": showTeam,
            "FromDate": $("#FromDate").val(),
            "ToDate": $("#ToDate").val(),
            "IsLeaveOnly": leaveOnly,
            "paramUserId": $("#SearchUserID").val(),
            "RequestMenuUser": $("#RequestLevelPerson").val()
        },
        success: function (data) {
            $('#divForHistoryLeave').html(data);
        },
        complete: function () {
            $("#divLoading").hide();
            if ($("#RequestLevelPerson").val() == "My") {
                $('[id^=collapse]').collapse("show");
            }
        },
        error: function () {
            $("#divLoading").hide();
        }
    });
}
function loadTeamProfiles() {
    if ($("#OnlyReportedToMe").val() == undefined) {
        var showTeam = false;
    }
    else {
        var showTeam = $("#OnlyReportedToMe").prop('checked');
    }
    if ($("#HideInactiveEmp").val() == undefined) {
        var hideInactive = true;
    }
    else {
        var hideInactive = $("#HideInactiveEmp").prop('checked');
    }

    if ($("#Name").val() != undefined) {
        if (!ValidateAutocompleteName($("#Name").val(), $("#SearchUserID").val())) {
            Clearshowalert("Please Choose a valid Username from the List.", "alert alert-danger");
            return;
        }
    }

    $.ajax({
        type: 'GET',
        cache: false,
        beforeSend: function () {
            $("#divLoading").show();
        },
        url: "/Profile/TeamProfileData",
        data: {
            "onlyReportedToMe": showTeam,
            "paramUserId": $("#SearchUserID").val(),
            "requestMenuUser": $("#RequestLevelPerson").val(),
            "hideInactiveEmp": hideInactive
        },
        success: function (data) {
            $('#divEmpProfile').html(data);
        },
        complete: function () {
            $("#divLoading").hide();
            if ($("#RequestLevelPerson").val() == "My") {
                $('[id^=collapse]').collapse("show");
            }
        },
        error: function () {
            $("#divLoading").hide();
        }
    });
}

function loadYearwiseLeaveSummary() {
    $("#alert_placeholder").empty();
    if ($("#Name").val() != undefined) {
        if (!ValidateAutocompleteName($("#Name").val(), $("#SearchUserID").val())) {
            Clearshowalert("Please Choose a valid Username from the List.", "alert alert-danger");
            return;
        }
    }

    if ($("#OnlyReportedToMe").val() == undefined) {
        var showTeam = false;
    }
    else {
        var showTeam = $("#OnlyReportedToMe").prop('checked');
    }

    $("#divLoading").show();
    $("#divForLeaveSummary")
        .load('/Admin/loadYearwiseLeaveSummary?Year=' + $("#Year").val() + '&reqUsr=' + $("#RequestLevelPerson").val() + '&paramUserId=' + $("#SearchUserID").val() + '&OnlyReportedToMe=' + showTeam,
        function () {
            $("#table_id").dataTable()
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 210  // Means Less header height
            }, 400);
        });
}
function loadMonthwiseCount() {
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
    if ($("#OnlyReportedToMe").val() == undefined) {
        var showTeam = false;
    }
    else {
        var showTeam = $("#OnlyReportedToMe").prop('checked');
    }
    $("#divLoading").show();
    $("#divForLeaveMonthCount")
        .load('/Admin/LoadMonthWiseLeaveCount?Year=' + $("#Year").val() + '&reqUsr=' + $("#RequestLevelPerson").val() + '&Name=' + name + '&OnlyReportedToMe=' + showTeam,
        function () {
            $('#Monthwisetable_id').DataTable({
                "scrollX": true,
                "bSort": false,
                "bAutoWidth": false,
                fixedColumns: {
                    leftColumns: 2,
                },
                "columnDefs": [
                    { "width": "200px", "targets": 1 }
                ],
            });

            $("#divLoading").hide();

            $('html, body').animate({
                scrollTop: 210  // Means Less header height
            }, 400);
        });
}
function callProfileEdit() {
    $('.close').click()
    if ($("#Name").val() == undefined) {
        var name = "";
    }
    else {
        $("#alert_placeholder").empty();
        if (name == "" && $("#RequestLevelPerson").val() != "My") {
            if ($('#alert') != undefined && $('#alert') != "") {
                $('#alert').remove();
            }
            Clearshowalert("Please enter the employee name.", "alert alert-danger");
            return;
        }
        if ($("#Name").val() != "") {
            var name = $("#Name").val().replace(/ /g, "|");
        }
        else {
            name = "";
            Clearshowalert("Please enter the employee name.", "alert alert-danger");
            return;
        }
    }
    $.ajax({
        method: "POST",
        url: '/Profile/CallProfileEdit?Name=' + $("#Name").val(),
        data: { name: name },
        success: function (result) {
            if (result == "InvalidName") {
                Clearshowalert("Employee profile not found for the entered name.", "alert alert-danger")
            }
            else {
                window.location.href = result.redirectToUrl
            }
        },
        error: function () {
        }
    });
}
function callProfileView() {
    $('.close').click()
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
    $.ajax({
        method: "POST",
        url: '/Profile/CallProfileView?Name=' + $("#Name").val(),
        data: { name: name },
        success: function (result) {
            if (result == "InvalidName") {
                showalert("", "Employee profile not found for the entered name.", "alert alert-danger")
            }
            else {
                window.location.href = result.redirectToUrl
            }
        },
        error: function () {
        }
    });
}
function callApplyFor() {
    $('.close').click()
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
    $.ajax({
        method: "POST",
        url: '/Leaves/CallApplyFor',
        data: { name: name },
        success: function (result) {
            if (result == "InvalidName") {
                showalert("", "Employee profile not found for the entered name.", "alert alert-danger")
            }
            else {
                window.location.href = result.redirectToUrl
            }
        },
        error: function () {
        }
    });
}
function loadPendingCount() {
    if ($("#hdnIsMLSApprvr").val() == "True") {
        $.ajax({
            method: "GET",
            url: '/DashBoard/LoadPendingCount',
            cache: false,
            success: function (response) {
                $('#divPendingCount').html(response);
            },
            complete: function () {
            },
            error: function () {
            }
        });
    }
}

function LoadTeamStatus() {
    if ($("#hdnIsMLSApprvr").val() == "True" ||
        $("#hdnUserRole").val() == "HR" ||
        $("#hdnUserRole").val().toUpperCase() == "ADMIN") {
        $.ajax({
            method: "GET",
            url: '/DashBoard/LoadTeamStatus',
            success: function (response) {
                $('#divTeamStatus').html(response);

                $("#TeamTimeSheetID").dataTable({
                    "bLengthChange": false,
                    "bInfo": false,
                    searching: false,
                    "paging": false
                });
            },
            complete: function () {
            },
            error: function () {
            }
        });
    }
}

function loadDaywiseLeaves() {
    $("#alert_placeholder").empty();

    if ($("#alert") != undefined) {
        $("#alert").remove();
    }

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
    if ($("#Name").val() != undefined) {
        if (!ValidateAutocompleteName($("#Name").val(), $("#SearchUserID").val())) {
            Clearshowalert("Please Choose a valid Username from the List.", "alert alert-danger");
            return;
        }
    }

    $("#divLoading").show();
    $("#divForDaywiseLeave")
        .load('/Admin/loadDaywiseLeaves?paramUserId=' + $("#SearchUserID").val() + '&FromDate=' + $("#FromDate").val() + '&ToDate=' + $("#ToDate").val() + '&IsLeaveOnly=' + leaveOnly + '&OnlyReportedToMe=' + showTeam + '&reqUsr=' + $("#RequestLevelPerson").val() + '&DonotShowRejected=' + donotshowRejected,
        function () {
            $("#Daywisetable_id").dataTable({
                "aaSorting": [], columnDefs: [
                    { type: 'date-eu', targets: 3 }
                ]
            })
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 230  // Means Less header height
            }, 400);
        });
}
function loadPermissionDetail() {
    $("#alert_placeholder").empty();

    if ($("#alert") != undefined) {
        $("#alert").remove();
    }

    if ($("#OnlyReportedToMe").val() == undefined) {
        var showTeam = false;
    }
    else {
        var showTeam = $("#OnlyReportedToMe").prop('checked');
    }
    if ($("#Name").val() != undefined) {
        if (!ValidateAutocompleteName($("#Name").val(), $("#SearchUserID").val())) {
            Clearshowalert("Please Choose a valid Username from the List.", "alert alert-danger");
            return;
        }
    }

    $("#divLoading").show();
    $("#divForPermissionDetail")
        .load('/Admin/GetPermissionDetail?paramUserId=' + $("#SearchUserID").val() + '&reqUsr=' + $("#RequestLevelPerson").val() + '&startDate=' + $("#FromDate").val() + '&endDate=' + $("#ToDate").val() + '&OnlyReportedToMe=' + showTeam,
        function () {
            $("#Permissions_id").dataTable({
                columnDefs: [
                    { type: 'date-eu', targets: 4 }]
            });
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 230 // Means Less header height
            }, 400);
        });
}
function loadOverTimePermissionDetail() {
    $("#alert_placeholder").empty();

    if ($("#alert") != undefined) {
        $("#alert").remove();
    }

    if ($("#OnlyReportedToMe").val() == undefined) {
        var showTeam = false;
    }
    else {
        var showTeam = $("#OnlyReportedToMe").prop('checked');
    }
    if ($("#Name").val() != undefined) {
        if (!ValidateAutocompleteName($("#Name").val(), $("#SearchUserID").val())) {
            Clearshowalert("Please Choose a valid Username from the List.", "alert alert-danger");
            return;
        }
    }

    $("#divLoading").show();
    $("#divForPermissionDetail")
        .load('/Admin/GetOverTimePermissionDetail?paramUserId=' + $("#SearchUserID").val() + '&reqUsr=' + $("#RequestLevelPerson").val() + '&startDate=' + $("#FromDate").val() + '&endDate=' + $("#ToDate").val() + '&OnlyReportedToMe=' + showTeam,
        function () {
            $("#Permissions_id").dataTable({
                columnDefs: [
                    { type: 'date-eu', targets: 4 }]
            });
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 230 // Means Less header height
            }, 400);
        });
}

function hideLeaveSplit(e) {
    $("#LeaveDtlSplit" + e).css("display", "none");
}
function hideLeaveCalculation() {
    $("#LeaveDtlSplit").css("display", "none");
}

function showLeaveDtlSplit(e) {
    $("#divLoading").show();

    $("#LeaveDtlSplit" + e)
        .load('/Leaves/ShowLeaveDetail?LeaveId=' + e,
        function () {
            $("#divLoading").hide();
            $("#LeaveDtlSplit" + e).toggle("fast");
        });
}

function LoadLeaveDtlSplit() {
    $("#divLoading").show();

    $("#LeaveDtlSplit")
        .load('/Leaves/GetLeaveDetailCalculation?LeaveFrom=' + $("#LeaveFrom").val() + '&LeaveUpto=' + $("#LeaveUpto").val() + '&LeaveFromTime=' + $("#LeaveFromTime").val() + '&LeaveUptoTime=' + $("#LeaveUptoTime").val() + '&LeaveTyp=' + $("#LeaveType").val(),
        function () {
            $("#divLoading").hide();
            $('#LeaveDtlSplit').toggle("fast");
        });
}

function hideSplitDiv() {
    $('#LeaveDtlSplit').toggle();
}

function ApproveLeave(LeaveId, userId) {
    $("[id=alertDivId]").hide();
    var obj = new Object();
    obj.LeaveId = LeaveId;
    obj.Comment = $("#Comment_" + LeaveId).val();
    obj.Status = "A";
    obj.userId = userId;
    if (obj.Comment == "") {
        showalert(userId, "Please enter approver's comment.", "alert alert-danger")
        return;
    }

    ChangeStatus(obj);
}
function CancelApprovedLeave(LeaveId, userId) {
    $("[id=alertDivId]").hide();
    var obj = new Object();
    obj.LeaveId = LeaveId;
    obj.Comment = $("#Comment_" + LeaveId).val();
    obj.Status = "C";
    obj.userId = userId;
    if (obj.Comment == "") {
        showalert(userId, "Please enter approver's comment.", "alert alert-danger")
        return;
    }

    ChangeStatus(obj);
}
function RejectLeave(LeaveId, userId) {
    $("[id=alertDivId]").hide();
    var obj = new Object();
    obj.LeaveId = LeaveId;
    obj.Comment = $("#Comment_" + LeaveId).val();
    if (obj.Comment == "") {
        showalert(userId, "Please enter approver's comment.", "alert alert-danger")
        return;
    }
    obj.Status = "R";
    obj.userId = userId;
    ChangeStatus(obj);
}
function ChangeStatus(obj) {
    var resMessage = "";
    var LeaveId = obj.LeaveId;
    $.ajax({
        method: "post",
        url: "/Leaves/ChangeStatus",
        data: obj,
        type: "json",
        beforeSend: function () {
            $("#divLoading").show();
        },
        success: function (response) {
            if (response == "Saved") {
                $("#Request-" + LeaveId).hide();
                loadLeaveSummary(obj.userId);
            }
            if (response = "Saved") {
                $("#divLoading").hide();
                if (obj.Status == "A") {
                    resMessage = "Request approved successfully."
                }
                else if (obj.Status == "R") {
                    resMessage = "Request rejected successfully."
                }
                else if (obj.Status == "C") {
                    resMessage = "Request cancelled successfully."
                }
                showalert(obj.userId, resMessage, "alert alert-success")
            }
            else if (response = "EmailFailed") {
                $("#divLoading").hide();
                if (obj.Status == "A") {
                    resMessage = "Request approved successfully, but Email sending failed."
                }
                else if (obj.Status == "R") {
                    resMessage = "Request rejected successfully, but Email sending failed."
                }
                else if (obj.Status == "C") {
                    resMessage = "Request cancelled successfully, but Email sending failed."
                }
                showalert(obj.userId, resMessage, "alert alert-success")
            }
            else if (response = "NotSaved") {
                $("#divLoading").hide();
                resMessage = "Leave status not changed.";
                showalert(obj.userId, resMessage, "alert alert-danger")
            }
        }
    });
}

function showalert(userId, message, alerttype) {
    if (userId == "") {
        $('#alert_placeholder').append('<div id="alertdiv" class="alert ' + alerttype + '"><a class="close" data-dismiss="alert">×</a><span>' + message + '</span></div>')
    }
    else {
        var alertDivId = "alertdiv" + userId;
        $('#alert_placeholder' + userId).append('<div id=alertDivId class="alert ' + alerttype + '"><a class="close" data-dismiss="alert">×</a><span>' + message + '</span></div>')
    }
}

function isTimeBasedLayout() {
    var arr = ($("#hdnTimebasedLeaveTypeIds").val()).split(',');
    var found = $.inArray($("#LeaveType").val(), arr);
    if (found == -1)
        return false;
    else
        return true;
}
function hourEntryLayout() {
    if (isTimeBasedLayout() == true) {
        $(".duration").hide();
        $(".timeentry").show();
        $(".fromentry").hide();
        
        $('#PermissionTimeFrom').timepicker({ 'scrollDefault': '10am' });
        $('#PermissionTimeTo').timepicker({ 'scrollDefault': '10am' });
        $("#LeaveUpto").val($("#LeaveFrom").val())
    }
    else {
        $(".duration").show();
        $(".timeentry").hide();
        $(".fromentry").show();
    }
    hideRuleText()
}
function hideRuleText() {
    if ($('#LeaveType :selected').text().indexOf("Sick") != -1)
        $('#divSickLeaveMsg > p').html("* Please submit a medical certificate to your manager for sick leaves greater than 3 days.");
    else if ($('#LeaveType :selected').text().indexOf("Compensatory Off") != -1)
        $('#divSickLeaveMsg > p').html("* Please provide the date against which the Compensatory Off is to be availed in the Reason.");  
    else if ($('#LeaveType :selected').text().indexOf("Over") != -1) {
        //$('#datepicker').datepicker('destroy');
        $('#divSickLeaveMsg > p').html("* Applies only to BPO employees.");
    }
    else
        $('#divSickLeaveMsg > p').html("");


    //$('.datepicker').datetimepicker(
    //            {
                   
    //                disabledDates: returnHolidayArray()
    //            }
    //            )

    //$('.datepicker').datetimepicker().
}
function CountLeaveDays() {
    if ($("#LeaveFrom").val() == $("#LeaveUpto").val()) {
        if ($("#LeaveUptoTime").val() == "F") {
            $("#LeaveUptoTime").val("A");
        }
    }
    hideElementsForHalfDay();

    if (isTimeBasedLayout() == true) {
        $("#LeaveUpto").val($("#LeaveFrom").val());
    }
    else {
        var duration = 0;
        $.ajax({
            url: "/Leaves/ReturnDuration",
            async: false,
            cache: false,
            method: "post",
            headers: {
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache',
                'Expires': '0'
            },
            beforeSend: function () {
            },
            data: { "LeaveFrom": $("#LeaveFrom").val(), "LeaveUpto": $("#LeaveUpto").val(), "LeaveFromTime": $("#LeaveFromTime").val(), "LeaveUptoTime": $("#LeaveUptoTime").val() },
            success: function (count) {
                duration = count;
            },
            error: function (exception) {
                alert(exception);
            },
            complete: function () {
            }
        });

        $("#NumberOfDays").val(duration);
    }
}
function hideElementsForHalfDay() {
    if (isTimeBasedLayout() == false) {
        if ($("#LeaveFromTime").val() == "F") {
            $(".halfday").hide()
            $(".firsthalfonly").hide()
            $("#LeaveUpto").val($("#LeaveFrom").val())
        }
        else {
            $(".halfday").show()
            $(".firsthalfonly").show()
        }
        if ($("#LeaveUpto").val() == $("#LeaveFrom").val()) {
            if ($("#LeaveFromTime").val() != "A") {
                $(".halfday").hide()
            }
            else {
                $(".halfday").show()
            }
        }
        else {
            if ($("#LeaveFromTime").val() != "F") {
                $(".halfday").show()
            }
        }
    }
}

function loadLeaveBalanceProfile() {
    SetUserIDForAutoCompleteName(nameList, $("#Name").val(), "UserID");
    if (!ValidateAutocompleteName($("#Name").val(), $("#UserID").val())) {
        Clearshowalert("Please Choose a valid Username from the List.", "alert alert-danger");
        return;
    }

    $("#alert_placeholder").empty();
    if ($("#Name").val() == "") {
        if ($('#alert') != undefined && $('#alert') != "") {
            $('#alert').remove();
        }
        Clearshowalert("Please enter the employee name.", "alert alert-danger");
        return;
    }
    $.ajax({
        type: 'GET',
        cache: false,
        beforeSend: function () {
            $("#divLoading").show();
        },
        url: "/Profile/EmployeeLeaveBalanceDetails",
        data: {
            "UserId": $("#UserID").val()
        },
        success: function (data) {
            $('#divEmpProfile').html(data);
        },
        complete: function () {
            $("#divLoading").hide();
        },
        error: function () {
            $("#divLoading").hide();
        }
    });
}

function AddTotalDays(index) {
    var CreditOrDebit = $('#CreditOrDebit' + index).val();

    var NoOfDays = 0;
    var BalanceDays = 0;

    if ($("#BalanceDays" + index).val() == undefined || $("#BalanceDays" + index).val() == '') {
        BalanceDays = 0;
    } else {
        BalanceDays = $("#BalanceDays" + index).val();
    }

    if ($("#NoOfDays" + index).val() == undefined || $("#NoOfDays" + index).val() == '') {
        NoOfDays = 0;
    } else {
        NoOfDays = $("#NoOfDays" + index).val();
        NoOfDays = (NoOfDays * 1).toString();
        $("#NoOfDays" + index).val(NoOfDays);
    }

    if (NoOfDays > 0) {
        if (NoOfDays.indexOf(".") > -1) {
            var decPart = (NoOfDays + "").split(".")[1];
            if (decPart != "0" && decPart != "5") {
                Clearshowalert("No of days after decimal point should be 0 or 5", "alert alert-danger");
                $("#NoOfDays" + index).focus();
                return;
            }
        }

        if (CreditOrDebit == 'D' && parseFloat(BalanceDays) < parseFloat(NoOfDays)) {
            Clearshowalert("No of days should be less than Existing Balance days", "alert alert-danger");
            $("#NoOfDays" + index).focus();
            return;
        }

        if (CreditOrDebit != '') {
            var Total = (parseFloat(NoOfDays) + parseFloat(BalanceDays)).toFixed(1);
            if (CreditOrDebit == 'D') {
                Total = (parseFloat(BalanceDays) - parseFloat(NoOfDays)).toFixed(1);
            }
            $("#TotalDays" + index).val(Total);
        }
        else {
            $("#TotalDays" + index).val(BalanceDays);
        }
    } else {
        $("#TotalDays" + index).val("");
    }
}

function isNumber(evt) {
    evt = (evt) ? evt : window.event;
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }
    return true;
}

function isNumberKey(evt, element) {
    var charCode = (evt.which) ? evt.which : event.keyCode;

    if (charCode > 31 && (charCode < 48 || charCode > 57) && !(charCode == 46))
        return false;
    else {
        var index = $(element).val().indexOf('.');
        if (index > 0 && charCode == 46) {
            return false;
        }
    }
    return true;
}

function SubmitLeaveBalanceForm(count) {
    var things = [];
    var valid = false;
    for (var i = 0; i < count; i++) {
        var CreditOrDebit = $('#CreditOrDebit' + i).val();

        if ($("#NoOfDays" + i).val() == undefined || $("#NoOfDays" + i).val() == '') {
            NoOfDays = 0;
        } else {
            NoOfDays = $("#NoOfDays" + i).val();
            NoOfDays = (NoOfDays * 1).toString();
            $("#NoOfDays" + i).val(NoOfDays);
        }

        var balanceDays = 0;

        if ($("#BalanceDays" + i).val() == undefined || $("#BalanceDays" + i).val() == '') {
            balanceDays = 0;
        } else {
            balanceDays = $("#BalanceDays" + i).val();
        }
        if (NoOfDays > 0) {
            valid = true;

            if (NoOfDays.indexOf(".") > -1) {
                var decPart = (NoOfDays + "").split(".")[1];
                if (decPart != "0" && decPart != "5") {
                    Clearshowalert("No of days after decimal point should be 0 or 5", "alert alert-danger");
                    $("#NoOfDays" + i).focus();
                    return;
                }
            }

            if (CreditOrDebit == 'D' && parseFloat(balanceDays) < parseFloat(NoOfDays)) {
                Clearshowalert("No of days should be less than Existing Balance days.", "alert alert-danger");
                $("#NoOfDays" + i).focus();
                return;
            }

            things.push(
                {
                    UserId: $("#UserId").val(),
                    LeaveType: $("#LeaveType" + i).val(), BalanceDays: balanceDays,
                    CreditOrDebit: CreditOrDebit, NoOfDays: NoOfDays, TotalDays: $("#TotalDays" + i).val(),
                    LeaveTypeId: $("#LeaveTypeId" + i).val(), LeaveBalanceId: $("#LeaveBalanceId" + i).val(), Remarks: $("#Remarks" + i).val()
                });
        } else {
            $("#TotalDays" + i).val("");
        }
    }

    if (valid) {
        var userid = $("#UserId").val();
        things = JSON.stringify({ 'lst': things, 'EmpUserid': userid });
        $.ajax({
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: 'POST',
            url: '/Profile/SaveLeaveBalance',
            data: things,
            success: function (result) {
                if (result == "Saved") {
                    $("#btnsave").attr("disabled", true);
                    Clearshowalert("Employee Leave Balance saved Successfully.", "alert alert-success");
                }
                else if (result == "Need Role") {
                    $("#btnsave").attr("disabled", true);
                    Clearshowalert("Only the user with role 'HR' is allowed to do this action.", "alert alert-danger");
                }
                else {
                    Clearshowalert(result, "alert alert-danger");
                }
            },
            failure: function (response) {
                Clearshowalert(response.message, "alert alert-danger");
            }
        });
    }
    else {
        Clearshowalert("No of Days should be greater than 0", "alert alert-danger");
        return;
    }
}

function Clearshowalert(message, alerttype) {
    $("#alert_placeholder").empty();
    $('#alert_placeholder').append('<div id="alertdiv" class="alert ' + alerttype + '"><span class="close" data-dismiss="alert">×</span><span>' + message + '</span></div>');
}

function loadTransactionLog() {
    if ($("#OnlyReportedToMe").val() == undefined) {
        var showTeam = false;
    }
    else {
        var showTeam = $("#OnlyReportedToMe").prop('checked');
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
    $("#alert_placeholder").empty();
    if (name == "" && $("#RequestLevelPerson").val() != "My") {
        if ($('#alert') != undefined && $('#alert') != "") {
            $('#alert').remove();
        }
        Clearshowalert("Please enter the employee name.", "alert alert-danger");
        return;
    }
    $("#divLoading").show();
    $("#divForHistoryLeave").load('/Admin/GetTransactionLog?OnlyReportedToMe=' + showTeam + '&Name=' + name + '&RequestMenuUser=' + $("#RequestLevelPerson").val(),
        function () {
            $(".transaction").dataTable({
                "paging": false, "bFilter": false, "bInfo": false, "aaSorting": [],
                columnDefs: [
                    { type: 'date-eu', targets: 1 },
                    { type: 'date-eu', targets: 2 },
                    { type: 'date-eu', targets: 3 }
                ]
            });
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 210  // Means Less header height
            }, 400);
        });
}

function loadAttendanceRangeSummary() {
    $("#alert_placeholder").empty();
    try {
        $("#showalert").empty();
    }
    catch (e) {
    }

    var myDirectEmployees = false;

    if ($("#RequestLevelPerson").val() === "Team") {
        myDirectEmployees = $("#mydirectemployeecheck").is(':checked');
    }
    if ($("#RequestLevelPerson").val() === "My") {
        URL = '/Admin/loadEmployeeAttendance?&FromDate=' + $('#FromDate').val() + '&ToDate=' + $('#ToDate').val() + '&requestLevelPerson=' + $('#RequestLevelPerson').val() + '&myDirectEmployees=' + myDirectEmployees;
    }
    else {
        URL = '/Admin/loadEmployeeAttendance?ID=' + $("#UserID").val() + '&FromDate=' + $('#FromDate').val() + '&ToDate=' + $('#ToDate').val() + '&requestLevelPerson=' + $('#RequestLevelPerson').val() + '&myDirectEmployees=' + myDirectEmployees;
        if (!ValidateAutocompleteName($("#Name").val(), $("#UserID").val())) {
            Clearshowalert("Please Choose a valid Username from the List.", "alert alert-danger");
            return;
        }
    }
    $("#divLoading").show();
    $("#divForEmployeeAttendance")
        .load(URL,
        function (responseText, textStatus, req) {
            $("#divLoading").hide();
            if (textStatus == "error") {
                Clearshowalert("No Records Found", "alert alert-danger");
                $('#Attendancetable_id').DataTable().clear().destroy();
            }
            else {
                $(".dtatable").dataTable({
                    "aaSorting": [],
                    columnDefs: [
                        { type: 'date-eu', targets: 0 }
                    ]
                });
                $('html, body').animate({
                    scrollTop: 230  // Means Less header height
                }, 400);
            }
        });
}

function loadAccesCardAttendanceRangeSummary() {
    $("#alert_placeholder").empty();
    try {
        $("#showalert").empty();
    }
    catch (e) {
    }

    URL = '/Admin/loadAccesCardEmployeeAttendance?ID=' + $("#CardID").val() + '&FromDate=' + $('#FromDate').val() + '&ToDate=' + $('#ToDate').val() + '&requestLevelPerson=' + $('#RequestLevelPerson').val();

    var x = $("#CardID").val();
    if (isNaN(x)) {
        Clearshowalert("Please enter a valid Card Id.", "alert alert-danger");
        return;
    }

    $("#divLoading").show();
    $("#divForEmployeeAttendance")
        .load(URL,
        function (responseText, textStatus, req) {
            $("#divLoading").hide();
            if (textStatus == "error") {
                Clearshowalert("No Records Found", "alert alert-danger");
                $('#Attendancetable_id').DataTable().clear().destroy();
            }
            else {
                $(".dtatable").dataTable({
                    "aaSorting": [],
                    columnDefs: [
                        { type: 'date-eu', targets: 0 }
                    ]
                });
                $('html, body').animate({
                    scrollTop: 230  // Means Less header height
                }, 400);
            }
        });
}

function loadTimeSheetSummary() {
    $("#alert_placeholder").empty();
    var URL = '/Admin/LoadMyTeamTimesheet';

    var myDirectEmployees = false;

    if ($("#RequestLevelPerson").val() === "My") {
        URL = '/Admin/LoadMyTimesheet';
    }
    else {
        SetUserIDForAutoCompleteName(nameList, $("#Name").val(), "UserID");
        if (!ValidateAutocompleteName($("#Name").val(), $("#UserID").val())) {
            $("#divForTimesheet").html("");
            $("#showalert").empty();
            Clearshowalert("Please choose the Username from the List.", "alert alert-danger");
            return;
        }
    }

    if ($("#RequestLevelPerson").val() === "Team") {
        myDirectEmployees = $("#mydirectemployeecheck").is(':checked');
    }

    $("#divForTimesheet").html("");
    $("#alert_placeholder").html("");

    $("#divLoading").show();
    $("#divForTimesheet")
        .load(URL, {
            TimeSheetQueryModelObj: {
                FromDate: $("#FromDate").val(), ToDate: $("#ToDate").val(),
                Name: $("#Name").val(), MyDirectEmployees: myDirectEmployees,
                UserID: $("#UserID").val()
            }
        },
        function (responseText, textStatus, req) {
            if (textStatus == "error") {
                Clearshowalert("No Records Found", "alert alert-danger");
                $('.dtatable').DataTable().clear().destroy();
            }
            else {
                $(".dtatable").dataTable({
                    "aaSorting": [], "pageLength": 50,
                    columnDefs: [
                        { type: 'date-eu', targets: 0 }
                    ]
                });
                $('html, body').animate({
                    scrollTop: 230  // Means Less header height
                }, 400);
            }
            $("#divLoading").hide();
        });
}

function loadShiftMasterDetails() {
    $("#divLoading").show();
    $("#divForShiftAllocation").load('/Shift/GetShiftMasterDetail',
        function () {
            $("#ShiftDetail").dataTable({
                columnDefs: [
                    { targets: 'no-sort', orderable: false }
                ]
            });
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 230 // Means Less header height
            }, 400);
        });
}

function loadEarnedLeaveMasterDetails() {
    $("#divLoading").show();
    $("#divForEarnedLeaveCreditAllocation").load('/Profile/GetEarnedLeaveMasterDetail',
        function () {
            $("#earnedLeaveDetail").dataTable({
                "paging": false,
                aaSorting: [1, 'asc'],
                columnDefs: [
                    { targets: 'no-sort', orderable: false }
                ]
            });
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 230 // Means Less header height
            }, 400);
        });
}

function processELCredit() {
    bootbox.confirm({
        title: "Confirm EL Credit Process",
        message: "Are you sure you want to process the EL Credit?",
        buttons: {
            cancel: {
                label: '<i class="fa fa-times"></i> Cancel'
            },
            confirm: {
                label: '<i class="fa fa-check"></i> Confirm'
            }
        },
        callback: function (result) {
            if (result == true) {
                $("#divLoading").show();
                $.ajax({
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    type: 'POST',
                    url: '/Profile/UpdateEarnedLeaves',
                    success: function (result) {
                        if (result == "Saved") {
                            $("#elCreditMsgSuccess").show();
                            $("#divLoading").hide();
                            $("#idELStatus").hide();
                            $("#divForEarnedLeaveCreditAllocation").hide();
                            $("#elCreditOperaions").hide();
                        }
                        else {
                            $("#elCreditMsgError").show();
                            $("#divLoading").hide();
                            Clearshowalert(result, "alert alert-danger");
                        }
                    },
                    failure: function (response) {
                        $("#elCreditMsgError").show();
                        $("#divLoading").hide();
                        Clearshowalert(response.message, "alert alert-danger");
                    }
                });
            }
        }
    });
}

function AddShiftPopup(shiftId) {
    $("#alert_placeholder").empty();
    $("#divLoading").show();
    $("#ModelTitle").html("Add New Shift");
    if (shiftId !== 0) {
        $("#ModelTitle").html("Edit Shift");
    }

    $("#divForAddShift").load('/Shift/GetShiftMasterDetailwithId?shiftId=' + shiftId,
        function () {
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 230 // Means Less header height
            }, 400);
        });
    $('#myModal').on('shown.bs.modal', function (e) {
        $('.timepicker').timepicker({
            timeFormat: "HH:mm"
        }
        );
    });
    $("#myModal").modal('show');
}

function SaveShiftMaster() {
    var shiftName, fromTime, toTime, shiftId;
    shiftName = $("#ShiftName").val().trim();
    shiftId = $("#ShiftId").val().trim();
    fromTime = $("#fromTime").val().trim();
    toTime = $("#toTime").val().trim();

    if (fromTime == '') {
        Clearshowalert("Please enter the Start Time.", "alert alert-danger");
        return;
    }

    if (toTime == '') {
        Clearshowalert("Please enter the End Time.", "alert alert-danger");
        return;
    }

    if (fromTime == toTime) {
        Clearshowalert("Start time and End time should not be same.", "alert alert-danger");
        return;
    }

    $("#divLoading").show();
    var things = JSON.stringify({ 'shiftId': shiftId, 'shiftName': shiftName, 'fromTime': fromTime, 'toTime': toTime });
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: '/Shift/SaveShiftMaster',
        data: things,
        success: function (result) {
            if (result == "Saved") {
                $("#btnsave").attr("disabled", true);
                window.location.reload();
            }
            else if (result === "Need Role") {
                $("#btnsave").attr("disabled", true);
                Clearshowalert("Only the user with role 'HR' is allowed to do this action.", "alert alert-danger");
            }
            else {
                Clearshowalert(result, "alert alert-danger");
            }
        },
        failure: function (response) {
            Clearshowalert(response.message, "alert alert-danger");
        }
    });

    $("#divLoading").hide();
}
function loadShiftDetails() {
    $("#divLoading").show();
    var RequestLevelPerson = $("#RequestLevelPerson").val();

    $("#divForShiftAllocation").load('/Shift/GetShiftDetail?RequestMenuUser=' + RequestLevelPerson,
        function () {
            $("#ShiftDetail").dataTable({
                columnDefs: [
                    { targets: 'no-sort', orderable: false }
                ]
            });
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 230 // Means Less header height
            }, 400);
        });
}

function loadEmployeeShifts() {
    $("#divLoading").show();
    var RequestLevelPerson = $("#RequestLevelPerson").val();
    var table = null;
    $("#divForAddShift").load('/Shift/GetShiftDetailsForUsers?RequestMenuUser=' + RequestLevelPerson,
        function () {
            table = $("#addShiftDetail").DataTable(
                {
                    columnDefs: [
                        { targets: 'no-sort', orderable: false, searchable: false }
                    ],
                    order: [[1, 'asc']],
                    stateSave: true
                });
            $("#divLoading").hide();

            $('#select-all').on('click', function () {
                var rows = table.rows({ 'search': 'applied' }).nodes();
                $('input[type="checkbox"]', rows).prop('checked', this.checked);
            });
            $('html, body').animate({
                scrollTop: 230,
            }, 400);
        });

    $("#alert_placeholder").empty();
}

function toDate(dateStr) {
    var parts = dateStr.split("-");
    return new Date(parts[2], parts[1] - 1, parts[0]);
}
function SaveEmployeeShift() {
    var checkedValues = table.$('input:checkbox:checked').map(function () {
        return $(this).val();
    }).get();

    $("#alert_placeholder").empty();

    var Shift = $("#Shift").val();
    var FromDate = $("#FromDate").val();
    var ToDate = $("#ToDate").val();
    var RequestLevelPerson = $("#RequestLevelPerson").val();

    if (Shift == '') {
        Clearshowalert("Please select the Shift.", "alert alert-danger");
        return;
    }

    if (FromDate == '' || ToDate == '') {
        Clearshowalert("Please select From Date and To Date.", "alert alert-danger");
        return;
    }

    var from = toDate(FromDate);
    var to = toDate(ToDate);

    if (from > to) {
        Clearshowalert("Invalid Date Range.", "alert alert-danger");
        return;
    }

    if (checkedValues == '' || checkedValues == null) {
        Clearshowalert("Please select atleast one employee.", "alert alert-danger");
        return;
    }

    var input = JSON.stringify({
        'UserId': checkedValues, 'Shift': Shift, 'FromDate': FromDate, 'ToDate': ToDate, 'RequestMenuUser': RequestLevelPerson
    });
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: '/Shift/SaveEmployeeShift',
        data: input,
        success: function (result) {
            if (result == "Saved") {
                Clearshowalert("Employees Shift updated successfully.", "alert alert-success");
            }
            else {
                Clearshowalert(result, "alert alert-danger");
            }
        },
        failure: function (response) {
            Clearshowalert(response.message, "alert alert-danger");
        }
    });
}

function GetEmployeeShiftDetails(FromDate, ToDate, Shift) {
    var UserId = ($("#RequestLevelPerson").val() == "My") ? 0 : $("#UserID").val();

    if ($("#RequestLevelPerson").val() != "My") {
        SetUserIDForAutoCompleteName(nameList, $("#Name").val(), "UserID");
        if (!ValidateAutocompleteName($("#Name").val(), $("#UserID").val())) {
            Clearshowalert("Please Choose a valid Username from the List.", "alert alert-danger");
            return;
        }

        UserId = $("#UserID").val();
    }

    $("#alert_placeholder").empty();
    if ($("#Name").val() == "" && $("#RequestLevelPerson").val() != "My") {
        if ($('#alert') != undefined && $('#alert') != "") {
            $('#alert').remove();
        }
        Clearshowalert("Please enter the employee name", "alert alert-danger");
        return;
    }

    $("#divLoading").show();
    $("#divForHistoryLeave").load('/Shift/GetEmployeeShiftDetails?UserId=' + UserId + '&RequestMenuUser=' + $("#RequestLevelPerson").val() + '&FromDate=' + FromDate + '&ToDate=' + ToDate + '&Shift=' + Shift + '&rnd=' + Math.round(Math.random() * 10000),
        function () {
            $(".shift").dataTable({ pageLength: 50, bPaginate: false, bInfo: false });
            $("#divLoading").hide();
            $('html, body').animate({
                scrollTop: 210  // Means Less header height
            }, 400);
        });
}

function SaveIndividualEmployeeShift() {
    $("#alert_placeholder").empty();
    var Shift = $("#Shift").val();
    var FromDate = $("#FromDate").val();
    var ToDate = $("#ToDate").val();
    var UserId = $("#UserId").val();
    var RequestLevelPerson = $("#RequestLevelPerson").val();

    if (Shift == '') {
        Clearshowalert("Please select the Shift", "alert alert-danger");
        return;
    }

    if (FromDate == '' || ToDate == '') {
        Clearshowalert("Please select From Date and To Date", "alert alert-danger");
        return;
    }

    var from = toDate(FromDate);
    var to = toDate(ToDate);

    if (from > to) {
        Clearshowalert("Invalid Date Range", "alert alert-danger");
        return;
    }

    var input = JSON.stringify({
        'FromDate': from, 'ToDate': to, 'Shift': Shift, 'UserId': UserId, 'RequestMenuUser': RequestLevelPerson
    });
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: '/Shift/SaveIndividualEmployeeShift',
        data: input,
        success: function (result) {
            if (result == "Saved") {
                GetEmployeeShiftDetails(FromDate, ToDate, Shift);
                Clearshowalert("Shift updated Successfully.", "alert alert-success");
            }
            else {
                Clearshowalert(result, "alert alert-danger");
            }
        },
        failure: function (response) {
            Clearshowalert(response.message, "alert alert-danger");
        }
    });
}
function ValidateAutocompleteName(name, userID) {
    if (name != "") {
        if (userID == "") {
            return false;
        }
    }
    if (name == "") {
        if (userID != "") {
            $("#SearchUserID").val("");
            $("#UserID").val("");
        }
    }
    return true;
}

function ValidateAceessCardNumber(CardId, userID) {
    if (CardId == "") {
        if (userID != "") {
            $("#SearchUserID").val("");
            $("#UserID").val("");
        }
    }
    return true;
}

function SetUserIDForAutoCompleteName(userList, name, hiddenFieldID) {
    var user = $.grep(userList, function (user) {
        if (user.label.trim() == name.trim()) {
            return user.value;
        }
    });
    if (user.length > 0) {
        $("#" + hiddenFieldID).val(user[0].value);
    }
}