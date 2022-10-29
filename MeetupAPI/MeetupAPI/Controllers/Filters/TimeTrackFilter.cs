using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace MeetupAPI.Controllers.Filters
{
    public class TimeTrackFilter : IActionFilter
    {
        private readonly ILogger<TimeTrackFilter> _logger;
        private Stopwatch _stopwatch;

        public TimeTrackFilter(ILogger<TimeTrackFilter> logger)
        {
            this._logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch.Stop();

            var milliseconds = _stopwatch.ElapsedMilliseconds;
            var action = context.ActionDescriptor.DisplayName;

            // Debug.WriteLine($"Action [{action}], executed in: {milliseconds} milliseconds.");
            _logger.LogInformation($"Action [{action}], executed in: {milliseconds} milliseconds.");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }
    }
}
