FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./src/MetadataUtility/MetadataUtility.csproj ./src/MetadataUtility/MetadataUtility.csproj
RUN dotnet restore ./src/MetadataUtility/MetadataUtility.csproj

# Copy everything else and build
COPY ./ ./
RUN dotnet publish -r linux-x64 --self-contained -o publish ./src/MetadataUtility/MetadataUtility.csproj


FROM debian:buster-slim

RUN mkdir /emu

WORKDIR /emu
COPY --from=build-env /app/publish .

# Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true 
# Set the invariant mode since icu-libs isn't included (see https://github.com/dotnet/announcements/issues/20)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

ENTRYPOINT ./emu