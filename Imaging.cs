using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;	//For WPF imaging
using System.Runtime.InteropServices;
using System.IO;	//for FileStream
using System.Runtime.ExceptionServices; //for catching accessviolations in try catch


namespace QCam
{
	public partial class FormMain
	{
		private bool capture_error_shown = false;

		[HandleProcessCorruptedStateExceptions]
		private unsafe void cameraCaptureEntryPoint()
		{
			try
			{
				capture();
			}
			catch (Exception e)
			{
				Console.WriteLine("Fatal capture error: " + e.Message + " Sequence aborted.");
				System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "capture.txt", e.ToString());
				if (!capture_error_shown) (new Thread(() => { MessageBox.Show("Fatal capture error: " + e.Message + " Sequence aborted.", DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information); capture_error_shown = false; })).Start();
				capture_error_shown = true;
			}
		}

		[HandleProcessCorruptedStateExceptions]
		private unsafe void capture()
		{
			long ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			pictureBeingTaken = true;
			Thread.MemoryBarrier();

			// variables declaration :

			Console.Write("\n\n=======================================\nStarting capture ...\n");


			ushort* buf, buf2, buf3, buf4;
			short PicNr, WaitImg;

			Boolean wDoubleImage = enableDoubleImage.Checked;			//HERE
			ushort width = Convert.ToUInt16(textBoxWidth.Text);			//HERE
			ushort height = Convert.ToUInt16(textBoxHeight.Text);		//HERE

			buf = null; buf2 = null; buf3 = null; buf4 = null;

			bool bReturn;
			uint err = 0;

			//
			PicNr = 2;
			WaitImg = 2;
			if (!wDoubleImage)
			{
				PicNr = 4;
				WaitImg = 4;
			}

			camera.GetLastError();
			forceUpdate = true;
			CameraUpdate();         //Because default values after opening, update with current/latest settings
			if (camera is PixelflyQE && snapshot) camera.Set("TriggerMode", 1);
			forceUpdate = false;

			camera.SetReady(wDoubleImage, PicNr);

			if (camera.GetLastError() > 0)
			{
				Console.WriteLine("Error while starting capture.");
			}
			else
			{
				// Waiting for buffers to be set:

				long elapsed = 0;
				int timeOut = (int)(sequenceTime + triggerWait) * 1000;
				short watched_buffer;
				string fileName;

				//Console.WriteLine(Convert.ToString(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - ms) + "ms elapsed ...");
				Console.Write("Wait for Trigger, Timeout in " + Convert.ToString(sequenceTime + triggerWait) + "s ...\n");

				ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

				do
				{
					Thread.Sleep(10);
					elapsed = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - ms;

					if (camera.GetLastError() > 0) elapsed = -1;
				}
				while (elapsed > -1 && camera.WaitOnImage(WaitImg) && elapsed < timeOut); //waits till "buffer event is set" for Image 1 (Image 2 if no DoubleImage)
				ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
				if (elapsed >= timeOut)
				{
					Console.WriteLine("Timeout error\n");
					bReturn = false;
				}
				else if (camera.GetLastError() > 0 || elapsed == -1)
				{
					Console.WriteLine("Error while acquiring Image\n");
					bReturn = false;
				}
				else
				{
					buf = camera.GetImage(1);
					buf2 = camera.GetImage(2);
					if (!wDoubleImage)
					{
						buf3 = camera.GetImage(3);
						buf4 = camera.GetImage(4);
					}

					if (camera.GetLastError() > 0) bReturn = false;
					else bReturn = true;
				}

				if (buf == null) Console.WriteLine("Buf1 Null error\n");
				if (buf2 == null) Console.WriteLine("Buf2 Null error\n");
				if (!wDoubleImage && buf3 == null) Console.WriteLine("Buf3 Null error\n");
				if (!wDoubleImage && buf4 == null) Console.WriteLine("Buf4 Null error\n");

				// Saving picture :

				Thread.MemoryBarrier();
				pictureHasBeenTaken = bReturn;
				Thread.MemoryBarrier();

				if (bReturn)
				{
					// Get save file name
					fileName = saveFileDialog1.FileName; //check if saveFileDialog has been executed; in that case Snapshot has been done					//HERE
					saveFileDialog1.FileName = "";  //clear for next time																					//HERE

					if (string.IsNullOrEmpty(fileName))
					{
						fileName = filenameBase + (tiff ? ".tif" : ".png");	//if tiff then ".tif" else ".png"; folder is included in template
						//@"\" + shot_folder_name + "_" + Properties.Settings.Default.cameraName + ".png";
					}


					#region Saving
					Console.WriteLine("Processing picture, " + fileName + "...");
					try
					{
						//Console.WriteLine(Convert.ToString(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - ms) + "ms elapsed ...");
						process16bitImage(-0.5, 5.0, width, height, fileName, buf, buf2, buf3, buf4, wDoubleImage, fluoPixCheckbox.Checked);		//HERE
					}
					catch (Exception e)
					{
						Console.WriteLine("Fatal processing error: " + e.Message + " Sequence has been lost.");
						System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "processing.txt", e.ToString());
						if (!capture_error_shown) (new Thread(() => { MessageBox.Show("Fatal processing error: " + e.Message + " Sequence has been lost.", DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information); capture_error_shown = false; })).Start();
						capture_error_shown = true;
					}

					Console.WriteLine("Done!");
					#endregion
				}
			}

