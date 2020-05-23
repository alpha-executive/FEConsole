(function () {
    "use strict";

    angular
      .module('ngObjectRepository')
        .controller("SystemEventController", SystemEventController);

    SystemEventController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory"];

    function SystemEventController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory) {
        var vm = this;
        vm.objectDefinitions = [];
        vm.pager = {};
        vm.pageSize = 15;
        vm.appEvents = [];

        UserFactory.getAccessToken()
            .then(function (response) {
                init();
                return response;
            });

        function init() {
            ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
                function (data) {
                    vm.objectDefinitions = data;

                    return vm.objectDefinitions;
                }).then(function (data) {
                    onPageChange(1);
                });
        }


        vm.onPageClick = function(pageIndex) {
            if (vm.pager.currentPage == pageIndex)
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

                vm.reloadSystemEvents();
            });
        }

        vm.reCalculatePager = function (pageIndex) {
            var objDefinitionId = vm.getObjectDefintionIdByName("AppEvent");
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

        vm.eventDelete = function (evt) {
            if (evt.objectID != 0 && evt.objectID != null) {
                ObjectRepositoryDataService.deleteServiceObject(evt.objectID)
                .then(function (data) {
                    //recalculate the current page when a item is deleted in a full page
                    //escape the first page, it's no need to reload the first page.
                    if (vm.pager.totalPages >= vm.pager.currentPage) {
                        var navPageIndex = vm.appEvents.length - 1 <= 0
                               ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                        onPageChange(navPageIndex);
                    }
                    else {
                        var index = vm.appEvents.indexOf(book);
                        if (index >= 0) {
                            vm.appEvents.splice(index, 1);
                        }
                    }
                });
            }
        }

        vm.reloadSystemEvents = function () {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "AppEvent",
                 ["eventTitle", "eventDetails", "eventDateTime", "eventLevel", "eventOwner"].join(),
                 vm.pager.currentPage,
                 vm.pageSize,
                 null
             ).then(function (data) {
                 vm.appEvents.splice(0, vm.appEvents.length);
                 if (Array.isArray(data) && data.length > 0) {

                     for (var i = 0; i < data.length; i++) {
                         var appEvent = objectUtilService.parseServiceObject(data[i]);
                         vm.appEvents.push(appEvent);
                     }
                 }

                 return vm.appEvents;
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