using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.IO;

using CsvHelper;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;

namespace loggenerator
{

    public partial class LogForm : System.Windows.Forms.Form
    {
        public static Dictionary<string,List<IPLocation>> ipranges;
        public static Dictionary<string, Country> countries;
        private static string datafilepath = "data\\dbip-country-2017-04.csv";
        private static string datafilepath2 = "data\\country.csv";

        Criteria cratiria;

        public LogForm()
        {
            InitializeComponent();
            ipranges = new Dictionary<string,List<IPLocation>>();
            countries = new Dictionary<string, Country>();
            cratiria = new Criteria();


            checkDataFiles(ref datafilepath);
            checkDataFiles(ref datafilepath2);

            new Thread(loaddata).Start();

            //loaddata();
        }

        private string checkDataFiles(ref string file)
        {
            if (File.Exists(file) == false)
            {
                MessageBox.Show("data file \"" + file + "\" not found, please select it manually");
                OpenFileDialog op = new OpenFileDialog();
                if (op.ShowDialog() == DialogResult.OK)
                    file = op.FileName;
                else
                {
                    MessageBox.Show("Failed to load some data files, the program will exit");
                    this.Close();
                }
            }

            return file;
        }

        void testingRandomizer()
        {
            long testIndex = 0;
            long testSize = 100_000_000;
            long checkevery = testSize / 1000;

            Criteria x = new Criteria();

            x.AddBrowser(Browsers.Chrome, 1);
            x.AddBrowser(Browsers.FirFox, 10);

            Stopwatch t = new Stopwatch();
            t.Start();
            int chromecount = 0;
            for (testIndex = 0; testIndex < testSize; testIndex++)
            {
                if (x.GeneratBrowser() == Browsers.Chrome)
                    chromecount++;

                if (testIndex % checkevery == 0)
                    notify((testIndex * 100 / testSize) +
                    " %\r\nresult =" + Math.Round(1.0 * testIndex / chromecount, 2) +
                    "%\r\ntime = " + t.ElapsedMilliseconds + " ms");
            }
            t.Stop();

            notify((testIndex * 100 / testSize) +
                    " %\r\nresult =" + Math.Round(1.0 * testIndex / chromecount, 2) +
                    "%\r\ntime = " + t.ElapsedMilliseconds + " ms");
        }
        void testingRandomizerAroundAverage()
        {
            Series series = new Series();
            series.Points.Clear();

            long testIndex = 0;
            long testSize = 1_00;
            long checkevery = testSize / 100;
            int tmp = 0;
            double sum = 0;
            int max=-100, min=10000,big=0,less=0;

            Stopwatch t = new Stopwatch();
            t.Start();

            for (testIndex = 0; testIndex < testSize; testIndex++)
            {
                tmp = Randomizer.RandomAroundAverage(55);
                series.Points.AddY(tmp);

                if (tmp > max) max = tmp;
                if (tmp < min) min = tmp;
                if (tmp > 55) big++;
                if (tmp < 55) less++;

                sum += tmp;

                if (testIndex % checkevery == 0)
                    notify("idx = " + testIndex + "\r\nmax:"+max +" min:" + min + " big:" +big + " les:" + less +"\r\ntmp = " + tmp + "\r\navg = " + (sum / (testIndex + 1)) + "\r\nsum = " + sum + "\r\ntim = " + t.ElapsedMilliseconds + " ms");
            }
            t.Stop();

            notify("idx = " + testIndex + "\r\nmax:" + max + " min:" + min + " big:" + big + " les:" + less + "\r\ntmp = " + tmp + "\r\navg = " + (sum / testSize) + "\r\nsum = " + sum + "\r\ntim = " + t.ElapsedMilliseconds + " ms");

            //chart1.Series.Clear();
            //chart1.Series.Add(series);
        }

        private void loaddata()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            lstBrowsers.Items.AddRange(Enum.GetNames(typeof(Browsers)));
            lstOS.Items.AddRange(Enum.GetNames(typeof(OperationSystems)));

