﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using Pirat.Codes;
using Pirat.DatabaseContext;
using Pirat.Exceptions;
using Pirat.Model;
using Pirat.Model.Entity;

namespace Pirat.Services.Resource
{
    public class ResourceUpdateService : IResourceUpdateService
    {
        private readonly ILogger<ResourceUpdateService> _logger;

        private readonly DemandContext _context;

        private readonly IAddressMaker _addressMaker;

        private readonly QueryHelper _queryHelper;


        public ResourceUpdateService(ILogger<ResourceUpdateService> logger, DemandContext context, IAddressMaker addressMaker)
        {
            _logger = logger;
            _context = context;
            _addressMaker = addressMaker;

            _queryHelper = new QueryHelper(context);

        }

        /// <summary>
        /// Inserts a consumable entity into the database which gets <c>offerId</c> as foreign key for the offer entity it relates to.
        /// In addition, an address entity is created based on the address information in <c>consumable</c> and is inserted into the
        /// database with relation to the new consumable entity.
        /// </summary>
        /// <param name="offerId">The unique id of the offer</param>
        /// <param name="consumable">The consumable from which the entity is created</param>
        /// <returns></returns>
        private async Task Insert(int offerId, Consumable consumable)
        {
            var consumableEntity = new ConsumableEntity().build(consumable);
            var addressEntity = new AddressEntity().build(consumable.address);

            _addressMaker.SetCoordinates(addressEntity);
            addressEntity.Insert(_context);

            consumableEntity.offer_id = offerId;
            consumableEntity.address_id = addressEntity.id;
            consumableEntity.Insert(_context);

            consumable.id = consumableEntity.id;
        }

        /// <summary>
        /// Inserts a device entity into the database which gets <c>offerId</c> as foreign key for the offer entity it relates to.
        /// In addition, an address entity is created based on the address information in <c>device</c> and is inserted into the
        /// database with relation to the new device entity.
        /// </summary>
        /// <param name="offerId">The unique id of the offer</param>
        /// <param name="device">The device from which the entity is created</param>
        /// <returns></returns>
        private async Task Insert(int offerId, Device device)
        {
            var deviceEntity = new DeviceEntity().build(device);
            var addressEntity = new AddressEntity().build(device.address);

            _addressMaker.SetCoordinates(addressEntity);
            addressEntity.Insert(_context);

            deviceEntity.offer_id = offerId;
            deviceEntity.address_id = addressEntity.id;
            deviceEntity.Insert(_context);

            device.id = deviceEntity.id;
        }

        /// <summary>
        /// Inserts a personal entity into the database which gets <c>offerId</c> as foreign key for the personal entity it relates to.
        /// In addition, an address entity is created based on the address information in <c>personal</c> and is inserted into the
        /// database with relation to the new personal entity.
        /// </summary>
        /// <param name="offerId">The unique id of the offer</param>
        /// <param name="personal">The personal from which the entity is created</param>
        /// <returns></returns>
        private async Task Insert(int offerId, Personal personal)
        {
            var personEntity = new PersonalEntity().build(personal);
            var addressEntity = new AddressEntity().build(personal.address);

            _addressMaker.SetCoordinates(addressEntity);
            addressEntity.Insert(_context);

            personEntity.offer_id = offerId;
            personEntity.address_id = addressEntity.id;
            personEntity.Insert(_context);

            personal.id = personEntity.id;
        }


        public async Task<string> InsertAsync(Offer offer)
        {
            var provider = offer.provider;

            //Build as entities

            var offerEntity = new OfferEntity().build(provider);
            var offerAddressEntity = new AddressEntity().build(provider.address);

            //Create the coordinates and store the address of the offer

            _addressMaker.SetCoordinates(offerAddressEntity);
            await offerAddressEntity.InsertAsync(_context);

            //Store the offer including the address id as foreign key, the token and a timestamp
            offerEntity.address_id = offerAddressEntity.id;
            offerEntity.token = createToken();
            offerEntity.timestamp = DateTime.Now;
            await offerEntity.InsertAsync(_context);

            //create the entities for the resources, calculate their coordinates, give them the offer foreign key and store them
            //Update the original offer with the ids from the created entities (helps us for testing and if we want to do more stuff with the offer in future features)

            int offer_id = offerEntity.id;

            if (!(offer.consumables is null))
            {
                foreach (var c in offer.consumables)
                {
                    Insert(offer_id, c);
                }
            }
            if (!(offer.personals is null))
            {
                foreach (var p in offer.personals)
                {
                    Insert(offer_id, p);
                }
            }
            if (!(offer.devices is null))
            {
                foreach (var d in offer.devices)
                {
                    Insert(offer_id, d);
                }
            }

            //Give back only the token
            return offerEntity.token;
        }

