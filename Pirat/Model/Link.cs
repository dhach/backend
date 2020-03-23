﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pirat.Model
{
    public class Link
    {
        [Key]
        public string token { get; set; }

        public int provider_id { get; set; }

        public int[] consumable_ids { get; set; }

        public int[] device_ids { get; set; }

        public int[] manpower_ids { get; set; }
    }
}
