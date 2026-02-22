using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Payroll.Infrastructure;

public class DesignTimePayrollDbContextFactory : IDesignTimeDbContextFactory<PayrollDbContext>
{
    public PayrollDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PayrollDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=PayrollPTY;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=true");
        return new PayrollDbContext(optionsBuilder.Options);
    }
}
