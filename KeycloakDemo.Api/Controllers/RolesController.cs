using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakDemo.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class RolesController : ControllerBase
{
    [Authorize(Roles = "admin")]
    [HttpGet("admin/{adminName}")]
    public ActionResult GetAdminName(string adminName)
    {
        var adminInfo = new {
            endpoint = Request.GetDisplayUrl(),
            adminName = adminName
        };
        return Ok(adminInfo);
    }

    [Authorize(Roles = "user")]
    [HttpGet("user/{userName}")]
    public ActionResult GetUserName(string userName)
    {
        var userInfo = new {
            endpoint = Request.GetDisplayUrl(),
            adminName = userName
        };
        return Ok(userInfo);
    }

    [Authorize(Roles = "user,admin")]
    [HttpGet("either/{name}")]
    public ActionResult GetEitherName(string name)
    {
        var eitherInfo = new {
            endpoint = Request.GetDisplayUrl(),
            adminName = name
        };
        return Ok(eitherInfo);
    }
}