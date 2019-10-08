using System;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Timers;


namespace QCam
{
	public partial class FormMain
	{
        public System.Timers.Timer camWareTimer;
        public System.Timers.Timer triggerTimer;

		private void loadAndorAdvancedSettings(string what)
		{
			if (camera.Descr.isAdvanced)
			{
				if (what == "ALL")
				{
					//int max = camera.Get("noFKVSS");
					//for (int i = 0; i < max; i++)
					//	FKVSSBox.Items.Add(i.ToString());

					//max = camera.Get("noADChannels");
					//for (int i = 0; i < max; i++)
					//	ADChannelBox.Items.Add(i.ToString());

					//max = camera.Get("noHSS");
					//for (int i = 0; i < max; i++)
					//	HSSBox.Items.Add(i.ToString());
				
					

					ADChannelBox.Items.Clear();
					ushort[] adModes = camera.ArrGet("ADChannels");
					for (int i = 0; i < adModes.Length; i++)
						ADChannelBox.Items.Add(adModes[i]);

					FKVSSBox.Items.Clear();
					ushort[] fkvModes = camera.ArrGet("FKVSS");
					for (int i = 0; i < fkvModes.Length; i++)
						FKVSSBox.Items.Add(fkvModes[i]);

					HSSBox.Items.Clear();
					ushort[] hssModes = camera.ArrGet("HSS");
					for (int i = 0; i < hssModes.Length; i++)
						HSSBox.Items.Add(hssModes[i]);

					VSSBox.Items.Clear();
					ushort[] vssModes = camera.ArrGet("VSS");
					for (int i = 0; i < vssModes.Length; i++)
						VSSBox.Items.Add(vssModes[i]);

					FKVSSBox.Enabled = true;
					ADChannelBox.Enabled = true;
					HSSBox.Enabled = true;
					VSSBox.Enabled = true;
					VClockVoltageBox.Enabled = true;
					ModeBox.Enabled = true;
					SignalBox.Enabled = true;

					textBoxNoImages.Enabled = true;
					textBoxTemperature.Enabled = true;

					camera.Set("ADChannel", Properties.Settings.Default.channelID);
					ADChannelBox.SelectedIndex = camera.Get("ADChannel");

					FKVSSBox.SelectedIndex = camera.Get("FKVSS");
					HSSBox.SelectedIndex = camera.Get("HSS");
					VSSBox.SelectedIndex = camera.Get("VSS");
					VClockVoltageBox.SelectedIndex = camera.Get("VClockVoltage");
				}
				else if (what == "HSS")										//HSS depend on ADCHannel.
				{
					camera.Set("ADChannel", ADChannelBox.SelectedIndex);	//So first we set ADChannel (making sure ADChannel is set to what is currently displayed on GUI) ... 

					HSSBox.Items.Clear();
					ushort[] hssModes = camera.ArrGet("HSS");				//...then the HSS Modes for the ADChannel are loaded into the dropdownbox.
					for (int i = 0; i < hssModes.Length; i++)
						HSSBox.Items.Add(hssModes[i]);

					HSSBox.SelectedIndex = camera.Get("HSS");
				}
			}
		}

		private void loadGainSettings()
		{
			gainBox.Items.Clear();
			ushort[] modes = camera.ArrGet("ConversionFactors");
			for (int i = 0; i < modes.Length; i++)
				gainBox.Items.Add(modes[i]);

			gainBox.SelectedIndex = 0;
		}

