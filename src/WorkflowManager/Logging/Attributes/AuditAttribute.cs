using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Monai.Deploy.WorkflowManager.Logging.Logging;

namespace Monai.Deploy.WorkflowManager.Logging.Attributes
{
    public class LogActionFilterAttribute : ActionFilterAttribute
    {
        private readonly ILogger _logger;

        /// <summary>  
        /// Initializes a new instance of the <see cref="LogActionFilterAttribute" /> class.  
        /// </summary>  
        /// <param name="logger">The logger.</param> 
        public LogActionFilterAttribute(ILogger<LogActionFilterAttribute> logger)
        {
            _logger = logger;
        }

        /// <summary>  
        /// Called when [action executing].  
        /// </summary>  
        /// <param name="context">The current executing context.</param>  
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogControllerStartTime(context);
            base.OnActionExecuting(context);
        }

        /// <summary>  
        /// Called when [result executed].  
        /// </summary>  
        /// <param name="context">The current executing context.</param>  
        public override void OnResultExecuted(ResultExecutedContext context)
        {
            _logger.LogControllerStartTime(context);
            base.OnResultExecuted(context);
        }
    }
}
