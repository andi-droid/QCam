using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCam
{
	static class Andor_GetCameraType
	{
		//From SDK doc/ATMCD32D.h:
		//
		// AC_CAMERATYPE_PDA 0
		// AC_CAMERATYPE_IXON 1
		// AC_CAMERATYPE_ICCD 2
		// AC_CAMERATYPE_EMCCD 3
		// AC_CAMERATYPE_CCD 4
		// AC_CAMERATYPE_ISTAR 5
		// AC_CAMERATYPE_VIDEO 6
		// AC_CAMERATYPE_IDUS 7
		// AC_CAMERATYPE_NEWTON 8
		// AC_CAMERATYPE_SURCAM 9
		// AC_CAMERATYPE_USBICCD 10
		// AC_CAMERATYPE_LUCA 11
		// AC_CAMERATYPE_RESERVED 12
		// AC_CAMERATYPE_IKON 13
		// AC_CAMERATYPE_INGAAS 14
		// AC_CAMERATYPE_IVAC 15
		// AC_CAMERATYPE_UNPROGRAMMED 16
		// AC_CAMERATYPE_CLARA 17
		// AC_CAMERATYPE_USBISTAR 18
		// AC_CAMERATYPE_SIMCAM 19
		// AC_CAMERATYPE_NEO 20
		// AC_CAMERATYPE_IXONULTRA 21
		// AC_CAMERATYPE_VOLMOS 22

		public static string Get(uint id)
		{
			switch (id)
			{
				case 0: return "PDA";
				case 1: return "IXON";
				case 2: return "ICCD";
				case 3: return "EMCCD";
				case 4: return "CCD";
				case 5: return "ISTAR";
				case 6: return "VIDEO";
				case 7: return "IDUS";
				case 8: return "NEWTON";
				case 9: return "SURCAM";
				case 10: return "USBICCD";
				case 11: return "LUCA";
				case 12: return "RESERVED";
				case 13: return "IKON";
				case 14: return "INGAAS";
				case 15: return "IVAC";
				case 16: return "UNPROGRAMMED";
				case 17: return "CLARA";
				case 18: return "USBISTAR";
				case 19: return "SICAM";
				case 20: return "NEO";
				case 21: return "IXONULTRA";
				case 22: return "VOLMOS";
				default: return "UNKNOWN";
			}
		}
	}
}
