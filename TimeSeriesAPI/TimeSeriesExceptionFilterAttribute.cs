using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace TimeSeriesAPI
{
    public class TimeSeriesExceptionFilterAttribute: ExceptionFilterAttribute, IExceptionFilter
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            //Check the Exception Type

            if (actionExecutedContext.Exception is TimeSeriesException)
            {
                var TimeSeriesExceptionobj = actionExecutedContext.Exception as TimeSeriesException;
                var errorMessagError = new System.Web.Http.HttpError(TimeSeriesExceptionobj.ErrorDescription)
                { { "ErrorCode", TimeSeriesExceptionobj.ErrorCode } };
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, errorMessagError);
            }
            else
                
            {
                var errorMessagError = new System.Web.Http.HttpError("Oops some internal Exception.Please contact your administrator") { { "ErrorCode", 500 } };
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessagError);
            }
        }
    }
}