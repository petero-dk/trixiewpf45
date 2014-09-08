/****************************************************************************
	Trixie - Tricks for IE
	オプションダイアログ

	WPF簡易MVVMだから好きにいじれる

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
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;

using Mizutama.Lib.MVVM;

namespace Trixie
{
	/// <summary>
	/// OptionDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class OptionDialog : Window , INotifyPropertyChanged
	{
		/// <summary>
		/// バインドするためのスクリプトリスト
		/// </summary>
		public ObservableCollection<ScriptSetting> Settings { get; set; }

		public OptionDialog( Bho trixie )
		{
			if ( TranslationManager.Instance.TranslationProvider == null )
			{
				// setup Translator because COM is created without calling App.OnStartup()
				var xml = Trixie.Properties.Resources.Localizer;
				var tx = new XmlTranslationProvider( xml );
				TranslationManager.Instance.TranslationProvider = tx;
			}

			Settings = new ObservableCollection<ScriptSetting>();
			mTrixie = trixie;

			InitializeComponent();

			DataContext = this;
		}
		
		/// <summary>
		/// ロード済みスクリプトをXAMLで参照できるようにとEnable設定をOKするまで仮保持するために
		/// Notifyオブジェクトで包んでObservableCollectionに突っ込む
		/// </summary>
		/// <param name="loadedScripts"></param>
		public void Init( IEnumerable<TrixieScript> loadedScripts )
		{
			Settings.Clear();
			foreach ( var script in loadedScripts )
			{
				var setting = new ScriptSetting() { Enabled = script.Enabled , Script = script };
				Settings.Add( setting );
			}
		}

		/// <summary>
		/// スクリプトファイルの再スキャン
		/// コマンドにすべきだけど手抜き
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Reload_Click( object sender , RoutedEventArgs e )
		{
			var scripts = mTrixie.ReloadScripts();
			Init( scripts );
		}

		/// <summary>
		/// OKなので仮保持していたEnableを設定に反映
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OK_Click( object sender , RoutedEventArgs e )
		{
			foreach ( var setting in Settings )
			{
				setting.Script.Enabled = setting.Enabled;
			}
			mTrixie.UpdateConfigXml();

			DialogResult = true;
		}

		/// <summary>
		/// キャンセルはキャンセルだ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cancel_Click( object sender , RoutedEventArgs e )
		{
			DialogResult = false;
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged( [System.Runtime.CompilerServices.CallerMemberName]string propertyName=null )
		{
			PropertyChangedEventHandler handler = PropertyChanged;

			if ( handler != null )
			{
				handler( this , new PropertyChangedEventArgs( propertyName ) );
			}
		}

		#endregion INotifyPropertyChanged

		private Bho mTrixie;
	}

	/// <summary>
	/// Enableを仮保持するためのNotifyオブジェクト
	/// </summary>
	public class ScriptSetting : INotifyPropertyChanged
	{
		public bool Enabled
		{
			get { return _Enabled; }
			set
			{
				_Enabled = value;
				RaisePropertyChanged();
			}
		}
		private bool _Enabled;

		public TrixieScript Script { get; set; }

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged( [System.Runtime.CompilerServices.CallerMemberName]string propertyName=null )
		{
			PropertyChangedEventHandler handler = PropertyChanged;

			if ( handler != null )
			{
				handler( this , new PropertyChangedEventArgs( propertyName ) );
			}
		}

		#endregion INotifyPropertyChanged
	}
}
