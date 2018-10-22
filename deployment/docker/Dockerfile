FROM microsoft/dotnet:2.1-sdk AS build-env 
WORKDIR /app 
 
# Copy files to /app 
COPY . ./ 
 
# Change working directory to Crossroads.Service.Finance
WORKDIR /app/Crossroads.Service.Auth

# Change work directory back to /app - root of proj 
WORKDIR /app
 
# Run Unit Tests 
# RUN dotnet test Crossroads.Service.Finance.Test/Crossroads.Service.Finance.Test.csproj 
# RUN dotnet test MinistryPlatform.Test/MinistryPlatform.Test.csproj
# RUN dotnet test Pushpay.Test/Pushpay.Test.csproj
 
# Publish build to out directory 
RUN dotnet publish -c Release -o out 
 
# Step 1: Build runtime image 
FROM microsoft/dotnet:2.1-aspnetcore-runtime

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