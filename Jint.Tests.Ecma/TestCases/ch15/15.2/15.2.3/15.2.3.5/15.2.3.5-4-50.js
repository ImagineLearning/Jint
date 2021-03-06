/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch15/15.2/15.2.3/15.2.3.5/15.2.3.5-4-50.js
 * @description Object.create - 'enumerable' property of one property in 'Properties' is own data property that overrides an inherited data property (8.10.5 step 3.a)
 */


function testcase() {

        var accessed = false;
        var proto = {
            enumerable: true
        };

        var ConstructFun = function () { };
        ConstructFun.prototype = proto;
        var descObj = new ConstructFun();

        Object.defineProperty(descObj, "enumerable", {
            value: false
        });

        var newObj = Object.create({}, {
            prop: descObj 
        });

        for (var property in newObj) {
            if (property === "prop") {
                accessed = true;
            }
        }
        return !accessed;
    }
runTestCase(testcase);
