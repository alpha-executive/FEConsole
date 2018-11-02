using System.Collections;
using System.Collections.Generic;

namespace testconsole.EntityModels
{
    public class UserInfo
    {
        public int UserInfoId{get;set;}
        public string UserName{get;set;}

        public string UserPassword{get;set;}

        public string Email{get;set;}
        
        public List<IMContactInfo> UserIMContacts{get;set;}
    }
}