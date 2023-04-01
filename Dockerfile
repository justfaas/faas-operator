# Build the operator
#FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0 as build
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/nightly/sdk:8.0-preview AS build
ARG TARGETARCH
WORKDIR /operator

RUN apt update && apt install libxml2-utils -y

COPY ./src/faas-operator.csproj ./
COPY add-nuget-config.sh /tmp/
RUN chmod u+x /tmp/add-nuget-config.sh
RUN --mount=type=secret,id=nuget.config /tmp/add-nuget-config.sh nuget.config \
    dotnet restore -a $TARGETARCH

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
