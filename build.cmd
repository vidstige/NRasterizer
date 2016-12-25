@echo Off
ECHO Starting build

ECHO Restoring packages
nuget restore NRasterizer.sln
if not "%errorlevel%"=="0" goto failure

dotnet restore ./NOpenType/
if not "%errorlevel%"=="0" goto failure

ECHO Running msbuild
msbuild NRasterizer.sln /p:Configuration=Release
if not "%errorlevel%"=="0" goto failure

REM run only if gitversion has ran i.e. from appveyor    
if not "%GitVersion_NuGetVersion%" == "" (
    cd NOpenType
    ECHO Setting version number to "%GitVersion_NuGetVersion%"
    dotnet version "%GitVersion_NuGetVersion%"
    cd ../
    if not "%errorlevel%"=="0" goto failure
)


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
