/**
    data service used by the application.
**/
(function ()
{
    "use strict";

    var ngapp = angular.module('fePortal');
    ngapp.factory("ObjectRepositoryDataService", repositoryDataService);

    repositoryDataService.$inject = ["$http", "$log", "API_BASEURL"];

    function repositoryDataService($http, logger, API_BASEURL) {
        var webApiBaseUrl = API_BASEURL;
        return {
            getSharedArticles : getSharedArticles,
            getSharedBooks: getSharedBooks,
            getSharedImages: getSharedImages,
            getSharedDocuments: getSharedDocuments,
            getSharedBookCount: getSharedBookCount,
            getSharedImageCount: getSharedImageCount,
            getSharedArticleCount: getSharedArticleCount,
            getSharedDocumentCount: getSharedDocumentCount
        };
        /*==========================Shared Service Objects ========================*/
        function sendSharedObjectsRequestProxy(config, orimethod) {
            return $http(config)
             .then(complete)
             .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for ' + orimethod + ' - ' + response.data);
                return response.data;
            }
        }

        function getSharedArticles(pageIndex, pageSize) {
            if (pageIndex == null) {
                pageIndex = 1;
            }
            if (pageSize == null) {
                pageSize = Number.MAX_SAFE_INTEGER;
            }

            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/home/SharedArticles' + "?page=" + pageIndex + "&pagesize=" + pageSize,
            };

            return sendSharedObjectsRequestProxy(config, "getSharedArticles");
        }


        function getSharedBooks(pageIndex, pageSize) {
            if (pageIndex == null) {
                pageIndex = 1;
            }
            if (pageSize == null) {
                pageSize = Number.MAX_SAFE_INTEGER;
            }

            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/home/SharedBooks' + "?page=" + pageIndex + "&pagesize=" + pageSize,
            };

            return sendSharedObjectsRequestProxy(config, "SharedBooks");
        }

        function getSharedImages(pageIndex, pageSize) {
            if (pageIndex == null) {
                pageIndex = 1;
            }
            if (pageSize == null) {
                pageSize = Number.MAX_SAFE_INTEGER;
            }

            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/home/SharedImages' + "?page=" + pageIndex + "&pagesize=" + pageSize,
            };

            return sendSharedObjectsRequestProxy(config, "SharedImages");
        }

        function getSharedDocuments(pageIndex, pageSize) {
            if (pageIndex == null) {
                pageIndex = 1;
            }
            if (pageSize == null) {
                pageSize = Number.MAX_SAFE_INTEGER;
            }

            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/home/SharedDocuments' + "?page=" + pageIndex + "&pagesize=" + pageSize,
            };

            return sendSharedObjectsRequestProxy(config, "getSharedDocuments");
        }
        
        function getSharedBookCount() {
            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/home/GetSharedBookCount'
            };

            return sendSharedObjectsRequestProxy(config, "getSharedBookCount");
        }

        function getSharedArticleCount() {
            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/home/GetSharedArticleCount'
            };

            return sendSharedObjectsRequestProxy(config, "getSharedArticleCount");
        }

        function getSharedDocumentCount() {
            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/home/GetSharedDocumentCount'
            };

            return sendSharedObjectsRequestProxy(config, "getSharedDocumentCount");
        }

        function getSharedImageCount() {
            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/home/GetSharedImageCount'
            };

            return sendSharedObjectsRequestProxy(config, "getSharedImageCount");
        }
        /*==========================Service Objects ===============================*/
    }

})();