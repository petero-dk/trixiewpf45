/****************************************************************************
	MVVMライブラリ
	ビュー（XAML）用ユーティリティ

	コンバーター・ビヘイビアなどのうち
	BlendSDKを必要としないもの

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
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Mizutama.Lib.MVVM
{
	#region Converters

	/// <summary>
	/// 汎用null判定
	/// ConverterParameterにコンマ区切りのパラメータを渡すことで
	/// 結果反転(inv)、判別関数変更(isnullorempty/isnullorwhitespace)もできる
	/// </summary>
	public class IsNullConverter : IValueConverter
	{
		public object Convert( object value , Type targetType , object parameter , System.Globalization.CultureInfo culture )
		{
			var str = value as string;

			var pstr = parameter as string;
			bool result = false;
			if ( !string.IsNullOrWhiteSpace( pstr ) )
			{
				var parms = pstr.Split( ',' );

				if ( parms.Any( p => p.ToLower() == "isnullorempty" ) )
				{
					result = string.IsNullOrEmpty( str );
				}
				else if ( parms.Any( p => p.ToLower() == "isnullorwhitespace" ) )
				{
					result = string.IsNullOrWhiteSpace( str );
				}
				else
				{
					result = value == null;
				}
				if ( parms.Any( p => p.ToLower() == "inv" ) )
				{
					result = !result;
				}
			}

			return result;
		}

		public object ConvertBack( object value , Type targetType , object parameter , System.Globalization.CultureInfo culture )
		{
			throw new InvalidOperationException( "IsNullConverter can only be used for OneWay." );
		}
	}

	/// <summary>
	/// enum<->boolコンバーター
	/// </summary>
	public class EnumBooleanConverter : System.Windows.Data.IValueConverter
	{
		public object Convert( object value , Type targetType , object parameter , System.Globalization.CultureInfo culture )
		{
			string ParameterString = parameter as string;

			if ( ParameterString == null )
			{
				return DependencyProperty.UnsetValue;
			}

			if ( Enum.IsDefined( value.GetType() , value ) == false )
			{
				return DependencyProperty.UnsetValue;
			}

			object paramvalue = Enum.Parse( value.GetType() , ParameterString );
			if ( paramvalue.GetType() == value.GetType() )
			{
				return paramvalue.Equals( value );
			}

			if ( (int)paramvalue == (int)value )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public object ConvertBack( object value , Type targetType , object parameter , System.Globalization.CultureInfo culture )
		{
			string ParameterString = parameter as string;
			if ( ParameterString == null )
			{
				return DependencyProperty.UnsetValue;
			}

			return Enum.Parse( targetType , ParameterString );
		}
	}

	/// <summary>
	/// http://stackoverflow.com/questions/660528/how-to-display-row-numbers-in-a-listview
	/// ItemsControlにして汎用化
	/// </summary>
	public class ListViewIndexConverter : IValueConverter
	{
		public object Convert( object value , Type TargetType , object parameter , System.Globalization.CultureInfo culture )
		{
			var item = (DependencyObject)value;
			var ctrl = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer( item );
			int index = ctrl.ItemContainerGenerator.IndexFromContainer( item ) + 1;
			return index.ToString();
		}
		public object ConvertBack( object value , Type targetType , object parameter , System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException();
		}
	}

	#endregion Converters

	// http://stackoverflow.com/questions/563195/wpf-textbox-databind-on-enterkey-press
	// Usege:
	//   <TextBox Text="{Binding Path=ItemName, UpdateSourceTrigger=Explicit}"
	//            libm:UpdateByEnterBehavior.Property="TextBox.Text"
	//            />
	public static class UpdateByEnterBehavior
	{
		public static readonly DependencyProperty PropertyProperty = DependencyProperty.RegisterAttached
		(
			"Property" ,
			typeof( DependencyProperty ) ,
			typeof( UpdateByEnterBehavior ) ,
			new PropertyMetadata( null , OnPropertyPropertyChanged )
		);

		public static void SetProperty( DependencyObject dp , DependencyProperty value )
		{
			dp.SetValue( PropertyProperty , value );
		}

		public static DependencyProperty GetProperty( DependencyObject dp )
		{
			return (DependencyProperty)dp.GetValue( PropertyProperty );
		}

		private static void OnPropertyPropertyChanged( DependencyObject dp , DependencyPropertyChangedEventArgs e )
		{
			UIElement element = dp as UIElement;

			if ( element == null )
			{
				return;
			}

			if ( e.OldValue != null )
			{
				element.PreviewKeyDown -= HandlePreviewKeyDown;
			}

			if ( e.NewValue != null )
			{
				element.PreviewKeyDown += new KeyEventHandler( HandlePreviewKeyDown );
			}
		}

		private static void HandlePreviewKeyDown( object sender , KeyEventArgs e )
		{
			if ( e.Key == Key.Enter )
			{
				DoUpdateSource( e.Source );
			}
		}

		private static void DoUpdateSource( object source )
		{
			DependencyProperty property = GetProperty( source as DependencyObject );

			if ( property == null )
			{
				return;
			}

			UIElement elt = source as UIElement;

			if ( elt == null )
			{
				return;
			}

			BindingExpression binding = BindingOperations.GetBindingExpression( elt , property );

			if ( binding != null )
			{
				binding.UpdateSource();
			}
		}
	}

	#region ModalResult

	/// <summary>
	/// なぜWPF（のButton）にはDialogResultがないんだろう
	/// http://stackoverflow.com/questions/1759372/where-is-button-dialogresult-in-wpf
	/// <Button Content="Click Me" libm:ButtonHelper.DialogResult="True" />
	/// </summary>
	public class ButtonHelper
	{
		// Boilerplate code to register attached property "bool? DialogResult" 
		public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached
		(
			"DialogResult" ,
			typeof( bool? ) ,
			typeof( ButtonHelper ) ,
			new UIPropertyMetadata
			(
				( obj , e ) =>
				{
					// Implementation of DialogResult functionality
					var button = obj as System.Windows.Controls.Button;
					if ( button==null )
					{
						throw new InvalidOperationException( "Can only use ButtonHelper.DialogResult on a Button control" );
					}
					button.Click += ( sender , e2 ) => Window.GetWindow( button ).DialogResult = GetDialogResult( button );
				}
			)
		);

		public static bool? GetDialogResult( DependencyObject obj )
		{
			return (bool?)obj.GetValue( DialogResultProperty );
		}

		public static void SetDialogResult( DependencyObject obj , bool? value )
		{
			obj.SetValue( DialogResultProperty , value );
		}
	}

	/// <summary>
	/// VMのバインドでやるなら
	/// http://blog.excastle.com/2010/07/25/mvvm-and-dialogresult-with-no-code-behind/
	/// <Window ...
	///        xmlns:libm="clr-namespace:WpfKMoni.Lib.WPF"
	///        libm:DialogCloser.DialogResult="{Binding DialogResult}">
	/// </summary>
	public static class DialogCloser
	{
		public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached
		(
			"DialogResult" ,
			typeof( bool? ) ,
			typeof( DialogCloser ) ,
			new PropertyMetadata( DialogResultChanged )
		);

		public static void SetDialogResult( Window target , bool? value )
		{
			target.SetValue( DialogResultProperty , value );
		}

		private static void DialogResultChanged( DependencyObject d , DependencyPropertyChangedEventArgs e )
		{
			var window = d as Window;
			if ( window != null )
			{
				window.DialogResult = e.NewValue as bool?;
			}
		}
	}

	#endregion ModalResult
}
