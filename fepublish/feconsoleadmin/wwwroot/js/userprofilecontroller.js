//user profile controller
; (function () {
    "use strict";

    angular
        .module('ngObjectRepository')
          .controller("BasicInfoController", BasicInfoController);
    angular
        .module('ngObjectRepository')
          .controller("ContactController", ContactController);
    angular
        .module('ngObjectRepository')
          .controller("AddressController", AddressController);
    angular
        .module('ngObjectRepository')
          .controller("WorkExperiencesController", WorkExperiencesController);
    angular
        .module('ngObjectRepository')
          .controller("SkillController", SkillController);

    angular
      .module('ngObjectRepository')
        .controller("EducationController", EducationController);

    angular.module('ngObjectRepository').filter("formataddress", function () {
        return function (address) {
            if (address != null) {
                var retaddr = "";
                if (AppLang.lang == "English") {
                    if (address.properties.address1.value != null)
                        retaddr += address.properties.address1.value + ", ";
                    if (address.properties.address.value != null)
                        retaddr += address.properties.address.value + ", ";
                    if (address.properties.postCode.value != null)
                        retaddr += address.properties.postCode.value + " ";
                    if (address.properties.city.value != null)
                        retaddr += address.properties.city.value + ", ";
                    if (address.properties.province.value != null)
                        retaddr += address.properties.province.value + ", ";
                    if (address.properties.country.value != null)
                        retaddr += address.properties.country.value;
                } else {
                    if (address.properties.country.value != null)
                        retaddr += address.properties.country.value;
                    if (address.properties.province.value != null)
                        retaddr += address.properties.province.value;
                    if (address.properties.city.value != null)
                        retaddr += address.properties.city.value;
                    if (address.properties.address.value != null)
                        retaddr += address.properties.address.value;
                    if (address.properties.address1.value != null)
                        retaddr += address.properties.address1.value;
                    if (address.properties.postCode.value != null)
                        retaddr += " " + address.properties.postCode.value;
                }

                return retaddr;
            }
        };
    });

    BasicInfoController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "Upload","UserFactory","API_BASEURL"];
    ContactController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory"];
    AddressController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory"];
    WorkExperiencesController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory"];
    SkillController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory"];
    EducationController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory"];


    function BasicInfoController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, Upload, UserFactory, API_BASEURL) {
        var vm = this;
        vm.basicinfo = {};
        vm.objectDefinitions = [];
        vm.errorMsg = null;
        vm.isEditBasicInfo = false 
        vm.init = init;
        vm.currentUserProfileId = null;
        vm.adminUser = null;
        vm.baseUrl = API_BASEURL;

        function init(userProfileId) {
            vm.currentUserProfileId = userProfileId || $scope.currentUserProfileID;
            var initLoad = function () {
                ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
                    function (data) {
                        vm.objectDefinitions = data;
                        return vm.objectDefinitions;
                    });

                ObjectRepositoryDataService.getAdminLoginName()
                    .then(function (data) {
                        vm.adminUser = data;
                        return vm.adminUser;
                    });

                //UserProfile
                reloadUserInfo(vm.currentUserProfileId);
            };

            UserFactory.getAccessToken()
                .then(function (response) {
                    initLoad();
                    return response;
                });
        }


        vm.errorMsg = "";
        //for file upload handler.
        vm.uploadFiles = function (file, errFiles, objfield) {
            vm.f = file;
            vm.errFile = errFiles && errFiles[0];
            if (file) {
                file.showprogress = true;

                file.upload = Upload.upload({
                    url: vm.baseUrl + '/api/Files',
                    data: { file: file }
                });

                file.upload.then(function (response) {
                    file.result = response.data;
                    if (file.result.files.length > 0) {
                        objfield.fileName = file.result.files[0].fileName;
                        objfield.fileUrl = file.result.files[0].fileUrl;
                        objfield.fileCRC = file.result.files[0].fileCRC;
                        objfield.fileExtension = file.result.files[0].fileExtension;
                        objfield.created = file.result.files[0].created;
                        objfield.updated = file.result.files[0].updated;
                        objfield.freated = file.result.files[0].created;
                        objfield.fileSize = file.result.files[0].fileSize;
                        objfield.fileFullPath = file.result.files[0].fileFullPath;
                    }

                    file.showprogress = false;
                }, function (response) {
                    if (response.status > 0)
                        vm.errorMsg = response.status + ': ' + response.data;
                }, function (evt) {
                    file.progress = Math.min(100, parseInt(100.0 *
                                             evt.loaded / evt.total));
                });
            }
        }


        vm.getObjectDefintionIdByName = function(definitionName){
            for(var i=0; i<vm.objectDefinitions.length; i++){
                if(vm.objectDefinitions[i].objectDefinitionName.toUpperCase() == definitionName.toUpperCase()){
                    return vm.objectDefinitions[i].objectDefinitionID;
                }
            }

            return -1;
        }

        vm.onBasicInfoEditing = function () {
            if (vm.basicinfo.objectID == null) {
                var tmpObj = {};
                tmpObj.objectDefinitionId = vm.getObjectDefintionIdByName("UserInfo");
                tmpObj.objectName = "UserInfo";
                objectUtilService.addStringProperty(tmpObj, "firstName", null);
                objectUtilService.addStringProperty(tmpObj, "lastName", null);
                objectUtilService.addDateTimeProperty(tmpObj, "birthDate", null);
                objectUtilService.addIntegerProperty(tmpObj, "gender", null);
                objectUtilService.addStringProperty(tmpObj, "ID", null);
                objectUtilService.addFileProperty(tmpObj, "Image", null);
                objectUtilService.addStringProperty(tmpObj, "userExternalId", vm.currentUserProfileId);

                vm.basicinfo = objectUtilService.parseServiceObject(tmpObj);
            }

            vm.isEditBasicInfo = true;
        }

        vm.cancelUserInfoEdit = function () {
            reloadUserInfo(vm.currentUserProfileId);
            vm.isEditBasicInfo = false;
        }

        vm.saveUserinfo = function () {
            objectUtilService.saveServiceObject(vm.basicinfo,
                function (data) {
                    if (data == null || data == "" || data.objectID != null) {
                        Notification.success({
                            message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                            delay: 3000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: 'Warn',
                        });

                        //update the object id.
                        if (data != null && data != "") {
                            vm.basicinfo.objectID = data.objectID;
                            vm.basicinfo.onlyUpdateProperties = true;
                        }
                    }
                    else {
                        //something error happend.
                        Notification.error({
                            message: AppLang.getFormatString(AppLang.PROFILE_CHANGE_FAILED, [data.toString()]),
                            delay: 5000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: 'Error'
                        });
                    }
                    vm.isEditBasicInfo = false;
                });
        }

        
        
        function reloadUserInfo(userProfileId) {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                    "UserInfo",
                    ["firstName", "lastName", "birthDate", "gender", "ID", "image", "userExternalId"].join(","),
                    null,
                    null,
                    "userExternalId," + userProfileId
                ).then(function (data) {
                    if (Array.isArray(data) && data.length > 0) {
                        vm.basicinfo = objectUtilService.parseServiceObject(data[0]);
                        return vm.basicinfo;
                    }
                });
        }
    }

    function ContactController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory) {
        var vm = this;
        vm.contact = {};
        vm.isEditContact = false;
        vm.init = init;
        vm.objectDefinitions = null;
        vm.currentUserProfileId = null;
        vm.adminUser = null;

        function init(userProfileId) {
            vm.currentUserProfileId = userProfileId || $scope.currentUserProfileID;
            var initLoad = function () {
                ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
                    function (data) {
                        vm.objectDefinitions = data;
                        return vm.objectDefinitions;
                    });

                ObjectRepositoryDataService.getAdminLoginName()
                    .then(function (data) {
                        vm.adminUser = data;
                        return vm.adminUser;
                    });

                //Contact
                reloadContact(vm.currentUserProfileId);
            };

            UserFactory.getAccessToken()
                .then(function (response) {
                    initLoad();
                    return response;
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


        vm.onContactEditing = function () {
            if (vm.contact.objectID == null) {
                var tmpObj = {};
                tmpObj.objectDefinitionId = vm.getObjectDefintionIdByName("Contact");
                tmpObj.objectName = "Contact";

                objectUtilService.addStringProperty(tmpObj, "phone", null);
                objectUtilService.addStringProperty(tmpObj, "mobile", null);
                objectUtilService.addStringProperty(tmpObj, "eMail", null);
                objectUtilService.addStringProperty(tmpObj, "weChat", null);
                objectUtilService.addStringProperty(tmpObj, "QQ", null);
                objectUtilService.addStringProperty(tmpObj, "tweet", null);
                objectUtilService.addStringProperty(tmpObj, "linkedIn", null);
                objectUtilService.addStringProperty(tmpObj, "facebook", null);
                objectUtilService.addStringProperty(tmpObj, "freeNote", null);
                objectUtilService.addStringProperty(tmpObj, "userExternalId", vm.currentUserProfileId);

                vm.contact = objectUtilService.parseServiceObject(tmpObj);
            }

            vm.isEditContact = true;
        }

        vm.cancelContactEdit = function () {
            reloadContact(vm.currentUserProfileId);
            vm.isEditContact = false;
        }

        vm.saveContact = function () {
            objectUtilService.saveServiceObject(vm.contact,
                function (data) {
                    if (data == null || data == "" || data.objectID != null) {
                        Notification.success({
                            message: 'Change Saved!',
                            delay: 3000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: 'Warn',
                        });

                        //update the object id.
                        if (data != null && data != "") {
                            vm.contact.objectID = data.objectID;
                            vm.contact.onlyUpdateProperties = true;
                        }
                    }
                    else {
                        //something error happend.
                        Notification.error({
                            message: 'Change Faild: ' + data.toString(),
                            delay: 5000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: 'Error'
                        });
                    }
                    vm.isEditContact = false;
                }
            );
        }

        function reloadContact(userProfileId) {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                             "Contact",
                             ["phone", "mobile", "eMail", "weChat", "QQ", "tweet", "linkedIn", "facebook", "freeNote", "userExternalId"].join(),
                             null,
                             null,
                             "userExternalId," + userProfileId
                         ).then(function (data) {
                             if (Array.isArray(data) && data.length > 0) {
                                 vm.contact = objectUtilService.parseServiceObject(data[0]);

                                 return vm.contact;
                             }
                         });
        }
    }

    function AddressController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory) {
        var vm = this;
        vm.addresses = [];
        vm.isEditAddress = false;
        vm.init = init;
        vm.objectDefinitions = null;
        vm.currentUserProfileId = null;
        vm.adminUser = null;

        function init(userProfileId) {
            vm.currentUserProfileId = userProfileId || $scope.currentUserProfileID;
            var initLoad = function () {
                ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
                    function (data) {
                        vm.objectDefinitions = data;
                        return vm.objectDefinitions;
                    });

                ObjectRepositoryDataService.getAdminLoginName()
                    .then(function (data) {
                        vm.adminUser = data;
                        return vm.adminUser;
                    });

                //addresses
                reloadAddresses(vm.currentUserProfileId);
            };

            UserFactory.getAccessToken()
                .then(function (response) {
                    initLoad();
                    return response;
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

        vm.onAddressEditing = function () {
            vm.isEditAddress = true;
        }

        vm.onAddNewAddress = function () {
            var address = {};
            address.objectDefinitionId = vm.getObjectDefintionIdByName("Address");
            address.objectName = "Address";

            objectUtilService.addStringProperty(address, "country", null);
            objectUtilService.addStringProperty(address, "province", null);
            objectUtilService.addStringProperty(address, "city", null);
            objectUtilService.addStringProperty(address, "address", null);
            objectUtilService.addStringProperty(address, "address1", null);
            objectUtilService.addStringProperty(address, "postCode", null);
            objectUtilService.addStringProperty(address, "userExternalId", vm.currentUserProfileId);

            vm.addresses.push(objectUtilService.parseServiceObject(address));
        }

        vm.deleteAddress = function (addr) {
            objectUtilService.deleteObjectFromArrary(vm.addresses, addr, null);
        }

        vm.saveAddress = function () {
            if (vm.addresses.length <= 0)
                return;

            objectUtilService.saveServiceObjects(vm.addresses, 0, function (data, currIdx) {
                if (currIdx >= vm.addresses.length) {
                    vm.isEditAddress = false;

                    if (data != null && data != "" && data.objectID != null) {
                        Notification.success({
                            message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                            delay: 3000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_WARN,
                        });
                    }
                    else {
                        //something error happend.
                        Notification.error({
                            message: AppLang.COMMON_EDIT_SAVE_FAILED,
                            delay: 5000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_ERROR
                        });
                    }
                }
            });
        }

        vm.cancelAddressEdit = function () {
            reloadAddresses(vm.currentUserProfileId);
            vm.isEditAddress = false;
        }
        function reloadAddresses(userProfileId) {
            //clear all the existing elements.
            vm.addresses.splice(0, vm.addresses.length);
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Address",
                 ["country", "province", "city", "address", "address1", "postCode", "userExternalId"].join(),
                 null,
                 null,
                 "userExternalId," + userProfileId
             ).then(function (data) {
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var address = objectUtilService.parseServiceObject(data[i]);
                         vm.addresses.push(address);
                     }
                 }

                 return vm.addresses;
             });
        }
    }

    function WorkExperiencesController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory) {
        var vm = this;
        vm.workexperiences = [];
        vm.isEditWorkExperiences = false;
        vm.init = init;
        vm.objectDefinitions = null;
        vm.currentUserProfileId = null;
        vm.adminUser = null;

        function init(userProfileId) {

            vm.currentUserProfileId = userProfileId || $scope.currentUserProfileID;
            
            var initLoad = function () {
                ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
                    function (data) {
                        vm.objectDefinitions = data;
                        return vm.objectDefinitions;
                    });

                ObjectRepositoryDataService.getAdminLoginName()
                    .then(function (data) {
                        vm.adminUser = data;
                        return vm.adminUser;
                    });

                //workexperiences
                reloadWorkExperience(vm.currentUserProfileId);
            };

            UserFactory.getAccessToken()
                .then(function (response) {
                    initLoad();
                    return response;
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

        vm.onWorkExperienceEditing = function () {
            vm.isEditWorkExperiences = true;
        }
        vm.onSaveWorkExperiences = function () {
            if (vm.workexperiences.length <= 0)
                return;

            objectUtilService.saveServiceObjects(vm.workexperiences, 0, function (data, currIdx) {
                if (currIdx >= vm.workexperiences.length) {
                    vm.isEditWorkExperiences = false;

                    if (data != null && data != "" && data.objectID != null) {
                        Notification.success({
                            message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                            delay: 3000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_WARN,
                        });
                    }
                    else {
                        //something error happend.
                        Notification.error({
                            message: AppLang.getFormatString(AppLang.PROFILE_CHANGE_FAILED, [data.toString()]),
                            delay: 5000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_ERROR
                        });
                    }
                }
            });

        }
        vm.onAddWorkExperience = function () {
            var workexperience = {};
            workexperience.objectDefinitionId = vm.getObjectDefintionIdByName("WorkExperience");
            workexperience.objectName = "Work Experience";

            objectUtilService.addStringProperty(workexperience, "company", null);
            objectUtilService.addStringProperty(workexperience, "companyUrl", null);
            objectUtilService.addStringProperty(workexperience, "jobTitle", null);
            objectUtilService.addDateTimeProperty(workexperience, "startDate", null);
            objectUtilService.addDateTimeProperty(workexperience, "endDate", null);
            objectUtilService.addStringProperty(workexperience, "eventDescription", null);
            objectUtilService.addStringProperty(workexperience, "userExternalId", vm.currentUserProfileId);

            vm.workexperiences.push(objectUtilService.parseServiceObject(workexperience));
        }

        vm.deleteWorkExperience = function (exp) {
            objectUtilService.deleteObjectFromArrary(vm.workexperiences, exp, null);
        }

        vm.cancelWorkExperiencesEdit = function () {
            vm.isEditWorkExperiences = false;
            reloadWorkExperience(vm.currentUserProfileId)
        }

        function reloadWorkExperience(userProfileId) {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                  "WorkExperience",
                  ["company", "companyUrl", "jobTitle", "startDate", "endDate", "eventDescription", "userExternalId"].join(),
                  null,
                  null,
                  "userExternalId," + userProfileId
              ).then(function (data) {
                  if (Array.isArray(data) && data.length > 0) {
                      vm.workexperiences.splice(0, vm.workexperiences.length);
                      for (var i = 0; i < data.length; i++) {
                          var workexperience = objectUtilService.parseServiceObject(data[i]);
                      
                          vm.workexperiences.push(workexperience);
                      }
                  }

                  return vm.workexperiences;
              });
        }
    }

    function SkillController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory) {
        var vm = this;
        vm.skills = [];
        vm.objectDefinitions = [];
        vm.init = init;
        vm.isEditSkills = false;
        vm.currentUserProfileId = null;
        vm.adminUser = null;

        function init(userProfileId) {

            vm.currentUserProfileId = userProfileId || $scope.currentUserProfileID;

            var initLoad = function () {
                ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
                    function (data) {
                        vm.objectDefinitions = data;
                        return vm.objectDefinitions;
                    });

                ObjectRepositoryDataService.getAdminLoginName()
                    .then(function (data) {
                        vm.adminUser = data;
                        return vm.adminUser;
                    });

                //skills
                reloadSkills(vm.currentUserProfileId);
            };

            UserFactory.getAccessToken()
                .then(function (response) {
                    initLoad();
                    return response;
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

        vm.onSkillEditing = function () {
            vm.isEditSkills = true;
        }

        vm.onAddSkill = function () {
            var skill = {};
            skill.objectDefinitionId = vm.getObjectDefintionIdByName("Skill");
            skill.objectName = "Technical Skill";

            objectUtilService.addStringProperty(skill, "skillName", null);
            objectUtilService.addStringProperty(skill, "skillDescription", null);
            objectUtilService.addIntegerProperty(skill, "level", 0);
            objectUtilService.addStringProperty(skill, "userExternalId", vm.currentUserProfileId);

            vm.skills.push(objectUtilService.parseServiceObject(skill));
        }
        vm.saveSkills = function () {
            if (vm.skills.length <= 0)
                return;

            objectUtilService.saveServiceObjects(vm.skills, 0, function (data, currIdx) {
                if (currIdx >= vm.skills.length) {
                    vm.isEditSkills = false;

                    if (data != null && data != "" && data.objectID != null) {
                        Notification.success({
                            message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                            delay: 3000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_WARN,
                        });
                    }
                    else {
                        //something error happend.
                        Notification.error({
                            message: AppLang.getFormatString(AppLang.PROFILE_CHANGE_FAILED, [data.toString()]),
                            delay: 5000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_ERROR
                        });
                    }
                }
            });

        }
        vm.deleteSkill = function (skill) {
            objectUtilService.deleteObjectFromArrary(vm.skills, skill, null);
        }
        vm.cancelSkillsEdit = function () {
            reloadSkills(vm.currentUserProfileId);
            vm.isEditSkills = false;
        }

        function reloadSkills(userProfileId) {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                  "Skill",
                  ["skillName", "skillDescription", "level", "userExternalId"].join(),
                  null,
                  null,
                  "userExternalId," + userProfileId
              ).then(function (data) {
                  if (Array.isArray(data) && data.length > 0) {
                      vm.skills.splice(0, vm.skills.length);

                      for (var i = 0; i < data.length; i++) {
                          var skill = objectUtilService.parseServiceObject(data[i]);
                          vm.skills.push(skill);
                      }
                  }

                  return vm.skills;
              });
        }
    }

    function EducationController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory) {
        var vm = this;
        vm.educations = [];
        vm.isEditEducation = false;
        vm.objectDefinitions = [];
        vm.init = init;
        vm.currentUserProfileId = null;
        vm.adminUser = null;

        function init(userProfileId) {
            vm.currentUserProfileId = userProfileId || $scope.currentUserProfileID;
            var initLoad = function () {
                ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
                    function (data) {
                        vm.objectDefinitions = data;
                        return vm.objectDefinitions;
                    });

                ObjectRepositoryDataService.getAdminLoginName()
                    .then(function (data) {
                        vm.adminUser = data;
                        return vm.adminUser;
                    });

                //Education
                reloadEducations(vm.currentUserProfileId);
            };

            UserFactory.getAccessToken()
                .then(function (response) {
                    initLoad();
                    return response;
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

        vm.onEducationEditing = function () {
            vm.isEditEducation = true;
        }
        vm.deleteEducation = function (edu) {
            objectUtilService.deleteObjectFromArrary(vm.educations, edu, null);
        }
        vm.onAddEducation = function () {
            var education = {};

            education.objectDefinitionId = vm.getObjectDefintionIdByName("Education");
            education.objectName = "Education";

            objectUtilService.addStringProperty(education, "school", null);
            objectUtilService.addStringProperty(education, "grade", null);
            objectUtilService.addDateTimeProperty(education, "enrollDate", null);
            objectUtilService.addDateTimeProperty(education, "exitDate", null);
            objectUtilService.addStringProperty(education, "degree", null);
            objectUtilService.addStringProperty(education, "userExternalId", vm.currentUserProfileId);

            vm.educations.push(objectUtilService.parseServiceObject(education));
        }
        vm.saveEductions = function () {
            if (vm.educations.length <= 0)
                return;

            objectUtilService.saveServiceObjects(vm.educations, 0, function (data, currIdx) {
                if (currIdx >= vm.educations.length) {
                    vm.isEditEducation = false;

                    if (data != null && data != "" && data.objectID != null) {
                        Notification.success({
                            message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                            delay: 3000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_WARN
                        });
                    }
                    else {
                        //something error happend.
                        Notification.error({
                            message: AppLang.getFormatString(AppLang.PROFILE_CHANGE_FAILED, [data.toString()]),
                            delay: 5000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_ERROR
                        });
                    }
                }
            });
        }
        vm.cancelEducationsEdit = function () {
            reloadEducations(vm.currentUserProfileId);
            vm.isEditEducation = false;
        }

        function reloadEducations(userProfileId) {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                  "Education",
                  ["school", "grade", "enrollDate", "exitDate", "degree", "userExternalId"].join(),
                  null,
                  null,
                  "userExternalId," + userProfileId
              ).then(function (data) {
                  if (Array.isArray(data) && data.length > 0) {
                      vm.educations.splice(0, vm.educations.length);
                      for (var i = 0; i < data.length; i++) {
                          var education = objectUtilService.parseServiceObject(data[i]);
                          vm.educations.push(education);
                      }
                  }

                  return vm.educations;
              });
        }
    }

})();