(function () {
    'use strict'
    angular
       .module('ngObjectRepository')
         .controller("GeneralContactController", GeneralContactController);

    GeneralContactController.$inject = ["$scope", "$q", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory"];

    function GeneralContactController($scope, $q, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory) {
        var vm = this;
        vm.displayMode = "contactList";
        vm.currentGeneralContact = null;
        vm.generalcontacts = [];
        vm.objectDefinitions = [];
        vm.isEditGeneralContactProfile = false;
        vm.generalCancelobject = null;
        vm.pager = {};
        vm.pageSize = 18;
        vm.onPageClick = onPageClick;
        vm.reCalculatePager = reCalculatePager;
        vm.getObjectDefintionIdByName = getObjectDefintionIdByName;
       
        vm.slider = {
            options: {
                floor: 0,
                ceil: 100,
                showSelectionBar: true
            }
        };

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

                vm.reloadgeneralcontacts(pageIndex);
            });
        }

        function createContact(contactUID) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("GeneralContact");
            tempObj.objectName = contactUID;

            objectUtilService.addStringProperty(tempObj, "contactUID", contactUID);
            objectUtilService.addIntegerProperty(tempObj, "tapLevel", 0);
            objectUtilService.addStringProperty(tempObj, "relationship", null);

            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function reCalculatePager(pageIndex) {
            var objDefinitionId = vm.getObjectDefintionIdByName("GeneralContact");
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

        vm.onReturn2List = function () {
            onPageClick(vm.pager.currentPage, true);
            vm.displayMode = "contactList";
        }
        vm.onGeneralContactEditing = function (generalContact) {
            vm.generalCancelobject = objectUtilService.cloneJsonObject(generalContact);
            vm.isEditGeneralContactProfile = true;
        }

        vm.saveGeneralContact = function () {
            objectUtilService.saveServiceObject(vm.currentGeneralContact, function (data) {
                vm.isEditGeneralContactProfile = false;
            });
        }

        vm.cancelGeneralContactEdit = function () {
            vm.isEditGeneralContactProfile = false;
            vm.currentGeneralContact = vm.generalCancelobject;
        }

        vm.editGeneralContactProfile = function (generalcontact) {
            if (generalcontact == null) {
                ObjectRepositoryDataService.getUUID().then(function (uuid) {
                    var contact = createContact(uuid);
                    objectUtilService.saveServiceObject(contact, function (data) {
                        var svccontact = objectUtilService.parseServiceObject(data);
                        vm.currentGeneralContact = svccontact;
                        vm.generalcontacts.unshift(svccontact);
                        $scope.currentUserProfileID = uuid;
                        vm.displayMode = "contactEdit";
                    });
                });
            }
            else {
                vm.currentGeneralContact = generalcontact;
                $scope.currentUserProfileID = generalcontact.properties.contactUID.value;
                vm.displayMode = "contactEdit";
            }
        }
        vm.deleteGeneralContact = function (contact) {
            //delete user info
            deleteGeneralContactReferenceObjects("UserInfo", contact.properties.contactUID.value);
            //delete the contact info
            deleteGeneralContactReferenceObjects("Contact", contact.properties.contactUID.value);
            //delete the address info
            deleteGeneralContactReferenceObjects("Address", contact.properties.contactUID.value);
            //delete the workexperiences inforamtion
            deleteGeneralContactReferenceObjects("WorkExperience", contact.properties.contactUID.value);
            //delete the skills info
            deleteGeneralContactReferenceObjects("Skill", contact.properties.contactUID.value);
            //delete the education info
            deleteGeneralContactReferenceObjects("Education", contact.properties.contactUID.value);

            ObjectRepositoryDataService.deleteServiceObject(contact.objectID)
            .then(function (data) {
                if (vm.pager.totalPages > vm.pager.currentPage
                           || (vm.generalcontacts.length <= 1 && vm.pager.totalPages == vm.pager.currentPage)) {
                    var navPageIndex = vm.generalcontacts.length - 1 <= 0
                           ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                    onPageChange(navPageIndex, true);
                }
                else {
                    var index = vm.generalcontacts.indexOf(contact);
                    if (index >= 0) {
                        vm.generalcontacts.splice(index, 1);
                    }
                }
            });
        }
        vm.reloadgeneralcontacts = function(pageIndex) {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                "GeneralContact",
                ["contactUID", "tapLevel", "relationship"].join(),
                pageIndex,
                vm.pageSize,
                null
            ).then(function (data) {
                vm.generalcontacts.splice(0, vm.generalcontacts.length);
                if (Array.isArray(data) && data.length > 0) {
                    for (var i = 0; i < data.length; i++) {
                        var contact = objectUtilService.parseServiceObject(data[i]);
                        vm.generalcontacts.push(contact);
                    }
                }

                return vm.generalcontacts;
            })
           .then(function (generalcontacts) {
               var refIds = [];
               if (Array.isArray(generalcontacts) && generalcontacts.length > 0) {
                   for (var i = 0; i < generalcontacts.length; i++) {
                       refIds.push(generalcontacts[i].properties.contactUID.value);
                   }
               }

               return refIds;
           })
           .then(function (refIds) {
               if (refIds != null && refIds.length > 0) {
                   ObjectRepositoryDataService.getServiceObjectsWithFilters(
                            "UserInfo",
                            ["firstName", "lastName", "birthDate", "gender", "ID", "image", "userExternalId"].join(","),
                            null,
                            null,
                            "userExternalId," + refIds.join()
                       ).then(function (profiles) {
                           if (Array.isArray(profiles)) {
                               for (var i = 0; i < profiles.length; i++) {
                                   var profile = objectUtilService.parseServiceObject(profiles[i]);
                                   for (var j = 0; j < vm.generalcontacts.length; j++) {
                                       var contact = vm.generalcontacts[j];
                                       if (profile.properties.UserExternalId.value == contact.properties.contactUID.value) {
                                           contact.properties.profile = profile.properties;
                                       }
                                   }
                               }
                           }
                       });
               }

               return refIds;
           })
           .then(function (refIds) {
               if (refIds != null && refIds.length > 0) {
                   ObjectRepositoryDataService.getServiceObjectsWithFilters(
                            "Contact",
                            ["phone", "mobile", "eMail", "weChat", "QQ", "tweet", "linkedIn", "facebook", "freeNote", "userExternalId"].join(),
                            null,
                            null,
                            "userExternalId," + refIds.join()
                        ).then(function (contacts) {
                            if (Array.isArray(contacts) && contacts.length > 0) {
                                for (var i = 0; i < contacts.length; i++) {
                                    var contactInfo = objectUtilService.parseServiceObject(contacts[i]);
                                   
                                    for (var j = 0; j < vm.generalcontacts.length; j++) {
                                        var generalContact = vm.generalcontacts[j];
                                        if (contactInfo.properties.UserExternalId.value == generalContact.properties.contactUID.value) {
                                            generalContact.properties.contact = contactInfo.properties;
                                        }
                                    }
                                }
                            }
                        });
               }
               return refIds;
           });
        }

        function deleteGeneralContactReferenceObjects(objectDefName, userExternalId) {
            //delete the Address information
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                objectDefName,
                "userExternalId",
                null,
                null,
                "userExternalId," + userExternalId
            ).then(function (svcobjects) {
                if (Array.isArray(svcobjects) && svcobjects.length > 0) {
                    for (var i = 0; i < svcobjects.length; i++) {
                        ObjectRepositoryDataService.deleteServiceObject(svcobjects[i].objectID);
                    }
                }
            });
        }

        function getObjectDefintionIdByName(definitionName) {
            for (var i = 0; i < vm.objectDefinitions.length; i++) {
                if (vm.objectDefinitions[i].objectDefinitionName.toUpperCase() == definitionName.toUpperCase()) {
                    return vm.objectDefinitions[i].objectDefinitionID;
                }
            }

            return -1;
        }
    }
})();