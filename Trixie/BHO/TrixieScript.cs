/****************************************************************************
	Trixie - Tricks for IE
	GreaseMonkey取り扱いクラス

	Trixieまんまを整理したもの。

	Copyright (C) 2013 Mizutama(水玉 ◆qHK1vdR8FRIm)
	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
****************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using mshtml;

namespace Trixie
{
	public class TrixieScript
	{
		#region Properties

		public bool Enabled { get; set; }

		public string Name { get; private set; }
		public string Namespace { get; private set; }
		public string Path { get; set; }
		public string Description { get; private set; }

		public string Includes { get; private set; }
		public string Excludes { get; private set; }

		#endregion Properties

		public TrixieScript()
		{
			Enabled = true;
		}

		#region Methods

		/// <summary>
		/// 実行条件をチェックして必要ならスクリプトを実行
		/// </summary>
		/// <param name="document"></param>
		public void Invoke( HTMLDocument document )
		{
			if ( !Enabled )
			{
				// 許可されてなければ何もしない
				return;
			}
			var url = document.location.toString();
			if ( mRegexExcludes != null )
			{
				if ( mRegexExcludes.IsMatch( url ) )
				{
					// 適応除外URLなので何もしない
					return;
				}
			}

			if ( (mRegexIncludes == null)
			  ||  mRegexIncludes.IsMatch( url )
			   )
			{
				// 適応URLなので
				try
				{
					// 実行
					document.parentWindow.execScript( this.mScript , "JavaScript" );
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// GreaseMonkey形式のファイルを読み込んでメタデータを取り出す
		/// </summary>
		/// <param name="file"></param>
		/// <param name="enabled"></param>
		/// <returns></returns>
		public bool Load( string file , bool enabled )
		{
			Path = file;
			mScript = File.ReadAllText( file );

			// parse GreaseMonkey metadata
			Regex regex2 = new Regex( @"\." , RegexOptions.None );
			Regex regex3 = new Regex( @"\*" , RegexOptions.None );
			string incPattern = string.Empty;
			string excPattern = string.Empty;
			var matches = new Regex( @"[\s*]?//\s*@(?<key>\w+)\s*(?<value>.*)" , RegexOptions.None ).Matches( mScript );
			foreach ( Match mc in matches )
			{
				var key = mc.Groups["key"].Value;
				var val = mc.Groups["value"].Value.Trim();
				switch ( key )
				{
					case "description":
						if ( string.IsNullOrWhiteSpace( Description ) )
						{
							Description = val;
						}
						else
						{
							Description += " " + val;
						}
						break;

					case "name":
						Name = val;
						break;

					case "namespace":
						Namespace = val;
						break;

					case "include":
						if ( string.IsNullOrWhiteSpace( Includes ) )
						{
							Includes = val;
						}
						else
						{
							Includes += "\n" + val;
						}
						val = regex2.Replace( val , @"\." );
						val = regex3.Replace( val , ".*" );
						if ( !string.IsNullOrWhiteSpace( incPattern ) )
						{
							incPattern += "|";
						}
						incPattern += "(" + val + ")";
						break;

					case "exclude":
						if ( string.IsNullOrWhiteSpace( Excludes ) )
						{
							Excludes = val;
						}
						else
						{
							Excludes += "\n" + val;
						}
						val = regex2.Replace( val , @"\." );
						val = regex3.Replace( val , ".*" );
						if ( !string.IsNullOrWhiteSpace( excPattern ) )
						{
							excPattern += "|";
						}
						excPattern += "(" + val + ")";
						break;
				}
			}

			if ( string.IsNullOrWhiteSpace( Name ) || string.IsNullOrWhiteSpace( Namespace ) )
			{
				return false;
			}
			if ( !string.IsNullOrWhiteSpace( incPattern ) )
			{
				mRegexIncludes = new Regex( incPattern , RegexOptions.Compiled );
			}
			if ( !string.IsNullOrWhiteSpace( excPattern ) )
			{
				mRegexExcludes = new Regex( excPattern , RegexOptions.Compiled );
			}
			Enabled = enabled;

			return true;
		}

		public override string ToString()
		{
			return Name;
		}

		#endregion Methods

		#region Private Fields

		private Regex mRegexIncludes;
		private Regex mRegexExcludes;
		private string mScript;

		#endregion Private Fields
	}
}

