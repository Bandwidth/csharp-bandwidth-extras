#!/bin/bash
rm -rf bin
dotnet restore
dotnet pack -c Release --include-symbols
dotnet nuget push bin/Release/*.nupkg -s https://api.nuget.org/v3/index.json -k $NUGET_API_KEY
