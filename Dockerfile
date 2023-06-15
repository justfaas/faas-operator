# Build the operator
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0 as build
ARG TARGETARCH
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /operator

RUN apt update && apt install libxml2-utils -y

# restore dependencies
COPY ./src/faas-operator.csproj ./
RUN --mount=type=secret,id=GITHUB_TOKEN \
    dotnet nuget add source \
        --store-password-in-clear-text \
        -n justfaas \
        -u justfaas \
        -p $(cat /run/secrets/GITHUB_TOKEN) \
        https://nuget.pkg.github.com/justfaas/index.json
RUN dotnet restore -a $TARGETARCH

COPY ./src/. ./
RUN dotnet publish -c release -a $TARGETARCH -o dist faas-operator.csproj

# The runner for the application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as final

RUN addgroup k8s-operator && useradd -G k8s-operator operator-user

WORKDIR /operator
COPY --from=build /operator/dist/ ./
RUN chown operator-user:k8s-operator -R .

USER operator-user

ENTRYPOINT [ "dotnet", "faas-operator.dll" ]
