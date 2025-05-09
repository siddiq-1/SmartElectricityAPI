FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
ENV TZ="Europe/Tallinn"
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SmartElectricityAPI.csproj", "."]
RUN dotnet restore "./SmartElectricityAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SmartElectricityAPI.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "SmartElectricityAPI.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SmartElectricityAPI.dll"]