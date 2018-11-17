window.jsInterop = {
    openAuthWindow: function (url) {
        open(url, 'DotNetForceOAuth', 'width=600,height=800');
    },
    getStorage: function (key) {
        return sessionStorage.getItem(key);
    },
    setStorage: function (key, value) {
        sessionStorage.setItem(key, value);
    },
    waitForForSession: function (key, callback) {
        requestAnimationFrame(function () {
            var value = sessionStorage.getItem(key);
            if (value) {
                var result = callback.invokeMethodAsync('Callback', key, value);
                if (value != result) {
                    sessionStorage.setItem(key, result);
                }
            }
            waitForForSession(key, callback);
        });
    },
    addEventListener: function (type, callback) {
        addEventListener(type, function (ev) {
            var result = callback.invokeMethodAsync('Callback', type, JSON.stringify(ev));
            if (result === 'false') {
                return false;
            }
        });
    }
};