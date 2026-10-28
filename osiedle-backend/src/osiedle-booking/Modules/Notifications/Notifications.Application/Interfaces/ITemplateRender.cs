namespace Notifications.Application.Interfaces;

public interface ITemplateRender
{
    Task<string> RenderAsync<TModel>(string templateName,TModel model);
}