using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCam
{
	[Serializable]
	public class CameraInfo
	{
		string _cameraName;
		int _axisID;
		public String CameraName { get { return _cameraName; } set { _cameraName = value; } }
		public int AxisID { get { return _axisID; } set { _axisID = value; } }
	}
}
