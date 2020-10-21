set ASPNETCORE_ENVIRONMENT=HttpProd
@echo off
pushd .
echo "starting feconsle admin..."
cd feconsoleadmin
start /B dotnet FE.Creator.Admin.dll --urls="http://*:8090"
popd

pushd .
echo "starting feconsle api..."
cd feconsoleapi
start /B dotnet FE.Creator.FEConsoleAPI.dll --urls="http://*:8091"
popd

pushd .
echo "starting feconsle portal..."
cd feconsoleportal
start /B dotnet FE.Creator.FEConsolePortal.dll --urls="http://*:8093"
popd

pushd .
echo "starting feconsle identity server..."
cd feidentityserver
start /B dotnet FE.Creator.IdentityServer.dll --urls="http://*:8092"
popd

start http://localhost:8090

pause