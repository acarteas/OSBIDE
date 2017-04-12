$(document).ready(documentReady);

//Called when the document has finished loading and is safe to make DOM calls
function documentReady() {
    parseDates();

    //set up loading icons on form submit
    $("form.spinner").submit(function () {
        $(".submit-loading").each(function () {
            $(this).css("visibility", "visible");

            //turn off after 7 seconds
            setTimeout(function () { $(".submit-loading").css("visibility", "hidden"); }, 7000)
        });
    });

    //set up all datepicker elements
    $(".datepicker").each(function () {
        $(this).datepicker();
    });

    //update all of our UTC offset information that gets sent to the server
    var localDate = new Date();
    var localOffset = localDate.getTimezoneOffset();
    $('input.utc-offset').each(function () {
        $(this).val(localOffset);
    });

    $("#Deliverable").closest("div.row").hide();
    if ($("#CourseId").length > 0 && $("#CourseId").val() != "-1") {
        FileManager.updateCourseDependencies();
    }

    $("#CourseId").change(function () {
        FileManager.updateCourseDependencies();
    });

    $("#file").change(function (e) {

        e.stopPropagation();
        e.preventDefault();

        FileManager.validateFileExtension();
        $(".notice").fadeOut();
    });

    $("#upload").submit(function (e) {

        if (!FileManager.validateFileExtension() ) {
            e.stopPropagation();
            e.preventDefault();
        }
    });

    Nav.highlightCurrentView();
}

function parseTimeElement(htmlElementId) {
    var element = $(htmlElementId);
    var milliseconds = element.attr('datetime');
    var formatString = element.attr('data-date-format');
    var currentDate = moment.utc(milliseconds, 'X');
    var localDate = new Date();
    var localOffset = localDate.getTimezoneOffset();
    currentDate = currentDate.subtract('minutes', localOffset);
    return currentDate.format(formatString);
}

//converts UTC times to local (browser) times
function parseDates() {
    $('time.utc-time').each(function (index) {
        $(this).html(parseTimeElement(this));

        $(this).removeClass('utc-time');
        $(this).addClass('local-time');

    });
}

if (typeof (Nav) == "undefined") {
    var Nav = {
        highlightCurrentView: function () {

            var activeId = $("section[data-tab]").first().attr("data-tab");
            $("header > ul > li > a").removeClass("active");
            $("header > ul > li[data-tab='" + activeId + "'] > a").addClass("active");

            $("div[data-wzstep='1']").show();
        }
    };
}

if (typeof (FileManager) == "undefined") {
    var FileManager = {

        updateCourseDependencies: function(){

            var url = $("#rootUrl").val() + "Analytics/GetCourseDeliverables?courseId=" + $("#CourseId").val();
            var dateFrom = $("#DateFrom").val();
            if (dateFrom.length > 0) {
                url = url + "&dateFrom=" + (new Date(dateFrom)).yyyymmdd();
            }
            var dateTo = $("#DateTo").val();
            if (dateTo.length > 0) {
                url = url + "&dateTo=" + (new Date(dateTo)).yyyymmdd();
            }

            $.getJSON(url, function (data) {

                var items = "<option>Any</option>";
                $.each(data, function (i, deliverable) {
                    items += "<option>" + deliverable + "</option>";
                });
                $("#Deliverable").html(items);
                $("#Deliverable").closest("div.row").show();
            });
        },

        hash: {
            ".csv" : 1,
            ".zip" : 1,
            ".zip.zip" : 1, //accept ".zip.zip" and ".csv.csv" because they are valid types, the user just named them with extension as well
            ".csv.csv": 1,
            ".xlsx": 1,
            ".xls": 1,
        },

        validateFileExtension: function() {
            var re = /\..+$/;
            var ext = $("#file").val().match(re);

            if (this.hash[ext] == 1) {

                $("#upload").removeAttr("disabled");
                $("#fileMsg").text("");

                if (ext == ".xlsx" || ext == ".xls") {
                    $(".context-section").fadeIn("slow");
                }
                else {
                    $(".context-section").fadeOut("fast");
                }

                return true;
            }

            $("#fileMsg").text("Invalid file extension!");
            return false;
        }
    };
}

Date.prototype.yyyymmdd = function () {
    var yyyy = this.getFullYear().toString();
    var mm = (this.getMonth() + 1).toString();
    var dd = this.getDate().toString();
    return yyyy + "-" + (mm[1] ? mm : "0" + mm[0]) + "-" + (dd[1] ? dd : "0" + dd[0]);
};
