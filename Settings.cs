using System.ComponentModel;
using System.Collections.Generic;

namespace QCam.Properties
{

    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

		//[Category("Network Configuration")]
		//[Description("IP address of the viewer")]
		//[global::System.Configuration.UserScopedSettingAttribute()]
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		//[global::System.Configuration.DefaultSettingValueAttribute("tassilli")]
		//public string viewerAddress
		//{
		//	get
		//	{
		//		return ((string)(this["viewerAddress"]));
		//	}
		//	set
		//	{
		//		this["viewerAddress"] = value;
		//	}
		//}

        [Category("Network Configuration")]
        [Description("Port on which Cicero communicates")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("12121")]
        public int pcPort
        {
            get
            {
                return ((int)(this["pcPort"]));
            }
            set
            {
                this["pcPort"] = value;
            }
        }

		[Category("Network Configuration")]
		[Description("Port on which remoting clients communicate")]
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("12120")]
		public int remotingPort
		{
			get
			{
				return ((int)(this["remotingPort"]));
			}
			set
			{
				this["remotingPort"] = value;
			}
		}

        [Category("Network Configuration")]
        [Description("Port on which the viewer should receive the image")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("12221")]
        public int viewerPort
        {
            get
            {
                return ((int)(this["viewerPort"]));
            }
            set
            {
                this["viewerPort"] = value;
            }
        }

        

        [Browsable(false)]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool fluoPix
        {
            get
            {
                return ((bool)(this["fluoPix"]));
            }
            set
            {
                this["fluoPix"] = value;
            }
        }

		[Browsable(false)]
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("0")]
		public int axisID
		{
			get
			{
				return ((int)(this["axisID"]));
			}
			set
			{
				this["axisID"] = value;
			}
		}

		[Browsable(false)]
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("0")]
		public int channelID
		{
			get
			{
				return ((int)(this["channelID"]));
			}
			set
			{
				this["channelID"] = value;
			}
		}

        [Browsable(false)]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public float expPix
        {
            get
            {
                return ((float)(this["expPix"]));
            }
            set
            {
                this["expPix"] = value;
            }
        }

        [Browsable(false)]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public float gainPix
        {
            get
            {
                return ((float)(this["gainPix"]));
            }
            set
            {
                this["gainPix"] = value;
            }
        }


        [Browsable(false)]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool enabPix
        {
            get
            {
                return ((bool)(this["enabPix"]));
            }
            set
            {
                this["enabPix"] = value;
            }
        }

		//[Browsable(false)]
		//[global::System.Configuration.UserScopedSettingAttribute()]
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		//[global::System.Configuration.DefaultSettingValueAttribute("")]
		//public System.Collections.ArrayList axisBoxList
		//{
		//	get
		//	{
		//		return ((System.Collections.ArrayList)(this["axisBoxList"]));
		//	}
		//	set
		//	{
		//		this["axisBoxList"] = value;
		//	}
		//}


        //[Category("Image processing settings")]
        //[Description("Decides if the images used to compute the optical density are shown and saved")]
        //[global::System.Configuration.UserScopedSettingAttribute()]
        //[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        //[global::System.Configuration.DefaultSettingValueAttribute("True")]
        //public bool showRawImages
        //{
        //    get
        //    {
        //        //return ((bool)(this["showImagesAbsFirewire"]));
        //        return ((bool)(this["showRawImages"]));
        //    }
        //    set
        //    {
        //        this["showRawImages"] = value;
        //        FormMain.Form1.ShowRaw = value;
        //    }
        //}

        //[Category("Image processing settings")]
        //[Description("Decides if the optical density is shown and saved")]
        //[global::System.Configuration.UserScopedSettingAttribute()]
        //[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        //[global::System.Configuration.DefaultSettingValueAttribute("False")]
        //public bool showOD
        //{
        //    get
        //    {
        //        //return ((bool)(this["showImagesAbsFirewire"]));
        //        return ((bool)(this["showOD"]));
        //    }
        //    set
        //    {
        //        this["showOD"] = value;
        //        FormMain.Form1.ShowOD = value;
        //    }
        //}

        [Category("Image processing settings")]
        [Description("Decides if reference picture and dark picture are subtracted (experimental build)")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool subDark
        {
            get
            {
                return ((bool)(this["subDark"]));
            }
            set
            {
                this["subDark"] = value;
                FormMain.Form1.AutoSubDark = value;
            }
        }

		[Category("Image processing settings")]
		[Description("(Only relevant if subDark enabled:) From first image, subtract dark image no. (1,2)")]
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("1")]
		public int subDarkImage1
		{
			get
			{
				return ((int)(this["subDarkImage1"]));
			}
			set
			{
				this["subDarkImage1"] = value;
				FormMain.Form1.SubDarkImg1 = value;
			}
		}

		[Category("Image processing settings")]
		[Description("(Only relevant if subDark enabled:) From second image, subtract dark image no. (1,2)")]
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("2")]
		public int subDarkImage2
		{
			get
			{
				return ((int)(this["subDarkImage2"]));
			}
			set
			{
				this["subDarkImage2"] = value;
				FormMain.Form1.SubDarkImg2 = value;
			}
		}

        [Category("Image processing settings")]
        [Description("Decides if dark picture is saved")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool saveDark
        {
            get
            {
                return ((bool)(this["saveDark"]));
            }
            set
            {
                this["saveDark"] = value;
                FormMain.Form1.SaveDark = value;
            }
        }

        [Category("Image processing settings")]
        [Description("Directory the picture will be saved in, use YYYY = year, MM = month, DD = day as templates.")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"D:\YYYY_MM_DD")]
        public string baseFolder
        {
            get
            {
                return ((string)(this["baseFolder"]));
            }
            set
            {
                this["baseFolder"] = value;
                FormMain.Form1.changeFolder(); 
            }
        }

		[Category("Image processing settings")]
		[Description("NN=CamName,YYYY=year,MM=month,DD=day,AA=axis, I...I=ID. Other letters may cause unexpected behaviour.")]
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("YYYY_MM_DD_AA_IIII")]
		public string filenameFormat
		{
			get
			{
				return ((string)(this["filenameFormat"]));
			}
			set
			{
				this["filenameFormat"] = value;
				FormMain.Form1.FilenameFormat = value;
				var templ = FormMain.Form1.generateFileString(value, null, 0);
				System.Windows.Forms.MessageBox.Show("Set to: " + templ + "_atoms" + (FormMain.Form1.Tiff ? ".tif" : ".png"));
			}
		}

		[Category("Image processing settings")]
		[Description("NN=CamName,YYYY=year,MM=month,DD=day,AA=axis, I...I=ID. Other letters may cause unexpected behaviour.")]
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("YYYY_MM_DD_AA_IIII")]
		public string protonameFormat
		{
			get
			{
				return ((string)(this["protonameFormat"]));
			}
			set
			{
				this["protonameFormat"] = value;
				FormMain.Form1.ProtonameFormat = value;
				var templ = FormMain.Form1.generateFileString(value, null, 0);
				System.Windows.Forms.MessageBox.Show("Set to: " + templ + FormMain.Form1.ProtoExt);
			}
		}
		[Category("Image processing settings")]
		[Description("Anything that comes after the format string, including extension.")]
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("_t_proto.dat")]
		public string protoExtension
		{
			get
			{
				return ((string)(this["protoExtension"]));
			}
			set
			{
				this["protoExtension"] = value;
				FormMain.Form1.ProtoExt = value;
				var templ = FormMain.Form1.generateFileString(FormMain.Form1.ProtonameFormat, null, 0);
				System.Windows.Forms.MessageBox.Show("Set to: " + templ + value);
			}
		}

        [Category("Image processing settings")]
        [Description("Decides if the internal fileID is generated by a counter (if false, will count upwards based on last existing file in dir)")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool autoCount
        {
            get
            {
                return ((bool)(this["autoCount"]));
            }
            set
            {
                this["autoCount"] = value;
                FormMain.Form1.AutoCount = value;
            }
        }

		[Category("Image processing settings")]
		[Description("List of all cameras, with corresponding axisID.")]
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("")]
		public List<CameraInfo> cameraList
		{
			get
			{
				return ((List<CameraInfo>)(this["cameraList"]));
			}
			set
			{
				this["cameraList"] = value;
			}
		}

        [Category("Logging settings")]
        [Description("Enable Console logging")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool showLog
        {
            get
            {
                //return ((bool)(this["showImagesAbsFirewire"]));
                return ((bool)(this["showLog"]));
            }
            set
            {
                this["showLog"] = value;
                FormMain.Form1.ShowLog = value;
            }
        }

        [Category("Camera Settings")]
        [Description("Sets program name")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#Cam")]
        public string programName
        {
            get
            {

				return ((string)(this["programName"]));
            }
            set
            {
				this["programName"] = value;
            }
        }

        [Category("Camera Settings")]
        [Description("Sets minimum wait time for camera trigger in seconds")]
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-5")]
        public int triggerWait
        {
            get
            {

                return ((int)(this["triggerWait"]));
            }
            set
            {
                this["triggerWait"] = value;
                FormMain.Form1.TriggerWait = value;

            }
        }

        //[Category("Camera ID")]
        //[Description("ID of the selected camera (set to 1 if only one camera is connected to the computer)")]
        //[global::System.Configuration.UserScopedSettingAttribute()]
        //[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        //[global::System.Configuration.DefaultSettingValueAttribute("1")]
        //public int cameraID
        //{
        //    get
        //    {
//
        //        return ((int)(this["cameraID"]));
        //    }
        //    set
        //    {
        //        this["cameraID"] = value;
//
        //    }
        //}
    }
}
