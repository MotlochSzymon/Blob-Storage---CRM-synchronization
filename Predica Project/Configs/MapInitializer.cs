using AutoMapper;
using Predica.ExternalModels;
using Predica.Xrm.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predica.Configs
{
    public static class MapInitializer
    {
        public static IMapper Activate()
        {
            var map = new MapperConfiguration(stp =>
            {
                stp.CreateMap<ConferenceLead, Lead>()
                .ForMember(x => x.new_yearofbirth,
                m => m.MapFrom(a => DateTime.Now.Year - a.Age))
                .ForMember(x => x.new_conferencebegindate,
                m => m.MapFrom(a => a.ConferenceBeginDate))
                .ForMember(x => x.new_conferenceenddate,
                m => m.MapFrom(a => a.ConferenceEndDate))
                .ForMember(x => x.EMailAddress1,
                m => m.MapFrom(a => a.Email))
                 .ForMember(x => x.Address1_Country,
                m => m.MapFrom(a => a.Country))
                ;
            });

            return map.CreateMapper();
        }
    }
}
