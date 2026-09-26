# Builds the SPA as a static artifact and serves it with nginx - proves
# ADR-0007's decision that the SPA is deployable independently of the API
# (see backend/Dockerfile for the API's separate artifact). The API base URL
# is baked in at build time via VITE_API_BASE_URL (see .env.example).

FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
