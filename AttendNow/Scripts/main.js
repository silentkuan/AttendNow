//------------------------------------------------------//
//------------------Multiple Page Use-------------------//
//------------------------------------------------------//


//----------------User Friendly Features Part Start----------------------//
//when click enter will go next input

$(".form-control").keypress(function (e) {
    if (e.which === 13) { // Check if Enter key is pressed (key code 13)
        e.preventDefault(); // Prevent the default behavior (form submission)

        var $this = $(this);
        var nextInput = $this.closest('form').find('.form-control').eq($this.index('.form-control') + 1);

        if (nextInput.length) {
            // Check if the next input is a dropdown (select element)
            
                nextInput.focus();
           
        }
    }
});

//click to check all checkbox
function selectAllCheckboxes(checkbox, checkboxOption) {
    var checkboxes = document.getElementsByName(checkboxOption);
    for (var i = 0; i < checkboxes.length; i++) {
        checkboxes[i].checked = checkbox.checked;
    }
    handleCheckboxChange(checkboxOption);
}
function uncheckAllCheckboxes(checkboxOption) {
    var checkboxes = document.getElementsByName(checkboxOption);
    for (var i = 0; i < checkboxes.length; i++) {
        checkboxes[i].checked = false;
    }
    handleCheckboxChange(checkboxOption);
}

function selectAllPermission(checkbox) {
    $('#creator-favourite').prop('checked', false);
    $('#editor-favourite').prop('checked', false);
    if (checkbox.checked) {
        $('.icheckbox_square-green').addClass('checked');
        $('.i-checks').prop('checked', true);
    } else {
        $('.icheckbox_square-green').removeClass('checked');
        $('.i-checks').prop('checked', false);
        
       
    }


}

function selectCreatorPermission(checkbox) {
    $('#admin-favourite').prop('checked', false);
    $('#editor-favourite').prop('checked', false);
    if (checkbox.checked) {
        $('.icheckbox_square-green').removeClass('checked');
        $('.i-checks').prop('checked', false);
        $('#participantStatus').parent().addClass('checked');
        $('#participantStatus').prop('checked', true);

        $('#participantViewFunction').parent().addClass('checked');
        $('#participantViewFunction').prop('checked', true);

        $('#participantAddFunction').parent().addClass('checked');
        $('#participantAddFunction').prop('checked', true);

        $('#participantEditFunction').parent().addClass('checked');
        $('#participantEditFunction').prop('checked', true);

        $('#participantDeleteFunction').parent().addClass('checked');
        $('#participantDeleteFunction').prop('checked', true);


        $('#meetingStatus').parent().addClass('checked');
        $('#meetingStatus').prop('checked', true);

        $('#meetingViewFunction').parent().addClass('checked');
        $('#meetingViewFunction').prop('checked', true);

        $('#meetingAddFunction').parent().addClass('checked');
        $('#meetingAddFunction').prop('checked', true);

        $('#meetingEditFunction').parent().addClass('checked');
        $('#meetingEditFunction').prop('checked', true);

        $('#meetingDeleteFunction').parent().addClass('checked');
        $('#meetingDeleteFunction').prop('checked', true);
    } else {
        $('#participantStatus').parent().removeClass('checked');
        $('#participantStatus').prop('checked', false);

        $('#participantViewFunction').parent().removeClass('checked');
        $('#participantViewFunction').prop('checked', false);

        $('#participantAddFunction').parent().removeClass('checked');
        $('#participantAddFunction').prop('checked', false);

        $('#participantEditFunction').parent().removeClass('checked');
        $('#participantEditFunction').prop('checked', false);

        $('#participantDeleteFunction').parent().removeClass('checked');
        $('#participantDeleteFunction').prop('checked', false);


        $('#meetingStatus').parent().removeClass('checked');
        $('#meetingStatus').prop('checked', false);

        $('#meetingViewFunction').parent().removeClass('checked');
        $('#meetingViewFunction').prop('checked', false);

        $('#meetingAddFunction').parent().removeClass('checked');
        $('#meetingAddFunction').prop('checked', false);

        $('#meetingEditFunction').parent().removeClass('checked');
        $('#meetingEditFunction').prop('checked', false);

        $('#meetingDeleteFunction').parent().removeClass('checked');
        $('#meetingDeleteFunction').prop('checked', false);

    }
   
}


function selectEditorPermission(checkbox) {
    $('#creator-favourite').prop('checked', false);
    $('#admin-favourite').prop('checked', false);
    if (checkbox.checked) {
        $('.icheckbox_square-green').removeClass('checked');
        $('.i-checks').prop('checked', false);
        $('#participantStatus').parent().addClass('checked');
        $('#participantStatus').prop('checked', true);

        $('#participantViewFunction').parent().addClass('checked');
        $('#participantViewFunction').prop('checked', true);

        $('#participantAddFunction').parent().addClass('checked');
        $('#participantAddFunction').prop('checked', true);

        $('#participantEditFunction').parent().addClass('checked');
        $('#participantEditFunction').prop('checked', true);

        

        $('#meetingStatus').parent().addClass('checked');
        $('#meetingStatus').prop('checked', true);

        $('#meetingViewFunction').parent().addClass('checked');
        $('#meetingViewFunction').prop('checked', true);

        $('#meetingAddFunction').parent().addClass('checked');
        $('#meetingAddFunction').prop('checked', true);

        $('#meetingEditFunction').parent().addClass('checked');
        $('#meetingEditFunction').prop('checked', true);

       
    } else {
        $('#participantStatus').parent().removeClass('checked');
        $('#participantStatus').prop('checked', false);

        $('#participantViewFunction').parent().removeClass('checked');
        $('#participantViewFunction').prop('checked', false);

        $('#participantAddFunction').parent().removeClass('checked');
        $('#participantAddFunction').prop('checked', false);

        $('#participantEditFunction').parent().removeClass('checked');
        $('#participantEditFunction').prop('checked', false);

     

        $('#meetingStatus').parent().removeClass('checked');
        $('#meetingStatus').prop('checked', false);

        $('#meetingViewFunction').parent().removeClass('checked');
        $('#meetingViewFunction').prop('checked', false);

        $('#meetingAddFunction').parent().removeClass('checked');
        $('#meetingAddFunction').prop('checked', false);

        $('#meetingEditFunction').parent().removeClass('checked');
        $('#meetingEditFunction').prop('checked', false);

      

    }

}


function clearFilter() {
    uncheckAllCheckboxes('typeFilter');
    uncheckAllCheckboxes('conditionFilter');
    uncheckAllCheckboxes('statusFilter');
    $('.selectAllCheckbox').prop('checked', false);
    $('#searchCriteria').val('null');
    $('#searchInput').val('');
    $('#searchInput').prop('disabled', true);
    $('#meetingStartDate').val('');
    $('#meetingEndDate').val('');
    $('#searchType').val('null');
    $('#endDate').val('');
    $('#startDate').val('');
    $('#endDate').prop('disabled', true);
    $('#startDate').prop('disabled', true);

}

