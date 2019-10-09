/* Version 2.0
 * -message from Cicero now includes a bool to decide wether to save or not
 * -saves optical density in png format
 * -sends through tcp the two images with their timestamp
 */


using System;
using System.Drawing;
//using System.Collections;
//using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
//using System.Text;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
//using System.Drawing.Imaging;
//using WINX32Lib;
//using KINETICSTOFRAMESLib;
//using System.Management;
//using System.Linq;
using System.Diagnostics;
using System.Timers;
using System.Runtime.InteropServices;   //For DllImport


namespace QCam
{
	public partial class FormMain : System.Windows.Forms.Form
	{
        //------------------------------------------------------------------------------------------------//

		#region Various Initial stuff & prog entry point

        #region Global variables

        //--------------------------- Stuff for positioning the console window ---------------------------//
        
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        private static IntPtr MyConsole = GetConsoleWindow();
        const int SWP_NOSIZE = 0x0001;

        //--------------------------- Global variables ----------------------------------------------------//

        public static FormMain Form1;

        ICamera camera;

        double sequenceTime;
        bool aborted = false;
        bool FCameraUse = false;
        ushort CamID = 0;
        bool formClosed;

        private static object objLock = new object();

        Socket socketForViewer;
        Thread serverThread;
		Thread remotingThread;
        Thread cameraCaptureThread;
        Queue<string> filesToView = new Queue<string>();

        System.Timers.Timer chrono = new System.Timers.Timer(70);

        string the_year;
        string the_month;
        string the_day;
        string day_folder;
        string shotID;
        string axisID;
		string idFormat;
		string filenameBase;

        int globalImgCounter = 0;

		bool abortServer;

        bool camera_connected;

        bool pictureBeingTaken;
        bool pictureHasBeenTaken;

        bool forceUpdate = false;

		bool tiff = true;
		bool png = false;
		bool tiffCompress = false;

		bool cooling = false;

		bool videoProps = false;

		bool snapshot;

        bool saveThisImage;
        //bool showOD = Properties.Settings.Default.showOD;
        //bool showRaw = Properties.Settings.Default.showRawImages;
        bool autoSubDark = Properties.Settings.Default.subDark;
        bool autoCount = Properties.Settings.Default.autoCount;
        bool showLog = Properties.Settings.Default.showLog;
        bool saveDark = Properties.Settings.Default.saveDark;
		int subDarkImg1 = Properties.Settings.Default.subDarkImage1;
		int subDarkImg2 = Properties.Settings.Default.subDarkImage2;

		public string FilenameBase
		{
			get
			{
				return filenameBase;
			}
		}
		public string ShotID
		{
			get
			{
				return shotID;
			}
		}
		public bool AbortServer
		{
			get
			{
				return abortServer;
			}
		}

		public bool MainFormClosed
		{
			get
			{
				return formClosed;
			}
		}

        //public bool ShowOD
        //{
        //    set
        //    {
        //        showOD = value;
        //    }
        //}

        //public bool ShowRaw
        //{
        //    set
        //    {
        //        showRaw = value;
        //    }
        //}

        public bool AutoSubDark
        {
            set
            {
                autoSubDark = value;
            }
        }

		public int SubDarkImg1
		{
			set
			{
				if (value < 0 && value > 1)
					subDarkImg1 = 1;
				else subDarkImg1 = value;
			}
		}

		public int SubDarkImg2
		{
			set
			{
				if (value < 0 && value > 1)
					subDarkImg2 = 2;
				else subDarkImg2 = value;
			}
		}

        public bool AutoCount
        {
            set
            {
                autoCount = value;
            }
        }

        public bool ShowLog
        {
            get
            {
                return showLog;
            }
            set
            {
                showLog = value;
            }
        }

        public bool SaveDark
        {
            set
            {
                saveDark = value;
            }
        }

		public bool Tiff
		{
			get
			{
				return tiff;
			}
		}

        string cameraName = Properties.Settings.Default.programName;

		#region GUI Components
		private Panel panel1;
		private TextBox textBoxNoImages;
		private Label labelNoImages;
		private GroupBox groupBoxShutter;
		private Label labelSTTClose;
		private TextBox textBoxSTTOpen;
		private Label labelSTTOpen;
		private Label labelSTT;
		private GroupBox groupBoxShifting;
		private Label labelVSAmp;
		private Label labelVSS;
		private Label labelFKVSS;
		private Label labelHSS;
		private Label labelADChannel;
		private Label labelRange;
		private Label labelC;
		private Label labelCoolingSwitch;
		private Panel panel4;
		private Label labelTemp;
		private Panel panel3;
		private Panel panel2;
		private Label tempLable_60;
		private Label tempLable_30;
		private Label tempLable30;
		private Label labelAxis;
		private ComboBox axisBox;
		private Label tempLable0;
		private Label tempLabel60;
		private TrackBar tempBar;
		private ComboBox cameraSelectorBox;
		private CheckBox enableDoubleImage;
		private CheckBox enableCamera;
		private TextBox textBoxWidth;
		private TextBox textBoxHeight;
		private CheckBox fluoPixCheckbox;
		private TextBox textBoxCamera;
		private TextBox textBoxXOffset;
		private TextBox textBoxTemperature;
		private TextBox textBoxYOffset;
		private Label labelTemperature;
		private Label labelGain;
		private Label labelCamera;
		private TextBox textBoxSubX;
		private Label labelExposure;
		private Button buttonConnect;
		private TextBox textBoxSubY;
		private Label labelCooling;
		private Button buttonUpdate;
		private Label labelWidth;
		private RadioButton radioButton8Bits;
		private Button buttonRefresh;
		private Label labelHeight;
		private TextBox textBoxExposure;
		private CheckBox checkBoxBinning;
		private Label labelXOffset;
		private Button buttonSnapshot;
		private Label labelYOffset;
		private Button buttonPreview;
		private Label labelSubX;
		private Button buttonVideoProps;
		private RadioButton radioButton16Bits;
		private Label labelSubY;
		private TextBox textBoxSTTClose;
		private ComboBox SignalBox;
		private ComboBox ModeBox;
		private Label labelSignal;
		private ComboBox FKVSSBox;
		private ComboBox HSSBox;
		private ComboBox ADChannelBox;
		private ComboBox gainBox;
		private ComboBox VSSBox;
		private CheckBox checkBoxCompress;
		private System.Windows.Forms.Timer timerServerControl;
		private GroupBox groupBoxEM;
		private Label labelCaution;
		private Label labelEM;
		private CheckBox checkBoxEM;
		private TextBox textBoxEM;
		private ComboBox VClockVoltageBox;
		#endregion

		string filenameFormat = Properties.Settings.Default.filenameFormat;
		public string FilenameFormat
		{
			set
			{
				filenameFormat = value;
			}
		}

		string protonameFormat = Properties.Settings.Default.protonameFormat;
		public string ProtonameFormat
		{
			get
			{
				return protonameFormat;
			}
			set
			{
				protonameFormat = value;
			}
		}

		string protoExt = Properties.Settings.Default.protoExtension;
		public string ProtoExt
		{
			get
			{
				return protoExt;
			}
			set
			{
				protoExt = value;
			}
		}

		public string IdFormat
		{
			get
			{
				return idFormat;
			}
		}

        int triggerWait = Properties.Settings.Default.triggerWait;
        public int TriggerWait
        {
            set
            {
                triggerWait = value;
            }
        }



		public void updateAxisBox()
		{
			if (Properties.Settings.Default.cameraList.Count > 0)
			{
				axisBox.Items.Clear();
				foreach (CameraInfo l in Properties.Settings.Default.cameraList)
				{
					axisBox.Items.Add(l.AxisID.ToString() + " (" + l.CameraName + ")");
				}
				try
				{
					axisBox.SelectedIndex = Properties.Settings.Default.axisID;
				}
				catch (Exception)
				{
					axisBox.SelectedIndex = 0;
				}
				axisID = Properties.Settings.Default.cameraList[axisBox.SelectedIndex].AxisID.ToString();
				cameraName = Properties.Settings.Default.cameraList[axisBox.SelectedIndex].CameraName;
			}
			else
			{
				axisBox.Items.Clear();
				int max = Properties.Settings.Default.axisID;
				for (int i = 0; i <= max; i++)
					axisBox.Items.Add(i.ToString());
				axisBox.SelectedIndex = Properties.Settings.Default.axisID;
				axisID = axisBox.SelectedIndex.ToString();
			}
		}

		public void setAxisID()
		{
			//get current AxisID
			if (Properties.Settings.Default.cameraList.Count > 0)
			{
				axisID = Properties.Settings.Default.cameraList[axisBox.SelectedIndex].AxisID.ToString();
				cameraName = Properties.Settings.Default.cameraList[axisBox.SelectedIndex].CameraName;
			}
			else
			{
				axisID = axisBox.SelectedIndex.ToString();
			}
		}

