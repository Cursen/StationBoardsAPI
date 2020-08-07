using Newtonsoft.Json;
using StationBoardsWebAPIAssigment2.Models;
using StationBoardsWebAPIAssigment2.NationalRailServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StationBoardsWebAPIAssigment2.Controllers
{
    public class DepartureBoardRowDTOController : ApiController
    {
        [HttpGet]
        [Route("api/stations/GetDepartureBoard")]
        //The main link to ask for, It calls the sollution for getting metro and train rows, Sorts them then returns the list.
        public List<DepartureBoardRowDTO> GetDepartureBoardsAsync(string railStationList, string metrolinkStationList)
        {

            List<DepartureBoardRowDTO> rows = new List<DepartureBoardRowDTO>();
            //prevents error for spliting the string.
            if (railStationList != null)
            {
                string[] railList = railStationList.Split(',');
                for (int i = 0; i < railList.Length; i++)
                {
                    rows.AddRange(GetTrainDepartureBoards(railList[i]));
                }
            }
            if (metrolinkStationList != null)
            {
                string[] metroList = metrolinkStationList.Split(',');

                for (int i = 0; i < metroList.Length; i++)
                {
                    rows.AddRange(GetMetroDepartureBoards(metroList[i]));
                }
            }
            rows.Sort((x, y) => x.ScheduledTime.CompareTo(y.ScheduledTime));
            return rows;
        }
        [HttpGet]
        [Route("api/stations/GetMetroBoards")]
        //Process of getting the metro rows.
        public List<DepartureBoardRowDTO> GetMetroDepartureBoards(String stationCode)
        {
            List<DepartureBoardRowDTO> rows = new List<DepartureBoardRowDTO>();
            //connection to api
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "9ecb7dc5cae84e60a13e23e5d4904e97");
            var uri = $"https://api.tfgm.com/odata/Metrolinks?$filter=TLAREF eq '{stationCode}'";
            try
            {
                var response = client.GetAsync(uri).Result;
                //if something went wrong on api side, run same error style as if try,catch setup.
                if (response.IsSuccessStatusCode)
                {
                    var serialisedContent = response.Content.ReadAsStringAsync().Result;
                    var deserialsedContent = JsonConvert.DeserializeObject<MetrolinkRoot>(serialisedContent);
                    foreach (var screen in deserialsedContent.value)
                    {
                        //4 objects due to the screen setup of having up to 4 incoming trains per stored as strings.
                        DepartureBoardRowDTO tram0 = new DepartureBoardRowDTO
                        {
                            StationName = screen.StationLocation,
                            Destination = screen.Dest0,
                            Expected = screen.Status0,
                            Platform = "TRAM"
                        };

                        DepartureBoardRowDTO tram1 = new DepartureBoardRowDTO
                        {
                            StationName = screen.StationLocation,
                            Destination = screen.Dest1,
                            Expected = screen.Status1,
                            Platform = "TRAM"
                        };

                        DepartureBoardRowDTO tram2 = new DepartureBoardRowDTO
                        {
                            StationName = screen.StationLocation,
                            Destination = screen.Dest2,
                            Expected = screen.Status2,
                            Platform = "TRAM"
                        };

                        DepartureBoardRowDTO tram3 = new DepartureBoardRowDTO
                        {
                            StationName = screen.StationLocation,
                            Destination = screen.Dest3,
                            Expected = screen.Status3,
                            Platform = "TRAM"
                        };
                        //use these to figure out when its supposed to arrive.
                        var now = screen.LastUpdated;
                        var time = now;
                        //check if it is allready stored in the list, and make sure it is not a empty train, since they have their destination set to "" always.
                        if (tram0.Destination != "")
                        {
                            time = now.AddMinutes(Int32.Parse(screen.Wait0));
                            tram0.ScheduledTime = time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
                            if (!rows.Exists(t => t.ScheduledTime == tram0.ScheduledTime && t.Destination == tram0.Destination))
                            {
                                rows.Add(tram0);
                            }
                        }
                        if (tram0.Destination != "")
                        {
                            time = now.AddMinutes(Int32.Parse(screen.Wait1));
                            tram1.ScheduledTime = time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
                            if (!rows.Exists(t => t.ScheduledTime == tram1.ScheduledTime && t.Destination == tram1.Destination))
                            {
                                rows.Add(tram1);
                            }
                        }
                        if (tram2.Destination != "")
                        {
                            time = now.AddMinutes(Int32.Parse(screen.Wait2));
                            tram2.ScheduledTime = time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
                            if (!rows.Exists(t => t.ScheduledTime == tram2.ScheduledTime && t.Destination == tram2.Destination))
                            {
                                rows.Add(tram2);
                            }
                        }
                        if (tram3.Destination != "")
                        {
                            time = now.AddMinutes(Int32.Parse(screen.Wait3));
                            tram3.ScheduledTime = time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
                            if (!rows.Exists(t => t.ScheduledTime == tram3.ScheduledTime && t.Destination == tram3.Destination))
                            {
                                rows.Add(tram3);
                            }
                        }
                    }
                }
                else
                {
                    rows.Add(Error("TRAM"));
                }
            }
            catch
            {
                rows.Add(Error("TRAM"));
            }
            //quick change for Due to on Time.
            foreach (var r in rows.Where(r => r.Expected == "Due"))
            {
                r.Expected = "On time";
            }
            return rows;
        }
        [HttpGet]
        [Route("api/stations/GetTrainBoards")]
        //process of getting the Train rows.
        public List<DepartureBoardRowDTO> GetTrainDepartureBoards(String stationCode)
        {
            List<DepartureBoardRowDTO> rows = new List<DepartureBoardRowDTO>();
            try
            {
                //connectiong to Train api.
                var nrServiceClient = new LDBServiceSoapClient();
                StationBoardWithDetails2 board = nrServiceClient.GetDepBoardWithDetails(
                    new AccessToken { TokenValue = "94e96b3b-ee5d-43cb-ac3b-d49b5520579c" },
                    100,
                    stationCode,
                    String.Empty,
                    FilterType.to,
                    0,
                    120);
                //process of adding to the list rows.
                foreach (var s in board.trainServices)
                {
                    //remove what is gonna be set as Destination(since it is not to be included in via later)
                    var k = s.subsequentCallingPoints.FirstOrDefault().callingPoint.ToList();
                    k.Reverse();
                    k.Remove(k.First());
                    k.Reverse();

                    //Test of doing it through editing strings, was replaced with Reversing and removing.
                    /*string.join(string,array)
                    for(int i = 0; i<s.subsequentCallingPoints.FirstOrDefault().callingPoint.Length-2; i++)
                    {
                        points = points + s.subsequentCallingPoints.FirstOrDefault().callingPoint[i].locationName;
                        points = points + ", ";
                    }
                    points = points + s.subsequentCallingPoints.FirstOrDefault().callingPoint[s.subsequentCallingPoints.FirstOrDefault().callingPoint.Length - 2].locationName;
                    */

                    //figure out the time to give it.
                    var now = board.generatedAt;
                    var sched = s.std;
                    TimeSpan ts = TimeSpan.Parse(sched);
                    var expect = now.Date + ts;

                    //create flatened object and add to list of rows.
                    DepartureBoardRowDTO dep = new DepartureBoardRowDTO
                    {
                        StationName = board.locationName,
                        Destination = s.subsequentCallingPoints.LastOrDefault().callingPoint.LastOrDefault().locationName,
                        Via = String.Join(", ", k.Select(name => name.locationName)),
                        ScheduledTime = expect.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                        Expected = s.etd,
                        Platform = s.platform
                    };
                    rows.Add(dep);
                }
            }
            catch
            {
                rows.Add(Error("TRAIN"));
            }
            return rows;
        }
        //if something goes wrong at any point, then just return 1 object containing error message.
        public DepartureBoardRowDTO Error(string type)
        {
            var error = new DepartureBoardRowDTO();
            if (type == "TRAM")
            {
                error.Via = "live data not available please call helpline: 02036150600";
                error.Platform = type;
            }
            else
            {
                error.Via = "live data not available please call helpline: 03457114141";
                error.Platform = "TRAIN";
            }
            return error;
        }
    }
}
