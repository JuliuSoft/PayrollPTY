using Payroll.Domain;

namespace Payroll.Application;

public record PayrollComputationItem(PayrollLineCategory Category,string Code,string Description,decimal Amount,decimal Base,decimal Rate,string RuleId,DateOnly EffectiveDate,string RoundingMode);
public record PayrollComputationResult(decimal Gross,decimal Deductions,decimal Net,decimal EmployerCost, List<PayrollComputationItem> LineItems, List<string> Warnings);
public record IsrBracket(decimal From, decimal? To, decimal Rate, decimal FixedQuota);

public class PayrollInput {
    public required Employee Employee { get; init; }
    public required StatutoryConfig Config { get; init; }
    public decimal WorkedHours { get; init; } = 0;
    public decimal OneTimeEarnings { get; init; } = 0;
    public decimal OtherDeductions { get; init; } = 0;
}

public interface IPayrollEngine { PayrollComputationResult Compute(PayrollInput input); }
