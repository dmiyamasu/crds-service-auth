FROM cypress/base:8
RUN pwd
RUN ls
RUN ls ../
#WORKDIR /app
WORKDIR /cypress-service-auth

COPY package.json .
COPY package-lock.json .

# COPY cypress /cypress-service-auth/cypress
# COPY cypress.json /cypress-service-auth/cypress.json
COPY wait-for-it.sh .

# dependencies will be installed only if the package files change
# COPY package.json .
# COPY package-lock.json .

# by setting CI environment variable we switch the Cypress install messages
# to small "started / finished" and avoid 1000s of lines of progress messages
# https://github.com/cypress-io/cypress/issues/1243
ENV CI=1
RUN npm ci
# verify that Cypress has been installed correctly.
# running this command separately from "cypress run" will also cache its result
# to avoid verifying again when running the tests
RUN npx cypress verify

# COPY cypress cypress
# COPY cypress.json .
# COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh
#DEBUG
RUN ls
RUN ls ../