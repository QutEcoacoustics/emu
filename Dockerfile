FROM mcr.microsoft.com/dotnet/nightly/sdk:8.0.100-preview.3 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./src/Emu/Emu.csproj ./src/Emu/Emu.csproj
RUN dotnet restore ./src/Emu/Emu.csproj

# Copy everything else and build
COPY ./ ./
# Mount git so the build can infer version number
RUN dotnet publish -r linux-x64 --self-contained -o publish ./src/Emu/Emu.csproj


FROM debian:buster-slim

RUN mkdir /emu

WORKDIR /emu
COPY --from=build-env /app/publish .

# Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_gcServer=1
# Set the invariant mode since icu-libs isn't included (see https://github.com/dotnet/announcements/issues/20)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

CMD ./emu