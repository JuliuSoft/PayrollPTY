using System.Text.Json;
using Payroll.Application;
using Payroll.Domain;

namespace Payroll.Tests;

public class PayrollEngineTests {
    [Fact]
    public void Computes_Deterministic_Result() {
        var engine = new PayrollEngine();
        var config = new StatutoryConfig{ EffectiveDate = new DateOnly(2025,1,1), CssEmployeeRate=0.0975m, CssEmployerRate=0.1275m, IsrBracketsJson=JsonSerializer.Serialize(new List<IsrBracket>{ new(0,1000,0,0), new(1000,5000,0.15m,0) })};
        var emp = new Employee{ FullName="A", NationalId="8-1", HireDate=new DateOnly(2020,1,1), SalaryType=SalaryType.Monthly, BaseSalary=2000, BankAccount="PA123"};
        var result = engine.Compute(new PayrollInput{ Employee=emp, Config=config});
        Assert.Equal(2000m, result.Gross);
        Assert.Equal(195m, result.LineItems.First(x=>x.Code=="CSS_EMP").Amount);
        Assert.Equal(120.75m, result.LineItems.First(x=>x.Code=="ISR").Amount);
        Assert.Equal(1684.25m, result.Net);
    }
}
