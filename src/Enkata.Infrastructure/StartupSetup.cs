using Enkata.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enkata.Infrastructure;

public static class StartupSetup
{
  public static void AddDbContext(this IServiceCollection services, string connectionString) {
    services.AddDbContext<AppDbContext>(options =>
      options.UseSqlite(connectionString)); // will be created in web project root
    
    services.AddDbContext<AppIdentityDbContext>(options =>
      options.UseSqlite(connectionString)); // Will be created in web project
    
    // services.AddDbContext<AppDbContext>(options =>
    //   options.UseNpgsql(connectionString));
    //
    // services.AddDbContext<AppIdentityDbContext>(options =>
    //   options.UseNpgsql(connectionString));
  }
}
