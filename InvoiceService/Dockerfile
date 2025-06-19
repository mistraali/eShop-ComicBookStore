FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InvoiceService/InvoiceService.csproj", "InvoiceService/"]
COPY ["InvoiceService.Application/InvoiceService.Application.csproj", "InvoiceService.Application/"]
COPY ["InvoiceService.Domain/InvoiceService.Domain.csproj", "InvoiceService.Domain/"]
RUN dotnet restore "InvoiceService/InvoiceService.csproj"
COPY . .
WORKDIR "/src/InvoiceService"
RUN dotnet build "InvoiceService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InvoiceService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvoiceService.dll"]
