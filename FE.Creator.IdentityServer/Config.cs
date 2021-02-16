// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace FE.Creator.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };

        public static string SecretString(string secret)
        {
            var secretVal = (new Secret(secret.Sha256())).Value;
            return secretVal;
        }
        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("feconsoleapi", "FEConsole API")
            };
        public static IEnumerable<Client> Clients(IConfigurationSection section)
        {
            //the way to get a IdentityServer4 Secret
            //Console.WriteLine("password".Sha256());

            var sectionClients = section.GetChildren()
                                        .Select(client =>
                              {
                                  Client bindClient = new Client();
                                  client.Bind(bindClient);
                                  
                                  if (bindClient.AllowedGrantTypes.Contains(GrantType.AuthorizationCode))
                                  {
                                      bindClient.RefreshTokenExpiration = TokenExpiration.Sliding;
                                      bindClient.RefreshTokenUsage = TokenUsage.ReUse;
                                  }                                  
                                  return bindClient;
                              });

            return sectionClients.ToList<Client>();
        }

    }
}