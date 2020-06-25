using Grpc.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Grpc.Server.Datas
{
    public class ApplicationDbContext: IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> 
            options):base(options)
        {
            
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Image> Images { get; set; }
    }
}
