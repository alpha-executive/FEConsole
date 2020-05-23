;(function () {
    "use strict";

    angular
        .module('ngObjectRepository')
          .controller("DocumentController", DocumentController);

    DocumentController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "Upload", "UserFactory","API_BASEURL"];
    
    function DocumentController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, Upload, UserFactory, API_BASEURL) {
        var vm = this;
        //root folder.
        vm.currentEditingDirectory = null;
        vm.currentWorkingDirectoryId = "0";
        vm.searchText = "";
        vm.navpaths = [];
        vm.directories = [];
        vm.documents = [];
        vm.currentEditingDocument = null;
        vm.objectDefinitions = [];
        vm.displayMode = "list";
        vm.viewMode = "listView";
        vm.cancelObject = {};
        vm.baseUrl = API_BASEURL;

        UserFactory.getAccessToken()
            .then(function (response) {
                init();
                return response;
            });

        function init() {
            ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
                function(data){
                    vm.objectDefinitions = data;

                    return vm.objectDefinitions;
                }).then(function(data){
                    var rootFolder = createNewFolderObject(AppLang.BOOK_INDEX_ROOT_FOLDER, vm.currentWorkingDirectoryId);
                    rootFolder.objectID = 0;
                    vm.currentEditingDirectory = rootFolder;
                    vm.navpaths.unshift(rootFolder);

                    vm.reloadDirectories();
                    vm.reloadFiles();

                    return rootFolder;
                });
        }

        

        function createNewFolderObject(objectName, parentDirectory) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("FileDirectory");
            tempObj.objectName = objectName;
            objectUtilService.addStringProperty(tempObj, "directoryPathFlag", parentDirectory);
            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function createNewFileObject(objectName, parentDirectory) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("Documents");
            tempObj.objectName = objectName;

            objectUtilService.addFileProperty(tempObj, "documentFile", null);
            objectUtilService.addObjectRefProperty(tempObj, "documentDirectory", parseInt(parentDirectory))
            objectUtilService.addStringProperty(tempObj, "documentVersion", "1.0");
            objectUtilService.addIntegerProperty(tempObj, "documentSharedLevel", 0);

            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        vm.setViewMode = function (viewMode) {
            if (viewMode != vm.viewMode) {
                vm.viewMode = viewMode;
            }
        }

        vm.documentEditing = function (document) {
            vm.displayMode = "documentEditing";

            vm.currentEditingDocument = document;
            if (vm.currentEditingDocument == null) {
                var tempObj = createNewFileObject(AppLang.BOOK_INDEX_NEW_DOCUMENT, vm.currentWorkingDirectoryId);
                vm.documents.unshift(tempObj);
                vm.currentEditingDocument = tempObj;
            }
            
            vm.cancelObject = objectUtilService.cloneJsonObject(vm.currentEditingDocument);
        }

        vm.documentDelete = function (document) {
            if (document.objectID != 0 && document.objectID != null) {
                ObjectRepositoryDataService.deleteServiceObject(document.objectID);
            }
            
            var index = vm.documents.indexOf(document);
            if (index >= 0) {
                vm.documents.splice(index, 1);
            }
        }
        vm.directorEditing = function (dir) {
            vm.displayMode = "dirEditing";
            //it's a new create directory
            if (dir == null) {
                var tempObj = createNewFolderObject(AppLang.BOOK_INDEX_NEW_FOLDER, vm.currentWorkingDirectoryId);
                vm.currentEditingDirectory = tempObj;
                vm.directories.push(tempObj);
            }
            else {
                vm.currentEditingDirectory = dir;
            }
            
            vm.cancelObject = objectUtilService.cloneJsonObject(vm.currentEditingDirectory);
        }

        vm.cancelDirectoryEdit = function () {
            vm.displayMode = "list";
            var index = vm.directories.indexOf(vm.currentEditingDirectory);

            if (index >= 0) {
                vm.cancelObject.objectID == 0 || vm.cancelObject.objectID == null ? vm.directories.splice(index, 1)
                : vm.directories.splice(index, 1, vm.cancelObject);
            }
        }

        vm.deleteDirectory = function (dir) {
            //skip the root directory.
            if (dir.objectID == null || dir.objectID == 0)
                return;

            //delete the files under this folder
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Documents",
                 "documentDirectory",
                 null,
                 null,
                 "documentDirectory," + dir.objectID
             ).then(function (data) {
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var f = data[i];
                         if (f.objectID != 0 && f.objectID != null) {
                             ObjectRepositoryDataService.deleteServiceObject(f.objectID);
                         }
                     }
                 }
             }).then(function (data) {
                 //delete sub folders.
                 ObjectRepositoryDataService.getServiceObjectsWithFilters(
                       "FileDirectory",
                       "directoryPathFlag",
                       null,
                       null,
                       "directoryPathFlag," + dir.objectID
                   ).then(function (data) {
                       for (var i = 0; i < data.length; i++) {
                               var dir = data[i];
                               //recurve to delete all the sub directories.
                               if (dir.objectID != 0 && dir.objectID != null) {
                                   vm.deleteDirectory(dir);
                               }
                           }

                       return data;
                   }).then(function(data){
                       if (dir.objectID != null && dir.objectID != 0) {
                           ObjectRepositoryDataService.deleteServiceObject(dir.objectID);
                       }

                       //delete the directory object from directory list.
                       var index = vm.directories.indexOf(dir);
                       if (index >= 0) {
                           vm.directories.splice(index, 1);
                       }
                   });
             });
        }

        vm.saveDirectory = function () {
            objectUtilService.saveServiceObject(vm.currentEditingDirectory, function (data) {
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
                        var tmpDir = objectUtilService.parseServiceObject(data);
                        var index = vm.directories.indexOf(vm.currentEditingDirectory);
                        if (index >= 0) {
                            vm.directories.splice(index, 1, tmpDir);
                        }

                        vm.currentEditingDirectory = tmpDir;
                        vm.displayMode = "list";
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
            });
        }
        
        //for file upload handler.
        vm.uploadFiles = function (file, errFiles, documentobj) {
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
                        documentobj.objectName = objectUtilService.cloneJsonObject(file.result.files[0].fileName);
                        documentobj.properties.documentFile.fileName = file.result.files[0].fileName;
                        documentobj.properties.documentFile.fileUrl = file.result.files[0].fileUrl;
                        documentobj.properties.documentFile.fileCRC = file.result.files[0].fileCRC;
                        documentobj.properties.documentFile.fileExtension = file.result.files[0].fileExtension;
                        documentobj.properties.documentFile.created = file.result.files[0].created;
                        documentobj.properties.documentFile.updated = file.result.files[0].updated;
                        documentobj.properties.documentFile.freated = file.result.files[0].created;
                        documentobj.properties.documentFile.fileSize = file.result.files[0].fileSize;
                        documentobj.properties.documentFile.fileFullPath = file.result.files[0].fileFullPath;
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

        vm.saveDocument = function () {
            objectUtilService.saveServiceObject(vm.currentEditingDocument, function (data) {
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
                        var tmpdoc = objectUtilService.parseServiceObject(data);
                        var index = vm.documents.indexOf(vm.currentEditingDocument);
                        if (index >= 0) {
                            vm.documents.splice(index, 1, tmpdoc);
                        }
                        vm.displayMode = "list";
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
            });
        }
        vm.cancelDocumentEdit = function () {
            var index = vm.documents.indexOf(vm.currentEditingDocument);
            if (index >= 0) {
                vm.cancelObject.objectID == null || vm.cancelObject.objectID == 0 ? vm.documents.splice(index, 1) :
                        vm.documents.splice(index, 1, vm.cancelObject);
            }

            vm.displayMode = "list";  
        }
        
        vm.folderdbclick = function (folder) {
            vm.navpaths.push(folder);
            vm.currentWorkingDirectoryId = folder.objectID.toString();
            vm.reloadDirectories();
            vm.reloadFiles();
        }

        vm.navPath = function (navItem) {
            //if navitem is the current navitem, then stop processing it.
            if (navItem.objectID == vm.currentWorkingDirectoryId)
                return;

            vm.displayMode = "list";
            var index = vm.navpaths.indexOf(navItem);
            vm.navpaths = vm.navpaths.splice(0, index + 1);
            vm.currentWorkingDirectoryId = navItem.objectID.toString();
            vm.reloadDirectories();
            vm.reloadFiles();
        }

        vm.reloadDirectories = function () {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                  "FileDirectory",
                  "directoryPathFlag",
                  null,
                  null,
                  "directoryPathFlag," + vm.currentWorkingDirectoryId
              ).then(function (data) {
                  vm.directories.splice(0, vm.directories.length);
                  if (Array.isArray(data) && data.length > 0) {
                    
                      for (var i = 0; i < data.length; i++) {
                          var directory = objectUtilService.parseServiceObject(data[i]);
                          vm.directories.push(directory);
                      }
                  }

                  return vm.directories;
              });
        }

        vm.reloadFiles = function () {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Documents",
                 ["documentFile", "documentDirectory", "documentVersion", "documentSharedLevel"].join(),
                 null,
                 null,
                 "documentDirectory," + vm.currentWorkingDirectoryId
             ).then(function (data) {
                 vm.documents.splice(0, vm.documents.length);
                 if (Array.isArray(data) && data.length > 0) {
                    
                     for (var i = 0; i < data.length; i++) {
                         var document = objectUtilService.parseServiceObject(data[i]);
                         vm.documents.push(document);
                     }
                 }
                 return vm.documents;
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