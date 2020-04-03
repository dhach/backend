﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Pirat.Model.Entity;

namespace Pirat.Model
{
    public abstract class ItemBase : Resource
    {

        [JsonProperty]
        [Required]
        [FromQuery(Name = "category")]
        public string category { get; set; }

        [JsonProperty]
        [FromQuery(Name = "name")]
        public string name { get; set; }

        [JsonProperty]
        [FromQuery(Name = "manufacturer")]
        public string manufacturer { get; set; }

        [JsonProperty]
        [FromQuery(Name = "ordernumber")]
        public string ordernumber { get; set; }

        [JsonProperty]
        [FromQuery(Name = "amount")]
        public int amount { get; set; }

        [JsonProperty]
        [FromQuery(Name = "annotation")]
        public string annotation { get; set; }


        public override string ToString()
        {
            var s = "{category=" + category + ", name=" + name + ", manufacturer=" + manufacturer
                + ", ordernumber=" + ordernumber + ", amount=" + amount + ", annotation=" + annotation + "}";
            return s;
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as ItemBase);
        }
        
        public bool Equals(ItemBase other)
        {
            return other != null 
                   && base.Equals(other)
                   && category.Equals(other.category, StringComparison.Ordinal)
                   && name.Equals(other.name, StringComparison.Ordinal)
                   && manufacturer.Equals(other.manufacturer, StringComparison.Ordinal)
                   && ordernumber.Equals(other.ordernumber, StringComparison.Ordinal)
                   && amount == other.amount
                   && annotation.Equals(other.annotation, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(category, name, manufacturer, ordernumber, amount, annotation);
        }
    }

    public class Item : ItemBase
    {
        public Address address { get; set; }

        [JsonProperty]
        [FromQuery(Name = "kilometer")]
        public int kilometer { get; set; }


        public override string ToString()
        {
            string baseString = base.ToString();
            string s = "{base=" + baseString + ", address=" + address + ", kilometer=" + kilometer + "}";
            return s;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as Item);
        }

        public bool Equals(Item other)
        {
            return other != null && base.Equals(other) && address.Equals(other.address) && kilometer == other.kilometer;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), address, kilometer);
        }

    }
    
}
