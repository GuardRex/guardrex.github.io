/*!
 * IE10 viewport hack for Surface/desktop Windows 8 bug
 * Copyright 2014-2015 Twitter, Inc.
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 */

// See the Getting Started docs for more information:
// http://getbootstrap.com/getting-started/#support-ie10-width

(function () {
  'use strict';

  if (navigator.userAgent.match(/IEMobile\/10\.0/)) {
    var msViewportStyle = document.createElement('style')
    msViewportStyle.appendChild(
      document.createTextNode(
        '@-ms-viewport{width:auto!important}'
      )
    )
    document.querySelector('head').appendChild(msViewportStyle)
  }

  var c = new Array([62,35,255],[60,255,60],[255,35,98],[45,175,230],[255,0,255],[255,128,0]);
  var s = 0;
  var ci = [0,1,2,3];
  var s = 0.002;

  function updateGradient()
  {
    var c0_0 = c[ci[0]];
    var c0_1 = c[ci[1]];
    var c1_0 = c[ci[2]];
    var c1_1 = c[ci[3]];
    var is = 1 - s;
    var r1 = Math.round(is * c0_0[0] + s * c0_1[0]);
    var g1 = Math.round(is * c0_0[1] + s * c0_1[1]);
    var b1 = Math.round(is * c0_0[2] + s * c0_1[2]);
    var c1 = "rgb("+r1+","+g1+","+b1+")";
    var r2 = Math.round(is * c1_0[0] + s * c1_1[0]);
    var g2 = Math.round(is * c1_0[1] + s * c1_1[1]);
    var b2 = Math.round(is * c1_0[2] + s * c1_1[2]);
    var c2 = "rgb("+r2+","+g2+","+b2+")";

    document.getElementById("gradient").style.background = "-webkit-gradient(linear, left top, right top, from("+c1+"), to("+c2+"))";
    document.getElementById("gradient").style.background = "-moz-linear-gradient(left, "+c1+" 0%, "+c2+" 100%)";

    s += s;

    if ( s >= 1 ) {
        s %= 1;
        ci[0] = ci[1];
        ci[2] = ci[3];
        ci[1] = ( ci[1] + Math.floor( 1 + Math.random() * (c.length - 1))) % c.length;
        ci[3] = ( ci[3] + Math.floor( 1 + Math.random() * (c.length - 1))) % c.length;
    }
  }
  
  setInterval(updateGradient,10);

})();
