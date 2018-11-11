using FE.Creator.ObjectRepository.EntityModels;
using FE.Creator.ObjectRepository.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.Utils
{
   internal class ObjectConverter
    {

        public static GeneralObjectDefinitionField ConvertSvcField2ObjDefinitionField(ObjectDefinitionField f, GeneralObjectDefinitionField field)
        {
            switch (f.GeneralObjectDefinitionFiledType)
            {
                case GeneralObjectDefinitionFieldType.PrimeType:
                    return ParsePrimeDefinitionField(f as PrimeDefinitionField, field as PrimeObjectDefintionField);
                case GeneralObjectDefinitionFieldType.SingleSelection:
                    return ParseSingleSelectionDefinitionField(f as SingleSDefinitionField, field as SingleSelectionDefinitionField);
                case GeneralObjectDefinitionFieldType.ObjectReference:
                    return ParseObjectReferenceDefinitionField(f as ObjRefDefinitionField, field as ObjRefObjectDefinitionField);
                case GeneralObjectDefinitionFieldType.File:
                    return ParseGeneralObjectDefinitionField(f, field);
                default:
                    throw new NotSupportedException(string.Format("{0} is not supported!", f.GeneralObjectDefinitionFiledType));
            }
        }

        private static SingleSelectionDefinitionField ParseSingleSelectionDefinitionField(SingleSDefinitionField f, SingleSelectionDefinitionField field)
        {
            if (field == null)
                field = new SingleSelectionDefinitionField();

            field.GeneralObjectDefinitionFieldKey = f.ObjectDefinitionFieldKey;
            field.GeneralObjectDefinitionFieldName = f.ObjectDefinitionFieldName;
            field.GeneralObjectDefinitionFiledType = f.GeneralObjectDefinitionFiledType;

            foreach (var item in f.SelectionItems)
            {
                var selectItem = (from s in field.SelectionItems
                                  where s.GeneralObjectDefinitionSelectItemID == item.SelectItemID
                                  select s).FirstOrDefault();

                if (selectItem != null)
                {
                    //update
                    selectItem.SelectDisplayName = item.SelectDisplayName;
                    selectItem.SelectItemKey = item.SelectItemKey;
                }
                else
                {
                    //insert.
                    field.SelectionItems.Add(new GeneralObjectDefinitionSelectItem()
                    {
                        SelectItemKey = item.SelectItemKey,
                        SelectDisplayName = item.SelectDisplayName
                    });
                }
            }

            return field;
        }

        private static ObjRefObjectDefinitionField ParseObjectReferenceDefinitionField(ObjRefDefinitionField f, ObjRefObjectDefinitionField field)
        {
            if (field == null)
                field = new ObjRefObjectDefinitionField();

            field.GeneralObjectDefinitionFieldKey = f.ObjectDefinitionFieldKey;
            field.GeneralObjectDefinitionFieldName = f.ObjectDefinitionFieldName;
            field.GeneralObjectDefinitionFiledType = f.GeneralObjectDefinitionFiledType;
            field.ReferedObjectDefinitionID = f.ReferedObjectDefinitionID;

            return field;
        }

        private static GeneralObjectDefinitionField ParseGeneralObjectDefinitionField(ObjectDefinitionField f, GeneralObjectDefinitionField field)
        {
            if (field == null)
                field = new GeneralObjectDefinitionField();

            field.GeneralObjectDefinitionFieldKey = f.ObjectDefinitionFieldKey;
            field.GeneralObjectDefinitionFieldName = f.ObjectDefinitionFieldName;
            field.GeneralObjectDefinitionFiledType = f.GeneralObjectDefinitionFiledType;

            return field;
        }

        private static PrimeObjectDefintionField ParsePrimeDefinitionField(PrimeDefinitionField f, PrimeObjectDefintionField field)
        {
            if (field == null)
                field = new PrimeObjectDefintionField();

            field.GeneralObjectDefinitionFieldKey = f.ObjectDefinitionFieldKey;
            field.GeneralObjectDefinitionFieldName = f.ObjectDefinitionFieldName;
            field.GeneralObjectDefinitionFiledType = f.GeneralObjectDefinitionFiledType;
            field.PrimeDataType = f.PrimeDataType;

            return field;
        }


        private static ObjectDefinitionField ConvertObjectDefField2SvcDefField(GeneralObjectDefinitionField field)
        {
            switch (field.GeneralObjectDefinitionFiledType)
            {
                case GeneralObjectDefinitionFieldType.File:
                    return Convert2FileDefinitionField(field);
                case GeneralObjectDefinitionFieldType.ObjectReference:
                    return Convert2ObjectRefDefinitionField(field as ObjRefObjectDefinitionField);
                case GeneralObjectDefinitionFieldType.PrimeType:
                    return Convert2ObjectPrimeDefinitionField(field as PrimeObjectDefintionField);
                case GeneralObjectDefinitionFieldType.SingleSelection:
                    return Convert2ObjectSingleSelectionDefinitionField(field as SingleSelectionDefinitionField);
                default:
                    throw new NotSupportedException(string.Format("{0} is not supported.", field.GeneralObjectDefinitionFiledType));
            }
        }

        private static SingleSDefinitionField Convert2ObjectSingleSelectionDefinitionField(SingleSelectionDefinitionField field)
        {
            var svcDefField = new SingleSDefinitionField();

            svcDefField.GeneralObjectDefinitionFiledType = field.GeneralObjectDefinitionFiledType;
            svcDefField.ObjectDefinitionFieldID = field.GeneralObjectDefinitionFieldID;
            svcDefField.ObjectDefinitionFieldKey = field.GeneralObjectDefinitionFieldKey;
            svcDefField.ObjectDefinitionFieldName = field.GeneralObjectDefinitionFieldName;

            foreach (var si in field.SelectionItems)
            {
                svcDefField.SelectionItems.Add(new DefinitionSelectItem {
                     SelectDisplayName = si.SelectDisplayName,
                     SelectItemID = si.GeneralObjectDefinitionSelectItemID,
                     SelectItemKey = si.SelectItemKey
                });
            }

            return svcDefField;
        }

        private static PrimeDefinitionField Convert2ObjectPrimeDefinitionField(PrimeObjectDefintionField field)
        {
            var svcDefField = new PrimeDefinitionField();

            svcDefField.GeneralObjectDefinitionFiledType = field.GeneralObjectDefinitionFiledType;
            svcDefField.ObjectDefinitionFieldID = field.GeneralObjectDefinitionFieldID;
            svcDefField.ObjectDefinitionFieldKey = field.GeneralObjectDefinitionFieldKey;
            svcDefField.ObjectDefinitionFieldName = field.GeneralObjectDefinitionFieldName;

            svcDefField.PrimeDataType = field.PrimeDataType;

            return svcDefField;
        }

        private static ObjectDefinitionField Convert2ObjectRefDefinitionField(ObjRefObjectDefinitionField field)
        {
            var svcDefField = new ObjRefDefinitionField();

            svcDefField.GeneralObjectDefinitionFiledType = field.GeneralObjectDefinitionFiledType;
            svcDefField.ObjectDefinitionFieldID = field.GeneralObjectDefinitionFieldID;
            svcDefField.ObjectDefinitionFieldKey = field.GeneralObjectDefinitionFieldKey;
            svcDefField.ObjectDefinitionFieldName = field.GeneralObjectDefinitionFieldName;
            svcDefField.ReferedObjectDefinitionID = field.ReferedObjectDefinitionID;

            return svcDefField;
        }

        private static ObjectDefinitionField Convert2FileDefinitionField(GeneralObjectDefinitionField field)
        {
            var svcDefField = new ObjectDefinitionField(GeneralObjectDefinitionFieldType.File);
            svcDefField.GeneralObjectDefinitionFiledType = field.GeneralObjectDefinitionFiledType;
            svcDefField.ObjectDefinitionFieldID = field.GeneralObjectDefinitionFieldID;
            svcDefField.ObjectDefinitionFieldKey = field.GeneralObjectDefinitionFieldKey;
            svcDefField.ObjectDefinitionFieldName = field.GeneralObjectDefinitionFieldName;

            return svcDefField;
        }

        /// <summary>
        /// Convert GeneralObjectDefinition list to ObjectDefintion list
        /// </summary>
        /// <param name="objDefList"></param>
        /// <returns></returns>
        public static List<ObjectDefinition> ConvertToObjectDefinitionList(List<GeneralObjectDefinition> objDefList)
        {
            List<ObjectDefinition> retObjList = new List<ObjectDefinition>();

            foreach (var objDef in objDefList)
            {
                ObjectDefinition def = new ObjectDefinition();
                def.ObjectDefinitionID = objDef.GeneralObjectDefinitionID;
                def.ObjectDefinitionKey = objDef.GeneralObjectDefinitionKey;
                def.ObjectDefinitionName = objDef.GeneralObjectDefinitionName;
                def.ObjectOwner = objDef.ObjectOwner;
                def.UpdatedBy = objDef.UpdatedBy;
                def.Created = objDef.Created;
                def.Updated = objDef.Updated;
                def.ObjectDefinitionGroupID = objDef.GeneralObjectDefinitionGroupID;

                foreach (var field in objDef.GeneralObjectDefinitionFields)
                {
                    def.ObjectFields.Add(ConvertObjectDefField2SvcDefField(field));
                }

                retObjList.Add(def);
            }

            return retObjList;
        }

        public static List<ServiceObject> ConvertToServiceObjectList(List<GeneralObject> objectList, string[] properties)
        {
            List<ServiceObject> svsObjects = new List<ServiceObject>();
            foreach (var obj in objectList)
            {
                ServiceObject sObject = new ServiceObject();
                sObject.ObjectID = obj.GeneralObjectID;
                sObject.ObjectName = obj.GeneralObjectName;
                sObject.ObjectOwner = obj.ObjectOwner;
                sObject.Updated = obj.Updated;
                sObject.Created = obj.Created;
                sObject.CreatedBy = obj.CreatedBy;
                sObject.UpdatedBy = obj.UpdatedBy;
                sObject.ObjectDefinitionId = obj.GeneralObjectDefinitionID;

                if (properties != null)
                {
                    foreach (var objField in obj.GeneralObjectFields)
                    {
                        if (!properties.Contains(objField.GeneralObjectDefinitionField.GeneralObjectDefinitionFieldName, new IgnoreCaseStringCompare<string>()))
                            continue;

                        ObjectKeyValuePair kvPair = new ObjectKeyValuePair();
                        kvPair.KeyName = objField.GeneralObjectDefinitionField.GeneralObjectDefinitionFieldName;
                        kvPair.Value = ObjectConverter.EntityField2ServiceField(objField);
                        sObject.Properties.Add(kvPair);
                    }
                }

                svsObjects.Add(sObject);
            }

            return svsObjects;
        }


        public static object EntityField2ServiceField(GeneralObjectField field)
        {
            switch (field.GeneralObjectDefinitionField.GeneralObjectDefinitionFiledType)
            {
                case GeneralObjectDefinitionFieldType.File:
                    return EntityFileField2ServiceFileField(field as FileGeneralObjectField);
                case GeneralObjectDefinitionFieldType.ObjectReference:
                    return EntityRefField2ServiceRefField(field as GeneralObjectReferenceField);
                case GeneralObjectDefinitionFieldType.PrimeType:
                    return EntityPrimeField2ServicePrimeField(field as PrimeGeneralObjectField);
                case GeneralObjectDefinitionFieldType.SingleSelection:
                    return EntitySingleSField2ServiceSingleSField(field as SingleSelectionGeneralObjectField);
                default:
                    throw new NotSupportedException(string.Format("{0} is not supported by the system.", field.GeneralObjectDefinitionField.GeneralObjectDefinitionFiledType));
            }
        }

        public static SingleSelectionField EntitySingleSField2ServiceSingleSField(SingleSelectionGeneralObjectField selectionField)
        {
            SingleSelectionField field = new SingleSelectionField();
            field.ObjectFieldID = selectionField.GeneralObjectFieldID;
            field.SelectedItemID = selectionField.SelectedItemID;

            return field;
        }

        public static PrimeObjectField EntityPrimeField2ServicePrimeField(PrimeGeneralObjectField primeField)
        {
            PrimeObjectField pServiceField = new PrimeObjectField();

            pServiceField.ObjectFieldID = primeField.GeneralObjectFieldID;
            PrimeObjectDefintionField defineField = primeField.GeneralObjectDefinitionField as PrimeObjectDefintionField;

            //set the prime object data type.
            pServiceField.PrimeDataType = defineField.PrimeDataType;
            switch (defineField.PrimeDataType)
            {
                case PrimeFieldDataType.String:
                    pServiceField.Value = primeField.StringValue;
                    break;
                case PrimeFieldDataType.Binary:
                    pServiceField.Value = primeField.BinaryValue;
                    break;
                case PrimeFieldDataType.Integer:
                    pServiceField.Value = primeField.IntegerValue;
                    break;
                case PrimeFieldDataType.Long:
                    pServiceField.Value = primeField.LongValue;
                    break;
                case PrimeFieldDataType.Number:
                    pServiceField.Value = primeField.NumberValue;
                    break;
                case PrimeFieldDataType.Datetime:
                    pServiceField.Value = primeField.DateTimeValue;
                    break;
                default:
                    throw new NotSupportedException("Can not convert type " + defineField.PrimeDataType.ToString());
            }

            return pServiceField;
        }

        public static ObjectReferenceField EntityRefField2ServiceRefField(GeneralObjectReferenceField refField)
        {
            ObjectReferenceField objRefField = new ObjectReferenceField();
            objRefField.ReferedGeneralObjectID = refField.ReferedGeneralObjectID;
            objRefField.ObjectFieldID = refField.GeneralObjectFieldID;

            return objRefField;
        }

        public static ObjectFileField EntityFileField2ServiceFileField(FileGeneralObjectField fileField)
        {
            ObjectFileField fServiceField = new ObjectFileField();

            fServiceField.ObjectFieldID = fileField.GeneralObjectFieldID;
            fServiceField.FileCRC = fileField.FileCRC;
            fServiceField.Created = fileField.Created;
            fServiceField.FileExtension = fileField.FileExtension;
            fServiceField.FileFullPath = fileField.FileFullPath;
            fServiceField.FileName = fileField.FileName;
            fServiceField.FileSize = fileField.FileSize;
            fServiceField.FileUrl = fileField.FileUrl;
            fServiceField.Updated = fileField.Updated;

            return fServiceField;
        }

        public static List<ObjectDefinitionGroup> Convert2ObjectDefinitionGroupList(List<GeneralObjectDefinitionGroup> objDefGroupList)
        {
            List<ObjectDefinitionGroup> objList = new List<ObjectDefinitionGroup>();
            foreach (var objdef in objDefGroupList)
            {
                var sObjDef = ConvertEntityOGroup2ServiceOGroup(objdef);
                objList.Add(sObjDef);
            }

            return objList;
        }

        private static ObjectDefinitionGroup ConvertEntityOGroup2ServiceOGroup(GeneralObjectDefinitionGroup objdef)
        {
            ObjectDefinitionGroup objGroup = Convert2ObjectDefinitionGroup(objdef);

            if (objdef.ParentGroup != null)
                objGroup.ParentGroup = Convert2ObjectDefinitionGroup(objdef.ParentGroup);

            if (objdef.ChildrenGroups != null)
            {
                foreach (var childdef in objdef.ChildrenGroups)
                {
                    objGroup.ChidrenGroups.Add(Convert2ObjectDefinitionGroup(childdef));
                }
            }

            return objGroup;
        }

        private static ObjectDefinitionGroup Convert2ObjectDefinitionGroup(GeneralObjectDefinitionGroup objdef)
        {
            ObjectDefinitionGroup objGroup = new ObjectDefinitionGroup();
            objGroup.GroupID = objdef.GeneralObjectDefinitionGroupID;
            objGroup.GroupKey = objdef.GeneralObjectDefinitionGroupKey;
            objGroup.GroupName = objdef.GeneralObjectDefinitionGroupName;

            return objGroup;
        }

    }
}