		private void CameraConnect()
		{

			uint err = 0;

			if (!Connected)
			{
				if (!error_server)
				{
					//directory for pics is all set up & ready
					Console.WriteLine("\rConnecting camera ...");
					try
					{
						Connected = true;


						camera.GetLastError();	//Flush error buffer
						Console.WriteLine("Opening ...");
						err = camera.Open();

						if (camera.Descr.name != cameraSelectorBox.SelectedItem.ToString()) throw new Exception("No " + cameraSelectorBox.SelectedItem.ToString() + " found.");

						labelRange.Text = "Range " + camera.Descr.MinTemp + " ... " + camera.Descr.MaxTemp + " °C";
						Console.WriteLine("Camera ready.");

						if (camera.Set("TriggerMode", 2) > 0) Console.WriteLine("Error in Set switch"); 
						//Triggermode 1 software, 2 extern (software & extern for Pixelfly USB)
						//Triggermode for Andor needs to be tested.

						// enable everything
						toggleForm(true);
						loadAndorAdvancedSettings("ALL");
						loadGainSettings();

						//CameraRefresh();

						ToggleDoubleImage();
						enableDoubleImage.Checked = true;
						if (camera.Descr.DoubleImage == 1) Console.WriteLine("\rDoubleImage supported.");

						cameraName = cameraSelectorBox.SelectedItem.ToString();
						this.Text = cameraName + " in " + the_day + the_month + the_year + " on " + System.Net.Dns.GetHostName().ToString();
					}
					catch (Exception ex)
					{
						Console.WriteLine("Cannot open camera (1):" + Environment.NewLine + ex.ToString());
						camera_connected = false;
						this.enableCamera.Checked = false;
						this.enableCamera.Enabled = false;
						disposeCameras();
						return;
					}
					if (err > 0 || camera.GetLastError() > 0)
					{
						Console.WriteLine("Cannot open camera (2).");
						camera_connected = false;
						this.enableCamera.Checked = false;
						this.enableCamera.Enabled = false;
						disposeCameras();
						return;
					}
					else
					{
						this.enableCamera.Enabled = true;
						camera_connected = enableCamera.Checked;
					}

					CameraRefresh();
					textBoxExposure.Text = Properties.Settings.Default.expPix.ToString();
					CameraUpdate();

					if (abortServer)
					{
						//restart Server
						//**** START THE SERVER THREAD ****
						Console.Write("Starting Server Thread ... ");
						abortServer = false;
						serverThread = new Thread(new ThreadStart(serverEntryPoint));
						serverThread.Start();			//ServerMain.cs
					}
				}
				else
				{
					// error while creating directory, please check settings
					MessageBox.Show("No valid picture folder has been set. Please check Settings.");
				}
			}
			else
			{
				if (camera.Get("Temperature") >= 0 || formClosed)
				{
					Console.Write("Disconnecting camera ... ");

					abortServer = true;
					while (serverThread != null && serverThread.IsAlive)
					{ Thread.Sleep(10); }

					try
					{
						// disable everything
						Connected = false;
						toggleForm(false);

						camera.Close();
						Console.Write("Done.\n");
						camera_connected = false;
						cameraName = Properties.Settings.Default.programName;
						this.Text = cameraName + " in " + the_day + the_month + the_year + " on " + System.Net.Dns.GetHostName().ToString();

						Console.WriteLine("Camera offline.");
					}
					catch
					{
						MessageBox.Show("Unable to disconnect from camera.");
						//re-activate Connected and camera_connected??
					}
				}
				else MessageBox.Show("Temperature below 0°, turn off cooling and wait till it has risen.");
			}
		}

