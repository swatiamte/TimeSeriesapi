using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSeriesAPI.Models
{
    public class TimeSeriesResponse
    {
        public string date;
        public string value;

        public TimeSeriesResponse(string date, string value)
        {
            this.date = date;
            this.value = value;


        }
    }
    public class EnvironmentListResponse
    {
        public string displayName;
        public string EnviornmentId;

        public EnvironmentListResponse(string displayName, string EnviornmentId)
        {

            this.displayName = displayName;
            this.EnviornmentId = EnviornmentId;
        }
    }

    public class MetaDataPropertiesResponse
    {
        public string name;
        public MetaDataPropertiesResponse(string name)
        {
            this.name = name;
        }


    }
}