﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSSEditor {
	static class Program {
		private static string gct, pac;
		private static void findFiles(string[] args) {
			args = args ?? new string[0];
			gct = args.Length > 0 ? args[0]
				: File.Exists(@"data\gecko\codes\RSBE01.gct") ? @"data\gecko\codes\RSBE01.gct"
				: File.Exists(@"codes\RSBE01.gct") ? @"codes\RSBE01.gct"
				: File.Exists(@"LegacyTE\RSBE01.gct") ? @"LegacyTE\RSBE01.gct"
                : File.Exists(@"LegacyXP\RSBE01.gct") ? @"LegacyXP\RSBE01.gct"
                : File.Exists(@"RSBE01.gct") ? @"RSBE01.gct"
                : null;
			pac = args.Length > 1 ? args[1]
				: File.Exists(@"private\wii\app\RSBE\pf\menu2\sc_selmap.pac") ? @"private\wii\app\RSBE\pf\menu2\sc_selmap.pac"
				: File.Exists(@"projectm\pf\menu2\sc_selmap.pac") ? @"projectm\pf\menu2\sc_selmap.pac"
                : File.Exists(@"minusery\pf\menu2\sc_selmap.pac") ? @"minusery\pf\menu2\sc_selmap.pac"
				: File.Exists(@"private\wii\app\RSBE\pf\system\common5.pac") ? @"private\wii\app\RSBE\pf\system\common5.pac"
				: File.Exists(@"projectm\pf\system\common5.pac") ? @"projectm\pf\system\common5.pac"
				: File.Exists(@"minusery\pf\system\common5.pac") ? @"minusery\pf\system\common5.pac"
                : File.Exists(@"LegacyTE\pf\menu2\sc_selmap.pac") ? @"LegacyTE\pf\menu2\sc_selmap.pac"
                : File.Exists(@"LegacyXP\pf\menu2\sc_selmap.pac") ? @"LegacyXP\pf\menu2\sc_selmap.pac"
                : null;
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try {
				accessLibraries();
			} catch (Exception e) {
				MessageBox.Show(null, e.Message, e.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			findFiles(args);

			Application.Run(new SSSEditorForm(gct, pac));
		}

		private static void accessLibraries() {
			var D = BrawlLib.Properties.Settings.Default;
			if (D.GetType().GetProperty("HideMDL0Errors") != null) {
				D.GetType().InvokeMember("HideMDL0Errors", System.Reflection.BindingFlags.SetProperty, null, D, new object[] { true });
			}
		}
	}
}
