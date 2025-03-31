export default function authHeader() {
    const user = JSON.parse(localStorage.getItem("user"));
    const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

    const headers = {
        'X-TimeZone': timeZone
    };
    
    if (user && user.accessToken) {
        headers.Authorization = 'Bearer ' + user.accessToken ;
        return headers;
    } else {
        return {};
    }
}
