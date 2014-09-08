/****************************************************************************
	Trixie - Tricks for IE
	BHO、言ってみればメインクラス

	もう正直に言う。Trixieを.NET Reflectorでリバースして整理したもの。
	だって2006年のVer.0.2.3で開発は止まったし
	配布サイト http://www.bhelpuri.net/Trixie は消滅しているし
	Googleを筆頭とする数多のサイトがいろいろ余計なことしてくれてるおかげでまっとーにブラウズできない
	FireFoxにはGreaseMonkeyがあるからいろいろ仕込めるがIEにはないじゃんか。
	なら作るか？とごそごそしてたらそーいえばTrixieがあったなぁ、いまどうなってんの？とググったら
	まあそういうことで、GreaseMonkey互換部をまっさらから作るのもめんどいのでパクったというわけさ。
	でもオリジナルTrixieは相当古い（コーディングだ）し、最新テクノロジー()で書き換えてるからいいよね。

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
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using mshtml;
using SHDocVw;

namespace Trixie
{
	[
	 ComVisible( true ) ,
	 Guid( "B0744341-96E0-4341-9ED2-8BC36CE0CCD0" ) ,
	 ClassInterface( ClassInterfaceType.None )
	]
	public class Bho : IObjectWithSite
	{
		/// <summary>
		/// インスタンス化されたTrixieのリスト
		/// </summary>
		public static List<Bho> gInstances = new List<Bho>();

		#region COM Register

		private const string BHO_REGISTRY_KEY_NAME = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects";

		[ComRegisterFunction]
		public static void RegisterBHO( Type type )
		{
			// BHOを登録するためのレジストリキーを取得
			var registryKey = Registry.LocalMachine.OpenSubKey( BHO_REGISTRY_KEY_NAME , true );

			if ( registryKey == null )
			{
				// なければ作る
				registryKey = Registry.LocalMachine.CreateSubKey( BHO_REGISTRY_KEY_NAME );
			}

			// BHOのGUIDを登録する
			string guid = type.GUID.ToString( "B" );	// GUID{xxxx-xx-...}形式
			RegistryKey ourKey = registryKey.OpenSubKey( guid );
			if ( ourKey == null )
			{
				// なければ作る
				ourKey = registryKey.CreateSubKey( guid );
			}
			// 既定の値として名前を入れておこう
			ourKey.SetValue( "" , "Trixie BHO" , RegistryValueKind.String );

			// (普通の)Explorerで使わないように
			ourKey.SetValue( "NoExplorer" , 1 , RegistryValueKind.DWord );

			registryKey.Close();
			ourKey.Close();
		}

		[ComUnregisterFunction]
		public static void UnregisterBHO( Type type )
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey( BHO_REGISTRY_KEY_NAME , true );
			string guid = type.GUID.ToString( "B" );

			if ( registryKey != null )
			{
				registryKey.DeleteSubKey( guid , false );
			}
		}

		#endregion COM Register

		#region IObjectWithSite

		/// <summary>
		/// IEによりTrixieがインスタンス化され取り付けられたまたは取り外された
		/// のでフックを付けてGreaseMonkeyを読み込んで実行
		/// またはフックを取り外し
		/// </summary>
		/// <param name="site"></param>
		/// <returns></returns>
		public int SetSite( object site )
		{
			if ( site != null )
			{
				gInstances.Add( this );

				// setup webbrowser hook
				mWebBrowser = (WebBrowser)site;
				mWebBrowser.DocumentComplete += DocumentComplete;
				mWebBrowser.OnQuit += WebBrowser_OnQuit;

				// load GreaseMonkies
				var asm = Assembly.GetExecutingAssembly();
				mBasePath = Path.GetDirectoryName( asm.Location );
				LoadScripts();
			}
			else
			{
				// remove hook
				mWebBrowser.DocumentComplete -= DocumentComplete;
				mWebBrowser.OnQuit -= WebBrowser_OnQuit;
				mWebBrowser = null;
			}

			return 0;
		}

		/// <summary>
		/// んー、なんかお呪い？
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="ppvSite"></param>
		/// <returns></returns>
		public int GetSite( ref Guid guid , out IntPtr ppvSite )
		{
			IntPtr punk = Marshal.GetIUnknownForObject( mWebBrowser );
			int hr = Marshal.QueryInterface( punk , ref guid , out ppvSite );
			Marshal.Release( punk );

			return hr;
		}

		#endregion IObjectWithSite

		#region Methods

		/// <summary>
		/// GreaseMonkeyをロードしなおす
		/// </summary>
		/// <returns></returns>
		public List<TrixieScript> ReloadScripts()
		{
			mInitialized = false;
			LoadScripts();

			return mTrixieScripts;
		}

		/// <summary>
		/// オプションダイアログを表示 
		/// </summary>
		public static void ShowOptionsDlg()
		{
			foreach ( var bho in gInstances )
			{
				if ( bho != null )
				{
					// 初めに見つかった生きているTrixieインスタンスで表示
					var options = new OptionDialog( bho );
					options.Init( mTrixieScripts );
					options.ShowDialog();
					return;
				}
			}
		}

		/// <summary>
		/// 設定を更新
		/// </summary>
		public void UpdateConfigXml()
        {
            try
            {
                mLock.EnterWriteLock();
                mSettings.Clear();
				foreach ( var script in mTrixieScripts )
				{
					var name = Path.GetFileName( script.Path );
					mSettings.Add( name , script.Enabled );
				}
				Update( mBasePath );
            }
            finally
            {
                mLock.ExitWriteLock();
            }
        }

		#endregion Methods

		#region Implementaion

		#region WebBrowser Events

		/// <summary>
		/// 取り付いたウェブページのロードが完了したのでGreaseMonkeyを仕込む
		/// </summary>
		/// <param name="pDisp"></param>
		/// <param name="URL"></param>
		private void DocumentComplete( object pDisp , ref object URL )
		{
			try
			{
				Trace.WriteLine( "DocumentComplete: " + URL );

				HTMLDocument document = (HTMLDocument)mWebBrowser.Document;
				//string code = "(function()\r\n                    {\r\n                        trixieXmlHttp = new Array();\r\n                        trixieXmlHttpNdx = 0;\r\n                        GM_xmlhttpRequest = function(details)\r\n                        {\r\n                            var o = new ActiveXObject('Trixie.TrixieXmlHttp');\r\n                            o.init();\r\n                            if (details.headers != null)\r\n                            {\r\n                                for (e in details.headers)\r\n                                {\r\n                                    o.addHeader(e + ': ' + details.headers[e]);\r\n                                }\r\n                            }\r\n                            var data = null;\r\n                            if (details.data != null)\r\n                                data = details.data;\r\n                            var ndx = trixieXmlHttpNdx++;\r\n                            trixieXmlHttp[ndx] = details;\r\n                            o.open(details.method, details.url, data, document, ndx);\r\n                        }\r\n                    }\r\n                )();";
				//document.parentWindow.execScript( code , "JavaScript" );
				mLock.EnterReadLock();
				try
				{
					foreach ( var script in mTrixieScripts )
					{
						script.Invoke( document );
					}
				}
				catch
				{
				}
				mLock.ExitReadLock();
			}
			catch
			{
			}
		}

		/// <summary>
		/// ブラウザのページが閉じられたのでインスタンスリストから削除
		/// </summary>
		private void WebBrowser_OnQuit()
		{
			gInstances.Remove( this );
		}

		#endregion WebBrowser Events

		/// <summary>
		/// GreaseMonkeyの読み込み
		/// </summary>
		private void LoadScripts()
		{
			var folder = Path.Combine( mBasePath , "Scripts" );
			if ( !Directory.Exists( folder ) )
			{
				return;
			}

			if ( !mInitialized )
			{
				try
				{
					mLock.EnterUpgradeableReadLock();
					if ( !mInitialized )
					{
						mTrixieScripts.Clear();
						Init( mBasePath );
						var files = Directory.GetFiles( folder , "*.js" );
						bool flag = false;
						foreach ( var file in files )
						{
							try
							{
								var script = new TrixieScript();
								if ( script.Load( file , true ) )
								{
									mTrixieScripts.Add( script );
									var name = Path.GetFileName( file );
									if ( !mSettings.ContainsKey( name ) )
									{
										flag = true;
									}
									else
									{
										script.Enabled = mSettings[name];
									}
								}
							}
							catch ( Exception exception )
							{
								Trace.WriteLine( exception );
							}
						}
						if ( flag )
						{
							UpdateConfigXml();
						}
						mInitialized = true;
					}
				}
				finally
				{
					mLock.ExitUpgradeableReadLock();
				}
			}
		}

		#region Setting

		private const string cSettingFile = "Trixie.config.xml";

		private void Init( string path )
		{
			var file = Path.Combine( path , cSettingFile );
			mSettings = new Dictionary<string , bool>();
			if ( !File.Exists( file ) )
			{
				// ファイルがなかったら作っておく
				Update( path );
			}

			var xdoc = XElement.Load( file );
			foreach ( var sc in xdoc.Elements( "script" ) )
			{
				var name = sc.Attribute( "name" ).Value;
				var enabled = (bool)sc.Attribute( "enabled" );
				mSettings.Add( name , enabled );
			}
		}

		private void Update( string path )
		{
			var file = Path.Combine( path , cSettingFile );

			var xdoc = new XElement( "settings" );
			foreach ( var kv in mSettings )
			{
				xdoc.Add
				(
					new XElement
					(
						"scripts" ,
						new XAttribute( "name" , kv.Key ) ,
						new XAttribute( "enabled" , kv.Value )
					)
				);
			}

			File.WriteAllText( file , xdoc.ToString() );
		}

		#endregion Setting

		#endregion Implementaion

		#region Private Fields

		private static bool mInitialized = false;
		private static ReaderWriterLockSlim mLock = new ReaderWriterLockSlim();
		private static Dictionary<string , bool> mSettings = new Dictionary<string , bool>();
		private static List<TrixieScript> mTrixieScripts = new List<TrixieScript>();

		private string mBasePath = null;
		private WebBrowser mWebBrowser;

		#endregion Private Fields
	}
}

