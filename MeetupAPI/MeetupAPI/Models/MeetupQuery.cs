using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetupAPI.Models
{
    public class MeetupQuery
    {
        public string SearchPhrase { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
