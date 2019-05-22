FROM cypress/base:8
WORKDIR /cypress-service-auth

COPY package.json .
COPY package-lock.json .
COPY cypress cypress
COPY cypress.json .
COPY wait-for-it.sh .

ENV CI=1
RUN npm ci

RUN npx cypress verify

RUN chmod +x wait-for-it.sh
