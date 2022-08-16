/*
 * Copyright 2021-2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
            _logger.LogControllerEndTime(context);
            base.OnResultExecuted(context);
        }
    }
}
