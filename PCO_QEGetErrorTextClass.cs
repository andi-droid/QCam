using System;
using System.Collections.Generic;
using System.Text;

namespace QCam
{
    public class PCO_QEGetErrorTextClass
    {
        public static void PCO_GetErrorText(UInt32 dwerr, ref string pbuf)
        {
            string layertxt ="";
            string errortxt ="";
            string msg;
            //========================================================================================================//
            // Commmon error messages                                                                                 //
            //========================================================================================================//
            if (dwerr == 0)
            {
                pbuf = "No error.";
                return;
            }
            //========================================================================================================//
            // Library error messages                                                                                 //
            //========================================================================================================//
            else if (dwerr < 15)
            {
                layertxt = "Library error";
                switch(dwerr)
                {
                    case 1:
                        errortxt = "initialization failed; no camera connected";
                        break;
                    case 2:
                        errortxt = "timeout in any function";
                        break;
                    case 3:
                        errortxt = "function call with wrong parameter";
                        break;
                    case 4:
                        errortxt = "cannot locate PCI card or card driver";
                        break;
                    case 5:
                        errortxt = "wrong operating system";
                        break;
                    case 6:
                        errortxt = "no or wrong driver installed";
                        break;
                    case 7:
                        errortxt = "IO function failed";
                        break;
                    case 8:
                        errortxt = "n/a; reserved";
                        break;
                    case 9:
                        errortxt = "invalid camera mode";
                        break;
                    case 10:
                        errortxt = "n/a; reserved";
                        break;
                    case 11:
                        errortxt = "device is hold by another process";
                        break;
                    case 12:
                        errortxt = "error in reading or writing data to board";
                        break;
                    case 13:
                        errortxt = "wrong driver function";
                        break;
                    case 14:
                        errortxt = "n/a; reserved";
                        break;
                }
            }
            //========================================================================================================//
            // Driver error messages                                                                                  //
            //========================================================================================================//
            else if (dwerr < 234)
            {
                layertxt = "Driver error";
                switch(dwerr)
                {
                    case 101:
                        errortxt = "timeout in any driver function";
                        break;
                    case 102:
                        errortxt = "board is hold by an other process";
                        break;
                    case 103:
                        errortxt = "wrong boardtype";
                        break;
                    case 104:
                        errortxt = "cannot match processhandle to a board";
                        break;
                    case 105:
                        errortxt = "failed to init PCI";
                        break;
                    case 106:
                        errortxt = "no board found";
                        break;
                    case 107:
                        errortxt = "read configuratuion registers failed";
                        break;
                    case 108:
                        errortxt = "board has wrong configuration";
                        break;
                    case 109:
                        errortxt = "memory allocation error";
                        break;
                    case 110:
                        errortxt = "camera is busy";
                        break;
                    case 111:
                        errortxt = "board is not idle";
                        break;
                    case 112:
                        errortxt = "wrong parameter in function call";
                        break;
                    case 113:
                        errortxt = "head is disconnected";
                        break;
                    //case 113:
                    //    errortxt = "head verification failed"; ??? This was c/p straight from the SDK doc ...
                    //    break;
                    case 114:
                        errortxt = "board cannot work with attached head";
                        break;
                    case 116:
                        errortxt = "board initialisation FPGA failed";
                        break;
                    case 117:
                        errortxt = "board initialisation NVRAM failed";
                        break;
                    case 120:
                        errortxt = "not enough IO-buffer space for return values";
                        break;
                    case 121:
                        errortxt = "not enough IO-buffer space for return values";
                        break;
                    case 122:
                        errortxt = "Head power is switched off";
                        break;
                    case 130:
                        errortxt = "picture buffer not prepared for transfer";
                        break;
                    case 131:
                        errortxt = "picture buffer in use";
                        break;
                    case 132:
                        errortxt = "picture buffer hold by another process";
                        break;
                    case 133:
                        errortxt = "picture buffer not found";
                        break;
                    case 134:
                        errortxt = "picture buffer cannot be freed";
                        break;
                    case 135:
                        errortxt = "cannot allocate more picture buffer";
                        break;
                    case 136:
                        errortxt = "no memory left for picture buffer";
                        break;
                    case 137:
                        errortxt = "memory reserve failed";
                        break;
                    case 138:
                        errortxt = "memory commit failed";
                        break;
                    case 139:
                        errortxt = "allocate internal memory LUT failed";
                        break;
                    case 140:
                        errortxt = "allocate internal memory PAGETAB failed";
                        break;
                    case 148:
                        errortxt = "event not available";
                        break;
                    case 149:
                        errortxt = "delete event failed";
                        break;
                    case 156:
                        errortxt = "enable interrupts failed";
                        break;
                    case 157:
                        errortxt = "disable interrupts failed";
                        break;
                    case 158:
                        errortxt = "no interrupt connected to the board";
                        break;
                    case 164:
                        errortxt = "timeout in DMA";
                        break;
                    case 165:
                        errortxt = "no dma buffer found";
                        break;
                    case 166:
                        errortxt = "locking of pages failed";
                        break;
                    case 167:
                        errortxt = "unlocking of pages failed";
                        break;
                    case 168:
                        errortxt = "DMA buffersize to small";
                        break;
                    case 169:
                        errortxt = "PCI-Bus error in DMA";
                        break;
                    case 170:
                        errortxt = "DMA is runnig, command not allowed";
                        break;
                    case 228:
                        errortxt = "get processor failed";
                        break;
                    case 229:
                        errortxt = "n/a; reserved";
                        break;
                    case 230:
                        errortxt = "wrong processor found";
                        break;
                    case 231:
                        errortxt = "wrong processor size";
                        break;
                    case 232:
                        errortxt = "wrong processor device";
                        break;
                    case 233:
                        errortxt = "read flash failed";
                        break;
                }
            }
            else
            {
                pbuf = "Unknown error";
                return;
            }

            msg = layertxt;
            msg = String.Concat(msg, " error ");
            msg = String.Concat(msg, dwerr.ToString());
            msg = String.Concat(msg, ": ");
            msg = String.Concat(msg, errortxt);

            pbuf = msg;
        }
    }
}
