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
    public class ApplicationTest
    {
        [Fact]
        public void ListAllTest()
        {
          var context = new MockContext<IApplication>();
          context.Arrange(m => m.List(The<ApplicationQuery>.Is(q => q.Size.Value == 1000), null)).Returns(new Application[1]);
          var application = new MockApplication(context);
          var items = application.ListAll();
          Assert.Equal(1, items.Count());
        }

        [Fact]
        public void GetByNameTest()
        {
          var context = new MockContext<IApplication>();
          context.Arrange(m => m.List(The<ApplicationQuery>.Is(q => q.Size.Value == 1000), null)).Returns(new []{
            new Application
            {
              Id = "appId1",
              Name = "app"
            }
          });
          var application = new MockApplication(context);
          var item = application.GetByName("app");
          Assert.Equal("appId1", item.Id);
        }

        [Fact]
        public async void GetOrCreateAsyncTest()
        {
          var context = new MockContext<IApplication>();
          context.Arrange(m => m.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new []{
            new Application
            {
              Id = "appId2",
              Name = "app on localhost"
            }
          });
          var application = new MockApplication(context);
          var id = await application.GetOrCreateAsync(new CreateApplicationData
          {
            Name = "app"
          }, "localhost");
          Assert.Equal("appId2", id);
        }

        [Fact]
        public async void GetOrCreateAsyncTest2()
        {
          var context = new MockContext<IApplication>();
          context.Arrange(m => m.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new Application[0]);
          context.Arrange(m => m.CreateAsync(The<CreateApplicationData>.Is(q => q.IncomingCallUrl == "https://localhost/bandwidth/callback/call"), null)).Returns(Task.FromResult("appId3"));
          var application = new MockApplication(context);
          var id = await application.GetOrCreateAsync(new CreateApplicationData
          {
            Name = "app"
          }, "localhost");
          Assert.Equal("appId3", id);
        }

        [Fact]
        public async void GetOrCreateAsyncTest3()
        {
          var context = new MockContext<IApplication>();
          context.Arrange(m => m.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new Application[0]);
          context.Arrange(m => m.CreateAsync(The<CreateApplicationData>.Is(q => q.IncomingCallUrl == "https://localhost/bandwidth/callback/call"), null)).Returns(Task.FromResult("appId4"));
          var application = new MockApplication(context);
          var id = await application.GetOrCreateAsync("app", "localhost");
          Assert.Equal("appId4", id);
        }

    }
}