function clearModel() {
    $('.rejectBtn').hide();


    $('#confirmChange').show();
    $('#confirmChange').removeClass("btn-info");
    $('#confirmChange').removeClass("btn-warning");
    $('#confirmChange').removeClass("btn-danger");
}
$('#showPassword').click(function () {
    var passwordField = $('#password, #password_register, #password_login');
    console.log("123");
    var passwordError = $('#password_error');

    if (passwordField.attr('type') === 'password') {
        passwordField.attr('type', 'text');
        passwordError.text(''); // Clear any error message
        $('#showPassword-icon').addClass("fa-eye");
        $('#showPassword-icon').removeClass("fa-eye-slash");
    } else {
        passwordField.attr('type', 'password');
        $('#showPassword-icon').addClass("fa-eye-slash");
        $('#showPassword-icon').removeClass("fa-eye");
    }
});
function checkAllDefineTable(checked) {
    $('.user-define').each(function () {
        // Check if the checkbox is not already checked
        if (checked.checked==true) {
            // Set the checkbox to checked
            $(this).prop('checked', true);

            // You can perform additional actions here if needed
        } else {
            $(this).prop('checked', false);
        }
    });
}
//customize datatable column
function defineTable(module,action) {
    var checkboxData = [];

    // Collect checkbox values
    $('.user-define').each(function () {
        var name = $(this).attr('name');
        var value = $(this).prop('checked') ? "A" : "V"; // Check if the checkbox is checked

        // Add each checkbox value as an object to the array
        checkboxData.push({ name: name, value: value });
    });

    // Send the collected data to the action using AJAX
    $.ajax({
        url: '/Home/' + action,
        type: 'POST',
        data: JSON.stringify({ checkboxData: checkboxData, module: module }),
        contentType: 'application/json',
        success: function (result) {
            location.reload();
        },

    });
}

//scroll to filter
function goFilter() {
   
    $('#filter-model').modal('show');
}
function goCertificate() {
    
    $('#filter-model').modal('show');
}
function getRandomColor(count, totalCount) {
    const hue = (count / totalCount) * 360;
    const rgbColor = hslToRgb(hue, 90, 40); // Lower lightness for darker colors
    return rgbToHex(rgbColor[0], rgbColor[1], rgbColor[2]);
}

function hslToRgb(h, s, l) {
    h /= 360;
    s /= 100;
    l /= 100;
    let r, g, b;

    if (s === 0) {
        r = g = b = l;
    } else {
        const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        const p = 2 * l - q;

        const hueToRgb = (p, q, t) => {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6) return p + (q - p) * 6 * t;
            if (t < 1 / 2) return q;
            if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
            return p;
        };

        r = hueToRgb(p, q, h + 1 / 3);
        g = hueToRgb(p, q, h);
        b = hueToRgb(p, q, h - 1 / 3);
    }

    return [Math.round(r * 255), Math.round(g * 255), Math.round(b * 255)];
}

function rgbToHex(r, g, b) {
    return `#${(1 << 24 | r << 16 | g << 8 | b).toString(16).slice(1)}`;
}
function goDefineTable() {
    /*$('.ibox-content').show();
    $('html, body').animate({
        scrollTop: $('#filter').offset().top
    }, 1000);*/
    $('#define-table').modal('show');
}
$('td[data-href]').click(function () {
    window.location.href = $(this).data('href');
});

function goBack() {
    window.history.back();
}

function exportCertificate() {
    $('.certificate').addClass('noprint');
    window.print();
    $('.certificate').removeClass('noprint');
}

//----------------User Friendly Features Part End----------------------//


