﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pirat.Model.Entity.Resource.Demands;
using Pirat.Model.Entity.Resource.Stock;

namespace Pirat.Model.Api.Resource
{
    public class Provider
    {
        [JsonProperty] [Required] public string name { get; set; } = string.Empty;

        [JsonProperty] [Required] public string organisation { get; set; } = string.Empty;

        [JsonProperty] public string phone { get; set; } = string.Empty;

        [JsonProperty] [Required] public string mail { get; set; } = string.Empty;

        [JsonProperty] [Required] public bool ispublic { get; set; }

        [JsonProperty] public Address address { get; set; }

        [JsonProperty]
        [FromQuery(Name = "kilometer")]
        public int kilometer { get; set; }

        public Provider Build(OfferEntity o)
        {
            name = o.name;
            organisation = o.organisation;
            phone = o.phone;
            mail = o.mail;
            ispublic = o.ispublic;
            return this;
        }

        public Provider Build(DemandEntity d)
        {
            name = d.name;
            organisation = d.institution;
            phone = d.phone;
            mail = d.mail;
            return this;
        }

        public Provider Build(Address a)
        {
            address = a;
            return this;
        }

        public Provider Build(int kilometer)
        {
            this.kilometer = kilometer;
            return this;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as Provider);
        }

        public bool Equals(Provider other)
        {
            return other != null
                   && string.Equals(name, other.name, StringComparison.Ordinal)
                   && string.Equals(organisation, other.organisation, StringComparison.Ordinal)
                   && string.Equals(phone, other.phone, StringComparison.Ordinal)
                   && string.Equals(mail, other.mail, StringComparison.Ordinal)
                   && ispublic == other.ispublic
                   && Equals(address, other.address)
                   && kilometer == other.kilometer;
        }

        public override string ToString()
        {
            return "Provider={" +
                   $"name={name}, organisation={organisation}, phone={phone}, mail={mail}, ispublic={ispublic}, address={address}, kilometer={kilometer}" +
                   "}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, organisation, phone, mail, ispublic, address, kilometer);
        }
    }
}