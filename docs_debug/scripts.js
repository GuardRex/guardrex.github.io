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

  var colors = new Array([62,35,255],[60,255,60],[255,35,98],[45,175,230],[255,0,255],[255,128,0]);
  var step = 0;
  var colorIndices = [0,1,2,3];
  var gradientSpeed = 0.002;
  
  function updateGradient()
  {
    var c0_0 = colors[colorIndices[0]];
    var c0_1 = colors[colorIndices[1]];
    var c1_0 = colors[colorIndices[2]];
    var c1_1 = colors[colorIndices[3]];
    var istep = 1 - step;
    var r1 = Math.round(istep * c0_0[0] + step * c0_1[0]);
    var g1 = Math.round(istep * c0_0[1] + step * c0_1[1]);
    var b1 = Math.round(istep * c0_0[2] + step * c0_1[2]);
    var color1 = "rgb("+r1+","+g1+","+b1+")";
    var r2 = Math.round(istep * c1_0[0] + step * c1_1[0]);
    var g2 = Math.round(istep * c1_0[1] + step * c1_1[1]);
    var b2 = Math.round(istep * c1_0[2] + step * c1_1[2]);
    var color2 = "rgb("+r2+","+g2+","+b2+")";

    document.getElementById("gradient").style.background = "-webkit-gradient(linear, left top, right top, from("+color1+"), to("+color2+"))";
    document.getElementById("gradient").style.background = "-moz-linear-gradient(left, "+color1+" 0%, "+color2+" 100%)";
  
    step += gradientSpeed;
    
    if ( step >= 1 ) {
      step %= 1;
      colorIndices[0] = colorIndices[1];
      colorIndices[2] = colorIndices[3];
      colorIndices[1] = ( colorIndices[1] + Math.floor( 1 + Math.random() * (colors.length - 1))) % colors.length;
      colorIndices[3] = ( colorIndices[3] + Math.floor( 1 + Math.random() * (colors.length - 1))) % colors.length;
    }
  }
  
  if ( document.getElementById("gradient") != null ) {
    document.getElementById("header_container").style.borderBottom = "none";
    setInterval(updateGradient,10);
  }

  registerListener('load', setLazy);
  registerListener('load', lazyLoad);
  registerListener('scroll', lazyLoad);
  
  var lazy = [];
  
  function setLazy(){
      lazy = document.getElementsByClassName('lazy');
      console.log('Found ' + lazy.length + ' lazy images');
  } 
  
  function lazyLoad(){
      for(var i=0; i<lazy.length; i++){
          if(isInViewport(lazy[i])){
              if (lazy[i].getAttribute('data-src')){
                  lazy[i].src = lazy[i].getAttribute('data-src');
                  lazy[i].removeAttribute('data-src');
              }
          }
      }
      
      cleanLazy();
  }
  
  function cleanLazy(){
      lazy = Array.prototype.filter.call(lazy, function(l){ return l.getAttribute('data-src');});
  }
  
  function isInViewport(el){
      var rect = el.getBoundingClientRect();
      
      return (
          rect.bottom >= 0 && 
          rect.right >= 0 && 
          rect.top <= (window.innerHeight || document.documentElement.clientHeight) && 
          rect.left <= (window.innerWidth || document.documentElement.clientWidth)
       );
  }
  
  function registerListener(event, func) {
      if (window.addEventListener) {
          window.addEventListener(event, func)
      } else {
          window.attachEvent('on' + event, func)
      }
  }

})();