            CsvReader csv2 = new CsvReader(new StreamReader(datafilepath2));
            while (csv2.Read())
                countries.Add(csv2.GetField<string>(0).ToLower(), new Country() { Name = csv2.GetField<string>(0), Code = csv2.GetField<string>(1) });

            lstCountries.Items.AddRange(countries.Select(x => x.Value.Name).ToArray());


            ipranges = new Dictionary<string,List<IPLocation>>();
            CsvReader csv = new CsvReader(new StreamReader(datafilepath));
            string code;
            while (csv.Read())
            {
                code = csv.GetField<string>(2).ToUpper();

                if (ipranges.ContainsKey(code) == false)
                    ipranges.Add(code, new List<IPLocation>());
                ipranges[code].Add(new IPLocation(){ IPStart = csv.GetField<string>(0), IPEnd = csv.GetField<string>(1), Country = code, });
            }

            sw.Stop();
            notify("data has been loaded - " + (sw.ElapsedMilliseconds / 1000) + " sec");
        }

        private void notify(string v)
        {
            lblNotifications.Text = "States:\r\n" + v;
        }
        private void notify(int p)
        {
            lblPrgoressBar.Text = p + "%   " + (stopwatch.ElapsedMilliseconds/1000) + "s\r\n" 
                + Math.Ceiling(((stopwatch.ElapsedMilliseconds / 1000) * 1.0 / Math.Max(p, 1)) * (100 - p)) + "sec left";
            progressBar1.Value = p;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (lstSelectedBrowsers.Items.Count == 0 || lstSelectedOS.Items.Count == 0 || lstSelectedCountries.Items.Count == 0)
            {
                notify("Error - Bad Setting\r\nadd browsers, OS or Country");
                return;
            }

            try
            {
                cratiria
                    .AddTotalVisitorsCount((int)nudVisitorsCount.Value)
                    .AddAverageVisitsCountForPerson((int)nudAverageVisitsPerMonth.Value)
                    .AddRegistedVisitorVsNotRegisted((int)nudRegistedVisitors.Value)
                    
                    .AddMale((int)nudMale.Value)
                    .AddFemale((int)nudFemale.Value)
                    
                    .AddPath(txtPath1.Text, (int)nudPath1.Value)
                    .AddPath(txtPath2.Text, (int)nudPath2.Value)
                    .AddPath(txtPath3.Text, (int)nudPath3.Value)
                    .AddPath(txtPath4.Text, (int)nudPath4.Value)
                    .AddPath(txtPath5.Text, (int)nudPath5.Value)
                    .AddPath(txtPath6.Text, (int)nudPath6.Value)

                    .AddHourOfDay(1, trkH1.Value)
                    .AddHourOfDay(2, trkH2.Value)
                    .AddHourOfDay(3, trkH3.Value)
                    .AddHourOfDay(4, trkH4.Value)
                    .AddHourOfDay(5, trkH5.Value)
                    .AddHourOfDay(6, trkH6.Value)
                    .AddHourOfDay(7, trkH7.Value)
                    .AddHourOfDay(8, trkH8.Value)
                    .AddHourOfDay(9, trkH9.Value)
                    .AddHourOfDay(10, trkH10.Value)
                    .AddHourOfDay(11, trkH11.Value)
                    .AddHourOfDay(12, trkH12.Value)
                    .AddHourOfDay(13, trkH13.Value)
                    .AddHourOfDay(14, trkH14.Value)
                    .AddHourOfDay(15, trkH15.Value)
                    .AddHourOfDay(16, trkH16.Value)
                    .AddHourOfDay(17, trkH17.Value)
                    .AddHourOfDay(18, trkH18.Value)
                    .AddHourOfDay(19, trkH19.Value)
                    .AddHourOfDay(20, trkH20.Value)
                    .AddHourOfDay(21, trkH21.Value)
                    .AddHourOfDay(22, trkH22.Value)
                    .AddHourOfDay(23, trkH23.Value)
                    .AddHourOfDay(24, trkH24.Value)

                    .AddDayOfWeek(1, trkD1.Value)
                    .AddDayOfWeek(2, trkD2.Value)
                    .AddDayOfWeek(3, trkD3.Value)
                    .AddDayOfWeek(4, trkD4.Value)
                    .AddDayOfWeek(5, trkD5.Value)
                    .AddDayOfWeek(6, trkD6.Value)
                    .AddDayOfWeek(7, trkD7.Value)

                    .AddWeekOfMounth(1, trkW1.Value)
                    .AddWeekOfMounth(2, trkW2.Value)
                    .AddWeekOfMounth(3, trkW3.Value)
                    .AddWeekOfMounth(4, trkW4.Value)
                    .AddWeekOfMounth(5, trkW5.Value)

                    .AddMonthOfYear(1, trkM1.Value)
                    .AddMonthOfYear(2, trkM2.Value)
                    .AddMonthOfYear(3, trkM3.Value)
                    .AddMonthOfYear(4, trkM4.Value)
                    .AddMonthOfYear(5, trkM5.Value)
                    .AddMonthOfYear(6, trkM6.Value)
                    .AddMonthOfYear(7, trkM7.Value)
                    .AddMonthOfYear(8, trkM8.Value)
                    .AddMonthOfYear(9, trkM9.Value)
                    .AddMonthOfYear(10, trkM10.Value)
                    .AddMonthOfYear(11, trkM11.Value)
                    .AddMonthOfYear(12, trkM12.Value);
            }
            catch { }

            SaveFileDialog sv = new SaveFileDialog();
            sv.Filter = "text file|*.txt";
            sv.FileName = "log";
            if (sv.ShowDialog() == DialogResult.OK)
            {
                savefilepath = sv.FileName;
                new Thread(generatelog) { IsBackground = true }.Start();
            }
        }

