(function () {
    "use strict";
    angular
        .module('ngObjectRepository')
        .controller("editObjectDefinitionGroupController", editObjectDefinitionGroupController);

    editObjectDefinitionGroupController.$inject = ["$scope", "$rootScope", "ObjectRepositoryDataService", "Notification", "UserFactory"];
    /*controller*/
    function editObjectDefinitionGroupController($scope, $rootScope, ObjectRepositoryDataService, Notification, UserFactory) {
        var scopeContext = this;
        scopeContext.currentGroup = {};
        scopeContext.SaveChange = SaveChange;
        scopeContext.DefinitionGroups = [];
        scopeContext.DeleteGroup = DeleteGroup;
        scopeContext.EditGroup = EditGroup;
        scopeContext.SetEdit = SetEdit;
        scopeContext.ShowObjDefinition = ShowObjDefinition;
        scopeContext.showDefinitionGroupView = true;
        scopeContext.showEditView = false;

        UserFactory.getAccessToken()
            .then(function (response) {
                Initialze();
               
                return response;
            });
        
        function Initialze() {
            ObjectRepositoryDataService.getObjectDefinitionGroups(null)
                .then(function (data) {
                    scopeContext.DefinitionGroups = data;

                    return scopeContext.DefinitionGroups;
                });
        }


        function ShowObjDefinition(group) {
            //fire the onshowtable event, which will be handle in ObjectDefinitionController.
            $rootScope.$broadcast('showtable', group);
            scopeContext.showDefinitionGroupView = false;
        }

        function EditGroup(group) {
            scopeContext.showEditView = true;
           
            if (typeof group !== 'undefined') {
                scopeContext.currentGroup = group;
            }
            else {
                //new add
                scopeContext.currentGroup = {};
            }
        }

        function SetEdit(isEdit) {
            scopeContext.showEditView = isEdit;
        }

        function DeleteGroup(group) {
            try {
                ObjectRepositoryDataService
                    .deleteDefinitionGroup(group.groupID)
                    .then(function (data) {
                        var index = scopeContext.DefinitionGroups.indexOf(group);
                        if (index >= 0) {
                            scopeContext.DefinitionGroups.splice(index, 1);
                        }
                        return data;
                    })
               
            }
            catch (e) { }
        }

        function SaveChange() {
            try{
                ObjectRepositoryDataService.createOrUpdateDefinitionGroup(scopeContext.currentGroup.groupID,
                    scopeContext.currentGroup)
                    .then(function(data){
                        Notification.success({
                                message: AppLang.COMMON_EDIT_SAVE_SUCCESS,
                                delay: 3000,
                                positionY: 'bottom',
                                positionX: 'right',
                                title: 'Success',
                        });

                            //for create, we will update the currentGroup model.
                            if (scopeContext.currentGroup.groupID == null) {
                                scopeContext.currentGroup = data;
                                scopeContext.DefinitionGroups.push(data);
                            }
                                   
                        });
            }
            catch(e)
            {
                Notification.error({
                    message: AppLang.COMMON_EDIT_SAVE_FAILED,
                    delay: 5000,
                    positionY: 'bottom',
                    positionX: 'right',
                    title: AppLang.COMMON_DLG_TITLE_ERROR
                });
            }
        }
    }
})();