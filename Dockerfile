FROM node:24-alpine AS admin-build
WORKDIR /app

COPY osiedle-admin/osiedle-admin/package.json osiedle-admin/osiedle-admin/package-lock.json ./
RUN npm ci

COPY osiedle-admin/osiedle-admin/ ./
RUN npm run build -- --base-href /admin/

FROM node:24-alpine AS client-build
WORKDIR /app

COPY osiedle-client/osiedle-client/package.json osiedle-client/osiedle-client/package-lock.json ./
RUN npm ci

COPY osiedle-client/osiedle-client/ ./
RUN npm run build

FROM nginx:alpine AS final

COPY --from=admin-build /app/dist/osiedle-admin/browser /usr/share/nginx/html/admin
COPY --from=client-build /app/dist/ /usr/share/nginx/html/

RUN printf '%s\n' \
    'server {' \
    '    listen 80;' \
    '    server_name _;' \
    '    root /usr/share/nginx/html;' \
    '' \
    '    location = /admin {' \
    '        return 301 /admin/;' \
    '    }' \
    '' \
    '    location /admin/ {' \
    '        try_files $uri $uri/ /admin/index.html;' \
    '    }' \
    '' \
    '    location / {' \
    '        try_files $uri $uri/ /index.html;' \
    '    }' \
    '}' \
    > /etc/nginx/conf.d/default.conf

EXPOSE 80