        string savefilepath;
        Stopwatch stopwatch = new Stopwatch();
        void generatelog()
        {
            notify("GENERATING . . . .  \r\nplease wait");
            
            stopwatch.Restart();
            Request.ResetSequence();
            var r = new Randomizer(notify).Generate(cratiria).ConvertToRequests().OrderBy(x => x.Time).Select(x => x.ToString());
            stopwatch.Stop();

            notify(" Writing data into Hard ");
            notify(99);

            File.WriteAllLines(savefilepath, r);

            notify(100);
            notify("GENERATING finished in " + stopwatch.ElapsedMilliseconds / 1000 + " sec");

            MessageBox.Show("file generated at '" + savefilepath +"'" );
        }

        private void btnAddBrowser_Click(object sender, EventArgs e)
        {
            cratiria.AddBrowser((Browsers)lstBrowsers.SelectedIndex, (int)nudBrowsers.Value);
            lstSelectedBrowsers.Items.Add(((Browsers)lstBrowsers.SelectedIndex) + " >> " + nudBrowsers.Value);
        }

        private void btnAddOS_Click(object sender, EventArgs e)
        {
            cratiria.AddOperationSystem((OperationSystems)lstOS.SelectedIndex, (int)nudOS.Value);
            lstSelectedOS.Items.Add(lstOS.SelectedItem + " >> " + nudOS.Value);
        }

        private void btnAddCountry_Click(object sender, EventArgs e)
        {
            cratiria.AddLocation(lstCountries.SelectedItem.ToString(), (int)nudCountries.Value);
            lstSelectedCountries.Items.Add(lstCountries.SelectedItem + " >> " + nudCountries.Value);
        }

