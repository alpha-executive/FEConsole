#!/bin/bash
function startapp()
{
   appname=$1
   dllname=$2
   host=$3

   cd $appname

   dotnet $dllname &
   
   cd ..

   return 0
}

startapp "feidentityserver" "FE.Creator.IdentityServer.dll"
startapp "feconsoleapi" "FE.Creator.FEConsoleAPI.dll"
startapp "feconsoleadmin" "FE.Creator.Admin.dll"
startapp "feconsoleportal" "FE.Creator.FEConsolePortal.dll"

