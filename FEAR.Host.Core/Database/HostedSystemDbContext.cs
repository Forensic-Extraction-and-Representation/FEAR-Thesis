using Microsoft.EntityFrameworkCore;
using FEAR.Hosted.Domain.Database;
using FEAR.Host.Domain.Database.Model;

namespace FEAR.Host.Core.Database
{
    public class HostedSystemDbContext : DbContext, IHostedSystemDbContext
    {
        public HostedSystemDbContext(DbContextOptions<HostedSystemDbContext> options) : base(options)
        {
        }
        public DbSet<SystemWideProperty> SystemWideProperties { get; set; }
    }
}