        private void btnSaveCriteria_Click(object sender, EventArgs e)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n\t<Settings>"
                           + "\r\n\t\t<nudVisitorsCount>" + nudVisitorsCount.Value + "</nudVisitorsCount>"
                           + "\r\n\t\t<nudAverageVisitsPerMonth>" + nudAverageVisitsPerMonth.Value + "</nudAverageVisitsPerMonth>"
                           + "\r\n\t\t<nudRegistedVisitors>" + nudRegistedVisitors.Value + "</nudRegistedVisitors>"
                           + "\r\n\t\t<nudMale>" + nudMale.Value + "</nudMale>"
                           + "\r\n\t\t<nudFemale>" + nudFemale.Value + "</nudFemale>"

                           + "\r\n\t\t<nudPath1>" + nudPath1.Value + "</nudPath1>"
                           + "\r\n\t\t<nudPath2>" + nudPath2.Value + "</nudPath2>"
                           + "\r\n\t\t<nudPath3>" + nudPath3.Value + "</nudPath3>"
                           + "\r\n\t\t<nudPath4>" + nudPath4.Value + "</nudPath4>"
                           + "\r\n\t\t<nudPath5>" + nudPath5.Value + "</nudPath5>"
                           + "\r\n\t\t<nudPath6>" + nudPath6.Value + "</nudPath6>"

                           + "\r\n\t\t<txtPath1>" + txtPath1.Text + "</txtPath1>"
                           + "\r\n\t\t<txtPath2>" + txtPath2.Text + "</txtPath2>"
                           + "\r\n\t\t<txtPath3>" + txtPath3.Text + "</txtPath3>"
                           + "\r\n\t\t<txtPath4>" + txtPath4.Text + "</txtPath4>"
                           + "\r\n\t\t<txtPath5>" + txtPath5.Text + "</txtPath5>"
                           + "\r\n\t\t<txtPath6>" + txtPath6.Text + "</txtPath6>"

                           + "\r\n\t\t<trkH1>" + trkH1.Value + "</trkH1>"
                           + "\r\n\t\t<trkH2>" + trkH2.Value + "</trkH2>"
                           + "\r\n\t\t<trkH3>" + trkH3.Value + "</trkH3>"
                           + "\r\n\t\t<trkH4>" + trkH4.Value + "</trkH4>"
                           + "\r\n\t\t<trkH5>" + trkH5.Value + "</trkH5>"
                           + "\r\n\t\t<trkH6>" + trkH6.Value + "</trkH6>"
                           + "\r\n\t\t<trkH7>" + trkH7.Value + "</trkH7>"
                           + "\r\n\t\t<trkH8>" + trkH8.Value + "</trkH8>"
                           + "\r\n\t\t<trkH9>" + trkH9.Value + "</trkH9>"
                           + "\r\n\t\t<trkH10>" + trkH10.Value + "</trkH10>"
                           + "\r\n\t\t<trkH11>" + trkH11.Value + "</trkH11>"
                           + "\r\n\t\t<trkH12>" + trkH12.Value + "</trkH12>"
                           + "\r\n\t\t<trkH13>" + trkH13.Value + "</trkH13>"
                           + "\r\n\t\t<trkH14>" + trkH14.Value + "</trkH14>"
                           + "\r\n\t\t<trkH15>" + trkH15.Value + "</trkH15>"
                           + "\r\n\t\t<trkH16>" + trkH16.Value + "</trkH16>"
                           + "\r\n\t\t<trkH17>" + trkH17.Value + "</trkH17>"
                           + "\r\n\t\t<trkH18>" + trkH18.Value + "</trkH18>"
                           + "\r\n\t\t<trkH19>" + trkH19.Value + "</trkH19>"
                           + "\r\n\t\t<trkH20>" + trkH20.Value + "</trkH20>"
                           + "\r\n\t\t<trkH21>" + trkH21.Value + "</trkH21>"
                           + "\r\n\t\t<trkH22>" + trkH22.Value + "</trkH22>"
                           + "\r\n\t\t<trkH23>" + trkH23.Value + "</trkH23>"
                           + "\r\n\t\t<trkH24>" + trkH24.Value + "</trkH24>"