			// Free buffer, stop recording :

			camera.SetFinished(PicNr);
			camera.Set("TriggerMode", 2);   //in case it was a software trigger, which set the Mode to 1 ...

			Thread.MemoryBarrier();
			pictureBeingTaken = false;
			Debug.Write("pixelfly entry point END\n");
			Console.WriteLine("=======================================\n");

			return;
		}

		//private bool connectToViewer()
		//{
		//	IPAddress lclhst = Dns.GetHostEntry(Properties.Settings.Default.viewerAddress).AddressList[0];
		//	IPEndPoint ipe = new IPEndPoint(lclhst, Properties.Settings.Default.viewerPort);
		//	socketForViewer =
		//		new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		//	try
		//	{
		//		socketForViewer.Connect(ipe);
		//	}
		//	catch
		//	{
		//		//MessageBox.Show(ex.ToString()); 
		//		return false;
		//	}
		//	return true;
		//}

		[HandleProcessCorruptedStateExceptions]
        public unsafe void process16bitImage(double min, double max, int imageWidth, int imageHeight, string fileName, ushort *buf1, ushort *buf2, ushort *buf3, ushort *buf4, Boolean wDoubleImage, bool isFluo)
		{
			//if (wDoubleImage)
			//{
			//	imageHeight /= 2; // then we have two images in the raw image height <-- this makes no sense at all -- the size of the (final, and that is what matters here) pic is always the same, and the raw(!) image height is never given?
			//}

            long ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

			byte[] image1 = new byte[imageWidth * imageHeight * 2]; // NB : size = image size * byte per pixel (here we have 16bits GreyScale = 2 byte per pixel)
			byte[] image2 = new byte[imageWidth * imageHeight * 2];
            byte[] image3 = new byte[imageWidth * imageHeight * 2];
            byte[] image4 = new byte[imageWidth * imageHeight * 2];
			byte[] OD = new byte[imageWidth * imageHeight * 2];
            UInt16 im1_buff = 0, im2_buff = 0, im3_buff = 0, im4_buff = 0;

            //byte[] byteArray = new byte[2];

            //UInt16* pbuf1, pbuf2, pbuf3, pbuf4;

            //pbuf1 = &buf1;
            //pbuf2 = &buf2;
            //pbuf3 = &buf3;
            //pbuf4 = &buf4;

            //Console.WriteLine(Convert.ToString(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - ms) +"ms elapsed ...");
			
            int i;
			for (i = 0; i < imageWidth * imageHeight; i++)
			{
				try
				{
					//for pointer operations ...
					if (!wDoubleImage)	//no DoubleImage
					{
						im1_buff = (UInt16)buf1[i];
						im2_buff = (UInt16)buf2[i];
						im3_buff = (UInt16)buf3[i];
						im4_buff = (UInt16)buf4[i];
					}
					else
					{
						im1_buff = (UInt16)buf1[i];
						im2_buff = (UInt16)buf1[imageWidth * imageHeight + i];
						im3_buff = (UInt16)buf2[i];
						im4_buff = (UInt16)buf2[imageWidth * imageHeight + i];
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Error in process16bitImage while executing pointer operations.");
					System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "pointer.txt", e.ToString() + ",,,," + i.ToString() + ",");
					break;
				}

				if (autoSubDark)
				{
					if (subDarkImg1 == 2) im1_buff = (ushort)Math.Abs(im1_buff - im4_buff);
					else im1_buff = (ushort)Math.Abs(im1_buff - im3_buff);						//for every other value use default: img1 - img3;

					if (subDarkImg2 == 1) im2_buff = (ushort)Math.Abs(im2_buff - im3_buff);
					else im2_buff = (ushort)Math.Abs(im2_buff - im4_buff);						//for every other value use default: img2 - img4;
				}
				try
				{
					//processing images

					// Image 1 :
					//byteArray[0] = ((byte*)buf1)[2 * i];											//Using Pointer Operations
					//byteArray[1] = ((byte*)buf1)[2 * i + 1];
					image1[2 * i + 0] = (byte)im1_buff;													//Using Shifts
					image1[2 * i + 1] = (byte)(im1_buff >> 8);											//Little Endian: Low bits into low byte, high bits into high byte. 
																										//For Tiff: First two byte "II": Little Endian (intel machines), that's our case here. The uShort is then encoded Little Endian too, so no need for conversion.

					//byteArray = BitConverter.GetBytes(im1_buff);									//

					//image1[6 * i + 0] = byteArray[0];    //R
					//image1[6 * i + 1] = byteArray[1];//R

					//image1[6 * i + 2] = byteArray[0];//= BitConverter.GetBytes(im1_buff)[0];//G
					//image1[6 * i + 3] = byteArray[1];//= BitConverter.GetBytes(im1_buff)[1];//G

					//image1[6 * i + 4] = byteArray[0];//= BitConverter.GetBytes(im1_buff)[0];//B
					//image1[6 * i + 5] = byteArray[1];//= BitConverter.GetBytes(im1_buff)[1];//B

					// Image 2 :
					//byteArray[0] = ((byte*)buf1)[2 * (imageWidth * imageHeight + i)];
					//byteArray[1] = ((byte*)buf1)[2 * (imageWidth * imageHeight + i) + 1];
					image2[2 * i + 0] = (byte)im2_buff;
					image2[2 * i + 1] = (byte)(im2_buff >> 8);
					//byteArray = BitConverter.GetBytes(im2_buff);

					//image2[6 * i] = byteArray[0];// BitConverter.GetBytes(im2_buff)[0];
					//image2[6 * i + 1] = byteArray[1]; //BitConverter.GetBytes(im2_buff)[1];

					//image2[6 * i + 2] = byteArray[0];// BitConverter.GetBytes(im2_buff)[0];
					//image2[6 * i + 3] = byteArray[1]; //BitConverter.GetBytes(im2_buff)[1];

					//image2[6 * i + 4] = byteArray[0]; // BitConverter.GetBytes(im2_buff)[0];
					//image2[6 * i + 5] = byteArray[1]; //BitConverter.GetBytes(im2_buff)[1];

					// Image 3 :
					//byteArray[0] = ((byte*)buf2)[2 * i];
					//byteArray[1] = ((byte*)buf2)[2 * i + 1];
					image3[2 * i + 0] = (byte)im3_buff;
					image3[2 * i + 1] = (byte)(im3_buff >> 8);
					//byteArray = BitConverter.GetBytes(im3_buff);

					//image3[6 * i] = byteArray[0];//BitConverter.GetBytes(im3_buff)[0];
					//image3[6 * i + 1] = byteArray[1];//BitConverter.GetBytes(im3_buff)[1];

					//image3[6 * i + 2] = byteArray[0];//BitConverter.GetBytes(im3_buff)[0];
					//image3[6 * i + 3] = byteArray[1];//BitConverter.GetBytes(im3_buff)[1];

					//image3[6 * i + 4] = byteArray[0];//BitConverter.GetBytes(im3_buff)[0];
					//image3[6 * i + 5] = byteArray[1];//BitConverter.GetBytes(im3_buff)[1];

					// Image 4 :
					//byteArray[0] = ((byte*)buf2)[2 * (imageWidth * imageHeight + i)];
					//byteArray[1] = ((byte*)buf2)[2 * (imageWidth * imageHeight + i) + 1];
					image4[2 * i + 0] = (byte)im4_buff;
					image4[2 * i + 1] = (byte)(im4_buff >> 8);
					//byteArray = BitConverter.GetBytes(im4_buff);

					//image4[6 * i] = byteArray[0];//BitConverter.GetBytes(im4_buff)[0];
					//image4[6 * i + 1] = byteArray[1];//BitConverter.GetBytes(im4_buff)[1];

					//image4[6 * i + 2] = byteArray[0];//BitConverter.GetBytes(im4_buff)[0];
					//image4[6 * i + 3] = byteArray[1];//BitConverter.GetBytes(im4_buff)[1];

					//image4[6 * i + 4] = byteArray[0];//BitConverter.GetBytes(im4_buff)[0];
					//image4[6 * i + 5] = byteArray[1];//BitConverter.GetBytes(im4_buff)[1];
				}
				catch (Exception e)
				{
					Console.WriteLine("Error in process16bitImage while processing images.");
					System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "processing.txt", e.ToString());
					break;
				}
			}

            //Console.WriteLine(Convert.ToString(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - ms) + "ms elapsed ...");
            Console.WriteLine("\n   (Note: Starting background saving process ...)\n");

            Thread cameraSaveImageThread = new Thread(() => saveImage(imageWidth, imageHeight, fileName, image1, image2, image3, image4, OD));
            cameraSaveImageThread.Start();


            //Console.WriteLine(Convert.ToString(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - ms) + "ms elapsed ...");
		}

		[HandleProcessCorruptedStateExceptions]
        private void saveImage(int imageWidth, int imageHeight, string fileNameRaw, byte[] image1, byte[] image2, byte[] image3, byte[] image4, byte[] OD)
        {
            //String fileNameRaw = fileName; //fileName.Replace(@"\Pictures\", @"\Pictures\RAW\");
			String fileName;
			long ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			//Console.WriteLine(Convert.ToString(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - ms) + "ms elapsed (1) ...");

			#region saveOD
			// Saving OD :

            //if (showOD)
            //{
            //    Bitmap bmpOD = new Bitmap(imageWidth, imageHeight, PixelFormat.Format48bppRgb);
            //    Rectangle dim = new Rectangle(0, 0, bmpOD.Width, bmpOD.Height);

            //    try
            //    {
            //        BitmapData picdata = bmpOD.LockBits(dim, ImageLockMode.ReadWrite, bmpOD.PixelFormat);
            //        Marshal.Copy(OD, 0, picdata.Scan0, OD.Length);
            //        bmpOD.UnlockBits(picdata);

            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show("Error while trying to create optical density:/n" + e.ToString());
            //        return;
            //    }
            //    try
            //    {
            //        bmpOD.Save(fileNameRaw.Replace(".tif", "_OD.tif"));
            //    }
            //    catch
            //    {
            //        MessageBox.Show("Error while saving picture " + shotID);
            //    }
            //}

            // Save raw images

            //if (showRaw)
            //{
            //    
			//}
			#endregion

			if (tiff)
			{
				//Save as TIFF

				try
				{
					int stride = (imageWidth * 16 + 7) / 8;				//Stride = difference in array pos between two image rows: (ImageWidth * bpp + 7)/8
					TiffCompressOption compr = TiffCompressOption.None;
					if (tiffCompress) compr = TiffCompressOption.Zip;

					fileName = fileNameRaw.Replace(".tif", "_atoms.tif");
					if (saveThisImage && System.IO.File.Exists(fileName))
					{
						Console.WriteLine("\n\n\n   !! Warning !!\n\n   Filename " + fileName + " already exists!");

						while (System.IO.File.Exists(fileName)) fileName = fileName + ".bck";

						Console.WriteLine("   Saving as " + fileName + "\n\n   Likely the program is corrupted, please restart.\n\n\n");
					}

					BitmapSource bmp1Source = BitmapSource.Create(imageWidth, imageHeight, 96, 96, System.Windows.Media.PixelFormats.Gray16, null, image1, stride);
					FileStream bmp1stream = new FileStream(fileName, FileMode.Create);
					TiffBitmapEncoder encoder1 = new TiffBitmapEncoder();
					encoder1.Compression = compr;//TiffCompressOption.None;
					encoder1.Frames.Add(BitmapFrame.Create(bmp1Source));
					encoder1.Save(bmp1stream);

					fileName = fileNameRaw.Replace(".tif", "_noatoms.tif");
					if (saveThisImage && System.IO.File.Exists(fileName))
					{
						Console.WriteLine("\n\n\n   !! Warning !!\n\n   Filename " + fileName + " already exists!");

						while (System.IO.File.Exists(fileName)) fileName = fileName + ".bck";

						Console.WriteLine("   Saving as " + fileName + "\n\n   Likely the program is corrupted, please restart.\n\n\n");
					}
					BitmapSource bmp2Source = BitmapSource.Create(imageWidth, imageHeight, 96, 96, System.Windows.Media.PixelFormats.Gray16, null, image2, stride);
					FileStream bmp2stream = new FileStream(fileName, FileMode.Create);
					TiffBitmapEncoder encoder2 = new TiffBitmapEncoder();
					encoder2.Compression = compr;//TiffCompressOption.None;
					encoder2.Frames.Add(BitmapFrame.Create(bmp2Source));
					encoder2.Save(bmp2stream);

					if (saveDark)
					{
						fileName = fileNameRaw.Replace(".tif", "_dark1.tif");
						if (saveThisImage && System.IO.File.Exists(fileName))
						{
							Console.WriteLine("\n\n\n   !! Warning !!\n\n   Filename " + fileName + " already exists!");

							while (System.IO.File.Exists(fileName)) fileName = fileName + ".bck";

							Console.WriteLine("   Saving as " + fileName + "\n\n   Likely the program is corrupted, please restart.\n\n\n");
						}
						BitmapSource bmp3Source = BitmapSource.Create(imageWidth, imageHeight, 96, 96, System.Windows.Media.PixelFormats.Gray16, null, image3, stride);
						FileStream bmp3stream = new FileStream(fileName, FileMode.Create);
						TiffBitmapEncoder encoder3 = new TiffBitmapEncoder();
						encoder3.Compression = compr;//TiffCompressOption.None;
						encoder3.Frames.Add(BitmapFrame.Create(bmp3Source));
						encoder3.Save(bmp3stream);

						fileName = fileNameRaw.Replace(".tif", "_dark2.tif");
						if (saveThisImage && System.IO.File.Exists(fileName))
						{
							Console.WriteLine("\n\n\n   !! Warning !!\n\n   Filename " + fileName + " already exists!");

							while (System.IO.File.Exists(fileName)) fileName = fileName + ".bck";

							Console.WriteLine("   Saving as " + fileName + "\n\n   Likely the program is corrupted, please restart.\n\n\n");
						}
						BitmapSource bmp4Source = BitmapSource.Create(imageWidth, imageHeight, 96, 96, System.Windows.Media.PixelFormats.Gray16, null, image4, stride);
						FileStream bmp4stream = new FileStream(fileName, FileMode.Create);
						TiffBitmapEncoder encoder4 = new TiffBitmapEncoder();
						encoder4.Compression = compr;//TiffCompressOption.None;
						encoder4.Frames.Add(BitmapFrame.Create(bmp4Source));
						encoder4.Save(bmp4stream);

						bmp3stream.Close();
						bmp4stream.Close();

						bmp3Source.Dispatcher.InvokeShutdown();
						bmp4Source.Dispatcher.InvokeShutdown();
					}

					bmp1stream.Close();
					bmp2stream.Close();

					bmp1Source.Dispatcher.InvokeShutdown();
					bmp2Source.Dispatcher.InvokeShutdown();
				}
				catch (Exception e)
				{
					Console.WriteLine("Error in saveImage while saving 48bitRGB TIFF");
					System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "saving.txt", "TIFF: " + e.ToString());
				}

				//Console.WriteLine(Convert.ToString(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - ms) + "ms elapsed (3) ...");
			}

			else if (png)
			{
				// Save as PNG

				try
				{
					int stride = (imageWidth * 16 + 7) / 8;				//Stride = difference in array pos between two image rows: (ImageWidth * bpp + 7)/8

					fileName = fileNameRaw.Replace(".png", "_atoms.png");
					if (saveThisImage && System.IO.File.Exists(fileName))
					{
						Console.WriteLine("\n\n\n   !! Warning !!\n\n   Filename " + fileName + " already exists!");

						while (System.IO.File.Exists(fileName)) fileName = fileName + ".bck";

						Console.WriteLine("   Saving as " + fileName + "\n\n   Likely the program is corrupted, please restart.\n\n\n");
					}
					BitmapSource bmp1Source = BitmapSource.Create(imageWidth, imageHeight, 96, 96, System.Windows.Media.PixelFormats.Gray16, null, image1, stride);
					FileStream bmp1stream = new FileStream(fileName, FileMode.Create);
					PngBitmapEncoder encoder1 = new PngBitmapEncoder();
					encoder1.Frames.Add(BitmapFrame.Create(bmp1Source));
					encoder1.Save(bmp1stream);

					fileName = fileNameRaw.Replace(".png", "_noatoms.png");
					if (saveThisImage && System.IO.File.Exists(fileName))
					{
						Console.WriteLine("\n\n\n   !! Warning !!\n\n   Filename " + fileName + " already exists!");

						while (System.IO.File.Exists(fileName)) fileName = fileName + ".bck";

						Console.WriteLine("   Saving as " + fileName + "\n\n   Likely the program is corrupted, please restart.\n\n\n");
					}
					BitmapSource bmp2Source = BitmapSource.Create(imageWidth, imageHeight, 96, 96, System.Windows.Media.PixelFormats.Gray16, null, image2, stride);
					FileStream bmp2stream = new FileStream(fileName, FileMode.Create);
					PngBitmapEncoder encoder2 = new PngBitmapEncoder();
					encoder2.Frames.Add(BitmapFrame.Create(bmp2Source));
					encoder2.Save(bmp2stream);

					if (saveDark)
					{
						fileName = fileNameRaw.Replace(".png", "_dark1.png");
						if (saveThisImage && System.IO.File.Exists(fileName))
						{
							Console.WriteLine("\n\n\n   !! Warning !!\n\n   Filename " + fileName + " already exists!");

							while (System.IO.File.Exists(fileName)) fileName = fileName + ".bck";

							Console.WriteLine("   Saving as " + fileName + "\n\n   Likely the program is corrupted, please restart.\n\n\n");
						}
						BitmapSource bmp3Source = BitmapSource.Create(imageWidth, imageHeight, 96, 96, System.Windows.Media.PixelFormats.Gray16, null, image3, stride);
						FileStream bmp3stream = new FileStream(fileName, FileMode.Create);
						PngBitmapEncoder encoder3 = new PngBitmapEncoder();
						encoder3.Frames.Add(BitmapFrame.Create(bmp3Source));
						encoder3.Save(bmp3stream);

						fileName = fileNameRaw.Replace(".png", "_dark2.png");
						if (saveThisImage && System.IO.File.Exists(fileName))
						{
							Console.WriteLine("\n\n\n   !! Warning !!\n\n   Filename " + fileName + " already exists!");

							while (System.IO.File.Exists(fileName)) fileName = fileName + ".bck";

							Console.WriteLine("   Saving as " + fileName + "\n\n   Likely the program is corrupted, please restart.\n\n\n");
						}
						BitmapSource bmp4Source = BitmapSource.Create(imageWidth, imageHeight, 96, 96, System.Windows.Media.PixelFormats.Gray16, null, image4, stride);
						FileStream bmp4stream = new FileStream(fileName, FileMode.Create);
						PngBitmapEncoder encoder4 = new PngBitmapEncoder();
						encoder4.Frames.Add(BitmapFrame.Create(bmp4Source));
						encoder4.Save(bmp4stream);

						bmp3stream.Close();
						bmp4stream.Close();

						bmp3Source.Dispatcher.InvokeShutdown();
						bmp4Source.Dispatcher.InvokeShutdown();

					}

					bmp1stream.Close();
					bmp2stream.Close();

					bmp1Source.Dispatcher.InvokeShutdown();
					bmp2Source.Dispatcher.InvokeShutdown();
				}
				catch (Exception e)
				{
					Console.WriteLine("Error in saveImage while saving 48bitRGB PNG");
					System.IO.File.WriteAllText(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "saving.txt", "PNG: " + e.ToString());
				}

				//Console.WriteLine(Convert.ToString(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - ms) + "ms elapsed (2) ...");
			}

            Console.WriteLine("\r   (Note: Saving done.)\n");
        }

		public unsafe byte[] create8bitImage(int imageWidth, int imageHeight, ushort* buf, int offset)
		{
			byte[] image1 = new byte[imageWidth * imageHeight];
			for (int i = 0; i < imageWidth * imageHeight; i++)
			{
				image1[i] = (byte)(buf[i + offset] * 255.0 / 65535.0);
			}

			return image1;

		}

		static unsafe UInt16[] MarshalUInt16(ushort* obj, int len)
		{
			UInt16[] arr = new UInt16[len];
			for (int i = 0; i < len / 2; i++)
			{
				arr[i] = (UInt16)obj[i];
			}
			return arr;
		}

 

	#region Unused functions
	
		public void processODFrom8bit(double min, double max, int imageWidth, int imageHeigth, string fileName, byte[] image1Buffer, byte[] image2Buffer, bool isFluo)
		{
			//    Rectangle rect = new Rectangle(0, 0, imageWidth, imageHeigth);

			//    if (Properties.Settings.Default.showRawImages)
			//    {
			//        Debug.Write("Saving abs images \n");
			//        //Bitmap bmp1 = new Bitmap((int)w, (int)h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			//        //Bitmap bmp2 = new Bitmap((int)w, (int)h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			//        String fileNameRaw = fileName.Replace(@"\Pictures\", @"\Pictures\RAW\");
			//        Bitmap bmp1 = new Bitmap(imageWidth, imageHeigth, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			//        Bitmap bmp2 = new Bitmap(imageWidth, imageHeigth, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

			//        //safe code: lock the memory before referencing it
			//        BitmapData bmpData1 = bmp1.LockBits(rect, ImageLockMode.ReadWrite,
			//            bmp1.PixelFormat);
			//        // Get the address of the first line.

			//        IntPtr ptr1 = bmpData1.Scan0;
			//        // Copy the RGB values into the array.
			//        Marshal.Copy(image1Buffer, 0, ptr1, image1Buffer.Length);
			//        //unlock memory (not freeing)
			//        bmp1.UnlockBits(bmpData1);

			//        // bmp1.Save(fileName.Replace(".png", "_1.png"),  // OLD Version
			//        //     System.Drawing.Imaging.ImageFormat.Png);

			//        bmp1.Save(fileNameRaw.Replace(".png", "_1.png"),
			//            System.Drawing.Imaging.ImageFormat.Png);

			//        //safe code: lock the memory before referencing it
			//        BitmapData bmpData2 = bmp2.LockBits(rect, ImageLockMode.ReadWrite,
			//            bmp2.PixelFormat);
			//        // Get the address of the first line.
			//        IntPtr ptr2 = bmpData2.Scan0;
			//        // Copy the RGB values into the array.
			//        Marshal.Copy(image2Buffer, 0, ptr2, image2Buffer.Length);
			//        //unlock memory (not freeing)
			//        bmp2.UnlockBits(bmpData2);
			//        //TODO: hack the tEXt chunk
			//        //bmpOD.Tag = "Max=" + max.ToString() + ";Min=" + min.ToString();

			//        //bmp2.Save(fileName.Replace(".png", "_2.png"),
			//        //    System.Drawing.Imaging.ImageFormat.Png);

			//        bmp2.Save(fileNameRaw.Replace(".png", "_2.png"),
			//            System.Drawing.Imaging.ImageFormat.Png);
			//    }

			//    byte[] OD = new byte[(image1Buffer).Length];
			//    if (!isFluo)
			//    {
			//        for (int i = 0; i < OD.Length; i++)
			//        {
			//            double logi = Math.Log((double)image2Buffer[i] / (double)image1Buffer[i]);
			//            if (logi > min)
			//                if (logi < max)
			//                    OD[i] = (byte)(255.0 / (max - min) * (logi - min));
			//                else
			//                    OD[i] = 255;
			//            else
			//                OD[i] = 0;
			//        }
			//    }
			//    else
			//    {
			//        for (int i = 0; i < OD.Length; i++)
			//        {
			//            int diff = Convert.ToInt32(image1Buffer[i]) - Convert.ToInt32(image2Buffer[i]);
			//            if (diff < 0)
			//                diff = 0;
			//            OD[i] = (byte)diff;
			//        }
			//    }
			//    Bitmap bmpOD = new Bitmap(imageWidth, imageHeigth, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

			//    try
			//    {
			//        //safe code: lock the memory before referencing it
			//        BitmapData bmpData = bmpOD.LockBits(rect, ImageLockMode.ReadWrite,
			//            bmpOD.PixelFormat);
			//        // Get the address of the first line.
			//        IntPtr ptr = bmpData.Scan0;
			//        // Copy the RGB values into the array.
			//        Marshal.Copy(OD, 0, ptr, OD.Length);
			//        //unlock memory (not freeing)
			//        bmpOD.UnlockBits(bmpData);
			//        //TODO: hack the tEXt chunk
			//        //bmpOD.Tag = "Max=" + max.ToString() + ";Min=" + min.ToString();
			//    }
			//    catch (Exception e)
			//    {
			//        MessageBox.Show("Error while trying to create optical density:/n" +
			//            e.ToString());
			//        return;
			//    }
			//    try
			//    {
			//        bmpOD.Save(fileName,
			//            System.Drawing.Imaging.ImageFormat.Png);
			//    }
			//    catch //(Exception e)
			//    {
			//        MessageBox.Show("Error while saving picture " + shot_folder_name);
			//    }
		}

		public void processODFrom16bit(double min, double max, int imageWidth, int imageHeigth, string fileName, byte[] image1Buffer, byte[] image2Buffer, bool isFluo)
		{
			//Rectangle rect = new Rectangle(0, 0, imageWidth, imageHeigth);


			//byte[] OD = new byte[(image1Buffer).Length];
			//if (isFluo)
			//{
			//    for (int i = 0; i < OD.Length; i += 2)
			//    {
			//        double logi = Math.Log((double)BitConverter.ToUInt16(image2Buffer, i) / (double)BitConverter.ToUInt16(image1Buffer, i));
			//        if (logi > min)
			//            if (logi < max)
			//            {
			//                OD[i] = BitConverter.GetBytes(Convert.ToUInt16(65535 / (max - min) * (logi - min)))[0];
			//                OD[i + 1] = BitConverter.GetBytes(Convert.ToUInt16(65535 / (max - min) * (logi - min)))[1];
			//            }
			//            else
			//            {
			//                OD[i] = 255; OD[i + 1] = 255;
			//            }
			//        else
			//            OD[i] = 0; OD[i + 1] = 0;
			//    }
			//}
			//else
			//{
			//    for (int i = 0; i < OD.Length; i += 2)
			//    {
			//        short diff = (short)(BitConverter.ToUInt16(image1Buffer, i) - BitConverter.ToUInt16(image2Buffer, i));
			//        if (diff < 0)
			//            diff = 0;
			//        OD[i] = BitConverter.GetBytes(diff)[0];
			//        OD[i + 1] = BitConverter.GetBytes(diff)[1];
			//    }
			//}
			//Bitmap bmpOD = new Bitmap(imageWidth, imageHeigth, System.Drawing.Imaging.PixelFormat.Format48bppRgb);

			//try
			//{
			//    //safe code: lock the memory before referencing it
			//    BitmapData bmpData = bmpOD.LockBits(rect, ImageLockMode.ReadWrite,
			//        bmpOD.PixelFormat);
			//    // Get the address of the first line.
			//    IntPtr ptr = bmpData.Scan0;
			//    // Copy the RGB values into the array.
			//    Marshal.Copy(OD, 0, ptr, OD.Length);
			//    //unlock memory (not freeing)
			//    bmpOD.UnlockBits(bmpData);
			//    //TODO: hack the tEXt chunk
			//    //bmpOD.Tag = "Max=" + max.ToString() + ";Min=" + min.ToString();
			//}
			//catch (Exception e)
			//{
			//    MessageBox.Show("Error while trying to create optical density:/n" +
			//        e.ToString());
			//    return;
			//}
			//try
			//{
			//    bmpOD.Save(fileName,
			//        System.Drawing.Imaging.ImageFormat.Png);
			//}
			//catch //(Exception e)
			//{
			//    MessageBox.Show("Error while saving picture " + shot_folder_name);
			//}
		}

	
		private string customFirewireCameraName(string rawFirewireName)
		{
			//string res = "";
			//if (rawFirewireName.Contains("A102"))
			//    res = "basler";
			//if (rawFirewireName.Contains("Guppy"))
			//    res = "guppy";
			//if (System.Environment.MachineName == "CAMERA")
			//    res += "";
			//if (System.Environment.MachineName == "MACMINI2")
			//    res += "2";
			//return res;
			return "not implemented";
		}

	#endregion

	}

}
