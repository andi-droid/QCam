using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QCam
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            this.settingsPropertyGrid.SelectedObject = Properties.Settings.Default;
        }

		private void Settings_FormClosed(object sender, FormClosedEventArgs e)
		{
			FormMain.Form1.updateAxisBox();
		}

		private string[][] getFiles(string folder)
		{
			var directory = new System.IO.DirectoryInfo(folder);
			var id = 0;

			if (directory == null || !directory.Exists)
				return new string[1][] { new String[] {"No file found.", "" }};

			System.IO.FileInfo[] files = directory.GetFiles("user.config", System.IO.SearchOption.AllDirectories);

			//MessageBox.Show(files[0].Directory.ToString());

			string[][] fileList = new string[files.Length][];
			for (int i = 0; i < files.Length; i++)
			{
				fileList[i] = new String[2];
				fileList[i][1] = files[i].Directory.ToString().Substring(folder.Length) + @"\" + files[i].Name;
				fileList[i][0] = files[i].LastWriteTime.ToString();
			}

			return fileList;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\" + Application.CompanyName;

			string[][] fileList = getFiles(basePath);

			Form settingsLoad = new Form();
			settingsLoad.FormBorderStyle = FormBorderStyle.FixedDialog;
			settingsLoad.Width = 600;
			settingsLoad.Height = 200;
			settingsLoad.MinimizeBox = false;
			settingsLoad.MaximizeBox = false;
			settingsLoad.TopMost = true;
			settingsLoad.ShowInTaskbar = false;
			settingsLoad.StartPosition = FormStartPosition.CenterScreen;
			
			Label labelSelLeft = new Label();
			labelSelLeft.Width = 70;
			labelSelLeft.Height = 30;
			labelSelLeft.Location = new Point(90, 130);
			labelSelLeft.Text = "No selection";
			labelSelLeft.TextAlign = ContentAlignment.MiddleCenter;

			Label labelSelRight = new Label();
			labelSelRight.Width = 70;
			labelSelRight.Height = 30;
			labelSelRight.Location = new Point(334 + 90, 130);
			labelSelRight.Text = "No Selection";
			labelSelRight.TextAlign = ContentAlignment.MiddleCenter;

			ListView leftFile = new ListView();
			leftFile.Width = 250;
			leftFile.Height = 120;
			leftFile.View = View.Details;
			leftFile.MultiSelect = false;
			leftFile.Columns.Add("DateTime", 120);
			leftFile.Columns.Add("Path (NEW)", 126);
			leftFile.SelectedIndexChanged += (s_sender, s_e) => { try { labelSelLeft.Text = leftFile.SelectedItems[0].SubItems[0].Text; } catch (Exception) { labelSelLeft.Text = "No selection"; } };

			Label labelShift = new Label();
			labelShift.Width = 60;
			labelShift.Height = 20;
			labelShift.Location = new Point(262, 82);
			labelShift.Text = "◄=";
			labelShift.TextAlign = ContentAlignment.MiddleCenter;
			labelShift.Font = new Font("Arial", 20);

			ListView rightFile = new ListView();
			rightFile.Width = 250;
			rightFile.Height = 120;
			rightFile.Location = new Point(334,0);
			rightFile.View = View.Details;
			rightFile.MultiSelect = false;
			rightFile.Columns.Add("DateTime", 120);
			rightFile.Columns.Add("Path (OLD)", 126);
			rightFile.SelectedIndexChanged += (s_sender, s_e) => { try { labelSelRight.Text = rightFile.SelectedItems[0].SubItems[0].Text; } catch (Exception) { labelSelRight.Text = "No selection"; } };

			Button buttonSaveClose = new Button();
			buttonSaveClose.Width = 150;
			buttonSaveClose.Height = 20;
			buttonSaveClose.Text = "Copy selected file && close";
			buttonSaveClose.Location = new Point(217,135);
			buttonSaveClose.Click += (s_sender, s_e) => {
				if (leftFile.SelectedItems.Count > 0 && rightFile.SelectedItems.Count > 0)
					System.IO.File.Copy(basePath + rightFile.SelectedItems[0].SubItems[1].Text, basePath + leftFile.SelectedItems[0].SubItems[1].Text, true);
				settingsLoad.Close(); };

			settingsLoad.FormClosed += (s_sender, s_e) => { Properties.Settings.Default.Reload(); this.settingsPropertyGrid.SelectedObject = Properties.Settings.Default; this.Show(); };
			settingsLoad.Controls.Add(leftFile);
			settingsLoad.Controls.Add(labelShift);
			settingsLoad.Controls.Add(rightFile);
			settingsLoad.Controls.Add(labelSelLeft);
			settingsLoad.Controls.Add(labelSelRight);
			settingsLoad.Controls.Add(buttonSaveClose);

			for (int i = 0; i < fileList.Length; i++)
			{
				leftFile.Sorting = SortOrder.Descending;
				rightFile.Sorting = SortOrder.Descending;
				leftFile.Items.Add(new ListViewItem(fileList[i]));
				rightFile.Items.Add(new ListViewItem(fileList[i]));
			}
			leftFile.Sort();
			rightFile.Sort();


			this.Hide();
			settingsLoad.ShowDialog();
		}
    }
}
