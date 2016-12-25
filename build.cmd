@echo Off
REM dotnet restore **/project.json
REM dotnet build **/project.json
REM dotnet test NRasterizer.Tests/

REM No glob support on Windows
dotnet restore NOpenType/project.json
dotnet restore NRasterizer.Tests/project.json
dotnet build --configuration Release NOpenType/project.json
dotnet build --configuration Release NRasterizer.Tests/project.json
dotnet test NRasterizer.Tests/

REM run only if gitversion has ran i.e. from appveyor    
 if not "%GitVersion_NuGetVersion%" == "" (
     cd NOpenType
     ECHO Setting version number to "%GitVersion_NuGetVersion%"
     dotnet version "%GitVersion_NuGetVersion%"
     cd ../
     if not "%errorlevel%"=="0" goto failure
 )

ECHO Building nuget packages
if not "%GitVersion_NuGetVersion%" == "" (
	dotnet pack -c Release ./NOpenType/project.json
)ELSE ( 
	dotnet pack -c Release --version-suffix "local-build" ./NOpenType/project.json
)
if not "%errorlevel%"=="0" goto failure

:success
ECHO successfully built project
REM exit 0
goto end

:failure
ECHO failed to build.
REM exit -1
goto end

:end
