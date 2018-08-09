﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrawlManagerLib;
using System.Reflection;

namespace BrawlSongManager {
	[DefaultEvent("ValueChanged")]
	public partial class CustomSongVolumeEditor : UserControl {
		private static Image SPEAKER = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("BrawlSongManager.speaker.png"));

		public bool ChangeMadeSinceCSVLoaded {get; private set;}

		private CustomSongVolumeCodeset _csv;
		public CustomSongVolumeCodeset CSV {
			get {
				return _csv;
			}
			set {
				_csv = value;
				ChangeMadeSinceCSVLoaded = false;
				reload();
			}
		}

		private string _basenameRequested;
		private Song _song;
		public Song Song {
			get {
				return _song;
			}
			set {
				_basenameRequested = value == null ? null : value.Filename;
				_song = value;
				reload();
			}
		}
		public string SongFilename {
			get {
				return _basenameRequested;
			}
			set {
				_basenameRequested = value;
				_song = value == null
					? null
					: SongIDMap.Songs.Where(s => s.Filename == value).FirstOrDefault();
				reload();
			}
		}

		public byte Value {
			get {
				return (byte)nudVolume.Value;
			}
			set {
				nudVolume.Value = value;
			}
		}

		public Image VolumeIcon {
			get {
				return pictureBox1.BackgroundImage;
			}
			set {
				pictureBox1.BackgroundImage = value == null ? value : BitmapUtilities.Resize((Bitmap)value, pictureBox1.Size);
			}
		}

		public string VolumeToolTip {
            get {
                return toolTip1.GetToolTip(pictureBox1);
            }
			set {
				toolTip1.SetToolTip(pictureBox1, value);
			}
		}

		public event EventHandler ValueChanged;

		public CustomSongVolumeEditor() {
			InitializeComponent();
		}

		private void reload() {
			this.VolumeToolTip = null;
			this.VolumeIcon = null;
			btnAdd.Text = "Add";

			lblSongID.Text =
				_basenameRequested == null ? ""
				: Song == null ? "Playback volume:"
				: Song.ID.ToString("X4");

			if (_basenameRequested == null) {
                this.VolumeToolTip = "No song selected";
                btnAdd.Visible = false;
				nudVolume.Enabled = false;
			} else if (Song == null) {
				this.VolumeToolTip = "Filename not recognized - volume will only affect playback in this program and will not be saved";
				this.VolumeIcon = SPEAKER;

				btnAdd.Visible = false;
				nudVolume.Enabled = true;
				nudVolume.Value = 80;
			} else if (CSV == null) {
				this.VolumeToolTip = "No GCT file found in: " + string.Join(", ", MainForm.GCT_PATHS);
				this.VolumeIcon = SPEAKER;

				btnAdd.Visible = false;
				nudVolume.Enabled = true;
				nudVolume.Value = Song.DefaultVolume ?? 0;
			} else if (CSV.Settings.ContainsKey(Song.ID)) {
				this.VolumeToolTip = "Custom Song Volume code set";

				btnAdd.Text = "Remove";
				btnAdd.Visible = true;
				nudVolume.Enabled = true;
				nudVolume.Value = CSV.Settings[Song.ID];
			} else if (Song.DefaultVolume == null) {
				this.VolumeToolTip = "Default volume unknown";
				this.VolumeIcon = SystemIcons.Warning.ToBitmap();

				btnAdd.Visible = true;
				nudVolume.Enabled = false;
				nudVolume.Value = 80;
			} else {
				this.VolumeToolTip = $"Default volume known: {Song.DefaultVolume}";
				btnAdd.Visible = true;
				nudVolume.Enabled = false;
				nudVolume.Value = Song.DefaultVolume ?? 0;
			}
		}

		private void btnAdd_Click(object sender, EventArgs e) {
			if (CSV.Settings.ContainsKey(Song.ID)) {
				CSV.Settings.Remove(Song.ID);
			} else {
				CSV.Settings.Add(Song.ID, (byte)nudVolume.Value);
			}
			ChangeMadeSinceCSVLoaded = true;
			reload();
		}

		public void SetVolume(byte b) {
			if (!nudVolume.Enabled) {
				MessageBox.Show("The volume editor control is not enabled. You might need to click the 'Add' button if there is one.");
			} else if (Song == null) {
				MessageBox.Show("No song is selected.");
			} else if (CSV == null) {
				MessageBox.Show("No Custom Song Volume code is loaded.");
			} else {
				byte oldval = CSV.Settings[Song.ID];
				if (oldval != Value) {
					ChangeMadeSinceCSVLoaded = true;
					CSV.Settings[Song.ID] = Value;
				}
			}
		}

		private void nudVolume_ValueChanged(object sender, EventArgs e) {
			// Don't update the CSV code if the song is unknown (in which case the number spinner acts only as a playback control)
			if (nudVolume.Enabled && Song != null && CSV != null) {
				byte oldval = CSV.Settings[Song.ID];
				if (oldval != Value) {
					ChangeMadeSinceCSVLoaded = true;
					CSV.Settings[Song.ID] = Value;
				}
			}
			if (ValueChanged != null) ValueChanged(this, new EventArgs());
		}
	}
}
