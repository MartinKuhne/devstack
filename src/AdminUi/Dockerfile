# Build stage
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
ARG VITE_GRAPHQL_API_URL=http://localhost:8087/graphql
ENV VITE_GRAPHQL_API_URL=$VITE_GRAPHQL_API_URL
RUN npm run build

# Production stage
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Create a script to inject environment variables at runtime
RUN mkdir -p /docker-entrypoint.d && \
    echo '#!/bin/sh' > /docker-entrypoint.d/10-env.sh && \
    echo 'if [ -n "$GRAPHQL_API_URL" ]; then sed -i "s|http://localhost:8087/graphql|$GRAPHQL_API_URL|g" /usr/share/nginx/html/*.js 2>/dev/null || true; fi' >> /docker-entrypoint.d/10-env.sh && \
    chmod +x /docker-entrypoint.d/10-env.sh

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
