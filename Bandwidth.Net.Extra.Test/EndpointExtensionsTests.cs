using System;
using System.Linq;
using Xunit;
using Bandwidth.Net.Api;
using LightMock;
using Bandwidth.Net.Extra.Test.Mocks;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bandwidth.Net.Extra.Test
{
    public class EndpointTest
    {
        [Fact]
        public void ListAllTest()
        {
          var context = new MockContext<IEndpoint>();
          context.Arrange(m => m.List("domainId", The<EndpointQuery>.Is(q => q.Size.Value == 1000), null)).Returns(new Endpoint[1]);
          var endpoint = new MockEndpoint(context);
          var items = endpoint.ListAll("domainId");
          Assert.Equal(1, items.Count());
        }

        [Fact]
        public void GetByNameTest()
        {
          var context = new MockContext<IEndpoint>();
          context.Arrange(m => m.List("domainId", The<EndpointQuery>.Is(q => q.Size.Value == 1000), null)).Returns(new []{
            new Endpoint
            {
              Id = "endpointId1",
              Name = "endpoint"
            }
          });
          var endpoint = new MockEndpoint(context);
          var item = endpoint.GetByName("domainId", "endpoint");
          Assert.Equal("endpointId1", item.Id);
        }

        [Fact]
        public async void GetOrCreateAsyncTest()
        {
          var context = new MockContext<IEndpoint>();
          context.Arrange(m => m.List("domainId", The<EndpointQuery>.IsAnyValue, null)).Returns(new []{
            new Endpoint
            {
              Id = "endpointId2",
              Name = "endpoint"
            }
          });
          var endpoint = new MockEndpoint(context);
          var item = await endpoint.GetOrCreateAsync(new CreateEndpointData
          {
            Name = "endpoint",
            ApplicationId = "appId",
            DomainId = "domainId"
          });
          Assert.Equal("endpointId2", item.Id);
        }

        [Fact]
        public async void GetOrCreateAsyncTest2()
        {
          var context = new MockContext<IEndpoint>();
          context.Arrange(m => m.List("domainId", The<EndpointQuery>.IsAnyValue, null)).Returns(new Endpoint[0]);
          context.Arrange(m => m.CreateAsync(The<CreateEndpointData>.Is(d => d.Name == "endpoint" && d.ApplicationId == "appId" && d.DomainId == "domainId"), null)).Returns(Task.FromResult("endpointId3"));
          context.Arrange(m => m.GetAsync("domainId", "endpointId3", null)).Returns(Task.FromResult(new Endpoint
          {
            Id = "endpointId3",
            Name = "endpoint"
          }));
          var endpoint = new MockEndpoint(context);
          var item = await endpoint.GetOrCreateAsync(new CreateEndpointData
          {
            Name = "endpoint",
            ApplicationId = "appId",
            DomainId = "domainId"
          });
          Assert.Equal("endpointId3", item.Id);
        }

        [Fact]
        public async void GetOrCreateAsyncTest3()
        {
          var context = new MockContext<IEndpoint>();
          context.Arrange(m => m.List("domainId", The<EndpointQuery>.IsAnyValue, null)).Returns(new Endpoint[0]);
          context.Arrange(m => m.CreateAsync(The<CreateEndpointData>.Is(d => d.Name == "endpoint" && d.ApplicationId == "appId" && d.DomainId == "domainId"), null)).Returns(Task.FromResult("endpointId4"));
          context.Arrange(m => m.GetAsync("domainId", "endpointId4", null)).Returns(Task.FromResult(new Endpoint
          {
            Id = "endpointId4",
            Name = "endpoint"
          }));
          var endpoint = new MockEndpoint(context);
          var item = await endpoint.GetOrCreateAsync("appId", "domainId", "endpoint");
          Assert.Equal("endpointId4", item.Id);
        }

    }
}
