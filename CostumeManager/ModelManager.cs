using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrawlLib.OpenGL;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlCostumeManager {
	public partial class ModelManager : UserControl {
		/// <summary>
		/// The string between "Fit" and the number.
		/// </summary>
		private string _charString;
		/// <summary>
		/// In case the file needs to be reloaded.
		/// </summary>
		private string _path;

	    /// <summary>
	    /// Should be disposed when you switch to a new file.
	    /// </summary>
		ResourceNode _root;

		public ResourceNode WorkingRoot {
			get {
				return _root;
			}
		}

		public Size? ModelPreviewSize {
			get {
				return Dock == DockStyle.Fill ? (Size?)null : modelPanel1.Size;
			}
			set {
				if (value == null) {
					modelPanel1.Dock = DockStyle.Fill;
				} else {
					modelPanel1.Dock = DockStyle.None;
					modelPanel1.Size = value.Value;
				}
			}
		}

		public bool ZoomOut;
		public bool UseExceptions;

		private string _delayedPath;

		public ModelManager() {
			InitializeComponent();
			UseExceptions = true;

			modelPanel1.DragEnter += modelPanel1_DragEnter;
			modelPanel1.DragDrop += modelPanel1_DragDrop;
		}

		void modelPanel1_DragEnter(object sender, DragEventArgs e) {
			if (_path != null && e.Data.GetDataPresent(DataFormats.FileDrop)) {
				string[] s = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (s.Length == 1) { // Can only drag and drop one file
					string filename = s[0].ToLower();
					if (filename.EndsWith(".pac") || filename.EndsWith(".pcs")) {
						e.Effect = DragDropEffects.Copy;
					}
				}
			}
		}

		void modelPanel1_DragDrop(object sender, DragEventArgs e) {
			if (e.Effect == DragDropEffects.Copy) {
				string newpath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
				this.BeginInvoke(new Action(() => {
					using (ResourceNode newroot = NodeFactory.FromFile(null, newpath)) {
						if (newroot is ARCNode) {
							string basePath = _path;
							if (Path.HasExtension(basePath)) {
								basePath = basePath.Substring(0, basePath.LastIndexOf('.'));
							}
							FileInfo pac = new FileInfo(basePath + ".pac");
							FileInfo pcs = new FileInfo(basePath + ".pcs");

							bool cont = true;
							if (pac.Exists || pcs.Exists) {
								cont = (DialogResult.OK == MessageBox.Show(
									"Replace " + pac.Name + "/" + pcs.Name + "?",
									"Overwrite?",
									MessageBoxButtons.OKCancel));
							}
							if (!cont) return;

							if (_root != null) {
								_root.Dispose();
								_root = null;
							}
							pac.Directory.Create();
							(newroot as ARCNode).ExportPAC(pac.FullName);
							(newroot as ARCNode).ExportPCS(pcs.FullName);

							if (ParentForm is CostumeManager) {
								(ParentForm as CostumeManager).updateCostumeSelectionPane();
							}

							LoadFile(_path);
						} else {
							MessageBox.Show("Invalid format: root node is not an ARC archive.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
				}));
			}
		}

		public void LoadFileDelayed(string delayedPath) {
			this._delayedPath = delayedPath;
			if (!String.IsNullOrWhiteSpace(_delayedPath)) {
				var tmp_timer = new System.Timers.Timer(1000);
				tmp_timer.AutoReset = false;
				tmp_timer.Elapsed += new System.Timers.ElapsedEventHandler(initializeModelPanel);
				tmp_timer.Enabled = true;
			}
		}

		private void initializeModelPanel(object o, System.Timers.ElapsedEventArgs target) {
			LoadFile(_delayedPath);
		}

		public void RefreshModel() {
			LoadFile(_path);
		}

		public void LoadFile(string path) {
			if (_root != null) {
				_root.Dispose();
				_root = null;
			}
			if (path == null) {
				return;
			}

			_path = path;
			_charString = getCharString(path);

			comboBox1.Items.Clear();
			modelPanel1.ClearAll();
			modelPanel1.Invalidate();
			this.Text = new FileInfo(path).Name;

			try {
				//if (!File.Exists(path)) path = Settings.Default.FallbackBrawlRoot + '\\' + path.Substring(path.IndexOf("fighter"));
				_root = NodeFactory.FromFile(null, path);
				List<MDL0Node> models = findAllMDL0s(_root);
				if (models.Count > 0) {
					comboBox1.Items.AddRange(models.ToArray());
					comboBox1.SelectedIndex = 0;
				}
			} catch (IOException) {
				
			}
		}

		private static string getCharString(string path) {
			string working = path.ToLower();
			working = working.Substring(working.LastIndexOf("fit")+3);
			int length = 0;
			foreach (char c in working) {
				if (c >= '0' && c <= '9') {
					break;
				} else {
					length++;
				}
			}
			working = working.Substring(0, length);
			return working;
		}

		public void LoadModel(MDL0Node model) {
			model.Populate();
			model.ResetToBindState();

			modelPanel1.ClearAll();
			modelPanel1.AddTarget((IRenderedObject)model);

			if (UseExceptions && PolygonsToDisable.ContainsKey(_charString)) {
				foreach (string polygonNum in PolygonsToDisable[_charString]) {
					MDL0ObjectNode poly = model.PolygonGroup.FindChild(polygonNum, false) as MDL0ObjectNode;
					if (poly != null) poly.IsRendering = false;
				}
			}

			Box box = model.GetBox();
			Vector3 min = box.Min, max = box.Max;
			if (ZoomOut) {
				min._x += 20;
				max._x -= 20;
			}
			modelPanel1.SetCamWithBox(min, max);
		}

		private List<MDL0Node> findAllMDL0s(ResourceNode root) {
			List<MDL0Node> list = new List<MDL0Node>();
			if (root is MDL0Node) {
				list.Add((MDL0Node)root);
			} else {
				foreach (ResourceNode node in root.Children) {
					list.AddRange(findAllMDL0s(node));
				}
			}
			return list;
		}

		private MDL0Node findFirstMDL0(ResourceNode root) {
			if (root is MDL0Node) {
				return (MDL0Node)root;
			}
			foreach (ResourceNode node in root.Children) {
				MDL0Node result = findFirstMDL0(node);
				if (result != null) {
					return result;
				}
			}
			return null;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
			object item = comboBox1.SelectedItem;
			if (item is MDL0Node) {
				LoadModel(item as MDL0Node);
			}
		}

		public static Dictionary<string, string[]> PolygonsToDisable = new Dictionary<string, string[]> {
			{ "mario", new string[] { "polygon4", "polygon5", "polygon6", "polygon7", "polygon12", "polygon13", "eyehurt", "facehurt", "hair_full", "FaceOuch", "EyeOuch" } }, // open eyelids
			{ "samus", new string[] { "Sphere", "Ball", "BallSpider", "BallLED", "BallCore", "BallHazard", "BallHazardLED", "SphereGold", "polygon15", "ball", "balllight", "polygon19", "polygon20" } }, // remove morph ball
            { "yoshi", new string[] {"polygon8", "polygon10" } }, // remove Final Smash eyes
			{ "kirby", new string[] { "polygon2", "polygon0", "polygon1", "polygon6", "polygon7" } }, // remove Final Smash eyes
            { "fox", new string[] { "polygon9", "polygon10", "polygon11", "polygon12", "polygon13", "polygon14", "polygon15", "polygon16", "polygon20", "polygon21", "polygon22", "eyeLyellow", "eyeRyellow", "HeadBlink", "HeadOuch", "HeadHalfBlink", "eyes_half_blink", "eyes_half_blink2", "fur_blink", "face_blink", "fur_ouch", "face_ouch" } }, // open eyelids			
			{ "luigi", new string[] { "polygon5", "polygon8", "polygon9", "polygon10", "polygon11", "polygon12", "polygon17", "EyeYellowL", "EyeYellowR", "Damage_Eye", "Eyebrow_Damage", "Damage_Face", "FaceHurt", "MaskHurt", "Hair_NoHat", "MaskFS", "Hair_Full", "Damage_Eyes", "Damage_Eyebrows", "Damage_Face" } }, // open eyelids
			{ "ness", new string[] { "polygon1", "polygon2", "polygon5", "polygon6", "polygon1_BurnHead", "polygon2BurnHead" } }, // remove wild "intro" hair and FS eyes
			{ "koopa", new string[] { "polygon22", "polygon21", "polygon20", "polygon19" } }, // remove shell
			{ "zelda", new string[] { "polygon19", "polygon21", "polygon25", "face_halfblink", "face_blink", "Blink", "Blink_Half", "Ouch", "face_ouch_" } }, // open eyelids
			{ "marth", new string[] { "FaceBlinkHalf", "FaceBlink", "FaceOuch", "MouthTalk", "polygon8","polygon9", "polygon10", "polygon11", "polygon13", "polygon14" } }, // open eyelids
			{ "metaknight", new string[] { "kage" } }, // remove shadow
            { "pit", new string[] { "polygon14","polygon15", "polygon16", "polygon17", "polygon19", "polygon20", "polygon22", "polygon23", "polygon28", "polygon29", "polygon31", "polygon32", "Blink", "Mouth_Open", "Hurt", "Yellow_Eyes", "Half_Blink", "Angry" } }, // open eyelids
            { "szerosuit", new string[] { "polygon3", "polygon4", "polygon5", "polygon6", "polygon8", "polygon9", "polygon14", "polygon15", "polygon17", "polygon18", "Face HalfBlink", "Eyelashes HalfBlink", "Face Blink", "Eyelashes Blink", "Face Ouch", "Eyelashes Ouch", "Mouth Talk" } }, // open eyelids
			{ "pikmin", new string[] {"polygon5", "helm", "helmet", "eyes_hurt", "Eyebuggy" } }, // "close" eyelids (normal facial expression for Olimar)
            { "mewtwo", new string[] { "polygon6", "polygon7", "polygon10", "polygon11", "polygon12", "polygon13", "EyelidsBlinkHalf", "EyelidsBlink" } }, // open eyelids
			{ "dedede", new string[] { "polygon12", "polygon13","polygon18", "polygon19", "newbloat","SwellBelt","puff1S", "puff2S", "puff3S", "BodyBLOAT" } }, // remove inflated Dedede
            { "lucario", new string[] { "polygon32", "polygon33", "polygon35", "polygon36" } }, // remove close eyelids
			{ "ike", new string[] { "polygon7", "polygon8", "polygon9", "polygon10", "polygon11", "polygon15", "polygon18", "polygon19", "polygon20", "polygon24", "polygon36", "polygon37", "polygon38", "polygon42", "polygon45", "polygon46", "polygon47", "polygon51", "FaceHalfBlinkAlpha", "FaceBlinkAlpha", "FaceTalkAlpha", "FaceOuchAlpha", "FaceHalfBlink", "FaceBlink", "FaceTalk", "FaceOuch", "Face BlinkHalf", "Neck BlinkHalf", "Face Blink", "Neck Blink", "Face Ouch", "Neck Ouch", "Face Talk", "Neck Talk" } }, // open eyelids
            { "roy", new string[] { "Roy_polygon7RY", "Roy_polygon8LY", "Half_Blink", "Mouth_Open", "hurt", "Blink", "FaceHalf", "FaceBlink", "FaceOuch", "FaceTalk", "EyeRYellow", "EyeLYellow", "TalkM_Eyes", "BlinkM_Eyes", "OuchM_Eyes", "BlinkHalf_Eyes", "OuchM_Mouth", "TalkM_Mouth", "TalkM_Alpha", "BlinkHalfM_Alpha", "OuchM_Alpha", "BlinkM_Alpha" } }, // open eyelids
            { "knuckles", new string[] { "Angry_Mouth", "Anrgy_Brows", "Angry_Brows", "Surprise_Mouth", "Surprise_Brows", "Fun_Mouth", "Fun_Brows", "Sad_Mouth", "Sad_Brows", "Grin_Mouth", "No_Mouth", "LidsClosed", "LidsHalf", "Sphere" } }, // remove expressions and sphere
			{ "wolf", new string[] { "EyeYellow", "AngryWolfLeftEar", "AngryWolfFace", "AngryEyepatchBand", "BlinkWolfLeftEar3", "BlinkWolfFace", "BlinkEyepatchBand", "HalfWolfFace", "HalfWolfLeftEar4", "HalfEyepatchBand7", "polygon8", "polygon9", "polygon10", "polygon11", "polygon28", "polygon29", "polygon30", "polygon31", "polygon32", "polygon33", "polygon50", "polygon51", "polygon52", "polygon53", "polygon76", "polygon77", "polygon94", "polygon95", "polygon96", "polygon97", "polygon98", "polygon99" } }, // open eyelids
            { "snake", new string[] { "face_halfblink", "eyelash_halfblink1", "eyelash_halfblink2", "face_blink", "eyelash_blink1", "eyelash_blink2", "face_ouch", "eyelash_ouch1", "eyelash_ouch2", "face_talk", "eyelash_talk1", "eyelash_talk2", "face_angry", "eyelash_angry1", "eyelash_angry2", "polygon0", "polygon1", "polygon2", "polygon3", "polygon4", "polygon5", "polygon6", "polygon7", "polygon8", "polygon9", "polygon10", "polygon14", "polygon15", "polygon16", "polygon17", "polygon18", "polygon19", "eyelash_angry", "eyelash_halfblink", "eyelash_blink", "eyelash_ouch", "face_ouch", "face_blink", "face_angry", "face_talk", "face_blink_half" } }, // open eyelids
			{ "sonic", new string[] {"polygon14", "polygon15", "polygon16", "polygon18", "polygon20", "polygon21", "polygon22", "polygon24", "polygon26", "polygon27", "polygon28", "polygon29", "polygon30", "polygon31", "polygon30", "Sphere02", "Sphere04" } }, // open eyelids, remove sphere, etc.
        };

		public Bitmap GrabScreenshot(bool withTransparency) {
			modelPanel1.Refresh();
			return modelPanel1.GetScreenshot(modelPanel1.ClientRectangle, withTransparency);
		}
	}
}
