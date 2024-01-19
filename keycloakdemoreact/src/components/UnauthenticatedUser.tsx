import UserService from "../services/UserService"

function UnauthenticatedUser() {
  const kc = UserService.keycloak;

    return (
        <>
          <div>
            <h1>
                Hello Anonymous, please log in &nbsp;
                <button onClick={() => kc.login()}>Login</button>
            </h1>
          </div>
        </>
      )
}

export default UnauthenticatedUser