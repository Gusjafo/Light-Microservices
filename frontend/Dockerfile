# Stage 1: build the Angular application
FROM node:18-alpine AS build
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm install
COPY . .
RUN npm run build -- --configuration production

# Stage 2: serve the compiled app with Nginx
FROM nginx:1.25-alpine
COPY --from=build /app/dist/app /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
