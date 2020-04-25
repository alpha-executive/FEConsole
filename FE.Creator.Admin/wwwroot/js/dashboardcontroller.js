; (function () {
    'use strict'
    angular
     .module('ngObjectRepository')
       .controller("DashboardController", DashboardController);

    DashboardController.$inject = ["$scope", "ObjectRepositoryDataService", "objectUtilService", "UserFactory"];
    function DashboardController($scope, ObjectRepositoryDataService, objectUtilService, UserFactory) {
        var vm = this;
        vm.objectDefinitions = [];
        vm.totalPhotosCount = 0;
        vm.totalPostCount = 0;
        vm.totalBooksCount = 0;
        vm.totalContactsCount = 0;
        vm.bookPageSize = 5;
        vm.postPageSize = 5;
        vm.imagePageSize = 6;

        vm.books = [];
        vm.images = [];
        vm.posts = [];

        UserFactory.getAccessToken()
            .then(function (response) {
                init();
                return response;
            });

        function init() {
            ObjectRepositoryDataService
                .getLightWeightObjectDefinitions()
                .then(
                    function (data) {
                        vm.objectDefinitions = data;
                        return vm.objectDefinitions;
                    }).then(function (data) {
                        getKeyObjectsCounts("Article");
                        getKeyObjectsCounts("Photos");
                        getKeyObjectsCounts("Books");
                        getKeyObjectsCounts("GeneralContact");

                        vm.reloadBooks();
                        vm.reloadPosts();
                        vm.reloadImages();

                        return data;
                    });
        }

        function getKeyObjectsCounts(objectName) {
            var objDefinitionId = vm.getObjectDefintionIdByName(objectName);
            ObjectRepositoryDataService.getServiceObjectCount(objDefinitionId).then(function (data) {
                if (!isNaN(data)) {
                    switch (objectName) {
                        case "Article":
                            vm.totalPostCount = data;
                            break;
                        case "Photos":
                            vm.totalPhotosCount = data;
                            break;
                        case "Books":
                            vm.totalBooksCount = data;
                            break;
                        case "GeneralContact":
                            vm.totalContactsCount = data;
                            break;
                        default:
                            break;
                    }
                }
            });
        }
        vm.reloadPosts = function () {
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Article",
                 ["articleDesc", "isOriginal", "articleImage", "articleSharedLevel"].join(),
                 null,
                 vm.postPageSize,
                 null
             ).then(function (data) {
                 vm.posts.splice(0, vm.posts.length);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var post = objectUtilService.parseServiceObject(data[i]);
                         //diary.properties.diaryContent.value = $sce.trustAsHtml(article.properties.diaryContent.value);
                         vm.posts.push(post);
                     }
                 }

                 return vm.posts;
             });
        }

        vm.reloadBooks = function () {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Books",
                 ["bookFile", "bookDesc", "bookAuthor", "bookVersion", "bookSharedLevel","bookISBN"].join(),
                 null,
                 vm.bookPageSize,
                 null
             ).then(function (data) {
                 vm.books.splice(0, vm.books.length);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var book = objectUtilService.parseServiceObject(data[i]);
                         vm.books.push(book);
                     }
                 }

                 return vm.books;
             });
        }

        vm.reloadImages = function () {
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Photos",
                 ["imageFile"].join(),
                 1,
                 vm.imagePageSize,
                 null
             ).then(function (data) {
                 vm.images.splice(0, vm.images.length);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var image = objectUtilService.parseServiceObject(data[i]);
                         vm.images.push(image);
                     }
                 }
                 return vm.images;
             });
        }

        vm.getObjectDefintionIdByName = function (definitionName) {
            for (var i = 0; i < vm.objectDefinitions.length; i++) {
                if (vm.objectDefinitions[i].objectDefinitionName.toUpperCase() == definitionName.toUpperCase()) {
                    return vm.objectDefinitions[i].objectDefinitionID;
                }
            }

            return -1;
        }
    }
})();

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
                    var option = getColumnChartOption(getYoYMonths(), [AppLang.INDEX_REPORT_NAME_POST, AppLang.INDEX_REPORT_NAME_DIARY, AppLang.INDEX_REPORT_NAME_BOOK]);
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
                    var option = getColumnChartOption(getYoYMonths(), [AppLang.INDEX_REPORT_NAME_IMGS]);
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