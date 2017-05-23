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
    public class PhoneNumberTest
    {
        [Fact]
        public void ListAllForApplicationTest()
        {
          var context = new MockContext<IPhoneNumber>();
          context.Arrange(m => m.List(The<PhoneNumberQuery>.Is(q => q.Size.Value == 1000 && q.ApplicationId == "appId"), null)).Returns(new PhoneNumber[1]);
          var phoneNumber = new MockPhoneNumber(context);
          var items = phoneNumber.ListAllForApplication("appId");
          Assert.Equal(1, items.Count());
        }

        [Fact]
        public void GetByNameTest()
        {
          var context = new MockContext<IPhoneNumber>();
          context.Arrange(m => m.List(The<PhoneNumberQuery>.Is(q => q.Size.Value == 1000 && q.ApplicationId == "appId" && q.Name == "name"), null)).Returns(new []{new PhoneNumber()});
          var phoneNumber = new MockPhoneNumber(context);
          var item = phoneNumber.GetByName("appId", "name");
          Assert.NotNull(item);
        }

        [Fact]
        public async void CreateLocalAsyncTest()
        {
          var context = new MockContext<IPhoneNumber>();
          var availableNumberContext = new MockContext<IAvailableNumber>();
          context.Arrange(m => m.UpdateAsync("phoneNumberId", The<UpdatePhoneNumberData>.Is(d => d.ApplicationId == "appId" && d.Name == "name"), null)).Returns(Task.FromResult(0));
          availableNumberContext.Arrange(m => m.SearchAndOrderLocalAsync(The<LocalNumberQueryForOrder>.Is(q => q.AreaCode == "910" && q.Quantity.Value == 1), null)).Returns(Task.FromResult(
            new[] 
            {
              new OrderedNumber
              {
                Number = "+1234567890",
                Location = "http://lovalhost/phoneNumberId"
              }
            }
          ));
          var phoneNumber = new MockPhoneNumber(context);
          var number = await phoneNumber.CreateLocalAsync(new MockAvailableNumber(availableNumberContext), "appId", new LocalNumberQueryForOrder
          {
            AreaCode = "910"
          }, "name", null);
          Assert.Equal("+1234567890", number);
        }

        [Fact]
        public async void CreateTollFreeAsyncTest()
        {
          var context = new MockContext<IPhoneNumber>();
          var availableNumberContext = new MockContext<IAvailableNumber>();
          context.Arrange(m => m.UpdateAsync("phoneNumberId", The<UpdatePhoneNumberData>.Is(d => d.ApplicationId == "appId" && d.Name == "name"), null)).Returns(Task.FromResult(0));
          availableNumberContext.Arrange(m => m.SearchAndOrderTollFreeAsync(The<TollFreeNumberQueryForOrder>.Is(q => q.Quantity.Value == 1), null)).Returns(Task.FromResult(
            new[] 
            {
              new OrderedNumber
              {
                Number = "+1234567890",
                Location = "http://lovalhost/phoneNumberId"
              }
            }
          ));
          var phoneNumber = new MockPhoneNumber(context);
          var number = await phoneNumber.CreateTollFreeAsync(new MockAvailableNumber(availableNumberContext), "appId", "name", null);
          Assert.Equal("+1234567890", number);
        }

        [Fact]
        public async void GetOrCreateLocalAsyncTest()
        {
          var context = new MockContext<IPhoneNumber>();
          var availableNumberContext = new MockContext<IAvailableNumber>();
          context.Arrange(m => m.List(The<PhoneNumberQuery>.Is(q => q.Size.Value == 1000 && q.ApplicationId == "appId" && q.Name == "name"), null)).Returns(new PhoneNumber[0]);
          context.Arrange(m => m.UpdateAsync("phoneNumberId", The<UpdatePhoneNumberData>.Is(d => d.ApplicationId == "appId" && d.Name == "name"), null)).Returns(Task.FromResult(0));
          availableNumberContext.Arrange(m => m.SearchAndOrderLocalAsync(The<LocalNumberQueryForOrder>.Is(q => q.AreaCode == "910" && q.Quantity.Value == 1), null)).Returns(Task.FromResult(
            new[] 
            {
              new OrderedNumber
              {
                Number = "+1234567890",
                Location = "http://lovalhost/phoneNumberId"
              }
            }
          ));
          var phoneNumber = new MockPhoneNumber(context);
          var number = await phoneNumber.GetOrCreateLocalAsync(new MockAvailableNumber(availableNumberContext), "appId", new LocalNumberQueryForOrder
          {
            AreaCode = "910"
          }, "name", null);
          Assert.Equal("+1234567890", number);
        }
        [Fact]
        public async void GetOrCreateLocalAsyncTest2()
        {
          var context = new MockContext<IPhoneNumber>();
          var availableNumberContext = new MockContext<IAvailableNumber>();
          context.Arrange(m => m.List(The<PhoneNumberQuery>.Is(q => q.Size.Value == 1000 && q.ApplicationId == "appId" && q.Name == "name"), null)).Returns(new []{new PhoneNumber{Number = "+1234567891"}});
          var phoneNumber = new MockPhoneNumber(context);
          var number = await phoneNumber.GetOrCreateLocalAsync(new MockAvailableNumber(availableNumberContext), "appId", new LocalNumberQueryForOrder
          {
            AreaCode = "910"
          }, "name", null);
          Assert.Equal("+1234567891", number);
        }

        [Fact]
        public async void GetOrCreateTollFreeAsyncTest()
        {
          var context = new MockContext<IPhoneNumber>();
          var availableNumberContext = new MockContext<IAvailableNumber>();
          context.Arrange(m => m.List(The<PhoneNumberQuery>.Is(q => q.Size.Value == 1000 && q.ApplicationId == "appId" && q.Name == "name"), null)).Returns(new PhoneNumber[0]);
          context.Arrange(m => m.UpdateAsync("phoneNumberId", The<UpdatePhoneNumberData>.Is(d => d.ApplicationId == "appId" && d.Name == "name"), null)).Returns(Task.FromResult(0));
          availableNumberContext.Arrange(m => m.SearchAndOrderTollFreeAsync(The<TollFreeNumberQueryForOrder>.Is(q => q.Quantity.Value == 1), null)).Returns(Task.FromResult(
            new[] 
            {
              new OrderedNumber
              {
                Number = "+1234567890",
                Location = "http://lovalhost/phoneNumberId"
              }
            }
          ));
          var phoneNumber = new MockPhoneNumber(context);
          var number = await phoneNumber.GetOrCreateTollFreeAsync(new MockAvailableNumber(availableNumberContext), "appId", "name", null);
          Assert.Equal("+1234567890", number);
        }
        [Fact]
        public async void GetOrCreateTollFreeAsyncTest2()
        {
          var context = new MockContext<IPhoneNumber>();
          var availableNumberContext = new MockContext<IAvailableNumber>();
          context.Arrange(m => m.List(The<PhoneNumberQuery>.Is(q => q.Size.Value == 1000 && q.ApplicationId == "appId" && q.Name == "name"), null)).Returns(new []{new PhoneNumber{Number = "+1234567891"}});
          var phoneNumber = new MockPhoneNumber(context);
          var number = await phoneNumber.GetOrCreateTollFreeAsync(new MockAvailableNumber(availableNumberContext), "appId", "name", null);
          Assert.Equal("+1234567891", number);
        }

    }
}
