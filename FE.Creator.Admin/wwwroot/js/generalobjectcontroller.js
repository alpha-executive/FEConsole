;(function () {
    //the object list page controller
    angular.module("ngObjectRepository")
        .controller('GeneralObjectListController', GeneralObjectListController);

    //configure the ngselect as bootstrap UI.
    angular.module("ngObjectRepository").config(function (uiSelectConfig) {
        uiSelectConfig.theme = 'bootstrap';
    });

    //ngselect filter.
    angular.module("ngObjectRepository").filter('propsFilter', function () {
        return function (items, props) {
            var out = [];

            if (angular.isArray(items)) {
                var keys = Object.keys(props);

                items.forEach(function (item) {
                    var itemMatches = false;

                    for (var i = 0; i < keys.length; i++) {
                        var prop = keys[i];
                        var text = props[prop].toLowerCase();
                        if (item[prop].toString().toLowerCase().indexOf(text) !== -1) {
                            itemMatches = true;
                            break;
                        }
                    }

                    if (itemMatches) {
                        out.push(item);
                    }
                });
            } else {
                // Let the output be the input untouched
                out = items;
            }

            return out;
        };
    });

    //object dynamic column filter.
    angular.module("ngObjectRepository")
        .filter('objectValueFilter', function () {
            return function (items, field) {
                var out = "";
                if (angular.isArray(items)) {
                    for (var idx = 0; idx < items.length; idx++) {
                        if (items[idx].keyName == field.objectDefinitionFieldName) {
                            switch (field.generalObjectDefinitionFiledType) {
                                case 0:
                                    out = '' + items[idx].value.value;
                                    break;
                                case 1:
                                    out = items[idx].referedObject != null ? items[idx].referedObject.objectName
                                        : '';
                                    break;
                                case 2:
                                    var findItem = null;
                                    for (var k = 0; k < items[idx].selectionItems.length; k++) {
                                        var item = items[idx].selectionItems[k];
                                        if (item.selectItemID == items[idx].value.selectedItemID) {
                                            findItem = item;
                                            break;
                                        }
                                    }

                                    out = findItem == null ? '' : findItem.selectDisplayName;
                                    break;
                                case 3:
                                    out = '' + items[idx].value.fileName;
                                    break;
                                default:
                                    break;
                            }

                            //if found, end the search.
                            break;
                        }//end if
                    }//end for
                }//end if

                return out;
            }
        });

    //controller injection service.
    GeneralObjectListController.$inject = ["$scope", "ObjectRepositoryDataService", "Upload", "Notification", "PagerService", "UserFactory", "API_BASEURL"];

    //controller main function.
    function GeneralObjectListController($scope, ObjectRepositoryDataService, Upload, Notification, PagerService, UserFactory, API_BASEURL) {
        var vm = this;

        vm.disabled = undefined; //enable ngselect
        vm.searchEnabled = undefined; //disable ngselect search
        vm.disabledNewObject = true; //disable the add new object
        vm.viewMode = 'list';  //by default, the view is list.
        vm.currentObjectDefinition = {}; //current object definition.
        vm.ObjectDefintions = []; //all the available system object definitions.
        vm.CustomObjectDefinitions = []; //all the custom defined object definitions.
        vm.ObjectDefGroups = [];  //all the available object definition groups.
        vm.GetObjectDefinitionGroup = GetObjectDefinitionGroup; //get the group name of the object definition
        vm.onDefinitionChanged = onDefinitionChanged; //object definition change event.
        vm.viewObjectDetails = viewObjectDetails; //view service object.
        vm.editObject = editObject; //edit service object.
        vm.deleteObject = deleteObject; //delete service object.
        vm.createObject = createObject; //create a service object.
        vm.saveChanges = saveChanges;   //save the changes to service object
        vm.loadReferedObjectList = loadReferedObjectList;   //load refered objects of the object reference field.
        vm.getObjectFieldTemplateUrl = getObjectFieldTemplateUrl; //get the template of the specific field type.
        vm.currentGeneralObject = {}; //current service object.
        vm.onSelectObjectReference = onSelectObjectReference;  //select a new object reference event.
        vm.ServiceObjectList = [];  //available object lists.
        vm.pager = {};  //for page purpose.
        vm.onPageClick = onPageClick;
        vm.pageSize = 11;
        vm.currentPageIndex = 0;
        vm.displayedColumns = [];
        vm.onColumnChange = onColumnChange;
        vm.baseUrl = API_BASEURL;


        UserFactory.getAccessToken()
            .then(function (response) {
                //initialize the controller.
                Activate();
                return response;
            });


        function onColumnChange(column) {
            var index = vm.displayedColumns.indexOf(column);
            if (index < 0) {
                vm.displayedColumns.push(column);
                column.checked = true;
            }
            else {
                vm.displayedColumns.splice(index, 1);
                column.checked = undefined;
            }
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
                vm.reloadServiceObjects(pageIndex);
            });
        }

        vm.reCalculatePager = function (pageIndex) {
            return ObjectRepositoryDataService.getServiceObjectCount(vm.currentObjectDefinition.objectDefinitionID)
                .then(function (data) {
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

        vm.reloadServiceObjects = function (pageIndex) {
            var searchColumns = [];
            vm.currentObjectDefinition.objectFields.forEach(function (of, idx, ar) {
                searchColumns.push(of.objectDefinitionFieldName);
            });

            ObjectRepositoryDataService.getServiceObjects(vm.currentObjectDefinition.objectDefinitionID, searchColumns.toString(), pageIndex, vm.pageSize)
                .then(function (data) {
                    vm.ServiceObjectList = data;
                    return vm.ServiceObjectList;
                });
        }

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
                        objfield.value.fileName = file.result.files[0].fileName;
                        objfield.value.fileUrl = file.result.files[0].fileUrl;
                        objfield.value.fileCRC = file.result.files[0].fileCRC;
                        objfield.value.fileExtension = file.result.files[0].fileExtension;
                        objfield.value.created = file.result.files[0].created;
                        objfield.value.updated = file.result.files[0].updated;
                        objfield.value.freated = file.result.files[0].created;
                        objfield.value.fileSize = file.result.files[0].fileSize;
                        objfield.value.fileFullPath = file.result.files[0].fileFullPath;
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


        vm.enable = function () {
            vm.disabled = false;
        };

        vm.disable = function () {
            vm.disabled = true;
        };

        vm.enableNewObject = function () {
            vm.disabledNewObject = false;
        }
        vm.disableNewObject = function () {
            vm.disabledNewObject = true;
        }

        vm.enableSearch = function () {
            vm.searchEnabled = true;
        };

        vm.disableSearch = function () {
            vm.searchEnabled = false;
        };
        vm.setViewMode = function (view) {
            vm.viewMode = view;
        }

        function Activate() {
            ObjectRepositoryDataService.getObjectDefinitionGroups()
                .then(function (data) {
                    vm.ObjectDefGroups = data;

                    return vm.ObjectDefGroups;
                });

            ObjectRepositoryDataService.getLightWeightObjectDefinitions()
                .then(function (data) {
                    vm.ObjectDefintions = data;

                    return vm.ObjectDefintions;
                });

            ObjectRepositoryDataService.getCustomObjectDefinitions()
                .then(function (data) {
                    if (angular.isArray(data)) {
                        vm.CustomObjectDefinitions = data;

                        return vm.CustomObjectDefinitions;
                    }
                });
        }

        function viewObjectDetails(objectid) {
            findObjectAndSetView(objectid, 'view')
        }

        function editObject(objectid) {
            findObjectAndSetView(objectid, 'edit');
        }


        function deleteObject(objectid) {
            ObjectRepositoryDataService.deleteServiceObject(objectid)
                .then(function (data) {
                    //if there is no error to delete the object
                    if (data != null && (data.status == 200
                                    || data.status == 204)) {
                        var foundServiceItem = null;
                        vm.ServiceObjectList.forEach(function (item, index, arr) {
                            if (item.objectID == objectid) {
                                foundServiceItem = item;
                            }
                        });

                        if (foundServiceItem != null) {
                            if (vm.pager.totalPages > vm.pager.currentPage
                                || (vm.ServiceObjectList.length <= 1 && vm.pager.totalPages == vm.pager.currentPage)) {
                                var navPageIndex = vm.ServiceObjectList.length - 1 <= 0
                                    ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                                onPageChange(navPageIndex);
                            }
                            else {
                                var index = vm.ServiceObjectList.indexOf(foundServiceItem);
                                if (index >= 0) {
                                    vm.ServiceObjectList.splice(index, 1);
                                }
                            }
                        }
                    }
                    else {
                        Notification.error({
                            message: AppLang.COMMON_EDIT_SAVE_FAILED + data.toString(),
                            delay: 5000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_ERROR
                        });
                    }
                });
        }

        //find the service object by id and set the view mode.
        function findObjectAndSetView(objectid, view) {
            for (var idx = 0; idx < vm.ServiceObjectList.length; idx++) {
                if (vm.ServiceObjectList[idx].objectID == objectid) {
                    vm.currentGeneralObject = vm.ServiceObjectList[idx];
                    loadReferedObjects(vm.currentGeneralObject);
                    vm.setViewMode(view);
                    break;
                }
            }
        }

        //load the refered objects for object refer field and single selection field.
        function loadReferedObjects(currSvcObj) {
            for (var i = 0; i < vm.currentObjectDefinition.objectFields.length; i++) {
                for (var idx = 0; idx < currSvcObj.properties.length; idx++) {
                    //refered object field.
                    if (vm.currentObjectDefinition.objectFields[i].generalObjectDefinitionFiledType == 1
                        && vm.currentObjectDefinition.objectFields[i].objectDefinitionFieldName == currSvcObj.properties[idx].keyName) {
                        var currField = currSvcObj.properties[idx];
                        loadObjectbyId(currField.value.referedGeneralObjectID)
                            .then(function (data) {
                                currField.referedObject = data;
                            });
                    }

                    //single selection
                    if (vm.currentObjectDefinition.objectFields[i].generalObjectDefinitionFiledType == 2
                        && vm.currentObjectDefinition.objectFields[i].objectDefinitionFieldName == currSvcObj.properties[idx].keyName) {

                        var currField = currSvcObj.properties[idx];
                        currField.selectionItems = vm.currentObjectDefinition.objectFields[i].selectionItems;
                    }
                }
            }
        }

        function loadReferedObjectList(field) {
            for (var i = 0; i < vm.currentObjectDefinition.objectFields.length; i++) {
                //refered object field.
                if (vm.currentObjectDefinition.objectFields[i].generalObjectDefinitionFiledType == 1
                    && vm.currentObjectDefinition.objectFields[i].objectDefinitionFieldName == field.keyName) {

                    if (field.referedObjectList == null) {
                        ObjectRepositoryDataService.getServiceObjects(vm.currentObjectDefinition.objectFields[i].referedObjectDefinitionID, null)
                            .then(function (data) {
                                field.referedObjectList = data || [];

                                return field.referedObjectList;
                            });
                    }
                }
            }
        }

        function onSelectObjectReference(obj, objfield) {
            objfield.referedObject = obj;
            objfield.value.referedGeneralObjectID = obj.objectID;
        }

        function GetObjectDefinitionGroup(objdef) {
            var foundItem = {};
            vm.ObjectDefGroups.forEach(function (item, index, arr) {
                if (objdef.objectDefinitionGroupID == item.groupID) {
                    foundItem = item;
                }
            });

            return foundItem.groupName || "Unknown Group";
        }

        function onDefinitionChanged($item, $model) {
            if ($item != null) {
                vm.enableNewObject();

                vm.displayedColumns = [];
                vm.pager = {};
                vm.currentPageIndex = 0;

                //set to page 1.
                onPageChange(1);
            }
            else {
                vm.disabledNewObject()
            }
        }

        //load the object general information by id.
        function loadObjectbyId(id) {
            return ObjectRepositoryDataService.getServiceObject(id)
                .then(function (data) {
                    return data;
                });
        }

        function getObjectFieldTemplateUrl(objectDefinitionFieldName) {

            var fieldType = -1;
            for (var index = 0; index < vm.currentObjectDefinition.objectFields.length; index++) {
                if (vm.currentObjectDefinition.objectFields[index].objectDefinitionFieldName == objectDefinitionFieldName) {
                    fieldType = vm.currentObjectDefinition.objectFields[index].generalObjectDefinitionFiledType;
                    break;
                }
            }

            switch (fieldType) {
                case 0:
                    return '/ngview/ObjectRepository/GeneralObjectPrimeField';
                case 1:
                    return '/ngview/ObjectRepository/GeneralObjectObjRefField'
                case 2:
                    return '/ngview/ObjectRepository/GeneralObjectSSelectField'
                case 3:
                    return '/ngview/ObjectRepository/GeneralObjectFileField'
                default:
                    return '';
            }
        }

        //initialize the new service object instance.
        function createObject() {
            vm.currentGeneralObject = {};
            vm.currentGeneralObject.properties = [];
            vm.currentGeneralObject.objectName = '';
            vm.currentGeneralObject.objectDefinitionId = vm.currentObjectDefinition.objectDefinitionID;

            for (var i = 0; i < vm.currentObjectDefinition.objectFields.length; i++) {
                var property = {};
                property.keyName = vm.currentObjectDefinition.objectFields[i].objectDefinitionFieldName;
                property.$type = "FE.Creator.ObjectRepository.ServiceModels.ObjectKeyValuePair, FE.Creator.ObjectRepository";
                property.value = {};
                if (vm.currentObjectDefinition.objectFields[i].generalObjectDefinitionFiledType == 0) {
                    property.value.$type = "FE.Creator.ObjectRepository.ServiceModels.PrimeObjectField, FE.Creator.ObjectRepository";
                    property.value.primeDataType = vm.currentObjectDefinition.objectFields[i].primeDataType;
                }

                if (vm.currentObjectDefinition.objectFields[i].generalObjectDefinitionFiledType == 1) {
                    property.referedObject = {};
                    property.value.$type = "FE.Creator.ObjectRepository.ServiceModels.ObjectReferenceField, FE.Creator.ObjectRepository";
                }
                if (vm.currentObjectDefinition.objectFields[i].generalObjectDefinitionFiledType == 2) {
                    property.value.$type = "FE.Creator.ObjectRepository.ServiceModels.SingleSelectionField, FE.Creator.ObjectRepository";
                    property.selectionItems = vm.currentObjectDefinition.objectFields[i].selectionItems;
                }

                if (vm.currentObjectDefinition.objectFields[i].generalObjectDefinitionFiledType == 3) {
                    property.value.$type = "FE.Creator.ObjectRepository.ServiceModels.ObjectFileField, FE.Creator.ObjectRepository";
                }

                vm.currentGeneralObject.properties.push(property);
            }

            vm.setViewMode('edit');
        }

        //save new/edit changes.
        function saveChanges() {
            try {
                ObjectRepositoryDataService.createOrUpdateServiceObject(vm.currentGeneralObject.objectID, vm.currentGeneralObject)
                    .then(function (data) {
                        if (data == null || data == "" || data.objectID != null) {

                            if (data != null && data != "") {
                                if ((vm.currentGeneralObject.objectID == null || vm.currentGeneralObject.objectID == 0)
                                    && vm.ServiceObjectList.length >= vm.pager.pageSize) {
                                    onPageChange(1);
                                } else {
                                    var searchColumns = [];
                                    vm.currentObjectDefinition.objectFields.forEach(function (of, idx, ar) {
                                        searchColumns.push(of.objectDefinitionFieldName);
                                    });

                                    //add the new created object to object list.
                                    ObjectRepositoryDataService.getServiceObject(data.objectID, searchColumns.toString())
                                        .then(function (data) {
                                            if (data.objectID != null && data.objectID > 0) {
                                                var index = vm.ServiceObjectList.indexOf(vm.currentGeneralObject);
                                                if (index >= 0) {
                                                    vm.ServiceObjectList.splice(index, 1, data);
                                                } else {
                                                    vm.ServiceObjectList.unshift(data);
                                                }
                                            }

                                            return data;
                                        });
                                }
                            }

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
                                message: AppLang.COMMON_EDIT_SAVE_FAILED + data.toString(),
                                delay: 5000,
                                positionY: 'bottom',
                                positionX: 'right',
                                title: AppLang.COMMON_DLG_TITLE_ERROR
                            });
                        }
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
})();
