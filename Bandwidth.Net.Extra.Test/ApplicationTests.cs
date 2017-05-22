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
    }
}
