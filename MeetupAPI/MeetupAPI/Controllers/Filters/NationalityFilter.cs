using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace MeetupAPI.Controllers.Filters
{
    public class NationalityFilter : Attribute, IAuthorizationFilter
    {
        private string[] _nationalities;

        public NationalityFilter(string nationalities)
        {
            this._nationalities = nationalities.Split(',');
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var nationality = context.HttpContext.User.FindFirst(c => c.Type == "Nationality").Value;

            if (!_nationalities.Any(c => c == nationality))
            {
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}
