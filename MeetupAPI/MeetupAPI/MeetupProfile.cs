using AutoMapper;
using MeetupAPI.Entities;
using MeetupAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetupAPI
{
    public class MeetupProfile : Profile
    {
        public MeetupProfile()
        {
            CreateMap<Meetup, MeetupDetailsDto>()
                .ForMember(m => m.City, map => map.MapFrom(meetup => meetup.Location.City))
                .ForMember(m => m.PostCode, map => map.MapFrom(meetup => meetup.Location.PostCode))
                .ForMember(m => m.Street, map => map.MapFrom(meetup => meetup.Location.Street));

            CreateMap<MeetupDto, Meetup>();

            CreateMap<LectureDto, Lecture>()
                .ReverseMap();
        }
    }
}
