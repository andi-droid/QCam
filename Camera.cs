using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCam
{
    static class Camera
    {
        public static ICamera CreateObject(int i, string path)
        {
			if (i == 0) return new PixelflyUSB();
			else if (i == 1) return new PixelflyQE();
			else if (i == 2) return new Andor_iKonCCD(path);
			else if (i == 3) return new Andor_iXonEMCCD(path);
			else return null;
        }
    }
}
