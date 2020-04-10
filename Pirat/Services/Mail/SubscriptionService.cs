﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pirat.DatabaseContext;
using Pirat.Model;

namespace Pirat.Services.Mail
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly DemandContext _context;

        private readonly IMailService _mailService;

        private readonly IAddressMaker _addressMaker;


        public SubscriptionService(DemandContext context, IMailService mailService, IAddressMaker addressMaker)
        {
            _context = context;
            _mailService = mailService;
            _addressMaker = addressMaker;
        }

        public void SubscribeRegion(RegionSubscription subscription)
        {
            AddressEntity addressEntity = new AddressEntity()
            {
                postalcode = subscription.postalcode,
                country = "Deutschland"
            };
            _addressMaker.SetCoordinates(addressEntity);
            subscription.latitude = addressEntity.latitude;
            subscription.longitude = addressEntity.longitude;
            subscription.active = true;
            subscription.Insert(_context);
        }

        public async Task SendEmails()
        {
            const int MAX_DISTANCE = 50;

            var queryAllSubscriptions = from rs in _context.region_subscription
                where rs.active
                select new {rs};
            var allSubscriptions = queryAllSubscriptions.Select(x => x.rs).ToList();

            var queryAllRecentDevices = from o in _context.offer
                join d in _context.device on o.id equals d.offer_id
                join a in _context.address on d.address_id equals a.id
                where (o.timestamp > DateTime.Now.AddDays(-1))
                select new { device = d, address = a };
            var allRecentDevices = queryAllRecentDevices.ToList();
            var queryAllRecentConsumables = from o in _context.offer
                join c in _context.consumable on o.id equals c.offer_id
                join a in _context.address on c.address_id equals a.id
                where (o.timestamp > DateTime.Now.AddDays(-1))
                select new { consumable = c, address = a };
            var allRecentConsumables = queryAllRecentConsumables.ToList();
            var queryAllRecentPersonnel = from o in _context.offer
                join p in _context.personal on o.id equals p.offer_id
                join a in _context.address on p.address_id equals a.id
                where (o.timestamp > DateTime.Now.AddDays(-1))
                select new { personnel = p, address = a };
            var allRecentPersonnel = queryAllRecentPersonnel.ToList();

            var postalCodeToSubscriptionsDictionary = new Dictionary<string, List<RegionSubscription>>();
            var postalCodeToResources = new Dictionary<string, ResourceList>();

            // Group subscriptions by postal code
            foreach (RegionSubscription subscription in allSubscriptions)
            {
                var ss = postalCodeToSubscriptionsDictionary.GetValueOrDefault(subscription.postalcode,
                    new List<RegionSubscription>());
                ss.Add(subscription);
                postalCodeToSubscriptionsDictionary[subscription.postalcode] = ss;
            }

            // Prepare data structures
            foreach (var (postalCode, _) in postalCodeToSubscriptionsDictionary)
            {
                postalCodeToResources[postalCode] = new ResourceList();
            }

            // Compute the distance between all recently offered resources to the relevant postal codes
            // and assign the resources to the postal codes if they are in close proximity.
            foreach (var da in allRecentDevices)
            {
                foreach (var (postalCode, ss) in postalCodeToSubscriptionsDictionary)
                {
                    double distance = ComputeDistance(da.address.latitude, da.address.longitude, 
                        ss[0].latitude, ss[0].longitude);
                    if (distance <= MAX_DISTANCE)
                    {
                        postalCodeToResources[postalCode].devices.Add(new Device().build(da.device));
                    }
                }
            }
            foreach (var ca in allRecentConsumables)
            {
                foreach (var (postalCode, ss) in postalCodeToSubscriptionsDictionary)
                {
                    double distance = ComputeDistance(ca.address.latitude, ca.address.longitude,
                        ss[0].latitude, ss[0].longitude);
                    if (distance <= MAX_DISTANCE)
                    {
                        postalCodeToResources[postalCode].consumables.Add(new Consumable().build(ca.consumable));
                    }
                }
            }
            foreach (var pa in allRecentPersonnel)
            {
                foreach (var (postalCode, ss) in postalCodeToSubscriptionsDictionary)
                {
                    double distance = ComputeDistance(pa.address.latitude, pa.address.longitude,
                        ss[0].latitude, ss[0].longitude);
                    if (distance <= MAX_DISTANCE)
                    {
                        postalCodeToResources[postalCode].personals.Add(new Personal().build(pa.personnel));
                    }
                }
            }

            // Send emails
            foreach (RegionSubscription subscription in allSubscriptions)
            {
                ResourceList resources = postalCodeToResources[subscription.postalcode];
                if (!resources.isEmpty())
                {
                    await this._mailService.sendNotificationAboutNewOffers(subscription, postalCodeToResources[subscription.postalcode]);
                }
            }
            await Task.CompletedTask;
        }


        private double ComputeDistance(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
        {
            //made a short sketch on paper and just got the formula ;)
            int earthRadius = 6371; //km
            var latitudeRadian = ConvertDegreesToRadians(decimal.ToDouble(latitude2 - latitude1));
            var longitudeRadian = ConvertDegreesToRadians(decimal.ToDouble(longitude2 - longitude1));


            var a = Math.Sin(latitudeRadian / 2) * Math.Sin(latitudeRadian / 2) +
                    Math.Cos(ConvertDegreesToRadians(decimal.ToDouble(latitude1))) *
                    Math.Cos(ConvertDegreesToRadians(decimal.ToDouble(latitude2))) * Math.Sin(longitudeRadian / 2) *
                    Math.Sin(longitudeRadian / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = earthRadius * c;
            return d;
        }


        private static double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }


        public class ResourceList
        {
            public List<Device> devices { get; set; } = new List<Device>();

            public List<Consumable> consumables { get; set; } = new List<Consumable>();

            public List<Personal> personals { get; set; } = new List<Personal>();


            public bool isEmpty()
            {
                return (devices.Count == 0) && (consumables.Count == 0) && (personals.Count == 0);
            }
        }
    }
}