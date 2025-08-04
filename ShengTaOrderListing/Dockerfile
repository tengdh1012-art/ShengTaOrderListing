# ========================
# 构建镜像
# ========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 拷贝 csproj 并还原依赖
COPY ["ShengTaOrderListing.csproj", "./"]
RUN dotnet restore "ShengTaOrderListing.csproj"

# 拷贝所有源码并发布
COPY . .
RUN dotnet publish "ShengTaOrderListing.csproj" -c Release -o /app/publish

# ========================
# 运行镜像
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 📦 无需安装 libssl-dev 或 libmariadb-dev，MySqlConnector 不依赖本地库
# 复制发布内容
COPY --from=build /app/publish .

# 设置入口
ENTRYPOINT ["dotnet", "ShengTaOrderListing.dll"]