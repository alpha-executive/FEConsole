@echo off
echo "start build on windows"
dotnet clean
dotnet publish -c Release

echo "packing application..."
rmdir /S /Q fepublish

mkdir fepublish\windows
mkdir fepublish\windows\feconsoleadmin
xcopy /Y /S /Q "FE.Creator.Admin/bin/Release/netcoreapp3.1/publish" "fepublish\windows\feconsoleadmin\"

mkdir fepublish\windows\feconsoleapi
xcopy /Y /S /Q "FE.Creator.FEConsoleAPI/bin/Release/netcoreapp3.1/publish" "fepublish\windows\feconsoleapi\"

mkdir fepublish\windows\feconsoleportal
xcopy /Y /S /Q "FE.Creator.FEConsolePortal/bin/Release/netcoreapp3.1/publish" "fepublish\windows\feconsoleportal\"

mkdir fepublish\windows\feidentityserver
xcopy /Y /S /Q "FE.Creator.IdentityServer/bin/Release/netcoreapp3.1/publish" "fepublish\windows\feidentityserver\"

mkdir fepublish\appsettings
mkdir fepublish\appsettings\feconsoleadmin
mkdir fepublish\appsettings\feconsoleapi
mkdir fepublish\appsettings\feconsoleportal
mkdir fepublish\appsettings\feidentityserver

echo "setup appsettings..."
move fepublish\windows\feconsoleadmin\appsettings.*    "fepublish\appsettings\feconsoleadmin\"
move fepublish\windows\feconsoleapi\appsettings.*      "fepublish\appsettings\feconsoleapi\"
move fepublish\windows\feconsoleportal\appsettings.*   "fepublish\appsettings\feconsoleportal\"
move fepublish\windows\feidentityserver\appsettings.*  "fepublish\appsettings\feidentityserver\"

xcopy /Y /Q "fepublish\appsettings\feconsoleadmin\appsettings.LocalProd.json" fepublish\windows\feconsoleadmin\
xcopy /Y /Q "fepublish\appsettings\feconsoleapi\appsettings.LocalProd.json" fepublish\windows\feconsoleapi\
xcopy /Y /Q "fepublish\appsettings\feconsoleapi\appsettings.json" fepublish\windows\feconsoleapi\
xcopy /Y /Q "fepublish\appsettings\feconsoleportal\appsettings.LocalProd.json" fepublish\windows\feconsoleportal\
xcopy /Y /Q "fepublish\appsettings\feidentityserver\appsettings.LocalProd.json" fepublish\windows\feidentityserver\

echo "deploy database"
xcopy /Y /Q fetechhub_identityserver.db fepublish\windows\feidentityserver

echo "deploy certificate key"
REM xcopy /Y /Q "localhost.pfx" "fepublish\windows\feconsoleadmin"
REM xcopy /Y /Q "localhost.pfx" "fepublish\windows\feconsoleapi"
REM xcopy /Y /Q "localhost.pfx" "fepublish\windows\feconsoleportal"
REM xcopy /Y /Q "localhost.pfx" "fepublish\windows\feidentityserver"

echo "deploy startup scripts"
xcopy /Y /Q "windows-scripts\startfeconsoleadmin.bat" "fepublish\windows\feconsoleadmin"
xcopy /Y /Q "windows-scripts\startfeconsoleapi.bat" "fepublish\windows\feconsoleapi"
xcopy /Y /Q "windows-scripts\startfeconsoleportal.bat" "fepublish\windows\feconsoleportal"
xcopy /Y /Q "windows-scripts\startfeidentityserver.bat" "fepublish\windows\feidentityserver"
xcopy /Y /Q "windows-scripts\startall.bat" "fepublish\windows"
xcopy /Y /Q "windows-scripts\quickstart.bat" "fepublish\windows"