		private void CameraRefresh()
		{
			// Déclarations :

			//textBoxWidth.Text = Convert.ToString(Pixelfly1.Descr.MaxHorzRes);
			//textBoxHeight.Text = Convert.ToString(Pixelfly1.Descr.MaxVertRes);
			textBoxXOffset.Text = "0";
			textBoxYOffset.Text = "0";
            textBoxSubX.Text = "1";
            textBoxSubY.Text = "1";

			// get exposure value

			Exposure = camera.FGet("Exposure");
			textBoxExposure.Text = Convert.ToString(Exposure);

			// get gain value

			Gain = camera.Get("GainMode");
			gainBox.SelectedIndex = Gain;

            // get ROI

            _Width = camera.Get("Width");
            _Height = camera.Get("Height");

            textBoxWidth.Text = Convert.ToString(_Width);
            textBoxHeight.Text = Convert.ToString(_Height);

			// get Offset

			textBoxXOffset.Text = Convert.ToString(camera.Get("OffsetX"));
			textBoxYOffset.Text = Convert.ToString(camera.Get("OffsetY"));

			// get Binning :

			UInt16 wBinHorz = 1;
			UInt16 wBinVert = 1;
			camera.GetLastError();	//flush error buffer

			wBinHorz = (ushort)camera.Get("BinHorz");
			wBinVert = (ushort)camera.Get("BinVert");

			if (camera.GetLastError() > 0)
			{
				MessageBox.Show("Unable to get binning.");
			}
			else
			{
                textBoxSubX.Text = wBinHorz.ToString();
                textBoxSubY.Text = wBinVert.ToString();

                if (wBinHorz > 1 || wBinVert > 1)
				{
					checkBoxBinning.Checked = true;
					textBoxSubX.ReadOnly = false;
					textBoxSubY.ReadOnly = false;
				}
				else
				{
					checkBoxBinning.Checked = false;
					textBoxSubX.ReadOnly = true;
					textBoxSubY.ReadOnly = true;
				}
			}

			// get double image mode status :

			UInt16 wDoubleImage = 0;

			if (camera.Descr.DoubleImage == 0)
			{
				Console.WriteLine("DoubleImage not supported.");
			}
			else if (camera.Descr.DoubleImage == 1)
			{
				//Console.WriteLine("\rDoubleImage supported.");
				wDoubleImage = (ushort)camera.Get("DoubleImageMode");
			}

			if (wDoubleImage == 1)
			{
				textBoxNoImages.Text = "2";
				enableDoubleImage.Checked = true;
			}
			else
			{
				textBoxNoImages.Text = "4";
				enableDoubleImage.Checked = false;
			}

			//advanced properties

			if (videoProps)
			{
				//Update them
				UInt16 wAmp = 1;

				wAmp = (ushort)camera.Get("Amp");
				if (wAmp == 1) checkBoxEM.Checked = false;
				else checkBoxEM.Checked = true;
				textBoxEM.Text = Convert.ToString(camera.Get("EMGain"));
				FKVSSBox.SelectedIndex = camera.Get("FKVSS");				//check what happens on PCO opening this.
				ADChannelBox.SelectedIndex = camera.Get("ADChannel");
				HSSBox.SelectedIndex = camera.Get("HSS");
				VClockVoltageBox.SelectedIndex = camera.Get("VClockVoltage");
				ModeBox.SelectedIndex = camera.Get("ShutterMode");
				SignalBox.SelectedIndex = camera.Get("ShutterOutput");
				//camera.Set("STTOpen", Convert.ToUInt16(textBoxSTTOpen.Text), "STTClose", Convert.ToUInt16(textBoxSTTClose.Text));
			}
		}

