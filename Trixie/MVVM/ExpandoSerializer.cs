/****************************************************************************
	汎用ライブラリ
	シリアライズ可能なExpandoObject

	ExpandoObjectをシリアライズ可能にしてアプリケーショングローバルにする
	そうするとXAMLだけでBindingでき、メンバが自動的に追加されるようになるので
	アプリケーションの起動・終了時に読み込み・保存すれば
	アプリケーション設定（ウインドウ位置とか）をXAMLだけで定義・保存できるようになる
	そこでXMLへ保存・読み込みする機能を実装する
	IXmlSerializableを実装するとちょっと面倒なので独自実装

	Copyright (C) 2012 Mizutama(水玉 ◆qHK1vdR8FRIm)
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
using System.Dynamic;
using System.Xml;
using System.Xml.Serialization;

namespace Mizutama.Lib
{
	public class ExpandoSerializer
	{
		/// <summary>
		/// ExpandoObjectをシリアライズする
		/// </summary>
		/// <param name="writer"></param>
		public static string ToXml( ExpandoObject bag , string root )
		{
			var xml = new StringWriterWithEncoding( Encoding.UTF8 );
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "  ";

			using ( var writer = XmlWriter.Create( xml , settings ) )
			{
				// ドキュメント開始
				writer.WriteStartDocument();

				// ルート要素
				writer.WriteStartElement( root );

				var thisdic = bag as IDictionary<string , object>;
				foreach ( string key in thisdic.Keys )
				{
					var value = thisdic[key];
					if ( value is Delegate )
					{
						continue;
					}

					Type type = null;
					if ( value != null )
					{
						type = value.GetType();
					}

					// キーをエレメント名にする
					writer.WriteStartElement( key as string );

					// 具の型をエレメントのアトリビュートにし具を書き出すのだが
					if ( value == null )
					{
						// nullは特別な型にして空エレメントにする
						writer.WriteAttributeString( "type" , "nil" );
					}
					else
					{
						// 型名をアトリビュートに設定
						if ( type.Name != "String" )
						{
							writer.WriteStartAttribute( "type" );
							writer.WriteString( type.FullName );
							writer.WriteEndAttribute();
						}
						if ( value is IConvertible )
						{
							// IConvertibleを持っている型（主に組み込み型）は単純変換ができるので
							// そのまま具を書き出す
							writer.WriteValue( value );
						}
						else
						{
							// 複雑な型はシリアライザに丸投げ、なのでXMLがキモくなる
							var ser = new XmlSerializer( value.GetType() );
							ser.Serialize( writer , value );
						}
					}

					// このエレメントは出来上がり
					writer.WriteEndElement();
				}

				// ルート要素終了
				writer.WriteEndElement();

				// ドキュメント終了
				writer.WriteEndDocument();
			}

			return xml.ToString();
		}

		public static ExpandoObject FromXml( string xml , string root )
		{
			var bag = new ExpandoObject();

			if ( string.IsNullOrEmpty( xml ) )
			{
				return bag;
			}

			using ( var reader = new XmlTextReader( xml , XmlNodeType.Document , null ) )
			{
				// ルート要素を探す
				while ( reader.Read() )
				{
					if ( (reader.NodeType == XmlNodeType.Element) && (reader.Name == root) )
					{
						// 見つけた
						break;
					}
				}
				if ( reader.EOF )
				{
					// 見つからなかった
					return bag;
				}

				// 項目読み込み
				var thisdic = bag as IDictionary<string , object>;
				while ( reader.Read() )
				{
					// 項目エレメントに移動
					if ( reader.NodeType != XmlNodeType.Element )
					{
						continue;
					}
					string name = reader.Name;

					// アトリビュートを取得、こいつには戻す型が書いてあるはず
					// アトリビュートには「type」しかないことに決め打っている
					string xmlType = null;
					if ( reader.MoveToNextAttribute() )
					{
						xmlType = reader.Value;
					}

					// 具に移動
					reader.MoveToContent();

					object value;
					if ( xmlType == "nil" )
					{
						// nullは特別なことをしていたのだった
						value = default( object );
					}
					else if ( string.IsNullOrEmpty( xmlType ) )
					{
						// アトリビュートが無いのはstringとしていたのさ
						value = reader.ReadString();
					}
					else
					{
						var type = GetTypeFromName( xmlType );
						if ( type == null )
						{
							// アプリ内にその型情報は無い
							continue;
						}
						else if ( type.GetInterface( "System.IConvertible" ) != null )
						{
							// 単純変換できるのものは変換読み込み
							value = reader.ReadElementContentAs( type , null );
						}
						else
						{
							// 複雑な型はデシリアライザに丸投げするので
							// まずデシリアライズするエレメントまで移動
							while ( reader.Read() && reader.NodeType != XmlNodeType.Element )
							{ }

							// デシリアライズ
							XmlSerializer ser = new XmlSerializer( type );
							value = ser.Deserialize( reader );
						}
					}

					// 項目追加
					thisdic.Add( name , value );
				}
			}

			return bag;
		}

		public static Type GetTypeFromName( string typeName )
		{
			var type = Type.GetType( typeName , false );
			if ( type != null )
			{
				return type;
			}

			// 未ロードのアセンブリにある型なのかもしれないのでスキャンしてみる
			foreach ( System.Reflection.Assembly ass in AppDomain.CurrentDomain.GetAssemblies() )
			{
				type = ass.GetType( typeName , false );
				if ( type != null )
				{
					break;
				}
			}
			return type;
		}
	}

	// http://stackoverflow.com/questions/427725/how-to-put-an-encoding-attribute-to-xml-other-that-utf-16-with-xmlwriter
	public sealed class StringWriterWithEncoding : System.IO.StringWriter
	{
		public StringWriterWithEncoding( Encoding encoding )
		{
			_Encoding = encoding;
		}

		public override Encoding Encoding
		{
			get { return _Encoding; }
		}
		private readonly Encoding _Encoding;
	}
}
