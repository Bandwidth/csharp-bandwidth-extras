using System;
using System.Linq;
using Xunit;
using Bandwidth.Net.Api;
using Bandwidth.Net.Extra.Test.Mocks;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using LightMock;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Bandwidth.Net.Extra.Test.Middlewares
{
    public class ServiceCollectionTests
    {
      [Fact]
      public void AddBandwidthTest()
      {
        var context = new MockContext<IServiceCollection>();
        context.Arrange(s=>s.Add(The<ServiceDescriptor>.Is(d => CheckServiceDescriptor(d))));
        var serviceCollection = new MockServiceCollection(context);
        serviceCollection.AddBandwidth(new BandwidthAuthData
        {
          UserId = "userId",
          ApiToken = "apiToken",
          ApiSecret = "apiSecret"
        });
        Assert.True(serviceCollection.Count > 0);
        Assert.NotNull(serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(IMemoryCache)));
      }

      private static readonly Type[] RegisteredTypes = new [] 
      {
        typeof(Client), 
        typeof(IAccount),
        typeof(IApplication),
        typeof(IAvailableNumber),
        typeof(IBridge),
        typeof(ICall),
        typeof(IDomain),
        typeof(IConference),
        typeof(IEndpoint),
        typeof(IError),
        typeof(IMedia),
        typeof(IMessage),
        typeof(INumberInfo),
        typeof(IPhoneNumber),
        typeof(IRecording),
        typeof(ITranscription),
        typeof(Bandwidth.Net.ApiV2.IMessage),
        typeof(IMemoryCache)
      };
      public static bool CheckServiceDescriptor(ServiceDescriptor descriptor)
      {
        return RegisteredTypes.Contains(descriptor.ServiceType) && descriptor.Lifetime == ServiceLifetime.Singleton;
      }
    }
}
