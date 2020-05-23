(function () {
    "use strict";

    angular
        .module('ngObjectRepository')
          .controller("BooksController", BooksController);

    BooksController.$inject = ["$scope", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "Upload", "UserFactory", "API_BASEURL"];

    function BooksController($scope, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, Upload, UserFactory, API_BASEURL) {
        var vm = this;
        vm.currentBookCategory = null;
        vm.categoryEditButtonLabel = AppLang.BOOK_BTN_NEW_CATEGORY;
        vm.searchText = "";
        vm.books = [];
        vm.totalBookNumber = 0;
        vm.bookCategories = [];
        vm.currentEditingBook = null;
        vm.objectDefinitions = [];
        vm.categoryEditMode = "list";
        vm.displayMode = "list";
        vm.cancelObject = {};

        vm.pager = {};  //for page purpose.
        vm.onPageClick = onPageClick;
        vm.pageSize = 25;
        vm.baseUrl = API_BASEURL;

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
                    //get all the categories and books.
                    vm.reloadBookCategories();
                    onPageChange(1);
                    vm.reCalculateBookNumbers();
                });
        }

        function onPageClick(pageIndex) {
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

                vm.reloadBooks();
            });
        }

        function createNewBookCategoryObject(objectName) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("BookCategory");
            tempObj.objectName = objectName;
            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function createNewBookObject(objectName, bookCategory) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("Books");
            tempObj.objectName = objectName;
            var categoryId = bookCategory == null ? 0 : bookCategory.objectID;

            objectUtilService.addFileProperty(tempObj, "bookFile", null);
            objectUtilService.addStringProperty(tempObj, "bookDesc", null);
            objectUtilService.addStringProperty(tempObj, "bookAuthor", null);
            objectUtilService.addStringProperty(tempObj, "bookVersion", null);
            objectUtilService.addObjectRefProperty(tempObj, "bookCategory", categoryId);
            objectUtilService.addIntegerProperty(tempObj, "bookSharedLevel", 0);
            objectUtilService.addStringProperty(tempObj, "bookISBN", null);

            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function deleteCatetoryItems(catBooks, category) {
            if (catBooks != null && catBooks.length > 0) {
                var f = catBooks.pop();
                if (f.objectID != 0 && f.objectID != null) {
                    ObjectRepositoryDataService
                        .deleteServiceObject(f.objectID)
                        .then(function (data) {
                            //delete the next item in this category.
                            deleteCatetoryItems(catBooks, category);

                            return data;
                        })
                        .then(function(data){
                            if (catBooks.length == 0 || catBooks.length == 0) {
                                //delete category finally.
                                deleteCategoryObject(category);
                            }
                        });
                }
            }
            else {
                deleteCategoryObject(category);
            }
        }
        function deleteCategoryObject(category) {
            if (category.objectID != null && category.objectID != 0) {
                ObjectRepositoryDataService
                    .deleteServiceObject(category.objectID)
                    .then(function (data) {
                        //delete the directory object from directory list.
                        var index = vm.bookCategories.indexOf(category);
                        if (index >= 0) {
                            vm.bookCategories.splice(index, 1);
                        }

                        //swtich to the default category.
                        if (category != null && category.objectID != 0 && category.objectID != null) {
                            //switch to the original category.
                            vm.switchCategory(null);
                        }

                        return data;
                    })
                    .then(function (data) {
                        vm.reCalculateBookNumbers();
                    });
            }
        }

        vm.bookEditing = function (book) {
            vm.displayMode = "editing";

            vm.currentEditingBook = book;
            if (vm.currentEditingBook == null) {
                var tempObj = createNewBookObject(AppLang.BOOK_LBL_NEW_BOOK, vm.currentBookCategory);
                vm.books.unshift(tempObj);
                vm.currentEditingBook = tempObj;
            }

            vm.cancelObject = objectUtilService.cloneJsonObject(vm.currentEditingBook);
        }

        vm.bookDelete = function (book) {
            if (book.objectID != 0 && book.objectID != null) {
                ObjectRepositoryDataService.deleteServiceObject(book.objectID)
                .then(function (data) {
                    //recalculate the current page when a item is deleted in a full page
                    //escape the first page, it's no need to reload the first page.
                    if (vm.pager.totalPages >= vm.pager.currentPage) {
                        var navPageIndex = vm.books.length - 1 <= 0
                               ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                        onPageChange(navPageIndex);
                    }
                    else {
                            var index = vm.books.indexOf(book);
                            if (index >= 0) {
                                vm.books.splice(index, 1);
                            }
                    }

                    vm.reCalculateBookNumbers();
                });
            }
        }

        vm.switchCategory = function (category) {
            if (vm.currentBookCategory != category) {
                vm.currentBookCategory = category;
                onPageChange(1);
            }
        }

        vm.categoryEditing = function (category) {
            //it's a new create directory
            if (category == null) {
                var tempObj = createNewBookCategoryObject(AppLang.BOOK_LBL_NEWCATEGORY);
                vm.currentBookCategory = tempObj;
                vm.bookCategories.push(tempObj);
            }
            else {
                vm.currentBookCategory = category;
            }

            vm.categoryEditMode = "edit";
            vm.cancelObject = objectUtilService.cloneJsonObject(vm.currentBookCategory);
        }

        vm.deleteCategory = function (category) {
            //delete the books under this category
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Books",
                 "bookCategory",
                 null,
                 null,
                 "bookCategory," + category.objectID
             ).then(function (data) {
                 if (Array.isArray(data)) {
                     //recurvely delete the category.
                     deleteCatetoryItems(data, category);
                 }
             });
        }

        vm.cancelCategoryEditing = function (category) {
            vm.categoryEditMode = "list";

            if (category == null)
                return;

            var index = vm.bookCategories.indexOf(category);
            if (index >= 0) {
                if (category.objectID == null || category.objectID == 0) {
                    vm.bookCategories.splice(index, 1);
                } else {
                    vm.bookCategories.splice(index, 1, vm.cancelObject);
                }
            }
        }

        vm.saveBookCategory = function () {
            if (vm.currentBookCategory == null)
                return;

            objectUtilService.saveServiceObject(vm.currentBookCategory, function (data) {
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
                        var tmpCategory = objectUtilService.parseServiceObject(data);
                        var index = vm.bookCategories.indexOf(vm.currentBookCategory);
                        if (index >= 0) {
                            vm.bookCategories.splice(index, 1, tmpCategory);
                        }
                        
                        //switch to the new added category.
                        if (vm.currentBookCategory.objectID == 0 || vm.currentBookCategory.objectID == null) {
                            vm.switchCategory(tmpCategory);
                        }

                        vm.categoryEditMode = "list";
                    }
                }
                else {
                    //something error happend.
                    Notification.error({
                        message: AppLang.COMMON_EDIT_SAVE_FAILED + data.toString(),
                        delay: 5000,
                        positionY: 'bottom',
                        positionX: 'right',
                        title: 'Error'
                    });
                }
            });
        }

        //for file upload handler.
        vm.uploadFiles = function (file, errFiles, bookobj) {
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
                        bookobj.objectName = objectUtilService.cloneJsonObject(file.result.files[0].fileName);
                        bookobj.properties.bookFile.fileName = file.result.files[0].fileName;
                        bookobj.properties.bookFile.fileUrl = file.result.files[0].fileUrl;
                        bookobj.properties.bookFile.fileCRC = file.result.files[0].fileCRC;
                        bookobj.properties.bookFile.fileExtension = file.result.files[0].fileExtension;
                        bookobj.properties.bookFile.created = file.result.files[0].created;
                        bookobj.properties.bookFile.updated = file.result.files[0].updated;
                        bookobj.properties.bookFile.freated = file.result.files[0].created;
                        bookobj.properties.bookFile.fileSize = file.result.files[0].fileSize;
                        bookobj.properties.bookFile.fileFullPath = file.result.files[0].fileFullPath;
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

        vm.saveBook = function () {
            objectUtilService.saveServiceObject(vm.currentEditingBook, function (data) {
                if (data == null || data == "" || data.objectID != null) {
                    //update the object id.
                    if (data != null && data != "") {
                        var tmpbook = objectUtilService.parseServiceObject(data);

                        //if it's a new add operation.
                        if ((vm.currentEditingBook.objectID == null || vm.currentEditingBook.objectID == 0)
                            && vm.books.length >= vm.pager.pageSize) {
                            //redirect to the first page.
                            onPageChange(1);
                        }
                        else {
                            //just update the saved changes,no need to refresh from server.
                            var index = vm.books.indexOf(vm.currentEditingBook);
                            if (index >= 0) {
                                vm.books.splice(index, 1, tmpbook);
                            }
                        }

                        vm.displayMode = "list";
                        vm.reCalculateBookNumbers();

                        Notification.success({
                            message: 'Change Saved!',
                            delay: 3000,
                            positionY: 'bottom',
                            positionX: 'right',
                            title: 'Warn',
                        });
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
        vm.cancelBookEditing = function () {
            var index = vm.books.indexOf(vm.currentEditingBook);
            if (index >= 0) {
                vm.cancelObject.objectID == null || vm.cancelObject.objectID == 0 ? vm.books.splice(index, 1) :
                        vm.books.splice(index, 1, vm.cancelObject);
            }

            vm.displayMode = "list";
        }

        vm.reloadBookCategories = function () {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                  "BookCategory",
                  null,
                  null,
                  null,
                  null
              ).then(function (data) {
                  vm.bookCategories.splice(0, vm.bookCategories.length);
                  if (Array.isArray(data) && data.length > 0) {

                      for (var i = 0; i < data.length; i++) {
                          var bookCategory = objectUtilService.parseServiceObject(data[i]);
                          vm.bookCategories.unshift(bookCategory);
                      }
                  }

                  return vm.bookCategories;
              });
        }

        vm.reloadBooks = function () {
            var categoryFilters = vm.currentBookCategory != null ?
                "bookCategory," + vm.currentBookCategory.objectID : null;

            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Books",
                 ["bookFile", "bookDesc", "bookAuthor", "bookVersion", "bookSharedLevel", "bookCategory", "bookISBN"].join(),
                 vm.pager.currentPage,
                 vm.pageSize,
                 categoryFilters
             ).then(function (data) {
                 vm.books.splice(0, vm.books.length);
                 if (Array.isArray(data) && data.length > 0) {

                     for (var i = 0; i < data.length; i++) {
                         var book = objectUtilService.parseServiceObject(data[i]);
                         vm.books.push(book);
                     }
                 }

                 return vm.books;
             });
        }

        vm.reCalculateBookNumbers = function () {
            var objDefinitionId = vm.getObjectDefintionIdByName("Books");
            ObjectRepositoryDataService.getServiceObjectCount(objDefinitionId).then(function (data) {
                if (!isNaN(data)) {
                    vm.totalBookNumber = data;
                }
            });
        }

        vm.reCalculatePager = function (pageIndex) {
            var objDefinitionId = vm.getObjectDefintionIdByName("Books");
            var filter = vm.currentBookCategory != null ? "bookCategory," + vm.currentBookCategory.objectID : null;

            return ObjectRepositoryDataService.getServiceObjectCount(
                    objDefinitionId,
                    filter
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