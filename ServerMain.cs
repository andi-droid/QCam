using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices; //for catching accessviolations in try catch



namespace QCam
{

    public partial class FormMain
    {
		private bool file_error_shown = false;
		private bool server_error_shown = false;

        private int getLastID(string dayFolder)
        {
            var directory = new DirectoryInfo(dayFolder);
            var id = 0;
			var i = 0;

			//Read in template from settings
			string template = filenameFormat;
			//Create delimitor
			char delim = generateFileStringDelim(template);

            if (directory == null || !directory.Exists)
                return id;

			//This is actually terrible coding ...
			generateFileString(template, null, 0);	//need to call generateFileString to set idFormat
			string[] ftemplate = template.Split(delim);
			var pos = Array.IndexOf(ftemplate, idFormat.Replace("0", "I"));

			FileInfo[] files = directory.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            foreach (FileInfo file in files)
            {
				string[] fname = System.IO.Path.GetFileNameWithoutExtension(file.Name).Split(delim);
				if (fname.Length > 1 && pos > -1 && int.TryParse(fname[pos], out i)) // @todo: Need to change this to be more general! -- done. NR
				{
					if (i > id) //YYYY_MM_DD_AXIS_ID_[no]atoms.tif
						id = i;
				}
				else
				{
					if (!file_error_shown) (new Thread(() => { MessageBox.Show("Image folder possibly incorrect or corrupt. Check Settings/path and files to avoid data loss.", DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information); file_error_shown = false; })).Start();
					file_error_shown = true;
				}
            }
            return id;	
        }

