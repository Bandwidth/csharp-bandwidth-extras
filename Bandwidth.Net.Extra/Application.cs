using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bandwidth.Net;
using Bandwidth.Net.Api;

namespace Bandwidth.Net.Extra
{
    public static class ApplicationExtensions 
    {
      public const string CallCallbackPath = "/bandwidth/callback/call";
      public const string MessageCallbackPath = "/bandwidth/callback/message";
      public static IEnumerable<Application> ListAll(this IApplication application) 
      {
        return application.List(new ApplicationQuery{Size = 1000});
      }

      public static Application GetByName(this IApplication application, string name) 
      {
        return application.ListAll().FirstOrDefault(a => a.Name == name);
      }

      public static async Task<string> GetOrCreateAsync(this IApplication application, CreateApplicationData data, string host, bool useHttps = true, CancellationToken? cancellationToken = null) 
      {
        data.Name = $"{data.Name} on {host}";
        var app = application.GetByName(data.Name);
        if (app != null) 
        {
          return app.Id;
        }
        var baseUrl = $"http{(useHttps ? "s" : "")}://{host}";
        data.IncomingCallUrl = data.IncomingCallUrl ?? $"{baseUrl}{CallCallbackPath}";
        data.IncomingMessageUrl = data.IncomingMessageUrl ?? $"{baseUrl}{MessageCallbackPath}";
        data.AutoAnswer = data.AutoAnswer ?? true;
        data.CallbackHttpMethod = data.CallbackHttpMethod ?? CallbackHttpMethod.Post;
        return await application.CreateAsync(data, cancellationToken);
      }

      public static Task<string> GetOrCreateAsync(this IApplication application, string  name, string host, bool useHttps = true, CancellationToken? cancellationToken = null) 
      {
        return application.GetOrCreateAsync(new CreateApplicationData{Name = name}, host, useHttps, cancellationToken);
      }
    }
}
