﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pirat.DatabaseContext;
using Pirat.Model.Api.Resource;
using Pirat.Model.Entity.Resource.Common;
using Pirat.Model.Entity.Resource.Demands;
using Pirat.Services.Helper;

namespace Pirat.Services.Resource.Demand
{
    public class ResourceDemandQueryService : IResourceDemandQueryService
    {
        private readonly ILogger<IResourceDemandQueryService> _logger;

        private readonly ResourceContext _context;

        private readonly IAddressMaker _addressMaker;

        private readonly QueryHelper _queryHelper;


        public ResourceDemandQueryService(ILogger<ResourceDemandQueryService> logger, ResourceContext context, IAddressMaker addressMaker)
        {
            _logger = logger;
            _context = context;
            _addressMaker = addressMaker;

            _queryHelper = new QueryHelper(context);
        }


        public async IAsyncEnumerable<DemandResource<Consumable>> QueryDemandsAsync(Consumable con)
        {
            var consumable = new ConsumableDemandEntity().Build(con);


            var maxDistance = con.kilometer;
            AddressEntity locationOfDemandedConsumable = null;
            if (!string.IsNullOrEmpty(con.address.country) && !string.IsNullOrEmpty(con.address.postalcode))
            {
                var consumableAddress = con.address;
                locationOfDemandedConsumable = new AddressEntity().build(consumableAddress);
                _addressMaker.SetCoordinates(locationOfDemandedConsumable);
            }
            
            var query = from demand in _context.demand as IQueryable<DemandEntity>
                join c in _context.demand_consumable on demand.id equals c.demand_id
                join ad in _context.address on demand.address_id equals ad.id into tmp
                from ad in tmp.DefaultIfEmpty()
                where consumable.category.Equals(c.category) && !c.is_deleted
                select new {demand, c, ad};


            if (!string.IsNullOrEmpty(consumable.name))
            {
                query = query.Where(collection => consumable.name.Equals(collection.c.name));
            }

            if (!string.IsNullOrEmpty(consumable.manufacturer))
            {
                query = query.Where(collection => consumable.manufacturer.Equals(collection.c.manufacturer));
            }

            if (consumable.amount > 0)
            {
                query = query.Where(collection => consumable.amount <= collection.c.amount);
            }

            var results = await query.ToListAsync();

            foreach (var data in results)
            {
                var resource = new Consumable().build(data.c);

                // If the query specifies a location but the demand does not, the demand should not be considered.
                if (locationOfDemandedConsumable != null && data.ad == null)
                {
                    continue;
                }

                if (locationOfDemandedConsumable != null)
                {
                    var yLatitude = data.ad.latitude;
                    var yLongitude = data.ad.longitude;
                    var distance = DistanceCalculator.computeDistance(
                        locationOfDemandedConsumable.latitude, locationOfDemandedConsumable.longitude,
                        yLatitude, yLongitude);
                    if (distance > maxDistance && maxDistance != 0)
                    {
                        continue;
                    }
                    resource.kilometer = (int) Math.Round(distance);
                }
                
                var provider = new Provider().Build(data.demand);
                if (data.ad != null)
                {
                    provider.address = new Address().Build(data.ad);
                }

                var demand = new DemandResource<Consumable>()
                {
                    provider = provider,
                    resource = resource
                };

                yield return demand;
            }
        }

        public async IAsyncEnumerable<DemandResource<Device>> QueryDemandsAsync(Device dev)
        {
            var device = new DeviceDemandEntity().Build(dev);


            var maxDistance = dev.kilometer;
            AddressEntity locationOfDemandedDevice = null;
            if (!string.IsNullOrEmpty(dev.address.country) && !string.IsNullOrEmpty(dev.address.postalcode))
            {
                var deviceAddress = dev.address;
                locationOfDemandedDevice = new AddressEntity().build(deviceAddress);
                _addressMaker.SetCoordinates(locationOfDemandedDevice);
            }


            var query = from demand in _context.demand as IQueryable<DemandEntity>
                join d in _context.demand_device on demand.id equals d.demand_id
                join ad in _context.address on demand.address_id equals ad.id into tmp
                from ad in tmp.DefaultIfEmpty()
                where device.category.Equals(d.category) && !d.is_deleted
                select new {demand, d, ad};

            if (!string.IsNullOrEmpty(device.name))
            {
                query = query.Where(collection => device.name.Equals(collection.d.name));
            }

            if (!string.IsNullOrEmpty(device.manufacturer))
            {
                query = query.Where(collection => device.manufacturer.Equals(collection.d.manufacturer));
            }

            if (device.amount > 0)
            {
                query = query.Where(collection => device.amount <= collection.d.amount);
            }

            List<DemandResource<Device>> resources = new List<DemandResource<Device>>();
            var results = await query.ToListAsync();

            foreach (var data in results)
            {
                var resource = new Device().Build(data.d);

                // If the query specifies a location but the demand does not, the demand should not be considered.
                if (locationOfDemandedDevice != null && data.ad == null)
                {
                    continue;
                }
                
                if (locationOfDemandedDevice != null)
                {
                    var yLatitude = data.ad.latitude;
                    var yLongitude = data.ad.longitude;
                    var distance = DistanceCalculator.computeDistance(
                        locationOfDemandedDevice.latitude, locationOfDemandedDevice.longitude,
                        yLatitude, yLongitude);
                    if (distance > maxDistance && maxDistance != 0)
                    {
                        continue;
                    }
                    resource.kilometer = (int) Math.Round(distance);
                }
                
                var provider = new Provider().Build(data.demand);
                if (data.ad != null)
                {
                    provider.address = new Address().Build(data.ad);
                }

                var demand = new DemandResource<Device>()
                {
                    provider = provider,
                    resource = resource
                };

                yield return demand;
            }
        }
    }
}