# 基础镜像（运行时）
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# 构建镜像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 拷贝 .csproj 并还原依赖
COPY ["ShengTaOrderListing.csproj", "./"]
RUN dotnet restore "ShengTaOrderListing.csproj"

# 拷贝其他源码并构建
COPY . .
RUN dotnet publish "ShengTaOrderListing.csproj" -c Release -o /app/publish

# 生成最终镜像
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShengTaOrderListing.dll"]
