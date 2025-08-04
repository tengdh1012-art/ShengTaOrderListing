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
# 运行镜像 + GDI+ 依赖
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 📦 添加 GDI+ 所需依赖（修复报表导出崩溃）
RUN apt-get update && \
    apt-get install -y libgdiplus libc6-dev && \
    ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll || true && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# 拷贝发布内容
COPY --from=build /app/publish .

# 设置入口
ENTRYPOINT ["dotnet", "ShengTaOrderListing.dll"]
