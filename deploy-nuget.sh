#!/bin/bash
set -e # Exit with nonzero exit code if anything fails

if [ -z "$TRAVIS_TAG" ]; then
    echo "Skipping deploy to nuget; just doing a build."
    exit 0
fi  

cd Bandwidth.Net.Extra
rm -rf bin
rm -rf obj

VERSION=$TRAVIS_TAG

# Pack nuget module and publish it
docker run -i -t --rm -v $PWD:/src -w /src microsoft/dotnet:1.1-sdk dotnet pack -c Release --include-symbols
docker run -i -t --rm -v $PWD:/src -w /src microsoft/dotnet:1.1-sdk dotnet nuget push bin/Release/*.nupkg -s https://api.nuget.org/v3/index.json -k %NUGET_API_KEY%

