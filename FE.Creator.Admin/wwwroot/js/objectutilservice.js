;(function () {
  angular
        .module('ngObjectRepository')
        .factory("objectUtilService", objectUtilService);

  objectUtilService.$inject = ["$log", "ObjectRepositoryDataService"];

  function objectUtilService($log, ObjectRepositoryDataService) {
      return {
          getStringPropertyValue: getSimplePropertyValue,
          getIntegerPropertyValue: getSimplePropertyValue,
          getLongPropertyValue : getSimplePropertyValue, 
          getDateTimePropertyValue : getSimplePropertyValue,
          getNumberPropertyValue: getSimplePropertyValue,
          getBinaryPropertyValue: getSimplePropertyValue,
          getFilePropertyValue: getFilePropertyValue,
          getObjectRefPropertyValue: getObjectRefPropertyValue,
          getSingleSelectionPropertyValue: getSingleSelectionPropertyValue,
          parseServiceObject: parseServiceObject,
          convertAsServiceObject: convertAsServiceObject,
          addStringProperty: addStringProperty,
          addIntegerProperty: addIntegerProperty,
          addLongProperty: addLongProperty,
          addDateTimeProperty: addDateTimeProperty,
          addNumberProperty: addNumberProperty,
          addBinaryProperty: addBinaryProperty,
          addFileProperty: addFileProperty,
          addObjectRefProperty: addObjectRefProperty,
          addSingleSPropery: addSingleSPropery,
          saveServiceObject: saveServiceObject,
          saveServiceObjects: saveServiceObjects,
          cloneJsonObject: cloneJsonObject,
          deleteObjectFromArrary: deleteObjectFromArrary
      }

      function getSimplePropertyValue(sourceObj, propName) {
          for (var i = 0; i < sourceObj.properties.length; i++) {
              var prop = sourceObj.properties[i];
              if (prop.keyName == propName) {
                  return prop.value.value;
              }
          }

          return null;
      }

      function getFilePropertyValue(sourceObj, propName) {
          for (var i = 0; i < sourceObj.properties.length; i++) {
              var prop = sourceObj.properties[i];
              if (prop.keyName == propName) {
                  return prop.value;
              }
          }

          return {};
      }

      function getSingleSelectionPropertyValue(sourceObj, propName) {
          for (var i = 0; i < sourceObj.properties.length; i++) {
              var prop = sourceObj.properties[i];
              if (prop.keyName == propName) {
                  return prop;
              }
          }

          return null;
      }

      function getObjectRefPropertyValue(sourceObj, propName) {
          for (var i = 0; i < sourceObj.properties.length; i++) {
              var prop = sourceObj.properties[i];
              if (prop.keyName == propName) {
                  return ObjectRepositoryDataService.getServiceObject(prop.value.referedGeneralObjectID)
                   .then(function (data) {
                       return data;
                   });
              }
          }
    
          return {};
      }

      function createPrimaryProperty(primaryTypeIndex,propName, propValue) {
          //String,
          //Integer,
          //Long,
          //Datetime,
          //Number,
          //Binary

          var property = {
              keyName: propName,
              $type: "FE.Creator.ObjectRepository.ServiceModels.ObjectKeyValuePair, FE.Creator.ObjectRepository",
              value: {
                  $type: "FE.Creator.ObjectRepository.ServiceModels.PrimeObjectField, FE.Creator.ObjectRepository",
                  primeDataType: primaryTypeIndex,
                  value: propValue
              }
          };

          return property;
      }

      function addStringProperty(sourceObj, propName, propValue) {
          if(sourceObj.properties == null)
          {
              sourceObj.properties = [];
          }

          var property = createPrimaryProperty(0, propName, propValue);
          sourceObj.properties.push(property);
      }

      function addIntegerProperty(sourceObj, propName, propValue){
          if (sourceObj.properties == null) {
              sourceObj.properties = [];
          }

          var property = createPrimaryProperty(1, propName, propValue);
          sourceObj.properties.push(property);
      }

      function addLongProperty(sourceObj, propName, propValue) {
          if (sourceObj.properties == null) {
              sourceObj.properties = [];
          }

          var property = createPrimaryProperty(2, propName, propValue);
          sourceObj.properties.push(property);
      }

      function addDateTimeProperty(sourceObj, propName, propValue) {
          if (sourceObj.properties == null) {
              sourceObj.properties = [];
          }

          var property = createPrimaryProperty(3, propName, propValue);
          sourceObj.properties.push(property);
      }

      function addNumberProperty(sourceObj, propName, propValue){
          if (sourceObj.properties == null) {
              sourceObj.properties = [];
          }

          var property = createPrimaryProperty(4, propName, propValue);
          sourceObj.properties.push(property);
      }

      function addBinaryProperty(sourceObj, propName, propValue) {
          if (sourceObj.properties == null) {
              sourceObj.properties = [];
          }

          var property = createPrimaryProperty(5, propName, propValue);
          sourceObj.properties.push(property);
      }
      function assignProperties(sourceObj, targetObj) {
          if (sourceObj == null || targetObj == null)
              return;

          for (var pname in sourceObj) {
              targetObj[pname] = sourceObj[pname];
          }
      }
      function addFileProperty(sourceObj, propName, propValue) {
          if (sourceObj.properties == null) {
              sourceObj.properties = [];
          }

          var property = {
              keyName: propName,
              $type: "FE.Creator.ObjectRepository.ServiceModels.ObjectKeyValuePair, FE.Creator.ObjectRepository",
              value: {
                  $type: "FE.Creator.ObjectRepository.ServiceModels.ObjectFileField, FE.Creator.ObjectRepository"
              }
          };
         
          assignProperties(propValue, property.value);
          sourceObj.properties.push(property);
      }

      function addObjectRefProperty(sourceObj, propName, propValue) {
          if (sourceObj.properties == null) {
              sourceObj.properties = [];
          }

          var property = {
              keyName: propName,
              $type: "FE.Creator.ObjectRepository.ServiceModels.ObjectKeyValuePair, FE.Creator.ObjectRepository",
              value: {
                  $type: "FE.Creator.ObjectRepository.ServiceModels.ObjectReferenceField, FE.Creator.ObjectRepository",
                  referedGeneralObjectID : propValue
              }
          };

          sourceObj.properties.push(property);
      }

      function addSingleSPropery(sourceObj, propName, propValue, selectionItems) {
          if (sourceObj.properties == null) {
              sourceObj.properties = [];
          }

          var property = {
              keyName: propName,
              $type: "FE.Creator.ObjectRepository.ServiceModels.ObjectKeyValuePair, FE.Creator.ObjectRepository",
              value: {
                  $type: "FE.Creator.ObjectRepository.ServiceModels.SingleSelectionField, FE.Creator.ObjectRepository",
                  selectionItems: selectionItems
              }
          };

          assignProperties(propValue, property.value);
          sourceObj.properties.push(property);
      }

      function parseServiceObject(sourceObj) {

          if (sourceObj == null)
              return null;

          var retObj = {
              objectID : sourceObj.objectID,
              objectName : sourceObj.objectName,
              objectDefinitionId : sourceObj.objectDefinitionId,
              created: sourceObj.created,
              createdBy: sourceObj.createdBy,
              updated: sourceObj.updated,
              updatedBy: sourceObj.updatedBy,
              onlyUpdateProperties: sourceObj.onlyUpdateProperties,
              objectOwner: sourceObj.objectOwner,
              properties: []
          };

          if (sourceObj.properties != null) {
              for (var i = 0; i < sourceObj.properties.length; i++) {
                  var prop = sourceObj.properties[i];
                  retObj.properties[prop.keyName] = prop.value;
              }
          }

          return retObj;
      }

      function convertAsServiceObject(sourceObj) {
          if (sourceObj == null)
              return null;

          var retObj = {
              objectID: sourceObj.objectID,
              objectName: sourceObj.objectName,
              objectDefinitionId: sourceObj.objectDefinitionId,
              created: sourceObj.created,
              createdBy: sourceObj.createdBy,
              updated: sourceObj.updated,
              updatedBy: sourceObj.updatedBy,
              onlyUpdateProperties: sourceObj.onlyUpdateProperties,
              objectOwner: sourceObj.objectOwner,
              properties: []
          };

          if (sourceObj.properties != null) {
              for (var prop in sourceObj.properties) {
                  var svProperty = {
                      keyName: prop,
                      $type: "FE.Creator.ObjectRepository.ServiceModels.ObjectKeyValuePair, FE.Creator.ObjectRepository",
                      value: sourceObj.properties[prop]
                  };

                  retObj.properties.push(svProperty);
              }
          }

          return retObj;
      }

      function saveServiceObject(editingObject, callback) {
          var svcObject = convertAsServiceObject(editingObject);
          return ObjectRepositoryDataService.createOrUpdateServiceObject(
                  svcObject.objectID,
                  svcObject
              ).then(function (data) {
                  if (callback != null)
                      callback(data);
              });
      }

      function saveServiceObjects(objArrary, currIndex, callback) {
          if (objArrary.length > 0) {
             return saveServiceObject(objArrary[currIndex],
              function (data) {
                  if (data != null && data != "" && data.objectID != null) {
                      objArrary[currIndex].objectID = data.objectID;
                      objArrary[currIndex].onlyUpdateProperties = true;
                  }

                  currIndex = currIndex + 1;
                  if (currIndex < objArrary.length) {
                      saveServiceObjects(objArrary, currIndex, callback);
                  }

                  if (callback != null) {
                      callback(data, currIndex);
                  }
              })
          }
      }

      function cloneJsonObject(obj) {
          // Handle the 3 simple types, and null or undefined
          if (null == obj || "object" != typeof obj) return obj;

          // Handle Date
          if (obj instanceof Date) {
              var copy = new Date();
              copy.setTime(obj.getTime());
              return copy;
          }

          // Handle Array
          if (obj instanceof Array) {
              var copy = [];
              for (var i = 0, len = obj.length; i < len; ++i) {
                  copy[i] = cloneJsonObject(obj[i]);
              }

              //even for Arry, it may still has properties.
              for (var attr in obj) {
                  if (obj.hasOwnProperty(attr)) copy[attr] = cloneJsonObject(obj[attr]);
              }
              return copy;
          }

          // Handle Object
          if (obj instanceof Object) {
              var copy = {};
              for (var attr in obj) {
                  if (obj.hasOwnProperty(attr)) copy[attr] = cloneJsonObject(obj[attr]);
              }
              return copy;
          }

          throw new Error("Unable to copy obj! Its type isn't supported.");
      }

      function deleteObjectFromArrary(objectArrary, tObj, callback) {
          var idx = objectArrary.indexOf(tObj);
          if (idx >= 0) {
              if (tObj.objectID != null) {
                  ObjectRepositoryDataService.deleteServiceObject(tObj.objectID).then(function (data) {
                      if (data != null && data != "" && data.status == 204) {
                          if (idx >= 0)
                              objectArrary.splice(idx, 1);
                      }

                      if (callback != null)
                          callback(data);
                  })
              }
              else {
                  if (idx >= 0)
                      objectArrary.splice(idx, 1);
              }
          }
      }
  }
})();