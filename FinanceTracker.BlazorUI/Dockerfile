FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FinanceTracker.BlazorUI/FinanceTracker.BlazorUI.csproj", "FinanceTracker.BlazorUI/"]
RUN dotnet restore "FinanceTracker.BlazorUI/FinanceTracker.BlazorUI.csproj"

COPY . .
WORKDIR "/src/FinanceTracker.BlazorUI"

RUN dotnet publish -c Release -o /app/publish

FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
