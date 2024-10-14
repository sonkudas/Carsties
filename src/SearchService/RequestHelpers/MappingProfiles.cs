using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using SearchService.Models;

namespace SearchService.RequestHelpers
{
    public class MappingProfiles :Profile
    {
        public MappingProfiles()
        {
              CreateMap<AuctionCreated,Item>();
              CreateMap<AuctionUpdated,Item>();
          /*   CreateMap<Item,AuctionDTO>();
            CreateMap<CreateAuctionDTO,Auction>()
            .ForMember(d=> d.Item,o=>o.MapFrom(s=>s));
            CreateMap<CreateAuctionDTO,Item>(); */
        }
    }
}