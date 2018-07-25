
# 基于microsoft/dotnet:latest构建Docker Image
FROM microsoft/dotnet:latest
 
WORKDIR /WebAppTest
 
# 将当前文件夹下的所有文件全部复制到工作目录
COPY . /WebAppTest
 
# 向外界暴露81端口
EXPOSE 85
 
# 执行dotnet DockerWebAPI.dll命令
CMD ["dotnet","run"]