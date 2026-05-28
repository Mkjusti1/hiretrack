using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HireTrack.Api.Domain.Entities;

namespace HireTrack.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationEvent> ApplicationEvents => Set<ApplicationEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.Name).IsRequired().HasMaxLength(200);
            e.Property(t => t.Slug).IsRequired().HasMaxLength(100);
        });

        builder.Entity<AppUser>(e =>
        {
            e.HasOne(u => u.Tenant)
             .WithMany(t => t.Users)
             .HasForeignKey(u => u.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
        });

        builder.Entity<Job>(e =>
        {
            e.HasKey(j => j.Id);
            e.HasOne(j => j.Tenant)
             .WithMany(t => t.Jobs)
             .HasForeignKey(j => j.TenantId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(j => j.CreatedBy)
             .WithMany()
             .HasForeignKey(j => j.CreatedById)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(j => j.Title).IsRequired().HasMaxLength(200);
            e.Property(j => j.Department).HasMaxLength(100);
            e.Property(j => j.Location).HasMaxLength(100);
        });

        builder.Entity<Candidate>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Tenant)
             .WithMany(t => t.Candidates)
             .HasForeignKey(c => c.TenantId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Email).IsRequired().HasMaxLength(200);
        });

        builder.Entity<Application>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Job)
             .WithMany(j => j.Applications)
             .HasForeignKey(a => a.JobId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Candidate)
             .WithMany(c => c.Applications)
             .HasForeignKey(a => a.CandidateId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Tenant)
             .WithMany()
             .HasForeignKey(a => a.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(a => new { a.JobId, a.CandidateId }).IsUnique();
        });

        builder.Entity<ApplicationEvent>(e =>
        {
            e.HasKey(ae => ae.Id);
            e.HasOne(ae => ae.Application)
             .WithMany(a => a.Events)
             .HasForeignKey(ae => ae.ApplicationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ae => ae.Actor)
             .WithMany()
             .HasForeignKey(ae => ae.ActorId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