                           + "\r\n\t\t<trkD1>" + trkD1.Value + "</trkD1>"
                           + "\r\n\t\t<trkD2>" + trkD2.Value + "</trkD2>"
                           + "\r\n\t\t<trkD3>" + trkD3.Value + "</trkD3>"
                           + "\r\n\t\t<trkD4>" + trkD4.Value + "</trkD4>"
                           + "\r\n\t\t<trkD5>" + trkD5.Value + "</trkD5>"
                           + "\r\n\t\t<trkD6>" + trkD6.Value + "</trkD6>"
                           + "\r\n\t\t<trkD7>" + trkD7.Value + "</trkD7>"
                           + "\r\n\t\t<trkW1>" + trkW1.Value + "</trkW1>"
                           + "\r\n\t\t<trkW2>" + trkW2.Value + "</trkW2>"
                           + "\r\n\t\t<trkW3>" + trkW3.Value + "</trkW3>"
                           + "\r\n\t\t<trkW4>" + trkW4.Value + "</trkW4>"
                           + "\r\n\t\t<trkW5>" + trkW5.Value + "</trkW5>"
                           + "\r\n\t\t<trkM1>" + trkM1.Value + "</trkM1>"
                           + "\r\n\t\t<trkM2>" + trkM2.Value + "</trkM2>"
                           + "\r\n\t\t<trkM3>" + trkM3.Value + "</trkM3>"
                           + "\r\n\t\t<trkM4>" + trkM4.Value + "</trkM4>"
                           + "\r\n\t\t<trkM5>" + trkM5.Value + "</trkM5>"
                           + "\r\n\t\t<trkM6>" + trkM6.Value + "</trkM6>"
                           + "\r\n\t\t<trkM7>" + trkM7.Value + "</trkM7>"
                           + "\r\n\t\t<trkM8>" + trkM8.Value + "</trkM8>"
                           + "\r\n\t\t<trkM9>" + trkM9.Value + "</trkM9>"
                           + "\r\n\t\t<trkM10>" + trkM10.Value + "</trkM10>"
                           + "\r\n\t\t<trkM11>" + trkM11.Value + "</trkM11>"
                           + "\r\n\t\t<trkM12>" + trkM12.Value + "</trkM12>"
                           
                           + convertToXML(lstSelectedBrowsers.Items, "lstSelectedBrowsers") 
                           + convertToXML(lstSelectedOS.Items, "lstSelectedOS")
                           + convertToXML(lstSelectedCountries.Items, "lstSelectedCountries")

