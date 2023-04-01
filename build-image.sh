#!/bin/bash

if [ -z "$1" ]; then
    echo "Tag not supplied. Image won't be published."

    docker buildx build --secret id=nuget.config,src=$HOME/.nuget/NuGet/NuGet.Config --platform linux/amd64,linux/arm64 -t faas-operator .
else
    docker buildx build --secret id=nuget.config,src=$HOME/.nuget/NuGet/NuGet.Config --push --platform linux/amd64,linux/arm64 -t goncalooliveira/faas-operator:$1 .
fi
