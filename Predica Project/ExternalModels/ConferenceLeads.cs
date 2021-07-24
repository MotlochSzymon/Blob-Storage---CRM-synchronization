using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predica.ExternalModels
{
    public class ConferenceLeadsList
    {
        [JsonProperty("leads")]
        public List<ConferenceLead> Leads { get; set; }
    }

    public class ConferenceLead
    {
        [JsonProperty("name")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("conferenceBegin")]
        public DateTime? ConferenceBeginDate { get; set; }

        [JsonProperty("conferenceEnd")]
        public DateTime? ConferenceEndDate { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
}
