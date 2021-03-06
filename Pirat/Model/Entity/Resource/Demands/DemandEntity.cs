﻿using System;
using System.Threading.Tasks;
using Pirat.DatabaseContext;
using Pirat.Model.Api.Resource;
using Pirat.Model.Entity.Resource.Common;
using Pirat.Other;

namespace Pirat.Model.Entity.Resource.Demands
{
    public class DemandEntity : IFindable, IDeletable, IUpdatable, IInsertable
    {
        //***Key
        public int id { get; set; }

        public string institution { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;

        public string mail { get; set; } = string.Empty;

        public string phone { get; set; } = string.Empty;

        //OPTIONAL
        public int? address_id { get; set; }


        //public bool ispublic { get; set; }

        //***Link token
        public string token { get; set; }

        public DateTime created_at_timestamp { get; set; }


        public DemandEntity Build(Provider p)
        {
            NullCheck.ThrowIfNull<Provider>(p);
            name = p.name;
            institution = p.organisation;
            phone = p.phone;
            mail = p.mail;
            return this;
        }

        public async Task<IFindable> FindAsync(ResourceContext context, int id)
        {
            NullCheck.ThrowIfNull<ResourceContext>(context);
            return await context.demand.FindAsync(id);
        }

        public async Task DeleteAsync(ResourceContext context)
        {
            NullCheck.ThrowIfNull<ResourceContext>(context);
            context.demand.Remove(this);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ResourceContext context)
        {
            NullCheck.ThrowIfNull<ResourceContext>(context);
            context.demand.Update(this);
            await context.SaveChangesAsync();
        }

        public async Task<IInsertable> InsertAsync(ResourceContext context)
        {
            NullCheck.ThrowIfNull<ResourceContext>(context);
            context.demand.Add(this);
            await context.SaveChangesAsync();
            return this;
        }
    }
}
