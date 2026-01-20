using BingMapsRESTToolkit;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OoBDev.Microsoft.BingMaps.Tests.Samples
{
    //static void Main(string[] args)
    //{
    //    Tests.GeoCodeTest();
    //    Tests.LocationRecogTest();
    //    Tests.FindTimeZoneTest();
    //    Tests.ConvertTimeZoneTest();
    //    Tests.ListTimeZoneTest();
    //}

    //https://raw.githubusercontent.com/microsoft/BingMapsRESTToolkit/master/Samples/Console/RESTToolkitTestConsoleApp/Program.cs
    [TestClass]
    [TestCategory(TestCategories.DevLocal)]
    public class RESTToolkitTestTestContextAppTests
    {
        public TestContext TestContext { get; set; }

        private const string _ApiKey = "Aq_xgo3-Ngout0eM7OmI9VYRxuI6BgiyFe6ywEG1ZY0tRcuhqvQv7Vr1FI5ivP5V";

        private void PrintTZResource(TimeZoneResponse tz)
        {

            TestContext?.WriteLine($"Name: {tz.GenericName}");
            TestContext?.WriteLine($"Windows ID: {tz.WindowsTimeZoneId}");
            TestContext?.WriteLine($"IANA ID: {tz.IANATimeZoneId}");
            TestContext?.WriteLine($"UTC offset: {tz.UtcOffset}");
            TestContext?.WriteLine($"Abbrev: {tz.Abbreviation}");

            if (tz.ConvertedTime != null)
            {
                var ctz = tz.ConvertedTime;
                TestContext?.WriteLine($"Local Time: {ctz.LocalTime}");
                TestContext?.WriteLine($"TZ Abbr: {ctz.TimeZoneDisplayAbbr} ");
                TestContext?.WriteLine($"TZ Name: {ctz.TimeZoneDisplayName}");
                TestContext?.WriteLine($"UTC offset: {ctz.UtcOffsetWithDst }");
            }

            if (tz.DstRule != null)
            {
                var dst = tz.DstRule;
                TestContext?.WriteLine("Start: {0} - {1} - {2} - {3}", dst.DstStartTime, dst.DstStartMonth, dst.DstStartDateRule, dst.DstAdjust1);
                TestContext?.WriteLine("End: {0} - {1} - {2} - {3}", dst.DstEndTime, dst.DstEndMonth, dst.DstEndDateRule, dst.DstAdjust2);
            }
        }


        private Resource[] GetResourcesFromRequest(BaseRestRequest rest_request)
        {
            var r = ServiceManager.GetResponseAsync(rest_request).GetAwaiter().GetResult();

            if (r?.ResourceSets?.FirstOrDefault().Resources?.Length <= 0)
                throw new Exception("No results found.");

            return r.ResourceSets[0].Resources;
        }

        /// <summary>
        ///  Convert Time Zone Test
        ///  https://msdn.microsoft.com/en-us/library/mt829733.aspx
        ///  
        ///  NOTE: The `ConvertTimeZoneRequest` requires a Datetime and a TimeZone ID
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public void ConvertTimeZoneTest()
        {
            TestContext?.WriteLine("Running Convert TZ Test");
            var dt = DateTimeHelper.GetDateTimeFromUTCString("2018-05-15T13:14:15Z");
            var request = new ConvertTimeZoneRequest(dt, "Cape Verde Standard Time") { BingMapsKey = _ApiKey };
            Resource[] resources = GetResourcesFromRequest(request);
            var tz = (resources[0] as RESTTimeZone);
            PrintTZResource(tz.TimeZone);

        }


        /// <summary>
        /// List Time Zone Test
        /// 
        /// https://msdn.microsoft.com/en-us/library/mt829734.aspx
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public void ListTimeZoneTest()
        {
            TestContext?.WriteLine("Running List TZ Test");
            var list_request = new ListTimeZonesRequest(true)
            {
                BingMapsKey = _ApiKey,
                TimeZoneStandard = "Windows"
            };
            TestContext?.WriteLine(list_request.GetRequestUrl());

            var resources = GetResourcesFromRequest(list_request);
            TestContext?.WriteLine("Printing first three TZ resources:\n");
            for (int i = 0; i < 3; i++)
                PrintTZResource((resources[i] as RESTTimeZone).TimeZone);

            TestContext?.WriteLine("Running Get TZ By ID Test");
            var get_tz_request = new ListTimeZonesRequest(false)
            {
                IncludeDstRules = true,
                BingMapsKey = _ApiKey,
                DestinationTZID = "Cape Verde Standard Time"
            };
            TestContext?.WriteLine(get_tz_request.GetRequestUrl());

            var tz_resources = GetResourcesFromRequest(get_tz_request);
            var tz = (tz_resources[0] as RESTTimeZone);
            PrintTZResource(tz.TimeZone);
        }

        /// <summary>
        /// Find Time Zone test
        /// 
        ///  https://msdn.microsoft.com/en-us/library/mt829732.aspx
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public void FindTimeZoneTest()
        {
            TestContext?.WriteLine("Running Find Time Zone Test: By Query");
            var dt = DateTime.Now;
            var query_tz_request = new FindTimeZoneRequest("Seattle, USA", dt) { BingMapsKey = _ApiKey };
            var query_resources = GetResourcesFromRequest(query_tz_request);
            TestContext?.WriteLine(query_tz_request.GetRequestUrl());

            var r_query = (query_resources[0] as RESTTimeZone);

            if (r_query.TimeZoneAtLocation.Length > 0)
            {
                var qtz = (r_query.TimeZoneAtLocation[0] as TimeZoneAtLocationResource);
                TestContext?.WriteLine($"Place Name: {qtz.PlaceName}");
                PrintTZResource(qtz.TimeZone[0] as TimeZoneResponse);
            }
            else
            {
                TestContext?.WriteLine("No Time Zone Query response.");
            }


            TestContext?.WriteLine("\nRunning Find Time Zone Test: By Point");
            Coordinate cpoint = new Coordinate(47.668915, -122.375789);
            var point_tz_request = new FindTimeZoneRequest(cpoint) { BingMapsKey = _ApiKey, IncludeDstRules = true };

            var point_resources = GetResourcesFromRequest(point_tz_request);
            var r_point = (point_resources[0] as RESTTimeZone);
            var tz = (r_point.TimeZone as TimeZoneResponse);

            TestContext?.WriteLine($"Time Zone: {r_point.TimeZone}");
            PrintTZResource(tz);
        }

        /// <summary>
        ///  Location Recognition Test
        ///  
        /// https://msdn.microsoft.com/en-US/library/mt847173.aspx
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public void LocationRecogTest()
        {
            TestContext?.WriteLine("Running Location Recognition Test");

            Coordinate cpoint = new Coordinate(47.668915, -122.375789);

            TestContext?.WriteLine("coord: {0}", cpoint.ToString());

            var request = new LocationRecogRequest() { BingMapsKey = _ApiKey, CenterPoint = cpoint };

            var resources = GetResourcesFromRequest(request);

            var r = (resources[0] as LocationRecog);

            if (r.AddressOfLocation.Length > 0)
                TestContext?.WriteLine($"Address:\n{r.AddressOfLocation}");

            if (r.BusinessAtLocation != null)
            {
                foreach (LocalBusiness business in r.BusinessAtLocation)
                {
                    TestContext?.WriteLine($"Business:\n{business.BusinessInfo.EntityName}");
                }
            }

            if (r.NaturalPOIAtLocation != null)
            {
                foreach (NaturalPOIAtLocationEntity poi in r.NaturalPOIAtLocation)
                {
                    TestContext?.WriteLine($"POI:\n{poi.EntityName}");
                }
            }
        }

        /// <summary>
        ///  Geocode Test
        ///  
        ///  
        /// </summary>
        [DataTestMethod]
        [DataRow("Seattle")]
        [DataRow("Columbus, Ohio")]
        [DataRow("576 S. Grant Ave, Columbus, Ohio 43206")]
        [DataRow("560 N. Grant Ave, Columbus, Ohio 43206")]
        [TestCategory(TestCategories.DevLocal)]
        public void GeoCodeTest(string query)
        {
            TestContext?.WriteLine("Running Geocode Test");
            var request = new GeocodeRequest()
            {
                BingMapsKey = _ApiKey,
                //Query = "Seattle"
                Query = query
            };

            var resources = GetResourcesFromRequest(request);

            foreach (var resource in resources.OfType<Location>())
            {
                TestContext?.WriteLine($"{nameof(resource.Name)}: {resource.Name}");
                TestContext?.WriteLine($"{nameof(resource.EntityType)}: {resource.EntityType}");
                TestContext?.WriteLine($"{nameof(resource.ConfidenceLevelType)}: {resource.ConfidenceLevelType}");
                //TestContext?.WriteLine($"{nameof(resource.QueryParseValues)}: {string.Join(";", resource.QueryParseValues?.Select(v => (v.Property, v.Value)))}");

                if (resource.Point != null)
                {
                    TestContext?.WriteLine($"{nameof(resource.Point)}: {resource.Point.Coordinates[0]},{resource.Point.Coordinates[1]}");
                }
                if (resource.Address != null)
                {
                    TestContext?.WriteLine($"=== {nameof(resource.Address)} ===");
                    TestContext?.WriteLine($"\t{nameof(resource.Address.AddressLine)}: {resource.Address.AddressLine}");
                    TestContext?.WriteLine($"\t{nameof(resource.Address.AdminDistrict)} (State): {resource.Address.AdminDistrict}");
                    TestContext?.WriteLine($"\t{nameof(resource.Address.AdminDistrict2)} (County): {resource.Address.AdminDistrict2}");
                    TestContext?.WriteLine($"\t{nameof(resource.Address.Locality)} (City): {resource.Address.Locality}");
                    TestContext?.WriteLine($"\t{nameof(resource.Address.PostalCode)} (PostalCode): {resource.Address.PostalCode}");
                }
            }
        }
    }
}