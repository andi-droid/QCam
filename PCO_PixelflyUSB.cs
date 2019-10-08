using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime;

using System.Runtime.InteropServices;
using System.Diagnostics;

using System.Runtime.ExceptionServices; //for catching accessviolations in try catch


namespace QCam
{
	unsafe class PixelflyUSB : ICamera
	{
		#region DLL import
		[DllImport("sc2_cam.dll", EntryPoint = "PCO_OpenCamera",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _OpenCamera(ref int pHandle, UInt16 wCamNum);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_CloseCamera",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _CloseCamera(int pHandle);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_GetCameraDescription",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetCameraDescription(int pHandle, ref PCO_Description strDescription);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_AllocateBuffer",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _AllocateBuffer(int pHandle, ref short sBufNr, int size, ref UInt16* wBuf, ref int hEvent);
		
		//HANDLE ph,SHORT* sBufNr,DWORD size,WORD** wBuf,HANDLE *hEvent

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_FreeBuffer",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _FreeBuffer(int pHandle, short sBufNr);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_RemoveBuffer",
			ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _RemoveBuffer(int pHandle);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_CancelImages",
			ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _CancelImages(int pHandle);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_ArmCamera",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _ArmCamera(int pHandle);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_CamLinkSetImageParameters",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _CamLinkSetImageParameters(int pHandle, UInt16 wXRes, UInt16 wYRes);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_SetRecordingState",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetRecordingState(int pHandle, UInt16 wRecState);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_SetDoubleImageMode",
		  ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetDoubleImageMode(int pHandle, UInt16 wDoubleImage);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_GetDoubleImageMode",
		  ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetDoubleImageMode(int pHandle, ref UInt16 wDoubleImage);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_AddBuffer",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _AddBuffer(int pHandle, UInt32 dwFirstImage, UInt32 dwLastImage, short sBufNr);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_AddBufferEx",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _AddBufferEx(int pHandle, UInt32 dwFirstImage, UInt32 dwLastImage, short sBufNr, UInt16 wXRes, UInt16 wYRes, UInt16 wBitPerPixel);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_GetBufferStatus",
		   ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetBufferStatus(int pHandle, short sBufNr, ref UInt32 dwStatusDll, ref UInt32 dwStatusDrv);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_SetTriggerMode",
		  ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetTriggerMode(int pHandle, UInt16 wTriggerMode);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_GetTriggerMode",
			ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetTriggerMode(int pHandle, ref UInt16 wTriggerMode);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_SetDelayExposureTime",
			ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetDelayExposureTime(int pHandle, UInt32 dwDelay, UInt32 dwExposure, UInt16 wTimeBaseDelay, UInt16 wTimeBaseExposure);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_GetDelayExposureTime",
		 ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetDelayExposureTime(int pHandle, ref UInt32 dwDelay, ref UInt32 dwExposure, ref UInt16 wTimeBaseDelay, ref UInt16 wTimeBaseExposure);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_GetConversionFactor",
		 ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetConversionFactor(int pHandle, ref UInt16 wConvFact);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_SetConversionFactor",
		 ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetConversionFactor(int pHandle, UInt16 wConvFact);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_SetBinning",
		 ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetBinning(int pHandle, UInt16 wBinHorz, UInt16 wBinVert);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_GetBinning",
		 ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _GetBinning(int pHandle, ref UInt16 wBinHorz, ref UInt16 wBinVert);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_ForceTrigger",
			ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _ForceTrigger(int pHandle, ref UInt16 wTriggered);

        [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetROI",
            ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetROI(int pHandle, ref UInt16 wRoiX0, ref UInt16 wRoiY0, ref UInt16 wRoiX1, ref UInt16 wRoiY1);

        [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetTemperature",
            ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        private static extern uint _GetTemperature(int pHandle, ref short sCCDTemp, ref short sCamTemp, ref short sPowTemp);

		[DllImport("sc2_cam.dll", EntryPoint = "PCO_SetBitAlignment",
			ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
		private static extern uint _SetBitAlignment(int pHandle, UInt16 wBitAlignment);
		
		#endregion

		[StructLayout(LayoutKind.Sequential)]
		private struct PCO_Description
		{
			public ushort wSize;                   // Sizeof this struct
			public ushort wSensorTypeDESC;         // Sensor type
			public ushort wSensorSubTypeDESC;      // Sensor subtype
			public ushort wMaxHorzResStdDESC;      // Maxmimum horz. resolution in std.mode
			public ushort wMaxVertResStdDESC;      // Maxmimum vert. resolution in std.mode
			public ushort wMaxHorzResExtDESC;      // Maxmimum horz. resolution in ext.mode
			public ushort wMaxVertResExtDESC;      // Maxmimum vert. resolution in ext.mode
			public ushort wDynResDESC;             // Dynamic resolution of ADC in bit
			public ushort wMaxBinHorzDESC;         // Maxmimum horz. binning
			public ushort wBinHorzSteppingDESC;    // Horz. bin. stepping (0:bin, 1:lin)
			public ushort wMaxBinVertDESC;         // Maxmimum vert. binning
			public ushort wBinVertSteppingDESC;    // Vert. bin. stepping (0:bin, 1:lin)
			public ushort wRoiHorStepsDESC;        // Minimum granularity of ROI in pixels
			public ushort wRoiVertStepsDESC;       // Minimum granularity of ROI in pixels
			public ushort wNumADCsDESC;            // Number of ADCs in system
			public ushort ZZwAlignDummy1;
			public uint dwPixelRateDESC1;      // Possible pixelrate in Hz
			public uint dwPixelRateDESC2;      // Possible pixelrate in Hz
			public uint dwPixelRateDESC3;      // Possible pixelrate in Hz
			public uint dwPixelRateDESC4;      // Possible pixelrate in Hz
			public uint ZZdwDummypr1, ZZdwDummypr2, ZZdwDummypr3, ZZdwDummypr4, ZZdwDummypr5;
			public uint ZZdwDummypr21, ZZdwDummypr22, ZZdwDummypr23, ZZdwDummypr24, ZZdwDummypr25;
			public uint ZZdwDummypr31, ZZdwDummypr32, ZZdwDummypr33, ZZdwDummypr34, ZZdwDummypr35;
			public uint ZZdwDummypr41, ZZdwDummypr42, ZZdwDummypr43, ZZdwDummypr44, ZZdwDummypr45;
			public ushort wConvFactDESC1;        // Possible conversion factor in e/cnt
			public ushort wConvFactDESC2;        // Possible conversion factor in e/cnt
			public ushort wConvFactDESC3;        // Possible conversion factor in e/cnt
			public ushort wConvFactDESC4;        // Possible conversion factor in e/cnt
			public ushort ZZdwDummycv1, ZZdwDummycv2, ZZdwDummycv3, ZZdwDummycv4, ZZdwDummycv5;
			public ushort ZZdwDummycv21, ZZdwDummycv22, ZZdwDummycv23, ZZdwDummycv24, ZZdwDummycv25;
			public ushort ZZdwDummycv31, ZZdwDummycv32, ZZdwDummycv33, ZZdwDummycv34, ZZdwDummycv35;
			public ushort ZZdwDummycv41, ZZdwDummycv42, ZZdwDummycv43, ZZdwDummycv44, ZZdwDummycv45;
			public ushort wIRDESC;                 // IR enhancment possibility
			public ushort ZZwAlignDummy2;
			public uint dwMinDelayDESC;          // Minimum delay time in ns
			public uint dwMaxDelayDESC;          // Maximum delay time in ms
			public uint dwMinDelayStepDESC;      // Minimum stepping of delay time in ns
			public uint dwMinExposureDESC;       // Minimum exposure time in ns
			public uint dwMaxExposureDESC;       // Maximum exposure time in ms
			public uint dwMinExposureStepDESC;   // Minimum stepping of exposure time in ns
			public uint dwMinDelayIRDESC;        // Minimum delay time in ns
			public uint dwMaxDelayIRDESC;        // Maximum delay time in ms
			public uint dwMinExposureIRDESC;     // Minimum exposure time in ns
			public uint dwMaxExposureIRDESC;     // Maximum exposure time in ms
			public ushort wTimeTableDESC;          // Timetable for exp/del possibility
			public ushort wDoubleImageDESC;        // Double image mode possibility
			public short sMinCoolSetDESC;         // Minimum value for cooling
			public short sMaxCoolSetDESC;         // Maximum value for cooling
			public short sDefaultCoolSetDESC;     // Default value for cooling
			public ushort wPowerDownModeDESC;      // Power down mode possibility 
			public ushort wOffsetRegulationDESC;   // Offset regulation possibility
			public ushort wColorPatternDESC;       // Color pattern of color chip
			// four nibbles (0,1,2,3) in ushort 
			//  ----------------- 
			//  | 3 | 2 | 1 | 0 |
			//  ----------------- 
			//   
			// describe row,column  2,2 2,1 1,2 1,1
			// 
			//   column1 column2
			//  ----------------- 
			//  |       |       |
			//  |   0   |   1   |   row1
			//  |       |       |
			//  -----------------
			//  |       |       |
			//  |   2   |   3   |   row2
			//  |       |       |
			//  -----------------
			// 
			public ushort wPatternTypeDESC;        // Pattern type of color chip
			// 0: Bayer pattern RGB
			// 1: Bayer pattern CMY
			public ushort wDSNUCorrectionModeDESC; // DSNU correction mode possibility
			public ushort ZZwAlignDummy3;          //
			public uint dwReservedDESC1, dwReservedDESC2, dwReservedDESC3, dwReservedDESC4;
			public uint dwReservedDESC5, dwReservedDESC6, dwReservedDESC7, dwReservedDESC8;
			public uint ZZdwDummy1, ZZdwDummy2, ZZdwDummy3, ZZdwDummy4, ZZdwDummy5;
			public uint ZZdwDummy21, ZZdwDummy22, ZZdwDummy23, ZZdwDummy24, ZZdwDummy25;
			public uint ZZdwDummy31, ZZdwDummy32, ZZdwDummy33, ZZdwDummy34, ZZdwDummy35;
			public uint ZZdwDummy41, ZZdwDummy42, ZZdwDummy43, ZZdwDummy44, ZZdwDummy45;
			public uint ZZdwDummy51, ZZdwDummy52, ZZdwDummy53, ZZdwDummy54, ZZdwDummy55;
			public uint ZZdwDummy61, ZZdwDummy62, ZZdwDummy63, ZZdwDummy64, ZZdwDummy65;
			public uint ZZdwDummy71, ZZdwDummy72, ZZdwDummy73, ZZdwDummy74, ZZdwDummy75;
			public uint ZZdwDummy81, ZZdwDummy82, ZZdwDummy83, ZZdwDummy84, ZZdwDummy85;
		} 
		
		private void ReportError(uint err, string source)
		{
			string errStr = "";
			if (err > 0)
			{
				descr.RunError = 1;
				PCO_USBGetErrorTextClass.PCO_GetErrorText(err, ref errStr);
				//Debug.Write(source + " error : " + errStr + "\n");
				//MessageBox.Show(source + " error : " + errStr);
				Console.WriteLine(source + " error : " + errStr);
				System.IO.Directory.CreateDirectory(@"C:\PCOLogs\");
				System.IO.File.WriteAllText(@"C:\PCOLogs\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "error.txt", source + " error : " + errStr);
			}
            else if (FormMain.Form1.ShowLog)
			{
				//Debug.Write(source + " : OK \n");
				Console.WriteLine(source + " : OK ");
				//MessageBox.Show(source + " : OK ");
			}
		}

		private int hCamera;	//Cam handle

		private ushort width, height;   //NOT actually implemented!!! (Camera is used with max. resolution)
		private int size;
		private int ishift;

		private short[] bufnr;
		private UInt16*[] buf;

		private int[] evhandle;

		private uint err = 0;

		private PCO_Description pcoDescr;
		private Description descr;
		private System.Collections.Generic.List<ushort> gainModes = new System.Collections.Generic.List<ushort>();

		private Object codeLock = new Object();

		//public

		public PixelflyUSB()//Konstruktor
		{
			buf = new UInt16*[8];//max. 8 Buffers, 0..7
			bufnr = new short[8];
			evhandle = new int[8];
			pcoDescr.wSize = (ushort)sizeof(PCO_Description);
            Console.WriteLine("--> Class constructed; \"-->\": class call.\n\n");
            Console.WriteLine("--> Pixelfly USB Supported Modes:\n");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("| 1 = software trigger          |");
            Console.WriteLine("| 2 = software & extern trigger |");
            Console.WriteLine("---------------------------------\n");
		}

		public Description Descr
		{
			get { return descr; }
		}

		public void SetReady(Boolean doubleImage, short picNr)
		{
			lock (codeLock)
			{
				ushort actualHeight = height;	//IMPORTANT!!!! Otherwise, height will not be resetted for DoubleImage *2.
				//width = descr.MaxHorzRes;
				//height = descr.MaxVertRes;

				size = width * height;
				//Console.WriteLine("--> Width: " + Convert.ToString(width));
				//Console.WriteLine("--> Height: " + Convert.ToString(height));

				if (doubleImage)
				{
					size *= 2; // if double image mode is on, the image's size will be twice the normal one...
					actualHeight *= 2; // idem 
				}

				Console.WriteLine("--> Allocate buffer(s) for " + Convert.ToString(size) + " byte ...");

				for (int i = 0; i < picNr; i++)
				{
					evhandle[i] = 0;
					bufnr[i] = -1;
					buf[i] = null;
					err = _AllocateBuffer(hCamera, ref bufnr[i], 2 * size, ref buf[i], ref evhandle[i]);
					ReportError(err, "--> AllocateBuffer " + i.ToString());
				}
				err = _CamLinkSetImageParameters(hCamera, width, actualHeight); //Mandatory for Cameralink and GigE. Don't care for all other interfaces, so leave it intact here.

				// Arm Camera and Start recording :

				err = _ArmCamera(hCamera);
				ReportError(err, "--> ArmCamera");

				err = _SetRecordingState(hCamera, 1);
				ReportError(err, "--> SetRecordingStateON");


				// Adding buffers :

				for (int i = 0; i < picNr; i++)
				{
					err = _AddBuffer(hCamera, 0, 0, bufnr[i]);
					//err = _AddBufferEx(hCamera, 0, 0, bufnr[i], width, actualHeight, 14);
					ReportError(err, "--> Addbuffer " + i.ToString());
				}
			}
		}
			
		public Boolean WaitOnImage(short img)
		{
			lock (codeLock)
			{
				UInt32 dwStatusDll = 0;
				UInt32 dwStatusDrv = 0;
				_GetBufferStatus(hCamera, (short)(img - 1), ref dwStatusDll, ref dwStatusDrv);
				err = dwStatusDrv;
				ReportError(err, "--> WaitOnImage");

				return ((dwStatusDll & 0x8000) == 0);//checks "buffer event is set" (code 0x8000). //return !(dwStatusDll == 0x8000) -- i.e. Wait == True 
			}
		}
			
		public UInt16* GetImage(int img)
		{
			lock (codeLock)
			{
				return buf[img - 1]; 
			}
		}

		public void SetFinished(short picNr)
		{
			lock (codeLock)
			{
				_CancelImages(hCamera);
				_SetRecordingState(hCamera, 0);
				ReportError(err, "--> SetRecordingStateOFF");
				err = _CancelImages(hCamera);
				ReportError(err, "--> CancelImages");

				for (int i = 0; i < picNr; i++)
					err = _FreeBuffer(hCamera, bufnr[i]);

				ReportError(err, "--> FreeBuffers"); 
			}
		}

		public UInt32 GetLastError()
		{
			lock (codeLock)
			{
				UInt32 _err = err;
				err = 0;
				return _err; 
			}
		}



		public uint Open()
		{
			lock (codeLock)
			{
				uint _err = 0;

				descr.RunError = 0;

				err = _OpenCamera(ref hCamera, 0);

				ReportError(err, "--> Open Camera");
				if (hCamera == 0) _err = 1;
				Console.WriteLine("--> Handle received: " + Convert.ToString(hCamera));

				err = _GetCameraDescription(hCamera, ref pcoDescr);
				ReportError(err, "--> Cam Description");

				descr.name = "PIXELFLY USB";
				descr.isAdvanced = false;
				descr.MaxHorzRes = pcoDescr.wMaxHorzResStdDESC;
				descr.MaxVertRes = pcoDescr.wMaxVertResStdDESC;
				descr.DoubleImage = pcoDescr.wDoubleImageDESC;

				Console.WriteLine("--> MaxWidth: " + Convert.ToString(pcoDescr.wMaxHorzResStdDESC));
				Console.WriteLine("--> MaxHeight: " + Convert.ToString(pcoDescr.wMaxVertResStdDESC));

				width = pcoDescr.wMaxHorzResStdDESC;
				height = pcoDescr.wMaxVertResStdDESC;
				ishift = 16 - pcoDescr.wDynResDESC;   // Dynamic resolution of ADC in bit

				_SetBitAlignment(hCamera, 0x0001);	//LSB Alignment

				//GainModes

				if (!(pcoDescr.wConvFactDESC1 == 0))
					gainModes.Add(pcoDescr.wConvFactDESC1);
				if (!(pcoDescr.wConvFactDESC2 == 0))
					gainModes.Add(pcoDescr.wConvFactDESC2);
				if (!(pcoDescr.wConvFactDESC3 == 0))
					gainModes.Add(pcoDescr.wConvFactDESC3);
				if (!(pcoDescr.wConvFactDESC4 == 0))
					gainModes.Add(pcoDescr.wConvFactDESC4);

				_SetRecordingState(hCamera, 0);
				ReportError(err, "--> SetRecordingStateOFF");

				return _err; 
			}
		}

		public void Close()
		{
			lock (codeLock)
			{
				gainModes.Clear();
				err = _SetRecordingState(hCamera, 0);
				ReportError(err, "--> SetRecordingStateOFF");
				err = _CancelImages(hCamera);
				ReportError(err, "--> CancelImages");
				err = _CloseCamera(hCamera);
				ReportError(err, "--> Close Camera"); 
			}
		}



		//public uint Set(string name, ushort value)
		//{
		//	uint _err = 0;

		//	if (name == "DoubleImageMode")
		//		err = _SetDoubleImageMode(hCamera, value);
		//	else if (name == "TriggerMode")
		//		err = _SetTriggerMode(hCamera, value);
		//	else if (name == "ConversionFactor")
		//		err = _SetConversionFactor(hCamera, value);
		//	else _err = 1;

		//	ReportError(err, "--> Set"+name);

		//	return _err;
		//}

		public uint Set(string name, Int32 value)
		{
			lock (codeLock)
			{
				uint _err = 0;

				if (name == "µsExposure")
					err = _SetDelayExposureTime(hCamera, 0, (uint)value, 0, 1);
				else if (name == "DoubleImageMode")
					err = _SetDoubleImageMode(hCamera, (ushort)value);
				else if (name == "TriggerMode")
					err = _SetTriggerMode(hCamera, (ushort)value);
				else if (name == "ConversionMode")
					err = _SetConversionFactor(hCamera, gainModes[value]);
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
					err = _SetBinning(hCamera, value1, value2);
				else if (name2 == "BinHorz" && name1 == "BinVert")
					err = _SetBinning(hCamera, value2, value1);
				else _err = 1;

				ReportError(err, "--> Set" + name1 + name2);
				return _err; 
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public short Get(string name)
		{
			lock (codeLock)
			{
				UInt16 wValue1 = 0;
				UInt16 wValue2 = 0;

				UInt16 unused1 = 0;
				UInt16 unused2 = 0;

				short wValue = 0;
				short unused = 0;

				uint _err = 0;

				if (name == "Temperature")
				{
					try
					{
						_err = _GetTemperature(hCamera, ref unused, ref wValue, ref unused);
						//ReportError(err, "--> Get" + name);
						if (wValue >= 0) wValue1 = (ushort)wValue;
					}

					catch (Exception e)
					{
						System.IO.File.WriteAllText(@"C:\PCOLogs\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "temperature.txt", e.ToString());
					}
				}
				else if (name == "DoubleImageMode")
				{
					err = _GetDoubleImageMode(hCamera, ref wValue1);
					ReportError(err, "--> Get" + name);
				}
				else if (name == "GainMode")
				{
					err = _GetConversionFactor(hCamera, ref wValue1);
					ReportError(err, "--> Get" + name);

					if (wValue1 == 0) ;											//if wValue1 = 0 convention is no gain settings existed, in which case gain is always set to the first (and only) index.
					else if (wValue1 == pcoDescr.wConvFactDESC1) wValue1 = 0;
					else if (wValue1 == pcoDescr.wConvFactDESC2) wValue1 = 1;
					else if (wValue1 == pcoDescr.wConvFactDESC3) wValue1 = 2;
					else if (wValue1 == pcoDescr.wConvFactDESC4) wValue1 = 3;
				}
				else if (name == "Width")
				{
					err = _GetROI(hCamera, ref wValue1, ref unused1, ref wValue2, ref unused2);
					wValue1 = (ushort)(1 + wValue2 - wValue1);
					ReportError(err, "--> Get" + name);
				}
				else if (name == "Height")
				{
					err = _GetROI(hCamera, ref unused1, ref wValue1, ref unused2, ref wValue2);
					wValue1 = (ushort)(1 + wValue2 - wValue1);
					ReportError(err, "--> Get" + name);
				}
				else if (name == "BinHorz")
				{
					err = _GetBinning(hCamera, ref wValue1, ref wValue2);
					ReportError(err, "--> Get" + name);
				}
				else if (name == "BinVert")
				{
					err = _GetBinning(hCamera, ref wValue2, ref wValue1);
					ReportError(err, "--> Get" + name);
				}				

				return (short)wValue1; 
			}
		}

		public ushort[] ArrGet(string name)
		{
			lock (codeLock)
			{
				ushort[] wValue;

				if (name == "ConversionFactors")
				{
					int number = gainModes.Count;

					if (number == 0) wValue = new ushort[1] { 100 };
					else
					{
						wValue = new ushort[number];

						for (int i = 0; i < number; i++)
							wValue[i] = gainModes[i];
					}
				}
				else wValue = new ushort[1] { 0 };

				return wValue; 
			}
		}

		public float FGet(string name)
		{
			lock (codeLock)
			{
				UInt32 w32Value1 = 0;
				UInt32 w32Value2 = 0;
				UInt16 w16Value1 = 0;
				UInt16 w16Value2 = 0;
				Single result = 0;

				if (name == "Exposure")
				{
					err = _GetDelayExposureTime(hCamera, ref w32Value1, ref w32Value2, ref w16Value1, ref w16Value2);	//handle, delay, exposure, BaseDelay, BaseExposure; in that order

					if (w16Value2 == 2)					//Timebase is millesecond
						result = (float)w32Value2;
					else if (w16Value2 == 1)			//Timebase is microsecond
						result = w32Value2 / (float)1000;
					else if (w16Value2 == 0)			//Timebase is nanosecond
						result = w32Value2 / (float)1000000;
				}

				if (name == "Delay")
				{
					err = _GetDelayExposureTime(hCamera, ref w32Value1, ref w32Value2, ref w16Value1, ref w16Value2);	//handle, delay, exposure, BaseDelay, BaseExposure; in that order

					if (w16Value1 == 2)					//Timebase is millesecond
						result = (float)w32Value1;
					else if (w16Value1 == 1)			//Timebase is microsecond
						result = w32Value1 / (float)1000;
					else if (w16Value1 == 0)			//Timebase is nanosecond
						result = w32Value1 / (float)1000000;
				}

				ReportError(err, "--> Get" + name);
				return result; 
			}
		}

		public uint ForceTrigger()
		{
			lock (codeLock)
			{
				UInt16 wTrig = 0;
				Console.WriteLine("--> Force Trigger ...\n");

				//err = _GetRecorderSubmode(hCamera, ref wTrig);
				//ReportError(err, "--> Submode");
				//Console.WriteLine("--> Buffer SubMode: "+wTrig);

				//wTrig = 0;
				err = _ForceTrigger(hCamera, ref wTrig);
				ReportError(err, "--> ForceTrigger");
				if (wTrig == 0)
				{
					Console.WriteLine("--> Trigger error\n");
					err = 1;
				}
				return err; 
			}
		}

		//rest, unused(?)
        public void CancelImages(short picNr)
		{
			lock (codeLock)
			{
				_CancelImages(hCamera); 
			}
		}

		#region old functions

		/*
		public uint GetCameraDescription(ref PCO_Description strDescription)
		{
			return _GetCameraDescription(hCamera, ref strDescription);
		}

		public uint GetDoubleImageMode(ref UInt16 wDoubleImage)
		{
			return _GetDoubleImageMode(hCamera, ref wDoubleImage);
		}

		public uint GetDelayExposureTime(ref UInt32 dwDelay, ref UInt32 dwExposure, ref UInt16 wTimeBaseDelay, ref UInt16 wTimeBaseExposure)
		{
			return _GetDelayExposureTime(hCamera, ref dwDelay, ref dwExposure, ref wTimeBaseDelay, ref wTimeBaseExposure);
		}

		public uint SetDelayExposureTime(UInt32 dwDelay, UInt32 dwExposure, UInt16 wTimeBaseDelay, UInt16 wTimeBaseExposure)
		{
			return _SetDelayExposureTime(hCamera, dwDelay, dwExposure, wTimeBaseDelay, wTimeBaseExposure);
		}

		public uint GetConversionFactor(ref UInt16 wConvFact)
		{
			return _GetConversionFactor(hCamera, ref wConvFact);
		}

		public uint GetBinning(ref UInt16 wBinHorz, ref UInt16 wBinVert)
		{
			return _GetBinning(hCamera, ref wBinHorz, ref wBinVert);
		}
*/

		//public uint SetBinning(UInt16 wBinHorz, UInt16 wBinVert)
		//{
		//	return _SetBinning(hCamera, wBinHorz, wBinVert);
		//}

		//public uint AllocateBuffer(ref short sBufNr, int size, ref UInt16* wBuf, ref int hEvent)
		//{
		//	return _AllocateBuffer(hCamera, ref sBufNr, size, ref wBuf, ref hEvent);
		//}

		//public uint SetConversionFactor(UInt16 wConvFact)
		//{
		//	return _SetConversionFactor(hCamera, wConvFact);
		//}


		//public uint FreeBuffer(short sBufNr)
		//{
		//	return _FreeBuffer(hCamera, sBufNr);
		//}

		//public uint Record()
		//{
		//	uint err = 0;
		//
		//	err = _ArmCamera(hCamera);
		//	if (err < 0) return err;
		//	else return _SetRecordingState(hCamera, 1);
		//}

		//public uint QueueBuffer(short sBufNr)
		//{
		//	return _AddBuffer(hCamera, 0, 0, sBufNr);
		//}

		//public uint WaitForBuffer(short sBufNr, ref UInt32 dwStatusDll, ref UInt32 dwStatusDrv)
		//{
		//	return _GetBufferStatus(hCamera, sBufNr, ref dwStatusDll, ref dwStatusDll);
		//}



		//public uint SetDoubleImageMode(UInt16 wDoubleImage)
		//{
		//	return _SetDoubleImageMode(hCamera, wDoubleImage);
		//}

		//public uint GetTriggerMode(ref UInt16 wTriggerMode)
		//{
		//	return _GetTriggerMode(hCamera, ref wTriggerMode);
		//}

		//public uint SetTriggerMode(UInt16 wTriggerMode)
		//{
		//	return _SetTriggerMode(hCamera, wTriggerMode);
		//}

		//public uint CamLinkSetImageParamters (UInt16 wXRes, UInt16 wYRes)
		//{
		//	return _CamLinkSetImageParameters(hCamera, wXRes, wYRes);
		//}
		#endregion
	}
}
