# Keycloak with ASP.NET Core Web API and SPA

## Keycloak Configuration
### Client Configuration: Front-End SPA
- Click on **Clients** >> **Create client**
- General Settings
    - Enter **Client ID** (e.g. keycloak-demo-frontend); 
    - Optionally, enter a **Name**
    - Click on **Next**
- Capability Config
    - **Client authentication**: off
    - **Authorization**: off
    - **Authentication flow**: check **Standard Flow** only
    - Click on **Next**
- Login settings: only following needed
    - **Valid redirect URIs**: http://localhost:5000/*
    - **Web origins**: http://localhost:5000
        - **Note** forward slash + asterisk (`/*`) at end of first one
        - **Note** no trailing forward slash (`/`) on second one
        - If **Web Origins** has a trailing slash, you get CORS errors
        - If **Valid redirect URIs** is missing `/*` you get `Invalid redirect_uri paramater` errors

### Create a test user in Keycloak
- Username: test1@test.com
- Password: test1

## SPA Configuration
- Keycloak-js docs: https://www.keycloak.org/docs/latest/securing_apps/#_javascript_adapter
- Another resource: https://github.com/hexadefence/keycloak-react-example/blob/master/src/App.js

## ASP.NET Core Web API Configuration
