using System.Text.Json;
using Payroll.Domain;

namespace Payroll.Application;

public class PayrollEngine : IPayrollEngine {
    public PayrollComputationResult Compute(PayrollInput input) {
        var c = input.Config;
        var rounding = MidpointRounding.AwayFromZero;
        var gross = input.Employee.SalaryType == SalaryType.Monthly ? input.Employee.BaseSalary + input.OneTimeEarnings : (input.Employee.HourlyRate * input.WorkedHours) + input.OneTimeEarnings;
        gross = Math.Round(gross, 2, rounding);
        var lines = new List<PayrollComputationItem> {
            new(PayrollLineCategory.Earning, "GROSS", "Gross earnings", gross, gross, 1m, "RULE_GROSS", c.EffectiveDate, c.RoundingRules)
        };
        var cssEmployee = Math.Round(gross * c.CssEmployeeRate, 2, rounding);
        lines.Add(new(PayrollLineCategory.Deduction, "CSS_EMP", "CSS employee", cssEmployee, gross, c.CssEmployeeRate, "RULE_CSS_EMP", c.EffectiveDate, c.RoundingRules));

        var taxable = Math.Max(0, gross - cssEmployee);
        var brackets = JsonSerializer.Deserialize<List<IsrBracket>>(c.IsrBracketsJson) ?? [];
        var isr = ComputeIsr(taxable, brackets, rounding);
        lines.Add(new(PayrollLineCategory.Tax, "ISR", "ISR withholding", isr, taxable, 0, "RULE_ISR", c.EffectiveDate, c.RoundingRules));

        var other = Math.Round(input.OtherDeductions,2,rounding);
        if (other > 0) lines.Add(new(PayrollLineCategory.Deduction, "OTHER", "Other deductions", other, gross, 0, "RULE_OTHER", c.EffectiveDate, c.RoundingRules));
        var deductions = cssEmployee + isr + other;
        var net = Math.Round(gross - deductions, 2, rounding);
        var cssEmployer = Math.Round(gross * c.CssEmployerRate,2,rounding);
        lines.Add(new(PayrollLineCategory.EmployerContribution, "CSS_EMPL", "CSS employer", cssEmployer, gross, c.CssEmployerRate, "RULE_CSS_EMPL", c.EffectiveDate, c.RoundingRules));
        var warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(input.Employee.BankAccount)) warnings.Add("Missing bank account.");
        if (net < 0) warnings.Add("Negative net pay.");
        return new(gross, deductions, net, gross + cssEmployer, lines, warnings);
    }

    private static decimal ComputeIsr(decimal taxable, List<IsrBracket> brackets, MidpointRounding rounding) {
        var match = brackets.OrderBy(x => x.From).LastOrDefault(b => taxable >= b.From && (b.To is null || taxable <= b.To));
        if (match is null) return 0;
        var tax = match.FixedQuota + ((taxable - match.From) * match.Rate);
        return Math.Round(Math.Max(0, tax), 2, rounding);
    }
}
