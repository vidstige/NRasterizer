#!/bin/bash
dotnet restore **/project.json
dotnet build **/project.json
dotnet test NOpenType.Test/
