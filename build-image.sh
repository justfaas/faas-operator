#!/bin/bash

temp=$(mktemp "${TMPDIR:-/tmp/}$(basename $0).XXXXXXXXXXXX")

echo $GITHUB_TOKEN > $temp

if [ -z "$1" ]; then
    echo "Tag not supplied. Image won't be published."

    docker buildx build --secret id=GITHUB_TOKEN,src=$temp --platform linux/amd64,linux/arm64 -t faas-operator .
else
    docker buildx build --secret id=GITHUB_TOKEN,src=$temp --push --platform linux/amd64,linux/arm64 -t goncalooliveira/faas-operator:$1 .
fi
