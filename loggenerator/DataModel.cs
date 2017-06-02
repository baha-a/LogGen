using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace loggenerator
{
    public class Group
    {
        public string Name { get; set; }
        public List<Person> People { get; set; }

        public Group()
        {
            People = new List<Person>();
        }
    }

    public class Person
    {
        public int ID { get; set; }

        public string CookiesID { get; set; }
        public string Profile { get; set; }
        public bool Registed { get; set; }

        public bool? MaleOrFemaleIfRegisted { get; set; }


        public string IP { get; set; }
        public Browsers Browsers { get; set; }
        public OperationSystems OperationSystem { get; set; }
        private List<Request> Requstes { get; set; }
        public Person()
        {
            Requstes = new List<Request>();
        }

        public Request[] GetRequests()
        {
            return Requstes.ToArray();
        }
        public Person AddRequest(Request r)
        {
            r.Father = this;
            Requstes.Add(r);
            return this;
        }
    }

    public class Request
    {
        public Request() { }
        public Request(Request r)
        {
            WatchingTime = r.WatchingTime;
            RequestedUrl = r.RequestedUrl;
            SourceUrl = r.SourceUrl;
            ResponseCode = r.ResponseCode;
            ResponseSize = r.ResponseSize;
        }

        public Person Father{ get; set; }


        public DateTime Time { get; set; }

        public int WatchingTime { get; set; }

        public string RequestedUrl { get; set; }
        public string SourceUrl { get; set; }

        public int ResponseCode { get; set; }
        public int ResponseSize { get; set; }

        static int sequence = 0;
        public static void ResetSequence() { sequence = 0; }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str
                        .Append((++sequence).ToString("D" + 10)).Append(" ")
                        .Append(Father.CookiesID).Append(" ")
                        .Append(Father.Registed ? (Father.MaleOrFemaleIfRegisted == true ? "MALE" : "FMLE") : "NONE").Append(" ")
                        .Append(Father.IP).Append(" '")
                        .Append(Father.Browsers.ToString()).Append("' '")
                        .Append(Father.OperationSystem.ToString()).Append("' ")
                        .Append(Time.ToString("HH:mm:ss dd-MM-yyyy")).Append(" '")
                        .Append(RequestedUrl).Append("' '")
                        .Append(SourceUrl).Append("'")
                        //.Append(ResponseCode).Append(" ")
                        //.Append(ResponseSize)
                        ;
            return str.ToString();
        }
    }


    public class IPLocation
    {
        public int Id { get; set; }
        public string IPStart { get; set; }
        public string IPEnd { get; set; }

        public string Country { get; set; }



        public override string ToString()
        {
            return Id + " - " + IPStart + " - " + IPEnd + " - " + Country;
        }



        public static string Generate(string location)
        {
            string code = "SY";
            if (LogForm.countries.ContainsKey(location.ToLower()))
                code = LogForm.countries[location.ToLower()].Code.ToUpper();

            List<IPLocation> ips = LogForm.ipranges[code];
            IPLocation ip = ips[random.Next(0, ips.Count)];


            uint v = StringIpToUInt(ip.IPStart);
            uint u = StringIpToUInt(ip.IPEnd);
            return GetIP(v, u, random.Next(0, GetIPRangeLength(v,u)));
        }

        private static uint StringIpToUInt(string ip)
        {
            return ipToUint(IPAddress.Parse(ip).GetAddressBytes());
        }

        #region hidden part
        public static int GetIPRangeLength(uint sIP, uint eIP)
        {
            return (int)(eIP - sIP);
        }

        public static string GetIP(uint sIP, uint eIP, int offest)
        {
            return new IPAddress(reverseBytesArray(sIP +(uint)offest)).ToString();
        }

        private static Random random = new Random();
        private static uint reverseBytesArray(uint ip)
        {
            byte[] bytes = BitConverter.GetBytes(ip);
            bytes = bytes.Reverse().ToArray();
            return (uint)BitConverter.ToInt32(bytes, 0);
        }
        private static uint ipToUint(byte[] ipBytes)
        {
            ByteConverter bConvert = new ByteConverter();
            uint ipUint = 0;

            int shift = 24; // indicates number of bits left for shifting
            foreach (byte b in ipBytes)
            {
                if (ipUint == 0)
                {
                    ipUint = (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                    shift -= 8;
                    continue;
                }

                if (shift >= 8)
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                else
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint));

                shift -= 8;
            }

            return ipUint;
        }
        #endregion
    }
    public class Country
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name + " - " + Code;
        }
    }

    public enum Browsers
    {
        Chrome,
        FirFox,
        Opera,
        Other
    }
    public enum OperationSystems
    {
        Windows,
        Linux,
        Mac,
        Other
    }
}