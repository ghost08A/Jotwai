# Builds and runs only Web.Jotwai (the JSON API) - proves ADR-0007's decision
# that the backend is deployable as its own artifact, independent of the SPA
# (see client/Dockerfile for the SPA's separate artifact).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Jotwai.Domain/Jotwai.Domain.csproj Jotwai.Domain/
COPY Jotwai.Service/Jotwai.Service.csproj Jotwai.Service/
COPY Web.Jotwai/Web.Jotwai.csproj Web.Jotwai/
RUN dotnet restore Web.Jotwai/Web.Jotwai.csproj

COPY Jotwai.Domain/ Jotwai.Domain/
COPY Jotwai.Service/ Jotwai.Service/
COPY Web.Jotwai/ Web.Jotwai/
RUN dotnet publish Web.Jotwai/Web.Jotwai.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Web.Jotwai.dll"]
