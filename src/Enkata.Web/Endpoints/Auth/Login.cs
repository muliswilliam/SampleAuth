using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ardalis.ApiEndpoints;
using Enkata.Infrastructure.Data;
using Enkata.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace Enkata.Web.Endpoints.Auth;

public class UserLoginRequest {
  [Required]
  public string Email { get; set; }
  [Required]
  public string Password { get; set; }
}

public class UserLoginResponse
{
  public string Token { get; set; }
  public string Message { get; set; }
}

public class UserLoginEndpoint : BaseAsyncEndpoint
  .WithRequest<UserLoginRequest>
  .WithResponse<UserLoginResponse>
{
  private readonly JwtBearerTokenSettings _jwtBearerTokenSettings;
  private readonly UserManager<ApplicationUser> _userManager;

  public UserLoginEndpoint(IOptions<JwtBearerTokenSettings> jwtTokenOptions, UserManager<ApplicationUser> userManager)
  {
    _jwtBearerTokenSettings = jwtTokenOptions.Value;
    _userManager = userManager;
  }


  [HttpPost("/Users/Login")]
  [SwaggerOperation(
    Summary = "Signs in a user",
    Description = "Sign in a user",
    OperationId = "User.Login",
    Tags = new[] { "UserEndpoints" })
  ]
  public override async Task<ActionResult<UserLoginResponse>> HandleAsync(UserLoginRequest request, CancellationToken cancellationToken = new CancellationToken())
  {
    try
    {
      var user = await ValidateUserAsync(request);
      if (user == null)
        return new BadRequestObjectResult(new { Message = "Login Failed" });
      var token = GenerateToken(user);
      return Ok(new UserLoginResponse {Token = token, Message = "Login Success"});
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }

  private async Task<ApplicationUser?> ValidateUserAsync(UserLoginRequest loginRequest)
  {
    var identityUser = await _userManager.FindByEmailAsync(loginRequest.Email);

    if (identityUser != null)
    {
      var result = _userManager.PasswordHasher.VerifyHashedPassword(
        identityUser,
        identityUser.PasswordHash,
        loginRequest.Password);
      return result == PasswordVerificationResult.Success ? identityUser : null;
    }

    return null;
  }

  private string GenerateToken(ApplicationUser user)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_jwtBearerTokenSettings.SecretKey);
    var tokenDescriptor = new SecurityTokenDescriptor()
    {
      Subject = new ClaimsIdentity(new Claim[]
      {
        new Claim(ClaimTypes.Name, user.UserName.ToString()), new Claim(ClaimTypes.Email, user.Email)
      }),
      Expires = DateTime.UtcNow.AddSeconds(_jwtBearerTokenSettings.ExpiryTimeInSeconds),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
      Audience = _jwtBearerTokenSettings.Audience,
      Issuer = _jwtBearerTokenSettings.Issuer
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    
    return tokenHandler.WriteToken(token);
  }
}
