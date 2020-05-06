;(function () {
    "use strict";

    angular
        .module('fePortal')
            .filter("checkFileType", function () {
        return function (file) {
            switch (file.fileExtension.toLowerCase()) {
                case ".docx":
                case ".doc":
                    return "fa-file-word-o";
                case ".pdf":
                    return 'fa-file-pdf-o';
                case ".txt":
                    return 'fa-file-text-o';
                case ".ppt":
                case ".pptx":
                    return 'fa-file-powerpoint-o';
                case ".xlsx":
                case ".xls":
                    return 'fa-file-excel-o';
                case ".zip":
                case ".7z":
                case ".tar":
                case ".gz":
                    return 'fa-file-archive-o';
                case ".png":
                case ".jpg":
                case ".bmp":
                case ".gif":
                case ".svg":
                case ".tif":
                    return 'fa-file-image-o';
                case ".mp3":
                case ".wav":
                case ".mp4":
                case ".mp4a":
                    return "fa-file-audio-o";
                case ".webm":
                case ".mkv":
                case ".avi":
                case ".wma":
                case ".rm":
                case ".rmvb":
                case ".3gp":
                case ".3g2":
                case ".m4v":
                    return "fa-file-movie-o"
                default:
                    return 'fa-file-o';
            }
        };
    });

    angular
        .module('fePortal')
          .controller("SharedBooksController", SharedBooksController);

    angular
       .module('fePortal')
         .controller("SharedArticlesController", SharedArticlesController);

    angular
       .module('fePortal')
         .controller("SharedImagesController", SharedImagesController);

    angular
        .module("fePortal")
            .controller("ShareDocumentsController", ShareDocumentsController);

    


    SharedBooksController.$inject = ["$scope", "ObjectRepositoryDataService", "PagerService", "objectUtilService"];
    SharedArticlesController.$inject = ["$scope", "ObjectRepositoryDataService","PagerService", "objectUtilService"];
    SharedImagesController.$inject = ["$scope", "ObjectRepositoryDataService", "PagerService", "objectUtilService", "Lightbox"];

    //books
    function SharedBooksController($scope, ObjectRepositoryDataService, PagerService, objectUtilService) {
        var vm = this;

        vm.pager = {};  //for page purpose.
        vm.onPageClick = onPageClick;
        vm.pageSize = bookListPageSize || 5;
        vm.books = [];

        vm.reCalculatePager = function (pageIndex) {
            return ObjectRepositoryDataService.getSharedBookCount()
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

        vm.reloadBooks = function () {
            ObjectRepositoryDataService.getSharedBooks(
                 vm.pager.currentPage,
                 vm.pageSize
            ).then(function (txtdata) {
                var data = JSON.parse(txtdata);
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

        init();

        function init() {
            onPageChange(1);
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
       
    }

    //articles (posts)
    function SharedArticlesController($scope, ObjectRepositoryDataService, PagerService, objectUtilService) {
        var vm = this;

        vm.pager = {};  //for page purpose.
        vm.onPageClick = onPageClick;
        vm.pageSize = articleListPageSize || 5;
        vm.articles = [];


        vm.reCalculatePager = function (pageIndex) {
            return ObjectRepositoryDataService.getSharedArticleCount()
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

        vm.reloadArticles = function () {
            ObjectRepositoryDataService.getSharedArticles(
                 vm.pager.currentPage,
                 vm.pageSize
             ).then(function (txtdata) {
                 vm.articles.splice(0, vm.articles.length);
                 var data = JSON.parse(txtdata);
                 if (Array.isArray(data) && data.length > 0) {
                     for (var i = 0; i < data.length; i++) {
                         var article = objectUtilService.parseServiceObject(data[i]);
                         vm.articles.push(article);
                     }
                 }

                 return vm.articles;
             });
        }

        init();

        function init() {
            onPageChange(1);
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

                vm.reloadArticles();
            });
        }
    }

    //images.
    function SharedImagesController($scope, ObjectRepositoryDataService, PagerService, objectUtilService, Lightbox) {
        var vm = this;

        vm.pager = {};  //for page purpose.
        vm.onPageClick = onPageClick;
        vm.pageSize = imageListPageSize || 6;
        vm.images = [];

        vm.reCalculatePager = function (pageIndex) {
            return ObjectRepositoryDataService.getSharedImageCount()
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

        vm.showLightboxImage = function (index) {
            Lightbox.openModal(vm.images, index);
        }

        vm.reloadImages = function () {
            ObjectRepositoryDataService.getSharedImages(
                 vm.pager.currentPage,
                 vm.pageSize
            ).then(function (txtdata) {
                 var data = JSON.parse(txtdata);
                 vm.images.splice(0, vm.images.length);
                if (Array.isArray(data) && data.length > 0) {
                    var displayImagesCount = data.length < vm.pageSize ? data.length : vm.pageSize;
                    for (var i = 0; i < displayImagesCount; i++) {
                         var image = objectUtilService.parseServiceObject(data[i]);

                         vm.images.push({
                             url: '/home/DownloadSharedImage/' + image.objectID,
                             thumbUrl: '/home/DownloadSharedImage/' + image.objectID + '?thumbinal=true',
                             caption: image.properties.imageDesc.value
                         });
                     }
                 }

                 return vm.images;
             });
        }

        init();

        function init() {
            onPageChange(1);
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

                vm.reloadImages();
            });
        }

    }

    //Documents.
    function ShareDocumentsController($scope, ObjectRepositoryDataService, PagerService, objectUtilService) {
        var vm = this;

        vm.pager = {};  //for page purpose.
        vm.onPageClick = onPageClick;
        vm.pageSize = fileListPageSize || 5;
        vm.documents = [];

        vm.reCalculatePager = function (pageIndex) {
            return ObjectRepositoryDataService.getSharedDocumentCount()
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

        vm.reloadDocuments = function () {
            ObjectRepositoryDataService.getSharedDocuments(
                 vm.pager.currentPage,
                 vm.pageSize
            ).then(function (txtdata) {
                var data = JSON.parse(txtdata);
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

        init();

        function init() {
            onPageChange(1);
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

                vm.reloadDocuments();
            });
        }

    }
})();