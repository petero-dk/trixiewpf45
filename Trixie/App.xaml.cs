/****************************************************************************
	Trixie - Tricks for IE
	アプリケーションクラス(COM登録)

	セルフレジスタなのでregasmしなくてらくちん

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
using System.Windows;
using System.Runtime.InteropServices;
using System.Reflection;

using Mizutama.Lib.MVVM;

namespace Trixie
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		/// <summary>
		/// バージョン文字列
		/// オプションダイアログで見る事ができる
		/// </summary>
		public static string UserAgent
		{
			get
			{
				return string.Format( "{0}/{1}.{2} ({3}-{4} {5})"
						, VerInfo.Name
						, VerInfo.Version.Major , VerInfo.Version.Minor
						, VerInfo.DevPhase , VerInfo.Config
						, VerInfo.Version );
			}
		}

		/// <summary>
		/// セルフレジスタ
		/// アプリケーションとしてはここだけ実行して終わる
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnStartup( object sender , StartupEventArgs e )
		{
			if ( TranslationManager.Instance.TranslationProvider == null )
			{
				// setup Translator
				var xml = Trixie.Properties.Resources.Localizer;
				var tx = new XmlTranslationProvider( xml );
				TranslationManager.Instance.TranslationProvider = tx;
			}

			// Register
			Assembly asm = Assembly.GetExecutingAssembly();
			RegistrationServices reg = new RegistrationServices();

			var args = Environment.GetCommandLineArgs();
			if ( args.Length > 2 )
			{
				if ( args[1].Equals( "/u" ) )
				{
					// Unregister with COM
					if ( reg.UnregisterAssembly( asm ) )
					{
						Console.Write( TranslationManager.Instance.Translate( "Unregistered" ) );
					}
					else
					{
						Console.Write( TranslationManager.Instance.Translate( "UnregisterFail" ) );
					}
				}
				else if ( args[1].Equals( "/r" ) )
				{
					// Register with COM
					if ( reg.RegisterAssembly( asm , AssemblyRegistrationFlags.SetCodeBase ) )
					{
						Console.Write( TranslationManager.Instance.Translate( "Registered" ) );
					}
					else
					{
						Console.Write( TranslationManager.Instance.Translate( "RegisterFail" ) );
					}
				}
				Shutdown();
				return;
			}

			Type installed = Type.GetTypeFromProgID( "Trixie.Bho" );
			if ( installed != null )
			{
				var result = MessageBox.Show( (string)TranslationManager.Instance.Translate( "Unregistering" ) , "Trixie" , MessageBoxButton.YesNo );
				if ( result == MessageBoxResult.Yes )
				{
					// Unregister with COM
					if ( reg.UnregisterAssembly( asm ) )
					{
						MessageBox.Show( (string)TranslationManager.Instance.Translate( "Unregistered" ) , "Trixie" );
					}
					else
					{
						MessageBox.Show( (string)TranslationManager.Instance.Translate( "UnregisterFail" ) , "Trixie" );
					}
				}
			}
			else
			{
				var result = MessageBox.Show( (string)TranslationManager.Instance.Translate( "Registering" ) , "Trixie" , MessageBoxButton.YesNo );
				if ( result == MessageBoxResult.Yes )
				{
					// Register with COM
					if ( reg.RegisterAssembly( asm , AssemblyRegistrationFlags.SetCodeBase ) )
					{
						MessageBox.Show( (string)TranslationManager.Instance.Translate( "Registered" ) , "Trixie" );
					}
					else
					{
						MessageBox.Show( (string)TranslationManager.Instance.Translate( "RegisterFail" ) , "Trixie" );
					}
				}
			}
			Shutdown();
		}
	}
}
