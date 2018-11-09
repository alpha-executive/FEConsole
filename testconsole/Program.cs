using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Polly;
using testconsole.EntityModels;

namespace testconsole {
    using Microsoft.Extensions.Configuration;
    using testconsole.EntityModels;
    using testconsole.mathlib;
    class Program {
        static readonly ILoggerFactory DBLoggerFactory = new LoggerFactory (
            new [] {
                new ConsoleLoggerProvider ((category, level) => level == LogLevel.Information, true)
            }
        );
        static void VerifyDBConnection (string connectionStr) {
            try {
                using (SqlConnection conn = new SqlConnection (connectionStr)) {
                    conn.Open ();
                }

                Console.WriteLine ("Connect to database");
            } catch (System.Exception) {
                Console.WriteLine ("Failed to connect to Database.");
            }

        }

        static IConfiguration GetConfigure () {
            var config = new ConfigurationBuilder ()
                .AddJsonFile ("appsettings.json")
                .Build ();

            return config;
        }

        static void ExecuteWithDBContext (Action<UserInfoDBContext> command) {

            var config = GetConfigure ();

            var options = new DbContextOptionsBuilder<UserInfoDBContext> ()
                .UseLoggerFactory (DBLoggerFactory)
                .UseSqlServer (config.GetConnectionString ("sqlServerDb"))
                .Options;
            using (UserInfoDBContext context = new UserInfoDBContext (options)) {
                command (context);
            }
        }

        static void QueryWithRawSql () {
            var config = GetConfigure ();

            var options = new DbContextOptionsBuilder<UserInfoDBContext> ()
                .UseSqlServer (config.GetConnectionString ("sqlServerDb"))
                .Options;

            using (var context = new UserInfoDBContext (options)) {
                var users = context
                    .users
                    .FromSql ("SELECT * FROM dbo.users")
                    .ToList ();

                foreach (var u in users) {
                    DisplayUsers (u);
                }
            }
        }

        static void ConflictRecordRetry () {
            ExecuteWithDBContext (db => {
                var user = db.users.First ();

                db.Database.ExecuteSqlCommand ("Update users set UserName = 'Udate With SQL' where UserInfoId = '" + user.UserInfoId + "'");
                Policy
                    .Handle<DbUpdateConcurrencyException> ()
                    .Retry (1, (e, i) => {
                        foreach (var et in ((DbUpdateConcurrencyException) e).Entries) {
                            if (et.Entity is UserInfo) {
                                var proposedValues = et.CurrentValues;
                                var databaseValues = et.GetDatabaseValues ();

                                foreach (var property in proposedValues.Properties) {
                                    var proposedValue = proposedValues[property];
                                    var databaseValue = databaseValues[property];

                                    if (property.Name.Equals ("UserName")
                                            && !proposedValue.Equals("Update With Entity"))
                                        proposedValues[property] = "Update With Entity";
                                }

                                et.OriginalValues.SetValues (databaseValues);
                            }
                        }
                    })
                    .Execute (ct => {
                        user.UserName = "Update With Entity";
                        db.SaveChanges ();
                    }, CancellationToken.None);
            });
        }
        static async Task UpdateUserAsync () {
            var config = GetConfigure ();
            var options = new DbContextOptionsBuilder<UserInfoDBContext> ()
                .UseSqlServer (config.GetConnectionString ("sqlServerDb"))
                .Options;

            using (var db = new UserInfoDBContext (options)) {
                var user = db.users.First ();
                user.UserName = "Async Update";
                await db.SaveChangesAsync ();
            }
        }

        static void CreateUsers () {

            ExecuteWithDBContext (db => {

                db.Database.EnsureCreated ();

                using (var transaction = db.Database.BeginTransaction ()) {
                    foreach (var u in db.users) {
                        db.Remove (u);
                    }

                    IMContactInfo contactInfo = new IMContactInfo ();
                    contactInfo.ProductName = "Wechat";
                    contactInfo.ProductProvider = "Tencent";
                    contactInfo.ContactAccount = "1234456";
                    UserInfo user = new UserInfo ();
                    user.Email = "a@b.com";
                    user.UserName = "test a";
                    user.UserPassword = "abc";
                    user.UserIMContacts = new System.Collections.Generic.List<IMContactInfo> ();
                    user.UserIMContacts.Add (contactInfo);

                    db.users.Add (user);
                    db.SaveChanges ();

                    transaction.Commit ();
                }
            });
        }

        static void QuerySqlServerEntity () {
            ExecuteWithDBContext (db => {
                var users = db.users
                    .AsNoTracking ()
                    .ToList ();
                foreach (var u in users) {
                    DisplayUsers (u);
                }
            });
        }

        static void ConnectionResiliency () {
            ExecuteWithDBContext (db => {
                var resiliencyExecutor = db.Database.CreateExecutionStrategy ();

                resiliencyExecutor.Execute (() => {
                    using (var context = new UserInfoDBContext ()) {
                        using (var transaction = context.Database.BeginTransaction ()) {
                            context.users.Add (new UserInfo () {
                                Email = "a@b.c",
                                    UserName = "Test A",
                                    UserPassword = "abc"
                            });

                            context.users.Add (new UserInfo () {
                                Email = "x@y.k",
                                    UserName = "Test B",
                                    UserPassword = "abc"
                            });

                            context.SaveChanges ();
                            transaction.Commit ();
                        }
                    }
                });
            });

        }

        static void DisplayUsers (UserInfo user) {
            Console.WriteLine ("========USER ===========");
            Console.WriteLine ("UserName: " + user.UserName);
            Console.WriteLine ("Email: " + user.Email);
        }

        static void Main (string[] args) {
            string sqlConStr = "Server=localhost;Database=xpressiondb;User Id=xpressionsa;Password=password;";

            Console.WriteLine (string.Format ("{0} + {1} = {2}", 1, 1, Calculator.Sum (1, 1)));
            Console.WriteLine ("Hello World!");
            Console.WriteLine ("SqlConnection: {sqlConStr}");

            int[] testArrays = new int[] { 1, 2, 3, 4, 5 };
            Span<int> tSpan = new Span<int> (testArrays, 2, 2);
            Console.WriteLine (string.Join (",", tSpan.ToArray ()));

            VerifyDBConnection (sqlConStr);

            CreateUsers ();
            QuerySqlServerEntity ();

            //  QueryWithRawSql();

            ConflictRecordRetry ();
            Console.WriteLine ("Users after the save conflict resolved:");
            QuerySqlServerEntity ();

            UpdateUserAsync ().Wait ();
            Console.WriteLine ("Users after the Async update done: ");
            QuerySqlServerEntity ();

            ConnectionResiliency ();
            Console.WriteLine ("Users after the Transaction Resilency:");
            QuerySqlServerEntity ();
        }
    }
}