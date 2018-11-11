using FE.Creator.ObjectRepository.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FE.Creator.ObjectRepository
{
   public interface IObjectService
    {
        /// <summary>
        /// create or update A object definition.
        /// when create, set ObjectDefinitionID to negative integer.
        /// </summary>
        /// <param name="objectDef"></param>
        /// <returns></returns>
        int CreateORUpdateObjectDefinition(ObjectDefinition objectDef);


        /// <summary>
        /// get the count of the object definition
        /// </summary>
        /// <returns></returns>
        int GetObjectDefinitionCount();

        /// <summary>
        /// Get all the object Definition
        /// </summary>
        /// <returns></returns>
        List<ObjectDefinition> GetAllObjectDefinitions();

        /// <summary>
        /// get object definitions of the specific page.
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        List<ObjectDefinition> GetObjectDefinitions(int currentPage, int pageSize);

        /// <summary>
        /// Get the object definition by object definition id.
        /// </summary>
        /// <param name="ObjDefId"></param>
        /// <returns></returns>
        ObjectDefinition GetObjectDefinitionById(int objDefId);


        /// <summary>
        /// Retrieve Object definition by definition name.
        /// </summary>
        /// <param name="definitionName"></param>
        /// <returns></returns>
        ObjectDefinition GetObjectDefinitionByName(string definitionName);

        /// <summary>
        /// Get the object definitions by definition group
        /// </summary>
        /// <param name="GroupId"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        List<ObjectDefinition> GetObjectDefinitionsByGroup(int GroupId, int currentPage, int pageSize, ServiceRequestContext requestContext);

        /// <summary>
        /// Get the object definitions except for the given group.
        /// </summary>
        /// <param name="GroupId">GROUP ID</param>
        /// <returns>object definitions not belone to the specified group.</returns>
        List<ObjectDefinition> GetObjectDefinitionsExceptGroup(int GroupId, ServiceRequestContext requestContext);
        /// <summary>
        /// Soft delete the object definition, add delete flag to the object definition.
        /// </summary>
        /// <param name="ObjDefId"></param>
        /// <returns></returns>
        int SoftDeleteObjectDefinition(int objDefId, string updatedBy);


        /// <summary>
        /// Create or update the genral object
        /// </summary>
        /// <param name="serviceObject"></param>
        /// <returns></returns>
        int CreateORUpdateGeneralObject(ServiceObject serviceObject);



        /// <summary>
        /// get the count of the general object.
        /// </summary>
        /// <returns></returns>
        int GetGeneralObjectCount(int ObjDefId, ServiceRequestContext requestContext);

        /// <summary>
        /// get the service objects by object id
        /// </summary>
        /// <param name="objectId">the object id</param>
        /// <param name="properties">the properties will retrieved from the object</param>
        /// <returns></returns>
        ServiceObject GetServiceObjectById(int objectId, string[] properties, ServiceRequestContext requestContext);


        /// <summary>
        /// get all the service objects
        /// </summary>
        /// <param name="ObjDefId">Object Definition Id</param>
        /// <param name="properties">the properties will be retrieved from the objects</param>
        /// <returns></returns>
        List<ServiceObject>  GetAllSerivceObjects(int ObjDefId, string[] properties, ServiceRequestContext requestContext);

        /// <summary>
        /// get service objects of the specific page.
        /// </summary>
        /// <param name="ObjDefId">the object definition id</param>
        /// <param name="properties">the properties will be retrieved from the objects</param>
        /// <param name="currentPage">current page number</param>
        /// <param name="pageSize">current page size</param>
        /// <returns></returns>
        List<ServiceObject> GetServiceObjects(int ObjDefId, string[] properties, int currentPage, int pageSize, ServiceRequestContext requestContext);

        /// <summary>
        /// Soft delete the service object, set a delete flag to the object.
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        int SoftDeleteServiceObject(int objectId, string updatedBy);

        /// <summary>
        /// Get all the ObjectDefinitionGroups.
        /// </summary>
        /// <returns></returns>
        List<ObjectDefinitionGroup> GetObjectDefinitionGroups(int? parentGroupId, ServiceRequestContext requestContext);

        /// <summary>
        /// Check whether the object definition group is exists.
        /// </summary>
        /// <param name="groupName">object definition group name</param>
        /// <returns></returns>
        bool IsObjectDefinitionGroupExists(string groupName);


        /// <summary>
        /// get the object definition group by name.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        ObjectDefinitionGroup GetObjectDefinitionGroupByName(string groupName);

        /// <summary>
        /// Get the ObjectDefintionGroup by ID.
        /// </summary>
        /// <param name="defId"></param>
        /// <returns></returns>
        ObjectDefinitionGroup GetObjectDefinitionGroupById(int defId);

        /// <summary>
        /// create or update the ObjectDefinitionGroup, return the id of the ObjectDefintionGroup
        /// </summary>
        /// <param name="objGroup"></param>
        /// <returns></returns>
        int CreateOrUpdateObjectDefinitionGroup(ObjectDefinitionGroup objGroup);

        /// <summary>
        /// Mark the Object Definition Group As Deleted, return the id of the deleted DefinitionGroup, -1 if there is no 
        /// such object definition.
        /// </summary>
        /// <param name="defId"></param>
        /// <returns></returns>
        int SoftDeleteObjectDefintionGroup(int defId);


        /// <summary>
        /// Delete the service object by the object Id.
        /// </summary>
        /// <param name="serviceObjectId"></param>
        void DeleteServiceObject(int serviceObjectId);


        /// <summary>
        /// Delete a object field by the object field Id.
        /// </summary>
        /// <param name="objectFieldId"></param>
        void DeleteObjectField(int objectFieldId);

        /// <summary>
        /// Delete the object Field Definition by field definition id.
        /// Be carefull: this will delete all the object fields that created based on this field definition as well.
        /// </summary>
        /// <param name="fieldDefinitionId"></param>
        void DeleteObjectDefinitionField(int fieldDefinitionId);

        /// <summary>
        /// Delete the object definition by definition id.
        /// Be Carefull: this will delete all the objects which created based on this definition as well.
        /// </summary>
        /// <param name="objectDefinitionId"></param>
        void DeleteObjectDefinition(int objectDefinitionId);


        void DeleteSingleSelectionFieldSelectionItem(int selectionItemId);
    }
}
