$(function () {
    var webApiBaseUrl = baseUrl;
    var loadUserProfileInfo = function (token) {
        var authHeaders = {
            Accept: "application/json; charset=utf-8",
            Authorization: 'Bearer ' + token
        };

        loadApplicationTheme(authHeaders);
        loadUserProfile(authHeaders)
        //load the task list.
        loadUserRecentTasks(authHeaders);

        function getServiceObjectPropertyValue(svcObj, propName) {
            if (svcObj == null || svcObj.properties == null || svcObj.properties.length == 0)
                return null;

            for (var i = 0; i < svcObj.properties.length; i++) {
                var prop = svcObj.properties[i];
                if (prop.keyName.toUpperCase() == propName.toUpperCase()) {
                    return prop.value;
                }
            }

            return null;
        }
        function applyTheme(theme) {
            $("BODY").removeClass("skin-blue-light");
            $("BODY").addClass(theme);

        }
        function loadApplicationTheme(authHeaders) {
            $.ajax({
                headers: authHeaders,
                url: webApiBaseUrl + "/api/GeneralObject/FindSysObjectsByFilter/AppConfig/"
                    + ["systemTheme", "pullMessageFromPublisher"].join(),
                dataType: "json",
                success: function (data) {
                    if (Array.isArray(data) && data.length > 0) {
                        var theme = getServiceObjectPropertyValue(data[0], "systemTheme");
                        if (theme != null
                            && theme.value != null
                            && theme.value != "") {
                            applyTheme(theme.value);
                        }

                        var pullMessageFromPublisher = getServiceObjectPropertyValue(data[0], "pullMessageFromPublisher");

                        if (pullMessageFromPublisher.value > 0) {
                            //load the provider updates.
                            loadProviderUpdates();
                        }

                        //load the system events.
                        loadSystemEvents();
                    }
                }
            });
        }

        function getErrorLevelIconStyles(level) {
            if (level === 0) {
                return "fa-times-circle text-red";
            }
            if (level === 1) {
                return "fa-warning text-yellow";
            }
            if (level === 2) {
                return "fa-info-circle text-blue";
            }

            return "fa-info-circle text-blue";
        }
        function loadUserRecentTasks(authHeaders) {
            $.ajax({
                headers: authHeaders,
                url: webApiBaseUrl + "/api/GeneralObject/FindServiceObjectsByFilter/Task/"
                    + ["taskStatus", "taskType"].join()
                    + "?pageIndex=1&pageSize=6",
                dataType: "json",
                success: function (data) {
                    var MAX_TASK_COUNT = 5;
                    if (Array.isArray(data) && data.length > 0) {
                        if (data.length > MAX_TASK_COUNT) {
                            $("#tasks_on_menu").find(".taskcount").text(MAX_TASK_COUNT + "+");
                            $("#tasks_on_menu").find(".header").text(
                                AppLang.getFormatString(AppLang.INDEX_TASK_FMT, [MAX_TASK_COUNT + "+"]));
                        }
                        else {
                            $("#tasks_on_menu").find(".taskcount").text(data.length);
                            $("#tasks_on_menu").find(".header").text(AppLang.getFormatString(AppLang.INDEX_TASK_FMT, [data.length.toString()]));
                        }


                        for (var i = 0; i < MAX_TASK_COUNT && i < data.length; i++) {
                            var progress = getServiceObjectPropertyValue(data[i], "taskStatus").value;
                            var taskType = getServiceObjectPropertyValue(data[i], "taskType").value;

                            var progressbg = "";
                            if (taskType == 0) {
                                progressbg = "progress-bar-red";
                            }
                            else if (taskType == 2) {
                                progressbg = "progress-bar-yellow";
                            }
                            else {
                                progressbg = "progress-bar-blue"
                            }
                            var mitem = "<li>" +
                                "<a href=\"#\">" +
                                "<h3>" +
                                data[i].objectName +
                                "<small class=\"pull-right\">" + progress + "%" + "</small>" +
                                "</h3>" +
                                "<div class=\"progress xs\">" +
                                "<div class=\"progress-bar " + progressbg + "\" style=\"width: " + progress + "%\" role=\"progressbar\" aria-valuenow=\"" + progress + "\" aria-valuemin=\"0\" aria-valuemax=\"100\">" +
                                "<span class=\"sr-only\">" + progress + "% Complete</span>" +
                                "</div>" +
                                "</div>" +
                                "</a>" +
                                "</li>";
                            $("#tasks_on_menu").find(".menu").append(mitem);
                        }
                    }
                }
            });
        }
        function loadSystemEvents(authHeaders) {
            var loadEventCount = 6;
            var MAX_EVENT_COUNT = 5;
            $.ajax({
                headers: authHeaders,
                url: "/Home/LatestSystemEvent",
                dataType: "json",
                data: { count: loadEventCount },
                type: "POST",
                success: function (data) {
                    
                    if (Array.isArray(data)
                        && data.length > 0) {
                        if (data.length > MAX_EVENT_COUNT) {
                            $("#event_counter").text(data.length + "+");
                        }
                        else {
                            $("#event_counter").text(data.length);
                        }
                        
                        $("#event_updates")
                            .find(".header")
                            .text(AppLang.getFormatString(AppLang.INDEX_EVENT_COUNTER_MSG_FMT, [data.length.toString()]));


                        for (var i = 0; i < data.length && i < MAX_EVENT_COUNT; i++) {
                            var item = "<li>" +
                                "<a href=\"#\">"
                                + "<i class=\"fa " + getErrorLevelIconStyles(data[i].eventLevel) + "\"></i>" + data[i].eventTitle
                                + "</a>"
                                + "</li>"

                            $("#event_updates").find(".menu").append(item);
                        }
                    }
                }
            });
        }
        function loadProviderUpdates() {
            $.ajax({
                url: "/Home/ProviderNotification",
                dataType: "json",
                success: function (data) {
                    var MAX_NOTIFY_COUNT = 5;
                    if (Array.isArray(data)
                        && data.length > 0) {

                        if (data.length > MAX_NOTIFY_COUNT) {
                            $("#msg_count").text(MAX_NOTIFY_COUNT + "+");
                            $("#provider_updates")
                                .find(".header")
                                .text(AppLang.getFormatString(AppLang.INDEX_PROVIDER_MESSAGE_FMT, [MAX_NOTIFY_COUNT + "+"]));
                        }
                        else {
                            $("#msg_count").text(data.length);
                            $("#provider_updates")
                                .find(".header")
                                .text(AppLang.getFormatString(AppLang.INDEX_PROVIDER_MESSAGE_FMT, [data.length.toString()]));
                        }

                        for (var i = 0; i < MAX_NOTIFY_COUNT && i < data.length; i++) {
                            var item = "<li>" +
                                "<a href=\"" + data[i].actionUrl + "\">" +
                                "<div class=\"pull-left\">" +
                                "<img src=\"" + data[i].imageSrc + "\" class=\"img-circle\">" +
                                "</div>" +
                                "<h4>" +
                                data[i].notifier
                                + "<small><i class=\"fa fa-clock-o\"></i> " + data[i].eventTime + "</small>" +
                                "</h4>" +
                                "<p style=\"white-space: pre-wrap;\">" + data[i].notifyDesc + "</p>" +
                                "</a>" +
                                "</li>";

                            $("#provider_updates").find(".menu").append(item);
                        }
                    }
                }
            });
        }
        function loadUserProfile(authHeaders) {
            $.ajax({
                headers: authHeaders,
                url: webApiBaseUrl + "/api/SystemUser/GetUserProfile/UserInfo/"
                    + ["firstName", "lastName", "birthDate", "gender", "ID", "image", "userExternalId"].join(),
                dataType: "json",
                success: function (data) {
                    if (Array.isArray(data) && data.length > 0) {
                        var userinfo = data[0];

                        var birthDateProperty = getServiceObjectPropertyValue(userinfo, "birthDate");
                        if (birthDateProperty != null && birthDateProperty.value != null) {
                            $("#loginUserBirthDate").text(moment(birthDateProperty.value).format("MMM Do"));
                        }

                        var imageProperty = getServiceObjectPropertyValue(userinfo, "Image");
                        if (imageProperty != null && imageProperty.fileUrl != null) {
                            $(".img-curr-login").attr("src", "/home/fwddownload/" + encodeURIComponent(imageProperty.fileUrl));
                        }
                        //if the current login name is empty, apply the user settings.
                        //if (currentLoginUser == null
                        //        || currentLoginUser === "") {
                        //    var userFirstNameProperty = getServiceObjectPropertyValue(userinfo, "firstName");
                        //    var userLastNameProperty = getServiceObjectPropertyValue(userinfo, "lastName");

                        //    var userLoginName = "";
                        //    if (userFirstNameProperty != null && userFirstNameProperty.value != null) {
                        //        userLoginName += userFirstNameProperty.value;
                        //    }

                        //    if (userLastNameProperty != null && userLastNameProperty.value != null) {
                        //        userLoginName += "," + userLastNameProperty.value;
                        //    }

                        //    $(".login-user-name").text(userLoginName);
                        //}
                    }
                }
            });
        }
    }; //end the loadUserProfileInfo

    //invoke the load action.
    $.ajax({
        url: '/home/GetWebApiAccessToken',
        dataType: 'json',
        success: function (data) {
            if (data.token) {
                loadUserProfileInfo(data.token);
            }
        }
    });

});