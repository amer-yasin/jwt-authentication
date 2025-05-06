export default function authHeader() {
    const user = JSON.parse(localStorage.getItem("user"));
    const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    const screenResolution = `${window.screen.width}x${window.screen.height}`;
    const browserLanguage = navigator.language || navigator.userLanguage;

    const headers = {
        'X-TimeZone': timeZone,
        'X-Screen-Resolution': screenResolution,
        'X-Browser-Language': browserLanguage
    };
    
    if (user && user.accessToken) {
        headers.Authorization = 'Bearer ' + user.accessToken;
        return headers;
    } else {
        return {};
    }
}
