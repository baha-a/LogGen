using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Weighted_Randomizer;

namespace loggenerator
{
    public class Criteria
    {
        IWeightedRandomizer<int> Browser { get; set; } 
        IWeightedRandomizer<int> OperationSystem { get; set; }

        IWeightedRandomizer<string> IPAdressLocation { get; set; }

        IWeightedRandomizer<int> HourOfDay { get; set; }
        IWeightedRandomizer<int> DayOfWeek { get; set; }
        IWeightedRandomizer<int> WeekOfMonth  { get; set; }
        IWeightedRandomizer<int> MonthOfYear { get; set; }

        IWeightedRandomizer<bool> RegistedVisitorVsNotRegisted { get; set; }
        IWeightedRandomizer<bool> MaleVsFemale { get; set; }

        IWeightedRandomizer<int> NavigationPaths { get; set; }
        List<Request[]> paths;

        public int TotalVisitorsCount { get; private set; }
        int averageVisitsCount;



        public Criteria()
        {
            Browser          = new DynamicWeightedRandomizer<int>();
            OperationSystem  = new DynamicWeightedRandomizer<int>();
            IPAdressLocation = new DynamicWeightedRandomizer<string>();

            HourOfDay        = new DynamicWeightedRandomizer<int>();
            DayOfWeek        = new DynamicWeightedRandomizer<int>();
            WeekOfMonth      = new DynamicWeightedRandomizer<int>();
            MonthOfYear      = new DynamicWeightedRandomizer<int>();

            NavigationPaths  = new DynamicWeightedRandomizer<int>();
            paths = new List<Request[]>();


            MaleVsFemale = new DynamicWeightedRandomizer<bool>();
        }
        public Criteria AddTotalVisitorsCount(int count)
        {
            TotalVisitorsCount = count;
            return this;
        }
        public Criteria AddAverageVisitsCountForPerson(int averagecount)
        {
            averageVisitsCount = averagecount;
            return this;
        }
        public Criteria AddRegistedVisitorVsNotRegisted(int registed)
        {
            if (TotalVisitorsCount <= 0)
                throw new Exception("TotalVisitorsCount must be bigger than zero");

            RegistedVisitorVsNotRegisted = new DynamicWeightedRandomizer<bool>();
            RegistedVisitorVsNotRegisted.Add(true, registed);
            RegistedVisitorVsNotRegisted.Add(false, TotalVisitorsCount - registed);
            return this;
        }


        public Criteria AddMale(int m)
        {
            MaleVsFemale.Add(true, m);
            return this;
        }

        public Criteria AddFemale(int f)
        {
            MaleVsFemale.Add(false, f);
            return this;
        }

        public Criteria AddBrowser(Browsers b, int weight)
        {
            Browser.Add((int)b, weight);
            return this;
        }
        public Criteria AddOperationSystem(OperationSystems os, int weight)
        {
            OperationSystem.Add((int)os, weight);
            return this;
        }

        public Criteria AddLocation(string ip, int weight)
        {
            IPAdressLocation.Add(ip, weight);
            return this;
        }

        public Criteria AddPath(string p, int weight)
        {
            if (string.IsNullOrEmpty(p))
                return this;

            paths.Add(DecodePaths(p));
            NavigationPaths.Add(paths.Count - 1, weight);
            return this;
        }


        public Criteria AddHourOfDay(int h, int weight)
        {
            HourOfDay.Add(h, weight);
            return this;
        }
        public Criteria AddDayOfWeek(int h, int weight)
        {
            DayOfWeek.Add(h, weight);
            return this;
        }
        public Criteria AddWeekOfMounth(int h, int weight)
        {
            WeekOfMonth.Add(h, weight);
            return this;
        }
        public Criteria AddMonthOfYear(int h, int weight)
        {
            MonthOfYear.Add(h, weight);
            return this;
        }



        public Browsers GeneratBrowser()
        {
            return (Browsers)Browser.NextWithReplacement();
        }
        public OperationSystems GeneratOS()
        {
            return (OperationSystems)OperationSystem.NextWithReplacement();
        }

        public string GeneratIPAddressLocation()
        {
            return IPAdressLocation.NextWithReplacement();
        }

        public string GeneratIPAddress()
        {
            return IPLocation.Generate(IPAdressLocation.NextWithReplacement());
        }

        public Request[] GeneratPath()
        {
            var a = paths[NavigationPaths.NextWithReplacement()];
            Request[] b = new Request[a.Length];
            for (int i = 0; i < a.Length; i++)
                b[i] = new Request(a[i]);
            return b;
        }

