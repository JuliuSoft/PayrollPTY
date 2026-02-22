using Microsoft.EntityFrameworkCore;
using Payroll.Domain;

namespace Payroll.Infrastructure;

public class PayrollDbContext(DbContextOptions<PayrollDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PaySchedule> PaySchedules => Set<PaySchedule>();
    public DbSet<PayPeriod> PayPeriods => Set<PayPeriod>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<PayrollResult> PayrollResults => Set<PayrollResult>();
    public DbSet<PayrollLineItem> PayrollLineItems => Set<PayrollLineItem>();
    public DbSet<StatutoryConfig> StatutoryConfigs => Set<StatutoryConfig>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>().HasIndex(x => x.NationalId).IsUnique();
        modelBuilder.Entity<AuditEvent>().Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        modelBuilder.Entity<PayrollRun>().HasIndex(x => new { x.PeriodId, x.RunType });
    }
}
