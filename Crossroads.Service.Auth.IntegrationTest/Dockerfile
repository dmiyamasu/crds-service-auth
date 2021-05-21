FROM cypress/included:3.2.0@sha256:9c7aab130b0e681bda3ac94c437af47d844e9dbfda377de7ff32640b4364d17f
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
