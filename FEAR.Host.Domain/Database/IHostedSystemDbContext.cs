using FEAR.Host.Domain.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace FEAR.Hosted.Domain.Database
{
    public interface IHostedSystemDbContext
    {
        public DbSet<SystemWideProperty> SystemWideProperties { get; set; }
    }
}