		private void CameraUpdate()
		{
			uint err = 0;

            if (!pictureBeingTaken || forceUpdate)
            {
                //avoid trying to update cam while taking pictures
                Console.WriteLine("Updating camera settings ...");

                if (Previewing)
                {
                    // Stop preview

                }

				camera.GetLastError();

				//Set Offset

				camera.Set("OffsetY", Convert.ToInt32(textBoxYOffset.Text));

				//Set Gain

				camera.Set("GainMode", gainBox.SelectedIndex);

                // Set new exposure

                Int32 dwExposure = 0;
                dwExposure = (Int32)(Convert.ToSingle(textBoxExposure.Text.Replace(".", ",")) * (float)1000);	//in µs, i.e. ms (form field) * 1000
                Properties.Settings.Default.expPix = Convert.ToSingle(textBoxExposure.Text.Replace(".", ","));

				camera.GetLastError();
                camera.Set("µsExposure", dwExposure);
                if (camera.GetLastError() > 0)
                {
                    MessageBox.Show("Unable to set new exposure. Reseting to old values.");
                    CameraRefresh();
                }

                // Set Double Image;

                UInt16 wDoubleImage = 0;

                if (camera.Descr.DoubleImage == 0)
                {
                    Console.WriteLine("DoubleImage not supported.");
                    enableDoubleImage.Checked = false;
                }
                else if (camera.Descr.DoubleImage == 1)
                {
                    Console.WriteLine("DoubleImage supported.");

                    if (enableDoubleImage.Checked)
                    {
                        wDoubleImage = 1;
                    }
					camera.GetLastError();
                    camera.Set("DoubleImageMode", wDoubleImage);
                    if (camera.GetLastError() > 0)
                    {
                        Console.WriteLine("Unable to set double image mode. Reseting to old values.");
                        //ReportError(err, "CamUpdate - SetDoubleImageMode");
                        CameraRefresh();
                    }

					//advanced properties

					if (videoProps)
					{
						//Update them
						camera.Set("EMGain", Convert.ToUInt16(textBoxEM.Text));
						camera.Set("FKVSS", FKVSSBox.SelectedIndex);
						camera.Set("ADChannel", ADChannelBox.SelectedIndex);
						camera.Set("HSS", HSSBox.SelectedIndex);
						camera.Set("VClockVoltage", VClockVoltageBox.SelectedIndex);
						camera.Set("ShutterMode", ModeBox.SelectedIndex);
						camera.Set("ShutterOutput", SignalBox.SelectedIndex);
						camera.Set("STTOpen", Convert.ToUInt16(textBoxSTTOpen.Text), "STTClose", Convert.ToUInt16(textBoxSTTClose.Text));
					}
                }


                // Set Binning :

                UInt16 wBinHorz;
                UInt16 wBinVert;

                if (checkBoxBinning.Checked)
                {
                    wBinHorz = Convert.ToUInt16(textBoxSubX.Text);
                    wBinVert = Convert.ToUInt16(textBoxSubY.Text);
                }
                else
                {
                    wBinHorz = (UInt16)1; //1x == no binning
                    wBinVert = (UInt16)1;
                }

				camera.GetLastError();
                camera.Set("BinHorz", wBinHorz, "BinVert", wBinVert);
                if (camera.GetLastError() > 0)
                {
                    MessageBox.Show("Unable to set Binning. Reseting to old values.");
                    //ReportError(err, "CamUpdate - SetBinning");
                    CameraRefresh();
                }

				//if (cooling) camera.Set("Temperature", Convert.ToInt32(textBoxTemperature.Text));

                // Set Resolution:

                // Set ROI (region or area of interest) window. The ROI must be equal to or smaller than the
                // absolute image area, which is defined by the settings of  formatand binning. If the binning
                // settings are changed, the user must adapt the ROI, before PCO_ArmCamera is accessed. The
                // binning setting sets the limits for the ROI. For example, a sensor with 1600x1200 and binning 2x2
                // will result in a maximum ROI of 800x600.

                camera.Set("Width", Convert.ToUInt16(textBoxWidth.Text), "Height", Convert.ToUInt16(textBoxHeight.Text));

                //Not actually implemented in CAMERA class yet, pictures will be taken at max. resolution

                //Gain, Offset (what's that?) missing as well

                CameraRefresh();
            }
			

			if (Previewing) { }

		}

