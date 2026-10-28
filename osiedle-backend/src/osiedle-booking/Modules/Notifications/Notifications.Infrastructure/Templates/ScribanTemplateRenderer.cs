using Notifications.Application.Interfaces;
using Scriban;

namespace Notifications.Infrastructure.Templates;

public class ScribanTemplateRenderer : ITemplateRender
{
    public  async Task<string> RenderAsync<TModel>(string templateName, TModel model)
    {
        var templatePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "Templates", "Email", $"{templateName}.html"
        );

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template {templateName}.html not found in the patch {templatePath}");

        var templateText = await File.ReadAllTextAsync(templatePath);
        var template = Template.Parse(templateText);

        return await template.RenderAsync(model, member => member.Name);
    }
}