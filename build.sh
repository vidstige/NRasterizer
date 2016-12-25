#!/bin/bash
set -e
dotnet restore **/project.json
dotnet build --configuration Release **/project.json
dotnet test NRasterizer.Tests/
