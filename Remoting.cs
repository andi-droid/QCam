using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices; //for catching accessviolations in try catch

namespace QCam
{
	public partial class FormMain
	{
		[HandleProcessCorruptedStateExceptions]
		private void remotingEntryPoint()
		{
			try
			{
				remotingServer();
			}
			catch (Exception e)
			{
				Console.Write("Fatal Remoting-Server error: " + e.Message + "\nTrying restart ... ");
				System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "remoting.txt", e.ToString());
				if (!server_error_shown) (new Thread(() => { MessageBox.Show("Server shutdown!\n\nFatal Remoting-Server error: " + e.Message + "\nAttempting automatic restart.", DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information); server_error_shown = false; })).Start();
				server_error_shown = true;
			}
		}

		private void remotingServer()
		{
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
			int port = Properties.Settings.Default.remotingPort;
			if (port == 0)
			{
				MessageBox.Show("Invalid port for listening.");
				return;
			}

			myListener = new TcpListener(lclhst, port);
			myListener.Start();

			int counter = 0;

			while (!abortServer && !formClosed)
			{
				Thread.MemoryBarrier();
				Thread.Sleep(10);
				if (myListener.Pending())
				{
					counter++;
					Remoting clientRemoting = new Remoting(myListener.AcceptSocket(), counter);
				}
			}

			myListener.Stop();
		}
	}


	public class Remoting
	{
		public Remoting(Socket remoteClientFromServer, int remoteNo)
		{
			Console.WriteLine("Instance #" + remoteNo + " of Remoting class constructed.\n");
			Thread ctThread = new Thread(comm);
			ctThread.Start(remoteClientFromServer);
		}

		private void comm(object remoteClientFromServer)
		{
			//handle communication
			Socket remoteClient = (Socket)remoteClientFromServer;
			remoteClient.SendTimeout = 100;
			string msg = "";
			long timeout = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

			try
			{
				//Comm Loop
				while (!(msg == "Closing") && !FormMain.Form1.MainFormClosed && !FormMain.Form1.AbortServer)
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

						if ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeout > 5000)
						{
							remoteClient.Send(tmp, 1, 0);
							timeout = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
						}
					}
					catch (SocketException e)
					{
						// 10035 == WSAEWOULDBLOCK 
						if (!(e.NativeErrorCode.Equals(10035)))
							break;
					}
					#endregion

					Thread.MemoryBarrier();

					//Wait for request
					#region processMessages
					if (!FormMain.Form1.AbortServer && !FormMain.Form1.MainFormClosed && remoteClient.Available > 0)
					{
						bytes = new byte[remoteClient.Available];
						i = remoteClient.Receive(bytes, 0, remoteClient.Available, SocketFlags.None);
					}
					else if (FormMain.Form1.AbortServer || FormMain.Form1.MainFormClosed)
						break;

					if (i > 0)
					{
						msg = Encoding.ASCII.GetString(bytes);

						if (msg == "Closing\n")
						{
							conf = "Confirmed\n";
							//conf = conf.Length.ToString("0000") + conf;

							bconf = Encoding.ASCII.GetBytes(conf);
							try
							{
								remoteClient.Send(bconf, 0, bconf.Length, SocketFlags.None);
							}
							catch { }
						}
						else if (msg == "GetImageNo\n")
						{
							conf = Path.GetFileName(FormMain.Form1.FilenameBase) + FormMain.Form1.ShotID;
							if (conf == "") conf = "none";
							conf = conf + "\n";
							//conf = conf.Length.ToString("0000") + conf;
							bconf = Encoding.ASCII.GetBytes(conf);
							try
							{
								remoteClient.Send(bconf, 0, bconf.Length, SocketFlags.None);
							}
							catch { }
						}
						else Console.WriteLine(msg);
					}
					#endregion
				}   //end of while loop
			}
			finally
			{
				if (remoteClient != null) remoteClient.Close();
				Console.WriteLine("Class closed.");
			}
		}
	}
}