		private void ToggleDoubleImage()
		{
			uint err = 0;
			UInt16 wDoubleImage = 0;

			if (enableDoubleImage.Checked && camera.Descr.DoubleImage == 0)
			{
				MessageBox.Show("DoubleImage not supported!");
				enableDoubleImage.Checked = false;
			}
			else if (camera.Descr.DoubleImage == 1)
			{
				textBoxNoImages.Text = "4";

				if (enableDoubleImage.Checked)
				{
					wDoubleImage = 1;
					textBoxNoImages.Text = "2";
					if (camera.Descr.isAdvanced)	//DoubleImage via FastKinetics mode for Andor --> Half the chip height for picture, storage area starting with Offset
					{
						textBoxHeight.Text = Convert.ToString((camera.Descr.MaxVertRes >> 1));
						textBoxYOffset.Text = Convert.ToString((camera.Descr.MaxVertRes >> 1));
						camera.Set("Height", Convert.ToUInt16(textBoxHeight.Text));
						camera.Set("OffsetY", Convert.ToUInt16(textBoxYOffset.Text));
					}
				}
				else if (camera.Descr.isAdvanced)
				{
					textBoxHeight.Text = Convert.ToString(camera.Descr.MaxVertRes);
					textBoxYOffset.Text = "0";
					camera.Set("Height", Convert.ToUInt16(textBoxHeight.Text));
					camera.Set("OffsetY", Convert.ToUInt16(textBoxYOffset.Text));
				}
				err = camera.Set("DoubleImageMode", wDoubleImage);
				if (err > 0)
				{
					Console.WriteLine("Unable to set double image mode. Reseting to old values.");
					//ReportError(err, "CamUpdate - SetDoubleImageMode");
					CameraRefresh();
				}
			}
		}

		private void ToggleEM()
		{
			//if camera.Descr.EM then EM available
			if (checkBoxEM.Checked)
			{
				textBoxEM.Enabled = true;
				camera.Set("Amp", 0);		//EM Gain 0
			}
			else
			{
				textBoxEM.Enabled = false;
				camera.Set("Amp", 1);		//Conventional 1
			}

			loadAndorAdvancedSettings("HSS");
		}
		
		private void CameraSnapshot()
		{
            saveFileDialog1.FileName = "snapshot";
            DialogResult res = saveFileDialog1.ShowDialog();       //Main must be set to [STAThread] 
			saveFileDialog1.FileName += (tiff ? ".tif" : ".png");
            if (res == DialogResult.Cancel) saveFileDialog1.FileName = "";

            if (autoCount) shotID = (globalImgCounter).ToString(idFormat);
            else
            {
                int lastID = getLastID(day_folder);
				shotID = (lastID).ToString(idFormat);
            }

			filenameBase = generateFileString(filenameFormat, null, Convert.ToInt32(shotID)) + "_SN";
			filenameBase = day_folder + @"\" + filenameBase;

            camera.Set("TriggerMode", 1);

			snapshot = true;
			sequenceTime = 10;

			cameraCaptureThread = new Thread(new ThreadStart(cameraCaptureEntryPoint));
			cameraCaptureThread.Start();

            triggerTimer.Start();
		}

        void triggerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            uint err = 0;

            err = camera.ForceTrigger();
            if (err > 0) Console.WriteLine("... but camera busy or not ready. Trigger not accepted.");

            Thread.Sleep(1000);

            err = camera.ForceTrigger();
            if (err > 0) Console.WriteLine("... but camera busy or not ready. Trigger not accepted.");

            if (!enableDoubleImage.Checked)
            {
                Thread.Sleep(1000);

                err = camera.ForceTrigger();
                if (err > 0) Console.WriteLine("... but camera busy or not ready. Trigger not accepted.");

                Thread.Sleep(1000);

                err = camera.ForceTrigger();
                if (err > 0) Console.WriteLine("... but camera busy or not ready. Trigger not accepted.");
            }

			snapshot = false;
        }

		private void ReportError(uint err, string source)
		{
			string errStr = "";
			if (err > 0)
			{

				PCO_USBGetErrorTextClass.PCO_GetErrorText(err, ref errStr);
				//Debug.Write(source + " error : " + errStr + "\n");
				//MessageBox.Show(source + " error : " + errStr);
				Console.WriteLine(source + " error : " + errStr);
			}
			else
			{
				//Debug.Write(source + " : OK \n");
				Console.WriteLine(source + " : OK ");
				//MessageBox.Show(source + " : OK ");
			}
		}