        bool error_server;
        System.Timers.Timer m_timer = new System.Timers.Timer();
        private bool Connected;
        private bool Previewing;

        private float Exposure;
        private int Gain;
        private int _Width;
        private int _Height;

        public IntPtr handle = IntPtr.Zero;

        public bool initIsDone = true;
        public System.Timers.Timer pleaseWaitTimer;
        public System.Timers.Timer tempTimer;
		private ImageList imageList1;

        public Process camWare;

		private static bool app_error_shown = false;

        #endregion

        [STAThread]
		static void Main()
		{
            SetWindowPos(MyConsole, 0, 400, 0, 0, 0, SWP_NOSIZE);

            CheckForIllegalCrossThreadCalls = false;
			Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
			Application.Run(Form1 = new FormMain());
		}

		static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Console.WriteLine("Fatal application error: " + e.Exception.Message);
			System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "application.txt", e.Exception.ToString());
			if (!app_error_shown) (new Thread(() => { MessageBox.Show("Fatal application error: " + e.Exception.Message, DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information); app_error_shown = false; })).Start();
			app_error_shown = true;
		}

		// Program entry point :
		public FormMain()
		{
			//
			// Required for Windows Form Designer support
			//

			InitializeComponent();
			Midnight_Timer_Start();
			getDayFolder();


			if (!false)	//error_server --> server drive, pictures folder
			{
				//this.Size = new Size(712, 335);
				chrono.AutoReset = true;
				formClosed = false;
				

				try
				{
					this.Text = cameraName + " in " + the_day + the_month + the_year + " on " + Dns.GetHostName().ToString(); //+ ":" + port.ToString();

				}
				catch { }


				//enablePixelfly.Checked = Properties.Settings.Default.enabPix;


				//initIsDone = false;
				//initCameras();


				updateAxisBox();
				//axisBox.SelectedIndex = 0;

				ModeBox.SelectedIndex = 1;
				SignalBox.SelectedIndex = 0;

				pleaseWaitDialog = new Form();
				pleaseWaitDialog.Location = new Point(20, 20);
				pleaseWaitDialog.Size = new Size(500, 120);
				Label pleaseWaitLabel = new Label();
				pleaseWaitDialog.Controls.Add(pleaseWaitLabel);
				pleaseWaitLabel.AutoSize = true;
				pleaseWaitLabel.Font = new System.Drawing.Font("Times New Roman", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				pleaseWaitLabel.Location = new System.Drawing.Point(5, 0);
				pleaseWaitLabel.Name = "pleaseWaitLabel";
				pleaseWaitLabel.Size = new System.Drawing.Size(101, 19);
				pleaseWaitLabel.Text = "Please Wait ...";

				pleaseWaitTimer = new System.Timers.Timer(500);
				pleaseWaitTimer.Elapsed += new ElapsedEventHandler(pleaseWaitTimer_Elapsed);
				pleaseWaitTimer.AutoReset = false;

				camWare = new Process();

				camWare.StartInfo.FileName = "C:\\Program Files (x86)\\CamWare\\CamWare.exe";

				camWareTimer = new System.Timers.Timer(500);
				camWareTimer.Elapsed += new ElapsedEventHandler(camWareTimer_Elapsed);
				camWareTimer.AutoReset = false;

                triggerTimer = new System.Timers.Timer(1000);
                triggerTimer.Elapsed += new ElapsedEventHandler(triggerTimer_Elapsed);
                triggerTimer.AutoReset = false;

                tempTimer = new System.Timers.Timer(100);
                tempTimer.Elapsed += new ElapsedEventHandler(tempTimer_Elapsed);
                tempTimer.AutoReset = true;

				//**** START THE SERVER THREADS ****
				Console.Write("Starting Server Threads ... ");
				abortServer = false;
				serverThread = new Thread(new ThreadStart(serverEntryPoint));
				serverThread.Start();			//ServerMain.cs

				remotingThread = new Thread(new ThreadStart(remotingEntryPoint));
				remotingThread.Start();			//Remoting.cs

				timerServerControl.Enabled = true;
			}

		}

		private void initCameras()
		{
			try
			{
				Console.WriteLine("\rInitialising camera values ...");

				Connected = false;
				Previewing = false;

				if (camera != null) camera = null;          //e.g. if camera could not be found an instance of the object will still have been created

				//MessageBox.Show(Path.GetDirectoryName(Application.ExecutablePath));

				camera = Camera.CreateObject(cameraSelectorBox.SelectedIndex, Path.GetDirectoryName(Application.ExecutablePath)); // Combobox value, 0 USB, 1 QE

				CameraConnect();		//functions.cs

				fluoPixCheckbox.Enabled = camera_connected;
				fluoPixCheckbox.Checked = Properties.Settings.Default.fluoPix;

				cameraSelectorBox.Enabled = !camera_connected;
				buttonConnect.Enabled = Connected;

				//textBoxExposure.Text = Properties.Settings.Default.expPix.ToString();
				//textBoxGain.Text = Properties.Settings.Default.gainPix.ToString();

				initIsDone = true;
			}
			catch (Exception e)
			{
				Console.Write("Fatal connection error: " + e.Message + "\nCamera is in an undefined state. Attempting disconnect ...\n");
				System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "connect.txt", e.ToString());
				if (!server_error_shown) (new Thread(() => { MessageBox.Show("Fatal connection error: " + e.Message + "\nCamera is in an undefined state. Attempting disconnect ...", DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information); server_error_shown = false; })).Start();
				server_error_shown = true;

				Connected = true;
				CameraConnect();
				camera = null;

				enableCamera.Checked = false;
				enableCamera.Enabled = false;

				initIsDone = true;
			}
		}

        private void disposeCameras()
        {
			if (videoProps) toggleAdvanced(false);

            if (Connected)
            {
                CameraConnect();    //disconnect
                camera = null;      //dispose of object instance

                //fluoPixCheckbox.Enabled = pixelfly_connected;
                //fluoPixCheckbox.Checked = Properties.Settings.Default.fluoPix;

                cameraSelectorBox.Enabled = !Connected;
                buttonConnect.Enabled = Connected;
            }
            else
            {
                camera = null;

                //fluoPixCheckbox.Enabled = pixelfly_connected;
                //fluoPixCheckbox.Checked = Properties.Settings.Default.fluoPix;

                cameraSelectorBox.Enabled = !Connected;
                buttonConnect.Enabled = Connected;
            }
        }

		private void toggleForm(Boolean on)
		{
				if (on)
				{
					labelTemp.Enabled = true;
					labelCamera.Enabled = false;
					textBoxCamera.Enabled = true;
					labelWidth.Enabled = true;
					textBoxWidth.Enabled = true;
					labelHeight.Enabled = true;
					textBoxHeight.Enabled = true;
					labelXOffset.Enabled = true;
					textBoxXOffset.Enabled = true;
					labelYOffset.Enabled = true;
					textBoxYOffset.Enabled = true;
					labelSubX.Enabled = true;
					textBoxSubX.Enabled = true;
					labelSubY.Enabled = true;
					textBoxSubY.Enabled = true;
					radioButton8Bits.Enabled = true;
					radioButton16Bits.Enabled = true;
					//buttonPreview.Enabled = true;
					buttonSnapshot.Enabled = true;
					checkBoxBinning.Enabled = true;
					buttonRefresh.Enabled = true;
					buttonUpdate.Enabled = true;
					labelExposure.Enabled = true;
					textBoxExposure.Enabled = true;
					labelGain.Enabled = true;
					gainBox.Enabled = true;
					buttonConnect.Text = "Disconnect";
					enableDoubleImage.Enabled = true;
                    tempTimer.Start();
				}
				else
				{
					labelTemp.Enabled = false;
					labelCamera.Enabled = true;
					textBoxCamera.Enabled = true;
					labelWidth.Enabled = false;
					textBoxWidth.Enabled = false;
					labelHeight.Enabled = false;
					textBoxHeight.Enabled = false;
					labelXOffset.Enabled = false;
					textBoxXOffset.Enabled = false;
					labelYOffset.Enabled = false;
					textBoxYOffset.Enabled = false;
					labelSubX.Enabled = false;
					textBoxSubX.Enabled = false;
					labelSubY.Enabled = false;
					textBoxSubY.Enabled = false;
					radioButton8Bits.Enabled = false;
					radioButton16Bits.Enabled = false;
					//buttonPreview.Enabled = false;
					buttonSnapshot.Enabled = false;
					checkBoxBinning.Enabled = false;
					buttonRefresh.Enabled = false;
					buttonUpdate.Enabled = false;
					labelExposure.Enabled = false;
					textBoxExposure.Enabled = false;
					labelGain.Enabled = false;
					gainBox.Enabled = false;
					buttonConnect.Text = "Connect";
					//comboBoxFKVSS.Enabled = false;
					//buttonVideoProps.Enabled = false;
					enableDoubleImage.Enabled = false;
					FKVSSBox.Enabled = false;
					ADChannelBox.Enabled = false;
					HSSBox.Enabled = false;
					VSSBox.Enabled = false;
					VClockVoltageBox.Enabled = false;
					ModeBox.Enabled = false;
					SignalBox.Enabled = false;
					textBoxNoImages.Enabled = false;
					textBoxTemperature.Enabled = false;
                    tempTimer.Stop();
                    tempBar.Value = 0;
				}
		}

        private void toggleAdvanced(Boolean on)
		{
			if (camera != null && camera.Descr.isAdvanced)
			{
				if (on)
				{
					this.Height += 200;
					groupBoxShifting.Visible = true;
					groupBoxShutter.Visible = true;
					videoProps = true;
				}
				else
				{
					this.Height -= 200;
					groupBoxShifting.Visible = false;
					groupBoxShutter.Visible = false;
					videoProps = false;
				} 
			}
		}
		#endregion

        private System.ComponentModel.IContainer components;

        #region Declarations Initiales

        private ToolStripMenuItem settingsToolStripMenuItem;
        private MenuStrip menuStrip1;
		private SaveFileDialog saveFileDialog1;
        Form pleaseWaitDialog;

		#endregion
		
		

		#region Other declarations

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{

				// Stop preview
				if (Previewing)
					CameraPreview();

				// Disconnect from camera
				if (Connected)
					CameraConnect();

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.groupBoxEM = new System.Windows.Forms.GroupBox();
			this.textBoxEM = new System.Windows.Forms.TextBox();
			this.labelCaution = new System.Windows.Forms.Label();
			this.labelEM = new System.Windows.Forms.Label();
			this.checkBoxEM = new System.Windows.Forms.CheckBox();
			this.gainBox = new System.Windows.Forms.ComboBox();
			this.textBoxNoImages = new System.Windows.Forms.TextBox();
			this.labelNoImages = new System.Windows.Forms.Label();
			this.groupBoxShutter = new System.Windows.Forms.GroupBox();
			this.SignalBox = new System.Windows.Forms.ComboBox();
			this.ModeBox = new System.Windows.Forms.ComboBox();
			this.textBoxSTTClose = new System.Windows.Forms.TextBox();
			this.labelSTTClose = new System.Windows.Forms.Label();
			this.textBoxSTTOpen = new System.Windows.Forms.TextBox();
			this.labelSTTOpen = new System.Windows.Forms.Label();
			this.labelSignal = new System.Windows.Forms.Label();
			this.labelSTT = new System.Windows.Forms.Label();
			this.groupBoxShifting = new System.Windows.Forms.GroupBox();
			this.VClockVoltageBox = new System.Windows.Forms.ComboBox();
			this.VSSBox = new System.Windows.Forms.ComboBox();
			this.HSSBox = new System.Windows.Forms.ComboBox();
			this.ADChannelBox = new System.Windows.Forms.ComboBox();
			this.FKVSSBox = new System.Windows.Forms.ComboBox();
			this.labelVSAmp = new System.Windows.Forms.Label();
			this.labelVSS = new System.Windows.Forms.Label();
			this.labelFKVSS = new System.Windows.Forms.Label();
			this.labelHSS = new System.Windows.Forms.Label();
			this.labelADChannel = new System.Windows.Forms.Label();
			this.labelRange = new System.Windows.Forms.Label();
			this.labelC = new System.Windows.Forms.Label();
			this.labelCoolingSwitch = new System.Windows.Forms.Label();
			this.panel4 = new System.Windows.Forms.Panel();
			this.labelTemp = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.tempLable_60 = new System.Windows.Forms.Label();
			this.tempLable_30 = new System.Windows.Forms.Label();
			this.tempLable30 = new System.Windows.Forms.Label();
			this.labelAxis = new System.Windows.Forms.Label();
			this.axisBox = new System.Windows.Forms.ComboBox();
			this.tempLable0 = new System.Windows.Forms.Label();
			this.tempLabel60 = new System.Windows.Forms.Label();
			this.tempBar = new System.Windows.Forms.TrackBar();
			this.cameraSelectorBox = new System.Windows.Forms.ComboBox();
			this.enableDoubleImage = new System.Windows.Forms.CheckBox();
			this.enableCamera = new System.Windows.Forms.CheckBox();
			this.textBoxWidth = new System.Windows.Forms.TextBox();
			this.textBoxHeight = new System.Windows.Forms.TextBox();
			this.fluoPixCheckbox = new System.Windows.Forms.CheckBox();
			this.textBoxCamera = new System.Windows.Forms.TextBox();
			this.textBoxXOffset = new System.Windows.Forms.TextBox();
			this.textBoxTemperature = new System.Windows.Forms.TextBox();
			this.textBoxYOffset = new System.Windows.Forms.TextBox();
			this.labelTemperature = new System.Windows.Forms.Label();
			this.labelGain = new System.Windows.Forms.Label();
			this.labelCamera = new System.Windows.Forms.Label();
			this.textBoxSubX = new System.Windows.Forms.TextBox();
			this.labelExposure = new System.Windows.Forms.Label();
			this.buttonConnect = new System.Windows.Forms.Button();
			this.textBoxSubY = new System.Windows.Forms.TextBox();
			this.labelCooling = new System.Windows.Forms.Label();
			this.buttonUpdate = new System.Windows.Forms.Button();
			this.labelWidth = new System.Windows.Forms.Label();
			this.radioButton8Bits = new System.Windows.Forms.RadioButton();
			this.buttonRefresh = new System.Windows.Forms.Button();
			this.labelHeight = new System.Windows.Forms.Label();
			this.textBoxExposure = new System.Windows.Forms.TextBox();
			this.checkBoxBinning = new System.Windows.Forms.CheckBox();
			this.labelXOffset = new System.Windows.Forms.Label();
			this.buttonSnapshot = new System.Windows.Forms.Button();
			this.labelYOffset = new System.Windows.Forms.Label();
			this.buttonPreview = new System.Windows.Forms.Button();
			this.labelSubX = new System.Windows.Forms.Label();
			this.buttonVideoProps = new System.Windows.Forms.Button();
			this.radioButton16Bits = new System.Windows.Forms.RadioButton();
			this.labelSubY = new System.Windows.Forms.Label();
			this.checkBoxCompress = new System.Windows.Forms.CheckBox();
			this.timerServerControl = new System.Windows.Forms.Timer(this.components);
			this.menuStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.groupBoxEM.SuspendLayout();
			this.groupBoxShutter.SuspendLayout();
			this.groupBoxShifting.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tempBar)).BeginInit();
			this.SuspendLayout();
			// 
			// settingsToolStripMenuItem
			// 
			this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			this.settingsToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
			this.settingsToolStripMenuItem.Text = "Settings...";
			this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(339, 24);
			this.menuStrip1.TabIndex = 80;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.AddExtension = false;
			this.saveFileDialog1.FilterIndex = 2;
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// panel1
			// 
			this.panel1.AutoSize = true;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.groupBoxEM);
			this.panel1.Controls.Add(this.gainBox);
			this.panel1.Controls.Add(this.textBoxNoImages);
			this.panel1.Controls.Add(this.labelNoImages);
			this.panel1.Controls.Add(this.groupBoxShutter);
			this.panel1.Controls.Add(this.groupBoxShifting);
			this.panel1.Controls.Add(this.labelRange);
			this.panel1.Controls.Add(this.labelC);
			this.panel1.Controls.Add(this.labelCoolingSwitch);
			this.panel1.Controls.Add(this.panel4);
			this.panel1.Controls.Add(this.labelTemp);
			this.panel1.Controls.Add(this.panel3);
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Controls.Add(this.tempLable_60);
			this.panel1.Controls.Add(this.tempLable_30);
			this.panel1.Controls.Add(this.tempLable30);
			this.panel1.Controls.Add(this.labelAxis);
			this.panel1.Controls.Add(this.axisBox);
			this.panel1.Controls.Add(this.tempLable0);
			this.panel1.Controls.Add(this.tempLabel60);
			this.panel1.Controls.Add(this.tempBar);
			this.panel1.Controls.Add(this.cameraSelectorBox);
			this.panel1.Controls.Add(this.enableDoubleImage);
			this.panel1.Controls.Add(this.enableCamera);
			this.panel1.Controls.Add(this.textBoxWidth);
			this.panel1.Controls.Add(this.textBoxHeight);
			this.panel1.Controls.Add(this.fluoPixCheckbox);
			this.panel1.Controls.Add(this.textBoxCamera);
			this.panel1.Controls.Add(this.textBoxXOffset);
			this.panel1.Controls.Add(this.textBoxTemperature);
			this.panel1.Controls.Add(this.textBoxYOffset);
			this.panel1.Controls.Add(this.labelTemperature);
			this.panel1.Controls.Add(this.labelGain);
			this.panel1.Controls.Add(this.labelCamera);
			this.panel1.Controls.Add(this.textBoxSubX);
			this.panel1.Controls.Add(this.labelExposure);
			this.panel1.Controls.Add(this.buttonConnect);
			this.panel1.Controls.Add(this.textBoxSubY);
			this.panel1.Controls.Add(this.labelCooling);
			this.panel1.Controls.Add(this.buttonUpdate);
			this.panel1.Controls.Add(this.labelWidth);
			this.panel1.Controls.Add(this.radioButton8Bits);
			this.panel1.Controls.Add(this.buttonRefresh);
			this.panel1.Controls.Add(this.labelHeight);
			this.panel1.Controls.Add(this.textBoxExposure);
			this.panel1.Controls.Add(this.checkBoxBinning);
			this.panel1.Controls.Add(this.labelXOffset);
			this.panel1.Controls.Add(this.buttonSnapshot);
			this.panel1.Controls.Add(this.labelYOffset);
			this.panel1.Controls.Add(this.buttonPreview);
			this.panel1.Controls.Add(this.labelSubX);
			this.panel1.Controls.Add(this.buttonVideoProps);
			this.panel1.Controls.Add(this.radioButton16Bits);
			this.panel1.Controls.Add(this.labelSubY);
			this.panel1.Controls.Add(this.checkBoxCompress);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 24);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(339, 551);
			this.panel1.TabIndex = 121;
			// 
			// groupBoxEM
			// 
			this.groupBoxEM.Controls.Add(this.textBoxEM);
			this.groupBoxEM.Controls.Add(this.labelCaution);
			this.groupBoxEM.Controls.Add(this.labelEM);
			this.groupBoxEM.Controls.Add(this.checkBoxEM);
			this.groupBoxEM.Location = new System.Drawing.Point(11, 495);
			this.groupBoxEM.Name = "groupBoxEM";
			this.groupBoxEM.Size = new System.Drawing.Size(315, 44);
			this.groupBoxEM.TabIndex = 184;
			this.groupBoxEM.TabStop = false;
			this.groupBoxEM.Text = "EM-Gain";
			// 
			// textBoxEM
			// 
			this.textBoxEM.Enabled = false;
			this.textBoxEM.Location = new System.Drawing.Point(131, 13);
			this.textBoxEM.Name = "textBoxEM";
			this.textBoxEM.Size = new System.Drawing.Size(44, 20);
			this.textBoxEM.TabIndex = 186;
			this.textBoxEM.Text = "0";
			// 
			// labelCaution
			// 
			this.labelCaution.ForeColor = System.Drawing.Color.Crimson;
			this.labelCaution.Location = new System.Drawing.Point(192, 12);
			this.labelCaution.Name = "labelCaution";
			this.labelCaution.Size = new System.Drawing.Size(112, 29);
			this.labelCaution.TabIndex = 185;
			this.labelCaution.Text = "CAUTION\r\nRisk of CCD damage";
			this.labelCaution.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// labelEM
			// 
			this.labelEM.Location = new System.Drawing.Point(175, 16);
			this.labelEM.Name = "labelEM";
			this.labelEM.Size = new System.Drawing.Size(56, 16);
			this.labelEM.TabIndex = 184;
			this.labelEM.Text = "x";
			// 
			// checkBoxEM
			// 
			this.checkBoxEM.Location = new System.Drawing.Point(6, 12);
			this.checkBoxEM.Name = "checkBoxEM";
			this.checkBoxEM.Size = new System.Drawing.Size(148, 24);
			this.checkBoxEM.TabIndex = 183;
			this.checkBoxEM.Text = "Enable EMCCD";
			this.checkBoxEM.CheckedChanged += new System.EventHandler(this.checkBoxEM_CheckedChanged);
			// 
			// gainBox
			// 
			this.gainBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.gainBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.gainBox.Enabled = false;
			this.gainBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.gainBox.FormattingEnabled = true;
			this.gainBox.ItemHeight = 13;
			this.gainBox.Location = new System.Drawing.Point(190, 61);
			this.gainBox.Name = "gainBox";
			this.gainBox.Size = new System.Drawing.Size(44, 21);
			this.gainBox.TabIndex = 181;
			// 
			// textBoxNoImages
			// 
			this.textBoxNoImages.Enabled = false;
			this.textBoxNoImages.Location = new System.Drawing.Point(106, 269);
			this.textBoxNoImages.Name = "textBoxNoImages";
			this.textBoxNoImages.ReadOnly = true;
			this.textBoxNoImages.Size = new System.Drawing.Size(16, 20);
			this.textBoxNoImages.TabIndex = 180;
			this.textBoxNoImages.Text = "2";
			// 
			// labelNoImages
			// 
			this.labelNoImages.Location = new System.Drawing.Point(14, 266);
			this.labelNoImages.Name = "labelNoImages";
			this.labelNoImages.Size = new System.Drawing.Size(86, 24);
			this.labelNoImages.TabIndex = 179;
			this.labelNoImages.Text = "Images in series";
			this.labelNoImages.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBoxShutter
			// 
			this.groupBoxShutter.Controls.Add(this.SignalBox);
			this.groupBoxShutter.Controls.Add(this.ModeBox);
			this.groupBoxShutter.Controls.Add(this.textBoxSTTClose);
			this.groupBoxShutter.Controls.Add(this.labelSTTClose);
			this.groupBoxShutter.Controls.Add(this.textBoxSTTOpen);
			this.groupBoxShutter.Controls.Add(this.labelSTTOpen);
			this.groupBoxShutter.Controls.Add(this.labelSignal);
			this.groupBoxShutter.Controls.Add(this.labelSTT);
			this.groupBoxShutter.Location = new System.Drawing.Point(203, 344);
			this.groupBoxShutter.Name = "groupBoxShutter";
			this.groupBoxShutter.Size = new System.Drawing.Size(123, 145);
			this.groupBoxShutter.TabIndex = 178;
			this.groupBoxShutter.TabStop = false;
			this.groupBoxShutter.Text = "Shutter";
			this.groupBoxShutter.Visible = false;
			// 
			// SignalBox
			// 
			this.SignalBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.SignalBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.SignalBox.DropDownWidth = 44;
			this.SignalBox.Enabled = false;
			this.SignalBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SignalBox.FormattingEnabled = true;
			this.SignalBox.ItemHeight = 13;
			this.SignalBox.Items.AddRange(new object[] {
            "Low",
            "High"});
			this.SignalBox.Location = new System.Drawing.Point(58, 41);
			this.SignalBox.Name = "SignalBox";
			this.SignalBox.Size = new System.Drawing.Size(54, 21);
			this.SignalBox.TabIndex = 182;
			// 
			// ModeBox
			// 
			this.ModeBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.ModeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ModeBox.DropDownWidth = 66;
			this.ModeBox.Enabled = false;
			this.ModeBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ModeBox.FormattingEnabled = true;
			this.ModeBox.ItemHeight = 13;
			this.ModeBox.Items.AddRange(new object[] {
            "Fully Auto",
            "Always Open",
            "Always Closed",
            "Open for FVB",
            "Open for Series"});
			this.ModeBox.Location = new System.Drawing.Point(9, 14);
			this.ModeBox.Name = "ModeBox";
			this.ModeBox.Size = new System.Drawing.Size(103, 21);
			this.ModeBox.TabIndex = 181;
			// 
			// textBoxSTTClose
			// 
			this.textBoxSTTClose.Location = new System.Drawing.Point(68, 119);
			this.textBoxSTTClose.Name = "textBoxSTTClose";
			this.textBoxSTTClose.ReadOnly = true;
			this.textBoxSTTClose.Size = new System.Drawing.Size(44, 20);
			this.textBoxSTTClose.TabIndex = 154;
			this.textBoxSTTClose.Text = "0";
			// 
			// labelSTTClose
			// 
			this.labelSTTClose.Location = new System.Drawing.Point(6, 123);
			this.labelSTTClose.Name = "labelSTTClose";
			this.labelSTTClose.Size = new System.Drawing.Size(59, 16);
			this.labelSTTClose.TabIndex = 153;
			this.labelSTTClose.Text = "Close (ms)";
			// 
			// textBoxSTTOpen
			// 
			this.textBoxSTTOpen.Location = new System.Drawing.Point(68, 93);
			this.textBoxSTTOpen.Name = "textBoxSTTOpen";
			this.textBoxSTTOpen.ReadOnly = true;
			this.textBoxSTTOpen.Size = new System.Drawing.Size(44, 20);
			this.textBoxSTTOpen.TabIndex = 152;
			this.textBoxSTTOpen.Text = "0";
			// 
			// labelSTTOpen
			// 
			this.labelSTTOpen.Location = new System.Drawing.Point(6, 97);
			this.labelSTTOpen.Name = "labelSTTOpen";
			this.labelSTTOpen.Size = new System.Drawing.Size(59, 16);
			this.labelSTTOpen.TabIndex = 151;
			this.labelSTTOpen.Text = "Open (ms)";
			// 
			// labelSignal
			// 
			this.labelSignal.Location = new System.Drawing.Point(6, 45);
			this.labelSignal.Name = "labelSignal";
			this.labelSignal.Size = new System.Drawing.Size(59, 16);
			this.labelSignal.TabIndex = 149;
			this.labelSignal.Text = "TTL out";
			// 
			// labelSTT
			// 
			this.labelSTT.Location = new System.Drawing.Point(4, 74);
			this.labelSTT.Name = "labelSTT";
			this.labelSTT.Size = new System.Drawing.Size(117, 16);
			this.labelSTT.TabIndex = 155;
			this.labelSTT.Text = "Shutter Transfer Time:";
			// 
			// groupBoxShifting
			// 
			this.groupBoxShifting.Controls.Add(this.VClockVoltageBox);
			this.groupBoxShifting.Controls.Add(this.VSSBox);
			this.groupBoxShifting.Controls.Add(this.HSSBox);
			this.groupBoxShifting.Controls.Add(this.ADChannelBox);
			this.groupBoxShifting.Controls.Add(this.FKVSSBox);
			this.groupBoxShifting.Controls.Add(this.labelVSAmp);
			this.groupBoxShifting.Controls.Add(this.labelVSS);
			this.groupBoxShifting.Controls.Add(this.labelFKVSS);
			this.groupBoxShifting.Controls.Add(this.labelHSS);
			this.groupBoxShifting.Controls.Add(this.labelADChannel);
			this.groupBoxShifting.Location = new System.Drawing.Point(11, 344);
			this.groupBoxShifting.Name = "groupBoxShifting";
			this.groupBoxShifting.Size = new System.Drawing.Size(186, 145);
			this.groupBoxShifting.TabIndex = 177;
			this.groupBoxShifting.TabStop = false;
			this.groupBoxShifting.Text = "Shifting (ns/pixelshift)";
			this.groupBoxShifting.Visible = false;
			// 
			// VClockVoltageBox
			// 
			this.VClockVoltageBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.VClockVoltageBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.VClockVoltageBox.DropDownWidth = 44;
			this.VClockVoltageBox.Enabled = false;
			this.VClockVoltageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.VClockVoltageBox.FormattingEnabled = true;
			this.VClockVoltageBox.ItemHeight = 13;
			this.VClockVoltageBox.Items.AddRange(new object[] {
            "Default",
            "+1",
            "+2",
            "+3",
            "+4"});
			this.VClockVoltageBox.Location = new System.Drawing.Point(131, 118);
			this.VClockVoltageBox.Name = "VClockVoltageBox";
			this.VClockVoltageBox.Size = new System.Drawing.Size(45, 21);
			this.VClockVoltageBox.TabIndex = 187;
			// 
			// VSSBox
			// 
			this.VSSBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.VSSBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.VSSBox.DropDownWidth = 44;
			this.VSSBox.Enabled = false;
			this.VSSBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.VSSBox.FormattingEnabled = true;
			this.VSSBox.ItemHeight = 13;
			this.VSSBox.Location = new System.Drawing.Point(131, 94);
			this.VSSBox.Name = "VSSBox";
			this.VSSBox.Size = new System.Drawing.Size(45, 21);
			this.VSSBox.TabIndex = 186;
			// 
			// HSSBox
			// 
			this.HSSBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.HSSBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.HSSBox.DropDownWidth = 44;
			this.HSSBox.Enabled = false;
			this.HSSBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HSSBox.FormattingEnabled = true;
			this.HSSBox.ItemHeight = 13;
			this.HSSBox.Location = new System.Drawing.Point(131, 68);
			this.HSSBox.Name = "HSSBox";
			this.HSSBox.Size = new System.Drawing.Size(45, 21);
			this.HSSBox.TabIndex = 185;
			// 
			// ADChannelBox
			// 
			this.ADChannelBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.ADChannelBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ADChannelBox.DropDownWidth = 44;
			this.ADChannelBox.Enabled = false;
			this.ADChannelBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ADChannelBox.FormattingEnabled = true;
			this.ADChannelBox.ItemHeight = 13;
			this.ADChannelBox.Location = new System.Drawing.Point(131, 41);
			this.ADChannelBox.Name = "ADChannelBox";
			this.ADChannelBox.Size = new System.Drawing.Size(45, 21);
			this.ADChannelBox.TabIndex = 184;
			this.ADChannelBox.SelectedIndexChanged += new System.EventHandler(this.ADChannelBox_SelectedIndexChanged);
			// 
			// FKVSSBox
			// 
			this.FKVSSBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.FKVSSBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.FKVSSBox.DropDownWidth = 44;
			this.FKVSSBox.Enabled = false;
			this.FKVSSBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FKVSSBox.FormattingEnabled = true;
			this.FKVSSBox.ItemHeight = 13;
			this.FKVSSBox.Location = new System.Drawing.Point(131, 14);
			this.FKVSSBox.Name = "FKVSSBox";
			this.FKVSSBox.Size = new System.Drawing.Size(45, 21);
			this.FKVSSBox.TabIndex = 183;
			// 
			// labelVSAmp
			// 
			this.labelVSAmp.Location = new System.Drawing.Point(6, 123);
			this.labelVSAmp.Name = "labelVSAmp";
			this.labelVSAmp.Size = new System.Drawing.Size(122, 19);
			this.labelVSAmp.TabIndex = 155;
			this.labelVSAmp.Text = "Vertical Clock Voltage";
			// 
			// labelVSS
			// 
			this.labelVSS.Location = new System.Drawing.Point(6, 97);
			this.labelVSS.Name = "labelVSS";
			this.labelVSS.Size = new System.Drawing.Size(108, 16);
			this.labelVSS.TabIndex = 153;
			this.labelVSS.Text = "Vertical Shift Speed";
			// 
			// labelFKVSS
			// 
			this.labelFKVSS.Location = new System.Drawing.Point(6, 19);
			this.labelFKVSS.Name = "labelFKVSS";
			this.labelFKVSS.Size = new System.Drawing.Size(122, 16);
			this.labelFKVSS.TabIndex = 145;
			this.labelFKVSS.Text = "FK Vertical Shift Speed";
			// 
			// labelHSS
			// 
			this.labelHSS.Location = new System.Drawing.Point(6, 71);
			this.labelHSS.Name = "labelHSS";
			this.labelHSS.Size = new System.Drawing.Size(126, 16);
			this.labelHSS.TabIndex = 151;
			this.labelHSS.Text = "Horiz. Shift Speed (kHz)";
			// 
			// labelADChannel
			// 
			this.labelADChannel.Location = new System.Drawing.Point(6, 45);
			this.labelADChannel.Name = "labelADChannel";
			this.labelADChannel.Size = new System.Drawing.Size(96, 16);
			this.labelADChannel.TabIndex = 149;
			this.labelADChannel.Text = "AD Channel";
			// 
			// labelRange
			// 
			this.labelRange.ForeColor = System.Drawing.Color.Blue;
			this.labelRange.Location = new System.Drawing.Point(149, 277);
			this.labelRange.Name = "labelRange";
			this.labelRange.Size = new System.Drawing.Size(116, 16);
			this.labelRange.TabIndex = 175;
			this.labelRange.Text = "Range -0 ... +0 °C";
			// 
			// labelC
			// 
			this.labelC.Location = new System.Drawing.Point(243, 255);
			this.labelC.Name = "labelC";
			this.labelC.Size = new System.Drawing.Size(18, 16);
			this.labelC.TabIndex = 174;
			this.labelC.Text = "°C";
			// 
			// labelCoolingSwitch
			// 
			this.labelCoolingSwitch.Cursor = System.Windows.Forms.Cursors.Hand;
			this.labelCoolingSwitch.ForeColor = System.Drawing.Color.Red;
			this.labelCoolingSwitch.Location = new System.Drawing.Point(215, 229);
			this.labelCoolingSwitch.Name = "labelCoolingSwitch";
			this.labelCoolingSwitch.Size = new System.Drawing.Size(60, 11);
			this.labelCoolingSwitch.TabIndex = 172;
			this.labelCoolingSwitch.Text = "■     Off";
			this.labelCoolingSwitch.Click += new System.EventHandler(this.labelCoolingSwitch_Click);
			// 
			// panel4
			// 
			this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel4.Location = new System.Drawing.Point(215, 226);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(25, 18);
			this.panel4.TabIndex = 173;
			// 
			// labelTemp
			// 
			this.labelTemp.Enabled = false;
			this.labelTemp.ForeColor = System.Drawing.Color.Green;
			this.labelTemp.Location = new System.Drawing.Point(271, 321);
			this.labelTemp.Name = "labelTemp";
			this.labelTemp.Size = new System.Drawing.Size(67, 19);
			this.labelTemp.TabIndex = 167;
			this.labelTemp.Text = "Steady";
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.Color.RoyalBlue;
			this.panel3.Location = new System.Drawing.Point(304, 255);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(3, 65);
			this.panel3.TabIndex = 171;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.OrangeRed;
			this.panel2.Location = new System.Drawing.Point(304, 190);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(3, 65);
			this.panel2.TabIndex = 170;
			// 
			// tempLable_60
			// 
			this.tempLable_60.AutoSize = true;
			this.tempLable_60.Location = new System.Drawing.Point(268, 308);
			this.tempLable_60.Name = "tempLable_60";
			this.tempLable_60.Size = new System.Drawing.Size(36, 13);
			this.tempLable_60.TabIndex = 169;
			this.tempLable_60.Text = "-60 °C";
			// 
			// tempLable_30
			// 
			this.tempLable_30.AutoSize = true;
			this.tempLable_30.Location = new System.Drawing.Point(268, 277);
			this.tempLable_30.Name = "tempLable_30";
			this.tempLable_30.Size = new System.Drawing.Size(36, 13);
			this.tempLable_30.TabIndex = 168;
			this.tempLable_30.Text = "-30 °C";
			// 
			// tempLable30
			// 
			this.tempLable30.AutoSize = true;
			this.tempLable30.Location = new System.Drawing.Point(271, 216);
			this.tempLable30.Name = "tempLable30";
			this.tempLable30.Size = new System.Drawing.Size(33, 13);
			this.tempLable30.TabIndex = 166;
			this.tempLable30.Text = "30 °C";
			// 
			// labelAxis
			// 
			this.labelAxis.Location = new System.Drawing.Point(19, 218);
			this.labelAxis.Name = "labelAxis";
			this.labelAxis.Size = new System.Drawing.Size(26, 16);
			this.labelAxis.TabIndex = 165;
			this.labelAxis.Text = "Axis";
			// 
			// axisBox
			// 
			this.axisBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.axisBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.axisBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.axisBox.FormattingEnabled = true;
			this.axisBox.ItemHeight = 13;
			this.axisBox.Items.AddRange(new object[] {
            "0",
            "1"});
			this.axisBox.Location = new System.Drawing.Point(17, 239);
			this.axisBox.Name = "axisBox";
			this.axisBox.Size = new System.Drawing.Size(105, 21);
			this.axisBox.TabIndex = 164;
			this.axisBox.SelectedIndexChanged += new System.EventHandler(this.axisBox_SelectedIndexChanged);
			// 
			// tempLable0
			// 
			this.tempLable0.AutoSize = true;
			this.tempLable0.Location = new System.Drawing.Point(277, 247);
			this.tempLable0.Name = "tempLable0";
			this.tempLable0.Size = new System.Drawing.Size(27, 13);
			this.tempLable0.TabIndex = 163;
			this.tempLable0.Text = "0 °C";
			// 
			// tempLabel60
			// 
			this.tempLabel60.AutoSize = true;
			this.tempLabel60.Location = new System.Drawing.Point(271, 184);
			this.tempLabel60.Name = "tempLabel60";
			this.tempLabel60.Size = new System.Drawing.Size(33, 13);
			this.tempLabel60.TabIndex = 162;
			this.tempLabel60.Text = "60 °C";
			// 
			// tempBar
			// 
			this.tempBar.BackColor = System.Drawing.SystemColors.Control;
			this.tempBar.LargeChange = 0;
			this.tempBar.Location = new System.Drawing.Point(304, 179);
			this.tempBar.Maximum = 60;
			this.tempBar.Minimum = -60;
			this.tempBar.Name = "tempBar";
			this.tempBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.tempBar.Size = new System.Drawing.Size(45, 150);
			this.tempBar.SmallChange = 0;
			this.tempBar.TabIndex = 161;
			this.tempBar.TickFrequency = 10;
			this.tempBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
			this.tempBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tempBar_MouseDown);
			// 
			// cameraSelectorBox
			// 
			this.cameraSelectorBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cameraSelectorBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cameraSelectorBox.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cameraSelectorBox.FormattingEnabled = true;
			this.cameraSelectorBox.Items.AddRange(new object[] {
            "PIXELFLY USB",
            "PIXELFLY QE",
            "ANDOR IKON",
            "ANDOR IXONULTRA"});
			this.cameraSelectorBox.Location = new System.Drawing.Point(14, 6);
			this.cameraSelectorBox.Name = "cameraSelectorBox";
			this.cameraSelectorBox.Size = new System.Drawing.Size(183, 27);
			this.cameraSelectorBox.TabIndex = 160;
			this.cameraSelectorBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// enableDoubleImage
			// 
			this.enableDoubleImage.Enabled = false;
			this.enableDoubleImage.Location = new System.Drawing.Point(171, 177);
			this.enableDoubleImage.Name = "enableDoubleImage";
			this.enableDoubleImage.Size = new System.Drawing.Size(94, 24);
			this.enableDoubleImage.TabIndex = 159;
			this.enableDoubleImage.Text = "Double Image";
			this.enableDoubleImage.CheckedChanged += new System.EventHandler(this.enableDoubleImage_CheckedChanged);
			// 
			// enableCamera
			// 
			this.enableCamera.AutoSize = true;
			this.enableCamera.Enabled = false;
			this.enableCamera.Location = new System.Drawing.Point(17, 38);
			this.enableCamera.Name = "enableCamera";
			this.enableCamera.Size = new System.Drawing.Size(187, 17);
			this.enableCamera.TabIndex = 158;
			this.enableCamera.Text = "Activate camera and lock selector";
			this.enableCamera.UseVisualStyleBackColor = true;
			this.enableCamera.CheckedChanged += new System.EventHandler(this.enableCamera_CheckedChanged);
			// 
			// textBoxWidth
			// 
			this.textBoxWidth.Enabled = false;
			this.textBoxWidth.Location = new System.Drawing.Point(78, 84);
			this.textBoxWidth.Name = "textBoxWidth";
			this.textBoxWidth.ReadOnly = true;
			this.textBoxWidth.Size = new System.Drawing.Size(44, 20);
			this.textBoxWidth.TabIndex = 123;
			// 
			// textBoxHeight
			// 
			this.textBoxHeight.Enabled = false;
			this.textBoxHeight.Location = new System.Drawing.Point(190, 84);
			this.textBoxHeight.Name = "textBoxHeight";
			this.textBoxHeight.ReadOnly = true;
			this.textBoxHeight.Size = new System.Drawing.Size(44, 20);
			this.textBoxHeight.TabIndex = 125;
			// 
			// fluoPixCheckbox
			// 
			this.fluoPixCheckbox.AutoSize = true;
			this.fluoPixCheckbox.Checked = true;
			this.fluoPixCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.fluoPixCheckbox.Location = new System.Drawing.Point(231, 1);
			this.fluoPixCheckbox.Name = "fluoPixCheckbox";
			this.fluoPixCheckbox.Size = new System.Drawing.Size(90, 17);
			this.fluoPixCheckbox.TabIndex = 157;
			this.fluoPixCheckbox.Text = "Fluorescence";
			this.fluoPixCheckbox.UseVisualStyleBackColor = true;
			this.fluoPixCheckbox.Visible = false;
			this.fluoPixCheckbox.CheckedChanged += new System.EventHandler(this.fluoPixCheckbox_CheckedChanged);
			// 
			// textBoxCamera
			// 
			this.textBoxCamera.Location = new System.Drawing.Point(203, 3);
			this.textBoxCamera.Name = "textBoxCamera";
			this.textBoxCamera.Size = new System.Drawing.Size(10, 20);
			this.textBoxCamera.TabIndex = 120;
			this.textBoxCamera.Text = "1";
			this.textBoxCamera.Visible = false;
			// 
			// textBoxXOffset
			// 
			this.textBoxXOffset.Enabled = false;
			this.textBoxXOffset.Location = new System.Drawing.Point(78, 108);
			this.textBoxXOffset.Name = "textBoxXOffset";
			this.textBoxXOffset.ReadOnly = true;
			this.textBoxXOffset.Size = new System.Drawing.Size(44, 20);
			this.textBoxXOffset.TabIndex = 127;
			// 
			// textBoxTemperature
			// 
			this.textBoxTemperature.Enabled = false;
			this.textBoxTemperature.Location = new System.Drawing.Point(215, 252);
			this.textBoxTemperature.Name = "textBoxTemperature";
			this.textBoxTemperature.Size = new System.Drawing.Size(26, 20);
			this.textBoxTemperature.TabIndex = 156;
			this.textBoxTemperature.Text = "-60";
			// 
			// textBoxYOffset
			// 
			this.textBoxYOffset.Enabled = false;
			this.textBoxYOffset.Location = new System.Drawing.Point(190, 108);
			this.textBoxYOffset.Name = "textBoxYOffset";
			this.textBoxYOffset.ReadOnly = true;
			this.textBoxYOffset.Size = new System.Drawing.Size(44, 20);
			this.textBoxYOffset.TabIndex = 129;
			// 
			// labelTemperature
			// 
			this.labelTemperature.Location = new System.Drawing.Point(149, 255);
			this.labelTemperature.Name = "labelTemperature";
			this.labelTemperature.Size = new System.Drawing.Size(67, 16);
			this.labelTemperature.TabIndex = 155;
			this.labelTemperature.Text = "Temperature";
			// 
			// labelGain
			// 
			this.labelGain.Enabled = false;
			this.labelGain.Location = new System.Drawing.Point(130, 64);
			this.labelGain.Name = "labelGain";
			this.labelGain.Size = new System.Drawing.Size(56, 16);
			this.labelGain.TabIndex = 143;
			this.labelGain.Text = "Gain (%)";
			// 
			// labelCamera
			// 
			this.labelCamera.Location = new System.Drawing.Point(212, 6);
			this.labelCamera.Name = "labelCamera";
			this.labelCamera.Size = new System.Drawing.Size(10, 20);
			this.labelCamera.TabIndex = 119;
			this.labelCamera.Text = "Camera";
			this.labelCamera.Visible = false;
			// 
			// textBoxSubX
			// 
			this.textBoxSubX.Enabled = false;
			this.textBoxSubX.Location = new System.Drawing.Point(78, 132);
			this.textBoxSubX.Name = "textBoxSubX";
			this.textBoxSubX.ReadOnly = true;
			this.textBoxSubX.Size = new System.Drawing.Size(44, 20);
			this.textBoxSubX.TabIndex = 131;
			// 
			// labelExposure
			// 
			this.labelExposure.Enabled = false;
			this.labelExposure.Location = new System.Drawing.Point(14, 63);
			this.labelExposure.Name = "labelExposure";
			this.labelExposure.Size = new System.Drawing.Size(60, 20);
			this.labelExposure.TabIndex = 141;
			this.labelExposure.Text = "Expo. (ms)";
			// 
			// buttonConnect
			// 
			this.buttonConnect.Enabled = false;
			this.buttonConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonConnect.Location = new System.Drawing.Point(246, 18);
			this.buttonConnect.Name = "buttonConnect";
			this.buttonConnect.Size = new System.Drawing.Size(76, 27);
			this.buttonConnect.TabIndex = 121;
			this.buttonConnect.Text = "Connect";
			this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
			// 
			// textBoxSubY
			// 
			this.textBoxSubY.Enabled = false;
			this.textBoxSubY.Location = new System.Drawing.Point(190, 132);
			this.textBoxSubY.Name = "textBoxSubY";
			this.textBoxSubY.ReadOnly = true;
			this.textBoxSubY.Size = new System.Drawing.Size(44, 20);
			this.textBoxSubY.TabIndex = 133;
			// 
			// labelCooling
			// 
			this.labelCooling.Location = new System.Drawing.Point(149, 229);
			this.labelCooling.Name = "labelCooling";
			this.labelCooling.Size = new System.Drawing.Size(60, 16);
			this.labelCooling.TabIndex = 153;
			this.labelCooling.Text = "Cooling";
			// 
			// buttonUpdate
			// 
			this.buttonUpdate.Enabled = false;
			this.buttonUpdate.Location = new System.Drawing.Point(246, 148);
			this.buttonUpdate.Name = "buttonUpdate";
			this.buttonUpdate.Size = new System.Drawing.Size(75, 23);
			this.buttonUpdate.TabIndex = 140;
			this.buttonUpdate.Text = "Update";
			this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
			// 
			// labelWidth
			// 
			this.labelWidth.Enabled = false;
			this.labelWidth.Location = new System.Drawing.Point(14, 88);
			this.labelWidth.Name = "labelWidth";
			this.labelWidth.Size = new System.Drawing.Size(56, 16);
			this.labelWidth.TabIndex = 122;
			this.labelWidth.Text = "Width";
			// 
			// radioButton8Bits
			// 
			this.radioButton8Bits.Location = new System.Drawing.Point(17, 155);
			this.radioButton8Bits.Name = "radioButton8Bits";
			this.radioButton8Bits.Size = new System.Drawing.Size(64, 24);
			this.radioButton8Bits.TabIndex = 134;
			this.radioButton8Bits.Text = "PNG";
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.Enabled = false;
			this.buttonRefresh.Location = new System.Drawing.Point(246, 116);
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
			this.buttonRefresh.TabIndex = 139;
			this.buttonRefresh.Text = "Refresh";
			this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
			// 
			// labelHeight
			// 
			this.labelHeight.Enabled = false;
			this.labelHeight.Location = new System.Drawing.Point(130, 88);
			this.labelHeight.Name = "labelHeight";
			this.labelHeight.Size = new System.Drawing.Size(56, 16);
			this.labelHeight.TabIndex = 124;
			this.labelHeight.Text = "Height";
			// 
			// textBoxExposure
			// 
			this.textBoxExposure.Enabled = false;
			this.textBoxExposure.Location = new System.Drawing.Point(78, 60);
			this.textBoxExposure.Name = "textBoxExposure";
			this.textBoxExposure.Size = new System.Drawing.Size(44, 20);
			this.textBoxExposure.TabIndex = 142;
			// 
			// checkBoxBinning
			// 
			this.checkBoxBinning.Enabled = false;
			this.checkBoxBinning.Location = new System.Drawing.Point(171, 155);
			this.checkBoxBinning.Name = "checkBoxBinning";
			this.checkBoxBinning.Size = new System.Drawing.Size(64, 24);
			this.checkBoxBinning.TabIndex = 138;
			this.checkBoxBinning.Text = "Binning";
			this.checkBoxBinning.CheckedChanged += new System.EventHandler(this.checkBoxBinning_CheckedChanged);
			// 
			// labelXOffset
			// 
			this.labelXOffset.Enabled = false;
			this.labelXOffset.Location = new System.Drawing.Point(14, 112);
			this.labelXOffset.Name = "labelXOffset";
			this.labelXOffset.Size = new System.Drawing.Size(56, 16);
			this.labelXOffset.TabIndex = 126;
			this.labelXOffset.Text = "X Offset";
			// 
			// buttonSnapshot
			// 
			this.buttonSnapshot.Enabled = false;
			this.buttonSnapshot.Location = new System.Drawing.Point(246, 84);
			this.buttonSnapshot.Name = "buttonSnapshot";
			this.buttonSnapshot.Size = new System.Drawing.Size(75, 23);
			this.buttonSnapshot.TabIndex = 137;
			this.buttonSnapshot.Text = "Snapshot";
			this.buttonSnapshot.Click += new System.EventHandler(this.buttonSnapshot_Click);
			// 
			// labelYOffset
			// 
			this.labelYOffset.Enabled = false;
			this.labelYOffset.Location = new System.Drawing.Point(130, 112);
			this.labelYOffset.Name = "labelYOffset";
			this.labelYOffset.Size = new System.Drawing.Size(56, 16);
			this.labelYOffset.TabIndex = 128;
			this.labelYOffset.Text = "Y Offset";
			// 
			// buttonPreview
			// 
			this.buttonPreview.Location = new System.Drawing.Point(246, 52);
			this.buttonPreview.Name = "buttonPreview";
			this.buttonPreview.Size = new System.Drawing.Size(75, 23);
			this.buttonPreview.TabIndex = 136;
			this.buttonPreview.Text = "Preview";
			this.buttonPreview.Click += new System.EventHandler(this.buttonPreview_Click);
			// 
			// labelSubX
			// 
			this.labelSubX.Enabled = false;
			this.labelSubX.Location = new System.Drawing.Point(14, 136);
			this.labelSubX.Name = "labelSubX";
			this.labelSubX.Size = new System.Drawing.Size(56, 16);
			this.labelSubX.TabIndex = 130;
			this.labelSubX.Text = "Sub/Bin X";
			// 
			// buttonVideoProps
			// 
			this.buttonVideoProps.Location = new System.Drawing.Point(14, 315);
			this.buttonVideoProps.Name = "buttonVideoProps";
			this.buttonVideoProps.Size = new System.Drawing.Size(156, 23);
			this.buttonVideoProps.TabIndex = 147;
			this.buttonVideoProps.Text = "Advanced camera options...";
			this.buttonVideoProps.Click += new System.EventHandler(this.buttonVideoProps_Click_1);
			// 
			// radioButton16Bits
			// 
			this.radioButton16Bits.Checked = true;
			this.radioButton16Bits.Location = new System.Drawing.Point(91, 155);
			this.radioButton16Bits.Name = "radioButton16Bits";
			this.radioButton16Bits.Size = new System.Drawing.Size(74, 24);
			this.radioButton16Bits.TabIndex = 135;
			this.radioButton16Bits.TabStop = true;
			this.radioButton16Bits.Text = "TIFF";
			this.radioButton16Bits.CheckedChanged += new System.EventHandler(this.radioButton16Bits_CheckedChanged);
			// 
			// labelSubY
			// 
			this.labelSubY.Enabled = false;
			this.labelSubY.Location = new System.Drawing.Point(130, 136);
			this.labelSubY.Name = "labelSubY";
			this.labelSubY.Size = new System.Drawing.Size(56, 16);
			this.labelSubY.TabIndex = 132;
			this.labelSubY.Text = "Sub/Bin Y";
			// 
			// checkBoxCompress
			// 
			this.checkBoxCompress.Location = new System.Drawing.Point(17, 177);
			this.checkBoxCompress.Name = "checkBoxCompress";
			this.checkBoxCompress.Size = new System.Drawing.Size(148, 24);
			this.checkBoxCompress.TabIndex = 182;
			this.checkBoxCompress.Text = "TIFF compression (ZIP)";
			this.checkBoxCompress.CheckedChanged += new System.EventHandler(this.checkBoxCompress_CheckedChanged);
			// 
			// timerServerControl
			// 
			this.timerServerControl.Interval = 1000;
			this.timerServerControl.Tick += new System.EventHandler(this.timerServerControl_Tick);
			// 
			// FormMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(339, 575);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "#Cam";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.groupBoxEM.ResumeLayout(false);
			this.groupBoxEM.PerformLayout();
			this.groupBoxShutter.ResumeLayout(false);
			this.groupBoxShutter.PerformLayout();
			this.groupBoxShifting.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tempBar)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		
		[STAThread]

		#endregion

        

        //--------------------------------------//


		#region General functions

		private void buttonPreview_Click(object sender, System.EventArgs e)
		{
			CameraPreview();
		}

		private void buttonRefresh_Click(object sender, System.EventArgs e)
		{
			CameraRefresh();
		}

		private void buttonConnect_Click(object sender, System.EventArgs e)
		{
			CameraConnect();
		}

		private void buttonUpdate_Click(object sender, System.EventArgs e)
		{
			CameraUpdate();
		}

		private void buttonSnapshot_Click(object sender, System.EventArgs e) // Not implemented yet for pixelfly, only call of CamWare while closing recording here
		{
			CameraSnapshot();
		}

		private void buttonVideoProps_Click(object sender, System.EventArgs e) // Not implemented yet for pixelfly 
		{
			// LucamCamera.DisplayPropertyPage(hCamera, Handle.ToInt32());
		}

		#endregion

		//--------------------------------------//


		#region Components callbacks


		private void Form1_Load(object sender, System.EventArgs e)
		{
			if (false)
				this.Close();
		}

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (Connected && camera.Get("Temperature") < 0 && MessageBox.Show("Are you sure you want to close the application?", "Temperature not above zero degree!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
				e.Cancel = true;
			else formClosed = true;
		}

		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form settingsForm = new Settings();
			settingsForm.ShowDialog();
		}

		private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			//System.Collections.ArrayList arraylist = new System.Collections.ArrayList(axisBox.Items);
			//Properties.Settings.Default.axisBoxList = arraylist;
			Properties.Settings.Default.Save();
		}

		private void fluoPixCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.fluoPix = fluoPixCheckbox.Checked;
		}

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cameraSelectorBox.SelectedIndex != -1)
                enableCamera.Enabled = true;
        }

		private void enableCamera_CheckedChanged(object sender, EventArgs e)
		{
			if (enableCamera.Checked && !camera_connected)
            {

                Refresh();
                //Properties.Settings.Default.enabPix = this.enablePixelfly.Checked;


                initIsDone = false;
                Thread initProcessThread = new Thread(new ThreadStart(initCameras));
                initProcessThread.IsBackground = true;
                initProcessThread.Start();
                pleaseWaitTimer.Start();
                pleaseWaitDialog.ShowDialog();
            }
			else if (!enableCamera.Checked && camera_connected)
            {
				if (MessageBox.Show("This will terminate all current and pending operations. Continue?", "Shutdown Camera", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
					disposeCameras();
				else enableCamera.Checked = true; //if dialog == no, reset chekbox to check, change nothing
            }
		}

		void pleaseWaitTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (initIsDone)
				pleaseWaitDialog.Close();
			else
				pleaseWaitTimer.Start();
		}

        private void tempTimer_Elapsed(object sender, EventArgs e)
        {
			if (!pictureBeingTaken)
			{
				this.tempBar.Value = camera.Get("Temperature");
				if (camera.Descr.TempStatus == 0) labelTemp.Text = "Off";				//Status of temperature while cooling: 0 off; 1 reached but not stabilized; 2 reached and stabilized; 3 not reached
				else if (camera.Descr.TempStatus == 1) labelTemp.Text = "Stabilizing";
				else if (camera.Descr.TempStatus == 2) labelTemp.Text = "Steady";
				else if (camera.Descr.TempStatus == 3) labelTemp.Text = "Cooling"; 
			}
        }


        private void tempBar_MouseDown(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Current CCD temperature is "+Convert.ToString(tempBar.Value)+"°C.", "CCD Temperature", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

		private void enableDoubleImage_CheckedChanged(object sender, EventArgs e)
		{
			ToggleDoubleImage();
		}

		private void checkBoxBinning_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxBinning.Checked)
			{
				textBoxSubX.ReadOnly = false;
				textBoxSubY.ReadOnly = false;
			}
			else
			{
				textBoxSubX.ReadOnly = true;
				textBoxSubY.ReadOnly = true;
			}

		}

		private void radioButton16Bits_CheckedChanged(object sender, EventArgs e)
		{
			tiff = !tiff;
			png = !png;
		}

		private void labelCoolingSwitch_Click(object sender, EventArgs e)
		{
			if (camera.Descr.isAdvanced)
			{
				cooling = !cooling;

				if (cooling)
				{
					try
					{
						camera.GetLastError();
						camera.Set("Temperature", Convert.ToInt32(textBoxTemperature.Text));
						if (camera.GetLastError() > 0) throw new Exception("Camera error.");
						camera.Set("Cooling", 1);
						if (camera.GetLastError() > 0) throw new Exception("Camera error.");

						labelCoolingSwitch.ForeColor = System.Drawing.Color.Green;
						labelCoolingSwitch.Text = "   ■  On";
					}
					catch (Exception)
					{
						cooling = false;
						MessageBox.Show("Temperature could not be set.");
					}
				}
				else
				{
					camera.GetLastError();
					camera.Set("Cooling", 0);
					if (camera.GetLastError() == 0)
					{
						labelCoolingSwitch.ForeColor = System.Drawing.Color.Red;
						labelCoolingSwitch.Text = "■     Off";
					}
					else
					{
						cooling = true;
						MessageBox.Show("Cooling could not be switched off.");
					}
				} 
			}
		}

		private void buttonVideoProps_Click_1(object sender, EventArgs e)
		{
			if (!videoProps) toggleAdvanced(true);
			else toggleAdvanced(false);
		}

		private void checkBoxCompress_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxCompress.Checked) tiffCompress = true;
			else tiffCompress = false;
		}

		private void ADChannelBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			loadAndorAdvancedSettings("HSS");	//HSS settings in Andor depend on selected AD Channel.
			Properties.Settings.Default.channelID = ADChannelBox.SelectedIndex;
		}

		private void axisBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.axisID = axisBox.SelectedIndex;
		}

		private void timerServerControl_Tick(object sender, EventArgs e)
		{
			if (!serverThread.IsAlive && !abortServer)	//restart Server if crashed
			{
				serverThread = new Thread(new ThreadStart(serverEntryPoint));
				serverThread.Start();			//ServerMain.cs
			}

			if (!remotingThread.IsAlive && !abortServer)	//restart Server if crashed
			{
				remotingThread = new Thread(new ThreadStart(remotingEntryPoint));
				remotingThread.Start();			//ServerMain.cs
			}
		}

		private void checkBoxEM_CheckedChanged(object sender, EventArgs e)
		{
			ToggleEM();
		}

		#endregion
	}



}



