using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using Microsoft.EntityFrameworkCore;

namespace testconsole
{
    using Microsoft.Extensions.Configuration;
    using testconsole.EntityModels;
    using testconsole.mathlib;
    class Program
    {
        static void VerifyDBConnection(string connectionStr)
        {
            try
            {
                using(SqlConnection conn = new SqlConnection(connectionStr))
                {
                    conn.Open();
                }

                Console.WriteLine("Connect to database");
            }
            catch (System.Exception)
            {                
                Console.WriteLine("Failed to connect to Database.");
            }
            
        }

        static void QuerySqlServerEntity()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            var options = new DbContextOptionsBuilder<UseInfoDBContext>()
                                .UseSqlServer(config.GetConnectionString("sqlServerDb"))
                                .Options;

            
            using(var db = new UseInfoDBContext(options)){
                db.Database.EnsureCreated();

                using(var transaction = db.Database.BeginTransaction()){
                foreach(var u in db.users){
                    db.Remove(u);
                }
                
                IMContactInfo contactInfo = new IMContactInfo();
                contactInfo.ProductName = "Wechat";
                contactInfo.ProductProvider = "Tencent";
                contactInfo.ContactAccount = "1234456";
                UserInfo user = new UserInfo();
                user.Email = "a@b.com";
                user.UserName = "test a";
                user.UserPassword = "abc";
                user.UserIMContacts = new System.Collections.Generic.List<IMContactInfo>();
                user.UserIMContacts.Add(contactInfo);

                db.users.Add(user);
                db.SaveChanges();

                foreach(var u in db.users){
                    DisplayUsers(u);
                }

                transaction.Commit();
              }
                
            }
        }

        static void DisplayUsers(UserInfo user){
            Console.WriteLine("========USER ===========");
            Console.WriteLine("UserName: " + user.UserName);
            Console.WriteLine("Email: " + user.Email);
        }
      
        static void Main(string[] args)
        {
            string sqlConStr = "Server=localhost;Database=xpressiondb;User Id=xpressionsa;Password=password;";


            Console.WriteLine(string.Format("{0} + {1} = {2}", 1, 1, Calculator.Sum(1, 1)));
            Console.WriteLine("Hello World!");

           int[] testArrays = new int[]{1,2,3,4,5};
           Span<int> tSpan = new Span<int>(testArrays, 2, 2);
           Console.WriteLine(string.Join(",", tSpan.ToArray()));

           VerifyDBConnection(sqlConStr);

           QuerySqlServerEntity();
        }
    }
}
