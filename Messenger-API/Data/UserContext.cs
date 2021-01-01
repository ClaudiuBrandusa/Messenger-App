using Messenger_API.Authentication;
using Messenger_API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Data
{
    public class UserContext : IdentityDbContext<ApplicationUser>
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {

        }

        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RefreshTokenEntity>()
                .HasKey(t => t.Token);

            base.OnModelCreating(builder);
        }
    }
}
