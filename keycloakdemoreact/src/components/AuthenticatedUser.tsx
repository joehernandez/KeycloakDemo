import UserService from "../services/UserService"
import HttpService from "../services/HttpService";

function AuthenticatedUser() {
  const kc = UserService.keycloak;
  const client = HttpService.getAxiosClient();
  const apiUrl = 'http://localhost:5083/WeatherForecast'
  const callApi = () => {
    client.get()
  }
    return (
        <>
          <div>
            <h1>
                Hello {kc.tokenParsed?.preferred_username} &nbsp;
                <button onClick={() => kc.logout()}>Log Out</button>
            </h1>
          </div>
        </>
      )
}

export default AuthenticatedUser