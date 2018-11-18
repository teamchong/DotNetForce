window.DNF = {
    toLocalhost(port) {
        location.href = location.href.replace('https://ste80.github.io/DotNetForce', `http://localhost:${port}`);
    },
    getStorage(key) {
        return sessionStorage.getItem(key);
    },
    setStorage(key, value) {
        sessionStorage.setItem(key, value);
    },
    addEventListener(type, callback) {
        addEventListener(type, function (ev) {
            var result = callback.invokeMethodAsync('Callback', type, JSON.stringify(ev));
            if (result === 'false') {
                return false;
            }
        });
    },
};