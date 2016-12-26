#!/bin/bash

echo Starting build

echo Restoring packages

nuget restore "./NRasterizer.sln"

dotnet restore "./NOpenType/"

echo Running msbuild
xbuild NRasterizer.sln /p:Configuration=Release

echo Running tests
nuget install NUnit.Console -Version 3.5.0 -OutputDirectory testrunner
mono ./testrunner/NUnit.ConsoleRunner.3.5.0/tools/nunit3-console.exe ./NRasterizer.Tests/bin/Release/NRasterizer.Tests.dll

echo Building nuget packages
if [ -z "$GitVersion_NuGetVersion" ];
then
    dotnet pack -c Release --version-suffix "local-build" ./NOpenType/project.json
else
    cd NOpenType
    echo Setting version number to "%GitVersion_NuGetVersion%"
    dotnet version "%GitVersion_NuGetVersion%"
    cd ../ 

    dotnet pack -c Release ./NOpenType/project.json
fi