                           + "\r\n\t</Settings>";

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "XML file|*.xml|Any file|*.*";
            if (save.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(save.FileName, xml);
                notify("saved at: \r\n" + save.FileName);
            }
        }

        private string convertToXML(ListBox.ObjectCollection list,string nodename)
        {
            string xml = "\r\n\t\t<"+ nodename + ">";
            for (int i = 0; i < list.Count; i++)
                xml += "\r\n\t\t\t<item value=\"" + parseValue(list[i].ToString()) + "\">" + parseName(list[i].ToString()) + "</item>";
            return xml + "\r\n\t\t</" + nodename + ">";
        }

        private string parseName(string v)
        {
            return v.Substring(0,v.IndexOf(" >> "));
        }

        private string parseValue(string v)
        {
            return v.Substring(v.IndexOf(" >> ") + 4);
        }

        private void btnLoadCriteria_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "XML file|*.xml|Any file|*.*";
            if (open.ShowDialog() == DialogResult.OK)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(open.FileName);

                nudVisitorsCount.Value = int.Parse(doc.SelectSingleNode("Settings/nudVisitorsCount").InnerText);
                nudAverageVisitsPerMonth.Value = int.Parse(doc.SelectSingleNode("Settings/nudAverageVisitsPerMonth").InnerText);
                nudRegistedVisitors.Value = int.Parse(doc.SelectSingleNode("Settings/nudRegistedVisitors").InnerText);
                nudMale.Value = int.Parse(doc.SelectSingleNode("Settings/nudMale").InnerText);
                nudFemale.Value = int.Parse(doc.SelectSingleNode("Settings/nudFemale").InnerText);
                nudPath1.Value = int.Parse(doc.SelectSingleNode("Settings/nudPath1").InnerText);
                nudPath2.Value = int.Parse(doc.SelectSingleNode("Settings/nudPath2").InnerText);
                nudPath3.Value = int.Parse(doc.SelectSingleNode("Settings/nudPath3").InnerText);
                nudPath4.Value = int.Parse(doc.SelectSingleNode("Settings/nudPath4").InnerText);
                nudPath5.Value = int.Parse(doc.SelectSingleNode("Settings/nudPath5").InnerText);
                nudPath6.Value = int.Parse(doc.SelectSingleNode("Settings/nudPath6").InnerText);

                txtPath1.Text = doc.SelectSingleNode("Settings/txtPath1").InnerText;
                txtPath2.Text = doc.SelectSingleNode("Settings/txtPath2").InnerText;
                txtPath3.Text = doc.SelectSingleNode("Settings/txtPath3").InnerText;
                txtPath4.Text = doc.SelectSingleNode("Settings/txtPath4").InnerText;
                txtPath5.Text = doc.SelectSingleNode("Settings/txtPath5").InnerText;
                txtPath6.Text = doc.SelectSingleNode("Settings/txtPath6").InnerText;

                trkH1.Value = int.Parse(doc.SelectSingleNode("Settings/trkH1").InnerText);
                trkH2.Value = int.Parse(doc.SelectSingleNode("Settings/trkH2").InnerText);
                trkH3.Value = int.Parse(doc.SelectSingleNode("Settings/trkH3").InnerText);
                trkH4.Value = int.Parse(doc.SelectSingleNode("Settings/trkH4").InnerText);
                trkH5.Value = int.Parse(doc.SelectSingleNode("Settings/trkH5").InnerText);
                trkH6.Value = int.Parse(doc.SelectSingleNode("Settings/trkH6").InnerText);
                trkH7.Value = int.Parse(doc.SelectSingleNode("Settings/trkH7").InnerText);
                trkH8.Value = int.Parse(doc.SelectSingleNode("Settings/trkH8").InnerText);
                trkH9.Value = int.Parse(doc.SelectSingleNode("Settings/trkH9").InnerText);
                trkH10.Value = int.Parse(doc.SelectSingleNode("Settings/trkH10").InnerText);
                trkH11.Value = int.Parse(doc.SelectSingleNode("Settings/trkH11").InnerText);
                trkH12.Value = int.Parse(doc.SelectSingleNode("Settings/trkH12").InnerText);
                trkH13.Value = int.Parse(doc.SelectSingleNode("Settings/trkH13").InnerText);
                trkH14.Value = int.Parse(doc.SelectSingleNode("Settings/trkH14").InnerText);
                trkH15.Value = int.Parse(doc.SelectSingleNode("Settings/trkH15").InnerText);
                trkH16.Value = int.Parse(doc.SelectSingleNode("Settings/trkH16").InnerText);
                trkH17.Value = int.Parse(doc.SelectSingleNode("Settings/trkH17").InnerText);
                trkH18.Value = int.Parse(doc.SelectSingleNode("Settings/trkH18").InnerText);
                trkH19.Value = int.Parse(doc.SelectSingleNode("Settings/trkH19").InnerText);
                trkH20.Value = int.Parse(doc.SelectSingleNode("Settings/trkH20").InnerText);
                trkH21.Value = int.Parse(doc.SelectSingleNode("Settings/trkH21").InnerText);
                trkH22.Value = int.Parse(doc.SelectSingleNode("Settings/trkH22").InnerText);
                trkH23.Value = int.Parse(doc.SelectSingleNode("Settings/trkH23").InnerText);
                trkH24.Value = int.Parse(doc.SelectSingleNode("Settings/trkH24").InnerText);

                trkD1.Value = int.Parse(doc.SelectSingleNode("Settings/trkD1").InnerText);
                trkD2.Value = int.Parse(doc.SelectSingleNode("Settings/trkD2").InnerText);
                trkD3.Value = int.Parse(doc.SelectSingleNode("Settings/trkD3").InnerText);
                trkD4.Value = int.Parse(doc.SelectSingleNode("Settings/trkD4").InnerText);
                trkD5.Value = int.Parse(doc.SelectSingleNode("Settings/trkD5").InnerText);
                trkD6.Value = int.Parse(doc.SelectSingleNode("Settings/trkD6").InnerText);
                trkD7.Value = int.Parse(doc.SelectSingleNode("Settings/trkD7").InnerText);
                trkW1.Value = int.Parse(doc.SelectSingleNode("Settings/trkW1").InnerText);
                trkW2.Value = int.Parse(doc.SelectSingleNode("Settings/trkW2").InnerText);
                trkW3.Value = int.Parse(doc.SelectSingleNode("Settings/trkW3").InnerText);
                trkW4.Value = int.Parse(doc.SelectSingleNode("Settings/trkW4").InnerText);
                trkW5.Value = int.Parse(doc.SelectSingleNode("Settings/trkW5").InnerText);
                trkM1.Value = int.Parse(doc.SelectSingleNode("Settings/trkM1").InnerText);
                trkM2.Value = int.Parse(doc.SelectSingleNode("Settings/trkM2").InnerText);
                trkM3.Value = int.Parse(doc.SelectSingleNode("Settings/trkM3").InnerText);
                trkM4.Value = int.Parse(doc.SelectSingleNode("Settings/trkM4").InnerText);
                trkM5.Value = int.Parse(doc.SelectSingleNode("Settings/trkM5").InnerText);
                trkM6.Value = int.Parse(doc.SelectSingleNode("Settings/trkM6").InnerText);
                trkM7.Value = int.Parse(doc.SelectSingleNode("Settings/trkM7").InnerText);
                trkM8.Value = int.Parse(doc.SelectSingleNode("Settings/trkM8").InnerText);
                trkM9.Value = int.Parse(doc.SelectSingleNode("Settings/trkM9").InnerText);
                trkM10.Value = int.Parse(doc.SelectSingleNode("Settings/trkM10").InnerText);
                trkM11.Value = int.Parse(doc.SelectSingleNode("Settings/trkM11").InnerText);
                trkM12.Value = int.Parse(doc.SelectSingleNode("Settings/trkM12").InnerText);

                resetCriteria();

                parseList(doc.SelectNodes("Settings/lstSelectedBrowsers/item"), lstSelectedBrowsers,
                    (x, y) => cratiria.AddBrowser((Browsers)Enum.Parse(typeof(Browsers), x), y));
                parseList(doc.SelectNodes("Settings/lstSelectedOS/item"), lstSelectedOS,
                    (x, y) => cratiria.AddOperationSystem((OperationSystems)Enum.Parse(typeof(OperationSystems), x), y));
                parseList(doc.SelectNodes("Settings/lstSelectedCountries/item"), lstSelectedCountries, (x, y) => cratiria.AddLocation(x, y));

                notify("loaded");
            }
        }

        private void resetCriteria()
        {
            cratiria = new Criteria();
            lstSelectedBrowsers.Items.Clear();
            lstSelectedOS.Items.Clear();
            lstSelectedCountries.Items.Clear();
        }

        private void parseList(XmlNodeList list,ListBox box,Action<string,int> action)
        {
            int value;
            string name;
            foreach (XmlNode n in list)
            {
                name = n.InnerText;
                value = int.Parse(n.Attributes["value"].Value);

                action(name, value);
                box.Items.Add(name + " >> " + value);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            resetCriteria();
        }
    }
}
