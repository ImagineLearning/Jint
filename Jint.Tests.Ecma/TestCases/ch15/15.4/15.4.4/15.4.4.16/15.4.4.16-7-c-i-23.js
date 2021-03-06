/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch15/15.4/15.4.4/15.4.4.16/15.4.4.16-7-c-i-23.js
 * @description Array.prototype.every - This object is an global object which contains index property
 */


function testcase() {

        function callbackfn(val, idx, obj) {
            if (idx === 0) {
                return val !== 11;
            } else {
                return true;
            }
        }

        try {
            var oldLen = fnGlobalObject().length;
            fnGlobalObject()[0] = 11;
            fnGlobalObject().length = 1;
            return !Array.prototype.every.call(fnGlobalObject(), callbackfn);
        } finally {
            delete fnGlobalObject()[0];
            fnGlobalObject().length = oldLen;
        }
    }
runTestCase(testcase);