        public async Task DeleteAsync(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length != Constants.TokenLength)
            {
                throw new ArgumentException(Error.ErrorCodes.INVALID_TOKEN);
            }

            var offer = await _queryHelper.RetrieveOfferFromTokenAsync(token);

            //Delete the offer. The resources have the offer id as foreign key and get deleted as well by the db.
            await offer.DeleteAsync(_context);
        }

        public async Task<int> ChangeInformationAsync(string token, Provider provider)
        {

            AddressEntity location = new AddressEntity().build(provider.address);
            _addressMaker.SetCoordinates(location);

            var query = 
                from o in _context.offer as IQueryable<OfferEntity>
                join ap in _context.address on o.address_id equals ap.id
                where o.token == token
                select new {o, ap};

            foreach(var collection in await query.ToListAsync())
            {
                collection.o.name = provider.name;
                //o.ispublic = provider.ispublic; //TODO everything non public so far
                collection.o.organisation = provider.organisation;
                collection.o.phone = provider.phone;
                collection.ap.OverwriteWith(location);
            }
            
            var changedRows = await _context.SaveChangesAsync();

            if (2 < changedRows)
            {
                throw new InvalidDataStateException(Error.FatalCodes.UPDATES_MADE_IN_TOO_MANY_ROWS);
            }

            return changedRows;
        }

        public async Task<int> ChangeInformationAsync(string token, Consumable consumable)
        {
            AddressEntity location = new AddressEntity().build(consumable.address);
            _addressMaker.SetCoordinates(location);

            var query = 
                from o in _context.offer as IQueryable<OfferEntity>
                join c in _context.consumable on o.id equals c.offer_id
                join ac in _context.address on c.address_id equals ac.id
                where o.token == token && c.id == consumable.id
                select new {o, c, ac};

            foreach(var collection in await query.ToListAsync())
            {
                collection.c.annotation = consumable.annotation;
                collection.c.unit = consumable.unit;
                collection.c.name = consumable.name;
                collection.c.manufacturer = consumable.manufacturer;
                collection.c.ordernumber = consumable.ordernumber;
                collection.ac.OverwriteWith(location);
            }

            var changedRows = await _context.SaveChangesAsync();

            if (2 < changedRows)
            {
                throw new InvalidDataStateException(Error.FatalCodes.UPDATES_MADE_IN_TOO_MANY_ROWS);
            }

            return changedRows;
        }

        public async Task<int> ChangeInformationAsync(string token, Device device)
        {
            AddressEntity location = new AddressEntity().build(device.address);
            _addressMaker.SetCoordinates(location);

            var query = 
                from o in _context.offer as IQueryable<OfferEntity>
                join d in _context.device on o.id equals d.offer_id
                join ad in _context.address on d.address_id equals ad.id
                where o.token == token && d.id == device.id
                select new { o, d, ad };

            foreach (var collection in await query.ToListAsync())
            {
                collection.d.annotation = device.annotation;
                collection.d.name = device.name;
                collection.d.manufacturer = device.manufacturer;
                collection.d.ordernumber = device.ordernumber;
                collection.ad.OverwriteWith(location);
            }

            var changedRows = await _context.SaveChangesAsync();

            if (2 < changedRows)
            {
                throw new InvalidDataStateException(Error.FatalCodes.UPDATES_MADE_IN_TOO_MANY_ROWS);
            }

            return changedRows;
        }

        public async Task<int> ChangeInformationAsync(string token, Personal personal)
        {
            AddressEntity location = new AddressEntity().build(personal.address);
            _addressMaker.SetCoordinates(location);

            var query = 
                from o in _context.offer as IQueryable<OfferEntity>
                join p in _context.personal on o.id equals p.offer_id
                join ap in _context.address on p.address_id equals ap.id
                where o.token == token && p.id == personal.id
                select new {o, p, ap};

            foreach (var collection in await query.ToListAsync())
            {
                collection.p.qualification = personal.qualification;
                collection.p.institution = personal.institution;
                collection.p.area = personal.area;
                collection.p.researchgroup = personal.researchgroup;
                collection.p.annotation = personal.annotation;
                collection.p.experience_rt_pcr = personal.experience_rt_pcr;
                collection.ap.OverwriteWith(location);
            }

            var changedRows = await _context.SaveChangesAsync();

            if (2 < changedRows)
            {
                throw new InvalidDataStateException(Error.FatalCodes.UPDATES_MADE_IN_TOO_MANY_ROWS);
            }

            return changedRows;
        }

