// Copyright (c) 2013 2013 Mizutama(水玉 ◆qHK1vdR8FRIm)
// This script is licensed under the MIT license.  See
// http://opensource.org/licenses/mit-license.php for more details.
//
// ==UserScript==
// @name          Google検索結果直リン
// @namespace     http://www.bhelpuri.net/Trixie/qHK1vdR8FRIm
// @description	  Googleの検索結果のリンクには変なリダイレクトが仕込まれてて
// @description	  IE11の「戻る」で戻れず、しつこく「戻る」しているとIEがクラッシュするので
// @description	  リンクを直リン化。ただリンクを書き換えてもGoogleJSがリダイレクトに戻すので
// @description	  元のリンクを邪魔にならないようによけて直リンを打ち込む
// @include       https://www.google.*/*
// ==/UserScript==

(function() 
{
	function googleDeLink()
	{
	    var alinks = document.getElementsByTagName( "a" );
	    var links = new Array();
	    for ( var i=0; i<alinks.length; i++ )
	    {
	        var mousedown = alinks[i].getAttribute( "onmousedown" );
	        if ( !alinks[i].marked && mousedown && mousedown.substring( 0 , 10 ) == "return rwt" )
	        {
	            links.push( alinks[i] );
	        }
	    }

	    for ( var i=0; i<links.length; i++ )
	    {
	        var dlink = document.createElement( "a" );
	        dlink.href = links[i].href;
	        dlink.innerHTML = links[i].innerHTML;
	        links[i].parentElement.appendChild( dlink );

	        links[i].innerHTML = "g";
	        links[i].style.fontSize = "8pt";
	        links[i].marked = true;
	    }

	    setTimeout( googleDeLink , 1000 );
	}
	googleDeLink();
	var div = '<div id="myOwnUniqueId12345" style="position:' + 
                 'fixed;top:70px;left:0px;z-index:9999;width=300px;' + 
                 'height=150px;">GoogleDe-Link</div>';
	document.body.insertAdjacentHTML( "afterBegin" , div );
})();
