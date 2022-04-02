﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BrawlCostumeManager {
	public class PortraitMap {
		#region static
		public static string[] KirbyHats = {
			"donkey", "falco", "mewtwo", "pikmin", "purin", "snake"
		};

		public class Fighter {
			public string Name { get; private set; }
			public int CharBustTexIndex { get; private set; }
			//public int? FighterIndex, CSSSlot;

			public Fighter(string Name, int CharBustTexIndex) {
				this.Name = Name;
				this.CharBustTexIndex = CharBustTexIndex;
			}

			public Fighter(string Name, int CharBustTexIndex, int FighterIndex, int CSSSlot)
			: this(Name, CharBustTexIndex) {
				//this.FighterIndex = FighterIndex;
				//this.CSSSlot = CSSSlot;
			}
		}

		/// <summary>
		/// Name/index pairs that are known to be used in Brawl or Project M 3.0.
		/// </summary>
		private static Fighter[] KnownFighters = {
			new Fighter("mario", 0),
			new Fighter("donkey", 1),
			new Fighter("link", 2),
			new Fighter("samus", 3),
			new Fighter("yoshi", 4),
			new Fighter("kirby", 5),
			new Fighter("fox", 6),
			new Fighter("pikachu", 7),
			new Fighter("luigi", 8),
			new Fighter("captain", 9),
			new Fighter("ness", 10),
			new Fighter("koopa", 11),
			new Fighter("peach", 12),
			new Fighter("zelda", 13),
			new Fighter("sheik", 14),
			new Fighter("popo", 15),
			new Fighter("marth", 16),
			new Fighter("gamewatch", 17),
			new Fighter("falco", 18),
			new Fighter("ganon", 19),
			new Fighter("metaknight", 21),
			new Fighter("pit", 22),
			new Fighter("szerosuit", 23),
			new Fighter("pikmin", 24),
			new Fighter("lucas", 25),
			new Fighter("diddy", 26),
			new Fighter("mewtwo", 27), // PM 3.0
			new Fighter("poketrainer", 27),
			new Fighter("pokelizardon", 28),
			new Fighter("pokezenigame", 29),
			new Fighter("pokefushigisou", 30),
			new Fighter("dedede", 31),
			new Fighter("lucario", 32),
			new Fighter("ike", 33),
			new Fighter("robot", 34),
			new Fighter("purin", 36),
			new Fighter("wario", 37),
            new Fighter("roy", 39), // PM 3.0
			new Fighter("toonlink", 40),
            new Fighter("knuckles", 42), // P+ & Knuckles
            new Fighter("wolf", 43),
            new Fighter("gkoopa", 44), // PM 3.6
			new Fighter("snake", 45),
            new Fighter("sonic", 46),
            new Fighter("warioman", 66), // PM 3.6
		};

		// Fighter, CSSSlot, Cosmetic
		private static readonly int[][] IndexMappings = {
			new int[] {0x00, 0x00, 0x00},
			new int[] {0x01, 0x01, 0x01},
			new int[] {0x02, 0x02, 0x02},
			new int[] {0x03, 0x03, 0x03},
			new int[] {0x04, 0x05, 0x04},
			new int[] {0x05, 0x06, 0x05},
			new int[] {0x06, 0x07, 0x06},
			new int[] {0x07, 0x08, 0x07},
			new int[] {0x08, 0x09, 0x08},
			new int[] {0x09, 0x0A, 0x09},
			new int[] {0x0A, 0x0B, 0x0A},
			new int[] {0x0B, 0x0C, 0x0B},
			new int[] {0x0C, 0x0D, 0x0C},
			new int[] {0x0D, 0x0E, 0x0D},
			new int[] {0x0E, 0x0F, 0x0E},
			new int[] {0x0F, 0x10, 0x0F},
			new int[] {0x11, 0x11, 0x10},
			new int[] {0x12, 0x12, 0x11},
			new int[] {0x13, 0x13, 0x12},
			new int[] {0x14, 0x14, 0x13},
			new int[] {0x16, 0x16, 0x15},
			new int[] {0x17, 0x17, 0x16},
			new int[] {0x18, 0x04, 0x17},
			new int[] {0x19, 0x18, 0x18},
			new int[] {0x1A, 0x19, 0x19},
			new int[] {0x1B, 0x1A, 0x1A},
			new int[] {0x1C, 0x1B, 0x1B},
			new int[] {0x1D, 0x1C, 0x1C},
			new int[] {0x1E, 0x1D, 0x1D},
			new int[] {0x1F, 0x1E, 0x1E},
			new int[] {0x20, 0x1F, 0x1F},
			new int[] {0x21, 0x20, 0x20},
			new int[] {0x22, 0x21, 0x21},
			new int[] {0x23, 0x22, 0x22},
			new int[] {0x25, 0x23, 0x23},
			new int[] {0x15, 0x15, 0x14},
			new int[] {0x29, 0x24, 0x25},
			new int[] {0x2C, 0x25, 0x27},
			new int[] {0x2E, 0x26, 0x28},
			new int[] {0x2F, 0x27, 0x29},
		};

		private static int? GetCSSSlot(int fighterIndex) {
			if (fighterIndex >= 0x3F) {
				return fighterIndex;
			}
			var q = from a in IndexMappings
					where a[0] == fighterIndex
					select a[1];
			if (q.Any()) {
				return q.First();
			}
			return null;
		}

		private static int? GetCosmeticSlot(int fighterIndex) {
			if (fighterIndex >= 0x3F) {
				return fighterIndex;
			}
			var q = from a in IndexMappings
					where a[0] == fighterIndex
					select a[2];
			if (q.Any()) {
				return q.First();
			}
			return null;
		}

		private static Dictionary<int, int[]> BrawlMappings = new Dictionary<int, int[]>() {
            {0, new int[] {0,6,3,4,5,2}},
            {1, new int[] {0,4,1,3,2,5}},
            {2, new int[] {0,1,3,5,6,4}},
            {3, new int[] {0,3,1,5,4,2}},
            {4, new int[] {0,1,3,4,5,6}},
            {5, new int[] {0,4,3,1,2,5}},
            {6, new int[] {0,4,1,2,3,5}},
            {7, new int[] {0,1,2,3}},
            {8, new int[] {0,5,1,3,4,6}},
            {9, new int[] {0,4,1,2,3,5}},
            {10, new int[] {0,5,4,2,3,6}},
            {11, new int[] {0,4,1,3,5,6}},
            {12, new int[] {0,5,1,3,2,4}},
            {13, new int[] {0,1,3,5,2,4}},
            {14, new int[] {0,1,3,5,2,4}},
            {15, new int[] {0,1,3,4,2,5}},
            {16, new int[] {0,1,2,4,5,3}},
            {17, new int[] {0,1,2,3,4,5}},
            {18, new int[] {0,5,3,1,2,4}},
            {19, new int[] {0,4,3,2,1,5}},
            {21, new int[] {0,4,1,2,3,5}},
            {22, new int[] {0,4,1,2,3,5}},
            {23, new int[] {0,3,1,5,4,2}},
            {24, new int[] {0,4,1,5,2,3}},
            {25, new int[] {0,4,1,3,2,5}},
            {26, new int[] {0,5,4,6,2,3}},
            {27, new int[] {0,1,2,3,4}},
            {28, new int[] {0,1,2,3,4}},
            {29, new int[] {0,1,2,3,4}},
            {30, new int[] {0,1,2,3,4}},
            {31, new int[] {0,6,2,5,3,4}},
            {32, new int[] {0,1,4,5,2}},
            {33, new int[] {0,5,1,3,2,4}},
            {34, new int[] {0,6,5,4,3,2}},
            {36, new int[] {0,1,4,3,2}},
            {37, new int[] {0,1,5,2,4,3,6,7,9,8,10,11}},
            {40, new int[] {0,1,3,4,5,6}},
            {43, new int[] {0,1,4,2,3,5}},
            {44, new int[] {0,4,1,3,5,6}},
            {45, new int[] {0,1,3,4,2,5}},
            {46, new int[] {0,5,4,2,1}},
            {66, new int[] {0,1,5,2,4,3,6,7,9,8,10,11}},
        };

		private static Dictionary<int, int[]> PM35Mappings = CompilePM35Mappings();
        private static Dictionary<int, int[]> PPlusMappings = CompilePPlusMappings();
        private static Dictionary<int, int[]> S3CMappings = CompileS3CMappings();
        private static Dictionary<int, int[]> BPJMappings = CompileBPJMappings();


        private static Dictionary<int, int[]> CompilePM35Mappings() {
			Dictionary<int, int[]> ret = new Dictionary<int, int[]>();
			for (int key=0; key<=66; key++) {
				switch (key) {
                // Some of these characters have their portraits in a different order than Brawl, or have their additional portraits "out of order."
                case 0:
                    // Mario
                    ret.Add(key, new int[] { 0, 6, 3, 2, 5, 7, 11, 8, 9, 10, /*end*/ 1, 4 });
                    break;
                case 3:
                    // Samus
                    ret.Add(key, new int[] { 0, 3, 1, 4, 2, 5, 7, 8, 6 });
                    break;
                case 12:
                    // Fox
                    ret.Add(key, new int[] { 0, 5, 1, 3, 2, 4, 7, 6 });
                    break;
                case 23:
                    // Zero Suit Samus
                    ret.Add(key, new int[] { 0, 3, 1, 5, 4, 2, 7, 8, 6 });
                    break;
                case 27:
                    // Mewtwo - uses Pokémon Trainer's character index
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5 });
                    break;
                case 28:
                    // Charizard
                    ret.Add(key, new int[] { 0, 1, 3, 2, 4, 5, 6 });
                    break;
                case 29:
                    // Squirtle
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 6, 7 });
                    break;
                case 37:
                    // Wario
                    ret.Add(key, new int[] { 6, 7, 10, 8, 11, 9, 0, 1, 3, 2, 5, 4 });
                    break;
                case 39:
                    // Roy
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 6 });
                    break;
                case 46:
                    // Sonic
                    ret.Add(key, new int[] { 0, 1, 4, 2, 5, 6, 7, 8 });
                    break;
                case 66:
                    // Wario Man
                    ret.Add(key, new int[] { 6, 7, 10, 8, 11, 9, 0, 1, 3, 2, 5, 4, 13, 14 });
                    break;
                default:
						if (!BrawlMappings.ContainsKey(key)) continue;

						/* All other characters in PM 3.6 follow a pattern: the portraits start out in the same order Brawl has them,
						   and any additional portraits are in order after the highest-numbered original portrait. */
						int[] arr1 = BrawlMappings[key];
						int max = arr1.Max();
						int[] arr2 = new int[12];
						Array.Copy(arr1, arr2, arr1.Length);
						for (var i = arr1.Length; i < 12; i++) {
							arr2[i] = ++max;
						}
						ret.Add(key, arr2);
						break;
				}
			}
			return ret;
		}
        private static Dictionary<int, int[]> CompilePPlusMappings() {
            Dictionary<int, int[]> ret = new Dictionary<int, int[]>();
            for (int key = 0; key <= 66; key++) {
                switch (key) {
                case 0:
                    // Mario
                    ret.Add(key, new int[] { 0, 6, 3, 2, 5, 4, 20, 21, 22, 23, 24, 25, 30, 31, 32 });
                    break;
				case 1:
					// Donkey Kong
					ret.Add(key, new int[] { 0, 4, 1, 3, 2, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
				case 2:
					// Link
					ret.Add(key, new int[] { 0, 1, 3, 5, 6, 4, 7, 8, 9, 20, 21, 22, 23, 24, 25 });
                    break;
                case 3:
                    // Samus
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 20, 23, 27, 30, 31, 32, 33, 35, 36, 37 });
                    break;
				case 4:
                    // Yoshi
                    ret.Add(key, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 20, 21, 22 });
                    break;
				case 5:
                    // Kirby
                    ret.Add(key, new int[] { 0, 4, 3, 1, 2, 5, 6, 7, 8, 9, 10, 11, 20, 21, 22 });
                    break;
				case 6:
                    // Fox
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 7:
                    // Pikachu
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 7, 5, 6, 8, 9, 10, 11, 12, 13, 14 });
                    break;
				case 8:
                    // Luigi
                    ret.Add(key, new int[] { 0, 5, 1, 3, 4, 6, 7, 20, 21, 22, 23, 30, 31, 32, 33 });
                    break;
				case 9:
                    // Captain Falcon
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 20, 30, 31, 32, 33, 34 });
                    break;
				case 10:
                    // Ness
                    ret.Add(key, new int[] { 0, 5, 4, 2, 3, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
				case 11:
                    // Bowser
                    ret.Add(key, new int[] { 0, 4, 1, 3, 5, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 12:
                    // Peach
                    ret.Add(key, new int[] { 0, 5, 1, 3, 2, 4, 6, 7, 8, 20, 19, 30, 31, 32, 33 });
                    break;
				case 13:
                    // Zelda
                    ret.Add(key, new int[] { 0, 1, 3, 5, 2, 4, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 14:
                    // Sheik
                    ret.Add(key, new int[] { 0, 1, 3, 5, 2, 4, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 15:
                    // Ice Climbers
                    ret.Add(key, new int[] { 0, 1, 3, 4, 2, 5, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 16:
                    // Marth
                    ret.Add(key, new int[] { 0, 1, 2, 4, 5, 3, 6, 20, 21, 22, 23, 30, 31, 32, 33 });
                    break;
                case 17:
                    // Mr. Game & Watch
                    ret.Add(key, new int[] { 0, 1, 4, 3, 2, 5, 6, 7, 8, 9, 10, 11, 14, 15, 16 });
                    break;
                case 18:
                    // Falco
                    ret.Add(key, new int[] { 0, 5, 3, 1, 2, 4, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 19:
                    // Ganondorf
                    ret.Add(key, new int[] { 0, 4, 3, 2, 1, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 21:
                    // Metaknight
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 22:
                    // Pit
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 23:
                    // Zero Suit Samus
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 20, 23, 27, 30, 31, 32, 33, 35, 36, 37 });
                    break;
                case 24:
                    // Olimar
                    ret.Add(key, new int[] { 0, 4, 1, 5, 2, 3, 6, 7, 20, 21, 30, 31, 32, 33, 34 });
                    break;
                case 25:
                    // Lucas
                    ret.Add(key, new int[] { 0, 4, 1, 3, 2, 5, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 26:
                    // Diddy Kong
                    ret.Add(key, new int[] { 0, 5, 4, 6, 2, 3, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 27:
                    // Mewtwo - uses Pokémon Trainer's character index
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 28:
                    // Charizard
                    ret.Add(key, new int[] { 0, 1, 3, 2, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 29:
                    // Squirtle
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 30:
                    // Ivysaur
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 31:
                    // King Dedede
                    ret.Add(key, new int[] { 0, 6, 2, 5, 3, 4, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 32:
                    // Lucario
                    ret.Add(key, new int[] { 0, 1, 4, 5, 2, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 33:
                    // Ike
                    ret.Add(key, new int[] { 0, 5, 1, 3, 2, 4, 6, 20, 21, 22, 23, 30, 31, 32, 33 });
                    break;
                case 34:
                    // R.O.B.
                    ret.Add(key, new int[] { 0, 6, 5, 4, 3, 2, 20, 21, 22, 23, 30, 31, 32, 33, 34 });
                    break;
                case 36:
                    // Jigglypuff
                    ret.Add(key, new int[] { 0, 1, 4, 3, 2, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
                    break;
                case 37:
                    // Wario
                    ret.Add(key, new int[] { 6, 7, 10, 8, 11, 9, 0, 1, 3, 2, 5, 4, 20, 21, 22, 23, 24, 25 });
                    break;
                case 39:
                    // Roy
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 40:
                    // Toon Link
                    ret.Add(key, new int[] { 0, 1, 3, 4, 5, 6, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 42:
                    // Knuckles
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 20, 21, 22, 23 });
                    break;
                case 43:
                    // Wolf
                    ret.Add(key, new int[] { 0, 1, 4, 2, 3, 5, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 44:
                    // Giga Bowser
                    ret.Add(key, new int[] { 0, 4, 1, 3, 5, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                 case 45:
                    // Snake
                    ret.Add(key, new int[] { 0, 1, 3, 4, 2, 5, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 46:
                    // Sonic
                    ret.Add(key, new int[] { 0, 1, 4, 2, 5, 6, 7, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 66:
                    // Wario Man
                    ret.Add(key, new int[] { 6, 7, 10, 8, 11, 9, 0, 1, 3, 2, 5, 4, 20, 21, 22, 23, 24, 25 });
                    break;
                }
            }
            return ret;
        }
        private static Dictionary<int, int[]> CompileS3CMappings() {
            Dictionary<int, int[]> ret = new Dictionary<int, int[]>();
            for (int key = 0; key <= 66; key++) {
                switch (key) {
                case 0:
                    // Mario
                    ret.Add(key, new int[] { 0, 6, 3, 2, 5, 4, 7, 8, 20, 21, 22, 23, 24, 25, 26, 27, 30, 31, 32, 33 });
                    break;
				case 1:
					// Donkey Kong
					ret.Add(key, new int[] { 0, 4, 1, 3, 2, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
				case 2:
					// Link
					ret.Add(key, new int[] { 0, 1, 3, 5, 6, 4, 7, 8, 9, 20, 21, 22, 23, 24, 25, 30, 31, 32, 33, 34 });
                    break;
                case 3:
                    // Samus
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 20, 23, 27, 30, 31, 32, 33, 35, 36, 37, 40, 41, 42, 43 });
                    break;
				case 4:
                    // Yoshi
                    ret.Add(key, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 20, 21, 22 });
                    break;
				case 5:
                    // Kirby
                    ret.Add(key, new int[] { 0, 4, 3, 1, 2, 5, 6, 7, 8, 9, 10, 11, 20, 21, 22, 23, 30, 31, 32, 33 });
                    break;
				case 6:
                    // Fox
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 7:
                    // Pikachu
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 6, 21, 22, 23, 24 });
                    break;
				case 8:
                    // Luigi
                    ret.Add(key, new int[] { 0, 5, 1, 3, 4, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
				case 9:
                    // Captain Falcon
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 20, 30, 31, 32, 33, 34 });
                    break;
				case 10:
                    // Ness
                    ret.Add(key, new int[] { 0, 5, 4, 2, 3, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
				case 11:
                    // Bowser
                    ret.Add(key, new int[] { 0, 4, 1, 3, 5, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 12:
                    // Peach
                    ret.Add(key, new int[] { 0, 5, 1, 3, 2, 4, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
				case 13:
                    // Zelda
                    ret.Add(key, new int[] { 0, 1, 3, 5, 2, 4, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 14:
                    // Sheik
                    ret.Add(key, new int[] { 0, 1, 3, 5, 2, 4, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 15:
                    // Ice Climbers
                    ret.Add(key, new int[] { 0, 1, 3, 4, 2, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 16:
                    // Marth
                    ret.Add(key, new int[] { 0, 1, 2, 4, 5, 3, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 17:
                    // Mr. Game & Watch
                    ret.Add(key, new int[] { 0, 1, 4, 3, 2, 5, 6, 7, 8, 9, 10, 11, 14, 15, 16 });
                    break;
                case 18:
                    // Falco
                    ret.Add(key, new int[] { 0, 5, 3, 1, 2, 4, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 19:
                    // Ganondorf
                    ret.Add(key, new int[] { 0, 4, 3, 2, 1, 5, 6, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 35 });
                    break;
                case 21:
                    // Metaknight
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 22:
                    // Pit
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 23:
                    // Zero Suit Samus
                    ret.Add(key, new int[] { 0, 3, 1, 4, 2, 5, 20, 25, 27, 30, 31, 32, 33, 35, 36, 37, 40, 41, 42, 43 });
                    break;
                case 24:
                    // Olimar
                    ret.Add(key, new int[] { 0, 4, 1, 5, 2, 3, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 25:
                    // Lucas
                    ret.Add(key, new int[] { 0, 4, 1, 3, 2, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 26:
                    // Diddy Kong
                    ret.Add(key, new int[] { 0, 5, 4, 6, 2, 3, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 27:
                    // Mewtwo - uses Pokémon Trainer's character index
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 28:
                    // Charizard
                    ret.Add(key, new int[] { 0, 1, 3, 2, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 29:
                    // Squirtle
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 30:
                    // Ivysaur
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 31:
                    // King Dedede
                    ret.Add(key, new int[] { 0, 6, 2, 5, 3, 4, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 32:
                    // Lucario
                    ret.Add(key, new int[] { 0, 1, 4, 5, 2, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 33:
                    // Ike
                    ret.Add(key, new int[] { 0, 5, 1, 3, 2, 4, 6, 7, 20, 21, 22, 23, 24, 25, 30, 31, 32, 33, 34, 35 });
                    break;
                case 34:
                    // R.O.B.
                    ret.Add(key, new int[] { 0, 6, 5, 4, 3, 2, 7, 8, 9, 10, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 36:
                    // Jigglypuff
                    ret.Add(key, new int[] { 0, 1, 4, 3, 2, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
                    break;
                case 37:
                    // Wario
                    ret.Add(key, new int[] { 6, 7, 10, 8, 11, 9, 12, 0, 1, 3, 2, 5, 4, 13, 20, 21, 22, 23, 24, 25 });
                    break;
                case 39:
                    // Roy
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 6, 20, 21, 22, 23, 30, 31, 32, 33 });
                    break;
                case 40:
                    // Toon Link
                    ret.Add(key, new int[] { 0, 1, 3, 4, 5, 6, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 42:
                    // Knuckles
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 20, 21, 22, 23 });
                    break;
                case 43:
                    // Wolf
                    ret.Add(key, new int[] { 0, 1, 4, 2, 3, 5, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 44:
                    // Giga Bowser
                    ret.Add(key, new int[] { 0, 4, 1, 3, 5, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                 case 45:
                    // Snake
                    ret.Add(key, new int[] { 0, 1, 3, 4, 2, 5, 6, 7, 20, 21, 22, 23, 24, 25, 30, 31, 32, 33, 34, 35 });
                    break;
                case 46:
                    // Sonic
                    ret.Add(key, new int[] { 0, 1, 4, 2, 5, 6, 7, 20, 21, 22, 23, 30, 31, 32, 33 });
                    break;
                case 66:
                    // Wario Man
                    ret.Add(key, new int[] { 6, 7, 10, 8, 11, 9, 12, 0, 1, 3, 2, 5, 4, 13, 20, 21, 22, 23, 24, 25 });
                    break;
                }
            }
            return ret;
        }

        private static Dictionary<int, int[]> CompileBPJMappings() {
            Dictionary<int, int[]> ret = new Dictionary<int, int[]>();
            for (int key = 0; key <= 66; key++) {
                switch (key) {
                case 0:
                    // Mario
                    ret.Add(key, new int[] { 0, 6, 3, 2, 5, 4, 7, 8, 20, 21, 22, 23, 24, 25, 26, 27, 30, 31, 32, 33 });
                    break;
				case 1:
					// Donkey Kong
					ret.Add(key, new int[] { 0, 4, 1, 3, 2, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
				case 2:
					// Link
					ret.Add(key, new int[] { 0, 1, 3, 5, 6, 4, 7, 8, 9, 20, 21, 22, 23, 24, 25, 30, 31, 32, 33, 34 });
                    break;
                case 3:
                    // Samus
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 20, 23, 27, 30, 31, 32, 33, 35, 36, 37, 40, 41, 42, 43 });
                    break;
				case 4:
                    // Yoshi
                    ret.Add(key, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 20, 21, 22 });
                    break;
				case 5:
                    // Kirby
                    ret.Add(key, new int[] { 0, 4, 3, 1, 2, 5, 6, 7, 8, 9, 10, 11, 20, 21, 22, 23, 30, 31, 32, 33 });
                    break;
				case 6:
                    // Fox
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 7:
                    // Pikachu
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 8, 5, 7, 9, 10, 11, 12, 13, 14, 15, 6, 21, 22, 23, 24 });
                    break;
				case 8:
                    // Luigi
                    ret.Add(key, new int[] { 0, 5, 1, 3, 4, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
				case 9:
                    // Captain Falcon
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 20, 30, 31, 32, 33, 34 });
                    break;
				case 10:
                    // Ness
                    ret.Add(key, new int[] { 0, 5, 4, 2, 3, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
				case 11:
                    // Bowser
                    ret.Add(key, new int[] { 0, 4, 1, 3, 5, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 12:
                    // Peach
                    ret.Add(key, new int[] { 0, 5, 1, 3, 2, 4, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
				case 13:
                    // Zelda
                    ret.Add(key, new int[] { 0, 1, 3, 5, 2, 4, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 14:
                    // Sheik
                    ret.Add(key, new int[] { 0, 1, 3, 5, 2, 4, 6, 7, 8, 20, 21, 22, 23, 24, 25 });
                    break;
                case 15:
                    // Ice Climbers
                    ret.Add(key, new int[] { 0, 1, 3, 4, 2, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 16:
                    // Marth
                    ret.Add(key, new int[] { 0, 1, 2, 4, 5, 3, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 40, 41, 42, 43, 44 });
                    break;
                case 17:
                    // Mr. Game & Watch
                    ret.Add(key, new int[] { 0, 1, 4, 3, 2, 5, 6, 7, 8, 9, 10, 11, 14, 15, 16 });
                    break;
                case 18:
                    // Falco
                    ret.Add(key, new int[] { 0, 5, 3, 1, 2, 4, 6, 7, 8, 20, 21, 22, 23, 24, 25, 26 });
                    break;
                case 19:
                    // Ganondorf
                    ret.Add(key, new int[] { 0, 4, 3, 2, 1, 5, 6, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 35 });
                    break;
                case 21:
                    // Metaknight
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 22:
                    // Pit
                    ret.Add(key, new int[] { 0, 4, 1, 2, 3, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 23:
                    // Zero Suit Samus
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 20, 25, 27, 30, 31, 32, 33, 35, 36, 37, 40, 41, 42, 43 });
                    break;
                case 24:
                    // Olimar
                    ret.Add(key, new int[] { 0, 4, 1, 5, 2, 3, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 25:
                    // Lucas
                    ret.Add(key, new int[] { 0, 4, 1, 3, 2, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 26:
                    // Diddy Kong
                    ret.Add(key, new int[] { 0, 5, 4, 6, 2, 3, 7, 8, 9, 10, 20, 21, 22, 23, 24, 25 });
                    break;
                case 27:
                    // Mewtwo - uses Pokémon Trainer's character index
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 28:
                    // Charizard
                    ret.Add(key, new int[] { 0, 1, 3, 2, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 29:
                    // Squirtle
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 30:
                    // Ivysaur
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 20, 21, 22, 23, 24 });
                    break;
                case 31:
                    // King Dedede
                    ret.Add(key, new int[] { 0, 6, 2, 5, 3, 4, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 32:
                    // Lucario
                    ret.Add(key, new int[] { 0, 1, 4, 5, 2, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                case 33:
                    // Ike
                    ret.Add(key, new int[] { 0, 5, 1, 3, 2, 4, 6, 7, 20, 21, 22, 23, 24, 25, 30, 31, 32, 33, 34, 35, 40, 41, 42, 43 });
                    break;
                case 34:
                    // R.O.B.
                    ret.Add(key, new int[] { 0, 6, 5, 4, 3, 2, 7, 8, 9, 10, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34 });
                    break;
                case 36:
                    // Jigglypuff
                    ret.Add(key, new int[] { 0, 1, 4, 3, 2, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
                    break;
                case 37:
                    // Wario
                    ret.Add(key, new int[] { 6, 7, 10, 8, 11, 9, 12, 0, 1, 3, 2, 5, 4, 13, 20, 21, 22, 23, 24, 25 });
                    break;
                case 39:
                    // Roy
                    ret.Add(key, new int[] { 0, 2, 1, 3, 4, 5, 6, 7, 8, 20, 21, 22, 23, 24, 25, 30, 31, 32, 33 });
                    break;
                case 40:
                    // Toon Link
                    ret.Add(key, new int[] { 0, 1, 3, 4, 5, 6, 20, 21, 22, 23, 24, 30, 31, 32, 33, 40, 41, 42, 43 });
                    break;
                case 42:
                    // Knuckles
                    ret.Add(key, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 20, 21, 22, 23 });
                    break;
                case 43:
                    // Wolf
                    ret.Add(key, new int[] { 0, 1, 4, 2, 3, 5, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 44:
                    // Giga Bowser
                    ret.Add(key, new int[] { 0, 4, 1, 3, 5, 6, 7, 8, 9, 10, 20, 21, 22, 23, 24 });
                    break;
                 case 45:
                    // Snake
                    ret.Add(key, new int[] { 0, 1, 3, 4, 2, 5, 6, 7, 20, 21, 22, 23, 24, 25, 30, 31, 32, 33, 34, 35 });
                    break;
                case 46:
                    // Sonic
                    ret.Add(key, new int[] { 0, 1, 4, 2, 5, 6, 7, 20, 21, 22, 23, 24, 30, 31, 32, 33 });
                    break;
                case 66:
                    // Wario Man
                    ret.Add(key, new int[] { 6, 7, 10, 8, 11, 9, 12, 0, 1, 3, 2, 5, 4, 13, 20, 21, 22, 23, 24, 25 });
                    break;
                }
            }
            return ret;
        }
        #endregion

        #region special subclasses
        public class ProjectM : PortraitMap {
			public ProjectM()
				: base() {
				foreach (int i in PM35Mappings.Keys) {
					this.AddPortraitMappings(i, PM35Mappings[i]);
				}
			}
		}
        public class Brawl : PortraitMap
        {
            public Brawl()
                : base() {
                foreach (int i in BrawlMappings.Keys) {
                    this.AddPortraitMappings(i, BrawlMappings[i]);
                }
            }
        }
        public class S3C : PortraitMap
        {
            public S3C()
                : base() {
                foreach (int i in S3CMappings.Keys) {
                    this.AddPortraitMappings(i, S3CMappings[i]);
                }
            }
        }
        public class BPJ : PortraitMap
        {
            public BPJ()
                : base() {
                foreach (int i in BPJMappings.Keys) {
                    this.AddPortraitMappings(i, BPJMappings[i]);
                }
            }
        }
        public class CBliss : PortraitMap {
			public override bool ContainsMapping(int index) {
				/* Do not check PortraitToCostumeMappings if cBliss is selected.
				   The program will be "uncertain" of all portrait mappings (yellow label) and
				   it will look for each portrait at the costume index - which is how cBliss works. */
				return additionalMappings.ContainsKey(index);
			}
		}
		#endregion

		private List<Fighter> additionalFighters;
		private Dictionary<int, int[]> additionalMappings;

		public PortraitMap() {
			additionalFighters = new List<Fighter>();
			additionalMappings = new Dictionary<int, int[]>();
		}

		public int CharBustTexFor(string name) {
			return (from f in additionalFighters
					where f.Name == name
					select (int?)f.CharBustTexIndex).FirstOrDefault()
				?? (from f in KnownFighters
					where f.Name == name
					select (int?)f.CharBustTexIndex).FirstOrDefault()
				?? -1;
		}

		public IEnumerable<string> GetKnownFighterNames() {
			return (from f in KnownFighters select f.Name)
				.Concat(from f in additionalFighters select f.Name)
				.Distinct();
		}

		public virtual bool ContainsMapping(int index) {
			return additionalMappings.ContainsKey(index) || PPlusMappings.ContainsKey(index);
		}

		public int[] GetPortraitMappings(int charBustTexIndex) {
			int[] arr;
			bool b = additionalMappings.TryGetValue(charBustTexIndex, out arr)
				|| PPlusMappings.TryGetValue(charBustTexIndex, out arr);
			return arr;
		}

		private int GetCharBustTexIndex(string name) {
			var q = additionalFighters.Concat(KnownFighters)
				.Where(f => string.Equals(f.Name, name, StringComparison.InvariantCultureIgnoreCase));
			if (!q.Any()) {
				throw new Exception("No known fighter found with name " + name + ".");
			}
			return q.First().CharBustTexIndex;
		}
		public void SetCharBustTexIndex(string name, int index) {
			name = name.ToLower();
			additionalFighters.Add(new Fighter(name, index));
			string n = name;
			if (KnownFighters.Any(f => f.Name == name)) {
				n += " (override)";
			}
			Console.WriteLine(n + ": char_bust_tex index = " + index);
		}

		public void AddPortraitMappings(int charBustTexIndex, int[] colors) {
			additionalMappings.Add(charBustTexIndex, colors);
			Console.WriteLine("Added color/portrait mappings for index " + charBustTexIndex);
		}
		public void AddPortraitMappings(string name, int[] colors) {
			AddPortraitMappings(GetCharBustTexIndex(name), colors);
		}

		public void ClearAll() {
			additionalFighters.Clear();
			additionalMappings.Clear();
		}

		private Dictionary<int, string> whoWasAdded_Debug;
		public void BrawlExScan(string brawlExDir) {
			if (whoWasAdded_Debug == null) whoWasAdded_Debug = new Dictionary<int, string>();
			if (Directory.Exists(brawlExDir)) {
				foreach (string fitc in Directory.EnumerateFiles(brawlExDir + "/FighterConfig")) {
					byte id;
					if (byte.TryParse(fitc.Substring(fitc.Length - 6, 2), NumberStyles.HexNumber, null, out id)) {
						byte[] fitc_data = File.ReadAllBytes(fitc);
						StringBuilder sb = new StringBuilder();
						for (int i = 0xb0; i < 0xc0; i++) {
							char c = (char)fitc_data[i];
							if (c == 0) break;
							sb.Append(c);
						}
						string name = sb.ToString();

						int? cosmeticIndex = PortraitMap.GetCosmeticSlot(id);
						if (cosmeticIndex != null) {
							string cosm = brawlExDir + "/CosmeticConfig/Cosmetic" + cosmeticIndex.Value.ToString("X2") + ".dat";
							if (File.Exists(cosm)) {
								Console.WriteLine("Cosmetic" + cosmeticIndex.Value.ToString("X2"));
								byte[] cosm_data = File.ReadAllBytes(cosm);
								this.SetCharBustTexIndex(name, cosm_data[0x10]);
							}
						}

						int? cssSlotIndex = PortraitMap.GetCSSSlot(id);
						if (cssSlotIndex != null) {
							string cssc = brawlExDir + "/CSSSlotConfig/CSSSlot" + cssSlotIndex.Value.ToString("X2") + ".dat";
							if (File.Exists(cssc)) {
								byte[] cssc_data = File.ReadAllBytes(cssc);
								List<int> colors = new List<int>();
								for (int i = 0x20; i < 0x40; i += 2) {
									if (cssc_data[i] == 0x0c) break;
									colors.Add(cssc_data[i + 1]);
								}
								try {
									this.AddPortraitMappings(name, colors.ToArray());
								} catch (ArgumentException) {
									string oldname = null;
									int i = GetCharBustTexIndex(name);
									whoWasAdded_Debug.TryGetValue(i, out oldname);
									System.Windows.Forms.MessageBox.Show("Could not add mappings for CSSSlot" +
										cssSlotIndex.Value.ToString("X2") +
										".dat (" + name + ") - mappings were already added for " +
										(oldname??"-null-") + " (char_bust_tex index " + i + ")");
								}
							}
						}
					}
				}
			}
		}
	}
}
