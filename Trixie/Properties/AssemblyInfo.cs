/****************************************************************************
	Trixie
	バージョン情報クラス
	BuildIncアドオン対応

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
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle( "Trixie" )]
[assembly: AssemblyDescription( "" )]
[assembly: AssemblyCompany( "" )]
[assembly: AssemblyProduct( "Trixie" )]
[assembly: AssemblyCopyright( "Copyright (c) 2013 Mizutama" )]
[assembly: AssemblyTrademark( "水玉 ◆qHK1vdR8FRIm" )]
[assembly: AssemblyCulture( "" )]

// ComVisible を false に設定すると、その型はこのアセンブリ内で COM コンポーネントから
// 参照不可能になります。COM からこのアセンブリ内の型にアクセスする場合は、
// その型の ComVisible 属性を true に設定してください。
[assembly: ComVisible( false )]

//ローカライズ可能なアプリケーションのビルドを開始するには、
//.csproj ファイルの <UICulture>CultureYouAreCodingWith</UICulture> を
//<PropertyGroup> 内部で設定します。たとえば、
//ソース ファイルで英語を使用している場合、<UICulture> を en-US に設定します。次に、
//下の NeutralResourceLanguage 属性のコメントを解除します。下の行の "en-US" を
//プロジェクト ファイルの UICulture 設定と一致するよう更新します。

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
	ResourceDictionaryLocation.None , //テーマ固有のリソース ディクショナリが置かれている場所
	//(リソースがページ、
	//またはアプリケーション リソース ディクショナリに見つからない場合に使用されます)
	ResourceDictionaryLocation.SourceAssembly //汎用リソース ディクショナリが置かれている場所
	//(リソースがページ、
	//アプリケーション、またはいずれのテーマ固有のリソース ディクショナリにも見つからない場合に使用されます)
)]


// アセンブリのバージョン情報は、以下の 4 つの値で構成されています:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion( "0.1.3.0" )]
//[assembly: AssemblyFileVersion( "1.0.0.0" )]

// アセンブリカスタム属性にビルド時の構成を埋め込む
#if DEBUG
[assembly: AssemblyConfiguration( "Debug" )]
#else
[assembly: AssemblyConfiguration( "Release" )]
#endif


// カスタム属性で開発フェーズを埋め込む
// 手動で設定、基本的に前に戻らない
// Alpha     プロトタイプ中
// Beta      開発中（機能登載中）
// RC        取説作成時（基本的に機能実装済みで検証中のとき、ただしそこで機能追加はあり得る）
// Release   リリース
[assembly: Trixie.DevelopPhase( "Beta" )]


// バージョン情報をこのファイルだけで管理できるようにする
// namespaceにプロジェクトの既定の名前空間を書かなければならない
namespace Trixie
{
	// カスタム属性
	[System.AttributeUsage( System.AttributeTargets.Assembly , Inherited=true )]
	public class DevelopPhaseAttribute : System.Attribute
	{
		public string Phase { get; private set; }

		public DevelopPhaseAttribute( string phase )
		{
			Phase = phase;
		}
	}

	// バージョン情報取得
	public static class VerInfo
	{
		public static readonly string Name;
		public static readonly System.Version Version;
		public static readonly string DevPhase;
		public static readonly string Config;

		static VerInfo()
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			Name = assembly.GetName().Name;
			Version = assembly.GetName().Version;

			object[] devph = assembly.GetCustomAttributes( typeof( DevelopPhaseAttribute ) , false );
			if ( devph.Length > 0 )
			{
				DevPhase = (devph[0] as DevelopPhaseAttribute).Phase;
			}

			object[] config = assembly.GetCustomAttributes( typeof( System.Reflection.AssemblyConfigurationAttribute ) , false );
			if ( config.Length > 0 )
			{
				Config = (config[0] as System.Reflection.AssemblyConfigurationAttribute).Configuration;
			}
		}
	}
}
