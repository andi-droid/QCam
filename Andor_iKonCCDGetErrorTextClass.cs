using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCam
{
    class Andor_iKonCCDGetErrorTextClass
    {
        //========================================================================================================//
        // Camera error messages                                                                                  //
        //========================================================================================================//
        static string[] ANDOR_ERROR_CAMERA_TXT = new string[]
        {
			"",												//0x4E20
			"Error communicating with camera.",				//0x4E21	DRV_ERROR_CODES
			"DRV_SUCCESS",									//0x4E22	DRV_SUCCESS
			"Device Driver not installed.",					//0x4E23	DRV_VXDNOTINSTALLED
			"DRV_ERROR_SCAN",								//0x4E24	DRV_ERROR_SCAN
			"DRV_ERROR_CHECK_SUM",							//0x4E25	DRV_ERROR_CHECK_SUM
			"Unable to load \"*.COF\" or \"*.RBF\" files.",	//0x4E26	DRV_ERROR_FILELOAD
			"DRV_UNKNOWN_FUNCTION",							//0x4E27	DRV_UNKNOWN_FUNCTION
			"DRV_ERROR_VXD_INIT",							//0x4E28	DRV_ERROR_VXD_INIT
			"DRV_ERROR_ADDRESS",							//0x4E29	DRV_ERROR_ADDRESS
			"Unable to allocate requested memory.",			//0x4E2A	DRV_ERROR_PAGELOCK
			"DRV_ERROR_PAGE_UNLOCK",						//0x4E2B	DRV_ERROR_PAGE_UNLOCK
			"DRV_ERROR_BOARDTEST",							//0x4E2C	DRV_ERROR_BOARDTEST
			"Unable to communicate with card/system.",		//0x4E2D	DRV_ERROR_ACK
			"DRV_ERROR_UP_FIFO",							//0x4E2E	DRV_ERROR_UP_FIFO
			"DRV_ERROR_PATTERN",							//0x4E2F	DRV_ERROR_PATTERN
        };

		//========================================================================================================//
		// Acquisition error messages                                                                             //
		//========================================================================================================//

        static string[] ANDOR_ERROR_ACQUISITION_TXT = new string[]
        {
			"",																			//0x4E30
            "DRV_ACQUISITION_ERRORS",													//0x4E31	DRV_ACQUISITION_ERRORS
			"Computer unable to read the data via the ISA slot at the required rate.",	//0x4E32	DRV_ACQ_BUFFER
			"DRV_ACQ_DOWNFIFO_FULL",													//0x4E33	DRV_ACQ_DOWNFIFO_FULL
			"DRV_PROC_UNKNOWN_INSTRUCTION",												//0x4E34	DRV_PROC_UNKNOWN_INSTRUCTION
			"DRV_ILLEGAL_OP_CODE",														//0x4E35	DRV_ILLEGAL_OP_CODE
			"Unable to meet Kinetic cycle time.",										//0x4E36	DRV_KINETIC_TIME_NOT_MET
			"Unable to meet Accumulate cycle time.",									//0x4E37	DRV_ACCUM_TIME_NOT_MET
			"No new data/No acquisition has taken place.",								//0x4E38	DRV_NO_NEW_DATA
			"",
			"Overflow of the spool buffer.",											//0x4E3A	DRV_SPOOLERROR
			"Error with spool settings.",												//0x4E3B	DRV_SPOOLSETUPERROR
        };

		//========================================================================================================//
		// Temp error messages                                                                                    //
		//========================================================================================================//

        static string[] ANDOR_ERROR_TEMPERATURE_TXT = new string[]
        {
			"",														//0x4E40
            "DRV_TEMPERATURE_CODES",								//0x4E41	DRV_TEMPERATURE_CODES
			"Temperature is OFF.",									//0x4E42	DRV_TEMP_OFF
			"Temperature reached but not stabilized.",				//0x4E43	DRV_TEMP_NOT_STABILIZED
			"Temperature has stabilized at set point.",				//0x4E44	DRV_TEMP_STABILIZED
			"Temperature has not reached set point.",				//0x4E45	DRV_TEMP_NOT_REACHED
			"DRV_TEMPERATURE_OUT_RANGE",							//0x4E46	DRV_TEMP_OUT_RANGE
			"DRV_TEMPERATURE_NOT_SUPPORTED",						//0x4E47	DRV_TEMP_NOT_SUPPORTED
			"Temperature had stabilized but has since drifted.",	//0x4E48	DRV_TEMP_DRIFT

        };

		//========================================================================================================//
		// General error messages                                                                                 //
		//========================================================================================================//

		static string[] ANDOR_ERROR_GENERAL_TXT = new string[]
        {
			"",																	//0x4E50
            "General error, refer to SDK manual/function call for specifics.",	//0x4E51	DRV_GENERAL_ERRORS
			"DRV_INVALID_AUX",													//0x4E52	DRV_INVALID_AUX
			"DRV_COF_NOTLOADED",												//0x4E53	DRV_COF_NOTLOADED
			"DRV_FPGAPROG",														//0x4E54	DRV_FPGAPROG
			"Unable to load \"*.RBF\".",										//0x4E55	DRV_FLEXERROR
			"Error communicating with GPIB card.",								//0x4E56	DRV_GPIBERROR
        };

		//========================================================================================================//
		// Function/Driver error messages                                                                         //
		//========================================================================================================//

		static string[] ANDOR_ERROR_DRIVER_TXT = new string[]
        {
			"DRV_DATATYPE",																		//0x4E60	DRV_DATATYPE
			"DRV_DRIVER_ERRORS",																//0x4E61	DRV_DRIVER_ERRORS
			"Function parameter #1 invalid, refer to SDK manual/function call for specifics.",	//0x4E62	DRV_P1INVALID
			"Function parameter #2 invalid, refer to SDK manual/function call for specifics.",	//0x4E63	DRV_P2INVALID
			"Function parameter #3 invalid, refer to SDK manual/function call for specifics.",	//0x4E64	DRV_P3INVALID
			"Function parameter #4 invalid, refer to SDK manual/function call for specifics.",	//0x4E65	DRV_P4INVALID
			"Unable to load \"DETECTOR.INI\".",													//0x4E66	DRV_INIERROR
			"Unable to load \"*.COF\".",														//0x4E67	DRV_COFERROR
			"Acquisition in progress.",															//0x4E68	DRV_ACQUIRING
			"Not acquiring/IDLE waiting on instructions.",										//0x4E69	DRV_IDLE
			"Executing temperature cycle.",														//0x4E6A	DRV_TEMPCYCLE
			"System not initialized.",															//0x4E6B	DRV_NOT_INITIALIZED
			"Function parameter #5 invalid, refer to SDK manual/function call for specifics.",	//0x4E6C	DRV_P5INVALID
			"Function parameter #6 invalid, refer to SDK manual/function call for specifics.",	//0x4E6D	DRV_P6INVALID
			"Not a valid mode, refer to SDK manual/function call for specifics.",				//0x4E6E	DRV_INVALID_MODE
			"Filter not available for current acquisition.",									//0x4E6F	DRV_INVALID_FILTER
        };

		//========================================================================================================//
		// Device error messages                                                                                  //
		//========================================================================================================//

		static string[] ANDOR_ERROR_DEVICE_TXT = new string[]
        {
			"DRV_I2CERRORS",																	//0x4E70	DRV_I2CERRORS
			"I2C device not present.",															//0x4E71	DRV_I2CDEVNOTFOUND
			"I2C command timed out.",															//0x4E72	DRV_I2CTIMEOUT
			"Function parameter #7 invalid, refer to SDK manual/function call for specifics.",	//0x4E73	DRV_P7INVALID
			"Function parameter #8 invalid? Error not referenced.",								//0x4E74
			"Function parameter #9 invalid? Error not referenced.",								//0x4E75
			"Function parameter #10 invalid? Error not referenced.",							//0x4E76
			"Function parameter #11 invalid? Error not referenced.",							//0x4E77
			"",																					//0x4E78
			"USB device error or unable to detect USB device",									//0x4E79	DRV_USBERROR
			"Integrate On Chip setup error.",													//0x4E7A	DRV_IOCERROR
			"DRV_VRMVERSIONERROR",																//0x4E7B	DRV_VRMVERSIONERROR
	        "",
			"DRV_USB_INTERRUPT_ENDPOINT_ERROR",													//0x4E7D	DRV_USB_INTERRUPT_ENDPOINT_ERROR
			"Invalid combination of tracks, out of memory or mode not available.",				//0x4E7E	DRV_RANDOM_TRACK_ERROR
			"DRV_INVALID_TRIGGER_MODE",															//0x4E7F	DRV_INVALID_TRIGGER_MODE
        };

		//========================================================================================================//
		// Misc. error messages                                                                                   //
		//========================================================================================================//

		static string[] ANDOR_ERROR_MISC_TXT = new string[]
        {
			"DRV_LOAD_FIRMWARE_ERROR",												//0x4E80	DRV_LOAD_FIRMWARE_ERROR
			"DRV_DIVIDE_BY_ZERO_ERROR",												//0x4E81	DRV_DIVIDE_BY_ZERO_ERROR
			"DRV_INVALID_RINGEXPOSURES",											//0x4E82	DRV_INVALID_RINGEXPOSURES
			"Range not multiple of horizontal binning.",							//0x4E83	DRV_BINNING_ERROR
			"Not a valid amplifier.",												//0x4E84	DRV_INVALID_AMPLIFIER
			"Count Convert mode not available with current acquisition settings.",	//0x4E85	DRV_INVALID_COUNTCONVERT_MODE
        };

		//========================================================================================================//
		// Memory error messages                                                                                  //
		//========================================================================================================//

		static string[] ANDOR_ERROR_MEMORY_TXT = new string[]
        {
			"",									//0x4E90
			"",									//0x4E91
			"",									//0x4E92
			"DRV_ERROR_MAP",					//0x4E93	DRV_ERROR_MAP
			"DRV_ERROR_UNMAP",					//0x4E94	DRV_ERROR_UNMAP
			"DRV_ERROR_MDL",					//0x4E95	DRV_ERROR_MDL
			"DRV_ERROR_UNMDL",					//0x4E96	DRV_ERROR_UNMDL
			"Output buffer size too small.",	//0x4E97	DRV_ERROR_BUFFSIZE
		    "",									//0x4E98
			"DRV_ERROR_NOHANDLE",				//0x4E99	DRV_ERROR_NOHANDLE
        };

		//========================================================================================================//
		// FPGA error messages                                                                                    //
		//========================================================================================================//

		static string[] ANDOR_ERROR_GATE_TXT = new string[]
        {
			"",									//0x4EA0
			"",									//0x4EA1
			"DRV_GATING_NOT_AVAILABLE",			//0x4EA2	DRV_GATING_NOT_AVAILABLE
			"DRV_FPGA_VOLTAGE_ERROR",			//0x4EA3	DRV_FPGA_VOLTAGE_ERROR
        };

		//========================================================================================================//
		// Common error messages                                                                                  //
		//========================================================================================================//

		static string[] ANDOR_ERROR_COMMON_TXT = new string[]
        {
			"Feature (currently?) not available.",//0x5200	DRV_NOT_AVAILABLE
			"",									//0x___1
			"",									//0x___2
			"",									//0x___3
			"",									//0x___4
			"",									//0x___5
			"",									//0x___6
			"",									//0x___7
			"",									//0x___8
			"",									//0x___9
			"",									//0x___A
			"",									//0x___B
			"",									//0x___C
			"",									//0x___D
			"No camera found.",					//0x51FE	DRV_ERROR_NOCAMERA
			"Not supported on this camera.",	//0x51FF	DRV_NOT_SUPPORTED
        };


		static string ERROR_CODE_OUTOFRANGE_TXT = "Error code out of range.";
		const UInt32 ANDOR_NOERROR = 0x00004E22;	//DRV_SUCCESS


		// ====================================================================================================== //
		// -- 1. Masks for evaluating error source and error code: ---------------------------------------------- //
		// ====================================================================================================== //
		const UInt32 ANDOR_ERROR_CODE_MASK = 0x0000000F;		// in this bit range the error codes reside
		const UInt32 ANDOR_ERROR_SOURCE_MASK = 0x000000F0;		// in this bit range the source codes reside

		// ====================================================================================================== //
		// -- 2. Source definitions: ---------------------------------------------------------------------------- //
		// ====================================================================================================== //
		const UInt32 ANDOR_ERROR_CAMERA = 0x00000020;			// camera error
		const UInt32 ANDOR_ERROR_ACQUISITION = 0x00000030;		// acquisition error
		const UInt32 ANDOR_ERROR_TEMPERATURE = 0x00000040;		// temp error
		const UInt32 ANDOR_ERROR_GENERAL = 0x00000050;			// general error
		const UInt32 ANDOR_ERROR_DRIVER = 0x00000060;			// driver error
		const UInt32 ANDOR_ERROR_DEVICE = 0x00000070;			// device error
		const UInt32 ANDOR_ERROR_MISC = 0x00000080;				// misc. error
		const UInt32 ANDOR_ERROR_MEMORY = 0x00000090;			// memory error
		const UInt32 ANDOR_ERROR_GATE = 0x000000A0;				// fpga error
		const UInt32 ANDOR_ERROR_COMMON1 = 0x000000F0;			// common error
		const UInt32 ANDOR_ERROR_COMMON2 = 0x00000000;			// common error



        public static void Andor_GetErrorText(UInt32 dwerr, ref string pbuf)
        {
			string sourcetxt;
			string errortxt;
			string msg;
			UInt32 index;

			if (dwerr == ANDOR_NOERROR)
			{
				pbuf = "No error.";
				return;
			}
        
			// -- evaluate source information within complete error code -- //
			// ------------------------------------------------------------ //

			index = dwerr & ANDOR_ERROR_CODE_MASK;

			switch(dwerr & ANDOR_ERROR_SOURCE_MASK)   // evaluate source
			{
				case ANDOR_ERROR_COMMON1:				//For both commons, do ...
				case ANDOR_ERROR_COMMON2:			
					sourcetxt = "Common";
					if (index < ANDOR_ERROR_COMMON_TXT.Length)
						errortxt = ANDOR_ERROR_COMMON_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				case ANDOR_ERROR_CAMERA:
					sourcetxt =  "Camera";
					if (index < ANDOR_ERROR_CAMERA_TXT.Length)
						errortxt = ANDOR_ERROR_CAMERA_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				case ANDOR_ERROR_ACQUISITION:
					sourcetxt = "Acquisition";
					if (index < ANDOR_ERROR_ACQUISITION_TXT.Length)
						errortxt = ANDOR_ERROR_ACQUISITION_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				case ANDOR_ERROR_TEMPERATURE:
					sourcetxt = "Temeprature";
					if (index < ANDOR_ERROR_TEMPERATURE_TXT.Length)
						errortxt = ANDOR_ERROR_TEMPERATURE_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				case ANDOR_ERROR_GENERAL:
					sourcetxt = "General";
					if (index < ANDOR_ERROR_GENERAL_TXT.Length)
						errortxt = ANDOR_ERROR_GENERAL_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				case ANDOR_ERROR_DRIVER:
					sourcetxt =  "Driver";
					if (index < ANDOR_ERROR_DRIVER_TXT.Length)
						errortxt = ANDOR_ERROR_DRIVER_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				case ANDOR_ERROR_DEVICE:
					sourcetxt = "Device";
					if (index < ANDOR_ERROR_DEVICE_TXT.Length)
						errortxt = ANDOR_ERROR_DEVICE_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				case ANDOR_ERROR_MISC:
					sourcetxt = "Misc.";
					if (index < ANDOR_ERROR_MISC_TXT.Length)
						errortxt = ANDOR_ERROR_MISC_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				case ANDOR_ERROR_MEMORY:
					sourcetxt = "Memory";
					if (index < ANDOR_ERROR_MEMORY_TXT.Length)
						errortxt = ANDOR_ERROR_MEMORY_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				case ANDOR_ERROR_GATE:
					sourcetxt = "Gate";
					if (index < ANDOR_ERROR_GATE_TXT.Length)
						errortxt = ANDOR_ERROR_GATE_TXT[index];
					else errortxt = ERROR_CODE_OUTOFRANGE_TXT;
				break;
				default:
					sourcetxt =  "Undefined source";
					errortxt = "";
				break;
			}

			if (errortxt == "") errortxt = "No error text available!";

			msg = sourcetxt;
			msg = String.Concat(msg, " error 0x");
			msg = String.Concat(msg, dwerr.ToString("X"));
			msg = String.Concat(msg, ": ");
			msg = String.Concat(msg, errortxt);

			pbuf = msg;
        }
    }
}
