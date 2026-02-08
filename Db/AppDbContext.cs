using Microsoft.EntityFrameworkCore;
using orchestrator_portal.Db;

public class AppDbContext : DbContext
{
    public DbSet<Projects> Projects { get; set; }

    public DbSet<Subscriptions> Subscriptions { get; set; }

    public DbSet<Organization> Organization { get; set; }

    public DbSet<Resources> Resources { get; set; }

    public DbSet<ServiceConnection> ServiceConnection { get; set; }

    public DbSet<ResourceAssociation> ResourceAssociation { get; set; }

    public DbSet<VariableGroup> VariableGroup { get; set; }

    public DbSet<VariableGroupAssociation> VariableGroupAssociation { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //make each project name unique 
        modelBuilder.Entity<Projects>()
            .HasIndex(p => p.ProjectId)
            .IsUnique();

        //make each organization name unique 
        modelBuilder.Entity<Organization>()
           .HasIndex(p => p.name)
           .IsUnique();

        //make each resource name unique 
        modelBuilder.Entity<Resources>()
            .HasIndex(p => p.Name)
            .IsUnique();

        //make each service connection name unique 
        modelBuilder.Entity<ServiceConnection>()
           .HasIndex(p => p.Name)
           .IsUnique();

        //make each var group name unique 
        modelBuilder.Entity<VariableGroup>()
            .HasIndex(p => p.Name)
            .IsUnique();

        //make each combination of vargroup id plus key unique
        modelBuilder.Entity<VariableGroupAssociation>()
            .HasIndex(p => new { p.VariableGroupId, p.Key }).IsUnique();
    }

}