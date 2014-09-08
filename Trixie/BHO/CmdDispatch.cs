/****************************************************************************
	Trixie - Tricks for IE
	メニュー拡張

	オプションダイアログを開くメニューをIEメニューに取り付ける

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
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Trixie
{
	[
	 ComVisible( true ) ,
	 Guid( "20CCCFEC-D26F-4ffe-996B-388B39C8CCCA" ) ,
	 ClassInterface( ClassInterfaceType.None )
	]
	public class CmdDispatch : IOleCommandTarget
	{
		public void Exec( Guid pguidCmdGroup , uint nCmdID , OLECMDEXECOPT nCmdexecopt , IntPtr pvaIn , IntPtr pvaOut )
		{
			if ( (nCmdID == 0) && (nCmdexecopt != OLECMDEXECOPT.OLECMDEXECOPT_SHOWHELP) )
			{
				Bho.ShowOptionsDlg();
			}
		}

		public void QueryStatus( Guid pguidCmdGroup , uint cCmds , ref OLECMD prgCmds , IntPtr pCmdText )
		{
			prgCmds.cmdf = OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED;
		}

		#region COM Register

		private const string CLSID_Shell_ToolbarExtExec = "{1FBA04EE-3024-11d2-8F1F-0000F87ABD16}";
		private const string IEEXTENSIONS_REGISTRY_KEY_NAME = @"Software\Microsoft\Internet Explorer\Extensions";

		[ComRegisterFunction]
		private static void RegisterMenu( Type type )
		{
			try
			{
				RegistryKey key2 = Registry.LocalMachine.OpenSubKey( IEEXTENSIONS_REGISTRY_KEY_NAME , true );
				string guid = type.GUID.ToString( "B" );
				RegistryKey key3 = key2.CreateSubKey( guid );
				key3.SetValue( "" , "Trixie Option Menu" , RegistryValueKind.String );
				key3.SetValue( "Default Visible" , "Yes" );
				key3.SetValue( "MenuText" , "Tri&xie Options..." );
				key3.SetValue( "MenuStatusBar" , "Show Trixie Options" );
				key3.SetValue( "CLSID" , CLSID_Shell_ToolbarExtExec );
				key3.SetValue( "ClsidExtension" , guid );
				key3.Close();
				key2.Close();
			}
			catch
			{
			}
		}

		[ComUnregisterFunction]
		private static void UnregisterMenu( Type type )
		{
			try
			{
				var registryKey = Registry.LocalMachine.OpenSubKey( IEEXTENSIONS_REGISTRY_KEY_NAME , true );
				if ( registryKey != null )
				{
					string guid = type.GUID.ToString( "B" );
					registryKey.DeleteSubKey( guid , false );
					registryKey.Close();
				}
			}
			catch
			{
			}
		}

		#endregion COM Register
	}
}

