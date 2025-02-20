FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AplicacaoTeste.Host/AplicacaoTeste.Host.csproj", "AplicacaoTeste.Host/"]
COPY ["AplicacaoTeste/AplicacaoTeste.csproj", "AplicacaoTeste/"]
RUN dotnet restore "AplicacaoTeste.Host/AplicacaoTeste.Host.csproj"
COPY . .
WORKDIR "/src/AplicacaoTeste.Host"
RUN dotnet build "AplicacaoTeste.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AplicacaoTeste.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AplicacaoTeste.Host.dll"]
