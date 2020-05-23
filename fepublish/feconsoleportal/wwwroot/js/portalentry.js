(function () {
    "use strict";
    var ngapp = angular.module('fePortal', ['bootstrapLightbox']);


    angular.module('fePortal').config(function (LightboxProvider) {
        // set a custom template
        LightboxProvider.templateUrl = '/home/AngularLightBoxTemplate';
    });

    ngapp.constant('API_BASEURL', baseUrl);
    ngapp.filter("fullUrl", function () {
        return function (url) {
            if (url != null) {
                return "/home/fwddownload/" + encodeURIComponent(url);
            }
        };
    }); 
})();