; (function () {
    'use strict'
    angular
     .module('ngObjectRepository')
       .controller("AppSettingsController", AppSettingsController);

    AppSettingsController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "objectUtilService", "UserFactory"];

    function AppSettingsController($scope, ObjectRepositoryDataService, Notification, objectUtilService, UserFactory) {
        var vm = this;
        vm.appsettings = {};
        vm.themes = [];
        vm.languages = [];

        UserFactory.getAccessToken()
            .then(function (response){
                init();
                return response;
            });

        function init() {
            reloadAppSettings();
            initThemes();
            initLanguages();
        }

        function initLanguages() {
            vm.languages.push({
                text: "English",
                langCode: "en-US"
            });
            vm.languages.push({
                text: "中文",
                langCode: "zh-CN"
            });
        }

        function initThemes() {
            //skin-blue
            vm.themes.push({
                name: "skin-blue",
                topLeft: {"background" : "#367fa9"},
                topRight: "bg-light-blue",
                bottomLeft: {"background": "#222d32"},
                bottomRight: { "background": "#f4f5f7" }
            });
            //skin_black
            vm.themes.push({
                name: "skin-black",
                topLeft: {"background" : "rgba(0,0,0,0.1)"},
                topRight: "#fefefe",
                bottomLeft: {"background" : "#222"},
                bottomRight: { "background": "#f4f5f7" }
            });
            //skin_purple
            vm.themes.push({
                name: "skin-purple",
                topLeft: {"background" : "#555299"},
                topRight: "bg-purple",
                bottomLeft: {"background" : "#222d32"},
                bottomRight: {"background" : "#f4f5f7"}
            });
            //skin_green
            vm.themes.push({
                name: "skin-green",
                topLeft: {"background" : "#008d4c"},
                topRight: "bg-green",
                bottomLeft: {"background" : "#222d32"},
                bottomRight: {"background" : "#f4f5f7"}
            });
            //skin_red
            vm.themes.push({
                name: "skin-red",
                topLeft: {"background" : "#dd4b39"},
                topRight: "bg-red",
                bottomLeft: {"background" : "#222d32"},
                bottomRight: {"background" : "#f4f5f7"}
            });
            //skin_yellow
            vm.themes.push({
                name: "skin-yellow",
                topLeft: {"background" : "#db8b0b"},
                topRight: "bg-yellow",
                bottomLeft: {"background" : "#222d32"},
                bottomRight: {"background" : "#f4f5f7"}
            });
           
            //skin_blue_light
            vm.themes.push({
                name: "skin-blue-light",
                topLeft: {"background" : "#367fa9"},
                topRight: "bg-light-blue",
                bottomLeft: {"background" : "#f9fafc"},
                bottomRight: {"background" : "#f4f5f7"}
            });
            //skin_black_light
            vm.themes.push({
                name: "skin-black-light",
                topLeft: {"background" : "rgba(0,0,0,0.1)"},
                topRight: "#fefefe",
                bottomLeft: {"background" : "#f9fafc"},
                bottomRight: {"background" : "#f4f5f7"}
            });
            //skin_purple_light
            vm.themes.push({
                name: "skin-purple-light",
                topLeft: { "background": "#555299" },
                topRight: "bg-purple",
                bottomLeft: {"background" : "#f9fafc"},
                bottomRight: {"background" : "#f4f5f7"}
            });
            //skin_green_light
            vm.themes.push({
                name: "skin-green-light",
                topLeft: {"background" : "#008d4c"},
                topRight: "bg-green",
                bottomLeft: {"background" : "#f9fafc"},
                bottomRight: {"background" : "#f4f5f7"}
            });
            //skin_red_light
            vm.themes.push({
                name: "skin-red-light",
                topLeft: {"background" : "#dd4b39"},
                topRight: "bg-red",
                bottomLeft: {"background" : "#f9fafc"},
                bottomRight: {"background" : "#f4f5f7"}
            });
            //skin_yellow_light
            vm.themes.push({
                name: "skin-yellow-light",
                topLeft: { "background": "#db8b0b" },
                topRight: "bg-yellow",
                bottomLeft: {"background" : "#f9fafc"},
                bottomRight: {"background" : "#f4f5f7"}
            });
        }

        vm.clearTheme = function () {
            if (vm.themes != null && vm.themes.length > 0) {
                for (var idx = 0; idx < vm.themes.length; idx++) {
                    var tm = vm.themes[idx].name;
                    if ($("body").hasClass(tm)) {
                        $("body").removeClass(tm);
                    }
                }
            }
        }
        vm.setTheme = function(theme){
            if (theme != vm.appsettings.properties.systemTheme.value) {
                vm.appsettings.properties.systemTheme.value = theme;
            }
            vm.clearTheme();
            if(!$("body").hasClass(theme)){
                $("body").addClass(theme);
            }
        }

        vm.saveSettings = function (setting) {
            objectUtilService.saveServiceObject(setting, function (data) {
                Notification.success({
                    message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                    delay: 3000,
                    positionY: 'bottom',
                    positionX: 'right',
                    title: 'Warn',
                });
            });
        }

       function reloadAppSettings(){
           return ObjectRepositoryDataService.getSysObjectsByFilters(
                   "AppConfig",
                   ["language", "dateTimeFormat", "systemTheme", "pullMessageFromPublisher", "pullMessagePublisherUrl"].join(),
                   1,
                   1,
                   null
               ).then(function (data) {
                   if (Array.isArray(data) && data.length == 1) {
                       vm.appsettings = objectUtilService.parseServiceObject(data[0]);
                   }

                   return vm.appsettings;
               });
        }
    }
})();