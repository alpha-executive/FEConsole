using System;
using System.Collections.Generic;
using System.Linq;
using FE.Creator.ObjectRepository;
using FE.Creator.ObjectRepository.EntityModels;
using FE.Creator.ObjectRepository.ServiceModels;
using Xunit;

namespace xtest {
    public class ObjectRepositoryTest {
        [Fact]
        public void CreateObjectDefGroupTest () {
            IObjectService service = new DefaultObjectService ();

            var objDefinitionGroups = service.GetObjectDefinitionGroups (null, null);
            var defaultGroup = objDefinitionGroups
                                        .Where(g=>g.GroupKey.Equals("GROUP_SERVER_OBJECT"))
                                        .FirstOrDefault();

            if (defaultGroup == null) {
                service.CreateOrUpdateObjectDefinitionGroup (new ObjectDefinitionGroup () {
                    GroupKey = "GROUP_SERVER_OBJECT",
                        GroupName = "SERVER OBJECT",
                });
            }

            Assert.True (service.GetObjectDefinitionGroups (null, null).Count == 1);
        }

        [Fact]
        public void CreateObjectDefinitionTest()
        {
            IObjectService service = new DefaultObjectService();

            ObjectDefinition definition = new ObjectDefinition();
            definition.IsFeildsUpdateOnly = false;
            definition.ObjectDefinitionKey = "PERSON_TEST";
            definition.ObjectDefinitionGroupID = service.GetObjectDefinitionGroups(null, null)[0].GroupID;
            definition.ObjectDefinitionName = "Person";
            definition.ObjectOwner = "Admin";
            definition.UpdatedBy = "Admin";

            //Person name
            definition.ObjectFields.Add(new PrimeDefinitionField()
            {
                ObjectDefinitionFieldKey = "PERSON_NAME",
                ObjectDefinitionFieldName = "Person Name",
                PrimeDataType = PrimeFieldDataType.String
            });


            SingleSDefinitionField selectionField = new SingleSDefinitionField()
            {
                ObjectDefinitionFieldKey = "PERSON_SEX",
                ObjectDefinitionFieldName = "Person Sex"
            };
            selectionField.SelectionItems.Add(new DefinitionSelectItem()
            {
                SelectDisplayName = "Male",
                SelectItemKey = "Male"
            });
            selectionField.SelectionItems.Add(
                     new DefinitionSelectItem()
                     {
                         SelectItemKey = "Female",
                         SelectDisplayName = "Female"
                     });

            //Person Sex
            definition.ObjectFields.Add(selectionField);

            //Person Age
            definition.ObjectFields.Add(new PrimeDefinitionField()
            {
                ObjectDefinitionFieldKey = "PERSON_AGE",
                ObjectDefinitionFieldName = "Person AGE",
                PrimeDataType = PrimeFieldDataType.Integer
            });

            //person Image
            definition.ObjectFields.Add(new ObjectDefinitionField(GeneralObjectDefinitionFieldType.File)
            {
                GeneralObjectDefinitionFiledType = GeneralObjectDefinitionFieldType.File,
                ObjectDefinitionFieldKey = "PERSON_IMAGE",
                ObjectDefinitionFieldName = "Person Image"
            });

            //Manager
            definition.ObjectFields.Add(new ObjRefDefinitionField()
            {
                ObjectDefinitionFieldKey = "PERSON_MANAGER",
                ObjectDefinitionFieldName = "Person Manager",
                ReferedObjectDefinitionID = 1
            });

            var objDef = service.GetAllObjectDefinitions()
                            .Where(od=>od.ObjectDefinitionName.Equals("Person"))
                            .FirstOrDefault();
            //avoid to insert the duplicated value.
            if(objDef != null)
                definition.ObjectDefinitionID = objDef.ObjectDefinitionID;

            int objdefintionId = service.CreateORUpdateObjectDefinition(definition);
            Assert.True(objdefintionId > 0);
        }