        private void displayAni(string kind, ref long timeout)
        {
            if (kind == "pending")
            {
                if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 5000)
                {
                    Console.Write("\rNetwork pending.....");
                    timeout = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + 2500;
                }
                else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 4000)
                    Console.Write("\rNetwork pending.... ");
                else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 3000)
                    Console.Write("\rNetwork pending...  ");
                else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 2000)
                    Console.Write("\rNetwork pending..   ");
                else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 1000)
                    Console.Write("\rNetwork pending.    ");
                else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 0)
                    Console.Write("\rNetwork pending     ");
            }
            else if (kind == "idling")
            {
                //
            }
        }

		[HandleProcessCorruptedStateExceptions]
        private void serverEntryPoint()
        {
			try
			{
				server();
			}
			catch (Exception e)
			{
				Console.Write("Fatal Server error: " + e.Message + "\nTrying restart ... ");
				System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "server.txt", e.ToString());
				if (!server_error_shown) (new Thread(() => { MessageBox.Show("Server shutdown!\n\nFatal Server error: " + e.Message + "\nAttempting automatic restart.", DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information); server_error_shown = false; })).Start();
				server_error_shown = true;
			}
        }

		private void server()
		{
			Console.Write("Done.\n");
			//Thread resources

			int lastID = 0;
			int inc = 1;

			string shotNameBuffer = "";
			string currentFileBaseString = "";
			string currentProtoBaseString = "";

			IPAddress[] localIPList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			IPAddress lclhst = null;
			foreach (IPAddress ip in localIPList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					lclhst = ip;
					break;
				}
			}



			TcpListener myListener;
			Socket socketForServer = null;
			int port = Properties.Settings.Default.pcPort;
			if (port == 0)
			{
				MessageBox.Show("Invalid port for listening.");
				return;
			}
			bool newInstructionsReceived = false;

			//Global variable setting

			pictureBeingTaken = false;
			pictureHasBeenTaken = false;
			globalImgCounter = getLastID(day_folder);



			#region Server Start
			myListener = new TcpListener(lclhst, port);
			myListener.Start();

			long timeout = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			while (!abortServer && !myListener.Pending() && !formClosed)
			{
				if (initIsDone) displayAni("pending", ref timeout);

				Thread.Sleep(1);
			}


			if (!formClosed && !abortServer)
			{
				socketForServer = myListener.AcceptSocket();
				socketForServer.SendTimeout = 100;
				Console.WriteLine("TCP Connection found.\n");
			}
			#endregion
			

			try
			{
			//server main loop: runs while the program is open
				#region Server Main Loop
				timeout = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
				while (!formClosed && !abortServer)
				{
					Thread.Sleep(10);
					byte[] bytes = new byte[0];
					string conf = "";
					byte[] bconf;
					int i = 0;

					//Check if connection is still open
					#region checkConnection
					try
					{
						byte[] tmp = new byte[1];

						if (!pictureBeingTaken && !pictureHasBeenTaken)
						{
							if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 5000)
							{
								Console.Write("\rNetwork idling.....");
								socketForServer.Send(tmp, 1, 0);
								timeout = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + 2500;
							}
							else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 4000)
								Console.Write("\rNetwork idling.... ");
							else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 3000)
								Console.Write("\rNetwork idling...  ");
							else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 2000)
								Console.Write("\rNetwork idling..   ");
							else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 1000)
								Console.Write("\rNetwork idling.    ");
							else if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 0)
								Console.Write("\rNetwork idling     ");
						}
					}
					catch (SocketException e)
					{
						// 10035 == WSAEWOULDBLOCK 
						if (e.NativeErrorCode.Equals(10035))
							Console.WriteLine("Still Connected, but the Send would block");
						else
						{
							Console.WriteLine("Network error: error code {0}!", e.NativeErrorCode);
							Console.WriteLine("Looping for incoming connection, pending ...");
							timeout = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
							while (!abortServer && !myListener.Pending() && !formClosed)
							{
								displayAni("pending", ref timeout);
								Thread.Sleep(1);
							}
							if (!abortServer && !formClosed)
							{
								socketForServer = myListener.AcceptSocket();
								socketForServer.SendTimeout = 100;
								Console.WriteLine("TCP Connection found.\n");
								timeout = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
							}
						}
					}
					#endregion

					Thread.MemoryBarrier();

					//if we have picture we inform the client (Cicero)

					if (pictureHasBeenTaken)
					{
						this.Text = "#" + shotID + " at " + cameraName + " in " + the_day + the_month + the_year + " on " + System.Net.Dns.GetHostName().ToString();
						conf = "\n   (Note: Picture has been taken on " + cameraName + " (" + Dns.GetHostName().ToString() + ") for " + shotID + ".)\n\n";
						Console.Write(conf);
						if (shotNameBuffer == "")   //no ID/filename received, send created filename ...
						{
							conf = currentProtoBaseString + protoExt;
							Console.Write("Sending " + conf + "\n\n");
							conf = conf.Length.ToString("0000") + conf;

							bconf = Encoding.ASCII.GetBytes(conf);
							try
							{
								socketForServer.Send(bconf, 0, bconf.Length, SocketFlags.None);
							}
							catch { }
						}
						pictureHasBeenTaken = false;
					}

					//if the client (Cicero) has messages we process them
					#region processMessages
					if (!abortServer && !formClosed && socketForServer.Available > 0)
					{
						Console.WriteLine("Message received\n");
						bytes = new byte[socketForServer.Available];
						i = socketForServer.Receive(bytes, 0, socketForServer.Available, SocketFlags.None);
					}
					else if (abortServer || formClosed)
						break;

					if (i > 0)
					{
						string msg = Encoding.ASCII.GetString(bytes);
						this.Refresh();
						Console.WriteLine("Message: " + msg);

						if (msg == "Closing")
						{
							conf = "Confirmed.";
							conf = conf.Length.ToString("0000") + conf;

							bconf = Encoding.ASCII.GetBytes(conf);
							try
							{
								socketForServer.Send(bconf, 0, bconf.Length, SocketFlags.None);
							}
							catch { }
							socketForServer.Close();
							Console.WriteLine("Looping for incoming connection, pending ...");
							timeout = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
							while (!abortServer && !myListener.Pending() && !formClosed)
							{
								displayAni("pending", ref timeout);

								Thread.Sleep(1);
							}
							if (!abortServer && !formClosed)
							{
								socketForServer = myListener.AcceptSocket();
								socketForServer.SendTimeout = 100;
								Console.WriteLine("TCP Connection found.\n");
								timeout = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
							}
							else
								break;

						}
						else if (msg == "Abort")
						{
							newInstructionsReceived = false;



						}
						else
						{
							try
							{
								//parse the message string
								shotNameBuffer = msg.Split('@')[0];
								sequenceTime = Double.Parse(msg.Split('@')[1].Replace('.',',')); // 2017.06.26 Benno Rem: Changed this to add functionality for German language PCs
								FCameraUse = bool.Parse(msg.Split('@')[2]);
								CamID = ushort.Parse(msg.Split('@')[3]);
								saveThisImage = bool.Parse(msg.Split('@')[4]);
								newInstructionsReceived = true;
							}
							catch
							{
								Console.WriteLine("Invalid command string.");
							}

						}
					}
					#endregion

					if (newInstructionsReceived && (cameraCaptureThread == null || !cameraCaptureThread.IsAlive))//!pictureBeingTakenOnPixelfly)//
					{
						string[] dateIDArray = null;

						if (saveThisImage) inc = 1;

						if (shotNameBuffer == "")
						{
							globalImgCounter += inc;

							if (autoCount)
							{
								shotID = globalImgCounter.ToString(idFormat);
							}
							else
							{
								lastID = getLastID(day_folder);
								shotID = (lastID + inc).ToString(idFormat);
							}
						}
						else
						{
							var delim = shotNameBuffer[0];	//first entry is delimiter the string is to be split by
							dateIDArray = shotNameBuffer.Split(delim);
							//Format fix for communication: _YYYY_MM_DD_IIII

							shotID = Convert.ToInt32(dateIDArray[4]).ToString(idFormat);
						}

						// @todo 2017/04/19: change this to accept file strings from the experimental control -- done. NR
						currentFileBaseString = generateFileString(filenameFormat, dateIDArray, Convert.ToInt32(shotID));		//generate pic filename
						currentProtoBaseString = generateFileString(protonameFormat, dateIDArray, Convert.ToInt32(shotID));	//generate proto filename
						filenameBase = day_folder + @"\" + currentFileBaseString;			//Add directory; this prevents the template being set before midnight, the sequence running over midnight, and the iamge ending up saved in the new folder.

						if (!saveThisImage) inc = 0;

						if (camera_connected)
						{
							cameraCaptureThread = new Thread(new ThreadStart(cameraCaptureEntryPoint));
							cameraCaptureThread.Start();
						}

						newInstructionsReceived = false;
					}
				}   //end of while loop
				#endregion
			}
			finally
			{
				while (cameraCaptureThread != null && cameraCaptureThread.IsAlive)
				{ Thread.Sleep(10); }

				myListener.Stop();
				if (socketForServer != null) socketForServer.Close();	//null = socket had never been opened

				Console.WriteLine("Server shutdown!");
			}

		}
    }
}
