window.DNF = {
    history: {
        back() {
            history.back();
        },
        forward() {
            history.forward();
        },
        go(delta) {
            history.go(delta);
        },
        pushState(data, title, url) {
            history.pushState(data, title, location.pathname + url);
        },
        replaceState(data, title, url) {
            history.replaceState(data, title, location.pathname + url);
        }
    },
    toLocalhost(port) {
        if (/^https:\/\/ste80\.github\.io\/DotNetForce/.test(location.href)) {
            history.replaceState(null, null, location.href.replace(/^https:\/\/ste80\.github\.io\/DotNetForce/, `http://localhost:${port}`));
        }
    },
    getSessionStorage(key) {
        return sessionStorage.getItem(key);
    },
    setSessionStorage(key, value) {
        sessionStorage.setItem(key, value);
    },
    getLocalStorage(key) {
        return localStorage.getItem(key);
    },
    setLocalStorage(key, value) {
        localStorage.setItem(key, value);
    },
    addEventListener(type, callback) {
        addEventListener(type, function (ev) {
            var result = callback.invokeMethodAsync('Callback', type, JSON.stringify(ev));
            if (result === 'false') {
                return false;
            }
        });
    }
};