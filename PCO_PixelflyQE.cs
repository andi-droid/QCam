using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime;

using System.Runtime.InteropServices;   //for DllImport
using System.Diagnostics;


namespace QCam
{
    unsafe class PixelflyQE : ICamera
    {
        #region DLL import
        [DllImport("pccam.dll", EntryPoint = "INITBOARD",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)] //CallingConvention: http://stackoverflow.com/questions/2390407/pinvokestackimbalance-c-sharp-call-to-unmanaged-c-function
        private static extern int _InitBoard(int board, ref int pHandle);

        [DllImport("pccam.dll", EntryPoint = "CLOSEBOARD",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _CloseBoard(ref int pHandle);

        [DllImport("pccam.dll", EntryPoint = "GETBOARDPAR",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _GetPara(int pHandle, UInt32[] ptr, int len);

        [DllImport("pccam.dll", EntryPoint = "SETMODE",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _SetMode(int pHandle, int mode, int explevel, int exptime, int hbin, int vbin, int gain, int offset, int bit_pix, int shift);

        [DllImport("pccam.dll", EntryPoint = "GETSIZES",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _GetSizes(int pHandle, ref int ccdxsize, ref int ccdysize, ref int actualxsize, ref int actualysize, ref int bit_pix);

        [DllImport("pccam.dll", EntryPoint = "ALLOCATE_BUFFER",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _AllocateBuffer(int pHandle, ref int bufnr, ref int size);

        [DllImport("pccam.dll", EntryPoint = "FREE_BUFFER",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _FreeBuffer(int pHandle, int bufnr);

        [DllImport("pccam.dll", EntryPoint = "MAP_BUFFER",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _MapBuffer(int pHandle, int bufnr, int size, int offset, ref UInt32 linadr);

        [DllImport("pccam.dll", EntryPoint = "UNMAP_BUFFER",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _UnmapBuffer(int pHandle, int bufnr);

        [DllImport("pccam.dll", EntryPoint = "ADD_BUFFER_TO_LIST",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _AddBuffer(int pHandle, int bufnr, int size, int offset, int data);

        [DllImport("pccam.dll", EntryPoint = "REMOVE_BUFFER_FROM_LIST",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _RemoveBuffer(int pHandle, int bufnr);

        [DllImport("pccam.dll", EntryPoint = "GETBUFFER_STATUS",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _GetBufferStatus(int pHandle, int bufnr, int mode, UInt32[] ptr, int len);    //devbuf structure array[22] of UInt32, see

        [DllImport("pccam.dll", EntryPoint = "START_CAMERA",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _StartCamera(int pHandle);

        [DllImport("pccam.dll", EntryPoint = "STOP_CAMERA",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _StopCamera(int pHandle);

        [DllImport("pccam.dll", EntryPoint = "TRIGGER_CAMERA",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _TriggerCamera(int pHandle);

        [DllImport("pccam.dll", EntryPoint = "READTEMPERATURE",
           ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _ReadTemperature(int pHandle, ref int temp);

        #endregion

        private void ReportError(int err, string source)
        {
            string errStr = "";
            err = (Math.Abs(err));
            if (err > 0)
            {

                PCO_QEGetErrorTextClass.PCO_GetErrorText((uint)err, ref errStr);
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

        private int width, height;
        private int hbin, vbin;
        private int mode;
        private int expTime;
        private int gain;

        private int expLevel = 100; //not implemented
        private int bitDepth = 12;  //dito 
        private int shift = 0;      //dito

        private int size;

        private int[] bufnr;
        private UInt32[] buf;

        private int[] evhandle;

        private int err = 0;

        private uint[] BOARDVAL;
        private uint[] DEVBUF;

        private Description descr;

		private Object codeLock = new Object();

        //public

        public PixelflyQE()//Konstruktor
        {
            buf = new UInt32[8];//max. 8 Buffers, 0..7
            bufnr = new int[8];
            evhandle = new int[8];

            BOARDVAL = new uint[13];
            DEVBUF = new uint[23];

            Console.WriteLine("--> Class constructed; \"-->\": class call.\n\n");
            Console.WriteLine("--> Pixelfly QE Supported Modes:\n");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("| 0x10 = single shutter hardware trigger |");
            Console.WriteLine("| 0x11 = single shutter software trigger |");
            Console.WriteLine("| 0x20 = double shutter hardware trigger |");
            Console.WriteLine("| 0x21 = double shutter software trigger |");
            Console.WriteLine("------------------------------------------\n");
        }

        public Description Descr
        {
            get { return descr; }
        }

        public void SetReady(Boolean doubleImage, short picNr)
        {
			lock (codeLock)
			{
				size = width * height * 2;  //size in byte
				//Console.WriteLine("--> Width: " + Convert.ToString(width));
				//Console.WriteLine("--> Height: " + Convert.ToString(height));

				if (doubleImage)
				{
					size *= 2; // if double image mode is on, the image's size will be twice the normal one...  
					//height *= 2; // idem, qe doc: "CCD vertical size *2 for double option"
				}

				Console.WriteLine("--> Allocate buffer(s) for " + Convert.ToString(size) + " byte ...");

				for (int i = 0; i < picNr; i++) //allocates picNr buffers, typically 2 (one for each image)
				{
					bufnr[i] = -1;
					err = _AllocateBuffer(hCamera, ref bufnr[i], ref size); //bufnr = -1 for allocating a new buffer or buffer number to reallocate with different size; size of Buffer in byte
					ReportError(err, "--> AllocateBuffer " + i.ToString());

					err = _GetBufferStatus(hCamera, bufnr[i], 0, DEVBUF, 92);
					if (((DEVBUF[0] >> 12) & 0x0F) == 1) Console.WriteLine("Error! Buffer " + Convert.ToString(i + 1));
				}

				for (int i = 0; i < picNr; i++) //maps the buffer into userspace
				{
					buf[i] = 0;
					err = _MapBuffer(hCamera, bufnr[i], size, 0, ref buf[i]);
					ReportError(err, "--> MapBuffer " + i.ToString());
					if (FormMain.Form1.ShowLog) Console.WriteLine("Buffer mapped to address " + Convert.ToString(buf[i]));

					err = _GetBufferStatus(hCamera, bufnr[i], 0, DEVBUF, 92);
					if (((DEVBUF[0] >> 12) & 0x0F) == 1) Console.WriteLine("Error! Buffer " + Convert.ToString(i + 1));
				}

				// Arm Camera and Start recording :
				Console.WriteLine("--> Arm camera ...");

				err = _StartCamera(hCamera);
				ReportError(err, "--> SetRecordingStateON");


				err = _GetBufferStatus(hCamera, bufnr[1 - 1], 0, DEVBUF, 92);
				if (FormMain.Form1.ShowLog) Console.WriteLine("Buffer 1 Status: " + Convert.ToString(DEVBUF[0], 2));
				err = _GetBufferStatus(hCamera, bufnr[2 - 1], 0, DEVBUF, 92);
				if (FormMain.Form1.ShowLog) Console.WriteLine("Buffer 2 Status: " + Convert.ToString(DEVBUF[0], 2));

				// Adding buffers :

				for (int i = 0; i < picNr; i++)
				{
					err = _AddBuffer(hCamera, bufnr[i], size, 0, 0);
					ReportError(err, "--> Addbuffer " + i.ToString());

					//err = _GetBufferStatus(hCamera, bufnr[i], 0, DEVBUF, 92);
					//if (((DEVBUF[0] >> 12) & 0x01) == 1) Console.WriteLine("Error! Buffer " + Convert.ToString(i + 1));
					//Console.WriteLine("Buffer " + Convert.ToString(i+1) + " Status: " + Convert.ToString(DEVBUF[0], 2));
					err = _GetBufferStatus(hCamera, bufnr[1 - 1], 0, DEVBUF, 92);
					if (FormMain.Form1.ShowLog) Console.WriteLine("Buffer 1 Status: " + Convert.ToString(DEVBUF[0], 2));
					err = _GetBufferStatus(hCamera, bufnr[2 - 1], 0, DEVBUF, 92);
					if (FormMain.Form1.ShowLog) Console.WriteLine("Buffer 2 Status: " + Convert.ToString(DEVBUF[0], 2));

					err = _GetBufferStatus(hCamera, bufnr[i], 0, DEVBUF, 92);
					if (((DEVBUF[0] >> 12) & 0x0F) == 1)
					{
						Console.WriteLine("Error! Buffer " + Convert.ToString(i + 1));
						err = 1;
					}
				}

				if (FormMain.Form1.ShowLog)
				{
					_GetBufferStatus(hCamera, bufnr[1 - 1], 0, DEVBUF, 92);
					Console.WriteLine("Buffer 1 Status: " + Convert.ToString(DEVBUF[0], 2));
					_GetBufferStatus(hCamera, bufnr[2 - 1], 0, DEVBUF, 92);
					Console.WriteLine("Buffer 2 Status: " + Convert.ToString(DEVBUF[0], 2));
				} 
			}
        }

        public Boolean WaitOnImage(short img)
        {
			lock (codeLock)
			{
				UInt32 dwStatusDll = 0;
				UInt32 dwStatusDrv = 0;
				//Console.WriteLine("--> WaitOnImage");

				//DEVBUF[0] = 0;

				err = _GetBufferStatus(hCamera, bufnr[img - 1], 0, DEVBUF, 92); //size 23*4 byte for 23 elements of uint
				//ReportError(err, "--> WaitOnImage ");

				//if (((DEVBUF[0] >> 1) & 0x01) == 0) Console.WriteLine("Buffer Leer");
				//else Console.WriteLine("Buffer Voll");

				//err = _GetBufferStatus(hCamera, bufnr[1 - 1], 0, DEVBUF, 92);
				//Console.WriteLine("Buffer 1 Status: " + Convert.ToString(DEVBUF[0], 2));
				//err = _GetBufferStatus(hCamera, bufnr[2 - 1], 0, DEVBUF, 92);
				//Console.WriteLine("Buffer 2 Status: " + Convert.ToString(DEVBUF[0], 2));
				if (((DEVBUF[0] >> 12) & 0x0F) == 1)
				{
					Console.WriteLine("Error! Buffer " + Convert.ToString(img) + ": " + Convert.ToString(DEVBUF[0], 2));
					err = 1;
				}

				return (((DEVBUF[0] >> 1) & 0x01) == 0);//check documentation 
			}
        }

        public UInt16* GetImage(int img)
        {
			lock (codeLock)
			{
				Console.WriteLine("--> Get Image ...");
				return (ushort*)buf[img - 1]; 
			}
        }

        public void SetFinished(short picNr)
        {
			lock (codeLock)
			{
				//DEVBUF[0] = 0;
				if (FormMain.Form1.ShowLog)
				{
					err = _GetBufferStatus(hCamera, bufnr[1 - 1], 0, DEVBUF, 92);
					Console.WriteLine("Buffer 1 Status: " + Convert.ToString(DEVBUF[0], 2));
					err = _GetBufferStatus(hCamera, bufnr[2 - 1], 0, DEVBUF, 92);
					Console.WriteLine("Buffer 2 Status: " + Convert.ToString(DEVBUF[0], 2));
				}

				err = _StopCamera(hCamera);
				ReportError(err, "--> SetRecordingStateOFF");

				for (int i = 0; i < picNr; i++)
				{
					err = _RemoveBuffer(hCamera, bufnr[i]);
					//err = _GetBufferStatus(hCamera, bufnr[i], 0, DEVBUF, 92);
					//if (((DEVBUF[0] >> 12) & 0x01) == 1) Console.WriteLine("Error! Buffer " + Convert.ToString(i + 1));

					err = _UnmapBuffer(hCamera, bufnr[i]);
					//err = _GetBufferStatus(hCamera, bufnr[i], 0, DEVBUF, 92);
					//if (((DEVBUF[0] >> 12) & 0x01) == 1) Console.WriteLine("Error! Buffer " + Convert.ToString(i + 1));

					err = _FreeBuffer(hCamera, bufnr[i]);
					//err = _GetBufferStatus(hCamera, bufnr[i], 0, DEVBUF, 92);
					//if (((DEVBUF[0] >> 12) & 0x01) == 1) Console.WriteLine("Error! Buffer " + Convert.ToString(i + 1));
				}

				ReportError(err, "--> FreeBuffers"); 
			}
        }

        public UInt32 GetLastError()
        {
			lock (codeLock)
			{
				UInt32 _err = (UInt32)(Math.Abs(err));
				err = 0;
				return _err; 
			}
        }



        public uint Open()
        {
			lock (codeLock)
			{
				uint _err = 0;
				int maxWidth = 0;
				int maxHeight = 0;

				err = _InitBoard(0, ref hCamera);

				ReportError(err, "--> Open Camera PixelflyQE");
				if (err != 0 || hCamera == 0) _err = 1;
				Console.WriteLine("\n--> Handle received: " + Convert.ToString(hCamera));

				err = _GetPara(hCamera, BOARDVAL, 51);                  //Imports 13 x 4 = 52 byte, i.e. Boardval[12] of uint (4 byte each), produces error (-3), 51 does not
				hbin = (int)((BOARDVAL[7] >> 7) & 0x01);
				vbin = (int)(BOARDVAL[7] & 0x0F);
				mode = (int)BOARDVAL[4];
				expTime = (int)BOARDVAL[5];
				gain = (int)BOARDVAL[8];
				ReportError(err, "--> Cam Description");

				err = _GetSizes(hCamera, ref maxWidth, ref maxHeight, ref width, ref height, ref bitDepth);

				descr.name = "PIXELFLY QE";
				descr.isAdvanced = false;
				descr.MaxHorzRes = (ushort)maxWidth;
				descr.MaxVertRes = (ushort)maxHeight;
				descr.DoubleImage = (ushort)(BOARDVAL[12] & 0x01);    //DoubleImage mode supported?

				Console.WriteLine("--> MaxWidth: " + Convert.ToString(descr.MaxHorzRes));
				Console.WriteLine("--> MaxHeight: " + Convert.ToString(descr.MaxVertRes));
				Console.WriteLine("--> DefaultMode: 0x" + Convert.ToString(BOARDVAL[4], 16)); //base 16, we want the hex representation

				return _err; 
			}
        }

        public void Close()
        {
			lock (codeLock)
			{
				//fixed (int* h = &hCamera)
				err = _CloseBoard(ref hCamera);
				ReportError(err, "--> Close Camera"); 
			}
        }



		//public uint Set(string name, ushort value)
		//{
		//	uint _err = 0;

		//	if (name == "DoubleImageMode")
		//	{
		//		mode = (int)value * 0x10 + 0x10;
		//		err = _SetMode(hCamera, mode, expLevel, expTime, hbin, vbin, gain, 0, bitDepth, shift); //value 0 --> 0x10 --> DoubleImage off, value 1 --> 0x20 --> DoubleImage on (both hardware trigger)       //mode, explevel, exposure (µs/ms depending on mode), hbin, vbin, gain, offset (unused), bitDepth, shift
		//		Console.WriteLine("--> Trigger set to 0x" + mode.ToString("x"));
		//	}
		//	else if (name == "TriggerMode") //Input: 0 auto/notused, 1 software, 2 software & extern, 3 extern; from PixelflyUSB. In practice 1 = software, 2 = extern
		//	{
		//													//Console.WriteLine("TriggerCamera mode old: 0x" + Convert.ToString(mode, 16));
		//		mode = ((int)mode & 0xF0) + 2 - (int)value; //bitmask gets mode 0xi0, +2 -value (1,2): 0xi0 hardware, 0xi1 software, i = 1,2,3
		//													//Console.WriteLine("TriggerCamera mode new: 0x" + Convert.ToString(mode, 16));
		//		Console.WriteLine("--> Trigger set to 0x" + mode.ToString("x"));
		//		err = _SetMode(hCamera, mode, expLevel, expTime, hbin, vbin, gain, 0, bitDepth, shift);
		//	}         
		//	else if (name == "ConversionFactor") //Gain
		//	{
		//		gain = (int)value;
		//		err = _SetMode(hCamera, mode, expLevel, expTime, hbin, vbin, gain, 0, bitDepth, shift);
		//	}
		//	else _err = 1;

		//	ReportError(err, "--> Set " + name + " to " + Convert.ToString(value));

		//	return _err;
		//}

        public uint Set(string name, Int32 value)
        {
			lock (codeLock)
			{
				uint _err = 0;

				if (name == "µsExposure")
				{
					expTime = value;
					err = _SetMode(hCamera, mode, expLevel, value, hbin, vbin, gain, 0, bitDepth, shift);       //mode, explevel, exposure (µs/ms depending on mode), hbin, vbin, gain, offset (unused), bitDepth, shift
				}
				else if (name == "DoubleImageMode")
				{
					mode = value * 0x10 + 0x10;
					err = _SetMode(hCamera, mode, expLevel, expTime, hbin, vbin, gain, 0, bitDepth, shift); //value 0 --> 0x10 --> DoubleImage off, value 1 --> 0x20 --> DoubleImage on (both hardware trigger)       //mode, explevel, exposure (µs/ms depending on mode), hbin, vbin, gain, offset (unused), bitDepth, shift
					Console.WriteLine("--> Trigger set to 0x" + mode.ToString("x"));
				}
				else if (name == "TriggerMode") //Input: 0 auto/notused, 1 software, 2 software & extern, 3 extern; from PixelflyUSB. In practice 1 = software, 2 = extern
				{
					//Console.WriteLine("TriggerCamera mode old: 0x" + Convert.ToString(mode, 16));
					mode = (mode & 0xF0) + 2 - value; //bitmask gets mode 0xi0, +2 -value (1,2): 0xi0 hardware, 0xi1 software, i = 1,2,3
					//Console.WriteLine("TriggerCamera mode new: 0x" + Convert.ToString(mode, 16));
					Console.WriteLine("--> Trigger set to 0x" + mode.ToString("x"));
					err = _SetMode(hCamera, mode, expLevel, expTime, hbin, vbin, gain, 0, bitDepth, shift);
				}
				else if (name == "GainMode") //Gain
				{
					gain = value;
					err = _SetMode(hCamera, mode, expLevel, expTime, hbin, vbin, gain, 0, bitDepth, shift);
				}
				else _err = 1;

				ReportError(err, "--> Set " + name + " to " + Convert.ToString(value));
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
					hbin = value1;
					vbin = value2;

					if (value1 == 1) hbin = 0;
					else hbin = hbin >> 1;
					if (value2 == 1) vbin = 0;
					else vbin = vbin >> 1;

					err = _SetMode(hCamera, mode, expLevel, expTime, hbin, vbin, gain, 0, bitDepth, shift);
				}
				else if (name2 == "BinHorz" && name1 == "BinVert")
				{
					hbin = value2;
					vbin = value1;
					err = _SetMode(hCamera, mode, expLevel, expTime, hbin, vbin, gain, 0, bitDepth, shift);
				}
				else _err = 1;

				ReportError(err, "--> Set " + name1 + " & " + name2 + " to " + Convert.ToString(value1) + " & " + Convert.ToString(value2));
				return _err; 
			}
        }

        public short Get(string name)
        {
			lock (codeLock)
			{
				int wValue1 = 0;
				int wValue2 = 0;

				int unused1 = 0;
				int unused2 = 0;
				int unused3 = 0;

				int _err = 0;

				err = _GetPara(hCamera, BOARDVAL, 51);
				hbin = (int)((BOARDVAL[7] >> 7) & 0x01);
				vbin = (int)(BOARDVAL[7] & 0x0F);
				mode = (int)BOARDVAL[4];
				gain = (int)BOARDVAL[8];

				if (name == "Temperature")
					_err = _ReadTemperature(hCamera, ref wValue1);
				else if (name == "DoubleImageMode")
				{
					//array[4] is Mode, Mode = 32, 33 (hex 0x20, 0x21) is DoubleImage on
					wValue1 = Convert.ToInt32((BOARDVAL[4] == 0x20) || (BOARDVAL[4] == 0x21));
					ReportError(err, "--> Get" + name);
				}
				else if (name == "GainMode")
				{
					//array[8] is Gain
					wValue1 = (int)BOARDVAL[8];
					ReportError(err, "--> Get" + name);
				}
				else if (name == "Width")
				{
					ReportError(err, "--> GetPara" + name);
					err = _GetSizes(hCamera, ref unused1, ref unused2, ref wValue1, ref wValue2, ref unused3);
					ReportError(err, "--> Get" + name);
				}
				else if (name == "Height")
				{
					ReportError(err, "--> GetPara" + name);
					err = _GetSizes(hCamera, ref unused1, ref unused2, ref wValue2, ref wValue1, ref unused3);
					if (mode == 0x20 || mode == 0x21) wValue1 = wValue1 / 2;
					ReportError(err, "--> Get" + name);
				}
				else if (name == "BinHorz")
				{
					wValue1 = hbin;
					if (hbin == 0) wValue1 = 1;
					else wValue1 = wValue1 << 1; //Mode 0 = 1x, Mode 1 = 2x, Mode 2 = 4x

					ReportError(err, "--> Get" + name);
				}
				else if (name == "BinVert")
				{
					wValue1 = vbin;
					if (vbin == 0) wValue1 = 1;
					else wValue1 = wValue1 << 1; //Mode 0 = 1x, Mode 1 = 2x, Mode 2 = 4x
					ReportError(err, "--> Get" + name);
				}

				return (short)wValue1; 
			}
        }

		public ushort[] ArrGet(string name)
		{
			lock (codeLock)
			{
				//if (name == "ConversionFactors") ...
				ushort[] dummy = new ushort[2] { 0,1 };	//QE has two gain modes: 0 = low, 1 = high
				return dummy; 
			}
		}

        public float FGet(string name)
        {
			lock (codeLock)
			{
				Single result = 0;


				if (name == "Exposure")
				{
					err = _GetPara(hCamera, BOARDVAL, 51);              //array[5] is Exposure, base depending on Mode i.e. array[4]
					expTime = (int)BOARDVAL[5];

					if ((BOARDVAL[4] == 0x30) || (BOARDVAL[4] == 0x31))	//Timebase is millesecond in video mode 0x30, 0x31, see sdk doc
						result = (float)expTime;
					else                                                //Timebase is microsecond for any other mode
						result = expTime / (float)1000;
				}

				if (name == "Delay")
				{
					err = 0;                                            //not available for this camera
				}

				ReportError(err, "--> Get" + name);
				return result; 
			}
        }

        public uint ForceTrigger()
        {
            //UInt16 wTrig = 0;
            Console.WriteLine("--> Force Trigger ...\n");

            //err = _GetRecorderSubmode(hCamera, ref wTrig);
            //ReportError(err, "--> Submode");
            //Console.WriteLine("--> Buffer SubMode: "+wTrig);

            //wTrig = 0;

            err = _TriggerCamera(hCamera);
            ReportError(err, "--> ForceTrigger");

           /* if (wTrig == 0)
            {
                Console.WriteLine("--> Trigger error\n");
                err = 1;
            }*/
            return (uint)err;
        }

        //rest, unused(?)

        public void CancelImages(short picNr)  //removes picNr of buffers
        {
			lock (codeLock)
			{
				for (int i = 0; i < picNr; i++) //allocates picNr buffers, typically 2 (one for each image)
				{
					err = _RemoveBuffer(hCamera, bufnr[i]);       //in PixelflyUSB, RemoveBuffer and CancelBuffer have same functionality (see SDK manual). Here, we only have RemoveBuffer.
				} 
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
