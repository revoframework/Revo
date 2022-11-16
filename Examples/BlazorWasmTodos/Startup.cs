using Microsoft.EntityFrameworkCore;
using Revo.Core.Configuration;
using Revo.EFCore.Configuration;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.Examples.BlazorWasmTodos.Blazor;

namespace Revo.Examples.BlazorWasmTodos;

public class Startup : RevoStartup
{
	public Startup(IConfiguration configuration) : base(configuration)
	{
	}

	protected override IRevoConfiguration CreateRevoConfiguration()
	{
		return new RevoConfiguration()
			.UseEFCoreDataAccess(
				contextBuilder => contextBuilder
					.UseSqlite("Data Source=todos.db"),
				advancedAction: config =>
				{
					config
						.AddConvention<BaseTypeAttributeConvention>(-200)
						.AddConvention<IdColumnsPrefixedWithTableNameConvention>(-110)
						.AddConvention<PrefixConvention>(-9)
						.AddConvention<SnakeCaseTableNamesConvention>(1)
						.AddConvention<SnakeCaseColumnNamesConvention>(1)
						.AddConvention<LowerCaseConvention>(2);
				})
			.UseAllEFCoreInfrastructure();
	}
}