//--------------------------Timezone Part Start-------------------------------//
function changeTimezone(startTime, endTime) {


    if ($('#timezone').val() === "V") {
        
        $('#startTime').text(convertVietnamTimeZone(moment(startTime).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A"));
        $('#endTime').text(convertVietnamTimeZone(moment(endTime).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A"));
    } else if ($('#timezone').val() === "M") {
        $('#startTime').text(convertMalaysiaTimeZone(moment(startTime).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A"));
        $('#endTime').text(convertMalaysiaTimeZone(moment(endTime).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A"));
    } else if ($('#timezone').val() === "J") {
        $('#startTime').text(convertJordanTimeZone(moment(startTime).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A"));
        $('#endTime').text(convertJordanTimeZone(moment(endTime).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A"));
    }
}
function convertTimeZoneCalendar(datetime, timezone) {
    // Assuming you have included the Moment.js library in your project

    const serverTime = moment(datetime).clone().utc();

    if (timezone == "M") {
        const malaysiaTimeMoment = serverTime.clone().utcOffset('+08:00').format('ddd MMM DD YYYY HH:mm:ss'); // Fixed: Provide the offset as a string

        return malaysiaTimeMoment;
    } else if (timezone == "V") {
        const vietnamTimeMoment = serverTime.clone().utcOffset('+07:00').format('ddd MMM DD YYYY HH:mm:ss'); // Fixed: Provide the offset as a string

        return vietnamTimeMoment;
    } else if (timezone == "J") {
        const jordanTimeMoment = serverTime.clone().utcOffset('+03:00').format('ddd MMM DD YYYY HH:mm:ss'); // Fixed: Provide the offset as a string

        return jordanTimeMoment;
    }

}
function convertTableTimeZone(timezone) {
    if (timezone== "V") { //Show vietnam timezone
        $('span.createDate').each(function (i) {
            var createDate = convertVietnamTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(createDate);
        });

        $('span.editDate').each(function (i) {
            if ($(this).text() != "No Data")
                var editDate = convertVietnamTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(editDate);
        });

        $('span.startDate').each(function (i) {

            var startDate = convertVietnamTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(startDate);
        });

        $('span.endDate').each(function (i) {

            var endDate = convertVietnamTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(endDate);
        });
    } else if (timezone == "J") {
       
        $('span.createDate').each(function (i) {
            
            var createDate = convertJordanTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(createDate);
        });

        $('span.editDate').each(function (i) {
            if ($(this).text() != "No Data")
                var editDate = convertJordanTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(editDate);
        });

        $('span.startDate').each(function (i) {

            var startDate = convertJordanTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(startDate);
        });

        $('span.endDate').each(function (i) {

            var endDate = convertJordanTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(endDate);
        });
    } else if (timezone == "M") {
        $('span.createDate').each(function (i) {
            var createDate = convertMalaysiaTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(createDate);
        });

        $('span.editDate').each(function (i) {
            if ($(this).text() != "No Data")
                var editDate = convertMalaysiaTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(editDate);
        });

        $('span.startDate').each(function (i) {

            var startDate = convertMalaysiaTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(startDate);
        });

        $('span.endDate').each(function (i) {

            var endDate = convertMalaysiaTimeZone($(this).text()).format("DD/MM/YYYY h:mm:ss A");
            $(this).text(endDate);
        });
    }
}
function checkTimeZone(timezone, startDate, endDate) {
    if (timezone == "V") {
        $('#startTimeMeeting').text(convertVietnamTimeZone(startDate).format("DD/MM/YYYY h:mm:ss A"));
        $('#endTimeMeeting').text(convertVietnamTimeZone(endDate).format("DD/MM/YYYY h:mm:ss A"));
    } else if (timezone == "J") {
        $('#startTimeMeeting').text(convertJordanTimeZone(startDate).format("DD/MM/YYYY h:mm:ss A"));
        $('#endTimeMeeting').text(convertJordanTimeZone(endDate).format("DD/MM/YYYY h:mm:ss A"));
    } else if (timezone == "M") {
        $('#startTimeMeeting').text(convertMalaysiaTimeZone(startDate).format("DD/MM/YYYY h:mm:ss A"));
        $('#endTimeMeeting').text(convertMalaysiaTimeZone(endDate).format("DD/MM/YYYY h:mm:ss A"));
    }


}
function convertTimeZoneGeneral(timezone) {
    if (timezone == "V") { //Show vietnam timezone
        var createDate = convertVietnamTimeZone($('#createDate').text()).format("DD/MM/YYYY h:mm:ss A");
        $('#createDate').text(createDate);
        if ($('#editDate').text() != "No Data") {
            var editDate = convertVietnamTimeZone($('#editDate').text()).format("DD/MM/YYYY h:mm:ss A");
            $('#editDate').text(editDate);
        }
        var pStartDate = convertVietnamTimeZone(moment($('#pStartDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A");
        $('#pStartDateShow').text(pStartDate);
        var pEndDate = convertVietnamTimeZone(moment($('#pEndDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A");
        $('#pEndDateShow').text(pEndDate);
        if ($('#titleAction').text() == "View Activity") {
            //convert from server time to vietnam timezone
            var startDate = convertVietnamTimeZone($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A");
            var endDate = convertVietnamTimeZone($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A");
            $('#startDateShow').text(startDate);
            $('#endDateShow').text(endDate);

        } else if ($('#titleAction').text() == "Edit Activity") {
            //convert from server time to vietnam timezone with correct format

            var startDate = convertVietnamTimeZone(moment($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("YYYY-MM-DDTHH:mm");
            var endDate = convertVietnamTimeZone(moment($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("YYYY-MM-DDTHH:mm");
            $('#startDate').val(startDate);
            $('#endDate').val(endDate);
        } else {
            var startDate = convertVietnamTimeZone($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A");
            var endDate = convertVietnamTimeZone($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A");
            $('#startDateShow').text(startDate);
            $('#endDateShow').text(endDate);
        }



    } else if (timezone == "J") { //show jordan timezone
        var createDate = convertJordanTimeZone($('#createDate').text()).format("DD/MM/YYYY h:mm:ss A");
        $('#createDate').text(createDate);
        if ($('#editDate').text() != "No Data") {
            var editDate = convertJordanTimeZone($('#editDate').text()).format("DD/MM/YYYY h:mm:ss A");
            $('#editDate').text(editDate);
        }
        var pStartDate = convertJordanTimeZone(moment($('#pStartDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A");
        $('#pStartDateShow').text(pStartDate);
        var pEndDate = convertJordanTimeZone(moment($('#pEndDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A");
        $('#pEndDateShow').text(pEndDate);
        var endDate = convertJordanTimeZone($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A");
        if ($('#titleAction').text() == "View Activity") {
            //convert from server time to vietnam timezone
            var startDate = convertJordanTimeZone($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A");
            var endDate = convertJordanTimeZone($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A");
            $('#startDateShow').text(startDate);
            $('#endDateShow').text(endDate);

        } else if ($('#titleAction').text() == "Edit Activity") {
            //convert from server time to vietnam timezone with correct format
            var startDate = convertJordanTimeZone(moment($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("YYYY-MM-DDTHH:mm");
            var endDate = convertJordanTimeZone(moment($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("YYYY-MM-DDTHH:mm");
            $('#startDate').val(startDate);
            $('#endDate').val(endDate);
        } else {
            var startDate = convertJordanTimeZone($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A");
            var endDate = convertJordanTimeZone($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A");
            $('#startDateShow').text(startDate);
            $('#endDateShow').text(endDate);
        }
    } else if (timezone == "M") { // show malaysia timezone
        //console.log($('#createDate').text());
        
        var createDate = convertMalaysiaTimeZone($('#createDate').text()).format("DD/MM/YYYY h:mm:ss A");
        $('#createDate').text(createDate);

        var pStartDate = convertMalaysiaTimeZone(moment($('#pStartDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A");
        $('#pStartDateShow').text(pStartDate);
        var pEndDate = convertMalaysiaTimeZone(moment($('#pEndDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A");
        $('#pEndDateShow').text(pEndDate);
        if ($('#editDate').text() != "No Data") {
            var editDate = convertMalaysiaTimeZone($('#editDate').text()).format("DD/MM/YYYY h:mm:ss A");
            $('#editDate').text(editDate);
        }

        var startDate = convertMalaysiaTimeZone($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A");
        var endDate = convertMalaysiaTimeZone($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A");
        $('#startDateShow').text(startDate);
        $('#endDateShow').text(endDate);
        if ($('#titleAction').text() == "View Activity") {
            //convert from server time to vietnam timezone
            var startDate = convertMalaysiaTimeZone(moment($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A");
            var endDate = convertMalaysiaTimeZone(moment($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("DD/MM/YYYY h:mm:ss A");
            $('#startDateShow').text(startDate);
            $('#endDateShow').text(endDate);

        } else if ($('#titleAction').text() == "Edit Activity") {
            //convert from server time to vietnam timezone with correct format
            var startDate = convertMalaysiaTimeZone(moment($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("YYYY-MM-DDTHH:mm");
            var endDate = convertMalaysiaTimeZone(moment($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A")).format("YYYY-MM-DDTHH:mm");
            $('#startDate').val(startDate);
            $('#endDate').val(endDate);
        } else {
            var startDate = convertMalaysiaTimeZone($('#startDate').val()).format("DD/MM/YYYY h:mm:ss A");
            var endDate = convertMalaysiaTimeZone($('#endDate').val()).format("DD/MM/YYYY h:mm:ss A");
            $('#startDateShow').text(startDate);
            $('#endDateShow').text(endDate);
        }
    }
}
function convertMalaysiaTimeZone(datetime) {
    // Assuming the server time is in the same timezone as the original time
    moment.tz.setDefault('Asia/Kuala_Lumpur');

    // Parse the input datetime string
    const serverTime = moment(datetime, "DD/MM/YYYY h:mm:ss A");

    // Convert to UTC
    const utcTime = serverTime.clone().utc();

    // Convert to Malaysia timezone
    const malaysiaTime = utcTime.clone().utcOffset('+08:00');
    return malaysiaTime;
}

function convertVietnamTimeZone(datetime) {
   
    moment.tz.setDefault('Asia/Kuala_Lumpur');

    // Parse the input datetime string
    const serverTime = moment(datetime, "DD/MM/YYYY h:mm:ss A");

    
    // Convert to UTC
    const utcTime = serverTime.clone().utc();

    // Convert to Vietnam timezone
    const vietnamTime = utcTime.clone().utcOffset('+07:00');
   
    return vietnamTime;
}

function convertJordanTimeZone(datetime) {
    // Assuming the server time is in the same timezone as the original time (Malaysia, UTC+8)
    moment.tz.setDefault('Asia/Kuala_Lumpur');

    // Parse the input datetime string
    const serverTime = moment(datetime, "DD/MM/YYYY h:mm:ss A");

    // Convert to UTC
    const utcTime = serverTime.clone().utc();

    // Convert to Jordan timezone (UTC+3)
    const jordanTime = utcTime.clone().utcOffset('+03:00');
    return jordanTime;
}

function getMalaysiaTimeZone(datetime) {
    const getDatetime = new Date(datetime);
    const malaysiaTimeOptions = {
        timeZone: 'Asia/Kuala_Lumpur',
        hour12: true,
        hour: 'numeric',
        minute: 'numeric',
        second: 'numeric',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    };

    const malaysiaDateTimeFormat = new Intl.DateTimeFormat('en-US', malaysiaTimeOptions);
    const malaysiaTimeZone = malaysiaDateTimeFormat.format(getDatetime);
    return malaysiaTimeZone;
}

function getVietnamTimeZone(datetime) {


    const vietnamTimeOptions = {
        timeZone: 'Asia/Ho_Chi_Minh',
        hour12: true,
        hour: 'numeric',
        minute: 'numeric',
        second: 'numeric',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    };

    const vietnamDateTimeFormat = new Intl.DateTimeFormat('en-US', vietnamTimeOptions);

    const malaysiaTime = new Date(getMalaysiaTimeZone(datetime));
    const vietnamTimeZone = vietnamDateTimeFormat.format(malaysiaTime);

    return vietnamTimeZone;
}

function getJordanTimeZone(datetime) {


    const jordanTimeOptions = {
        timeZone: 'Asia/Amman',
        hour12: true,
        hour: 'numeric',
        minute: 'numeric',
        second: 'numeric',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    };

    const jordanDateTimeFormat = new Intl.DateTimeFormat('en-US', jordanTimeOptions);


    const malaysiaTime = new Date(getMalaysiaTimeZone(datetime));
    const jordanTimeZone = jordanDateTimeFormat.format(malaysiaTime);

    return jordanTimeZone;
}
//--------------------------Timezone Part End-------------------------------//



//----------------DataTable Part Start-------------------//


//Setting for datatable
function initializeDataTable(model,type,title) {
    const currentTime = new Date();
    
    // Extract the various components of the time (year, month, day, hour, minute, second)
    const year = currentTime.getFullYear();
    const month = (currentTime.getMonth() + 1).toString().padStart(2, '0'); // Month is zero-based, so we add 1 and pad with '0'
    const day = currentTime.getDate().toString().padStart(2, '0');
    const hour = currentTime.getHours().toString().padStart(2, '0');
    const minute = currentTime.getMinutes().toString().padStart(2, '0');
    const second = currentTime.getSeconds().toString().padStart(2, '0');

    // Create the new title
    const filename = `${title} ${year}-${month}-${day} ${hour}:${minute}:${second}`;
    if (type == null||type==='') {
        type = '.statusAsc';
    }
    console.log('table' + type);
    $(type).DataTable({
        dom: '<"html5buttons"B>lTfgitp',
        buttons: [
            {
                extend: 'print',
                text: 'Export as PDF',
                title: '',
                customize: function (win) {
                    // Get the total number of rows in the DataTable
                    var rowCount = $(type).DataTable().rows().count();

                    // Wrap the table in a div
                    var tableWrapper = $('<div class="pl-3 pr-3"><h2>' +
                        '<img src="/Content/logo.png" alt="Your Logo" "  class="pb-3 " style="width:100px;">' +
                        '<span class="pl-3">' + title + ' for AttendNow System</span>' +
                        '</h2> <hr>' +

                        '<h3>Total ' + model + ': ' + rowCount + '</h3>').append($(win.document.body).find('table').clone());

                    // Remove the original table
                    $(win.document.body).find('table').remove();

                    // Append the wrapped table
                    $(win.document.body).append(tableWrapper);

                    // Other customizations...
                    $(win.document.body).addClass('white-bg');
                    $(win.document.body).css('font-size', '10px');
                    $(win.document.body).find('table').removeClass('dataTable');
                    $(win.document.body).find('table').removeAttr('role');

                    // Transform barcode content to HTML entities for printing
                    $(win.document.body).find('div table tbody tr td ').each(function () {
                        var content = $(this).html();
                        content = content.replace(/<bn>/gi, '&lt;BN&gt;')
                            .replace(/<ct>/gi, '&lt;CT&gt;')
                            .replace(/<m>/gi, '&lt;M&gt;')
                            .replace(/<l>/gi, '&lt;L&gt;')
                            .replace(/<p>/gi, '&lt;P&gt;')
                            .replace(/<s>/gi, '&lt;S&gt;')
                            .replace(/<r>/gi, '&lt;R&gt;');
                        $(this).html(content);
                    });

                    $(win.document.body).find('div table')
                        .addClass('compact')
                        .css('font-size', 'inherit')
                        .css('border-collapse', 'collapse');
                },
                exportOptions: {
                    columns: (type === '.statusAsc') ? ':visible:not(:last-child)' : ':visible'
                }
                
            }
        ]
    });



    
    }
    
    


// refresh datatable
function reinitializeDataTable(data,type, model,title) {


    // Append the search results to the table

    if (data) {
        $(type+' tbody').empty();
    $.each(data, function (index, item) {
        var newRow = $('<tr>');
        var c = "";
        // Create a table cell for each property and escape HTML entities
        var lastcell = $('<td class="centered-cell nowrap-cell">');
        var lastvalue = "";
        $.each(item, function (key, value) {

            if (key == "link") {
                c = value;

            } else {
                var cell = $('<td ' + c + '>');
                if (key == "editBtn" || key == "statusBtn" || key == "deleteBtn") {
                    lastvalue = lastvalue + value;
                } else if (value != "NS") {
                    if (key == "barcode") {
                        cell.text(value)
                    } else {
                        // Use $.text() to escape HTML entities
                        cell.html(value);
                        cell.html(value);
                    }


                    // Append the cell to the row
                    newRow.append(cell);
                }

            }

        });
        if (lastvalue != "") {
            lastcell.html(lastvalue);
            newRow.append(lastcell);
        }

        // Append the row to the table body

        
        $(type+' tbody').append(newRow);
        
        
        
    });
    }
    initializeDataTable(model, type,title);
    if (type == '.statusAsc' ) {
        statusAsc();
    } else {
        totalAsc(type);
    }
    

}

//set the status as order by in datatable
function statusAsc() {
    // Find the column index with data-column-name="status"
    var columnIndex = -1; // Initialize to -1, indicating not found
    $('.statusAsc thead th').each(function (index) {
        if ($(this).data('column-name') === 'status') {
            columnIndex = index;
            return false; // Exit the loop early if found
        }
    });

    // If columnIndex is found (not -1), order the DataTable by that column
    if (columnIndex !== -1) {
        $('.statusAsc').DataTable().order([[columnIndex, 'asc']]).draw();
    }


}



function totalAsc(type) {
    // Find the column index with data-column-name="status"
    var columnIndex = -1; // Initialize to -1, indicating not found
    $(type+' thead th').each(function (index) {
        if ($(this).data('column-name') === 'total') {
            columnIndex = index;
            return false; // Exit the loop early if found
        }
    });

    // If columnIndex is found (not -1), order the DataTable by that column
    if (columnIndex !== -1) {
        $(type).DataTable().order([[columnIndex, 'desc']]).draw();
    }

}


//----------------DataTable Part End-------------------//


//--------------------Function Part Start-----------------------//

//use for update item's status
function updateStatus(model, key, currentStatus,meeting_code,title,subtitle,timezone) {
    var newStatus = currentStatus === 'V' ? 'A' : 'V';
    var buttonText;
    
     buttonText = currentStatus === 'A' ? 'Inactive' : 'Reactive';
    
    
    
    $('#updateStatusConfirmation .modal-body').text('Are you sure you want to ' + buttonText.toLowerCase() + ' this ' + model.toLowerCase() + " ?");

    // Set up a click event for the "Yes" button in the modal
    $('#confirmChange').off('click').on('click', function () {
        // Close the modal

      

        // Perform Ajax request to update status for the specific user
        $.ajax({
            type: 'POST',
            url: '/' + model + '/UpdateStatus',
            data: { id: key, newStatus: newStatus },
            success: function (data) {
                if (data.success) {
                    
                    returnMessage = data.success;
                    console.log(returnMessage);
                    $('#updateStatusConfirmation').modal('hide');
                    searchAll(model, true, meeting_code,title,subtitle,timezone);
                   
                    // Department added successfully to the database
                } else {
                    if (data.logout) {
                        window.location.href = '/User/Logout';
                        errorMessage(data.logout);
                    }
                    errorMessage(data.error_message);
                   
                }
            },
            error: function () {
                errorMessage(data.error_message);
                

            }
           
        });
    });

    // Show the confirmation modal
    $('#updateStatusConfirmation').modal('show');
}

//use for update item's status
function updateMainStatus(model, key, currentStatus, meeting_code, title,reject,subtitle,timezone) {
    var newStatus = currentStatus === 'V' ? 'A' : 'V';
    var buttonText;

    buttonText = currentStatus === 'A' ? 'Inactive' : 'Reactive';


    $('#updateMainStatusConfirmation .modal-body').text('Are you sure you want to update this ' + model.toLowerCase() + " ?");
    
    if (currentStatus == 'A' || currentStatus == 'P') {
        $('#confirmChange').addClass('btn-danger');
        $('#confirmChange .ladda-label').text('Inactive');
        $('#confirmChange').attr('onclick', 'confirmUpdate("' + model + '","' + key + '", "V","' + meeting_code + '","' + title + '","' + subtitle + '","' + timezone + '")');

    } else {
        $('#confirmChange').addClass('btn-info');
        $('#confirmChange .ladda-label').text('Reactive');
        $('#confirmChange').attr('onclick', 'confirmUpdate("' + model + '","' + key + '", "A","' + meeting_code + '","' + title + '","' + subtitle + '","' + timezone + '")');
    }
    if (reject == 'Y') {
        $('.rejectBtn').show();
        $('#reject .ladda-label').text('Reject');
        $('#reject').attr('onclick', 'confirmUpdate("' + model + '","' + key + '", "P","' + meeting_code + '","' + title + '","' + subtitle + '","' + timezone + '")');
       
        if (currentStatus == "P" || currentStatus == "V") {
            newStatus = "A";  
            $('#reject .ladda-label').text('Approved');
            $('#reject').addClass('btn-info');
            
            $('#reject').attr('onclick', 'confirmUpdate("' + model + '","' + key + '", "A","' + meeting_code + '","' + title + '","' + subtitle + '","' + timezone + '")');
        }
    }
    // Show the confirmation modal
    $('#updateMainStatusConfirmation').modal('show');
 

 
}

function confirmUpdate(model,key,newStatus,meeting_code,title,subtitle,timezone) {
    // Perform Ajax request to update status for the specific user
    $.ajax({
        type: 'POST',
        url: '/' + model + '/UpdateStatus',
        data: { id: key, newStatus: newStatus },
        success: function (data) {
            if (data.success) {

                returnMessage = data.success;
                console.log(returnMessage);
                $('#updateMainStatusConfirmation').modal('hide');
                searchAll(model, true, meeting_code, title,subtitle,timezone);
                clearModel();
                // Department added successfully to the database
            } else {
                if (data.logout) {
                    window.location.href = '/User/Logout';
                    errorMessage(data.logout);
                }
                errorMessage(data.error_message);

            }
        },
        error: function () {
            errorMessage(data.error_message);


        }

    });

}
//when the option dropdown for filter change
$('#searchCriteria').on('change', function () {

   

    console.log($('#searchCriteria').val());
    if ($('#searchCriteria').val() === "null") {
        $('#searchInput').val('');
        $('#searchInput').prop('disabled', true);
    } else {
       
        $('#searchInput').prop('disabled', false);
    }

});

$('#searchType').on('change', function () {

   
    if ($('#searchType').val() === "null") {
        $('#endDate').val('');
        $('#startDate').val('');
        $('#endDate').prop('disabled', true);
        $('#startDate').prop('disabled', true);
    } else {
        $('#endDate').prop('disabled', false);
        $('#startDate').prop('disabled', false);
    }

});
/*function exportToPDF() {
    // Create a new jsPDF instance with A4 dimensions
    var pdf = new jsPDF({
        orientation: 'portrait',
        unit: 'mm',
        format: 'a4',
    });

    // Set font size and font type (optional)
    pdf.setFontSize(12);
    pdf.setFont('times', 'normal');

    // Add content to the PDF
    pdf.text(20, 20, 'Hello, this is your PDF content.');

    // Save or print the PDF
    pdf.save('document.pdf');  // Save the PDF with a specified name
    // Alternatively, you can open the print dialog:
    // pdf.autoPrint();
    // window.open(pdf.output('bloburl'), '_blank');
}*/


function printReportNGraf(model, type,title) {

    var table = $(type).DataTable();

    // Store the current page
    var currentPage = table.page();

    // Destroy the existing DataTable instance
    if ($.fn.DataTable.isDataTable(type)) {
        table.destroy();
    }

    // Reinitialize DataTable with the 'paging' option set to false
    $(type).DataTable({
        paging: false,
        searching: false
    });
    $(type).removeClass('dataTable');
    $(type).removeAttr('role');
    // Print the document
    window.print();

    // Re-enable pagination
    $(type).DataTable().destroy();
    initializeDataTable(model, type,title);
    $(type).addClass('dataTable');
    $(type).addAttr('role');



}
function searchAll(model, fromUpdate, meeting_code,title,subtitle,timezone) {
    var dataDate = '';
    var searchCriteria = $('#searchCriteria').val();
    var meeting_condition;
    if ($('#expireFilter').is(':checked')) {
        meeting_condition = 'E';
    } else {
        meeting_condition = '';
    }

    var searchText = $('#searchInput').val();
    var meeting_type = $('#meeting_type').val();
    var dateType = $('#searchType').val();
    var startDateValue = $('#startDate').val() ? new Date($('#startDate').val()).toISOString() : null;
    var endDateValue = $('#endDate').val() ? new Date($('#endDate').val()).toISOString() : null;

    var meetingStartDateValue = $('#meetingStartDate').val() ? new Date($('#meetingStartDate').val()).toISOString() : null;
    var meetingEndDateValue = $('#meetingEndDate').val() ? new Date($('#meetingEndDate').val()).toISOString() : null;
    if ((meetingStartDateValue != "null" && meetingStartDateValue != undefined) || (meetingEndDateValue != "null" && meetingEndDateValue != undefined)) {
        if (!$('#meetingStartDate').val() || !$('#meetingEndDate').val()) {

            console.log(dateType);
            $('#errorMessage').text('Please ensure you have selected both dates.');
            $('#errorMessage').show();
            return;
        } else {
            $('#validationMessage').text('');
            $('#errorMessage').hide();
            dataDate = "<b>(Meeting Date: From " + $('#meetingStartDate').val() + " to " + $('#meetingEndDate').val() + ")</b>";
            console.log(dataDate);
        }
        if (meetingStartDateValue > meetingEndDateValue) {


            $('#errorMessage').text('The End Date cannot be earlier than the Start Date.');

            $('#errorMessage').show();
            return;
        } else {
            $('#validationMessage').text('');
            $('#errorMessage').hide();
            dataDate = "<b>(Meeting Date: From " + $('#meetingStartDate').val() + " to " + $('#meetingEndDate').val() + ")</b>";
            console.log(dataDate);
            // Perform your search operation here

        }
    } else {
        $('#validationMessage').text('');
        $('#errorMessage').hide();
        meetingStartDateValue = null;
        meetingEndDateValue = null;
        $('.dataDate').text('');
        console.log(dataDate);
    }
    // Check if either input field is empty
    if (dateType != "null" && dateType != undefined) {

        if (!$('#startDate').val() || !$('#endDate').val()) {

            $('#errorMessage').text('Please ensure you have selected both dates.');
            $('#errorMessage').show();
            return;
        } else {
            $('#validationMessage').text('');
            $('#errorMessage').hide();
        }
        if (startDateValue > endDateValue) {


            $('#errorMessage').text('The End Date cannot be earlier than the Start Date.');

            $('#errorMessage').show();
            return;
        } else {
            $('#validationMessage').text('');
            $('#errorMessage').hide();
            // Perform your search operation here
            if (dataDate != "") {
                dataDate += "<b> && </b>";
            }
            dataDate += "<b>  (" + $("#searchType option:selected").text() + ": From " + $('#startDate').val() + " to " + $('#endDate').val() + ")</b>";
            console.log(dataDate);
        }
    } else {
        $('#validationMessage').text('');
        $('#errorMessage').hide();
        startDateValue = null;
        endDateValue = null;
        dataDate += "";
        console.log(dataDate);
    }

    
    const currentTime = new Date();
    const month = (currentTime.getMonth() + 1).toString().padStart(2, '0');

    $('.dataDate').html(dataDate);
    if (!$('#startDate').val() && !$('#endDate').val() && !$('#meetingStartDate').val() && !$('#meetingEndDate').val()) {
        $('.dataDate').html('<b>(Meeting Date: Month ' + month + ")");
    }
    
    var requestData = {
        statusOptions: selectedOptions['statusFilter'],
        conditionOptions: selectedOptions['conditionFilter'], //for meeting filter
       // typeOptions: selectedOptions['typeFilter'],//for meeting filter
        criteria: searchCriteria,
        text: searchText,
        dateType: dateType,
        startDate: startDateValue,
        endDate: endDateValue,
        meetingStartDate: meetingStartDateValue,
        meetingEndDate: meetingEndDateValue,
        meeting_code: meeting_code,
        meeting_type: meeting_type,
        meeting_condition: meeting_condition
    };

    $.ajax({
        url: '/' + model + '/Search',
        type: 'POST',
        data: requestData,
        success: function (data) {
            
            if (data.success) {
                // Destroy the current DataTable instance
                if (data.result) {
                    if (subtitle) {
                        model = subtitle;
                    }
                    $('.statusAsc').DataTable().destroy();
                    reinitializeDataTable(data.result, '.statusAsc', model,title);
                }
               
                //Pending Participant 
                
                
                //CHART MEETING //

                //Organized Meeting Report
                if (data.meetingCodeTop3 != null && data.meetingTotalTop3 != null && data.meetingTop3BasedDate != null) {
                    $('.reportMeeting').DataTable().destroy(); //test
                    reinitializeDataTable(data.meetingReportData, '.reportMeeting', 'Organized Meetings', 'Organized Meeting Report');

                    $('#meetingGraf').removeAttr('onclick');
                    const onclickValueMeeting = `showGraf('meeting', ${data.meetingCodeTop3}, ${data.meetingTotalTop3}, ${data.meetingTop3BasedDate},'${data.grafType}','Number of participants')`;

                    $("#meetingGraf").attr("onclick", onclickValueMeeting);
                    if ($("#meetingGraf").hasClass("active")) {
                        $("#meetingGraf").click();
                    }

                }

               

                //Regional Organized Chart
                if (data.meetingLocationNameTop3 != null && data.meetingLocationTotalTop3 != null && data.meetingLocationTop3BasedDate != null) {
                    $('.reportMeetingLocation').DataTable().destroy(); //test
                    reinitializeDataTable(data.locationReportData, '.reportMeetingLocation', 'Regional Organized', 'Regional Organized Report');

                    $('#locationGraf').removeAttr('onclick');
                    const onclickValueLocation = `showGraf('location', ${data.meetingLocationNameTop3}, ${data.meetingLocationTotalTop3}, ${data.meetingLocationTop3BasedDate},'${data.grafType}','Number of Activitys Organized')`;
                    $("#locationGraf").attr("onclick", onclickValueLocation);
                    if ($("#locationGraf").hasClass("active")) {
                        $("#locationGraf").click();
                    }
                }

                //Privacy Option Chart
                if (data.typeName != null && data.typeTotal != null && data.typeBasedDate != null) {
                    $('.reportType').DataTable().destroy(); //test
                    reinitializeDataTable(data.typeReportData, '.reportType', 'Meeting Privacy Option', 'Meeting Privacy Option Report');

                    $('#typeGraf').removeAttr('onclick');
                    const onclickValueType = `showGraf('type', ${data.typeName}, ${data.typeTotal}, ${data.typeBasedDate},'${data.grafType}','Number of Activitys Organized')`;
                    $("#typeGraf").attr("onclick", onclickValueType);
                    if ($("#typeGraf").hasClass("active")) {
                        $("#typeGraf").click();
                    }
                }


                //Mode Chart
                if (data.modeName != null && data.modeTotal != null && data.modeBasedDate != null) {
                    $('.reportMode').DataTable().destroy(); //test
                    reinitializeDataTable(data.modeReportData, '.reportMode', 'Meeting Modes', 'Meeting Mode Report');

                    $('#modeGraf').removeAttr('onclick');
                    const onclickValueMode = `showGraf('mode', ${data.modeName}, ${data.modeTotal}, ${data.modeBasedDate},'${data.grafType}','Number of Activitys Organized')`;
                    $("#modeGraf").attr("onclick", onclickValueMode);
                    if ($("#modeGraf").hasClass("active")) {
                        $("#modeGraf").click();
                    }
                }

                //Meeting Type Chart
                if (data.meetingTypeName != null && data.meetingTypeTotal != null && data.meetingTypeBasedDate != null) {
                    $('.reportMeetingType').DataTable().destroy(); //test
                    reinitializeDataTable(data.meetingTypeReportData, '.reportMeetingType', 'Meeting Types', 'Meeting Type Report');

                    $('#meetingTypeGraf').removeAttr('onclick');
                    const onclickValueMeetingType = `showGraf('meetingType', ${data.meetingTypeName}, ${data.meetingTypeTotal}, ${data.meetingTypeBasedDate},'${data.grafType}','Number of Activitys Organized')`;
                    $("#meetingTypeGraf").attr("onclick", onclickValueMeetingType);
                    if ($("#meetingTypeGraf").hasClass("active")) {
                        $("#meetingTypeGraf").click();
                    }
                }

                //Certificate Option Chart
                if (data.certificateName != null && data.certificateTotal != null && data.certificateBasedDate != null) {
                    $('.reportCertificate').DataTable().destroy(); //test
                    reinitializeDataTable(data.certificateReportData, '.reportCertificate', 'Meeting Certificate Options', 'Meeting Certificate Option Report');

                    $('#certificateGraf').removeAttr('onclick');
                    const onclickValueCertificate = `showGraf('certificate', ${data.certificateName}, ${data.certificateTotal}, ${data.certificateBasedDate},'${data.grafType}','Number of Activitys Organized')`;
                    $("#certificateGraf").attr("onclick", onclickValueCertificate);
                    if ($("#certificateGraf").hasClass("active")) {
                        $("#certificateGraf").click();
                    }
                }
                // Meeting Condition Chart
                if (data.conditionName != null && data.conditionTotal != null && data.conditionBasedDate != null) {
                    $('.reportCondition').DataTable().destroy(); //test
                    reinitializeDataTable(data.conditionReportData, '.reportCondition', 'Meeting Condition', 'Meeting Condition Report');

                    $('#conditionGraf').removeAttr('onclick');
                    const onclickValueCondition = `showGraf('condition', ${data.conditionName}, ${data.conditionTotal}, ${data.conditionBasedDate},'${data.grafType}','Number of Activitys Organized')`;
                    $("#conditionGraf").attr("onclick", onclickValueCondition);
                    if ($("#conditionGraf").hasClass("active")) {
                        $("#conditionGraf").click();
                    }
                }


                //Place Report Graf
                if (data.meetingPlaceNameTop3 != null && data.meetingPlaceTotalTop3 != null && data.meetingPlaceTop3BasedDate != null) {
                    $('.reportPlace').DataTable().destroy(); //test
                    reinitializeDataTable(data.placeReportData, '.reportPlace', 'Venue Organized', 'Venue Organized Report');

                    $('#placeGraf').removeAttr('onclick');
                    const onclickValuePlace = `showGraf('place', ${data.meetingPlaceNameTop3}, ${data.meetingPlaceTotalTop3}, ${data.meetingPlaceTop3BasedDate},'${data.grafType}','Number of Activitys Organized')`;

                    $("#placeGraf").attr("onclick", onclickValuePlace);
                    if ($("#placeGraf").hasClass("placeGraf")) {
                        $("#placeGraf").click();
                    }

                }
                //CHART PARTICIPANT//


                //Regional Participation Chart
                if (data.locationNameTop3 != null && data.locationTotalTop3 != null && data.locationTop3BasedDate != null) {
                    $('.reportLocation').DataTable().destroy(); //test
                    reinitializeDataTable(data.locationReportData, '.reportLocation', 'Regional Participation', 'Regional Participation Report');

                    $('#locationGraf').removeAttr('onclick');
                    const onclickValueLocation = `showGraf('location', ${data.locationNameTop3}, ${data.locationTotalTop3}, ${data.locationTop3BasedDate},'${data.grafType}','Number of participants')`;
                    $("#locationGraf").attr("onclick", onclickValueLocation);
                    if ($("#locationGraf").hasClass("active")) {
                        $("#locationGraf").click();
                    }
                }

                //Participant Chart
                if (data.participantNameTop3 != null && data.participantTotalTop3 != null && data.participantTop3BasedDate != null) {
                    $('.reportParticipant').DataTable().destroy(); //test
                    reinitializeDataTable(data.participantReportData, '.reportParticipant', 'Participants', 'Participant Report');

                    $('#participantGraf').removeAttr('onclick');
                    const onclickValueParticipant = `showGraf('participant', ${data.participantNameTop3}, ${data.participantTotalTop3}, ${data.participantTop3BasedDate},'${data.grafType}','Number of participation')`;
                    $("#participantGraf").attr("onclick", onclickValueParticipant);
                    if ($("#participantGraf").hasClass("active")) {
                        $("#participantGraf").click();
                    }
                }
                
                convertTableTimeZone(timezone); //convert timezone based on user's timezone
                
                if (fromUpdate) {
                    successMessage("Updated Successfully");
                } else {
                    successMessage("Filter Applied Successfully");
                }


              
            } else {
                errorMessage(data.error_message);
            }
            $('html, body').animate({
                scrollTop: $('#report').offset().top
            }, 1000);


            $('#filter-model').modal('hide');
            
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function handleCheckboxChange(checkboxOption) {
    // Capture selected checkboxes
    selectedOptions[checkboxOption] = [];
    console.log(selectedOptions[checkboxOption]);
    $('input[name=' + checkboxOption+']:checked').each(function () {
        selectedOptions[checkboxOption].push($(this).val());
    });

 
}
//set the data of option dropdown
$('.statusAsc thead th').each(function () {
    var columnHeader = $(this).text();
    var columnName = $(this).attr('data-column-name'); // You can add data attributes to specify column names

    if (columnName !== undefined && columnName !== "type" && columnName
        !== "condition" && columnName !== "status" && columnName !== "editDate" && columnName !== "joinDate"
        && columnName !== "startDate" && columnName !== "endDate"
        && columnName !== "createDate" && columnName !== "meetingStartDate" && columnName !== "meetingEndDate"
        && columnName !== "meeting_type") {
        $('#searchCriteria').append($('<option>', {
            value: columnName,
            text: columnHeader
        }));
    }


});

//click check checkbox or uncheck checkbox to update the status of item
function updateStatusWithChecked(model, key, checkbox,status,timezone) {
    var statusCheckbox = $(checkbox).prop('checked');
    var newStatus = statusCheckbox ? 'A' : 'V';

    var statusTextSpan = $('#statusText');
    if (status == "P") {
        newStatus = statusCheckbox ? 'A' : 'P';

       

    } else if(status=="S") {
        if (newStatus == "A") {
            $('.js-switch-pending').attr('checked', true);
            switcheryPending.setPosition(true);

        } else {
            $('.js-switch-pending').prop('checked', false);
            switcheryPending.setPosition(false);
        }
    }
    var newText = newStatus == 'A' ? 'Active' : (newStatus == 'P' ? 'Pending' : 'Inactive');

    
    statusTextSpan.text(newText);
    // Update the class based on newStatus
    if (newStatus == 'A') {
        statusTextSpan.removeClass('label-danger label-success').addClass('label-primary');
    } else if (newStatus == 'V') {
        statusTextSpan.removeClass('label-primary label-success').addClass('label-danger');
    } else {
        statusTextSpan.removeClass('label-danger label-primary').addClass('label-success');
    }
    // Perform Ajax request to update status

    // Show the confirmation modal
    $('#updateStatusConfirmation').modal('show');

    $('#updateStatusConfirmation .modal-body').text('Are you sure you want to ' + newText + ' this ' + model.toLowerCase() + " ?");

    // Set up a click event for the "Yes" button in the modal
    $('#confirmChange').off('click').on('click', function () {
        $.ajax({
            type: 'POST',
            url: '/' + model + '/UpdateStatus',

            data: { id: key, newStatus: newStatus },
            success: function (data) {
                if (data.success) {
                    console.log(data.userEditDate);
                    successMessage(data.message);
                    $('#editDate').text(data.userEditDate);
                    $('#editBy').text(data.userEditBy);
                    $('#editBy').removeClass('text-danger');
                    
                    $('#editDate').removeClass('text-danger');
                    
                    $('#updateStatusConfirmation').modal('hide');
                    returnMessage = data.success;
                   
                    convertTimeZoneGeneral(timezone);
                } else {
                    if (data.logout) {
                        window.location.href = '/User/Logout';
                        errorMessage(data.logout);
                    }
                    errorMessage(data.error_message);

                }
            },
            error: function () {
                errorMessage(data.error_message);


            }

        });
    });

    $('#cancelBtn').off('click').on('click', function () {

        if (status == "P") {
            if (newStatus == "A") {
                $('.js-switch-pending').prop('checked', false);
                switcheryPending.setPosition(false);

            } else {
                $('.js-switch-pending').attr('checked', true);
                switcheryPending.setPosition(true);
            }
        } else {
            if (newStatus == "A") {
                $('.js-switch-status').prop('checked', false);
                switchery.setPosition(false);

            } else {
                $('.js-switch-status').attr('checked', true);
                switchery.setPosition(true);
            }
        }
    });
    
}

//Graf function

    function getGraf(type, titleData, totalData, reportBasedDate,grafType,yTitle) {
     

        var reportData = {
            labels: titleData,
            datasets: [{
                data: totalData,
                backgroundColor: ["#1ab394", "#46edc8", "#9bff00"]
            }]
        };

        var ctx4 = document.getElementById(type + "ReportChart").getContext("2d");
        window.myChart = new Chart(ctx4, {
            type: grafType, data: reportData, options: {

                responsive: true,
                aspectRatio: false,

            }
        });

        //----------------------------Top3 Based Date-------------------------------//

        var end1 = -1;
        var data1 = [];
        var grafColor1 = ["#1ab394", "#46edc8", "#9bff00"];
        // Process data to create datasets
        console.log(reportBasedDate);
        // Create the chart]
        if(reportBasedDate!=''){
            const ctx = document.getElementById(type + 'ReportMultipleChart').getContext('2d');
            window.myMultipleChart = new Chart(ctx, {
                type: 'line',
                data: {
                    datasets: reportBasedDate
                        .map((group, index) => {
    
                            const total = group.reduce((acc, item) => acc + item[2], 0);
                            data1[index] = total;
                            if (data1[index] !== data1[index - 1]) {
                                end1++;
                            }
                            if (end1 < 3) {
                                return {
                                    label: group[0][0],
                                    borderColor: grafColor1[end1],
                                    backgroundColor: grafColor1[end1],
                                    data: group.map(item => [parseDate(item[1]), item[2]]),
                                    fill: false,
                                    tension: 0.15
                                };
                            }
    
                            // If the conditions are not met or end1 is greater than or equal to 5,
                            // return null to indicate that the dataset should be filtered out.
                            return null;
                        })
                        .filter(dataset => dataset !== null), // Filter out null values
                },
                options: {
    
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                unit: 'week',
                            },
                            title: {
                                display: true,
                                text: 'Date',
                            },
                        },
                        y: {
                            beginAtZero: true,
                            title: {
                                display: true,
                                text: yTitle,
                            },
                            ticks: {
                                precision: 0, // Set precision to 0 to display only integer values
                            },
                        },
                    },
    
                },
            });
        }
        

    }



function showGraf(type, titleData, totalData, reportBasedDate, grafType,yTitle) {
    console.log(grafType);
    console.log(titleData);
    console.log(totalData);
    console.log(reportBasedDate);
    if (window.myChart) {
        window.myChart.destroy();
    }
    if (window.myMultipleChart) {
        window.myMultipleChart.destroy();
    }
    setTimeout(function () {
        getGraf(type, titleData, totalData, reportBasedDate,grafType,yTitle);
    }, 100);
}

function parseDate(dateString) {
    console.log(dateString);
    var dateParts = dateString.split('/');
    return new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);
}

//--------------------Function Part End-----------------------//

//--------------------Message Part Start-----------------------//
//show successMessage function
function successMessage(message){
    if (message) {
        $('#errorMessage').hide();
        // Display the success message
        $('#informMessage').text(message);
        $('#informMessage').show();
        
        // Delay for 3 seconds and then hide the element
        setTimeout(function () {
            $('#informMessage').hide();
        }, 7000); // 7000 milliseconds (7 seconds)
    }
}


//show error message function
function errorMessage(message) {
    if (message) {
        // Display the success message
        $('#errorMessage').text(message);
        $('#errorMessage').show();
      
        
       
    }
}


//--------------------Message Part End-----------------------//


//--------------------Validation Part Start-----------------------//


// Generic function to validate a field and display an error message
function validateField(fieldId, errorMessage) {
    var fieldValue = $("#" + fieldId).val().trim();
    var errorId = fieldId + "_error";
    var parentField=$("#" + fieldId).parent();
    if (fieldValue === "") {

        $("#" + errorId).text(errorMessage);
        parentField.addClass("has-error")
        return false;
    } else {
        $("#" + errorId).text(""); // Clear any existing error message
        parentField.removeClass("has-error")
        return true;
    }
}

// Generic function to validate a dropdown and display an error message

function validateDropdown(dropdownId, defaultValue) {
    console.log(defaultValue);
    var selectedValue = $("#" + dropdownId).val();
    var errorId = dropdownId + "_error";
    var parentField = $("#" + dropdownId).parent();
    if (!selectedValue || selectedValue === defaultValue) {
        if (defaultValue == "PlaceMessage") {
            $("#" + errorId).text("Please provide either an Venue or an Online Activity URL.");
            parentField.addClass("has-error")
        }else
        if (defaultValue != "Place") {
            $("#" + errorId).text("Please select an option.");
            parentField.addClass("has-error")
        }
        
        return false;
    } else {
        $("#" + errorId).text(""); // Clear any existing error message
        parentField.removeClass("has-error")
        return true;
    }
}
function validateEmail(fieldId, errorMessage) {
    var fieldValue = $("#" + fieldId).val().trim();
    var errorId = fieldId + "_error";
    var parentField = $("#" + fieldId).parent();
    
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailRegex.test(fieldValue)) {
       
        $("#" + errorId).text(errorMessage);
        parentField.addClass("has-error");
        return false;
    } else {
       
        $("#" + errorId).text(""); // Clear any existing error message
        parentField.removeClass("has-error");
        return true;
    }
}

//--------------------Validation Part End-----------------------//

   






