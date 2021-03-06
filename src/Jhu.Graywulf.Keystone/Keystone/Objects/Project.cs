﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Jhu.Graywulf.Keystone
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project : Entity
    {
        [JsonProperty("domain_id")]
        public string DomainID { get; set; }

        [JsonProperty("domain")]
        public Domain Domain { get; set; }
    }
}
