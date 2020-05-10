; (function () {
    'use strict'
    angular
     .module('ngObjectRepository')
       .controller("DashboardController", DashboardController);

    DashboardController.$inject = ["$scope", "$filter", "ObjectRepositoryDataService", "objectUtilService", "UserFactory", "Lightbox"];
    function DashboardController($scope, $filter, ObjectRepositoryDataService, objectUtilService, UserFactory, Lightbox) {
        var vm = this;
        vm.objectDefinitions = [];
        vm.totalPhotosCount = 0;
        vm.totalPostCount = 0;
        vm.totalBooksCount = 0;
        vm.totalContactsCount = 0;
        vm.bookPageSize = 5;
        vm.postPageSize = 5;
        vm.imagePageSize = 6;

        vm.books = [];
        vm.images = [];
        vm.posts = [];

        UserFactory.getAccessToken()
            .then(function (response) {
                init();
                return response;
            });

        function init() {
            ObjectRepositoryDataService
                .getLightWeightObjectDefinitions()
                .then(
                    function (data) {
                        vm.objectDefinitions = data;
                        return vm.objectDefinitions;
                    }).then(function (data) {
                        getKeyObjectsCounts("Article");
                        getKeyObjectsCounts("Photos");
                        getKeyObjectsCounts("Books");
                        getKeyObjectsCounts("GeneralContact");

                        vm.reloadBooks();
                        vm.reloadPosts();
                        vm.reloadImages();

                        return data;
                    });
        }

        function getKeyObjectsCounts(objectName) {
            var objDefinitionId = vm.getObjectDefintionIdByName(objectName);
            ObjectRepositoryDataService.getServiceObjectCount(objDefinitionId).then(function (data) {
                if (!isNaN(data)) {
                    switch (objectName) {
                        case "Article":
                            vm.totalPostCount = data;
                            break;
                        case "Photos":
                            vm.totalPhotosCount = data;
                            break;
                        case "Books":
                            vm.totalBooksCount = data;
                            break;
                        case "GeneralContact":
                            vm.totalContactsCount = data;
                            break;
                        default:
                            break;
                    }
                }
            });
        }
        vm.reloadPosts = function () {
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Article",
                 ["articleDesc", "isOriginal", "articleImage", "articleSharedLevel"].join(),
                 null,
                 vm.postPageSize,
                 null
             ).then(function (data) {
                 vm.posts.splice(0, vm.posts.length);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var post = objectUtilService.parseServiceObject(data[i]);
                         //diary.properties.diaryContent.value = $sce.trustAsHtml(article.properties.diaryContent.value);
                         vm.posts.push(post);
                     }
                 }

                 return vm.posts;
             });
        }

        vm.reloadBooks = function () {
            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Books",
                 ["bookFile", "bookDesc", "bookAuthor", "bookVersion", "bookSharedLevel","bookISBN"].join(),
                 null,
                 vm.bookPageSize,
                 null
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

        vm.viewImageLightBox = function (index) {
            Lightbox.openModal(vm.images, index);
        }

        vm.reloadImages = function () {
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Photos",
                 ["imageDesc", "imageFile"].join(),
                 1,
                 vm.imagePageSize,
                 null
             ).then(function (data) {
                 vm.images.splice(0, vm.images.length);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var image = objectUtilService.parseServiceObject(data[i]);
                         vm.images.push({
                             objectID: image.objectID,
                             url: $filter('fullUrl')(image.properties.imageFile.fileUrl),
                             caption: image.properties.imageDesc.value,
                             objectName: image.objectName
                         });
                     }
                 }
                 return vm.images;
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
