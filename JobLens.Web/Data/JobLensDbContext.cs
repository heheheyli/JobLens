using JobLens.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace JobLens.Web.Data;

public class JobLensDbContext : DbContext
{
    public JobLensDbContext(DbContextOptions<JobLensDbContext> options) : base(options) { }

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<StatusChange> StatusChanges => Set<StatusChange>();
    public DbSet<SkillProfile> SkillProfiles => Set<SkillProfile>();
    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<JobApplication>()
            .HasIndex(a => a.Status);

        b.Entity<JobApplication>()
            .HasMany(a => a.StatusHistory)
            .WithOne(h => h.JobApplication!)
            .HasForeignKey(h => h.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Skill>()
            .HasOne(s => s.Profile)
            .WithMany(p => p.Skills)
            .HasForeignKey(s => s.SkillProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override int SaveChanges()
    {
        NormaliseDates();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormaliseDates();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void NormaliseDates()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            foreach (var prop in entry.Properties)
            {
                if (prop.CurrentValue is DateTime { Kind: DateTimeKind.Unspecified } dt)
                {
                    prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }
        }
    }
}