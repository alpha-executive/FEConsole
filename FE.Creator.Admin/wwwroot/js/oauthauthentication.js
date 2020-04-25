;(function () {
    'use strict';
    var ngapp = angular.module('ngObjectRepository');

    ngapp.factory('UserFactory', UserFactory);
    UserFactory.$inject = ['$http', 'OAuth2TokenFactory'];   

    ngapp.factory('OAuth2Interceptor', ['OAuth2TokenFactory']);
    OAuth2Interceptor.$inject = ['OAuth2TokenFactory'];

    ngapp.config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push(OAuth2Interceptor);
    }]);

    //We want to save the token for each request
    ngapp.factory('OAuth2TokenFactory', function OAuth2TokenFactory($window) {
        'use strict';
        var vm = this;
        vm.storedToken = null;

        return {
            getToken: getToken,
            setToken: setToken
        };

        function getToken() {
            return vm.storedToken;
        }

        function setToken(token) {
            if (token) {
                vm.storedToken = token;
            } else {
                vm.storedToken = null;
            }
        }
    });

    function UserFactory($http, OAuth2TokenFactory) {
        return {
            getAccessToken: getAccessToken
        };

        function getAccessToken() {
            return $http.get('/home/GetWebApiAccessToken').then(function (response) {
                OAuth2TokenFactory.setToken(response.data.token);
                return response;
            });
        }
    }

   
    function OAuth2Interceptor(OAuth2TokenFactory) {
        'use strict';
        return {
            request: addToken
        };

        function addToken(config) {
            var token = OAuth2TokenFactory.getToken();
            if (token) {
                config.headers = config.headers || {};
                config.headers.Authorization = 'Bearer ' + token;
            }
            return config;
        }
    };
})();