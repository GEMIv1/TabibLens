using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TabibLens.Api.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected Guid GetUserId()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sub == null || !Guid.TryParse(sub, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid token.");
            }
            return userId;
        }
    }
}
