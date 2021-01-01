function setCookie(name, value) {
    var expires = "; expires=session";
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

var token = getCookie("access_token");
var refreshToken = getCookie("refresh_token");

const url = location.protocol + "//" + location.host + "/GetTokenExpirationTime";

var data =
{
    "token": token,
    "refreshToken": refreshToken
}

var workerData =
{
    "expirationTimeUrl": url,
    "refreshUrl": location.protocol + "//" + location.host + "/Refresh",
    "data": data
}

var worker = new Worker("../js/tokens.js", {
    'name': 'refreshTokenThread'
});

worker.addEventListener('message', function (event) {
    var list = event.data.split(" ");
    if (list.length > 0) {
        if (list[0] === "token" && list.length > 1) {
            setCookie("access_token", list[1]);
        } else if (list[0] === "refreshToken" && list.length > 1) {
            setCookie("refresh_token", list[1]);
        } else if (event.data === "need data") {
            var d =
            {
                "token": getCookie("access_token"),
                "refreshToken": getCookie("refresh_token")
            }
            var data =
            {
                "expirationTimeUrl": location.protocol + "//" + location.host + "/GetTokenExpirationTime",
                "refreshUrl": location.protocol + "//" + location.host + "/Refresh",
                "data": d
            }
            worker.postMessage(data);
        }
    }
});

//worker.postMessage("stop"); // stop command
worker.postMessage(workerData);
