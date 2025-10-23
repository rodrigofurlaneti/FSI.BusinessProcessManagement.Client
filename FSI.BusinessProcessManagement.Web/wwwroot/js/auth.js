window.appAuth = {
    getToken: function () {
        try { return localStorage.getItem('auth_token') || null; }
        catch { return null; }
    }
};
