using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TimeSeriesAPI.Models;

namespace TimeSeriesAPI.Controllers
{
    public class TimeSeriesController : ApiController
    {

        [HttpGet]
        public async Task<HttpResponseMessage> GetEnvironmentList()
        {
            try
            {

                IEnumerable<EnvironmentListResponse> responseContent = await TimeSeriesInsightsLogic.GetEnvironmentList();

                return Request.CreateResponse(HttpStatusCode.OK, responseContent);
            }
            catch (Exception ex)
            {
                //var message = ex.Message;
                //throw new TimeSeriesException(400, ex.Message, HttpStatusCode.NoContent);
                var errorMessagError = new System.Web.Http.HttpError(ex.Message) { { "ErrorCode", 400 } ,{"Error Discription" ,"Internal Server Error" } };

                throw new HttpResponseException(ControllerContext.Request.CreateErrorResponse
                    (HttpStatusCode.MethodNotAllowed, errorMessagError));
            }


        }

        [HttpPost]
        public async Task<Dictionary<string, DateTime>> GetAvailability([FromBody]GetAvailabilityRequest EnvironmentRequest)
        {
            Dictionary<string, DateTime> lstResponse = new Dictionary<string, DateTime>();
              lstResponse = await TimeSeriesInsightsLogic.GetAvailability(EnvironmentRequest.EnvironmentId);
            return lstResponse;
        }
        [HttpGet]
        public async Task<Dictionary<string, DateTime>> GetAsync()
        {
            Dictionary<string, DateTime> lstResponse = new Dictionary<string, DateTime>();
            lstResponse = await TimeSeriesInsightsLogic.SampleAsync();
            return lstResponse;
        }
        [HttpPost]
        public async Task<IEnumerable<TimeSeriesResponse>> getEventData([FromBody]TimeSeriesRequest TimeSeriesRequestData)
        {

            IEnumerable<TimeSeriesResponse> responseContent = await TimeSeriesInsightsLogic.getEventData(TimeSeriesRequestData.EnvironmentId, TimeSeriesRequestData.from, TimeSeriesRequestData.to, TimeSeriesRequestData.TagName);
            return responseContent;
        }
        [HttpPost]
        public async Task<IEnumerable<MetaDataPropertiesResponse>> getMetaData([FromBody]GetMetaDataRequest MetadataRequestData)
        {

            IEnumerable<MetaDataPropertiesResponse> responseContent = await TimeSeriesInsightsLogic.GetMetaData(MetadataRequestData.EnvironmentId,MetadataRequestData.from, MetadataRequestData.to);
            return responseContent;
        }

    }
}