        public int GeneratHour()
        {
            return HourOfDay.NextWithReplacement();
        }
        public int GeneratDay()
        {
            return DayOfWeek.NextWithReplacement() * WeekOfMonth.NextWithReplacement();
        }
        public int GeneratMonth()
        {
            return MonthOfYear.NextWithReplacement();
        }
        public int GeneratYear()
        {
            return 2017;
        }

        public bool GenerateRegistedVisitorOrNotRegsited()
        {
            return RegistedVisitorVsNotRegisted.NextWithReplacement();
        }

        public int GenerateVisitsCount()
        {
            return Randomizer.RandomAroundAverage(averageVisitsCount);
        }

        public bool? GenerateMaleOrFemale()
        {
            try { return MaleVsFemale.NextWithReplacement(); }
            catch { }
            return null;
        }

        public static Request[] DecodePaths(string str)
        {
            str = str.Trim().Replace(" ","").ToUpper();

            string name = "", nameOld = "", numb = "";
            int leng = 0;
            List<Request> rq = new List<Request>();

            while (str.Length > 0)
            {
                name = str.Substring(0, str.IndexOf('('));
                numb = str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1);
                leng = (numb.Length > 0) ? int.Parse(numb) : 0;

                if (string.IsNullOrEmpty(nameOld) == false)
                    rq.Add(new Request() { SourceUrl = nameOld, RequestedUrl = name, WatchingTime = leng });

                nameOld = name;

                if (str.IndexOf(">") < 0)
                    break;
                str = str.Substring(str.IndexOf(">") + 1);
            }

            return rq.ToArray();
        }
    }

    public class Randomizer
    {
        Action<int> notify;

        public Randomizer(Action<int> notifycallpack = null)
        {
            notify = notifycallpack;
        }

        private Group group { get; set; }

        public Randomizer Generate(Criteria criteria)
        {
            group = new Group();
            Person temp;
            int session;
            for (int i = 0; i < criteria.TotalVisitorsCount; i++)
            {
                temp = new Person()
                {
                    ID = i,
                    CookiesID = RandomString(),
                    Registed = criteria.GenerateRegistedVisitorOrNotRegsited(),
                    IP = criteria.GeneratIPAddress(),
                    Browsers = criteria.GeneratBrowser(),
                    OperationSystem = criteria.GeneratOS(),
                    MaleOrFemaleIfRegisted = criteria.GenerateMaleOrFemale(),
                };

                session = criteria.GenerateVisitsCount();
                Request[] path;
                for (int j = 0; j < session; j++)
                {
                    path = criteria.GeneratPath();
                    path[0].Time = GenerateDatetime(criteria);
                    temp.AddRequest(path[0]);
                    for (int q = 1; q < path.Length; q++)
                    {
                        path[q].Time = path[q - 1].Time.AddSeconds( RandomAroundAverage( path[q-1].WatchingTime ));
                        temp.AddRequest(path[q]);
                    }
                }

                group.People.Add(temp);

                if (notify != null)
                    notify((int)Math.Round(i * 97.0 / criteria.TotalVisitorsCount));
            }

            return this;
        }

        public List<Request> ConvertToRequests()
        {
            List<Request> Requests = new List<Request>();
            foreach (var p in group.People)
                Requests.AddRange(p.GetRequests());

            if (notify != null)
                notify(98);
            return Requests;
        }

        private static DateTime GenerateDatetime(Criteria criteria)
        {
            bool b = false;
            DateTime t;
            do
            {
                t = BuildDate(
                    criteria.GeneratHour() - 1,
                    RandomMinute(),
                    RandomMinute(),
                    criteria.GeneratDay(),
                    criteria.GeneratMonth(),
                    criteria.GeneratYear(),
                    ref b);
            } while (b == false);
            return t;
        }

        static string[] formats = { "H:m:s d-M-yyyy" };
        public static DateTime BuildDate(int h, int m, int s, int d, int M, int y,ref bool res)
        {
            try
            {
                res = true;
                return new DateTime(y, M, d, h, m, s);
            }
            catch { }
            res = false;
            return DateTime.Now;

            //DateTime t;

            //res = DateTime.TryParseExact(
            //    h + ":" + m + ":" + s + " " + d + "-" + M + "-" + y,
            //    formats,
            //    CultureInfo.InvariantCulture,
            //    DateTimeStyles.None,
            //    out t);

            //return t;
        }


        static Random r = new Random();
        public static int RandomAroundAverage(double  average)
        {
            if (average == 1)
                return 1;

            double scale = average * 2;
            return (int)(r.NextDouble() * scale - (scale / 2) + average) + 1;
        }
        public static string RandomString(int Size = 16)
        {
            string input = "0123456789abcdefghijklmnopqrstuvwxyz";
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Size; i++)
                builder.Append(input[r.Next(0, input.Length)]);
            return builder.ToString();
        }

        public static int RandomMinute()
        {
            return r.Next(0,60);
        }
    }

}
