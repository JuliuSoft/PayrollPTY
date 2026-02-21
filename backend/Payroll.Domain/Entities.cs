namespace Payroll.Domain;

public enum SalaryType { Monthly, Hourly }
public enum EmployeeStatus { Active, Inactive }
public enum PayScheduleType { Quincenal, Mensual }
public enum PayrollRunType { Regular, Decimo }
public enum PayrollRunStatus { Draft, Validated, Preview, Approved, Locked }
public enum PayrollLineCategory { Earning, Deduction, Tax, EmployerContribution }

public class Employee {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public string? Department { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public string? BankAccount { get; set; }
    public SalaryType SalaryType { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal HourlyRate { get; set; }
    public string TaxConfig { get; set; } = "default";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PaySchedule { public Guid Id { get; set; } = Guid.NewGuid(); public PayScheduleType Type { get; set; } public string Name { get; set; } = string.Empty; }
public class PayPeriod { public Guid Id { get; set; } = Guid.NewGuid(); public Guid ScheduleId { get; set; } public DateOnly StartDate { get; set; } public DateOnly EndDate { get; set; } public DateOnly PayDate { get; set; } public string Status { get; set; } = "Open"; }
public class PayrollRun {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PeriodId { get; set; }
    public PayrollRunType RunType { get; set; } = PayrollRunType.Regular;
    public PayrollRunStatus Status { get; set; } = PayrollRunStatus.Draft;
    public string CreatedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class PayrollResult { public Guid Id { get; set; } = Guid.NewGuid(); public Guid RunId { get; set; } public Guid EmployeeId { get; set; } public decimal Gross { get; set; } public decimal Deductions { get; set; } public decimal Net { get; set; } public decimal EmployerCostTotal { get; set; } }
public class PayrollLineItem { public Guid Id { get; set; } = Guid.NewGuid(); public Guid ResultId { get; set; } public PayrollLineCategory Category { get; set; } public string Code { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public decimal Amount { get; set; } public decimal Base { get; set; } public decimal Rate { get; set; } public string RuleId { get; set; } = string.Empty; public DateOnly EffectiveDate { get; set; } public string RoundingMode { get; set; } = "AwayFromZero"; }
public class StatutoryConfig { public Guid Id { get; set; } = Guid.NewGuid(); public DateOnly EffectiveDate { get; set; } public decimal CssEmployeeRate { get; set; } public decimal CssEmployerRate { get; set; } public string IsrBracketsJson { get; set; } = "[]"; public string RoundingRules { get; set; } = "AwayFromZero"; }
public class AuditEvent { public Guid Id { get; set; } = Guid.NewGuid(); public string UserId { get; set; } = string.Empty; public string Action { get; set; } = string.Empty; public string Entity { get; set; } = string.Empty; public string EntityId { get; set; } = string.Empty; public string? BeforeJson { get; set; } public string? AfterJson { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow; }
public class Document { public Guid Id { get; set; } = Guid.NewGuid(); public string Type { get; set; } = string.Empty; public string FileName { get; set; } = string.Empty; public byte[] Content { get; set; } = Array.Empty<byte>(); public Guid RelatedEntityId { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow; }
public class AppUser { public Guid Id { get; set; } = Guid.NewGuid(); public string Username { get; set; } = string.Empty; public string PasswordHash { get; set; } = string.Empty; public string Role { get; set; } = "HR"; }
