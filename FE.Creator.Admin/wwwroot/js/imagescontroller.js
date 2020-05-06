(function () {
    "use strict";

    angular
       .module('ngObjectRepository')
        .controller("ImagesController", ImagesController);

    ImagesController.$inject = ["$scope", "$q", "$filter", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "Upload", "UserFactory", "API_BASEURL", "Lightbox"];

    function ImagesController($scope, $q, $filter, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, Upload, UserFactory, API_BASEURL, Lightbox) {
        var vm = this;
        vm.DEFAULT_IMAGE_album = 0;
        vm.displayMode = "imageList";
        vm.objectDefinitions = [];
        vm.cancelObject = null;
        vm.editingImageObject = null;
        vm.editingalbumObject = null;
        vm.images = [];
        vm.lightboxImages = [];
        vm.imageRows = [];
        vm.albums = [];
        vm.pager = {};
        vm.pageSize = 8;
        vm.onPageClick = onPageClick;
        vm.albumTypes = [];
        vm.selectionImages = [];
        //by default, it's show all image mode.
        vm.currentalbum = null;
        vm.displayLargeImage = false;
        vm.viewingImage = null;
        vm.baseUrl = API_BASEURL;

        UserFactory.getAccessToken()
            .then(function (response) {
                init();
                return response;
            });

        function init() {
            //$scope.$watch("vm.imageRows", null, true);
            ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
              function (data) {
                  vm.objectDefinitions = data;

                  return vm.objectDefinitions;
              }).then(function (data) {
                  //get all the categories and books.
                  //vm.reloadImages();
                  initalbumTypes();
                  onPageChange(1);
                  vm.reloadalbums();
              });
        }

        function initalbumTypes() {
            vm.albumTypes.push({
                typeName: AppLang.IMAGEMGR_STD_LIST,
                typeValue: 1
            });
            vm.albumTypes.push({
                typeName:  AppLang.IMAGEMGR_SLD_SHOW,
                typeValue: 2
            });
            vm.albumTypes.push({
                typeName: AppLang.IMAGEMGR_WATER_FLOW,
                typeValue: 3
            });
        }
        function createImageServiceObject(album, imageTitle) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("Photos");
            tempObj.objectName = imageTitle;

            objectUtilService.addFileProperty(tempObj, "imageFile", null);
            objectUtilService.addStringProperty(tempObj, "imageDesc", null);
            objectUtilService.addObjectRefProperty(tempObj, "imageCategory", album);
            objectUtilService.addIntegerProperty(tempObj, "imageSharedLevel", 0);

            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function createalbumServiceObject(albumName){
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("ImageCategory");
            tempObj.objectName = albumName;

            objectUtilService.addIntegerProperty(tempObj, "categorySharedLevel", 0);
            objectUtilService.addIntegerProperty(tempObj, "imageCategoryType", 1);
            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function sliceImages(images) {
            var colsize = 4;
            if (vm.imageRows.length > 0) {
                vm.imageRows.splice(0, vm.imageRows.length);
            }

            for (var i = 0; i < images.length; i += colsize) {
                vm.imageRows.push(images.slice(i, i + colsize));
            }
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
                vm.reloadImages(null,vm.pager.currentPage, vm.pageSize);
            });
        }

        function onPageClick(pageIndex, force) {
            if (vm.pager.currentPage == pageIndex && !force)
                return;

            onPageChange(pageIndex);
        }

        function changeImagesAlbum(album, images, isUpdateProgress) {
            var defers = [];
            var processedCount = 0;
            if (Array.isArray(images) && images.length > 0) {
                for (var i = 0; images != null && i < images.length; i++) {
                    var promise = function (idx) {
                        var defer = $q.defer();
                        var img = images[idx];

                        var referedObjectID = album == null ? 0 : album.objectID;
                        //release this image from the album.
                        img.properties.imageCategory.referedGeneralObjectID = referedObjectID;
                        objectUtilService.saveServiceObject(img, function (data) {
                            if (isUpdateProgress) {
                                processedCount += 1;
                            }

                            defer.resolve(img);
                        });

                        return defer.promise;
                    }

                    var cachedPromise = isUpdateProgress ? promise(i).then(function (data) {
                        vm.progress = Math.floor(processedCount * 100.0 / images.length);
                    }) : promise(i);

                    defers.push(cachedPromise);
                }
            }

            return $q.all(defers);
        }

        //get the display mode by album status.
        function getAlbumDisplayMode(album) {
            if (album == null)
                return "imageList";

            if (album.properties.imageCategoryType.value == 2) {
                return "carouselSlideImageshow";
            }

            if (album.properties.imageCategoryType.value == 3) {
                return "walterFlowImageShow";
            }

            return "standardImageList";
        }
        function clearImagesData() {
            if (vm.imageRows.length > 0) {
                vm.imageRows.splice(0, vm.imageRows.length);
            }
            if (vm.images.length > 0) {
                vm.images.splice(0, vm.images.length);
            }
        }
        vm.reCalculatePager = function (pageIndex) {
            var objDefinitionId = vm.getObjectDefintionIdByName("Photos");
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
        vm.switchTab = function (isalbum) {
            //clear the image to avoid display flash.
            clearImagesData();

            //reset the current album.
            vm.currentalbum = null;

            //if it's album
            if (isalbum) {
                vm.setDisplayMode("albumList");
                vm.reloadalbums();
            }
            else {
                vm.setDisplayMode("imageList");

                //force the page reload.
                vm.onPageClick(1, true);
            }
        }

        vm.switchalbum = function (album){
            if (vm.currentalbum != album) {
                vm.currentalbum = album;
            }
        }
        vm.onalbumDbClick = function (album) {
            vm.switchalbum(album);
            //if it's standard show.
            if (album != null) {
                vm.reloadAlbumImages(album);
                album.currentIndex = 1;
                album.viewIndex = 1;

                if (album.properties.imageCategoryType.value == 1) {
                    vm.setDisplayMode("standardImageList");
                }

                if (album.properties.imageCategoryType.value == 2) {
                    vm.setDisplayMode("carouselSlideImageshow");
                }

                if (album.properties.imageCategoryType.value == 3) {
                    vm.setDisplayMode("walterFlowImageShow");
                }
            }
        }

        vm.reloadAlbumImages = function (album) {
            //always get all the images from a album.
            vm.reloadImages(album, 1, null)
              .then(function (images) {
                  vm.loadMoreAlbumImages();
              })
        }

        vm.setDisplayMode = function (mode) {
            if (vm.displayMode != mode) {
                vm.displayMode = mode;
            }
        }
        vm.createOrEditImagealbum = function (album) {
            var svcObj = null;
            if (album == null) {
                svcObj = createalbumServiceObject(AppLang.IMAGEMGR_NEW_ALBUM);
            }
            else {
                svcObj = album;
            }

            vm.editingalbumObject = svcObj;
            vm.cancelObject = objectUtilService.cloneJsonObject(svcObj);
            vm.displayMode = "albumEdit";

            return svcObj;
        }

        vm.createOrEditImage = function (image) {
            var svcObj = null;
            if (image == null) {
                svcObj = createImageServiceObject(vm.DEFAULT_IMAGE_album, AppLang.IMAGEMGR_NEW_IMAGE);
            }
            else {
                svcObj = image;
            }

            vm.editingImageObject = svcObj;
            vm.cancelObject = objectUtilService.cloneJsonObject(svcObj);

            //editing a image in default image gallary.
            if (vm.currentalbum == null) {
                vm.displayMode = "imageEdit";
            }
            else {
                vm.displayMode = "albumImageEdit";
            }
        }

        vm.saveEditingImage = function () {
            objectUtilService.saveServiceObject(vm.editingImageObject, function (data) {
                if (data == null || data == "" || data.objectID != null) {
                    //update the object id.
                    if (data != null && data != "") {
                        var tmpimg = objectUtilService.parseServiceObject(data);

                        //if it's a new add operation.
                        //edit an image in album will always goes to else branch.
                        if ((vm.editingImageObject.objectID == null
                            || vm.editingImageObject.objectID == 0
                            || typeof (vm.editingImageObject.objectID) === 'undefined')
                            && vm.images.length >= vm.pager.pageSize) {
                            //redirect to the first page if the total image count is greater than page size.
                            //this could display the new added image.
                            onPageChange(1);
                        }
                        else {
                            var index = vm.images.indexOf(vm.editingImageObject);
                            //replace
                            if (index >= 0) {
                                vm.images.splice(index, 1, tmpimg);
                            }//insert
                            else {
                                vm.images.splice(0, 0, tmpimg);
                            }

                            //lightbox image support
                            vm.lightboxImages.push({
                                objectID: tmpimg.objectID,
                                url: $filter('fullUrl')(tmpimg.properties.imageFile.fileUrl),
                                caption: tmpimg.properties.imageDesc.value
                            })

                            sliceImages(vm.images);
                        }

                        vm.displayMode = getAlbumDisplayMode(vm.currentalbum);

                        Notification.success({
                            message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                            delay: 3000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_WARN,
                        });
                    }
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

        vm.cancelEditingImage = function () {
            var index = vm.images.indexOf(vm.editingImageObject);
            if (index >= 0) {
                vm.images.splice(index, 1, vm.cancelObject);
            }

            vm.displayMode = getAlbumDisplayMode(vm.currentalbum);
        }
        
        vm.saveEditingalbum = function () {
            objectUtilService.saveServiceObject(vm.editingalbumObject, function (data) {
                if (data == null || data == "" || data.objectID != null) {
                    //update the object id.
                    if (data != null && data != "") {
                        var tmpalbum = objectUtilService.parseServiceObject(data);

                        var index = vm.albums.indexOf(vm.editingalbumObject);
                        if (index >= 0) {
                            vm.albums.splice(index, 1, tmpalbum);
                        }
                        else {
                            vm.albums.unshift(tmpalbum);
                        }

                        vm.displayMode = "albumList";
                        Notification.success({
                            message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                            delay: 3000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: AppLang.COMMON_DLG_TITLE_WARN,
                        });
                    }
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

        vm.canceEditingalbum = function () {
            var index = vm.albums.indexOf(vm.editingalbumObject);
            if (index >= 0) {
                vm.albums.splice(index, 1, vm.cancelObject);
            }

            vm.displayMode = "albumList";
        }

        vm.deletealbum = function (album) {
            vm.reloadImages(album, 1, null)
              .then(function (images) {
                  changeImagesAlbum(null, images, false)
                    .then(function (data) {
                          ObjectRepositoryDataService.deleteServiceObject(album.objectID)
                          .then(function (data) {
                              var currIndex = vm.albums.indexOf(album);
                              if (currIndex >= 0) {
                                  vm.albums.splice(currIndex, 1);
                              }
                          });
                  });
              });
        }

        vm.deleteImage = function (image) {
            ObjectRepositoryDataService.deleteServiceObject(image.objectID).then(function (data) {
                if (vm.currentalbum == null && vm.pager.totalPages >= vm.pager.currentPage) {
                    var navPageIndex = vm.images.length - 1 <= 0
                           ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                    onPageChange(navPageIndex);
                }
                else {
                    var index = vm.images.indexOf(image);
                    if (index >= 0) {
                        vm.images.splice(index, 1);
                        sliceImages(vm.images);
                    }
                }
            });
        }

        vm.importImages = function (files, errFiles) {
            //import can be directly from alburm or from the default image gallary.
            vm.displayMode = vm.currentalbum == null ? "imageImport" : "albumImageImport";
            vm.files = files;
            vm.errorFiles = errFiles;
            vm.progress = 1;

            var reloaded = false;
            var uploadcount = 0;
            var objectCreationCount = 0;
            var errorCount = 0;
            
            var calculateProgress = function (uploadCount, creationCount) {
                vm.progress = Math.floor(uploadCount * 60.0 / files.length) +
                                                 Math.floor(creationCount * 40.0 / files.length);
                if (vm.progress == 100 && !reloaded) {
                    vm.displayMode = getAlbumDisplayMode(vm.currentalbum);
                    if (vm.displayMode == "imageList" || vm.displayMode == "standardImageList") {
                        //for all images list
                        if (vm.currentalbum == null) {
                            onPageChange(1);
                        }
                        else {
                            //for standard image list.
                            vm.onalbumDbClick(vm.currentalbum);
                            //vm.reloadAlbumImages(vm.currentalbum);
                        }
                    }
                    else {
                        //all image is required when the image is not in standard display mode.
                        vm.reloadAlbumImages(vm.currentalbum);
                    }

                    reloaded = true;
                }
            }

            var uploadSingleImage = function (file, totalfiles) {
                var uploadPromise = Upload.upload({
                    url: vm.baseUrl + '/api/Files?thumbinal=true',
                    data: {
                        file: file
                    }
                });

                var objCreationPromise = uploadPromise.then(function (response) {
                    file.result = response.data;
                    var albumObjectId = vm.currentalbum == null ? vm.DEFAULT_IMAGE_album : vm.currentalbum.objectID;
                    var imageobj = createImageServiceObject(albumObjectId, "New Image");
                    if (file.result.files.length > 0) {
                        uploadcount += 1;
                        calculateProgress(uploadcount, objectCreationCount);

                        imageobj.objectName = objectUtilService.cloneJsonObject(file.result.files[0].fileName);
                        imageobj.properties.imageFile.fileName = file.result.files[0].fileName;
                        imageobj.properties.imageFile.fileUrl = file.result.files[0].fileUrl;
                        imageobj.properties.imageFile.fileCRC = file.result.files[0].fileCRC;
                        imageobj.properties.imageFile.fileExtension = file.result.files[0].fileExtension;
                        imageobj.properties.imageFile.created = file.result.files[0].created;
                        imageobj.properties.imageFile.updated = file.result.files[0].updated;
                        imageobj.properties.imageFile.freated = file.result.files[0].created;
                        imageobj.properties.imageFile.fileSize = file.result.files[0].fileSize;
                        imageobj.properties.imageFile.fileFullPath = file.result.files[0].fileFullPath;
                    }

                    return imageobj;
                }, function (response) {
                    errorCount += 1;
                    if (response.status > 0)
                        vm.errorMsg = AppLang.getFormatString(AppLang.IMAGEMGR_UPLOAD_ERR_MSG, [errorCount, totalfiles]);
                }, function (evt) {
                });

                objCreationPromise.then(function (data) {
                    if (data != null && data.properties.imageFile != null) {
                        objectUtilService.saveServiceObject(data, function (data) {
                            objectCreationCount += 1;
                            calculateProgress(uploadcount, objectCreationCount);
                        });
                    }
                });
            }
            
            if (files != null && files.length > 0) {
                angular.forEach(files, function (file) {
                    uploadSingleImage(file, files.length);
                });
            }
        }

        vm.uploadImage = function (file, errFiles, imageobj) {
            vm.f = file;
            vm.errFile = errFiles && errFiles[0];
            if (file) {
                file.showprogress = true;

                file.upload = Upload.upload({
                    url: vm.baseUrl + '/api/Files?thumbinal=true',
                    data: {
                        file: file
                    }
                });

                file.upload.then(function (response) {
                    file.result = response.data;
                    if (file.result.files.length > 0) {
                        imageobj.objectName = objectUtilService.cloneJsonObject(file.result.files[0].fileName);
                        imageobj.properties.imageFile.fileName = file.result.files[0].fileName;
                        imageobj.properties.imageFile.fileUrl = file.result.files[0].fileUrl;
                        imageobj.properties.imageFile.fileCRC = file.result.files[0].fileCRC;
                        imageobj.properties.imageFile.fileExtension = file.result.files[0].fileExtension;
                        imageobj.properties.imageFile.created = file.result.files[0].created;
                        imageobj.properties.imageFile.updated = file.result.files[0].updated;
                        imageobj.properties.imageFile.freated = file.result.files[0].created;
                        imageobj.properties.imageFile.fileSize = file.result.files[0].fileSize;
                        imageobj.properties.imageFile.fileFullPath = file.result.files[0].fileFullPath;
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

        vm.saveSelectedImagesToAlbum = function () {
            var selectedImgs = [];
            if (vm.currentalbum == null)
                return;

            //filter the selected images
            for (var i = 0; i < vm.selectionImages.length; i++) {
                if (vm.selectionImages[i].selected) {
                    selectedImgs.push(vm.selectionImages[i]);
                }
            }

            changeImagesAlbum(vm.currentalbum, selectedImgs, true)
                .then(function (data) {
                    //return to the alburm view.
                    vm.onalbumDbClick(vm.currentalbum);
                });
        }

        vm.cancelAlbumImageSelection = function () {
            vm.onalbumDbClick(vm.currentalbum);
        }

        vm.viewLargeImage = function (img) {
            var image = $('#viewImageModal .img-responsive');
            var imgSpin = $("#imageLoadSpin");
            image.hide();
            imgSpin.show();

            console.log(img.properties.imageFile.fileUrl);
            var downloadImage = new Image();
            downloadImage.onload = function (event) {
                console.debug("onload is invoked");
                imgSpin.hide();
                image.show();
                image.attr("src", this.src);
            }
            downloadImage.onerror = function (error) {
                console.error("error is happened");
                console.error(error);
                imgSpin.find("p").text(AppLang.IMAGEMGR_VIEW_LOAD_ERROR);
            }
            downloadImage.src = $filter('fullUrl')(img.properties.imageFile.fileUrl);
            vm.viewingImage = img;
            $('#viewImageModal').modal('show');
        }

        vm.viewImageLightBox = function (img) {
            var currIndex = -1;
            for (var i = 0; i < vm.lightboxImages.length; i++) {
                if (vm.lightboxImages[i].objectID == img.objectID) {
                    currIndex = i;
                    break;
                }
            }

            Lightbox.openModal(vm.lightboxImages, currIndex);
        }

        vm.addImages2album = function () {
            vm.setDisplayMode("imageSelectionList");
            vm.progress = 0;
            album.currentSelectionIndex = 1;
            vm.reloadSelectionImages(1, vm.pageSize);
        }
        vm.reloadImages = function (album, pageIndex, pageSize) {
            var filter = album != null ?
                 "imageCategory," + album.objectID : null;

            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Photos",
                 ["imageFile", "imageDesc", "imageCategory", "imageSharedLevel"].join(),
                 pageIndex,
                 pageSize,
                 filter
            ).then(function (data) {
                 //clear the editing images and lightbox image.
                 vm.images.splice(0, vm.images.length);
                 vm.lightboxImages.splice(0, vm.images.length);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var image = objectUtilService.parseServiceObject(data[i]);
                         vm.images.push(image);
                         vm.lightboxImages.push({
                             objectID: image.objectID,
                             url: $filter('fullUrl')(image.properties.imageFile.fileUrl),
                             caption: image.properties.imageDesc.value
                         })
                     }
                 }

                 //slice is not required for album view mode.
                 if (vm.currentalbum == null) {
                     //group the images into rows.
                     sliceImages(vm.images);
                 }

                 return vm.images;
             });
        }

        vm.loadMoreSelectionImages = function (album) {
            if (album.currentSelectionIndex == null) {
                //first page already been loaded
                album.currentSelectionIndex = 2;
            }
            else {
                album.currentSelectionIndex += 1;
            }

            vm.reloadSelectionImages(album.currentSelectionIndex, vm.pageSize);
        }

        vm.loadMoreAlbumImages = function () {
            if (vm.currentalbum == null)
                return;

            if (vm.currentalbum.viewIndex == null) {
                vm.currentalbum.viewIndex = 1;
            }

            var images = vm.images.slice(0, vm.currentalbum.viewIndex * vm.pageSize);
            sliceImages(images);

            //set view index to next page.
            vm.currentalbum.viewIndex += 1;
        }

        vm.reloadSelectionImages = function (startIndex, pageSize) {
           return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                "Photos",
                ["imageFile", "imageCategory"].join(),
                startIndex,
                pageSize,
                "imageCategory,0"
            ).then(function (data) {
                if (startIndex == 1) {
                    vm.selectionImages.splice(0, vm.selectionImages.length);
                }

                if (Array.isArray(data) && data.length > 0) {
                    for (var i = 0; i < data.length; i++) {
                        var image = objectUtilService.parseServiceObject(data[i]);
                        vm.selectionImages.push(image);
                    }
                }

                return vm.selectionImages;
            });
        }

        vm.reloadalbums = function () {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "ImageCategory",
                 ["imageCategoryType", "categorySharedLevel"].join(),
                 null,
                 null,
                 null
             ).then(function (data) {
                 vm.albums.splice(0, vm.albums.length);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var album = objectUtilService.parseServiceObject(data[i]);
                         vm.albums.push(album);
                     }
                 }

                 return vm.albums
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