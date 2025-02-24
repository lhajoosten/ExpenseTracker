# Stage 1: Build the Angular app
FROM node:20 AS build

# Set the working directory
WORKDIR /app

# Copy package.json and package-lock.json
COPY package.json package-lock.json ./

# Install dependencies
RUN npm ci

# Copy the application code
COPY . .

# Build the Angular app
RUN npm run build --omit=dev

# Stage 2: Serve the Angular app with Nginx
FROM nginx:alpine

# Create directory for certificates
RUN mkdir -p /etc/nginx/certs

# Set working directory for Nginx
WORKDIR /usr/share/nginx/html

# Copy the built Angular app from the previous stage
COPY --from=build /app/dist/task-flow.web/browser .

# Copy custom Nginx configuration
COPY ./nginx/default.conf /etc/nginx/conf.d/default.conf

# Expose port 80 and 443
EXPOSE 80
EXPOSE 443

# Start Nginx server
CMD ["nginx", "-g", "daemon off;"]
