import Keycloak from "keycloak-js";

const keycloak = new Keycloak('/keycloak.json');

const initKeycloak = (onAuthenticationCallback:() => void) => {
    keycloak.init({
        onLoad: 'check-sso',
        silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
        pkceMethod: 'S256'
    })
    .then(() => {
        onAuthenticationCallback();
    })
};

const getUsername = () => keycloak.tokenParsed?.preferred_username;

const hasRole = (roles: string[]) => roles.some((role: string) => keycloak.hasRealmRole(role));

const UserService = {
    initKeycloak,
    keycloak,
};

export default UserService;