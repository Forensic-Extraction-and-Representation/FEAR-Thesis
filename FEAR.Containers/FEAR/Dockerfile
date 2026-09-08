# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
RUN apt-get update && apt-get install -y unzip zip nodejs npm  && \
    npm install -g --no-audit --no-fund typescript@5.9.2 webpack@5.101.0 webpack-cli@6.0.1
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
ENV PATH="$PATH:/root/.dotnet/tools"

COPY ["FEAR.Admin/FEAR.Admin.csproj", "FEAR.Admin/"]
COPY ["FEAR.Blazor.Shared/FEAR.Blazor.Shared.csproj", "FEAR.Blazor.Shared/"]
COPY ["FEAR.Domain/FEAR.Domain.csproj", "FEAR.Domain/"]
COPY ["FEAR.Host.Core/FEAR.Host.Core.csproj", "FEAR.Host.Core/"]
COPY ["FEAR.Host.Domain/FEAR.Host.Domain.csproj", "FEAR.Host.Domain/"]
COPY ["FEAR.Lang/FEAR.Lang.csproj", "FEAR.Lang/"]
COPY ["FEAR.Runtime/FEAR.Runtime.csproj", "FEAR.Runtime/"]
COPY ["FEAR.Scripts/FEAR.Scripts.esproj", "FEAR.Scripts/"]
COPY ["FEAR.Security/FEAR.Security.csproj", "FEAR.Security/"]
COPY ["FEAR.WASM/FEAR.WASM.csproj", "FEAR.WASM/"]

COPY ["FEAR.Admin", "FEAR.Admin/"]
COPY ["FEAR.Blazor.Shared", "FEAR.Blazor.Shared/"]
COPY ["FEAR.Domain", "FEAR.Domain/"]
COPY ["FEAR.Host.Core", "FEAR.Host.Core/"]
COPY ["FEAR.Host.Domain", "FEAR.Host.Domain/"]
COPY ["FEAR.Lang", "FEAR.Lang/"]
COPY ["FEAR.Runtime", "FEAR.Runtime/"]
COPY ["FEAR.Scripts", "FEAR.Scripts/"]
COPY ["FEAR.Security", "FEAR.Security/"]
COPY ["FEAR.WASM", "FEAR.WASM/"]

WORKDIR "/src/FEAR.Admin"
RUN dotnet restore "./FEAR.Admin.csproj"
RUN dotnet build "FEAR.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR "/src/FEAR.WASM"
RUN dotnet restore "./FEAR.WASM.csproj"

WORKDIR "/src/FEAR.WASM/npm_packages"
COPY FEAR.WASM/npm_packages/ ./

RUN npm install
RUN npm run build

WORKDIR "/src"
RUN set -e; \
    find . -name libman.json | while read file; do \
        dir=$(dirname "$file"); \
        echo "Restoring LibMan in $dir"; \
        (cd "$dir" && libman restore); \
    done

WORKDIR "/src/FEAR.Scripts"
RUN dotnet restore "./FEAR.Scripts.esproj"

