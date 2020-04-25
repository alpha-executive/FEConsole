(function () {
    "use strict";

    //for object defintion group
    var ngapp = angular.module('ngObjectRepository', ['ngRoute', "ngSanitize", 'ngMessages', 'ui-notification', 'ui.select', 'ngFileUpload']);
    ngapp.constant('API_BASEURL', baseUrl);
    ngapp.directive('convertToNumber', function () {
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                ngModel.$parsers.push(function (val) {
                    return parseInt(val, 10);
                });
                ngModel.$formatters.push(function (val) {
                    return '' + val;
                });
            }
        };
    });

    ngapp.filter('dateformatFilter', function () {
        return function (dateval, format) {
            var dateformatted = moment(dateval).format(format);

            return dateformatted;
        }
    });

    ngapp.directive('autoFocus', function ($timeout) {
        return {
            restrict: 'A',
            link: function (_scope, _element) {
                $timeout(function () {
                    _element[0].focus();
                }, 0);
            }
        };
    });

    ngapp.filter("checkFileType", function () {
        return function (file) {
            if (!file ||
                !file.fileExtension) {
                return 'fa-file-o';
            }
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
                case ".jpeg":
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

    ngapp.directive('starRating', function () {
        return {
            restrict: 'EA',
            template:
              '<ul class="star-rating" ng-class="{readonly: readonly}">' +
              '  <li ng-repeat="star in stars" class="star" ng-class="{filled: star.filled}" ng-click="toggle($index)">' +
              '    <i class="fa fa-star"></i>' + // or &#9733
              '  </li>' +
              '</ul>',
            scope: {
                ratingValue: '=ngModel',
                max: '=?', // optional (default is 5)
                onRatingSelect: '&?',
                readonly: '=?'
            },
            link: function (scope, element, attributes) {
                if (scope.max == undefined) {
                    scope.max = 5;
                }
                function updateStars() {
                    scope.stars = [];
                    for (var i = 0; i < scope.max; i++) {
                        scope.stars.push({
                            filled: i < scope.ratingValue
                        });
                    }
                };
                scope.toggle = function (index) {
                    if (scope.readonly == undefined || scope.readonly === false) {
                        scope.ratingValue = index + 1;

                        if(scope.onRatingSelect != null){
                            scope.onRatingSelect({
                                rating: index + 1
                            });
                        }
                    }
                };
                scope.$watch('ratingValue', function (oldValue, newValue) {
                    if (newValue || newValue === 0) {
                        updateStars();
                    }
                });
            }
        };
    });

    ngapp.filter("fullUrl", function () {
        return function (url) {
            if (url != null) {
                return "/home/fwddownload/" + encodeURIComponent(url);
            }
        };
    }); 
})();