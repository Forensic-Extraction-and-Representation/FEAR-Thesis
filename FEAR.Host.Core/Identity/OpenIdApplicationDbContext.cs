using FEAR.Host.Core.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Host.Core.Identity
{
    public class OpenIdApplicationDbContext : DbContext
    {
        public OpenIdApplicationDbContext(DbContextOptions<OpenIdApplicationDbContext> options) : base(options)
        {

        }
    }
}
