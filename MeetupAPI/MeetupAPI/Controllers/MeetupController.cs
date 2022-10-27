﻿using AutoMapper;
using MeetupAPI.Entities;
using MeetupAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MeetupAPI.Controllers
{
    // [Route("api/meetup")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeetupController : ControllerBase
    {
        private readonly MeetupContext _meetupContext;
        private readonly IMapper _mapper;

        public MeetupController(MeetupContext meetupContext, IMapper mapper)
        {
            _meetupContext = meetupContext;
            _mapper = mapper;
        }
        
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<List<MeetupDetailsDto>> Get()
        {
            #region Note
            // 3 ways to return our own status codes (other than leave it to .net to return the default)

            //// 1
            //return StatusCode(404, meetups);

            //// or 2
            //return NotFound(meetups);

            //// or 3
            //HttpContext.Response.StatusCode = 404;

            //return meetups;
            #endregion

            var meetups = _meetupContext.Meetups.Include(m => m.Location).ToList();

            var meetupDtos = _mapper.Map<List<MeetupDetailsDto>>(meetups);

            return Ok(meetupDtos);
        }

        [HttpGet("{name}")]
        // [Authorize(Policy = "HasNationality")]
        [Authorize(Policy = "AtLeast18")]
        public ActionResult<MeetupDetailsDto> Get(string name)
        {
            var meetup = _meetupContext.Meetups
                .Include(m => m.Location)
                .Include(m => m.Lectures)
                .FirstOrDefault(m => m.Name.Replace(" ", "-").ToLower() == name.ToLower());

            if (meetup == null)
            {
                return NotFound();
            }

            var meetupDto = _mapper.Map<MeetupDetailsDto>(meetup);
            return Ok(meetupDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public ActionResult Post([FromBody]MeetupDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var meetup = _mapper.Map<Meetup>(model);
            _meetupContext.Meetups.Add(meetup);
            _meetupContext.SaveChanges();

            var key = meetup.Name.Replace(" ", "-").ToLower();

            return Created("api/meetup/" + key, null);
        }

        [HttpPut("{name}")]
        public ActionResult Put(string name, [FromBody] MeetupDto model)
        {
            var meetup = _meetupContext.Meetups
                .FirstOrDefault(m => m.Name.Replace(" ", "-").ToLower() == name.ToLower());

            if (meetup == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            meetup.Name = model.Name;
            meetup.Organizer = model.Organizer;
            meetup.Date = model.Date;
            meetup.IsPrivate = model.IsPrivate;

            _meetupContext.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{name}")]
        public ActionResult Delete(string name)
        {
            var meetup = _meetupContext.Meetups
                .Include(m => m.Location)
                .FirstOrDefault(m => m.Name.Replace(" ", "-").ToLower() == name.ToLower());

            if (meetup == null)
            {
                return NotFound();
            }

            _meetupContext.Remove(meetup);

            _meetupContext.SaveChanges();

            return NoContent();
        }
    }
}
