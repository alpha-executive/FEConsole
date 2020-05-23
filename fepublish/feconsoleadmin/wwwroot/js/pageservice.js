(function () {
    "use strict";

    angular
        .module('ngObjectRepository')
        .factory("PagerService", PagerService);

    function PagerService() {

        return {
            createPager : createPager
        };

        function createPager(total, pageIndex, pageSize, pagerPageLength) {
            // default to first page
            pageIndex = pageIndex || 1;

            // default page size is 10
            pageSize = pageSize || 10;

            //default pager page length is 10.
            pagerPageLength = pagerPageLength || 10;

            // calculate total pages
            var totalPages = Math.ceil(total / pageSize);

            var startPage, endPage;
            if (totalPages <= pagerPageLength) {
                // less than total pages so show all
                startPage = 1;
                endPage = totalPages;
            } else {
                var middlePageIndex = Math.ceil(pagerPageLength / 2);
                // more than total pages so calculate start and end pages
                if (pageIndex <= middlePageIndex) {
                    startPage = 1;
                    endPage = pagerPageLength;
                } else if (pageIndex + pagerPageLength - middlePageIndex >= totalPages) {
                    startPage = totalPages - pagerPageLength + 1;
                    endPage = totalPages;
                } else {
                    startPage = pageIndex - middlePageIndex + 1;
                    endPage = pageIndex + pagerPageLength - middlePageIndex;
                }
            }

            // calculate start and end item indexes
            var startIndex = (pageIndex - 1) * pageSize;
            var endIndex = Math.min(startIndex + pageSize - 1, total - 1);

            // create an array of pages to ng-repeat in the pager control
            var pages = [];
            for (var i = startPage; i < endPage + 1; i++) {
                pages.push(i);
            }

            // return object with all pager properties required by the view
            return {
                total: total,
                pageIndex: pageIndex,
                pageSize: pageSize,
                totalPages: totalPages,
                startPage: startPage,
                endPage: endPage,
                startIndex: startIndex,
                endIndex: endIndex,
                pages: pages,
                disabledFirstPage: false,
                disabledLastPage: false
            };
        }
    }
})();