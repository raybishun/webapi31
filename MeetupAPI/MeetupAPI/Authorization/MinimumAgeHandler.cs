using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MeetupAPI.Authorization
{
    public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        private readonly ILogger<MinimumAgeHandler> _logger;

        public MinimumAgeHandler(ILogger<MinimumAgeHandler> logger)
        {
            this._logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
            var userEmail = context.User.FindFirst(c => c.Type == ClaimTypes.Name).Value;
            var dateOfBirth = DateTime.Parse(context.User.FindFirst(c => c.Type == "DateOfBirth").Value);

            _logger.LogInformation($"Handling minimum age requirement for: {userEmail}. [datofBirth: {dateOfBirth}].");

            if (dateOfBirth.AddYears(requirement.MinimumAge) <= DateTime.Today)
            {
                _logger.LogInformation("Access granted.");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogInformation("Access denied.");
            }

            return Task.CompletedTask;
        }
    }
}
