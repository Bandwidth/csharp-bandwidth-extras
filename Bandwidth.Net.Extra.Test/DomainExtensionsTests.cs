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
    public class DomainTest
    {
        [Fact]
        public void ListAllTest()
        {
          var context = new MockContext<IDomain>();
          context.Arrange(m => m.List(The<DomainQuery>.Is(q => q.Size == 100), null)).Returns(new Domain[1]);
          var domain = new MockDomain(context);
          var items = domain.ListAll();
          Assert.Equal(1, items.Count());
        }

        [Fact]
        public void GetByNameTest()
        {
          var context = new MockContext<IDomain>();
          context.Arrange(m => m.List(The<DomainQuery>.Is(q => q.Size == 100), null)).Returns(new []{
            new Domain
            {
              Id = "domainId1",
              Name = "domain"
            }
          });
          var domain = new MockDomain(context);
          var item = domain.GetByName("domain");
          Assert.Equal("domainId1", item.Id);
        }

        [Fact]
        public async void GetOrCreateAsyncTest()
        {
          var context = new MockContext<IDomain>();
          context.Arrange(m => m.List(The<DomainQuery>.IsAnyValue, null)).Returns(new []{
            new Domain
            {
              Id = "domainId2",
              Name = "domain"
            }
          });
          var domain = new MockDomain(context);
          var id = await domain.GetOrCreateAsync("domain");
          Assert.Equal("domainId2", id);
        }

        [Fact]
        public async void GetOrCreateAsyncTest2()
        {
          var context = new MockContext<IDomain>();
          context.Arrange(m => m.List(The<DomainQuery>.IsAnyValue, null)).Returns(new Domain[0]);
          context.Arrange(m => m.CreateAsync(The<CreateDomainData>.Is(q => q.Name == "domain"), null)).Returns(Task.FromResult("domainId3"));
          var domain = new MockDomain(context);
          var id = await domain.GetOrCreateAsync("domain");
          Assert.Equal("domainId3", id);
        }
    }
}
