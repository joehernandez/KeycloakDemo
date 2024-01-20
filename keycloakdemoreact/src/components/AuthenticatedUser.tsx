import UserService from "../services/UserService"
import HttpService from "../services/HttpService";
import { useEffect, useState } from "react";
import { Readings } from "./Readings";

function AuthenticatedUser() {
  const [readings, setReadings] = useState<Readings[]>([]);
  const kc = UserService.keycloak;
  const client = HttpService.getAxiosClient();
  const apiUrl = 'http://localhost:5083/WeatherForecast'
  const callApi = async () => {
    const response = await client.get(apiUrl)
    setReadings(response.data);
  }
  
  useEffect(() => {
    console.log(readings);
  }, [readings]);

  return (
    <>
      <div>
        <h1>
            Hello {kc.tokenParsed?.preferred_username} &nbsp;
            <button onClick={() => kc.logout()}>Log Out</button>
            <button onClick={callApi}>Call API</button>
        </h1>
      </div>
      <ul>
        {
          (Array.isArray(readings) && readings.length) ? 
            readings.map((reading, index) => (
              <li key={index}>{reading.temperatureF} - {reading.summary}</li>
            )) : 
            null
        }
      </ul>
    </>
  )
}

export default AuthenticatedUser