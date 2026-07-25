# Build stage
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
ARG GRAPHQL_API_URL=http://localhost:8087/graphql
RUN npm run build

# Production stage
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Create a script to inject environment variables at runtime
RUN mkdir -p /docker-entrypoint.d && \
    echo '#!/bin/sh' > /docker-entrypoint.d/10-env.sh && \
    echo 'API_URL="${GRAPHQL_API_URL:-http://localhost:8087/graphql}"' >> /docker-entrypoint.d/10-env.sh && \
    echo 'echo "window.__ENV__ = { GRAPHQL_API_URL: \"$API_URL\" };" > /usr/share/nginx/html/env-config.js' >> /docker-entrypoint.d/10-env.sh && \
    chmod +x /docker-entrypoint.d/10-env.sh

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
