/**
    data service used by the application.
**/
(function ()
{
    "use strict";
    angular
        .module('ngObjectRepository')
        .factory("ObjectRepositoryDataService", repositoryDataService);

    repositoryDataService.$inject = ["$http", "$log", "API_BASEURL"];

    function repositoryDataService($http, logger, API_BASEURL) {
        var webApiBaseUrl = API_BASEURL;
        return {
            getObjectDefinitionGroups: getObjectDefinitionGroups,
            getObjectDefinitionGroup: getObjectDefinitionGroup,
            createOrUpdateDefinitionGroup: createOrUpdateDefinitionGroup,
            deleteDefinitionGroup: deleteDefinitionGroup,
            getObjectDefintionsbyGroup: getObjectDefintionsbyGroup,
            getObjectDefinitionById: getObjectDefinitionById,
            getLightWeightObjectDefinitions: getLightWeightObjectDefinitions,
            getCustomObjectDefinitions: getCustomObjectDefinitions,
            createOrUpdateObjectDefintion: createOrUpdateObjectDefintion,
            deleteObjectDefintionField: deleteObjectDefintionField,
            deleteObjectDefintion: deleteObjectDefintion,
            deleteSingleSelectionFieldItem: deleteSingleSelectionFieldItem,
            getServiceObjects: getServiceObjects,
            getServiceObject: getServiceObject,
            getServiceObjectCount: getServiceObjectCount,
            getServiceObjectsWithFilters: getServiceObjectsWithFilters,
            getSysObjectsByFilters: getSysObjectsByFilters,
            getSharedArticles : getSharedArticles,
            getSharedBooks: getSharedBooks,
            getSharedImages: getSharedImages,
            getSharedDocuments: getSharedDocuments,
            getSharedBookCount: getSharedBookCount,
            getSharedImageCount: getSharedImageCount,
            getSharedArticleCount: getSharedArticleCount,
            getSharedDocumentCount: getSharedDocumentCount,
            createOrUpdateServiceObject: createOrUpdateServiceObject,
            deleteServiceObject: deleteServiceObject,
            getUsers: getUsers,
            getAdminLoginName: getAdminLoginName,
            getUserIdByLoginName : getUserIdByLoginName,
            resetPassword: resetPassword,
            getLicencedModules: getLicencedModules,
            getUUID: getUUID,
            encryptData: encryptData,
            decryptData: decryptData
        };

        //get the object defintion groups
        function getObjectDefinitionGroups(parentGroupId) {

            var url = webApiBaseUrl + "/api/ObjectDefinitionGroup/GetByParentId";

            if (parentGroupId != null)
                url = url + "/" + parentGroupId;

            return $http.get(url)
                .then(complete)
                .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response)
            {
                logger.error('XHR Failed for getObjectDefinitionGroups - ' + response.data);
                return response.data;
            }
        }

        //get the object defintion group by id.
        function getObjectDefinitionGroup(id){
            return $http.get(webApiBaseUrl + "/api/ObjectDefinitionGroup/"+ id)
               .then(complete)
               .catch(error);


           function complete(response) {
               return response.data;
           }

           function error(response) {
               logger.error('XHR Failed for getObjectDefinitionGroup - ' + response.data);
               return response.data;
           }
        }

        //create or update
        function createOrUpdateDefinitionGroup(id, grpdata) {
            //create
            if (id == null) {
                return $http.post(webApiBaseUrl + "/api/ObjectDefinitionGroup", grpdata)
                            .then(complete)
                            .catch(error);
            }//update
            else {
                return $http.put(webApiBaseUrl + "/api/ObjectDefinitionGroup/" + id, grpdata)
                            .then(complete)
                            .catch(error);
            }

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for createOrUpdateDefinitionGroup - ' + response.data);
                return response.data;
            }
        }

        //delete
        function deleteDefinitionGroup(id)
        {
            return $http.delete(webApiBaseUrl + "/api/ObjectDefinitionGroup/" + id)
                            .then(complete) 
                            .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for deleteDefinitionGroup - ' + response.data);
                return response.data;
            }
        }

        //===============================================Object Defintion API ================
        function getObjectDefintionsbyGroup(groupId) {
            var url = webApiBaseUrl + "/api/ObjectDefinition/FindObjectDefintionsByGroup";

            if (groupId != null)
                url = url + "/" + groupId;

            return $http.get(url)
                .then(complete)
                .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getObjectDefintionsbyGroup - ' + response.data);
                return response.data;
            }
        }

        function getObjectDefinitionById(objectId) {
            return $http.get(webApiBaseUrl + '/api/ObjectDefinition/FindObjectDefinition/' + objectId)
                .then(complete)
                .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getObjectDefintionsbyGroup - ' + response.data);
                return response.data;
            }
        }

        function getLightWeightObjectDefinitions() {
            return $http.get(webApiBaseUrl + '/api/ObjectDefinition/getSystemObjectDefinitions')
                .then(complete)
                .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getLightWeightObjectDefinitions - ' + response.data);
                return response.data;
            }
        }

        function getCustomObjectDefinitions() {
            return $http.get(webApiBaseUrl + '/api/ObjectDefinition/getCustomObjectDefinitions')
            .then(complete)
            .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getCustomObjectDefinitions - ' + response.data);
                return response.data;
            }
        }

        function createOrUpdateObjectDefintion(id, objdefdata) {
            //create
            if (id == null) {
                return $http.post(webApiBaseUrl + "/api/ObjectDefinition", 
                       objdefdata)
                    .then(complete)
                    .catch(error);
            }//update
            else {
                return $http.put(webApiBaseUrl + "/api/ObjectDefinition/" + id, objdefdata)
                            .then(complete)
                            .catch(error);
            }

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for createOrUpdateObjectDefintion - ' + response.data);
                return response.data;
            }
        }

        function deleteObjectDefintionField(id) {
            return $http.delete(webApiBaseUrl + "/api/ObjectDefinitionField/" + id)
                           .catch(error);

            function error(response) {
                logger.error('XHR Failed for deleteObjectDefintionField - ' + response.data);
                return response.data;
            }
        }

        function deleteObjectDefintion(id) {
            return $http.delete(webApiBaseUrl + "/api/ObjectDefinition/" + id)
                          .catch(error);

            function error(response) {
                logger.error('XHR Failed for deleteObjectDefintion - ' + response.data);
                return response.data;
            }
        }

        function deleteSingleSelectionFieldItem(id) {
            return $http.delete(webApiBaseUrl + "/api/SingleSelectionFieldItem/" + id)
                         .catch(error);

            function error(response) {
                logger.error('XHR Failed for deleteSingleSelectionFieldItem - ' + response.data);

                return response.data;
            }
        }
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
                url: webApiBaseUrl + '/api/SharedObjects/SharedArticles' + "?page=" + pageIndex + "&pagesize=" + pageSize,
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
                url: webApiBaseUrl + '/api/SharedObjects/SharedBooks' + "?page=" + pageIndex + "&pagesize=" + pageSize,
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
                url: webApiBaseUrl + '/api/SharedObjects/SharedImages' + "?page=" + pageIndex + "&pagesize=" + pageSize,
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
                url: webApiBaseUrl + '/api/SharedObjects/SharedDocuments' + "?page=" + pageIndex + "&pagesize=" + pageSize,
            };

            return sendSharedObjectsRequestProxy(config, "getSharedDocuments");
        }
        
        function getSharedBookCount() {
            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/api/SharedObjects/GetSharedBookCount'
            };

            return sendSharedObjectsRequestProxy(config, "getSharedBookCount");
        }

        function getSharedArticleCount() {
            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/api/SharedObjects/GetSharedArticleCount'
            };

            return sendSharedObjectsRequestProxy(config, "getSharedArticleCount");
        }

        function getSharedDocumentCount() {
            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/api/SharedObjects/GetSharedDocumentCount'
            };

            return sendSharedObjectsRequestProxy(config, "getSharedDocumentCount");
        }

        function getSharedImageCount() {
            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/api/SharedObjects/GetSharedImageCount'
            };

            return sendSharedObjectsRequestProxy(config, "getSharedImageCount");
        }
        /*==========================Service Objects ===============================*/
        function getServiceObjects(objectDefintionId, properties, pageIndex, pageSize) {
            if (pageIndex == null)
            {
                pageIndex = 1;
            }
            if (pageSize == null) {
                pageSize = Number.MAX_SAFE_INTEGER;
            }

            var config = {
                method: 'GET',
                url: webApiBaseUrl + '/api/GeneralObject/FindServiceObjects/' + objectDefintionId + "/" + properties + "?pageIndex="+pageIndex + "&pageSize=" + pageSize,
            };

            return $http(config)
              .then(complete)
              .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getServiceObjects - ' + response.data);
                return response.data;
            }
        }

        function getServiceObject(id, properties) {
            var url = properties != null ? '/api/GeneralObject/FindServiceObject/' + id + "/" + properties
                : '/api/GeneralObject/FindServiceObject/' + id;
            url = webApiBaseUrl + url;

            var config = {
                method: 'GET',
                url: url
            };

            return $http(config)
              .then(complete)
              .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getServiceObject - ' + response.data);
                return response.data;
            }
        }

        function getServiceObjectCount(id, filter) {
            var reqUrl = filter != null ? '/api/GeneralObject/CountObjectsById/' + id + "/" + filter
                : '/api/GeneralObject/CountObjectsById/' + id;
            reqUrl = webApiBaseUrl + reqUrl;

            var config = {
                method: 'GET',
                url: reqUrl
            };

            return $http(config)
              .then(complete)
              .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getServiceObjectCount - ' + response.data);
                return response.data;
            }
        }

        function sendServiceObjectFiltersRequest(rawurl, definitionName, parameters, pageIndex, pageSize, filters) {
            var url = rawurl + definitionName;
            if (parameters != null)
                url = url + '/' + parameters + "?";
            else
                url = url + "?";

            if (pageIndex != null)
                url = url + "&pageIndex=" + pageIndex;

            if (pageSize != null)
                url = url + "&pageSize=" + pageSize;

            if (filters != null)
                url = url + "&filters=" + filters;

            var config = {
                method: 'GET',
                url: url
            };

            return $http(config)
              .then(complete)
              .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for sendServiceObjectFiltersRequest - ' + response.data);
                return response.data;
            }
        }

        function getServiceObjectsWithFilters(definitionName, parameters, pageIndex, pageSize, filters) {
            var url = webApiBaseUrl + '/api/GeneralObject/FindServiceObjectsByFilter/';
            return sendServiceObjectFiltersRequest(url,
                definitionName,
                parameters,
                pageIndex,
                pageSize,
                filters);
        }

        
        function getSysObjectsByFilters(definitionName, parameters, pageIndex, pageSize, filters) {
            var url = webApiBaseUrl + '/api/GeneralObject/FindSysObjectsByFilter/';
            return sendServiceObjectFiltersRequest(url,
                definitionName,
                parameters,
                pageIndex,
                pageSize,
                filters);
        }

        function createOrUpdateServiceObject(id, svcObject) {
            //create
            if (id == null) {
                return $http.post(webApiBaseUrl + "/api/GeneralObject",
                       svcObject)
                    .then(complete)
                    .catch(error);
            }//update
            else {
                return $http.put(webApiBaseUrl + "/api/GeneralObject/" + id, svcObject)
                            .then(complete)
                            .catch(error);
            }

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for createOrUpdateServiceObject - ' + response.data);
                return response.data;
            }
        }

        function deleteServiceObject(objectid) {
            return $http.delete(webApiBaseUrl + "/api/GeneralObject/" + objectid)
                         .catch(error);

            function error(response) {
                logger.error('XHR Failed for deleteServiceObject - ' + response.data);

                return response.data;
            }
        }

        /*================User Service ==========================*/
        function getUsers() {
            return $http.get(webApiBaseUrl + '/api/SystemUser/List')
                .then(complete)
                .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getUsers - ' + response.data);
                return response.data;
            }
        }

        function getAdminLoginName() {
            return $http.get(webApiBaseUrl + '/api/SystemUser/GetAdminLoginName')
                .then(complete)
                .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getAdminLoginName - ' + response.data);
                return response.data;
            }
        }

        function getUserIdByLoginName() {
            return $http.get(webApiBaseUrl + '/api/SystemUser/GetUserIdByUserLoginName')
               .then(complete)
               .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getUserIdByLoginName - ' + response.data);
                return response.data;
            }
        }
        function resetPassword(userid) {
            return $http.post(webApiBaseUrl + "/api/SystemUser/ResetUserPassword/" + userid, userid)
                    .then(complete)
                    .catch(error);
             
            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for resetPassword - ' + response.data);
                return response.data;
            }
        }

        function getLicencedModules() {
            return $http.get(webApiBaseUrl + '/api/License/RegisterList')
              .then(complete)
              .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getLicencedModules - ' + response.data);
                return response.data;
            }
        }

        function getUUID() {
            return $http.get(webApiBaseUrl + '/api/UniqueIDGenerator')
                          .then(complete)
                          .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for getLicencedModules - ' + response.data);
                return response.data;
            }
        }

        function encryptData(data) {
            return $http.post(webApiBaseUrl + "/api/Cryptography/EncryptData",
                     data)
                  .then(complete)
                  .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for encryptData - ' + response.data);
                return response.data;
            }
        }

        function decryptData(data) {
            return $http.post(webApiBaseUrl + "/api/Cryptography/DecryptData",
                     data)
                  .then(complete)
                  .catch(error);

            function complete(response) {
                return response.data;
            }

            function error(response) {
                logger.error('XHR Failed for decryptData - ' + response.data);
                return response.data;
            }
        }
    }

})();