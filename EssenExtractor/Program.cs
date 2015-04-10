using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using DDay.iCal;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;

namespace EssenExtractor
{
    class Program
    {
        private static void Main(string[] args)
        {
            List<MenuDay> week = GetWeekMenu(@"http://heutemittag.at/index.php?site=wochenkarte&vars=lokal_id=17",
                "Campina");
            week.AddRange(GetWeekMenu(@"http://heutemittag.at/index.php?site=wochenkarte&vars=lokal_id=64", 
                "LimeGreen"));

            WriteICal(week, @"D:\Dropbox\Public\essen.ics");

        }

        private static List<MenuDay> GetWeekMenu(string url, string name = null)
        {
            List<MenuDay> days = new List<MenuDay>();

            HtmlDocument doc = ParseHtml(url);
            IEnumerable<HtmlNode> dayHeader = doc.DocumentNode.Descendants("th").Where(
                x => (x.Attributes["class"] != null && x.Attributes["class"].Value == "main"));


            foreach (HtmlNode htmlDay in dayHeader)
            {
                MenuDay day = new MenuDay
                {
                    Location = name,
                    MenuEntries = new List<MenuEntry>(),
                    DateTime = DateTime.Parse(htmlDay.InnerText)
                };

                HtmlNode dayContent = htmlDay.ParentNode.NextSibling;

                HtmlNode menus = dayContent.Descendants().FirstOrDefault(x => x.Name == "table");
                IEnumerable<HtmlNode> menuEntries = menus.Descendants("tr");

                bool noMenu = false;
                foreach (HtmlNode menuEntry in menuEntries)
                {
                    if (menuEntry.FirstChild.InnerText == "kein menü eingetragen.")
                    {
                        noMenu = true;
                    }
                    else
                    {
                        MenuEntry entry = new MenuEntry();
                        day.MenuEntries.Add(entry);

                        entry.Title = menuEntry.FirstChild.InnerText;
                        HtmlNode menuContent = menuEntry.ChildNodes[1];

                        IEnumerable<HtmlNode> menuListings = menuContent.Descendants("b");
                        foreach (HtmlNode listing in menuListings)
                        {
                            entry.MenuDesc += listing.InnerText + "\n";
                        }
                    }
                }
                if (!noMenu)
                {
                    days.Add(day);
                }
            }

            return days;
        }

        private static HtmlDocument ParseHtml(string url)
        {
            var parseTask = ParseHtmlAsync(url);
            parseTask.Wait();
            return parseTask.Result;
        }


        static async Task<HtmlDocument> ParseHtmlAsync(string website)
        {
            HttpClient http = new HttpClient();
            var response = await http.GetByteArrayAsync(website);
            String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            HtmlDocument result = new HtmlDocument();
            result.LoadHtml(source);

            return result;
        }

        //static void WriteICal(IEnumerable<MenuDay> days, string path)
        //{
        //    FileStream fs = new FileStream(path, FileMode.Create);
        //    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

        //    sw.WriteLine("BEGIN:VCALENDAR");
        //    sw.WriteLine("VERSION:2.0");
        //    //sw.WriteLine("PRODID:-//Hagenberg//Essensplan//DE");
        //    //sw.WriteLine("METHOD:PUBLISH");
        //    //sw.WriteLine("BEGIN:VTIMEZONE");
        //    //sw.WriteLine("TZID:Europe/Berlin");
        //    //sw.WriteLine("BEGIN:STANDARD");
        //    //sw.WriteLine("TZNAME:CET");
        //    //sw.WriteLine("DTSTART:19960101T020000");
        //    //sw.WriteLine("RRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=10");
        //    //sw.WriteLine("TZOFFSETFROM:+0200");
        //    //sw.WriteLine("TZOFFSETTO:+0100");
        //    //sw.WriteLine("END:STANDARD");
        //    //sw.WriteLine("BEGIN:DAYLIGHT");
        //    //sw.WriteLine("TZNAME:CEST");
        //    //sw.WriteLine("DTSTART:19960101T020000");
        //    //sw.WriteLine("RRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=3");
        //    //sw.WriteLine("TZOFFSETFROM:+0100");
        //    //sw.WriteLine("TZOFFSETTO:+0200");
        //    //sw.WriteLine("END:DAYLIGHT");
        //    //sw.WriteLine("END:VTIMEZONE");

