;(function () {
    'use strict'
    angular
     .module('ngObjectRepository')
       .controller("AccountRecordsController", AccountRecordsController);

    AccountRecordsController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory"];

    function AccountRecordsController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory) {
        var vm = this;
        vm.accountRecords = [];
        vm.displayMode = "accountRecordsList";
        vm.currentAccountRecord = null;
        vm.cancelObject = null;
        vm.pager = {};
        vm.objectDefinitions = [];
        vm.pageSize = 18;
        vm.onPageClick = onPageClick;

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
                vm.reloadAccountRecords(pageIndex);
            });
        }

        function saveCurrentEditingRecord() {
            objectUtilService.saveServiceObject(vm.currentAccountRecord, function (data) {
                if ((vm.currentAccountRecord.objectID == null || vm.currentAccountRecord.objectID == 0)
                   && vm.accountRecords.length >= vm.pager.pageSize) {
                    onPageChange(1);
                } else {
                    var account = objectUtilService.parseServiceObject(data);
                    account.passDisplayVal = "******";
                    var index = vm.accountRecords.indexOf(vm.currentAccountRecord);
                    if (index >= 0) {
                        vm.accountRecords.splice(index, 1, account);
                    } else {
                        vm.accountRecords.unshift(account);
                    }
                }
                vm.displayMode = "accountRecordsList";
                Notification.success({
                    message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                    delay: 3000,
                    positionY: 'bottom',
                    positionX: 'right',
                    title: 'Warn',
                });
            });
        }
        function createNewAccountRecord(objectName) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("AccountRecord");
            tempObj.objectName = objectName;
            tempObj.passDisplayVal = "******";

            objectUtilService.addStringProperty(tempObj, "accountDesc", null);
            objectUtilService.addIntegerProperty(tempObj, "accountType", 0);
            objectUtilService.addStringProperty(tempObj, "accountNumber", null);
            objectUtilService.addStringProperty(tempObj, "accountPassword", null);

            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        vm.reCalculatePager = function (pageIndex) {
            var objDefinitionId = vm.getObjectDefintionIdByName("AccountRecord");
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

        vm.createOrEditAccountRecord = function (accountRecord) {
            vm.currentAccountRecord = accountRecord;
            if (vm.currentAccountRecord == null) {
                vm.currentAccountRecord = createNewAccountRecord(AppLang.ACCOUNT_FIELD_NEW_REC);
            }

            vm.cancelObject = objectUtilService.cloneJsonObject(vm.currentAccountRecord);
            //vm.cancelObject = objectUtilService.cloneServiceObject();
            vm.displayMode = "accountRecordEditing";
        }

        vm.saveAccountRecord = function () {
            if (vm.cancelObject != null
                && vm.cancelObject.properties.accountPassword.value != vm.currentAccountRecord.properties.accountPassword.value) {
                var sendData = {
                    $type: "FE.Creator.Admin.Models.GenericDataModel, FE.Creator.Admin",
                    stringData: vm.currentAccountRecord.properties.accountPassword.value
                };

                ObjectRepositoryDataService.encryptData(sendData)
                .then(function (data) {
                    vm.currentAccountRecord.properties.accountPassword.value = data.stringData;
                    saveCurrentEditingRecord();
                });
            } else {
                saveCurrentEditingRecord();
            }
        }
        vm.showPassword = function (record) {
            if (record.passDisplayVal == "******") {
                var sendData = {
                    $type: "FE.Creator.Admin.Models.GenericDataModel, FE.Creator.Admin",
                    stringData: record.properties.accountPassword.value
                };

                ObjectRepositoryDataService.decryptData(sendData)
                .then(function (data) {
                    record.passDisplayVal = data.stringData;
                });
            }
            else {
                record.passDisplayVal = "******";
            }
        }
        vm.deleteAccountRecord = function (accountRecord) {
            ObjectRepositoryDataService.deleteServiceObject(accountRecord.objectID)
            .then(function (data) {
                if (vm.pager.totalPages > vm.pager.currentPage
                   || (vm.accountRecords.length <= 1 && vm.pager.totalPages == vm.pager.currentPage)) {
                    var navPageIndex = vm.accountRecords.length - 1 <= 0
                           ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                    onPageChange(navPageIndex);
                }
                else {
                    var index = vm.accountRecords.indexOf(accountRecord);
                    if (index >= 0) {
                        vm.accountRecords.splice(index, 1);
                    }
                }
            });
        }
        vm.cancelAccountRecordEditing = function () {
            var index = vm.accountRecords.indexOf(vm.currentAccountRecord);
            if (index >= 0) {
                vm.accountRecords.splice(index, 1, vm.cancelObject);
            }

            vm.displayMode = "accountRecordsList";
        }

        vm.reloadAccountRecords = function (pageIndex) {
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
               "AccountRecord",
               ["accountDesc", "accountType", "accountNumber", "accountPassword"].join(),
               pageIndex,
               vm.pageSize,
               null
           ).then(function (data) {
               vm.accountRecords.splice(0, vm.accountRecords.length);
               if (Array.isArray(data) && data.length > 0) {
                   for (var i = 0; i < data.length; i++) {
                       var account = objectUtilService.parseServiceObject(data[i]);
                       account.passDisplayVal = "******";
                       vm.accountRecords.push(account);
                   }
               }

               return vm.accountRecords;
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