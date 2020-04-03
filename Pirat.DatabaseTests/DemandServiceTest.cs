using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Pirat.DatabaseContext;
using Pirat.DatabaseTests.Examples;
using Pirat.Exceptions;
using Pirat.Model;
using Pirat.Model.Entity;
using Pirat.Services;
using Xunit;

namespace Pirat.DatabaseTests
{
    public class DemandServiceTest : IDisposable
    {

        private const string connectionString =
            "Server=localhost;Port=5432;Database=postgres;User ID=postgres;Password=postgres";

        private static DbContextOptions<DemandContext> options =
            new DbContextOptionsBuilder<DemandContext>().UseNpgsql(connectionString).Options;

        private static readonly DemandContext DemandContext = new DemandContext(options);

        private readonly DemandService _demandService;

        private readonly CaptainHookGenerator _captainHookGenerator;

        /// <summary>
        /// Called before each test
        /// </summary>
        public DemandServiceTest()
        {
            var logger = new Mock<ILogger<DemandService>>();
            var addressMaker = new Mock<IAddressMaker>();
            addressMaker.Setup(m => m.SetCoordinates(It.IsAny<AddressEntity>())).Callback((AddressEntity a) =>
            {
                a.latitude = new decimal(0.0);
                a.longitude = new decimal(0.0);
                a.hascoordinates = true;
            });
            _demandService = new DemandService(logger.Object, DemandContext, addressMaker.Object);
            _captainHookGenerator = new CaptainHookGenerator();
        }

        /// <summary>
        /// Called after each test
        /// </summary>
        public void Dispose()
        {
            //Nothing to do
        }


        [Fact]
        public void InsertOffer_QueryOfferElements_DeleteOffer()
        {
            //Insert the offer
            var offer = _captainHookGenerator.generateOffer();
            var token = _demandService.insert(offer).Result;
            Assert.True(token.Length == 30);

            //Query the link
            var entity = _demandService.queryLink(token).Result;
            Assert.Equal(offer.provider.name, entity.provider.name);

            //Now query the elements. If it is not empty we received the element back

            //Get device
            var queryDevice = _captainHookGenerator.GenerateDevice();
            var resultDevices = _demandService.QueryOffers(queryDevice).Result;
            Assert.NotNull(resultDevices);
            Assert.NotEmpty(resultDevices);
            var device = resultDevices.First();
            Assert.Equal(offer.devices.First().category, device.resource.category);
            Assert.Equal(offer.devices.First().name, device.resource.name);

            //Get consumable
            var queryConsumable = _captainHookGenerator.GenerateConsumable();
            var resultConsumables = _demandService.QueryOffers(queryConsumable).Result;
            Assert.NotNull(resultConsumables);
            Assert.NotEmpty(resultDevices);
            var consumable = resultConsumables.First();
            Assert.Equal(offer.consumables.First().category, consumable.resource.category);
            Assert.Equal(offer.consumables.First().name, consumable.resource.name);

            //Get personal
            var manpowerQuery = _captainHookGenerator.GenerateManpower();
            var resultPersonal = _demandService.QueryOffers(manpowerQuery).Result;
            Assert.NotNull(resultPersonal);
            Assert.NotEmpty(resultPersonal);
            var personal = resultPersonal.First();
            Assert.Equal(offer.personals.First().area, personal.resource.area);
            Assert.Equal(offer.personals.First().qualification, personal.resource.qualification);

            //Delete the offer and check if it worked
            var exception = Record.Exception(() => _demandService.delete(token).Result);
            Assert.Null(exception);
        }

        [Fact(Skip = "Not implemented")]
        public void InsertOffer_BadInputs() //TODO fix  this after merging
        {
            var offer = _captainHookGenerator.generateOffer();
            offer.provider.name = "";
            Assert.Throws<ArgumentException>(() => _demandService.insert(offer).Result);

            offer = _captainHookGenerator.generateOffer();
            offer.consumables.First().unit = "";
            Assert.Throws<ArgumentException>(() => _demandService.insert(offer).Result);

            offer = _captainHookGenerator.generateOffer();
            offer.devices.First().category = "";
            Assert.Throws<ArgumentException>(() => _demandService.insert(offer).Result);

            offer = _captainHookGenerator.generateOffer();
            offer.personals.First().qualification = "";
            Assert.Throws<ArgumentException>(() => _demandService.insert(offer).Result);

            offer = _captainHookGenerator.generateOffer();
            offer.personals.First().area = "";
            Assert.Throws<ArgumentException>(() => _demandService.insert(offer).Result);
        }

        [Fact]
        public void QueryLink_NotExist()
        {
            var token = "oooooWoooooWoooooWoooooWoooooW";
            Assert.Throws<DataNotFoundException>(() => _demandService.queryLink(token).Result);
        }



    }


}