        public Task ChangeConsumableAmountAsync(string token, int consumableId, int newAmount)
        {
            return ChangeConsumableAmountAsync(token, consumableId, newAmount, "");
        }

        public async Task ChangeConsumableAmountAsync(string token, int consumableId, int newAmount, string reason)
        {
            // Get consumable from database
            var query = 
                from o in _context.offer as IQueryable<OfferEntity>
                join c in _context.consumable on o.id equals c.offer_id
                where token.Equals(o.token)
                        && c.id == consumableId
                select c;
            var foundConsumables = await query.ToListAsync();

            if (foundConsumables.Count == 0)
            {
                throw new DataNotFoundException(Error.ErrorCodes.NOTFOUND_CONSUMABLE);
            }
            ConsumableEntity consumable = foundConsumables[0];
            
            // If amount has not changed: do nothing
            if (consumable.amount == newAmount)
            {
                return;
            }

            int diffAmount = Math.Abs(newAmount - consumable.amount);
            
            // If amount has increased: no reason required
            if (consumable.amount < newAmount)
            {
                consumable.amount = newAmount;
                await consumable.UpdateAsync(_context);
                
                // Add log
                await new ChangeEntity()
                {
                    change_type = "INCREASE_AMOUNT",
                    element_id = consumable.id,
                    element_type = "consumable",
                    diff_amount = diffAmount,
                    reason = reason,
                    timestamp = DateTime.Now
                }.InsertAsync(_context);
                
                return;
            }
            
            // If amount has decreased: ensure that a reason is provided
            if (reason.Trim().Length == 0)
            {
                throw new ArgumentException(Error.ErrorCodes.INVALID_REASON);
            }
            if (newAmount < 1)
            {
                throw new ArgumentException(Error.ErrorCodes.INVALID_AMOUNT_CONSUMABLE);
            }
            consumable.amount = newAmount;
            await consumable.UpdateAsync(_context);
            
            // Add log
            await new ChangeEntity()
            {
                change_type = "DECREASE_AMOUNT",
                element_id = consumable.id,
                element_type = "consumable",
                diff_amount = diffAmount,
                reason = reason,
                timestamp = DateTime.Now
            }.InsertAsync(_context);
        }

        public Task ChangeDeviceAmountAsync(string token, int deviceId, int newAmount)
        {
            return ChangeDeviceAmountAsync(token, deviceId, newAmount, null);
        }

        public async Task ChangeDeviceAmountAsync(string token, int deviceId, int newAmount, string reason)
        {
            // Get consumable from database
            var query = 
                from o in _context.offer as IQueryable<OfferEntity>
                join d in _context.device on o.id equals d.offer_id
                where token.Equals(o.token)
                      && d.id == deviceId
                select d;

            var foundDevices = await query.ToListAsync();

            if (foundDevices.Count == 0)
            {
                throw new DataNotFoundException(Error.ErrorCodes.NOTFOUND_CONSUMABLE);
            }

            DeviceEntity device = foundDevices[0];
            
            // If amount has not changed: do nothing
            if (device.amount == newAmount)
            {
                return;
            }

            int diffAmount = Math.Abs(newAmount - device.amount);

            // If amount has increased: no reason required
            if (device.amount < newAmount)
            {
                device.amount = newAmount;
                await device.UpdateAsync(_context);
                
                // Add log
                await new ChangeEntity()
                {
                    change_type = "INCREASE_AMOUNT",
                    element_id = device.id,
                    element_type = "device",
                    diff_amount = diffAmount,
                    reason = reason,
                    timestamp = DateTime.Now
                }.InsertAsync(_context);
                
                return;
            }
            
            // If amount has decreased: ensure that a reason is provided
            if (reason.Trim().Length == 0)
            {
                throw new ArgumentException(Error.ErrorCodes.INVALID_REASON);
            }
            if (newAmount < 1)
            {
                throw new ArgumentException(Error.ErrorCodes.INVALID_AMOUNT_DEVICE);
            }
            device.amount = newAmount;
            await device.UpdateAsync(_context);

            // Add log
            await new ChangeEntity()
            {
                change_type = "DECREASE_AMOUNT",
                element_id = device.id,
                element_type = "device",
                diff_amount = diffAmount,
                reason = reason,
                timestamp = DateTime.Now
            }.InsertAsync(_context);
        }

        public async Task AddResourceAsync(string token, Consumable consumable)
        {
            OfferEntity offerEntity = _queryHelper.retrieveOfferFromToken(token);

            await InsertAsync(offerEntity.id, consumable);
        }

