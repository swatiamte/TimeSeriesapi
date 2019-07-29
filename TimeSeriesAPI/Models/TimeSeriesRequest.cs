using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSeriesAPI.Models
{
    public class TimeSeriesRequest
    {
        public string EnvironmentId { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public string TagName { get; set; }
    }
    public class GetAvailabilityRequest
    {
        public string EnvironmentId { get; set; }
    }
    public class GetMetaDataRequest
    {
        public string EnvironmentId { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
    }

}