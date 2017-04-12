$(document).ready(function () {

    //listen for comment posts
    $("#post-comment-form").submit(function (event) {

        //disable button for 7 seconds to prevent duplicate clicks
        $("#post-comment-button").prop("disabled", true);
        setTimeout(function () { $("#post-comment-button").prop("disabled", false); }, 7000);
    });

    //set up animation for drop down
    $("#filter-options-button").click(function (e) {
        if ($("#filter-options").css('display') == 'none') {
            $("#filter-options").slideDown();
        }
        else {
            $("#filter-options").slideUp();
        }
    });

    TrendingNotifications.checkForUpdates();

    //feedpost keyword filter
    setKeywordSectionVisibility();
    $("#event_FeedPostEvent").click(function (e) {
        setKeywordSectionVisibility();
    });

    //submit feedpost
    $("#post-comment-form").submit(function (e) {

        var $commentEl = $("textarea[name='comment']");
        $commentEl.val($commentEl.prev().text());
        return;
    });

    //typeahead behavior
    $('.typeahead').textcomplete([
    { // html
        userHandle: null,
        match: /\B((?:@|#)\w*)$/,
        search: function (term, callback) {
            userHandle = term.substring(0, 1) == "@";
            term = term.substring(1);
            $.getJSON(document.location.origin + document.location.pathname + '/GetHashTags', { query: term, isHandle: userHandle })
              .done(function (resp) {
                  callback($.map(resp, function (mention) {
                      return mention.toLowerCase().indexOf(term.toLowerCase()) === 0 ? mention : null;
                  }));
              })
              .fail(function () {
                  callback([]); // Callback must be invoked even if something went wrong.
              });

        },
        index: 1,
        replace: function (mention) {
            if (userHandle)
                return '@' + mention + ' ';
            return '#' + mention + ' ';

        }
    }
    ], { appendTo: 'body' }).overlay([
    {
        match: /\B(?:@|#)\w+/g,
        css: {
            'background-color': '#c8cfda'
        }
    }
    ]);
});

function setKeywordSectionVisibility() {
    var $keywordSection = $("article[id='keywordSection']");
    if ($("#event_FeedPostEvent").prop("checked")) {
        $keywordSection.slideDown();
    }
    else {
        $keywordSection.slideUp();
    }
}



if (typeof (TrendingNotifications) == "undefined") {
    var TrendingNotifications = {
        checkForUpdates: function () {

            //populate trends
            $.getJSON(document.location.origin + document.location.pathname + "/GetTrendingNotifications", function (data) {
                if (data.length > 0) {
                    var hashitems = [], mentionitems = [];
                    $.each(data, function (idx, obj) {
                        if (obj.HashtagId != null) {
                            var val = obj.Hashtag;
                            hashitems.push("<li data-trend-name=='" + val + "'><a href='" + document.location.origin + document.location.pathname + "/Index?keyword=" + val + "&hash=1'>" + "#" + val + "</a></li>");
                        }
                        else if (obj.UserId != null) {
                            mentionitems.push("<li><a href='" + document.location.origin + "/Profile/Index/" +obj.UserId + "?component=UserProfile'>" +obj.FirstName + " " +obj.LastName + "</a> mentioned you in a <a href='/Feed/Details/" +obj.EventLogId + "?component=FeedDetails'>post</a></li>");
                        }
                    });

                    if (hashitems.length == 0) {
                        $(".trends-container").hide();
                    }
                    else {
                        $(".trends-container ul").children().remove().end().append(hashitems.join("")).fadeIn();
                        $(".trends-container").fadeIn();
                    }

                    if (mentionitems.length == 0) {
                        $(".notifications-container").hide();
                    }
                    else {
                        $(".notifications-container ul").children().remove().end().append(mentionitems.join(""));
                        $(".notifications-container").fadeIn();
                    }
                }

                setTimeout(TrendingNotifications.checkForUpdates, 30000);
            });
        }
    };
}