        public async Task AddResourceAsync(string token, Device device)
        {
            OfferEntity offerEntity = _queryHelper.retrieveOfferFromToken(token);

            await InsertAsync(offerEntity.id, device);
        }

        public async Task AddResourceAsync(string token, Personal personal)
        {
            OfferEntity offerEntity = _queryHelper.retrieveOfferFromToken(token);

            await InsertAsync(offerEntity.id, personal);
        }

        public async Task MarkConsumableAsDeletedAsync(string token, int consumableId, string reason)
        {
            if (reason.Trim().Length == 0)
            {
                throw new ArgumentException(Error.ErrorCodes.INVALID_REASON);
            }

            ConsumableEntity consumableEntity = (ConsumableEntity) await new ConsumableEntity().FindAsync(_context, consumableId);
            AddressEntity addressEntity = (AddressEntity) await new AddressEntity().FindAsync(_context, consumableEntity.address_id);


            if (consumableEntity is null)
            {
                throw new DataNotFoundException(Error.ErrorCodes.NOTFOUND_CONSUMABLE);
            }
            if (addressEntity is null)
            {
                throw new InvalidDataStateException(Error.FatalCodes.RESOURCE_WITHOUT_RELATED_ADDRESS);
            }

            consumableEntity.is_deleted = true;
            await consumableEntity.UpdateAsync(_context);

            addressEntity.is_deleted = true;
            await addressEntity.UpdateAsync(_context);

            await new ChangeEntity()
            {
                change_type = ChangeEntity.ChangeType.DeleteResource,
                element_id = consumableEntity.id,
                element_type = ChangeEntity.ElementType.Consumable,
                diff_amount = consumableEntity.amount,
                reason = reason,
                timestamp = DateTime.Now
            }.InsertAsync(_context);
        }

        public async Task MarkDeviceAsDeletedAsync(string token, int deviceId, string reason)
        {
            if (reason.Trim().Length == 0)
            {
                throw new ArgumentException(Error.ErrorCodes.INVALID_REASON);
            }

            DeviceEntity deviceEntity = (DeviceEntity) await new DeviceEntity().FindAsync(_context, deviceId);
            AddressEntity addressEntity = (AddressEntity) await new AddressEntity().FindAsync(_context, deviceEntity.address_id);

            if (deviceEntity is null)
            {
                throw new DataNotFoundException(Error.ErrorCodes.NOTFOUND_DEVICE);
            }
            if (addressEntity is null)
            {
                throw new InvalidDataStateException(Error.FatalCodes.RESOURCE_WITHOUT_RELATED_ADDRESS);
            }

            deviceEntity.is_deleted = true;
            await deviceEntity.UpdateAsync(_context);

            addressEntity.is_deleted = true;
            await addressEntity.UpdateAsync(_context);

            await new ChangeEntity()
            {
                change_type = ChangeEntity.ChangeType.DeleteResource,
                element_id = deviceEntity.id,
                element_type = ChangeEntity.ElementType.Device,
                diff_amount = deviceEntity.amount,
                reason = reason,
                timestamp = DateTime.Now
            }.InsertAsync(_context);
        }

        public async Task MarkPersonalAsDeletedAsync(string token, int personalId, string reason)
        {
            if (reason.Trim().Length == 0)
            {
                throw new ArgumentException(Error.ErrorCodes.INVALID_REASON);
            }

            PersonalEntity personalEntity = (PersonalEntity)await new PersonalEntity().FindAsync(_context, personalId);
            AddressEntity addressEntity = (AddressEntity)await new AddressEntity().FindAsync(_context, personalEntity.address_id);

            if (personalEntity is null)
            {
                throw new DataNotFoundException(Error.ErrorCodes.NOTFOUND_PERSONAL);
            }

            if (addressEntity is null)
            {
                throw new InvalidDataStateException(Error.FatalCodes.RESOURCE_WITHOUT_RELATED_ADDRESS);
            }

            personalEntity.is_deleted = true;
            await personalEntity.UpdateAsync(_context);

            addressEntity.is_deleted = true;
            await addressEntity.UpdateAsync(_context);

            await new ChangeEntity()
            {
                change_type = ChangeEntity.ChangeType.DeleteResource,
                element_id = personalEntity.id,
                element_type = ChangeEntity.ElementType.Personal,
                diff_amount = 1,
                reason = reason,
                timestamp = DateTime.Now
            }.InsertAsync(_context);
        }

        private string createToken()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, Constants.TokenLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
