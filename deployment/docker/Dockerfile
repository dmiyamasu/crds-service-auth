FROM microsoft/dotnet:2.1-sdk@sha256:4db9513d5b1bcf4b88c5700f9b831908d435d9771fd28a338b123e5ab8eec036 AS build-env 
WORKDIR /app 
 
# Copy files to /app 
COPY ./Crossroads.Service.Auth ./Crossroads.Service.Auth
COPY ./Crossroads.Service.Auth.Test ./Crossroads.Service.Auth.Test
COPY ./Crossroads.Service.Auth.sln ./Crossroads.Service.Auth.sln
 
# Run Unit Tests 
RUN dotnet test Crossroads.Service.Auth.Test/Crossroads.Service.Auth.Test.csproj 
 
# Publish build to out directory 
RUN dotnet publish -c Release -o out 
 
# Step 1: Build runtime image 
FROM microsoft/dotnet:2.1-aspnetcore-runtime@sha256:6ec6e5db161b5bd3b5472e4b0a31230823fb2eacff885be367022ae9a68281e5

WORKDIR /app
 
# Copy over the build from the previous step 
COPY --from=build-env /app/Crossroads.Service.Auth/out . 

# Install wget
RUN echo 'installing wget' \
&& apt-get update \
&& apt-get install -y wget

RUN echo 'installing gnupg' \
&& apt-get install -y gnupg

# Install new relic
RUN echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget -O- https://download.newrelic.com/548C16BF.gpg | apt-key add - \
&& apt-get update \
&& apt-get install newrelic-netcore20-agent

# Make sure this is running in production
ENV ASPNETCORE_ENVIRONMENT=Production

ENV CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent

CMD $CORECLR_NEWRELIC_HOME/run.sh dotnet Crossroads.Service.Auth.dll
# Run the dotnet entrypoint for the crdsfred dll 
# ENTRYPOINT ["dotnet", "Crossroads.Service.Auth.dll"]
# test dave
