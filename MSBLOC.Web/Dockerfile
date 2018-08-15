FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY MSBLOC.Web/MSBLOC.Web.csproj MSBLOC.Web/
COPY MSBLOC.Core/MSBLOC.Core.csproj MSBLOC.Core/
COPY MSBLOC.Infrastructure/MSBLOC.Infrastructure.csproj MSBLOC.Infrastructure/
RUN dotnet restore MSBLOC.Web/MSBLOC.Web.csproj
COPY . .
WORKDIR /src/MSBLOC.Web
RUN dotnet build MSBLOC.Web.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish MSBLOC.Web.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MSBLOC.Web.dll"]
