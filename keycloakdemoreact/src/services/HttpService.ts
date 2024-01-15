import axios from "axios";
import UserService from "./UserService";

const HttpMethods = {
    GET: 'GET',
    POST: 'POST',
    DELETE: 'DELETE',
};

const _axios = axios.create();

const configure = () => {
    _axios.interceptors.request.use((config) => {
        if (UserService.isLoggedIn()) {
            const updateTokenCallback = () => {
                config.headers.Authorization = `Bearer ${UserService.getToken()}`;
            };
            UserService.updateToken(updateTokenCallback);
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