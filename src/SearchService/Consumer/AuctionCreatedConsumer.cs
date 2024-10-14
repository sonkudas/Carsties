using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using AutoMapper;
using MongoDB.Entities;
using SearchService.Models;


namespace SearchService.Consumer
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;
        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
            
        }
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
           Console.WriteLine("Consuming Auction Created "+ context.Message.Id); 
           var item= _mapper.Map<Item>(context.Message);
           await item.SaveAsync();
        }
    }
}