		private void CameraPreview()
		{
			if (!Previewing)
			{
				Previewing = true;

				if (Connected) CameraConnect();

				buttonPreview.Text = "Stop Prev.";
				toggleForm(false);
				buttonConnect.Enabled = false;
				enableCamera.Enabled = false;
				Console.WriteLine("\nPixelfly is sleeping...");


				camWare.Start();
				camWareTimer.Start();
			}
			else
			{
				camWare.CloseMainWindow();
			}
		}

		void camWareTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (camWare.HasExited)
			{
				Previewing = false;
				buttonPreview.Text = "Preview";
				toggleForm(true);
				buttonConnect.Enabled = true;
				enableCamera.Enabled = true;
				Console.WriteLine("Pixelfly is back!\n");
				if (!Connected) CameraConnect();
			}
			else camWareTimer.Start();
		}

		#region Other functions, not implemented for pixelfly

		// Following functions are not implemented yet for the pixelfly model...

/*		unsafe private void CameraPreview() // Not implemented yet for pixelfly 
		{
			
			//            if (!Previewing)
			//            {
			//                CameraUpdate();

			//#if true
			//                // Connect callback function
			//                iFrameCount = 0;
			//                LuCamAPI.CallbackDelegate MyPreviewCallback = new LuCamAPI.CallbackDelegate(PreviewCallback);
			//                fixed (void* ptr = &iFrameCount)
			//                {
			//                    iCallbackID = LuCamAPI.Win32.LucamAddRgbPreviewCallback(hCamera, MyPreviewCallback, ptr, 2);
			//                }
			//#endif
			//                // Start preview



			//                if (false == LucamCamera.CreateDisplayWindow(hCamera, Properties.Settings.Default.cameraName + " preview", 268435456, this.Location.X + this.Width, this.Location.Y, 696, 520, 0, 0))
			//                {
			//                    MessageBox.Show("Unable to create window.");
			//                }
			//                if (false == LucamCamera.StreamVideoControl(hCamera, LUCAMAPICOMLib.LUCAM_STREAMING_MODE.START_DISPLAY, 0))
			//                {
			//                    MessageBox.Show("Unable to start preview.");
			//                }
			//                else
			//                {
			//                    Previewing = true;
			//                    buttonPreview.Text = "Stop";
			//                    //LucamCamera.AdjustDisplayWindow(hCamera, "test", 0, 0, 200, 200);

			//                }
			//            }
			//            else
			//            {
			//                // Stop preview
			//                if (false == LucamCamera.StreamVideoControl(hCamera, LUCAMAPICOMLib.LUCAM_STREAMING_MODE.STOP_STREAMING, 0))
			//                {
			//                    MessageBox.Show("Unable to stop preview.");
			//                }
			//                else
			//                {
			//                    if (false == LucamCamera.DestroyDisplayWindow(hCamera))
			//                        MessageBox.Show("Unable to destroy preview window");
			//                }
			//#if true
			//                // Remove callback
			//                LuCamAPI.Win32.LucamRemoveRgbPreviewCallback(hCamera, iCallbackID);
			//#endif

			//                Previewing = false;
			//                buttonPreview.Text = "Preview";
			//            }
		}
*/
		unsafe public static void PreviewCallback(uint pContext, byte* pData, uint dataLength, uint unused) // Not implemented yet for pixelfly
		{
			//int* piFrameCount = (int*)pContext;
			//byte bSomeByte = 255;
			//int i;

			//(*piFrameCount)++;

			//// Write to memory buffer (will draw white line in preview window at bottom)
			//for (i = 0; i < *piFrameCount; i++)
			//{
			//    // Marshal.WriteByte(pData, i, bSomeByte);
			//    pData[i] = bSomeByte;
			//    i = i + 1;
			//}
		}

		#endregion

	}
}
