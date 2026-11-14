using Notifications.Application.Interfaces;
using Scriban;

namespace Notifications.Infrastructure.Templates;

public class ScribanTemplateRenderer : ITemplateRender
{
    public async Task<string> RenderAsync<TModel>(string templateName, TModel model, CancellationToken ct)
    {
        var templatePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "Templates", "Email", $"{templateName}.html"
        );

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template {templateName}.html not found at path {templatePath}");
        }
        
        var templateText = await File.ReadAllTextAsync(templatePath, ct);
        
        var template = Template.Parse(templateText);
        if (template.HasErrors)
        {
            throw new InvalidOperationException($"Template parsing errors");
        }
        
        return await template.RenderAsync(model, member => member.Name);
    }
}