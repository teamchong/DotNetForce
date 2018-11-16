window.jsFunctions = {
    addEventListener: function (type, callback) {
        window.addEventListener(type, function (e) {
            DotNet.invokeMethodAsync('BlazorForce', callback, JSON.stringify(e));
        });
    }
};