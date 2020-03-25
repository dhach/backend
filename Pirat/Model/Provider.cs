﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pirat.Model.Entity;

namespace Pirat.Model
{
    public abstract class ProviderBase
    {
        [JsonProperty]
        [Required]
        public string name { get; set; }

        [JsonProperty]
        [Required]
        public string organisation { get; set; }

        [JsonProperty]
        public string phone { get; set; }

        [JsonProperty]
        [Required]
        public string mail { get; set; }
    }


    public class Provider : ProviderBase
    {

        [JsonProperty]
        public Address address { get; set; }

        [JsonProperty]
        [FromQuery(Name = "kilometer")]
        public int kilometer { get; set; }

        public static Provider of(ProviderEntity p)
        {
            return new Provider()
            {
                name = p.name,
                organisation = p.organisation,
                phone = p.phone,
                mail = p.mail
            };
        }

        public Provider build(Address a)
        {
            address = a;
            return this;
        }

        public Provider build(int kilometer)
        {
            this.kilometer = kilometer;
            return this;
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }
            if(!(obj is Provider))
            {
                return false;
            }
            var p = (Provider)obj;
            return (name.Equals(p.name) && organisation.Equals(p.organisation) && address.Equals(p.address) && mail.Equals(p.mail) && phone.Equals(p.phone));
        }

    }

    
}
