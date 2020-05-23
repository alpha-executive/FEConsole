;(function () {
    'use strict'
    angular.module("ngObjectRepository").filter('dateformatFilter', function () {
        return function (dateval, format) {
            var dateformatted = moment(dateval).format(format);

            return dateformatted;
        }
    });

    angular
      .module('ngObjectRepository')
        .controller("ArticleController", ArticleController);

    ArticleController.$inject = ["$scope", "$q", "$sce", "$timeout", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "Upload", "UserFactory", "API_BASEURL"];

    function ArticleController($scope, $q, $sce, $timeout, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, Upload, UserFactory, API_BASEURL) {
        var vm = this;
        vm.articles = [];
        vm.articlegroups = [];
        vm.pager = {};
        vm.pageSize = 18;
        vm.onPageClick = onPageClick;
        vm.cancelObject = null;
        vm.objectDefinitions = [];
        vm.currentArticleGroup = null;
       // vm.currentEditingArticleGroup = null;
        vm.currentEditingArticle = null;
        vm.currentViewArticle = null;
        vm.displayMode = "articleList";
        vm.currentArticleId = null;
        vm.baseUrl = API_BASEURL;
        //vm.groupDisplayMode = "groupList";

        UserFactory.getAccessToken()
              .then(function (response) {
                init();
                return response;
               });

        function init(currArticleId) {
            vm.currentArticleId = currArticleId || $scope.CurrentArticleId;

            ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
              function (data) {
                  vm.objectDefinitions = data;

                  return vm.objectDefinitions;
              }).then(function (data) {
                  vm.reloadArticleGroups();

                  //if the viewArticleID is not null, then it is a display mode, just display the article.
                  if (vm.currentArticleId != null
                      && vm.currentArticleId != "") {
                      vm.viewArticle({
                          objectID: vm.currentArticleId
                      });
                  } else {
                      onPageClick(1, true);
                  }
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
                vm.reloadArticles(pageIndex);
            });
        }
        
        function createArticle(objectName) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("Article");
            tempObj.objectName = objectName;
            var groupId = vm.currentArticleGroup == null ? 0 : vm.currentArticleGroup.objectID;
           
            objectUtilService.addStringProperty(tempObj, "articleDesc", null);
            objectUtilService.addIntegerProperty(tempObj, "isOriginal", 0);
            objectUtilService.addFileProperty(tempObj, "articleImage", null);
            objectUtilService.addStringProperty(tempObj, "articleContent", null);
            objectUtilService.addIntegerProperty(tempObj, "articleSharedLevel", 0);
            objectUtilService.addObjectRefProperty(tempObj, "articleGroup", groupId);

            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function createArticleGroup(objectName) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("ArticleGroup");
            tempObj.objectName = objectName;
            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function displayEditArticleView(article) {
            vm.currentEditingArticle = article;
            vm.cancelObject = objectUtilService.cloneJsonObject(article);
            vm.cancelObject.properties.articleContent.value = article.properties.articleContent.value;
            vm.setDisplayMode("articleEditing");
            $timeout(function () {
                initializeckEditor('articleEditor', AppLang.lang, baseUrl);
            }, 100)
        }

        function deleteGroupArticles(articles) {
            var defers = [];
            if (Array.isArray(articles) && articles.length > 0) {
                for (var i = 0; articles != null && i < articles.length; i++) {
                    var promise = function (idx) {
                        var defer = $q.defer();
                        var article = articles[idx];

                        //delete the single article instance.
                        ObjectRepositoryDataService.deleteServiceObject(article.objectID)
                        .then(function (data) {
                            defer.resolve(article);
                        });

                        return defer.promise;
                    }

                    defers.push(promise(i));
                }
            }

            return $q.all(defers);
        }

        vm.setDisplayMode = function (mode) {
            if (mode != vm.displayMode) {
                vm.displayMode = mode;
            }
        }

        vm.switchArticleGroup = function (group) {
            if (vm.currentArticleGroup != group) {
                vm.currentArticleGroup = group;
                onPageClick(1, true);
                vm.setDisplayMode("articleList");
            }
        }

        vm.viewArticle = function (article) {
            if (article.properties == null 
                || article.properties.articleContent == null
                || article.properties.articleContent.value == null) {

                //the article is not been loaded ever
                var attrs = "articleContent";
                if (article.properties == null) {
                    attrs = ["articleDesc", "articleContent", "isOriginal", "articleImage", "articleSharedLevel", "articleGroup"].join();
                }

                ObjectRepositoryDataService.getServiceObject(article.objectID, attrs)
                            .then(function (data) {
                                if (article.properties == null) {
                                    article = objectUtilService.parseServiceObject(data);
                                    article.properties.articleContent.value = $sce.trustAsHtml(article.properties.articleContent.value);
                                } else {
                                    var articleContent = objectUtilService.parseServiceObject(data);
                                    article.properties.articleContent = articleContent.properties.articleContent;
                                    article.properties.articleContent.value = $sce.trustAsHtml(article.properties.articleContent.value);
                                }

                                vm.currentViewArticle = article;
                                vm.setDisplayMode("articleView");

                                return data;
                            }).then(function (data) {
                                $timeout(function () {
                                    highlightCode("pre code");
                                }, 100);
                            });

            }else {
                vm.currentViewArticle = article;
                vm.setDisplayMode("articleView");
                $timeout(function () {
                    highlightCode("pre code");
                }, 100);
            }
        }

        vm.createOrEditArticle = function (article) {
            if (article == null) {
                var nArticle = createArticle(AppLang.POST_NEW_POST);
                displayEditArticleView(nArticle);
            }
            else {
                //get the article content.
                ObjectRepositoryDataService.getServiceObject(article.objectID, "articleContent")
                .then(function (data) {
                    var articleContent = objectUtilService.parseServiceObject(data);
                    article.properties.articleContent = articleContent.properties.articleContent;
                    article.properties.articleContent.value = $sce.trustAsHtml(article.properties.articleContent.value);
                    displayEditArticleView(article);
                });
            }
        }

        vm.uploadFiles = function (file, errFiles, objfield) {
            vm.f = file;
            vm.errFile = errFiles && errFiles[0];
            if (file) {
                file.showprogress = true;

                file.upload = Upload.upload({
                    url: vm.baseUrl + '/api/Files?thumbinal=true',
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

        vm.saveArticle = function () {
            var htmldata = CKEDITOR.instances.articleEditor.getData();
            vm.currentEditingArticle.properties.articleContent.value = htmldata;
            objectUtilService.saveServiceObject(vm.currentEditingArticle, function (data) {
                if ((vm.currentEditingArticle.objectID == null || vm.currentEditingArticle.objectID == 0)
                   && vm.articles.length >= vm.pager.pageSize) {
                    onPageChange(1);
                } else {
                    var diary = objectUtilService.parseServiceObject(data);
                    var index = vm.articles.indexOf(vm.currentEditingArticle);
                    if (index >= 0) {
                        vm.articles.splice(index, 1, diary);
                    } else {
                        vm.articles.unshift(diary);
                    }
                }

                Notification.success({
                    message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                    delay: 3000,
                    positionY: 'bottom',
                    positionX: 'right',
                    title: 'Warn',
                });
            });
        }

        vm.deleteArticle = function (article) {
            ObjectRepositoryDataService.deleteServiceObject(article.objectID)
            .then(function (data) {
                if (vm.pager.totalPages > vm.pager.currentPage
                   || (vm.articles.length <= 1 && vm.pager.totalPages == vm.pager.currentPage)) {
                    var navPageIndex = vm.articles.length - 1 <= 0
                           ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                    onPageChange(navPageIndex);
                }
                else {
                    var index = vm.articles.indexOf(article);
                    if (index >= 0) {
                        vm.articles.splice(index, 1);
                    }
                }
            });
        }

        vm.cancelArticleEditing = function () {
            var index = vm.articles.indexOf(vm.currentEditingArticle);
            if (index >= 0) {
                vm.articles.splice(index, 1, vm.cancelObject);
            }

            vm.setDisplayMode("articleList");
        }

        vm.return2List = function () {
            if (vm.currentArticleId != null) {
                onPageClick(1, true);
                vm.currentArticleId = null;
            }

            vm.setDisplayMode("articleList");
        }

        vm.createOrEditArticleGroup = function (articleGroup) {
            if (articleGroup == null) {
                vm.currentArticleGroup = createArticleGroup(AppLang.POST_NEW_GROUP);
            }
            else {
                vm.currentArticleGroup = articleGroup;
            }

            vm.cancelObject = objectUtilService.cloneJsonObject(vm.currentArticleGroup);

            //vm.groupDisplayMode = "groupEditing";
            vm.setDisplayMode("groupEditing");
        }

        vm.saveArticleGroup = function () {
            objectUtilService.saveServiceObject(vm.currentArticleGroup, function (data) {
                var group = objectUtilService.parseServiceObject(data);
                var index = vm.articlegroups.indexOf(vm.currentArticleGroup);
                if (index >= 0) {
                    vm.articlegroups.splice(index, 1, group);
                } else {
                    vm.articlegroups.push(group);
                }
                vm.setDisplayMode("articleList");
                //vm.groupDisplayMode = "groupList";

                Notification.success({
                    message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                    delay: 3000,
                    positionY: 'bottom',
                    positionX: 'right',
                    title: 'Warn',
                });
            });
        }

        vm.deleteArticleGroup = function (articleGroup) {
            //return the article group.
            if (articleGroup == null)
                return;

            ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Article",
                 "articleGroup",
                 null,
                 null,
                 "articleGroup," + articleGroup.objectID
             ).then(function (data) {
                 if (Array.isArray(data) && data.length > 0) {
                     deleteGroupArticles(data)
                     .then(function (data) {
                         ObjectRepositoryDataService.deleteServiceObject(articleGroup.objectID)
                         .then(function(data){
                             var index = vm.articlegroups.indexOf(articleGroup);
                             if (index >= 0) {
                                 vm.articlegroups.splice(index, 1);
                                 //switch to the default article group.
                                 vm.switchArticleGroup(null);
                             }
                         });
                     });
                 }
                 return data;
             });
        }

        vm.cancelArticleGroupEditing = function () {
            var index = vm.articlegroups.indexOf(vm.currentArticleGroup);
            if (index >= 0) {
                vm.articlegroups.splice(index, 1, vm.cancelObject);
            }

            vm.setDisplayMode("articleList");
            //vm.groupDisplayMode = "groupList";
        }

        vm.reCalculatePager = function (pageIndex) {
            var filter = vm.currentArticleGroup == null ? null : "articleGroup," + vm.currentArticleGroup.objectID;
            var objDefinitionId = vm.getObjectDefintionIdByName("Article");
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
        vm.reloadArticleGroups = function () {
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                "ArticleGroup",
                null,
                null,
                null,
                null
            ).then(function (data) {
                vm.articlegroups.splice(0, vm.articlegroups.length);
                if (Array.isArray(data) && data.length > 0) {
                    for (var i = 0; i < data.length; i++) {
                        var group = objectUtilService.parseServiceObject(data[i]);
                        //diary.properties.diaryContent.value = $sce.trustAsHtml(article.properties.diaryContent.value);
                        vm.articlegroups.push(group);
                    }
                }

                return vm.articlegroups;
            });
        }
        vm.reloadArticles = function (pageIndex) {
            var filter = vm.currentArticleGroup == null ? null : "articleGroup," + vm.currentArticleGroup.objectID;
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                 "Article",
                 ["articleDesc", "isOriginal", "articleImage", "articleSharedLevel", "articleGroup"].join(),
                 pageIndex,
                 vm.pageSize,
                 filter
             ).then(function (data) {
                 vm.articles.splice(0, vm.articles.length);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var article = objectUtilService.parseServiceObject(data[i]);
                         //diary.properties.diaryContent.value = $sce.trustAsHtml(article.properties.diaryContent.value);
                         vm.articles.push(article);
                     }
                 }

                 return vm.articles;
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