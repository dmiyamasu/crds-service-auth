FROM cypress/included:3.2.0
WORKDIR /cypress_auth_integration

COPY package.json .
COPY package-lock.json .
COPY cypress cypress
COPY cypress.json .
COPY wait-for-it.sh .

ENV CI=1
RUN npm ci

RUN npx cypress verify

RUN chmod +x wait-for-it.sh
