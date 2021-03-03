using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Infra.Net.SwaggerFilters
{
    public class AttachControllerNameFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
                operation.Summary = $"{operation.Summary} - {descriptor.ControllerTypeInfo.Namespace}.{descriptor.ControllerName}Controller.{descriptor.ActionName}";
        }
    }
}