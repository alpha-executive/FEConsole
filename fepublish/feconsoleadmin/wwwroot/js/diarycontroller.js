; (function () {
    'use strict'
    angular.module("ngObjectRepository").filter('dateformatFilter', function () {
        return function (dateval, format) {
            var dateformatted = moment(dateval).format(format);

            return dateformatted;
        }
    });

    angular
       .module('ngObjectRepository')
         .controller("DiaryController", DiaryController);
    
    DiaryController.$inject = ["$scope", "$sce", "$timeout", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory", "API_BASEURL"];
    function DiaryController($scope, $sce, $timeout, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory, API_BASEURL) {
        var vm = this;
        vm.diaries = [];
        vm.currentEditingDiary = null;
        vm.objectDefinitions = [];
        vm.pager = {};
        vm.pageSize = 7;
        vm.onPageClick = onPageClick;
        vm.cancelObject = null;
        vm.displayMode = "diaryList";
        vm.weatherstatus = [];
        vm.currentWeatherStatus = null;
        vm.baseUrl = API_BASEURL;

        UserFactory.getAccessToken()
            .then(function (response) {
                init();
                return response;
            });

        function initWeatherStatus() {
            vm.weatherstatus.push({
                wsindex : 0,
                cssclass: 'wi-day-sunny',
                label: AppLang.DIARY_WEATHER_SUNNY
            });
            vm.weatherstatus.push({
                wsindex: 1,
                cssclass: 'wi-day-cloudy',
                label: AppLang.DIARY_WEATHER_CLOUDY
            });
            vm.weatherstatus.push({
                wsindex: 2,
                cssclass: 'wi-day-cloudy-gusts',
                label: AppLang.DIARY_WEATHER_CLOUDY_GUSTS
            });
            vm.weatherstatus.push({
                wsindex: 3,
                cssclass: 'wi-day-cloudy-windy',
                label: AppLang.DIARY_WEATHER_CLOUDY_WINDY
            });
            vm.weatherstatus.push({
                wsindex: 4,
                cssclass: 'wi-day-fog',
                label: AppLang.DIARY_WEATHER_FOG
            });
            vm.weatherstatus.push({
                wsindex: 5,
                cssclass: 'wi-day-rain',
                label: AppLang.DIARY_WEATHER_RAIN
            });
            vm.weatherstatus.push({
                wsindex: 6,
                cssclass: 'wi-day-windy',
                label: AppLang.DIARY_WEATHER_WINDY
            });
            vm.weatherstatus.push({
                wsindex: 7,
                cssclass: 'wi-day-thunderstorm',
                label: AppLang.DIARY_WEATHER_STORM
            });
        }
        function init() {
            ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
               function (data) {
                   vm.objectDefinitions = data;

                   return vm.objectDefinitions;
               }).then(function (data) {
                   initWeatherStatus();
                   onPageClick(1, true);
               });
        }

        function onPageClick(pageIndex, force) {
            if (vm.pager.currentPage == pageIndex && !force)
                return;

            onPageChange(pageIndex);
        }

        function onPageChange(pageIndex) {
            vm.reCalculatePager(pageIndex).then(function (data) {
                if (pageIndex < 1) {
                    pageIndex = 1;
                }
                if (pageIndex > vm.pager.totalPages) {
                    pageIndex = vm.pager.totalPages;
                }
                vm.pager.currentPage = pageIndex;
                vm.reloadDiaries(pageIndex);
            });
        }

        function createNewDiary(){
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("Diary");
            tempObj.objectName = AppLang.DIARY_PREFIX_DIARY + (new Date()).toString();

            objectUtilService.addIntegerProperty(tempObj, "weatherStatus", 0);
            objectUtilService.addStringProperty(tempObj, "diaryContent", "&nbsp");
            objectUtilService.addIntegerProperty(tempObj, "diaryMood", 0);

            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        vm.getWeatherStatusById = function (weatherid) {
            //initialize the weather status.
            for (var i = 0; i < vm.weatherstatus.length; i++) {
                var wt = vm.weatherstatus[i];
                if (wt.wsindex == weatherid) {
                    vm.currentWeatherStatus = wt;
                    return wt;
                }
            }

            return null;
        }
        vm.setWeatherStatus = function (status) {
            vm.currentWeatherStatus = status;
            vm.currentEditingDiary.properties.weatherStatus.value = status.wsindex;
        }
        vm.reCalculatePager = function (pageIndex) {
            var objDefinitionId = vm.getObjectDefintionIdByName("Diary");
            return ObjectRepositoryDataService.getServiceObjectCount(
                    objDefinitionId,
                    null
                ).then(function (data) {
                    if (!isNaN(data)) {
                        //pager settings
                        if (pageIndex == null || pageIndex < 1)
                            pageIndex = 1;

                        vm.pager = PagerService.createPager(data, pageIndex, vm.pageSize, 10);
                        vm.pager.disabledLastPage = pageIndex > vm.pager.totalPages;
                        vm.pager.disabledFirstPage = pageIndex == 1;
                    }

                    return data;
                });
        }

        vm.createOrEditDiary = function (diary) {
            var tmpdiary = diary
            if (tmpdiary == null) {
                tmpdiary = createNewDiary();
            };

            
            vm.currentWeatherStatus = vm.getWeatherStatusById(tmpdiary.properties.weatherStatus.value);
            vm.currentEditingDiary = tmpdiary;
            vm.displayMode = "diaryEditing";
            vm.cancelObject = objectUtilService.cloneJsonObject(tmpdiary);
            //diary content should be reset since it's a angular scope function instead of value now.
            vm.cancelObject.properties.diaryContent.value = tmpdiary.properties.diaryContent.value;

            $timeout(function () {
                initializeckEditor('editor1', AppLang.lang, vm.baseUrl);
            }, 100);
        }

        vm.cancelDiaryEdit = function () {
            var index = vm.diaries.indexOf(vm.currentEditingDiary);
            if (index >= 0) {
                vm.diaries.splice(index, 1, vm.cancelObject);
            }
            vm.displayMode = "diaryList";
        }

        vm.return2List = function () {
            vm.displayMode = "diaryList";
        }

        vm.saveDiary = function () {
            var htmldata = CKEDITOR.instances.editor1.getData();
            vm.currentEditingDiary.properties.diaryContent.value = htmldata;
            objectUtilService.saveServiceObject(vm.currentEditingDiary, function (data) {
                if ((vm.currentEditingDiary.objectID == null || vm.currentEditingDiary.objectID == 0)
                    && vm.diaries.length >= vm.pager.pageSize) {
                    onPageChange(1);
                } else {
                    var diary = objectUtilService.parseServiceObject(data);
                    var index = vm.diaries.indexOf(vm.currentEditingDiary);
                    if (index >= 0) {
                        vm.diaries.splice(index, 1, diary);
                    } else {
                        vm.diaries.unshift(diary);
                    }
                }

                Notification.success({
                    message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                    delay: 3000,
                    positionY: 'bottom',
                    positionX: 'right',
                    title: 'Warn',
                });
            });
        }

        vm.deleteDiary = function (diary) {
            ObjectRepositoryDataService.deleteServiceObject(diary.objectID)
            .then(function (data) {
                //when delete the last record in first page or last page, or delete a record in middle page.
                if (vm.pager.totalPages > vm.pager.currentPage
                    || (vm.diaries.length <= 1 && vm.pager.totalPages == vm.pager.currentPage)) {
                    var navPageIndex = vm.diaries.length - 1 <= 0
                           ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                    onPageChange(navPageIndex);
                }
                else {
                    var index = vm.diaries.indexOf(diary);
                    if (index >= 0) {
                        vm.diaries.splice(index, 1);
                    }
                }
                vm.displayMode = "diaryList";
            });
        }

        vm.reloadDiaries = function (pageIndex) {
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Diary",
                 ["weatherStatus", "diaryContent", "diaryMood"].join(),
                 pageIndex,
                 vm.pageSize,
                 null
             ).then(function (data) {
                 vm.diaries.splice(0, vm.diaries.length);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var diary = objectUtilService.parseServiceObject(data[i]);
                         diary.properties.diaryContent.value = $sce.trustAsHtml(diary.properties.diaryContent.value);
                         vm.diaries.push(diary);
                     }
                 }

                 return vm.diaries;
             }).then(function(data){
                 $timeout(function () {
                     highlightCode("pre code");
                 }, 1000);
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