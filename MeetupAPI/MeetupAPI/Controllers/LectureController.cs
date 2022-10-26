﻿using AutoMapper;
using MeetupAPI.Entities;
using MeetupAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetupAPI.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/meetup/{meetupName}/lecture")]
    [ApiController]
    public class LectureController : ControllerBase
    {
        private readonly MeetupContext _meetupContext;
        private readonly IMapper _mapper;
        private readonly ILogger<LectureController> _logger;

        public LectureController(MeetupContext meetupContext, IMapper mapper, ILogger<LectureController> logger)
        {
            _meetupContext = meetupContext;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(string meetupName, int id)
        {
            var meetup = _meetupContext.Meetups
                .Include(m => m.Lectures)
                .FirstOrDefault(m => m.Name.Replace(" ", "-").ToLower() == meetupName.ToLower());

            if (meetup == null)
            {
                return NotFound();
            }

            var lecture = meetup.Lectures.FirstOrDefault(l => l.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            _meetupContext.Lectures.Remove(lecture);
            _meetupContext.SaveChanges();

            return NoContent();
        }

        [HttpDelete]
        public ActionResult Delete(string meetupName)
        {
            // throw new Exception("test");

            var meetup = _meetupContext.Meetups
                .Include(m => m.Lectures)
                .FirstOrDefault(m => m.Name.Replace(" ", "-").ToLower() == meetupName.ToLower());

            if (meetup == null)
            {
                return NotFound();
            }

            _logger.LogWarning($"Lectures for meetup {meetupName} were deleted.");

            _meetupContext.Lectures.RemoveRange(meetup.Lectures);
            _meetupContext.SaveChanges();

            return NoContent();
        }

        [HttpGet]
        public ActionResult Get(string meetupName)
        {
            var meetup = _meetupContext.Meetups
                .Include(m => m.Lectures)
                .FirstOrDefault(m => m.Name.Replace(" ", "-").ToLower() == meetupName.ToLower());

            if (meetup == null)
            {
                return NotFound();
            }

            var lectures = _mapper.Map<List<LectureDto>>(meetup.Lectures);

            return Ok(lectures);
        }

        [HttpPost]
        public ActionResult Post(string meetupName, [FromBody]LectureDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var meetup = _meetupContext.Meetups
                .Include(m => m.Lectures)
                .FirstOrDefault(m => m.Name.Replace(" ", "-").ToLower() == meetupName.ToLower());

            if (meetup == null)
            {
                return NotFound();
            }

            var lecture = _mapper.Map<Lecture>(model);
            meetup.Lectures.Add(lecture);

            _meetupContext.SaveChanges();

            return Created($"api/meetup/{meetupName}", null);
        }
    }
}
