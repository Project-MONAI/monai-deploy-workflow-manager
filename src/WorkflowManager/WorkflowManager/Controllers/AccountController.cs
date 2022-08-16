using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Monai.Deploy.WorkflowManager.Configuration;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Monai.Deploy.WorkflowManager.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ApiControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="options">Workflow Manager Options.</param>
        public AccountController(IOptions<WorkflowManagerOptions> options) : base(options)
        {

        }


        public IActionResult Authenticate()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "some_id")
            };
            var secretBytes = Encoding.UTF8.GetBytes("donkey");//TODO Constants.Secrets);
            var key = new SymmetricSecurityKey(secretBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;

            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                "http://localhost:8080/", //TODO Constants.Issuer,
                "monai-app", //TODO Constants.Audiance,
                claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.Now.AddHours(1),
                signingCredentials);
            var tokenJson = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { access_token = tokenJson });
        }
    }
}
