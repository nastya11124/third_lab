using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            Enum.GetNames(context.Type)
                .ToList()
                .ForEach(name => schema.Enum.Add(new OpenApiString($"{GetDisplayName(context.Type, name)}")));
        }
    }

    private string GetDisplayName(Type enumType, string name)
    {
        var displayAttribute = enumType.GetMember(name).First().GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? name;
    }
}