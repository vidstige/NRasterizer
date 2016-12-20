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

cd NOpenType
if not "%GitVersion_NuGetVersion%" == "" (
    ECHO Setting version number to "%GitVersion_NuGetVersion%"
    REM run only if gitversion has ran i.e. from appveyor
    dotnet version "%GitVersion_NuGetVersion%"
    if not "%errorlevel%"=="0" goto failure
)
cd ../NOpenType


ECHO Building nuget packages
dotnet pack -c Release
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