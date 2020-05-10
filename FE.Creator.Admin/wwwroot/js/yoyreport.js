
$(document).ready(function () {
    $.ajax({
        url: '/home/GetWebApiAccessToken',
        dataType: 'json',
        success: function (data) {
            if (data.token) {
                loadReport(data.token);
            }
        }
    });
    function loadReport(token) {
        //Task and Target status guague report
        $.ajax({
            headers: {
                Accept: "application/json; charset=utf-8",
                Authorization: 'Bearer ' + token
            },
            url: baseUrl + "/api/report/TargetStatusReport",
            dataType: "json",
            success: function (data) {
                if (Array.isArray(data) && data.length == 2) {
                    var option = getCarGaugesOption(AppLang.INDEX_REPORT_NAME_TARGET, AppLang.INDEX_REPORT_NAME_TASK);
                    option.title.text = AppLang.INDEX_AVG_PROGRESS_REPORT;
                    option.title.subtext = AppLang.INDEX_SUBTXT_TT;
                    option.series[0].data[0].value = data[0].toFixed(2);
                    option.series[1].data[0].value = data[1].toFixed(2);

                    createChart(document.getElementById('taskstatusreport'),
                        option,
                        'default'
                    );
                }
            }
        });

        //Key objects statistic bar report.
        $.ajax({
            headers: {
                Accept: "application/json; charset=utf-8",
                Authorization: 'Bearer ' + token
            },
            url: baseUrl + "/api/report/YOYObjectUsageReport/Article,Diary,Books",
            dataType: "json",
            success: function (data) {
                if (Array.isArray(data) && data.length > 0) {
                    var option = getColumnChartOption(getYoYMonths(12), [AppLang.INDEX_REPORT_NAME_POST, AppLang.INDEX_REPORT_NAME_DIARY, AppLang.INDEX_REPORT_NAME_BOOK]);
                    option.title.text = AppLang.INDEX_YOY_STATICS_REPORT;
                    option.title.subtext = AppLang.INDEX_SUBTXT_PBD;
                    option.series.push({
                        name: AppLang.INDEX_REPORT_NAME_POST,
                        type: 'line',
                        smooth: true,
                        data: data[0],
                        markLine: {
                            data: [
                                { type: 'average', name: AppLang.INDEX_REPORT_NAME_AVG }
                            ]
                        }
                    });
                    option.series.push({
                        name: AppLang.INDEX_REPORT_NAME_DIARY,
                        type: 'line',
                        smooth: true,
                        data: data[1],
                        markLine: {
                            data: [
                                { type: 'average', name: AppLang.INDEX_REPORT_NAME_AVG }
                            ]
                        }
                    });
                    option.series.push({
                        name: AppLang.INDEX_REPORT_NAME_BOOK,
                        type: 'line',
                        smooth: true,
                        data: data[2],
                        markLine: {
                            data: [
                                { type: 'average', name: AppLang.INDEX_REPORT_NAME_AVG }
                            ]
                        }
                    });

                    createChart(document.getElementById('objusagestatusreport'),
                        option,
                        'default'
                    );
                }
            }
        });

        //Image YoY Reports.
        $.ajax({
            headers: {
                Accept: "application/json; charset=utf-8",
                Authorization: 'Bearer ' + token
            },
            url: baseUrl + "/api/report/YOYObjectUsageReport/Photos",
            dataType: "json",
            success: function (data) {
                if (Array.isArray(data) && data.length > 0) {
                    var option = getColumnChartOption(getYoYMonths(12), [AppLang.INDEX_REPORT_NAME_IMGS]);
                    option.title.text = AppLang.INDEX_YOY_STATICS_REPORT;
                    option.title.subtext = AppLang.INDEX_SUBTXT_IMG;
                    option.series.push({
                        name: AppLang.INDEX_REPORT_NAME_IMGS,
                        type: 'line',
                        smooth: true,
                        data: data[0],
                        itemStyle: { normal: { areaStyle: { type: 'default' } } },
                        markLine: {
                            data: [
                                { type: 'average', name: AppLang.INDEX_REPORT_NAME_AVG }
                            ]
                        }
                    });
                    createChart(document.getElementById('yoyimagereport'),
                        option,
                        'default'
                    );
                }
            }
        });
    }
});