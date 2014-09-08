/****************************************************************************
	Trixie - Tricks for IE
	COMインターフェース

	まとめただけ

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
using System.Runtime.InteropServices;

namespace Trixie
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")]
	public interface IObjectWithSite
	{
		[PreserveSig]
		int SetSite( [MarshalAs( UnmanagedType.IUnknown )]object site );

		[PreserveSig]
		int GetSite( ref Guid guid , out IntPtr ppvSite );
	}


	[ComVisible( false )]
	public enum OLECMDTEXTF : uint
	{
		OLECMDTEXTF_NAME = 1 ,
		OLECMDTEXTF_NONE = 0 ,
		OLECMDTEXTF_STATUS = 2
	}

	[StructLayout( LayoutKind.Sequential ) , ComVisible( false )]
	public struct OLECMDTEXT
	{
		public OLECMDTEXTF cmdtextf;
		public uint cwActual;
		public uint cwBuf;
		public char[] rgwz;
	}

	[ComVisible( false )]
	public enum OLECMDF : uint
	{
		OLECMDF_ENABLED = 2 ,
		OLECMDF_LATCHED = 4 ,
		OLECMDF_NINCHED = 8 ,
		OLECMDF_SUPPORTED = 1
	}

	[ComVisible( false )]
	public enum OLECMDEXECOPT : uint
	{
		OLECMDEXECOPT_DODEFAULT = 0 ,
		OLECMDEXECOPT_DONTPROMPTUSER = 2 ,
		OLECMDEXECOPT_PROMPTUSER = 1 ,
		OLECMDEXECOPT_SHOWHELP = 3
	}

	[StructLayout( LayoutKind.Sequential ) , ComVisible( false )]
	public struct OLECMD
	{
		public uint cmdID;
		public OLECMDF cmdf;
	}

	[ComImport , Guid( "b722bccb-4e68-101b-a2bc-00aa00404770" ) , InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IOleCommandTarget
	{
		void QueryStatus( [In , MarshalAs( UnmanagedType.LPStruct )] Guid pguidCmdGroup , [In] uint cCmds , [In , Out] ref OLECMD prgCmds , [In , Out] IntPtr pCmdText );
		void Exec( [In , MarshalAs( UnmanagedType.LPStruct )] Guid pguidCmdGroup , [In] uint nCmdID , [In] OLECMDEXECOPT nCmdexecopt , [In] IntPtr pvaIn , [In , Out] IntPtr pvaOut );
	}
}

