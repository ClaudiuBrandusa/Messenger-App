const toUrlEncoded = obj => Object.keys(obj).map(k => k + '=' + obj[k]).join('&');

var data; // token and refreshToken in an object
var expirationTimeUrl; // url to get token expiration time endpoint
var refresh; // url to refresh token endpoint

async function getTokenExpirationTime(data, url) {
    let value = await fetch(url, {
        method: 'POST',
        credentials: 'include',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: toUrlEncoded(data)
    })
        .then(response => response.body)
        .then(response => response.getReader())
        .then((reader) => reader.read()
            .then(({ done, value }) => new TextDecoder("utf-8").decode(value))
            .then(val => parseFloat(val))
            .catch(error => console.error(error)));

    return await value;
};

async function getNewToken() {
    let token = await fetch(self.refresh, {
        method: 'POST',
        credentials: 'include',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: toUrlEncoded(self.data)
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                sendNewToken("status is: " + response.status); return "undefined"; // something went wrong
            }
                })
            .catch(error => console.error(error));
    return await token;
};

async function requestRefreshToken() {
    getTokenExpirationTime(self.data, self.expirationTimeUrl)
        .then(val => {
            if (isNaN(val)) return;
            if ((val * 1000) < 1000 * 60 * 5) {// if the expire time is less than five minutes then we refresh it
                
                // getting the new token
                getNewToken()
                    .then(val => {
                        if (val != "undefined") {
                            sendNewToken(val.token);
                            sendNewRefreshToken(val.refreshToken);
                        } // else something went wrong
                    });
                // after we change the token, we will need new credentials
                self.postMessage("need data");
            } // else we do not need to refresh the token
            setTimeout("requestRefreshToken()", 1000 * 60 * 0.3); // requesting new data at every 4 minutes
            return val;
        }
    );
};

function sendNewToken(token) {
    self.postMessage("token " + token);
};

function sendNewRefreshToken(token) {
    self.postMessage("refreshToken " + token);
};

function start(data, expirationTimeUrl, refresh) { // updating the input data
    self.data = data;
    self.expirationTimeUrl = expirationTimeUrl;
    self.refresh = refresh;
};

var status = false; 

self.onmessage = function (event) {
    if (event.data === "stop") {
        self.terminate;
    } else {
        start(event.data.data, event.data.expirationTimeUrl, event.data.refreshUrl);
        if (!status) { // checking if we have started the loop
            status = true;
            requestRefreshToken();
        }
    }
};
