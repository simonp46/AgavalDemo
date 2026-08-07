# syntax=docker/dockerfile:1.7

FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN --mount=type=secret,id=corporate_ca,required=false \
    if [ -s /run/secrets/corporate_ca ]; then \
        NODE_EXTRA_CA_CERTS=/run/secrets/corporate_ca npm ci; \
    else \
        npm ci; \
    fi

COPY . ./
RUN npm run build

FROM nginx:1.29-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/gestor-inventario/browser/ /usr/share/nginx/html/

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=12 \
    CMD wget --quiet --output-document=- http://127.0.0.1/health > /dev/null || exit 1

CMD ["nginx", "-g", "daemon off;"]
