﻿using System;
using System.Threading.Tasks;
using Pirat.DatabaseContext;
using Pirat.Model.Entity;

namespace Pirat.Model
{
    public class ChangeEntity : IInsertable
    {
        public int id { get; set; }
        
        public string element_type { get; set; }
        
        public int element_id { get; set; }
        
        public string change_type { get; set; }

        public string reason { get; set; } = string.Empty;
        
        public DateTime timestamp { get; set; } = DateTime.Now;

        public async Task<IInsertable> InsertAsync(DemandContext context)
        {
            context.change.Add(this);
            await context.SaveChangesAsync();
            return this;
        }

        public static class ElementType
        {
            public static string Device = "device";
            public static string Consumable = "consumable";
            public static string Personal = "personal";
        }

        public static class ChangeType
        {
            public static string IncreaseAmount = "INCREASE_AMOUNT";
            public static string DecreaseAmount = "DECREASE_AMOUNT";
            public static string DeleteResource = "DELETE_RESOURCE";
        }

    }

}