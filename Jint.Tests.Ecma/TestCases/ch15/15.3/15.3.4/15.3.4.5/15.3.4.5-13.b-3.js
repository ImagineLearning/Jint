/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch15/15.3/15.3.4/15.3.4.5/15.3.4.5-13.b-3.js
 * @description Function.prototype.bind, 'length' set to remaining number of expected args (all args prefilled)
 */


function testcase() {
  function foo(x, y) { }
  var o = {};
  
  var bf = foo.bind(o, 42, 101);
  if (bf.length === 0) {
    return true;
  }
 }
runTestCase(testcase);