        //    foreach (MenuDay day in days)
        //    {
        //        sw.WriteLine("BEGIN:VEVENT");
        //        //sw.WriteLine("DTSTAMP:{0}", DateTime.Now.ToString("yyyyMMddThhmmssZ")); // 20150302T224407Z
        //        sw.WriteLine("DTSTART;VALUE=DATE:{0}", day.DateTime.ToString("yyyyMMdd")); // 20141002T080000
        //        sw.WriteLine("DTEND;VALUE=DATE:{0}", day.DateTime.AddDays(1).ToString("yyyyMMdd")); // 20141002T102500            20150324
        //        //sw.WriteLine("DESCRIPTION;LANGUAGE=de:{0}", day.CreateDescription());
        //        //sw.WriteLine("LOCATION;LANGUAGE=de:{0}", day.Location);
        //        //sw.WriteLine("CATEGORIES:Essen");
        //        sw.WriteLine("SUMMARY;LANGUAGE=de:{0}", day.Location);
        //        //sw.WriteLine("UID:{0}", day.Location[0] + day.DateTime.ToString("yy-MM-dd")); // 1f1dc7a8-5d96-420b-bc68-960daad399a2
        //        //sw.WriteLine("X-MICROSOFT-CDO-BUSYSTATUS:TENTATIVE");
        //        //sw.WriteLine("PRIORITY:5");
        //        //sw.WriteLine("CLASS:PUBLIC");
        //        //sw.WriteLine("TRANSP:OPAQUE");
        //        //sw.WriteLine("SEQUENCE:0");
        //        sw.WriteLine("END:VEVENT");
        //    }
        //    /*
        //        BEGIN:VCALENDAR
        //        VERSION:2.0
        //        PRODID:-//Hagenberg//Essensplan//DE
        //        METHOD:PUBLISH
        //        BEGIN:VTIMEZONE
        //        TZID:Europe/Berlin
        //        BEGIN:STANDARD
        //        TZNAME:CET
        //        DTSTART:19960101T020000
        //        RRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=10
        //        TZOFFSETFROM:+0200
        //        TZOFFSETTO:+0100
        //        END:STANDARD
        //        BEGIN:DAYLIGHT
        //        TZNAME:CEST
        //        DTSTART:19960101T020000
        //        RRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=3
        //        TZOFFSETFROM:+0100
        //        TZOFFSETTO:+0200
        //        END:DAYLIGHT
        //        END:VTIMEZONE
        //     * 
        //        BEGIN:VEVENT
        //        DTSTAMP:20150302T224407Z
        //        DTSTART;TZID=Europe/Berlin:20141002T080000
        //        DTEND;TZID=Europe/Berlin:20141002T102500
        //        DESCRIPTION;LANGUAGE=de:09_STP1SET1\nTeambuilding und Präsentation\n\nTermintyp: Lehrveranstaltung\nUhrzeit: 08:00 - 10:25\nRaum: bet-at-home HS3 (FH2.025) \nOrt: Hagenberg\nUnterrichtssprache: de\nLektor: Pichler Petra \nStudiengang: SEBakk (0307)\n\n\n--\nStand: 02.03.2015 22:44:07\n
        //        LOCATION;LANGUAGE=de:Hagenberg\, bet-at-home HS3 (FH2.025) 
        //        CATEGORIES:Lehrveranstaltung
        //        SUMMARY;LANGUAGE=de:09_STP1SET1
        //        UID:1f1dc7a8-5d96-420b-bc68-960daad399a2
        //        X-MICROSOFT-CDO-BUSYSTATUS:TENTATIVE
        //        PRIORITY:5
        //        CLASS:PUBLIC
        //        TRANSP:OPAQUE
        //        SEQUENCE:0
        //        END:VEVENT
        //        END:VCALENDAR
             
        //     * 
        //     * 
        //     */
        //    sw.Write("END:VCALENDAR");
        //    sw.Flush();
        //}

        static void WriteICal(IEnumerable<MenuDay> days, string path)
        {
            iCalendar cal = new iCalendar();
            cal.Method = "PUBLISH";
            //cal.Name = "Essen @ Hagenberg";
            cal.ProductID = "ESSEN_AT_HGB";
            //cal.AddLocalTimeZone();

            foreach (MenuDay day in days)
            {
                Event ev = cal.Create<Event>();
                ev.Summary = day.Location;
                ev.UID = day.Location[0] + day.DateTime.ToString("yy-MM-dd");
                ev.Description = day.CreateDescription();

                ev.DTStart = new iCalDateTime(day.DateTime);
                ev.DTEnd = new iCalDateTime(day.DateTime);
                ev.IsAllDay = true;
            }

            SerializeCalendar(cal, path);
        }

        /// <summary>
        /// Saves the calendar to the specified <paramref name="filepath"/>.
        /// </summary>
        static void SaveCalendar(string filepath, IICalendar iCal)
        {
            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, filepath);
        }
        private static void SerializeCalendar(iCalendar cal, string path)
        {
            SaveCalendar(path, cal);
            /*
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            // Create a serialization context and serializer factory.
            // These will be used to build the serializer for our object.
            ISerializationContext ctx = new SerializationContext();
            ISerializerFactory factory = new DDay.iCal.Serialization.iCalendar.SerializerFactory();
            // Get a serializer for our object
            IStringSerializer serializer = factory.Build(cal.GetType(), ctx) as IStringSerializer;

            string output = serializer.SerializeToString(cal);

            File.WriteAllText(path, output, Encoding.UTF8);
             * */
        }
    }
}
