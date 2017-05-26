#!/bin/bash
dotnet restore
cd Bandwidth.Net.Extra.Test
dotnet test
