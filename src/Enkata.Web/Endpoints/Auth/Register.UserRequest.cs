using Microsoft.Build.Framework;

namespace Enkata.Web.Endpoints.Auth;

public class RegisterUserRequest
{
  [Required]
  public string UserName { get; set; }
  [Required]
  public string Email { get; set; }
  [Required]
  public string Password { get; set; }
}
