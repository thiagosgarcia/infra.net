namespace Infra.Net.SwaggerFilters;

public class RemoveVersionParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var versionParameter = operation.Parameters.SingleOrDefault(p => p.Name == "version");
        operation.Parameters.Remove(versionParameter);
    }
}
