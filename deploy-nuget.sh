#!/bin/bash
set -e # Exit with nonzero exit code if anything fails

if [ -z "$TRAVIS_TAG" ]; then
    echo "Skipping deploy to nuget; just doing a build."
    exit 0
fi  

# Pack nuget module and publish it
docker run -i -t --rm -v $PWD:/src -e VERSION=$TRAVIS_TAG -e NUGET_API_KEY=$NUGET_API_KEY -w /src/Bandwidth.Net.Extra microsoft/dotnet:1.1-sdk bash ../run-nuget.sh

