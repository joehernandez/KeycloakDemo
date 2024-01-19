import './App.css'
import AuthenticatedUser from './components/AuthenticatedUser'
import UnauthenticatedUser from './components/UnauthenticatedUser'
import UserService from './services/UserService'

function App() {

  return (
    <>
      {
        !UserService.keycloak.authenticated ? <UnauthenticatedUser /> : <AuthenticatedUser />
      }
      <div>
          <h2>Keycloak + React Demo</h2>
      </div>
    </>
  )
}

export default App
