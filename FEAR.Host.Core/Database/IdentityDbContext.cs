using FEAR.Domain.Dto.Authentication;
using Microsoft.EntityFrameworkCore;
using FEAR.Domain.Database;

namespace FEAR.Host.Core.Database
{
    /// <summary>
    /// Entity Framework Core database context for managing user identity, authentication, roles, and permissions.
    /// Maps identity-related entities to database tables and configures relationships and value generators.
    /// </summary>
    public class IdentityDbContext : DbContext, IIdentityDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbContext"/> class with the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the entity mappings, relationships, and value generators for the identity model.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map entities to tables
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserSecret>().ToTable("UserSecrets");
            modelBuilder.Entity<UserRefreshToken>().ToTable("UserRefreshTokens");
            modelBuilder.Entity<SystemAction>().ToTable("SystemActions");
            modelBuilder.Entity<PermissionSet>().ToTable("Permissions");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<SystemRoleAssignment>().ToTable("SystemRoleAssignments");
            modelBuilder.Entity<InvestigationRoleAssignment>().ToTable("InvestigationRoleAssignments");

            // Configure relationships for system role assignments
            modelBuilder.Entity<SystemRoleAssignment>()
                .HasOne(s => s.User)
                .WithMany(g => g.SystemRoleAssignments)
                .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<SystemRoleAssignment>()
                .HasOne(s => s.Role)
                .WithMany(g => g.SystemRoleAssignments)
                .HasForeignKey(s => s.RoleId);

            // Configure relationships for investigation role assignments
            modelBuilder.Entity<InvestigationRoleAssignment>()
                .HasOne(s => s.User)
                .WithMany(g => g.InvestigationRoleAssignments)
                .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<InvestigationRoleAssignment>()
                .HasOne(s => s.Role)
                .WithMany(g => g.InvestigationRoleAssignments)
                .HasForeignKey(s => s.RoleId);

            // Configure many-to-many relationships for permission sets and actions
            modelBuilder.Entity<PermissionSet>()
                .HasMany<SystemAction>(s => s.ApprovedActions)
                .WithMany(g => g.ApprovedPermissionSets)
                .UsingEntity(join => join.ToTable("PermissionSetsApprovedActions"));
            modelBuilder.Entity<PermissionSet>()
                .HasMany<SystemAction>(s => s.DeniedActions)
                .WithMany(g => g.DeniedPermissionSets)
                .UsingEntity(join => join.ToTable("PermissionSetsDeniedActions"));

            // Configure many-to-many relationship for roles and permission sets
            modelBuilder.Entity<Role>()
                .HasMany<PermissionSet>(s => s.Permissions)
                .WithMany(g => g.Roles);

            // Configure one-to-one relationship between user and user secret
            modelBuilder.Entity<User>()
                .HasOne(s => s.UserSecret)
                .WithOne(g => g.User)
                .HasForeignKey<UserSecret>(s => s.UserId);

            // Configure value generators for primary keys
            modelBuilder.Entity<User>().Property(u => u.UserId).HasValueGenerator<SequenialGuidGenerator>();
            modelBuilder.Entity<UserRefreshToken>().Property(u => u.UserRefreshTokenId).HasValueGenerator<SequenialGuidGenerator>();
            modelBuilder.Entity<SystemAction>().Property(u => u.SystemActionId).HasValueGenerator<SequenialGuidGenerator>();
            modelBuilder.Entity<PermissionSet>().Property(u => u.PermissionSetId).HasValueGenerator<SequenialGuidGenerator>();
            modelBuilder.Entity<Role>().Property(u => u.RoleId).HasValueGenerator<SequenialGuidGenerator>();
            modelBuilder.Entity<SystemRoleAssignment>().Property(u => u.SystemRoleAssignmentId).HasValueGenerator<SequenialGuidGenerator>();
            modelBuilder.Entity<InvestigationRoleAssignment>().Property(u => u.InvestigationRoleAssignmentId).HasValueGenerator<SequenialGuidGenerator>();
        }

        /// <summary>
        /// Gets or sets the users registered in the system.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the user secrets, such as credentials or keys, associated with users.
        /// </summary>
        public DbSet<UserSecret> UserSecrets { get; set; }

        /// <summary>
        /// Gets or sets the refresh tokens issued to users for authentication.
        /// </summary>
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        /// <summary>
        /// Gets or sets the system actions that can be performed, used for permission and authorization checks.
        /// </summary>
        public DbSet<SystemAction> SystemActions { get; set; }

        /// <summary>
        /// Gets or sets the permission sets, which define collections of allowed and denied actions.
        /// </summary>
        public DbSet<PermissionSet> PermissionSets { get; set; }

        /// <summary>
        /// Gets or sets the roles available in the system, each of which can have associated permissions.
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets the system-wide role assignments for users.
        /// </summary>
        public DbSet<SystemRoleAssignment> SystemRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets the investigation-specific role assignments for users.
        /// </summary>
        public DbSet<InvestigationRoleAssignment> InvestigationRoleAssignments { get; set; }
    }
}
