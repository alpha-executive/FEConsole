(function(){
    "use strict";

    var PrimaryTypes = ['String', 'Integer', 'Long', 'Datetime', 'Number', 'Binary'];
    var PrimaryTypesNameLangs = [AppLang.DATA_CUST_FIELD_STRING, AppLang.DATA_CUST_FIELD_INTEGER, AppLang.DATA_CUST_FIELD_LONG, AppLang.DATA_CUST_FIELD_DATETIME, AppLang.DATA_CUST_FIELD_NUMBER];
    var PrimaryTypesKeyCodeLangs = [AppLang.DATA_CUST_FIELD_STRING_KEY, AppLang.DATA_CUST_FIELD_INTEGER_KEY, AppLang.DATA_CUST_FIELD_LONG_KEY, AppLang.DATA_CUST_FIELD_DATETIME_KEY, AppLang.DATA_CUST_FIELD_NUMBER_KEY];
    var PrimaryTypesLangs = [AppLang.DATA_CUST_STRING_TYPE, AppLang.DATA_CUST_INTEGER_TYPE, AppLang.DATA_CUST_LONG_TYPE, AppLang.DATA_CUST_DATETIME_TYPE, AppLang.DATA_CUST_NUMBER_TYPE];
    angular
         .module('ngObjectRepository')
           .controller("ObjectDefinitionController", ObjectDefinitionController)
           .controller("ObjectDefintionListController", ObjectDefintionListController)
           .controller('ObjectDefintionEditController', ObjectDefintionEditController);

    angular.module('ngObjectRepository')
            .config(function ($routeProvider, $locationProvider) {
                $routeProvider
                 .when('/objdefs/:groupId', {
                     templateUrl: '/ngView/ObjectRepository/ObjectDefintionList',
                     controller: 'ObjectDefintionListController'
                 })
                .when('/objdefs/:groupId/:objdefid/edit',{
                    templateUrl: '/ngView/ObjectRepository/ObjectDefintionEdit',
                    controller: 'ObjectDefintionEditController'
                });

                // configure html5 to get links working on jsfiddle
                $locationProvider.html5Mode({
                    enabled: true,
                    requireBase: false
                });
            });

    angular.module('ngObjectRepository').filter("fieldTypeNameFilter", function () {
        return function (field) {
            switch (field.generalObjectDefinitionFiledType) {
                case 0:
                    return PrimaryTypesLangs[field.primeDataType];
                case 1:
                    return AppLang.DATA_CUST_OBJREF_TYPE;
                case 2:
                    return AppLang.DATA_CUST_SL_TYPE;
                case 3:
                    return AppLang.DATA_CUST_FILE_TYPE;
                default:
                    return AppLang.DATA_CUST_UNKNOWN_TYPE;
            }
        };
    });

    ObjectDefinitionController.$inject = ["$scope", "$route", "$routeParams", "$location", "ObjectRepositoryDataService", "UserFactory"];
    ObjectDefintionListController.$inject = ["$scope", "$route", "$routeParams", "$location", "ObjectRepositoryDataService", "UserFactory"];
    ObjectDefintionEditController.$inject = ["$scope", "$route", "$routeParams", "$location", "ObjectRepositoryDataService", "Notification", "UserFactory"];

    function ObjectDefintionEditController($scope, $route, $routeParams, $location, ObjectRepositoryDataService, Notification, UserFactory) {
        var scopeContext = $scope;
        scopeContext.$route = $route;
        scopeContext.$location = $location;
        scopeContext.$routeParams = $routeParams;
        scopeContext.objectDefId = $routeParams.objdefid;
        scopeContext.groupId = $routeParams.groupId;
        scopeContext.CurrentObjectDefinition = {};
        scopeContext.AvailableObjectDefinitions = [];
        scopeContext.getFieldTemplateUrl = getFieldTemplateUrl;
        scopeContext.addPrimaryField = addPrimaryField;
        scopeContext.addFileUploadField = addFileUploadField;
        scopeContext.addObjectReferenceField = addObjectReferenceField;
        scopeContext.addSingleSelectionField = addSingleSelectionField;
        scopeContext.deleteSingleSelectionItem = deleteSingleSelectionItem;
        scopeContext.deleteObjectDefinitionField = deleteObjectDefinitionField;
        scopeContext.addSingleSelectionItem = addSingleSelectionItem;
        scopeContext.saveChanges = saveChanges;

        UserFactory.getAccessToken()
            .then(function (response) {
                Initialize();
                return response;
            });

        function Initialize() {
            ObjectRepositoryDataService.getObjectDefinitionById(scopeContext.objectDefId)
                               .then(function (data) {
                                   scopeContext.CurrentObjectDefinition = data;

                                   if (scopeContext.CurrentObjectDefinition == null
                                       || scopeContext.CurrentObjectDefinition == "") {
                                       scopeContext.CurrentObjectDefinition = {};
                                   }

                                   if (scopeContext.CurrentObjectDefinition.objectFields == null) {
                                       scopeContext.CurrentObjectDefinition.objectFields = new Array();
                                   }

                                   scopeContext.CurrentObjectDefinition.objectDefinitionGroupID = scopeContext.groupId;
                               });

            ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(function (data) {
                scopeContext.AvailableObjectDefinitions = data;
            });
        }

        function getFieldTemplateUrl(typeid) {
            switch (typeid) {
                case 1:  //ObjectReference
                    return "/ngView/ObjectRepository/ObjRefDefinitionField";
                case 2:  //SingleSelection
                    return "/ngView/ObjectRepository/SingleSDefinitionField";
                default: //for File, PrimeType
                    return "/ngView/ObjectRepository/GeneralDefinitionField";
            }
        }

        function addPrimaryField(primeDataType) {
            var primaryDataType = {
                objectDefinitionFieldName: PrimaryTypesNameLangs[primeDataType],
                objectDefinitionFieldKey: PrimaryTypesKeyCodeLangs[primeDataType],
                generalObjectDefinitionFiledType: 0,
                primeDataType: primeDataType
            };

            scopeContext.CurrentObjectDefinition.objectFields.splice(0,0,primaryDataType);
        }

        function addFileUploadField() {
            var field = {
                objectDefinitionFieldName: AppLang.DATA_CUST_FIELD_FILE,
                objectDefinitionFieldKey: AppLang.DATA_CUST_FIELD_FILE_KEY,
                generalObjectDefinitionFiledType: 3
            };
            scopeContext.CurrentObjectDefinition.objectFields.splice(0, 0, field);
        }
        function addObjectReferenceField() {
            var field = {
                objectDefinitionFieldName: AppLang.DATA_CUST_FIELD_OBJ_REF,
                objectDefinitionFieldKey: AppLang.DATA_CUST_FIELD_OBJ_REF_KEY,
                generalObjectDefinitionFiledType: 1
            };
            scopeContext.CurrentObjectDefinition.objectFields.splice(0, 0, field);
        }

        function addSingleSelectionField() {
            var field = {
                objectDefinitionFieldName: AppLang.DATA_CUST_FIELD_SL,
                objectDefinitionFieldKey: AppLang.DATA_CUST_FIELD_SL_KEY,
                generalObjectDefinitionFiledType: 2
            };
            scopeContext.CurrentObjectDefinition.objectFields.splice(0, 0, field);
        }

        function deleteSingleSelectionItem(field, item){
            var itemIndex = field.selectionItems.indexOf(item);
            if (itemIndex >= 0) {

                if (item.selectItemID != null) {
                    try {
                        ObjectRepositoryDataService.deleteSingleSelectionFieldItem(item.selectItemID)
                        .then(function (data) {
                            //update the UI
                            field.selectionItems.splice(itemIndex, 1);
                        })
                    }
                    catch (e) {
                        Notification.error({
                            message: AppLang.COMMON_EDIT_SAVE_FAILED + e.message,
                            delay: 5000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_ERROR,
                        });
                    }
                }
                else {
                    field.selectionItems.splice(itemIndex, 1);
                }
            }
        }

        function addSingleSelectionItem(field) {
            if (field.selectionItems == null) {
                field.selectionItems = new Array();
            }

            field.selectionItems.push({
                selectDisplayName: AppLang.DATA_CUST_SL_ITEM,
                selectItemKey: AppLang.DATA_CUST_SL_ITEM_KEY
            });
        }

        function deleteObjectDefinitionField(field) {
            try{
                if (field != null) {
                    var index = scopeContext.CurrentObjectDefinition.objectFields.indexOf(field);
                    //if found
                    if (index >= 0) {

                        //if object defintion is already on server, then delete it.
                        if (field.objectDefinitionFieldID != null) {
                            ObjectRepositoryDataService.deleteObjectDefintionField(field.objectDefinitionFieldID)
                                .then(function (data) {
                                    scopeContext.CurrentObjectDefinition.objectFields.splice(index, 1);
                                });
                        }
                        else {
                            scopeContext.CurrentObjectDefinition.objectFields.splice(index, 1);
                        }
                    }
                }
            }
            catch (e) {
                Notification.error({ message: AppLang.COMMON_DELETE_FAILED + e.message, delay: 5000, positionY: 'bottom', positionX: 'right' });
            }
        }

        function saveChanges() {
            try{
                ObjectRepositoryDataService.createOrUpdateObjectDefintion(scopeContext.CurrentObjectDefinition.objectDefinitionID,
                    scopeContext.CurrentObjectDefinition)
                .then(function (data) {
                    Notification.success({
                        message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                        delay: 3000,
                        positionY: 'bottom',
                        positionX: 'right',
                        title: AppLang.COMMON_DLG_TITLE_WARN,
                    });
                });
            }
            catch (e) {
                Notification.error({
                    message: AppLang.COMMON_EDIT_SAVE_FAILED + e.message,
                    delay: 5000,
                    positionY: 'bottom',
                    positionX: 'right',
                    title: AppLang.COMMON_DLG_TITLE_ERROR
                });
            }
        }
    }

    function ObjectDefintionListController($scope, $route, $routeParams, $location, ObjectRepositoryDataService, UserFactory) {
        var scopeContext = $scope;
        scopeContext.$route = $route;
        scopeContext.$location = $location;
        scopeContext.$routeParams = $routeParams;
        scopeContext.groupId = $routeParams.groupId;
        scopeContext.ObjectDefintions = [];
        scopeContext.deleteObjectDefintion = deleteObjectDefintion;

        UserFactory.getAccessToken()
            .then(function (response) {
                Activate();
                return response;
            });

        function Activate(){
            return ObjectRepositoryDataService.getObjectDefintionsbyGroup(scopeContext.groupId)
                    .then(function(data){
                        scopeContext.ObjectDefintions = data;

                        return scopeContext.ObjectDefintions;
                    });
        }

        function deleteObjectDefintion(defintionId) {
            ObjectRepositoryDataService.deleteObjectDefintion(defintionId)
            .then(function () {
                    //delete the Object defintion in the list model, angular will update the UI automatically.
                    if(scopeContext.ObjectDefintions != null){
                        scopeContext.ObjectDefintions.forEach(function(item, index){
                            if (item.objectDefinitionID == defintionId) {
                                scopeContext.ObjectDefintions.splice(index, 1);
                            }
                        });
                    }
            });
        }
    }

    function ObjectDefinitionController($scope, $route, $routeParams, $location, ObjectRepositoryDataService, UserFactory) {

        var scopeContext = $scope;
        scopeContext.$route = $route;
        scopeContext.$location = $location;
        scopeContext.$routeParams = $routeParams;
        scopeContext.DefinitionGroups = [];

        UserFactory.getAccessToken()
            .then(function (response) {
                Activate(null);
                return response;
            });

        function Activate(parentGroupId) {
            return ObjectRepositoryDataService.getObjectDefinitionGroups(parentGroupId)
                        .then(function (data) {
                            scopeContext.DefinitionGroups = data;

                            return scopeContext.DefinitionGroups;
                        });
        }
    };

})();