import axios from "axios";
import UserService from "./UserService";

const HttpMethods = {
    GET: 'GET',
    POST: 'POST',
    DELETE: 'DELETE',
};

const _axios = axios.create();
const _kc = UserService.keycloak;

const configure = () => {
    _axios.interceptors.request.use((config) => {
        if (_kc.authenticated) {
            _kc.updateToken();
            config.headers.Authorization = `Bearer ${_kc.token}`;
        }
        return config;
    });
};

const getAxiosClient = () => _axios;

const HttpService = {
    HttpMethods,
    configure,
    getAxiosClient
};

export default HttpService;