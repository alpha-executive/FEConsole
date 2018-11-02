using System.ComponentModel.DataAnnotations;

namespace testconsole.EntityModels
{
    public class IMContactInfo
    {
        public int IMContactInfoId {get;set;}
       
        public string ProductName{get;set;}
        public string ProductProvider{get;set;}
        public string ContactAccount{get;set;}

         public int UserInfoId {get;set;}
         public UserInfo UserInfo {get;set;}
    }
}