        [Fact]
         public void GetObjectDefinitionByGroupTest()
        {
            IObjectService service = new DefaultObjectService();
            var objectGroup = service.GetObjectDefinitionGroups(null, null)[0];
            var objectDefinition = service.GetObjectDefinitionsByGroup(objectGroup.GroupID, 1, 10, null);

            Assert.True(objectDefinition.Count > 0);
        }

        [Fact]
        public void CreateObjectTest()
        {
            IObjectService service = new DefaultObjectService();
            ServiceObject svObject = new ServiceObject();
            svObject.ObjectName = "Peter";
            svObject.ObjectOwner = "Admin";
            svObject.OnlyUpdateProperties = false;
            svObject.UpdatedBy = "Admin";
            svObject.CreatedBy = "Admin";

            var objDef = service.GetAllObjectDefinitions()
                            .Where(od=>od.ObjectDefinitionName.Equals("Person"))
                            .FirstOrDefault();
                            
            if(objDef != null){
                svObject.ObjectDefinitionId = objDef.ObjectDefinitionID;
            }
            

             Console.WriteLine("ObjectDefintionID = " + svObject.ObjectDefinitionId);
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "Person Name",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = "Peter, Robert"
                }
            });
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "Person Sex",
                Value = new SingleSelectionField()
                {
                    SelectedItemID = 5
                }
            });
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "Person AGE",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.Integer,
                    Value = 30
                }
            });
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "Person Image",
                Value = new ObjectFileField()
                {
                    FileCRC = "10001001",
                    FileExtension = ".docx",
                    FileFullPath = "c:\\location.docx",
                    FileName = "location.docx",
                    FileUrl = "http://www.url.com",
                    FileSize = 10,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                }
            });

            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "Person Manager",
                Value = new ObjectReferenceField()
                {
                    ReferedGeneralObjectID = 1
                }
            });


            var objects = service.GetAllSerivceObjects(service.GetAllObjectDefinitions()[0].ObjectDefinitionID, new string[] {
                   "Person Name",
                   "Person Sex",
                   "Person AGE",
                   "Person Image",
                   "Person Manager"
            }, null);

            var currObj = objects.Where(o=>o.ObjectName.Equals("Peter")).FirstOrDefault();

            if(currObj != null){
                int objId = service.CreateORUpdateGeneralObject(svObject);
            }

            Assert.True(service.GetGeneralObjectCount(service.GetAllObjectDefinitions()[0].ObjectDefinitionID, null) >= 1);
        }

         [Fact]
        public void GetObjectTest()
        {
            IObjectService service = new DefaultObjectService();
            var objects = service.GetAllSerivceObjects(service.GetAllObjectDefinitions()[0].ObjectDefinitionID, new string[] {
                   "Person Name",
                   "Person Sex",
                   "Person AGE",
                   "Person Image",
                   "Person Manager"
            }, null);

            var obj = objects[0];
            
            Assert.Equal(obj.ObjectName, "Peter");
            Assert.Equal(obj.GetPropertyValue<PrimeObjectField>("Person Name").GetStrongTypeValue<string>(), "Peter, Robert");
            Assert.Equal(obj.GetPropertyValue<SingleSelectionField>("Person Sex").SelectedItemID, 5);
            Assert.Equal(obj.GetPropertyValue<PrimeObjectField>("Person AGE").GetStrongTypeValue<int>(), 30);
            Assert.Equal(obj.GetPropertyValue<ObjectFileField>("Person Image").FileName, "location.docx");
            Assert.Equal(obj.GetPropertyValue<ObjectReferenceField>("Person Manager").ReferedGeneralObjectID, 1);


            obj = service.GetServiceObjectById(obj.ObjectID, new string[] {
                   "Person Name",
                   "Person Sex",
                   "Person Manager"
            }, null);
            Assert.Equal(obj.ObjectName, "Peter");
            Assert.Equal(obj.GetPropertyValue<PrimeObjectField>("Person Name").GetStrongTypeValue<string>(), "Peter, Robert");
            Assert.Equal(obj.GetPropertyValue<SingleSelectionField>("Person Sex").SelectedItemID, 5);
            Assert.Equal(obj.GetPropertyValue<ObjectReferenceField>("Person Manager").ReferedGeneralObjectID, 1);


            objects = service.GetServiceObjects(obj.ObjectDefinitionId, new string[] {
                   "Person Name",
                   "Person Sex",
                   "Person Manager"
            }, 1, 10, null);
            obj = objects[0];
            Assert.Equal(obj.ObjectName, "Peter");
            Assert.Equal(obj.GetPropertyValue<PrimeObjectField>("Person Name").GetStrongTypeValue<string>(), "Peter, Robert");
            Assert.Equal(obj.GetPropertyValue<SingleSelectionField>("Person Sex").SelectedItemID, 5);
            Assert.Equal(obj.GetPropertyValue<ObjectReferenceField>("Person Manager").ReferedGeneralObjectID, 1);
        }

      /*  [Fact]
         public void UpdateObjectTest()
        {
            IObjectService service = new DefaultObjectService();
            ServiceObject svObject = service.GetAllSerivceObjects(service.GetAllObjectDefinitions()[0].ObjectDefinitionID, null, null)[0];
            svObject.OnlyUpdateProperties = true;

            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "Person Name",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = "Peter, Robert - Updated"
                }
            });

            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "Person Image",
                Value = new ObjectFileField()
                {
                    FileCRC = "10001001",
                    FileExtension = ".docx",
                    FileFullPath = "c:\\location-update.docx",
                    FileName = "location-update.docx",
                    FileUrl = "http://www.url.com",
                    FileSize = 10,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                }
            });

            int objectId = service.CreateORUpdateGeneralObject(svObject);
            var serviceobject = service.GetServiceObjectById(objectId, new string[]
             {
                "Person Name",
                "Person Image"
             }, null);

            Assert.Equal(serviceobject.GetPropertyValue<PrimeObjectField>("Person Name").GetStrongTypeValue<string>(), "Peter, Robert - Updated");
            Assert.Equal(serviceobject.GetPropertyValue<ObjectFileField>("Person Image").FileName, "location-update.docx");
        }

        [Fact]
          public void SoftDeleteObjectTest()
        {
            IObjectService service = new DefaultObjectService();
            ServiceObject svObject = service.GetAllSerivceObjects(service.GetAllObjectDefinitions()[0].ObjectDefinitionID, null, null)[0];
            int objectId = service.SoftDeleteServiceObject(svObject.ObjectID, "Tester");
            Assert.Equal(objectId, svObject.ObjectID);
            Assert.Equal(service.GetGeneralObjectCount(svObject.ObjectDefinitionId, null), 0);
        }

        [Fact]
        public void DeleteObjectFieldTest()
        {
            IObjectService service = new DefaultObjectService();

            var objects = service.GetAllSerivceObjects(service.GetAllObjectDefinitions()[0].ObjectDefinitionID, new string[] {
                   "Person Name"
            }, null);

            var obj = objects[0];
            Assert.NotNull(obj.GetPropertyValue<PrimeObjectField>("Person Name"));
            service.DeleteObjectField(obj.GetPropertyValue<PrimeObjectField>("Person Name").ObjectFieldID);
        }

        [Fact]
        public void DeleteObjectFieldDefinitionTest()
        {
            IObjectService service = new DefaultObjectService();

            var objects = service.GetAllSerivceObjects(service.GetAllObjectDefinitions()[0].ObjectDefinitionID, new string[] {
                   "Person Image"
            }, null);

            var obj = objects[0];
            Assert.NotNull(obj.GetPropertyValue<ObjectFileField>("Person Image"));

            service.DeleteObjectDefinitionField(4);
        }

        [Fact]
        public void DeleteObjectDefinitionTest()
        {
            IObjectService service = new DefaultObjectService();

            int definitionId = service.GetAllObjectDefinitions()[0].ObjectDefinitionID;
            var objects = service.GetAllSerivceObjects(definitionId, new string[] {
                   "Person Sex"
            }, null);

            var obj = objects[0];
            Assert.NotNull(obj.GetPropertyValue<SingleSelectionField>("Person Sex"));
            service.DeleteObjectDefinition(definitionId);
        } */

    }
}