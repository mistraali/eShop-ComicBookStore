FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CartService/CartService.csproj", "CartService/"]
COPY ["CartService.Application/CartService.Application.csproj", "CartService.Application/"]
COPY ["CartService.Domain/CartService.Domain.csproj", "CartService.Domain/"]
RUN dotnet restore "CartService/CartService.csproj"
COPY . .
WORKDIR "/src/CartService"
RUN dotnet build "CartService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CartService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CartService.dll"]
