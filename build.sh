#!/bin/bash
set -e
dotnet restore **/project.json
dotnet build **/project.json
dotnet test NRasterizer.Tests/
