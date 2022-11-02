using AutoMapper;
using MeetupAPI.Authorization;
using MeetupAPI.Controllers.Filters;
using MeetupAPI.Entities;
using MeetupAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MeetupAPI.Controllers
{
    // [Route("api/meetup")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    // [TimeTrackFilter]
    [ServiceFilter(typeof(TimeTrackFilter))]
    public class MeetupController : ControllerBase
    {
        private readonly MeetupContext _meetupContext;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public MeetupController(MeetupContext meetupContext, IMapper mapper, IAuthorizationService authorizationService)
        {
            _meetupContext = meetupContext;
            _mapper = mapper;
            _authorizationService = authorizationService;
        }
        
        [HttpGet]
        [AllowAnonymous]
        // [NationalityFilter("German,Russian")]
        public ActionResult<PageResult<MeetupDetailsDto>> GetAll([FromQuery]MeetupQuery query)
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

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var baseQuery = _meetupContext.Meetups
                .Include(m => m.Location)
                .Where(m => query.SearchPhrase == null ||
                    (m.Organizer.ToLower().Contains(query.SearchPhrase) || 
                    m.Name.ToLower().Contains(query.SearchPhrase)));

            var meetups = baseQuery
                .Skip(query.PageSize * (query.PageNumber -1))
                .Take(query.PageSize)
                .ToList();

            var totalCount = baseQuery.Count();

            var meetupDtos = _mapper.Map<List<MeetupDetailsDto>>(meetups);

            var result = new PageResult<MeetupDetailsDto>(meetupDtos, totalCount, query.PageNumber, query.PageSize);

            return Ok(result);
        }

        [HttpGet("{name}")]
        [NationalityFilter("English")]
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

            var userId = User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;

            meetup.CreatedById = int.Parse(userId);

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

            var authorizationResult = _authorizationService.AuthorizeAsync(User, meetup, new ResourceOperationRequirement(OperationType.Update))
                .Result;

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
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

            var authorizationResult = _authorizationService.AuthorizeAsync(User, meetup, new ResourceOperationRequirement(OperationType.Delete))
                .Result;

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            _meetupContext.Remove(meetup);

            _meetupContext.SaveChanges();

            return NoContent();
        }
    }
}
