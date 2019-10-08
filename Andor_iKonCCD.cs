using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace QCam
{
    unsafe class Andor_iKonCCD : ICamera
    {
        #region DLL import
        [DllImport("atmcd32d.dll", EntryPoint = "Initialize",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _Initialize(string dir);

        [DllImport("atmcd32d.dll", EntryPoint = "GetDetector",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetDetector(ref int xpixel, ref int ypixel);

        [DllImport("atmcd32d.dll", EntryPoint = "GetCapabilities",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetCapabilities(ref AndorCapabilities caps);

        [DllImport("atmcd32d.dll", EntryPoint = "GetHeadModel",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetHeadModel(StringBuilder model);

        [DllImport("atmcd32d.dll", EntryPoint = "CoolerON",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _CoolerOn();

        [DllImport("atmcd32d.dll", EntryPoint = "CoolerOFF",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _CoolerOff();

		[DllImport("atmcd32d.dll", EntryPoint = "GetTemperatureRange",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetTemperatureRange(ref int minTemp, ref int maxTemp);

        [DllImport("atmcd32d.dll", EntryPoint = "GetTemperature",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetTemperature(ref int temperature);

        [DllImport("atmcd32d.dll", EntryPoint = "SetTemperature",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetTemperature(int temperature);

		[DllImport("atmcd32d.dll", EntryPoint = "SetImage",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetImage(int hbin, int vbin, int hstart, int hend, int vstart, int vend);

		[DllImport("atmcd32d.dll", EntryPoint = "GetNumberPreAmpGains",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetNumberPreAmpGains(ref int noGains);

		[DllImport("atmcd32d.dll", EntryPoint = "GetPreAmpGain",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetPreAmpGain(int index, ref float gain);

		[DllImport("atmcd32d.dll", EntryPoint = "SetPreAmpGain",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetPreAmpGain(int index);

        [DllImport("atmcd32d.dll", EntryPoint = "SetAcquisitionMode",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetAcquisitionMode(int mode);

        [DllImport("atmcd32d.dll", EntryPoint = "SetReadMode",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetReadMode(int mode);

		[DllImport("atmcd32d.dll", EntryPoint = "GetNumberVSSpeeds",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetNumberVSSpeeds(ref int speeds);

		[DllImport("atmcd32d.dll", EntryPoint = "GetVSSpeed",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetVSSpeed(int index, ref float speed);

        [DllImport("atmcd32d.dll", EntryPoint = "GetFastestRecommendedVSSpeed",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetFastestRecommendedVSSpeed(ref int speedIndex, ref float speed);

        [DllImport("atmcd32d.dll", EntryPoint = "SetVSSpeed",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetVSSpeed(int speedIndex);

        [DllImport("atmcd32d.dll", EntryPoint = "GetNumberADChannels",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetNumberADChannels(ref int channels);  //Check later if needed, do we even have more than one AD for iKon-M? -- Yes, possible.

        [DllImport("atmcd32d.dll", EntryPoint = "SetADChannel",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetADChannel(int channel);               //dito

        [DllImport("atmcd32d.dll", EntryPoint = "GetNumberHSSpeeds",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetNumberHSSpeeds(int channels, int ampOut, ref int numberSpeeds);

        [DllImport("atmcd32d.dll", EntryPoint = "GetHSSpeed",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetHSSpeed(int channels, int ampOut, int speedIndex, ref float speed);

        [DllImport("atmcd32d.dll", EntryPoint = "SetHSSpeed",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetHSSpeed(int ampOut, int speedIndex);

        [DllImport("atmcd32d.dll", EntryPoint = "GetAcquisitionTimings",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetAcquisitionTimings(ref float exposure, ref float accumulate, ref float kinetic);

        [DllImport("atmcd32d.dll", EntryPoint = "SetExposureTime",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetExposureTime(float time);    //in seconds

		[DllImport("atmcd32d.dll", EntryPoint = "SetNumberKinetics",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetNumberKinetics(int number); 

        [DllImport("atmcd32d.dll", EntryPoint = "SetTriggerMode",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetTriggerMode(int mode);    

        [DllImport("atmcd32d.dll", EntryPoint = "SetFastKinetics",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetFastKinetics(int exposedRows, int seriesLength, float expTime, int binMode, int hbin, int vbin);

        [DllImport("atmcd32d.dll", EntryPoint = "SetFastKineticsEx",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _SetFastKineticsEx(int exposedRows, int seriesLength, float expTime, int binMode, int hbin, int vbin, int offset);

		[DllImport("atmcd32d.dll", EntryPoint = "GetNumberFKVShiftSpeeds",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetNumberFKVShiftSpeeds(ref int number);

		[DllImport("atmcd32d.dll", EntryPoint = "GetFKVShiftSpeedF",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetFKVShiftSpeedF(int index, ref float speed);

		[DllImport("atmcd32d.dll", EntryPoint = "SetFKVShiftSpeed",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetFKVShiftSpeed(int index);

        [DllImport("atmcd32d.dll", EntryPoint = "GetFKExposureTime",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetFKExposureTime(ref float expTime);

		[DllImport("atmcd32d.dll", EntryPoint = "SetVSAmplitude",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetVSAmplitude(int state);

		[DllImport("atmcd32d.dll", EntryPoint = "SetShutter",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetShutter(int typ, int mode, int closingtime, int openingtime);

        [DllImport("atmcd32d.dll", EntryPoint = "StartAcquisition",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _StartAcquisition();

		[DllImport("atmcd32d.dll", EntryPoint = "AbortAcquisition",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _AbortAcquisition();

        [DllImport("atmcd32d.dll", EntryPoint = "GetStatus",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetStatus(ref int status);

		[DllImport("atmcd32d.dll", EntryPoint = "GetNumberAvailableImages",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetNumberAvailableImages(ref int first, ref int last);

        [DllImport("atmcd32d.dll", EntryPoint = "GetAcquiredData16",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetAcquiredData16(ushort* buffArray, UInt32 size);

		[DllImport("atmcd32d.dll", EntryPoint = "GetImages16",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetImages16(int first, int last, ushort* buffArray, UInt32 size, ref int validFirst, ref int validLast);

		[DllImport("atmcd32d.dll", EntryPoint = "FreeInternalMemory",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _FreeInternalMemory();

        [DllImport("atmcd32d.dll", EntryPoint = "ShutDown",
           ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _ShutDown();

		[DllImport("atmcd32d.dll", EntryPoint = "SendSoftwareTrigger",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SendSoftwareTrigger();

		[DllImport("atmcd32d.dll", EntryPoint = "GetNumberAmp",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetNumberAmp(ref int amp);

		[DllImport("atmcd32d.dll", EntryPoint = "GetAmpDesc",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetAmpDesc(int index,  byte[] name, int len);

        //[DllImport(@"MyDll.dll", EntryPoint = "MyFunction", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        //public static extern int MyFunction([MarshalAsAttribute(UnmanagedType.LPWStr)] string upnPtr, int upnSize, [MarshalAsAttribute(UnmanagedType.LPWStr)] StringBuilder guidPtr, ref int guidSizePtr);
        //
        //as used from Def
        //long MyFunction(WCHAR* upn, long upnSize, WCHAR* guid, long* guidSize);
        //
        //call:
        //string upn = "foo@bar.com";
        //var guidSB = new StringBuilder(128);  //The capacity parameter (128) defines the maximum number of characters that can be stored in the memory allocated by the current instance.
        //int guidSizePtr =guidSB.Capacity;
        //MyFunction(upn, upn.Length, guidSB, ref guidSizePtr);
        //
        //from Andor Doc
        //#include “stdlib.h”
        //char szStr[512];
        //AT_WC wcszStr[512];
        //mbstowcs(wcszStr, szStr, 512);
        #endregion

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct AndorCapabilities
        {
            public uint ulSize;
            public uint ulAcqModes;
            public uint ulReadModes;
            public uint ulTriggerModes;
            public uint ulCameraType;
            public uint ulPixelMode;
            public uint ulSetFunctions;
            public uint ulGetFunctions;
            public uint ulFeatures;
            public uint ulPCICard;
            public uint ulEMGainCapability;
            public uint ulFTReadModes;
        }


        private void ReportError(uint err, string source)
        {
            string errStr = "";
            if (err == SUCCESS)
            {
                if (FormMain.Form1.ShowLog) Console.WriteLine(source + " : OK ");
            }
            else
            {
                Andor_iKonCCDGetErrorTextClass.Andor_GetErrorText(err, ref errStr);
                Console.WriteLine(source + " error : " + errStr);
				System.IO.Directory.CreateDirectory(@"C:\PCOLogs\");
				System.IO.File.WriteAllText(@"C:\PCOLogs\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "error.txt", source + " error : " + errStr);
            }
        }

        private const int READMODE = 4;	//Image Mode
		private const int DOUBLEIMAGE = 2;
		private const int SUCCESS = 20002;	//Error code for success

		private string path;	//exe directory, should hold driver/ini files for Andor

        private int width, height, offsetX, offsetY;
        private int hbin = 1, vbin = 1;
        private int mode;
		private int amp = 1;
		private int noInSeries = 2;		//No. of cycles
		private int currentInSeries = 1;
		private int noSingleImages;
		private int imageDouble;
        private int gainMode;
		private int shutterMode, shutterOutput, sttOpen, sttClose;
		private int FKVSS, HSS, VSS, ADChannel, VSAmplitude;

        private int expLevel = 100; //not implemented
        private int bitDepth = 12;  //dito 
        private int shift = 0;      //dito

        private int size;

		private IntPtr image_buf;

		private uint err = SUCCESS;

        private AndorCapabilities andorCaps;
        private Description descr;

		private Object codeLock = new Object();

        public Andor_iKonCCD(string p)
        {
			path = p;
            andorCaps.ulSize = (uint)sizeof(AndorCapabilities);
            Console.WriteLine("--> Class constructed; \"-->\": class call.\n\n");
            Console.WriteLine("--> Andor iKon-M Supported Modes:\n");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("| 00 = internal trigger          |");
            Console.WriteLine("| 01 = external trigger          |");
            Console.WriteLine("| 06 = external start            |");
            Console.WriteLine("| 07 = external exposure         |");
            Console.WriteLine("| 10 = software trigger          |");
            Console.WriteLine("| 12 = external charge shifting  |");    //Only available with certain iKon-M systems; as found in bit 7 of andorCaps.ulTriggerModes (1 = yes, 0 = no)
            Console.WriteLine("----------------------------------\n");
        }

        public unsafe Description Descr
        {
            get { return descr; }
        }


        public unsafe uint Open()
        {
			lock (codeLock)
			{
				int maxWidth = 0;
				int maxHeight = 0;
				int minTemp = 0;
				int maxTemp = 0;

				Console.Write("--> Initializing (this may take a few seconds) ...");
				err = _Initialize(path);
				ReportError(err, "--> Initialize");
				Console.Write("50%...");
				err = _GetCapabilities(ref andorCaps);
				ReportError(err, "--> Get Caps");
				Console.Write("66%...");
				err = _GetDetector(ref maxWidth, ref maxHeight);
				ReportError(err, "--> Get CCD");
				Console.Write("83%...");
				err = _GetTemperatureRange(ref minTemp, ref maxTemp);
				Console.Write("100%\n");

				descr.name = "ANDOR "+Andor_GetCameraType.Get(andorCaps.ulCameraType);
				descr.isAdvanced = true;
				descr.MinTemp = (short)minTemp;
				descr.MaxTemp = (short)maxTemp;
				descr.MaxHorzRes = (ushort)maxWidth;
				descr.MaxVertRes = (ushort)maxHeight;
				descr.DoubleImage = (ushort)(andorCaps.ulAcqModes >> 5 & 0x01);	//0x10 --> 00....0100000 (bit 5) in ulAcqModes = 1 => Fast Kinetics supported.

				Console.WriteLine("--> MaxWidth: " + Convert.ToString(descr.MaxHorzRes));
				Console.WriteLine("--> MaxHeight: " + Convert.ToString(descr.MaxVertRes));

				byte[] name = new byte [21];
				_GetNumberAmp(ref amp);

				for (int i = 0; i < amp; i++)
				{
					_GetAmpDesc(0, name, 21);
					Console.WriteLine("--> Amp: " + Encoding.ASCII.GetString(name));
				}
				amp = 1;		//Set amp to in-class representation of PreAmpGain, which is 1.

				width = descr.MaxHorzRes;
				height = descr.MaxVertRes;

				//Head Model
				//StringBuilder output = new StringBuilder(1024);
				//_GetHeadModel(output);
				//Console.WriteLine(output.ToString());

				err = _SetReadMode(READMODE);
				ReportError(err, "--> Set Readmode");

				float dummy = 0;
				err = _GetFastestRecommendedVSSpeed(ref VSS, ref dummy);
				ReportError(err, "--> Get Recommended VSS");
				err = _SetVSSpeed(VSS);
				ReportError(err, "--> Set VSS");

				FKVSS = VSS;
				err = _SetFKVShiftSpeed(FKVSS);
				ReportError(err, "--> Set FKVSS");

				err = _SetADChannel(ADChannel);
				ReportError(err, "--> Set ADChannel");

				err = _SetHSSpeed(0, HSS);
				ReportError(err, "--> Set HSS");

				VSAmplitude = 0; //Default value for SetVSAmplitude, so it doesn't need to be called here

				Console.WriteLine("--> Opening Shutter ...");
				shutterMode = 1;
				err = _SetShutter(0, shutterMode, 0, 0);
				ReportError(err, "--> Set Shutter");

				return 0; 
			}
        }

        public unsafe void Close()
        {
			lock (codeLock)
			{
				err = _AbortAcquisition();						//fix: checked if needed
				//ReportError(err, "--> AbortAcquisition");
				err = _ShutDown();
				ReportError(err, "--> Shutdown camera");
			}
        }

        public unsafe void SetReady(bool doubleImage, short picNr)
        {
			lock (codeLock)
			{
				size = width * height;	//size in pixel for one image

				if (doubleImage)
				{
					noInSeries = picNr;
					noSingleImages = 1;	// two images on CCD ("double image")
					imageDouble = 2;
					size *= 2; // if double image mode is on, the image's size will be twice the normal one...
				}
				else
				{
					noInSeries = 1;
					noSingleImages = picNr;
					imageDouble = 1;
					err = _SetNumberKinetics(picNr);
					ReportError(err, "--> Set No. Kinetics");
				}

				//Size in byte for (picNr) 16-bit images
				size = 2 * size * picNr;

				Console.WriteLine("--> Allocating " + size.ToString() + " bytes of buffer ...");
				image_buf = Marshal.AllocHGlobal(size);

				Console.WriteLine("--> Prepare Andor ...");

				currentInSeries = 1;
				err = _StartAcquisition();
				ReportError(err, "--> Start Acquisition");

				Console.WriteLine("--> Start recording ...");
			}
        }

        public unsafe void SetFinished(short picNr)
        {
			lock (codeLock)
			{
				err = _AbortAcquisition();					//fix: checked if needed
				//ReportError(err, "--> AbortAcquisition");
				Console.WriteLine("--> Free buffer ...");
				Marshal.FreeHGlobal(image_buf); 
			}
		}



        public unsafe bool WaitOnImage(short img)
        {
			lock (codeLock)
			{
				int status = 0;

				err = _GetStatus(ref status);

				if (status != 0x4E68 && status != 0x4E69)
				{
					ReportError((uint)status, "--> Acquiring");
					err = 1;
				}
				else if (status == 0x4E69 && currentInSeries < noInSeries)
				{
					ushort* p = ((ushort*)(image_buf) + (currentInSeries - 1) * 2 * width * height);// create pointer at buffer[0], buffer[2*imagesize], buffer[4*imagesize], ... Mind the pointer arithmetics automatically multiplies with the stride of ushort i.e. 16bit/2byte, so after casting image_buf as ushort, imagesize*4 would be wrong
					err = _GetAcquiredData16(p, (uint)(width * height * 2));
					ReportError(err, "--> Transfering Image (" + Convert.ToString(currentInSeries) + ")");

					currentInSeries += 1;
					err = _StartAcquisition();
					ReportError(err, "--> Start Acquisition (" + Convert.ToString(currentInSeries) + ")");

					err = _GetStatus(ref status);
				}
				//ReportError((uint)status, "--> Waiting");
				return (status == 0x4E68);	//0x4E68 == Acquiring, ! --> wait
			}
        }

        public unsafe void CancelImages(short picNr)
        {
            throw new NotImplementedException();
        }


       
        //GetSet

        public unsafe uint Set(string name, int value)
        {
			lock (codeLock)
			{
				uint _err = 0;

				if (name == "µsExposure")
				{
					float fvalue = (float)value / 1000000;

					if (mode == 4) err = _SetFastKineticsEx(height, DOUBLEIMAGE, fvalue, READMODE, hbin, vbin, offsetY);	//READMODE 4 = Image, compare ReadOutMode
					else err = _SetExposureTime(fvalue);
				}
				else if (name == "DoubleImageMode")
				{
					err = _SetAcquisitionMode(value + 3);	//0 = normal, 1 = doubleImage ==> 3 = Kinetic Series, 4 = FastKineticSeries
					if (err == SUCCESS) mode = value + 3;
				}
				else if (name == "Height")
					height = value;
				else if (name == "OffsetX")
					offsetX = value;
				else if (name == "OffsetY")
					offsetY = value;
				else if (name == "TriggerMode")
					err = _SetTriggerMode(value == 1 ? 10 : 1);		//Triggermode 1 software, 2 extern ==> 10 software, 1 external
				else if (name == "GainMode")
				{
					err = _SetPreAmpGain(value);
					if (err == SUCCESS) gainMode = value;
				}
				else if (name == "Temperature")
					err = _SetTemperature(value);
				else if (name == "Cooling")
				{
					if (value == 1) err = _CoolerOn();
					else err = _CoolerOff();
				}
				else if (name == "FKVSS")
				{
					err = _SetFKVShiftSpeed(value);
					if (err == SUCCESS) FKVSS = value;
				}
				else if (name == "HSS")
				{
					err = _SetHSSpeed(0, value);
					if (err == SUCCESS) HSS = value;
				}
				else if (name == "VSS")
				{
					err = _SetVSSpeed(value);
					if (err == SUCCESS) VSS = value;
				}
				else if (name == "ADChannel")
				{
					err = _SetADChannel(value);
					if (err == SUCCESS) ADChannel = value;
				}
				else if (name == "VClockVoltage")
				{
					err = _SetVSAmplitude(value);
					if (err == SUCCESS) VSAmplitude = value;
				}
				else if (name == "ShutterMode")
				{
					err = _SetShutter(shutterOutput, value, sttClose, sttOpen);
					if (err == SUCCESS) shutterMode = value;
				}
				else if (name == "ShutterOutput")
				{
					err = _SetShutter(value, shutterMode, sttClose, sttOpen);
					if (err == SUCCESS) shutterOutput = value;
				}
				else _err = 1;

				ReportError(err, "--> Set" + name);
				return _err; 
			}
        }

		public uint Set(string name1, ushort value1, string name2, ushort value2)
		{
			lock (codeLock)
			{
				uint _err = 0;

				if (name1 == "BinHorz" && name2 == "BinVert")
				{
					if (mode == 3)							//for mode = 4 (FastKinetics) this will be set with _SetFastKinetics (currently during set exposure)
					{
						err = _SetImage(value1, value2, offsetX + 1, width, offsetY + 1, height);
						ReportError(err, "--> SetImage");
					}
					else err = SUCCESS;
					if (err == SUCCESS)
					{
						hbin = value1;
						vbin = value2; 
					}
				}
				else if (name2 == "BinHorz" && name1 == "BinVert")
				{
					if (mode == 3)
					{
						err = _SetImage(value2, value1, offsetX + 1, width, offsetY + 1, height);
						ReportError(err, "--> SetImage");
					}
					else err = SUCCESS;
					if (err == SUCCESS)
					{
						hbin = value2;
						vbin = value1; 
					}
				}
				else if (name1 == "STTOpen" && name2 == "STTClose")
				{
					err = _SetShutter(shutterOutput, shutterMode, value2, value1);
					ReportError(err, "--> ShutterOutput");
					if (err == SUCCESS)
					{
						sttOpen = value1;
						sttClose = value2;
					}
				}
				else if (name2 == "STTOpen" && name1 == "STTClose")
				{
					err = _SetShutter(shutterOutput, shutterMode, value1, value2);
					ReportError(err, "--> ShutterOutput");
					if (err == SUCCESS)
					{
						sttOpen = value2;
						sttClose = value1;
					}
				}
				else _err = 1;

				return _err;
			}
		}

        public unsafe short Get(string name)
        {
			lock (codeLock)
			{
				int wValue = 0;
				uint ret;

				if (name == "Temperature")
				{
					ret = _GetTemperature(ref wValue);
					if (ret > 20033 && ret < 20038)
					{
						err = SUCCESS;
						descr.TempStatus = (ushort)(ret - 20034);
					}
					else err = ret;
				}
				else if (name == "DoubleImageMode")
				{
					wValue = mode - 3;
				}
				else if (name == "GainMode")
				{
					wValue = gainMode;
				}
				else if (name == "Amp")
				{
					wValue = amp;
				}
				else if (name == "Width")
				{
					wValue = width;
				}
				else if (name == "Height")
				{
					wValue = height;
				}
				else if (name == "OffsetX")
				{
					wValue = offsetX;
				}
				else if (name == "OffsetY")
				{
					wValue = offsetY;
				}
				else if (name == "BinHorz")
				{
					wValue = hbin;
				}
				else if (name == "BinVert")
				{
					wValue = vbin;
				}
				else if (name == "noFKVSS")				//Fix (Remove)
				{
					_GetNumberFKVShiftSpeeds(ref wValue);
				}
				else if (name == "noADChannels")		//Fix (Remove)
				{
					_GetNumberADChannels(ref wValue);
				}
				else if (name == "noHSS")				//Fix (Remove)
				{
					_GetNumberHSSpeeds(0, 0, ref wValue);
				}
				else if (name == "FKVSS")
				{
					wValue = FKVSS;
				}
				else if (name == "HSS")
				{
					wValue = HSS;
				}
				else if (name == "VSS")
				{
					wValue = VSS;
				}
				else if (name == "ADChannel")
				{
					wValue = ADChannel;
				}
				else if (name == "VClockVoltage")
				{
					wValue = VSAmplitude;
				}
				else if (name == "ShutterMode")
				{
					wValue = shutterMode;
				}
				else if (name == "ShutterOutput")
				{
					wValue = shutterOutput;
				}

				return (short)(wValue); 
			}
        }

		public ushort[] ArrGet(string name)
		{
			lock (codeLock)
			{
				ushort[] wValue;
				int number = 0;

				if (name == "ConversionFactors")
				{
					err = _GetNumberPreAmpGains(ref number);
					ReportError(err, "--> Get PreAmpGain");
					if (number == 0) wValue = new ushort[1] { 100 };
					else
					{
						wValue = new ushort[number];
						float value = 0;

						for (int i = 0; i < number; i++)
						{
							_GetPreAmpGain(i, ref value);
							wValue[i] = (ushort)(value * 100);
						}			
					}
				}
				else if (name == "ADChannels")
				{
					err = _GetNumberADChannels(ref number);
					ReportError(err, "--> Get ADChannels");
					if (number == 0) wValue = new ushort[1] { 0 };
					else
					{
						wValue = new ushort[number];

						for (int i = 0; i < number; i++)
						{
							wValue[i] = (ushort)i;
						}
					}
				}
				else if (name == "FKVSS")
				{
					err = _GetNumberFKVShiftSpeeds(ref number);
					if (number == 0) wValue = new ushort[1] { 0 };
					else
					{
						wValue = new ushort[number];
						float value = 0;

						for (int i = 0; i < number; i++)
						{
							err = _GetFKVShiftSpeedF(i, ref value);
							wValue[i] = (ushort)(value * 1000);	//retuns in ns/pixelshift
						}
					}
					ReportError(err, "--> Get FKVSS");
				}
				else if (name == "HSS")
				{
					err = _GetNumberHSSpeeds(ADChannel, 0, ref number);
					ReportError(err, "--> Get HSS");
					if (number == 0) wValue = new ushort[1] { 0 };
					else
					{
						wValue = new ushort[number];
						float value = 0;

						for (int i = 0; i < number; i++)
						{
							_GetHSSpeed(ADChannel, 0, i, ref value);
							wValue[i] = (ushort)(value * 1000);	//retuns in kHz/pixelshift
						}
					}
				}
				else if (name == "VSS")
				{
					err = _GetNumberVSSpeeds(ref number);
					ReportError(err, "--> Get VSS");
					if (number == 0) wValue = new ushort[1] { 0 };
					else
					{
						wValue = new ushort[number];
						float value = 0;

						for (int i = 0; i < number; i++)
						{
							_GetVSSpeed(i, ref value);
							wValue[i] = (ushort)(value * 1000);	//retuns in ns/pixelshift
						}
					}
				}
				else wValue = new ushort[1] { 0 };

				return wValue; 
			}
		}

		public unsafe float FGet(string name)
		{
			lock (codeLock)
			{
				Single result = 0;
				Single dummy1 = 0, dummy2 = 0;

				if (name == "Exposure")
				{
					if (mode == 4) err = _GetFKExposureTime(ref result);	//return in s
					else err = _GetAcquisitionTimings(ref result, ref dummy1, ref dummy2);
					result *= 1000;							//into ms
				}

				ReportError(err, "--> Get" + name);
				return result;
			}
		}

        public unsafe ushort* GetImage(int img)
        {
			lock (codeLock)
			{
				int first = 0, last = 0;
				err = _GetNumberAvailableImages(ref first, ref last);
				ReportError(err, "--> NumberAvailableImages (index first:" + Convert.ToString(first) + " index last:" + Convert.ToString(last) + ")");

				if (img == noInSeries)
				{
					ushort* p = ((ushort*)(image_buf) + (img - 1) * 2 * width * height);	// create pointer at last two-pic-block, buffer[noInSeries*imagesize], ... Mind the pointer arithmetics automatically multiplies with the stride of ushort i.e. 16bit/2byte, so after casting image_buf as ushort, noInSeries*imagesize*2 would be wrong
					err = _GetAcquiredData16(p, (uint)(noSingleImages * imageDouble * width * height));
					ReportError(err, "--> Transfering Image (" + Convert.ToString(img) + ")");
				}

				return ((ushort*)(image_buf) + (img - 1) * imageDouble * width * height);	//Return in two chunks; image 1 &2, image 3 & 4
			}
        }

        public unsafe uint GetLastError()
        {
			lock (codeLock)
			{
				UInt32 _err = (uint)Math.Abs(err - SUCCESS);
				err = SUCCESS;
				return _err;
			}
        }


        public unsafe uint ForceTrigger()
        {
			lock (codeLock)
			{
				Console.WriteLine("--> Force Trigger ...\n");
				err = _SendSoftwareTrigger();
				ReportError(err, "--> ForceTrigger");
				return err; ; 
			}
        }
    }
}
