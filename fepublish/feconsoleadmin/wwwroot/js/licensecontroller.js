(function () {
    "use strict";

    angular
        .module('ngObjectRepository')
          .controller("LicenseController", LicenseController);

    LicenseController.$inject = ["$scope", "ObjectRepositoryDataService", "Upload", "Notification", "PagerService", "UserFactory", "API_BASEURL"];
    function LicenseController($scope, ObjectRepositoryDataService, Upload, Notification, PagerService, UserFactory, API_BASEURL) {
        var vm = this;
        vm.errorMsg = "";
        vm.moduleList = [];
        vm.baseUrl = API_BASEURL;

        UserFactory.getAccessToken()
            .then(function (response) {
                reloadLicenseList();
                return response;
            });

        //for file upload handler.
        vm.uploadFiles = function (file, errFiles) {
            vm.f = file;
            vm.errFile = errFiles && errFiles[0];
            if (file) {
                file.showprogress = true;

                file.upload = Upload.upload({
                    url: vm.baseUrl + '/api/License',
                    data: { licenseFile: file }
                });

                file.upload.then(function (response) {
                    file.result = response.data;
                    reloadLicenseList();
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

        function reloadLicenseList(){
            ObjectRepositoryDataService.getLicencedModules()
                        .then(function (data) {
                            if (data != null && Array.isArray(data)
                                && data.length > 0) {
                                vm.moduleList = data;
                            }
                        });
        }
    }
})();