using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCam
{
    public struct Description
    {
		public string name;
		public bool isAdvanced;
        public ushort MaxHorzRes;      // Maxmimum horz. resolution in std.mode
        public ushort MaxVertRes;      // Maxmimum vert. resolution in std.mode
        public ushort DoubleImage;        // Double image mode possibility
		public ushort TempStatus;			//Status of temperature while cooling: 0 off; 1 reached but not stabilized; 2 reached and stabilized; 3 not reached
		public short MinTemp;
		public short MaxTemp;
		public ushort RunError;			//1 if error occured during run, 0 else
    }

    public unsafe interface ICamera
    {
		ushort[] ArrGet(string name);
        void CancelImages(short picNr);
        void Close();
        float FGet(string name);
        uint ForceTrigger();
        short Get(string name);
        UInt16* GetImage(int img);
        UInt32 GetLastError();
        uint Open();
        //RemoveBuffer();
        //uint Set(string name, ushort value);
        uint Set(string name, int value);
        uint Set(string name1, ushort name2, string value1, ushort value2);
        void SetFinished(short picNr);
        void SetReady(bool doubleImage, short picNr);
        bool WaitOnImage(short img);

        Description Descr
        {
            get;
        }
    }
}
