using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StationBoardsWebAPIAssigment2.Models
{
    public class DepartureBoardRowDTO
    {
        public string StationName { get; set; }
        public string Destination { get; set; }
        public string ScheduledTime { get; set; }
        public string Expected { get; set; }
        public string Platform { get; set; }
        public string Via { get; set; }
    }
}