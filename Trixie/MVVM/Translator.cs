/****************************************************************************
	MVVM Library
	Markup Extention for localized string

	refactored http://wpftutorial.net/LocalizeMarkupExtension.html
 

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
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using System.Globalization;
using System.Threading;

namespace Mizutama.Lib.MVVM
{
	/// <summary>
	/// The Translate Markup extension is a binding to a TranslationData
	/// that provides a translated resource of the specified key
	/// </summary>
	public class TranslateExtension : System.Windows.Data.Binding
	{
		[ConstructorArgument( "key" )]
		public string Key
		{
			get { return _Key; }
			set
			{
				_Key = value;

				 // set this Binding to the value of the Key object which notify PropertyChanged
				 // when TranslationManager(.Instance).CurrentLaguage is changed
				Path = new PropertyPath( "Value" );
				Source = new TranslationData( Key );
			}
		}
		private string _Key;

		public TranslateExtension() { }

		public TranslateExtension( string key )
		{
			Key = key;
		}
	}

	/// <summary>
	/// bindable translated string value listening LanguageChangedEvent from LanguageChangedEventManager
	/// </summary>
	public class TranslationData : IWeakEventListener , INotifyPropertyChanged
	{
		public object Value
		{
			get
			{
				return TranslationManager.Instance.Translate( _Key );
			}
		}
		private string _Key;

		/// <summary>
		/// Initializes a new instance of the <see cref="TranslationData"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		public TranslationData( string key )
		{
			_Key = key;
			LanguageChangedEventManager.AddListener( TranslationManager.Instance , this );
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="TranslationData"/> is reclaimed by garbage collection.
		/// </summary>
		~TranslationData()
		{
			LanguageChangedEventManager.RemoveListener( TranslationManager.Instance , this );
		}

		public bool ReceiveWeakEvent( Type managerType , object sender , EventArgs e )
		{
			if ( managerType == typeof( LanguageChangedEventManager ) )
			{
				if ( PropertyChanged != null )
				{
					PropertyChanged( this , new PropertyChangedEventArgs( "Value" ) );
				}
				return true;
			}
			return false;
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}

	/// <summary>
	/// singleton of a WeakEventManager to deliver a LanguageChangedEvent
	/// </summary>
	public class LanguageChangedEventManager : WeakEventManager
	{
		#region Add/Remove Listener

		public static void AddListener( TranslationManager source , IWeakEventListener listener )
		{
			CurrentManager.ProtectedAddListener( source , listener );
		}

		public static void RemoveListener( TranslationManager source , IWeakEventListener listener )
		{
			CurrentManager.ProtectedRemoveListener( source , listener );
		}

		#endregion Add/Remove Listener

		#region Overrides

		protected override void StartListening( object source )
		{
			var manager = (TranslationManager)source;
			manager.LanguageChanged += OnLanguageChanged;
		}

		protected override void StopListening( Object source )
		{
			var manager = (TranslationManager)source;
			manager.LanguageChanged -= OnLanguageChanged;
		}

		#endregion Overrides

		#region Implementation

		private void OnLanguageChanged( object sender , EventArgs e )
		{
			DeliverEvent( sender , e );
		}

		private static LanguageChangedEventManager CurrentManager
		{
			get
			{
				Type managerType = typeof( LanguageChangedEventManager );
				var manager = (LanguageChangedEventManager)GetCurrentManager( managerType );
				if ( manager == null )
				{
					manager = new LanguageChangedEventManager();
					SetCurrentManager( managerType , manager );
				}
				return manager;
			}
		}

		#endregion Implementation
	}

	/// <summary>
	/// manages available languages and current language , hosting TranslationProvider
	/// </summary>
	public class TranslationManager
	{
		#region Properties

		public static TranslationManager Instance { get; private set; }

		public CultureInfo CurrentLanguage
		{
			get { return Thread.CurrentThread.CurrentUICulture; }
			set
			{
				if ( value != Thread.CurrentThread.CurrentUICulture )
				{
					Thread.CurrentThread.CurrentUICulture = value;
					if ( LanguageChanged != null )
					{
						LanguageChanged( this , EventArgs.Empty );
					}
				}
			}
		}

		public IEnumerable<CultureInfo> Languages
		{
			get
			{
				if ( TranslationProvider != null )
				{
					return TranslationProvider.Languages;
				}
				return Enumerable.Empty<CultureInfo>();
			}
		}

		public ITranslationProvider TranslationProvider { get; set; }

		#endregion Properties

		#region Events

		public event EventHandler LanguageChanged;

		#endregion Events

		#region Construcor

		static TranslationManager()
		{
			Instance = new TranslationManager();
		}

		private TranslationManager() { }

		#endregion Construcor

		public object Translate( string key )
		{
			if ( TranslationProvider!= null )
			{
				object translatedValue = TranslationProvider.Translate( key );
				if ( translatedValue != null )
				{
					return translatedValue;
				}
			}
			return string.Format( "!{0}!" , key );
		}
	}

	/// <summary>
	/// how can you know the current language?
	/// see Thread.CurrentThread.CurrentUICulture or TranslationManager(.Instance).CurrentLanguage
	/// </summary>
	public interface ITranslationProvider
	{
		/// <summary>
		/// Translates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		object Translate( string key );

		/// <summary>
		/// Gets the available languages.
		/// </summary>
		/// <value>The available languages.</value>
		IEnumerable<CultureInfo> Languages { get; }
	}

	/// <summary>
	/// translation provider for XML
	/// the XML should be
	///<?xml version="1.0" encoding="utf-8" ?>
	///<Strings>
	///  <Item Key="binding_key">
	///	  <String>string for invaliant culture</String>
	///	  <String Language="language_name">string for the language</String>
	///  </Item>
	///     :
	///</Strings>
	/// language_name shoudl be as CultureInfo.Name
	/// XAML as
	///  <TextBlock Text="{libm:Translate binding_key}" />
	/// </summary>
	public class XmlTranslationProvider : ITranslationProvider
	{
		public IEnumerable<CultureInfo> Languages { get { return mLocalizer.Keys; } }

		/// <summary>
		/// setup translation provider with the dictionary from the xml
		/// </summary>
		/// <param name="baseName">Name of the base.</param>
		/// <param name="assembly">The assembly.</param>
		public XmlTranslationProvider( string xml )
		{
			var xdoc = System.Xml.Linq.XDocument.Parse( xml );
			var items = xdoc.Root.Elements( "Item" );
			foreach ( var item in items )
			{
				var key = item.Attribute( "Key" ).Value;
				if ( !string.IsNullOrWhiteSpace( key ) )
				{
					foreach ( var str in item.Elements( "String" ) )
					{
						var culture = CultureInfo.InvariantCulture;
						if ( str.Attribute( "Language" ) != null )
						{
							culture = new CultureInfo( str.Attribute( "Language" ).Value );
						}
						if ( !mLocalizer.ContainsKey( culture ) )
						{
							mLocalizer[culture] = new Dictionary<string , string>();
						}
						mLocalizer[culture][key] = str.Value;
					}
				}
			}
		}

		public object Translate( string key )
		{
			var culture = TranslationManager.Instance.CurrentLanguage;
			return mLocalizer.ContainsKey( culture ) ? mLocalizer[culture][key] : mLocalizer[CultureInfo.InvariantCulture][key];
		}

		private Dictionary<CultureInfo , Dictionary<string , string>> mLocalizer = new Dictionary<CultureInfo , Dictionary<string , string>>();
	}
}
