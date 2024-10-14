using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;

using Contracts;

using MassTransit;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController( AuctionDbContext context,IMapper mapper,IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions(string date)
        {
            var query=_context.Auctions.OrderBy(x=> x.Item.Make).AsQueryable();
            if(!string.IsNullOrEmpty(date))
            {
                query=query.Where(x=> x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime())>0);
            }
           /*  var auctions=await _context.Auctions
                               .Include(a=> a.Item)
                               .OrderBy(x=>x.Item.Make)
                               .ToListAsync();
            
            return _mapper.Map<List<AuctionDTO>>(auctions); */
            return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid Id)
        {
            var auction=await _context.Auctions
                               .Include(a=> a.Item)
                               .FirstOrDefaultAsync(x=> x.Id==Id);

            if(auction==null) return NotFound();
            
            return _mapper.Map<AuctionDTO>(auction);
        }

         [HttpPost]
        public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO createAuctionDTO)
        {
            var auction=_mapper.Map<Auction>(createAuctionDTO);
            auction.Seller="Test";
            _context.Auctions.Add(auction);
              var newAuction=_mapper.Map<AuctionDTO>(auction);
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            var result= await _context.SaveChangesAsync()>0;
          
            if(!result) return BadRequest("Data could not be saved");

            if(auction==null) return NotFound();
            
            return  CreatedAtAction(nameof(GetAuctionById), new {auction.Id},newAuction);
            
        }

        [HttpPut]
        public async Task<ActionResult> UodateAuction (Guid Id, UpdateAuctionDTO updateAuctionDTO)
        {
            var auction= await _context.Auctions.Include(x=>x.Item)
                          .FirstOrDefaultAsync();
            
            if(auction==null) return NotFound();
            auction.Item.Make=updateAuctionDTO.Make?? auction.Item.Make;
            auction.Item.Model=updateAuctionDTO.Model?? auction.Item.Model;
            auction.Item.Color=updateAuctionDTO.Color?? auction.Item.Color;
            auction.Item.Milage=updateAuctionDTO.Milage?? auction.Item.Milage;
            auction.Item.Year=updateAuctionDTO.Year?? auction.Item.Year;
            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
             var result= await _context.SaveChangesAsync()>0;
             if(result) return Ok();

             return BadRequest("data not saved");
        }

        [HttpDelete]
        public async Task<ActionResult<AuctionDTO>> DeleteAuction( Guid Id)
        {
            var auction= await _context.Auctions.FindAsync(Id);

            if(auction==null) return NotFound();

            _context.Auctions.Remove(auction);
            await _publishEndpoint.Publish<AuctionDeleted>(new AuctionDeleted{Id=auction.Id.ToString()} );
            var result= await _context.SaveChangesAsync()>0;

            if(!result) return BadRequest("Data could not be deleted");

            if(auction==null) return NotFound();
            
             return Ok();
        }

      
    }
}