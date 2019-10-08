using System;
using System.Timers;
using System.Globalization;
using System.Windows.Forms;
using System.Threading;
using System.Net;

namespace QCam
{
    public partial class FormMain
    {
		public string generateFileString(string template, string[] dateID, int id)
		{
			// generates ID format
			idFormat = "";
			foreach (char c in template)
				if (c == 'I') idFormat = idFormat + "0";

			// generates Filename from template sans ID
			setAxisID();	//Get latest AxisID
			if (dateID == null)
			{
				dateID = new string[] { "", the_year, the_month, the_day };
			}

			template = template.Replace(idFormat.Replace("0", "I"), id.ToString(idFormat));
			template = template.Replace("NN", cameraName);
			template = template.Replace("YYYY", dateID[1]);
			template = template.Replace("MM", dateID[2]);
			template = template.Replace("DD", dateID[3]);
			template = template.Replace("AA", axisID);

			return template;
		}

		public char generateFileStringDelim(string template)
		{
			var delim = '_';

			var pos = template.IndexOf("YYYY");

			if (pos < 0) return delim;

			if (pos > 0) delim = template[pos -= 1];
			else if (pos + 4 < template.Length) delim = template[pos + 4];		
			return delim;
		}
        public void changeFolder()
        {
            getDayFolder();
        }
        private void getDayFolder()
        {
            //string root = @"X:\";
            string root = Properties.Settings.Default.baseFolder;

            saveFileDialog1.InitialDirectory = root;

            //root = @"C:\Documents and Settings\dareau\Bureau\paris tree root\"; // To remove...

            error_server = false;
            DateTime today = DateTime.Today;
            the_year = today.Year.ToString();
            CultureInfo the_current_culture = CultureInfo.CurrentCulture;
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
            //the_month = String.Format("{0:MMM}", today).ToString();
            //Thread.CurrentThread.CurrentCulture = the_current_culture;
            //the_month = the_month.Substring(0, 1).ToUpper() + the_month.Substring(1);
            the_month = today.Month.ToString();
            the_day = today.Day.ToString();
            if (the_day.Length == 1)
                the_day = "0" + the_day;
            if (the_month.Length == 1)
                the_month = "0" + the_month;

			root = root.Replace("YYYY", the_year);
			root = root.Replace("MM", the_month);
			root = root.Replace("DD", the_day);

			day_folder = root;
            //day_folder = root + @"\" + the_year + "_" + the_month + "_" + the_day;

			//MessageBox.Show("Set to: " + root);

            if (!System.IO.Directory.Exists(day_folder))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(day_folder);
                }
                catch
                {
					error_server = true;
                    MessageBox.Show(day_folder + " could not be created.");
                }
            }
            
        }

        public void Midnight_Timer_Start()
        {
            // Subtract the current time, from midnigh (tomorrow).

            // This will return a value, which will be used to

            // SetTimer the Timer interval

            TimeSpan ts = System.DateTime.Today.AddDays(1).Subtract(DateTime.Now);

            // We only want the Hours, Minuters and Seconds until midnight

            TimeSpan tsMidnight = new TimeSpan(ts.Hours, ts.Minutes, ts.Seconds + 2);

            // Set the Timer

            m_timer = new System.Timers.Timer(tsMidnight.TotalMilliseconds);
            m_timer.SynchronizingObject = this;
            // Set the event handler

            m_timer.Elapsed += new ElapsedEventHandler(Midnight_reached);

            // Start the timer

            m_timer.Start();
        }

        private void Midnight_reached(object sender, ElapsedEventArgs e)
        {
            // now raise a event


            m_timer.Stop();
            getDayFolder();
            this.Text = "Camera program in " + the_day + the_month + the_year + " on " + Dns.GetHostName().ToString(); //+ ":" + port.ToString();
            Midnight_Timer_Start();
        }
    }
}
