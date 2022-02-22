using Ardalis.ApiEndpoints;
using Enkata.Infrastructure.Data;
using Enkata.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Enkata.Web.Endpoints.Auth;


public class RegisterEndpoint : BaseAsyncEndpoint
  .WithRequest<RegisterUserRequest>
  .WithResponse<RegisterUserResponse>
{
  private readonly IOptions<JwtBearerTokenSettings> _jwtTokenOptions;
  private readonly UserManager<ApplicationUser> _userManager;

  public RegisterEndpoint(IOptions<JwtBearerTokenSettings> jwtTokenOptions, UserManager<ApplicationUser> userManager)
  {
    _jwtTokenOptions = jwtTokenOptions;
    _userManager = userManager;
  }


  [HttpPost("/Users")]
  [SwaggerOperation(
    Summary = "Creates a new Uer",
    Description = "Creates a new user",
    OperationId = "User.Create",
    Tags = new[] { "UserEndpoints" })
  ]
  public override async Task<ActionResult<RegisterUserResponse>> HandleAsync(RegisterUserRequest request, CancellationToken token)
  {
    try
    {
      var identityUser = new ApplicationUser() { UserName = request.UserName, Email = request.Email };
      var result = await _userManager.CreateAsync(identityUser, request.Password);

      if (result.Succeeded)
      {
        var response =
          new RegisterUserResponse() {Id = identityUser.Id, Email = request.Email, Username = request.UserName};
        // To get Id, fetch the user from the database
        return Ok(response);
      }
      else
      {
        return new BadRequestObjectResult(new {Message = "User Registration Failed", Errors = result.Errors});
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }
}