WORKDIR "/src"
RUN echo "Packing FEAR.Scripts..."
RUN mkdir -p fear-scripts/GFEAR && \
    mkdir -p fear-scripts/RFEAR && \
    mkdir -p fear-scripts/Packages && \
    cp -r FEAR.Scripts/GFEAR/* fear-scripts/GFEAR/ && \
    cp -r FEAR.Scripts/RFEAR/* fear-scripts/RFEAR/ && \
    cp FEAR.Scripts/index.manifest fear-scripts/

RUN if [ -d "FEAR.Scripts/Packages" ]; then \
          cd FEAR.Scripts/Packages; \
          for dir in *; do \
            cd "$dir"; \
            zip -r "../${dir}.zip" *; \ 
            echo "Packed $dir into ${dir}.zip"; \
            cd ..; \ 
          done; \ 
        fi
WORKDIR "/src"
RUN cp FEAR.Scripts/Packages/*.zip fear-scripts/Packages/

COPY ["FEAR.Hosted/FEAR.Hosted.csproj", "FEAR.Hosted/"]
COPY ["FEAR.Hosted", "FEAR.Hosted/"]
WORKDIR "/src/FEAR.Hosted"
RUN dotnet restore "./FEAR.Hosted.csproj"

WORKDIR "/src"
RUN set -e; \
    find . -name libman.json | while read file; do \
        dir=$(dirname "$file"); \
        echo "Restoring LibMan in $dir"; \
        (cd "$dir" && libman restore); \
    done


FROM build AS publish
RUN apt-get update && apt-get install -y unzip zip nodejs npm
RUN mkdir -p /app/publish-scripts && cp -r /src/fear-scripts/* /app/publish-scripts/

WORKDIR "/src/FEAR.Hosted"
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FEAR.Hosted.csproj" --property:WarningLevel=0 -c $BUILD_CONFIGURATION -o /app/publish-hosted

WORKDIR "/src/FEAR.WASM"
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FEAR.WASM.csproj" --property:WarningLevel=0 -c $BUILD_CONFIGURATION -o /app/publish-wasm

FROM base AS final
ARG DEPLOY_SRC=deploy
ARG TLD=fear.local
ARG DEPLOY_TARGET=/var/www
ARG USER_UID=1200
ARG GROUP_GID=1200
ARG UNAME=appuser
ARG GNAME=appgroup
USER root

# install nginx and tini
RUN apt-get update && apt-get install -y nginx git aspnetcore-runtime-8.0
# for debugging
RUN apt-get install -y curl vim

COPY ["${DEPLOY_SRC}/nginx/", "/etc/nginx/sites-available/"]

# Link api.<TLD>, scripts.<TLD>, and ui.<TLD> to the sites-enabled directory
RUN ln -s /etc/nginx/sites-available/api.${TLD} /etc/nginx/sites-enabled/ && \
    ln -s /etc/nginx/sites-available/scripts.${TLD} /etc/nginx/sites-enabled/ && \
    ln -s /etc/nginx/sites-available/ui.${TLD} /etc/nginx/sites-enabled/

RUN groupadd --gid $GROUP_GID $GNAME && \
    useradd --create-home --shell /bin/bash --uid $USER_UID --gid $GROUP_GID --groups $GNAME $UNAME && \
    mkdir -p /opt/fear/bin/fear-hosted && \
    mkdir -p /opt/fear/lib/cases/

COPY ["${DEPLOY_SRC}/ssl/certs", "/etc/ssl/certs/"]
COPY ["${DEPLOY_SRC}/ssl/private", "/etc/ssl/private/"]

RUN date > /tmp/build_date
COPY ["${DEPLOY_SRC}/ssl/extra", "/usr/local/share/ca-certificates/"]
RUN update-ca-certificates

RUN chown -R $UNAME:$GNAME /etc/ssl/private/*fear.app.key && \
    chmod 600 /etc/ssl/private/*fear.app.key

WORKDIR $DEPLOY_TARGET
COPY --from=publish /app/publish-scripts/ ./fear-scripts

COPY --from=publish /app/publish-wasm/wwwroot/ ./fear-wasm
COPY ["${DEPLOY_SRC}/wasm-appsettings.json", "./fear-wasm/appsettings.json"]

COPY --from=publish /app/publish-hosted/ /opt/fear/bin/fear-hosted
# Remove all appsetting files from the FEAR.Hosted publish output, as we will copy our own versions in next step
RUN rm  -rf /opt/fear/bin/fear-hosted/appsettings*.json && \
    rm  -rf /opt/fear/bin/fear-hosted/wwwroot/administration/appsettings*.json && \
    rm  -rf /opt/fear/bin/fear-hosted/keyconfig.json

COPY ["${DEPLOY_SRC}/hosted-appsettings.json", "/opt/fear/bin/fear-hosted/appsettings.json"]
COPY ["${DEPLOY_SRC}/hosted-keyconfig.json", "/opt/fear/bin/fear-hosted/keyconfig.json"]
COPY ["${DEPLOY_SRC}/admin-appsettings.json", "/opt/fear/bin/fear-hosted/wwwroot/administration/appsettings.json"]
COPY ["${DEPLOY_SRC}/case_creation_defaults.json", "/opt/fear/bin/fear-hosted/case_creation_defaults.json"]
COPY ["${DEPLOY_SRC}/initial_cases.json", "/opt/fear/bin/fear-hosted/initial_cases.json"]
COPY ["${DEPLOY_SRC}/initial_color_maps.json", "/opt/fear/bin/fear-hosted/initial_color_maps.json"]

COPY ["FEAR.Containers/FEAR/docker-entrypoint.sh", "/"]
RUN sed -i 's/\r$//' /docker-entrypoint.sh && \
    date '+%Y-%m-%d @ %H:%M' > /startup-marker.txt
ENTRYPOINT ["/bin/bash", "/docker-entrypoint.sh"]