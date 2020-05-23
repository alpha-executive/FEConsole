(function () {
    'use strict'
    angular
       .module('ngObjectRepository')
         .controller("TaskController", TaskController);

    TaskController.$inject = ["$scope", "$q", "ObjectRepositoryDataService", "Notification", "PagerService", "objectUtilService", "UserFactory"];
    function TaskController($scope, $q, ObjectRepositoryDataService, Notification, PagerService, objectUtilService, UserFactory) {
        var vm = this;
        vm.tasks = [];
        vm.targets = [];
        vm.objectDefinitions = [];
        vm.displayMode = "targetList";
        vm.currentTask = null;
        vm.currentTarget = null;
        vm.pager = {};
        vm.pageSize = 18;
        vm.progress = 0;
        vm.onPageClick = onPageClick;
        vm.cancelObject = null;

        UserFactory.getAccessToken()
            .then(function (response) {
                init();
                return response;
            });

        function init() {
            ObjectRepositoryDataService.getLightWeightObjectDefinitions().then(
               function (data) {
                   vm.objectDefinitions = data;

                   return vm.objectDefinitions;
               }).then(function (data) {
                   //get all the categories and books.
                   onPageClick(1, true);
               });
        }

        function createTask(target, taskName) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("Task");
            tempObj.objectName = taskName;
        
            objectUtilService.addStringProperty(tempObj, "taskDesc", null);
            objectUtilService.addIntegerProperty(tempObj, "taskType", 0);
            objectUtilService.addDateTimeProperty(tempObj, "taskStartDate", null);
            objectUtilService.addDateTimeProperty(tempObj, "taskExpEndDate", null);
            objectUtilService.addDateTimeProperty(tempObj, "taskEndDate", null);
            objectUtilService.addIntegerProperty(tempObj, "taskStatus", 0);
            objectUtilService.addObjectRefProperty(tempObj, "target", target.objectID);

            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function createTarget(targetName) {
            var tempObj = {};
            tempObj.objectDefinitionId = vm.getObjectDefintionIdByName("Target");
            tempObj.objectName = targetName;

            objectUtilService.addStringProperty(tempObj, "targetDesc", null);
            objectUtilService.addIntegerProperty(tempObj, "targetType", 0);
            objectUtilService.addDateTimeProperty(tempObj, "targetStartDate", null);
            objectUtilService.addIntegerProperty(tempObj, "targetStatus", 0);
            objectUtilService.addDateTimeProperty(tempObj, "targetCompleteDate", null);

            tempObj = objectUtilService.parseServiceObject(tempObj);

            return tempObj;
        }

        function onPageClick(pageIndex, force) {
            if (vm.pager.currentPage == pageIndex && !force)
                return;

            var isTarget = vm.displayMode == "targetList";
            onPageChange(pageIndex, isTarget);
        }

        function onPageChange(pageIndex, isTarget) {
            vm.reCalculatePager(pageIndex, isTarget).then(function (data) {
                if (pageIndex < 1) {
                    pageIndex = 1;
                }
                if (pageIndex > vm.pager.totalPages) {
                    pageIndex = vm.pager.totalPages;
                }
                vm.pager.currentPage = pageIndex;

                if (isTarget) {
                    vm.reloadTarget(pageIndex);
                }
                else {
                    vm.reloadTask(pageIndex);
                }
            });
        }

        function updateTaskStatus(percentage, task) {
            var originalStatus = task.properties.taskStatus.value;
            task.properties.taskStatus.value = task.properties.taskStatus.value + percentage > 100 ? 100 : task.properties.taskStatus.value + percentage;
            if (task.properties.taskStatus.value == 100) {
                if (task.properties.taskEndDate == null) {
                    objectUtilService.addDateTimeProperty(task, "taskEndDate", new Date());
                }
                else {
                    task.properties.taskEndDate.value = new Date();
                }
            } //first time increase.
            else if(task.properties.taskStatus.value <= 20) {
                if (task.properties.taskStartDate == null) {
                    objectUtilService.addDateTimeProperty(task, "taskStartDate", new Date());
                }
                else {
                    task.properties.taskStartDate.value = new Date();
                }
            }

            objectUtilService.saveServiceObject(task, function (data) {
                if (data.status == null || data.status == 200) {
                    updateTargetStatus(task.properties.target.referedGeneralObjectID)
                }
                else {
                    task.properties.taskStatus.value = originalStatus;
                }
            });
        }

        function deleteTargetTasks(tasks, isUpdateProgress) {
            var defers = [];
            var processedCount = 0;
            if (Array.isArray(tasks) && tasks.length > 0) {
                for (var i = 0; tasks != null && i < tasks.length; i++) {
                    var promise = function (idx) {
                        var defer = $q.defer();
                        var task = tasks[idx];

                        ObjectRepositoryDataService.deleteServiceObject(task.objectID)
                        .then(function (data) {
                            if (isUpdateProgress) {
                                processedCount += 1;
                            }

                            defer.resolve(task);
                        });

                        return defer.promise;
                    }

                    var cachedPromise = isUpdateProgress ? promise(i).then(function (data) {
                        vm.progress = Math.floor(processedCount * 100.0 / tasks.length);
                        return vm.progress;
                    }) : promise(i);

                    defers.push(cachedPromise);
                }
            }

            return $q.all(defers);
        }

        function updateTargetStatus(targetid) {
          return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                "Task",
                ["taskStatus", "target"].join(),
                null,
                null,
                "target," + targetid
            ).then(function (data) {
                var percentage = 0;
                if (Array.isArray(data) && data.length > 0) {
                    for (var i = 0; i < data.length; i++) {
                        var task = objectUtilService.parseServiceObject(data[i]);
                        percentage += task.properties.taskStatus.value;
                    }
                    percentage = percentage / data.length;
                }

                return percentage;
            }).then(function(data){
                if (!isNaN(data)) {
                    ObjectRepositoryDataService.getServiceObject(targetid)
                   .then(function (data) {
                       objectUtilService.addIntegerProperty(data, "targetStatus", 0);
                       var target = objectUtilService.parseServiceObject(data);
                       return target;
                   })
                   .then(function (target) {
                       target.properties.targetStatus.value = Math.floor(data);
                       objectUtilService.saveServiceObject(target, function (data) { });
                   });
                }

                return data;
            });
        }

        function getTaskDisplayMode() {
            return vm.currentTarget == null ? "taskList" : "targetTaskList";
        }
        vm.switchTab = function (isTarget) {
            //reset the pager and task/target global variables.
            vm.currentTarget = null;
            vm.currentTask = null;
            vm.pager = {};

            if (isTarget) {
                vm.setDisplayMode("targetList");
            }
            else {
                vm.setDisplayMode("taskList");
            }

            //force the page refresh.
            vm.onPageClick(1, true);
        }

        vm.navToTargetList = function (target) {
            vm.setDisplayMode("targetList");
            vm.currentTarget = null;
            onPageClick(vm.pager.currentPage, true);
        }

        vm.setDisplayMode = function (mode) {
            if(vm.displayMode != mode)
            {
                vm.displayMode = mode;
            }
        }
        vm.reCalculatePager = function (pageIndex, isTarget) {
            var objDefinitionId = isTarget ?
                vm.getObjectDefintionIdByName("Target") : vm.getObjectDefintionIdByName("Task");
            var filter = isTarget ? null :
                (vm.currentTarget == null ? null : "target, " + vm.currentTarget.objectID);

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

        vm.createOrUpdateTask = function (task) {
            var tempTask = task;
            if (tempTask == null) {
                tempTask = createTask(vm.currentTarget, "New Task");
            };

            vm.currentTask = tempTask;
            vm.cancelObject = objectUtilService.cloneJsonObject(tempTask);
            vm.currentTarget == null ? vm.setDisplayMode("taskEditing") : vm.setDisplayMode("targetTaskEditing");
        }

        vm.createorUpdateTarget = function (target) {
            var tempTarget = target;
            if (tempTarget == null) {
                tempTarget = createTarget("New Target");
            }

            vm.currentTarget = tempTarget;
            vm.cancelObject = objectUtilService.cloneJsonObject(tempTarget);
            vm.setDisplayMode("targetEditing");
        }

        vm.saveTask = function (task) {
            objectUtilService.saveServiceObject(vm.currentTask, function (data) {
                var displayMode = getTaskDisplayMode();
                vm.setDisplayMode(displayMode);

                if ((vm.currentTask.objectID == null || vm.currentTask.objectID == 0)
                    && vm.tasks.length >= vm.pager.pageSize){
                    onPageChange(1, false);
                } else {
                    var tmpTask = objectUtilService.parseServiceObject(data);
                    var index = vm.tasks.indexOf(vm.currentTask);
                    if (index >= 0) {
                        vm.tasks.splice(index, 1, tmpTask);
                    } else {
                        vm.tasks.unshift(tmpTask);
                    }
                }

                var targetid = task == null ? vm.currentTarget.objectID : task.properties.target.referedGeneralObjectID;
                updateTargetStatus(targetid)
                .then(function (data) {
                    Notification.success({
                        message: 'Change Saved!',
                        delay: 3000,
                        positionY: 'bottom',
                        positionX: 'right',
                        title: 'Warn',
                    });
                });
            });
        }

        vm.startTask = function (task) {
            //set the task status value to 20.
            updateTaskStatus(20, task);
        }

        vm.completeTask = function (task) {
            updateTaskStatus(100, task);
        }

        vm.saveTarget = function (target) {
            objectUtilService.saveServiceObject(vm.currentTarget, function (data) {
                vm.setDisplayMode("targetList");

                //new insert.
                if ((vm.currentTarget.objectID == null || vm.currentTarget.objectID == 0)
                    && vm.targets.length >= vm.pager.pageSize) {
                    onPageClick(1, true);
                }
                else {
                    var index = vm.targets.indexOf(vm.currentTarget);
                    var target = objectUtilService.parseServiceObject(data);
                    if (index >= 0) {
                        vm.targets.splice(index, 1, target);
                    } else {
                        vm.targets.unshift(target);
                    }
                }
                
               
                Notification.success({
                    message: 'Change Saved!',
                    delay: 3000,
                    positionY: 'bottom',
                    positionX: 'right',
                    title: 'Warn',
                });
            });
        }

        vm.deleteTask = function (task) {
            ObjectRepositoryDataService.deleteServiceObject(task.objectID)
            .then(function (data) {
                //when delete the last record in first page or last page, or delete a record in middle page.
                if (vm.pager.totalPages > vm.pager.currentPage
                    || (vm.tasks.length <= 1 && vm.pager.totalPages == vm.pager.currentPage)) {
                    var navPageIndex = vm.tasks.length - 1 <= 0
                           ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                    onPageChange(navPageIndex, false);
                }
                else {
                     var index = vm.tasks.indexOf(task);
                     if (index >= 0) {
                         vm.tasks.splice(index, 1);
                     }
                 }

                var displayMode = getTaskDisplayMode();
                vm.setDisplayMode(displayMode);
            })
            .then(function (data) {
                updateTargetStatus(task.properties.target.referedGeneralObjectID);
            });
        }

        vm.deleteTarget = function (target) {
         ObjectRepositoryDataService.getServiceObjectsWithFilters(
             "Task",
             "target",
             null,
             null,
             "target," + target.objectID
         ).then(function (data) {
            deleteTargetTasks(data, true)
           .then(function (data) {
               ObjectRepositoryDataService.deleteServiceObject(target.objectID)
                  .then(function (data) {
                      if (vm.pager.totalPages > vm.pager.currentPage
                           || (vm.targets.length <= 1 && vm.pager.totalPages == vm.pager.currentPage)) {
                          var navPageIndex = vm.targets.length - 1 <= 0
                                 ? vm.pager.currentPage - 1 : vm.pager.currentPage;
                          onPageChange(navPageIndex, true);
                      }
                      else {
                          var index = vm.targets.indexOf(target);
                          if (index >= 0) {
                              vm.targets.splice(index, 1);
                          }
                      }
                  });
           });
         });

           
        }

        vm.cancelTarget = function () {
            var index = vm.targets.indexOf(vm.currentTarget);
            if (index >= 0) {
                vm.targets.splice(index, 1, vm.cancelObject);
            }
            vm.setDisplayMode("targetList");
        }
        vm.cancelTask = function () {
            var index = vm.tasks.indexOf(vm.currentTask);
            if (index >= 0) {
                vm.tasks.splice(index, 1, vm.cancelObject);
            }

            var displayMode = getTaskDisplayMode();
            vm.setDisplayMode(displayMode);
        }

        vm.showTargetTasks = function (target) {
            vm.currentTarget = target;
            vm.setDisplayMode("targetTaskList");
            vm.reloadTask(1);
        }
        vm.reloadTarget = function (pageIndex) {
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
               "Target",
               ["targetDesc", "targetType", "targetStartDate", "targetStatus", "targetCompleteDate"].join(),
               pageIndex,
               vm.pageSize,
               null
           ).then(function (data) {
               vm.targets.splice(0, vm.targets.length);
               if (Array.isArray(data) && data.length > 0) {
                   for (var i = 0; i < data.length; i++) {
                       var target = objectUtilService.parseServiceObject(data[i]);
                       vm.targets.push(target);
                   }
               }

               return vm.targets;
           });
        }

        vm.reloadTask = function (pageIndex) {
            var filter = vm.currentTarget == null ? null : "target," + vm.currentTarget.objectID;
            return ObjectRepositoryDataService.getServiceObjectsWithFilters(
                "Task",
                ["taskDesc", "taskType", "taskStartDate", "taskExpEndDate", "taskEndDate", "taskStatus", "target"].join(),
                pageIndex,
                vm.displayMode == "targetTaskList" ? null : vm.pageSize,
                filter
            ).then(function (data) {
                vm.tasks.splice(0, vm.tasks.length);
                if (Array.isArray(data) && data.length > 0) {
                    for (var i = 0; i < data.length; i++) {
                        var task = objectUtilService.parseServiceObject(data[i]);
                        vm.tasks.push(task);
                    }
                }

                return vm.tasks;
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