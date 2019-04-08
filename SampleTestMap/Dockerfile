#### Image ####
FROM microsoft/dotnet:2.2-sdk

#### Copy files ####

#### Setup Steps ####

#### Entry Point ####
COPY runtests.sh /
RUN ["chmod", "+x", "/runtests.sh"]
ENTRYPOINT /runtests.sh
