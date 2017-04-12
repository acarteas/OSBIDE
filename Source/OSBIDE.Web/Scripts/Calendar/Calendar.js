
var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

$(document).ready(function () {

    $("#back").click(function () { updateCalendar(-1); });
    $("#forward").click(function () { updateCalendar(1); });

    $("input[type='radio']").click(function () {

        updateRadioDependencies();
        updateCalendar(0);
    })

    $("input[type='checkbox']").click(function () {

        if ($("input[type='checkbox']:checked").length < 6) {

            updateMeasureBackground();

            if ($("#hourlychart").is(':visible')) {

                onDayClick(yearG, monthG, dayG, false);
            }
            else {

                updateCalendar(0);
            }
        }
        else {

            $(this).prop("checked", false)
            alert("The calendar can only show 5 or less measures at a time!");
        }
    })

    $("#hourly a").click(function () {

        updateCalendar(0);
    })

    updateMeasureBackground();
    updateRadioDependencies();
    updateCalendar(0);
});

function updateCalendar(monthOffset) {

    d3.select("svg").remove();
    updateDisplayArea(false, false, true);


    // collect selected measures
    var measures = [];
    $("input:checkbox[name='SelectedMeasureTypes']:checked").each(function () {
        measures.push(parseInt(this.value));
    });

    $.getJSON(document.location.origin + document.location.pathname + "/GetMeasures", { a: $("input[name = 'AggregationFunction']").val(), c: $("input[name = 'CourseId']").val(), o: monthOffset, m: measures.toString()}, function (result) {

        var data = JSON.parse(result);

        updateDisplayArea(true, false, false);
        data.month = data.month - 1;
        $("#currentMonth").text(calendarLabel(data.year, data.month));

        // calendar
        var chart = d3.trendingCalendar().height(700).onDayClick(onDayClick);
        d3.select("#chart").selectAll("svg").data([data]).enter().append("svg")
                                                                    .attr("width", 850)
                                                                    .attr("height", 850)
                                                                .append("g")
                                                                    .call(chart);
    });
}

function updateMeasureBackground() {

    $("input[type='checkbox']").each(function (i, e){
        if (!$(e).is(":checked")) {

            $(e).next().css({ "background-color": "transparent", "color": "#333" });
        }
        else {

            $(e).next().css({ "background-color": $(e).attr("data-color"), "color": "#fff" });
        }
    })
}

function updateRadioDependencies() {

    var aggVal = $("input[type='radio']:checked").val();

    $("input[type='checkbox'][agg-func]").each(function () {
        if ($(this).attr("agg-func") == aggVal) {
            var id = $(this).attr("id");
            $(this).attr("disabled", false).next().attr("for", id);
        }
        else
            $(this).prop("checked", false).attr("disabled", true).next().css({ "background-color": "transparent", "color": "#333" }).attr("for", "");
    });
}

function onDayClick(year, month, day, reload) {

    //preserve globale
    yearG = year, monthG = month, dayG = day;

    d3.select("svg").remove();

    var data = getHourlyData(year, month, day, reload);
    if (data.measures.length > 0) {

        updateDisplayArea(false, true, false);
        $("#currentDay").text(monthNames[month] + " " + day + ", " + year);

        drawHourlyChart(data);
    }
    else {

        updateDisplayArea(false, false, false);
    }

    var measures = $("input:checked");
}

function drawHourlyChart(data) {

    var margin = { top: 20, right: 20, bottom: 30, left: 50 },
        width = 600 - margin.left - margin.right,
        height = 300 - margin.top - margin.bottom;

    var x = d3.scale.linear().domain([0, 24]).range([0, width]);
    var y = d3.scale.linear().domain([0, data.max]).range([height, 0]);

    var xAxis = d3.svg.axis().scale(x).orient("bottom");
    var yAxis = d3.svg.axis().scale(y).orient("left");

    var svg = d3.select("#hourlychart").append("svg")
                .attr("width", width + margin.left + margin.right)
                .attr("height", height + margin.top + margin.bottom)
              .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    var line = d3.svg.line()
        .x(function (d) { return x(d.hour); })
        .y(function (d) { return y(d.value); })
        .interpolate("linear");

    svg.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + height + ")")
        .call(xAxis);

    svg.append("g")
        .attr("class", "y axis")
        .call(yAxis)
        .append("text")
        .attr("transform", "rotate(-90)")
        .attr("y", 6)
        .attr("dy", ".71em")
        .style("text-anchor", "end");

    for (var m = 0; m < data.measures.length; m++) {

        svg.append("path")
            .attr("class", "line")
            .attr("d", line(data.measures[m].values))
            .style("stroke", data.measures[m].color);
    }
}

function updateDisplayArea(showCalendar, hourly, showSpinner) {

    if (showSpinner) {

        $("#no-data-message").hide();
        $("#hourly").hide();
        $("#calendar").hide();
        $("[data-type='spinner']").show();
    }
    else if (showCalendar) {

        $("#no-data-message").hide();
        $("#hourly").hide();
        $("#calendar").show();
        $("[data-type='spinner']").hide();
    }
    else if (hourly) {

        $("#no-data-message").hide();
        $("#hourly").show();
        $("#calendar").hide();
        $("[data-type='spinner']").hide();
    }
    else {

        $("#no-data-message").show();
        $("#hourly").hide();
        $("#calendar").hide();
        $("[data-type='spinner']").hide();
    }
}

function calendarLabel(year, month) {

    var monthToDisplay = new Date(year, month, 1, 0, 0, 0, 0);
    monthToDisplay.setMonth(month +1);
    var monthTo = monthToDisplay.getMonth();

    if (month < monthTo) {

        return monthNames[month]+ " - " + monthNames[monthTo]+ " " +year;
    }
    else {

        return monthNames[month]+ " " +year + " - " + monthNames[monthTo] + " " +(year +1);
    }
}

function getHourlyData(year, month, day, reload) {

    if (reload) {

        var m = [];
        $("input[type='checkbox']").each(function () {

            m.push({
                title: $(this).next().text(),
                color: getMeasureColor($(this).attr("id")),
                values: get24HourRandomData()
            });
        });

        hourlybuffer = m;
    }

    var mm = [];
    $("input:checked").each(function () {

        var selected = $(this).next().text();

        hourlybuffer.forEach(function (entry) {

            if (entry.title === selected) {
                mm.push({ title: entry.title, color: entry.color, values: entry.values });
            }
        });
    });

    return { max: 150, measures: mm };
}

function getRandomData(length) {

    var randomData = [];
    for (var idx = 0; idx < length; idx++) {
        randomData.push(Math.floor(Math.random() * 100));
    }
    return randomData;
}

function get24HourRandomData() {

    var randomData = [];
    for (var idx = 0; idx < 24; idx++) {
        randomData.push({ hour: idx, value: Math.floor(Math.random() * 100) });
    }
    return randomData;
}
