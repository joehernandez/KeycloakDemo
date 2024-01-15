import Keycloak from "keycloak-js";

const _kc = new Keycloak('/keycloak.json');

const initKeycloak = (onAuthenticationCallback:() => void) => {
    _kc.init({
        onLoad: 'check-sso',
        silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
        pkceMethod: 'S256'
    })
    .then((authenticated) => {
        if (authenticated) {
            onAuthenticationCallback();
        } else {
            console.warn("not authenticated");
            doLogin();
        }
    })
};

const doLogin = _kc.login;
const doLogout = _kc.logout;
const getToken = () => _kc.token;
const isLoggedIn = () => !!_kc.token;

const updateToken = (successCallback:() => void) => 
    _kc.updateToken(5)
    .then(successCallback)
    .catch(doLogin);

const getUsername = () => _kc.tokenParsed?.preferred_username;

const hasRole = (roles: string[]) => roles.some((role: string) => _kc.hasRealmRole(role));

const UserService = {
    initKeycloak,
    doLogin,
    doLogout,
    getToken,
    isLoggedIn,
    updateToken,
    getUsername,
    hasRole
};

export default UserService;