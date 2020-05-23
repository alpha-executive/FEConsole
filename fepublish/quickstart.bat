@echo off
pushd .
echo "starting feconsle admin..."
cd feconsoleadmin
start /B dotnet FE.Creator.Admin.dll --urls="https://*:9080"
popd

pushd .
echo "starting feconsle api..."
cd feconsoleapi
start /B dotnet FE.Creator.FEConsoleAPI.dll --urls="https://*:5001"
popd

pushd .
echo "starting feconsle portal..."
cd feconsoleportal
start /B dotnet FE.Creator.FEConsolePortal.dll --urls="https://*"
popd

pushd .
echo "starting feconsle identity server..."
cd feidentityserver
start /B dotnet FE.Creator.IdentityServer.dll --urls="https://*:5002"
popd

start https://localhost:9080

pause