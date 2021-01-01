"use strict";

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

// keep in mind that the connection url is hard coded for now
var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:49499/messagehub", {
    accessTokenFactory: () => getCookie("access_token")
}).build();

connection.on("ReceiveMessage", function (user, message) {

    // finding the message place
    var messages = document.getElementById("chat-message-list");
    if (messages == null) {
        alert("null");
        return;
    }

    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    // we are not using the user variable for now

    // creating the message html object
    var message = document.createElement("div");
    message.classList.add("message-row");
    message.classList.add("other-message");

    var message_content = document.createElement("div");
    message_content.classList.add("message-content");

    var image = document.createElement("img");

    // image source will be hard coded for now
    image.src = window.location.protocol + "//" + window.location.host + "/images/user2.png";
    image.width = "40";
    image.height = "40";

    message_content.appendChild(image);

    var message_content_text = document.createElement("div");
    message_content_text.classList.add("message-text");

    message_content_text.textContent = msg;

    message_content.appendChild(message_content_text);

    var message_content_date = document.createElement("div");
    message_content_date.classList.add("message-time");
    // we will leave the date hard coded for now
    message_content_date.textContent = "Now";

    message_content.appendChild(message_content_date);

    message.appendChild(message_content);

    messages.appendChild(message);

    // scrolling down to the last message
    messages.scrollTop = message.offsetTop;
});

connection.on("SendMessage", function (message) {

    // finding the message place
    var messages = document.getElementById("chat-message-list");
    if (messages == null) {
        alert("null");
        return;
    }

    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    
    // creating the message html object
    var message = document.createElement("div");
    message.classList.add("message-row");
    message.classList.add("you-message");

    var message_content = document.createElement("div");
    message_content.classList.add("message-content");
    var message_content_text = document.createElement("div");
    message_content_text.classList.add("message-text");

    message_content_text.textContent = msg;
    
    message_content.appendChild(message_content_text);
    
    var message_content_date = document.createElement("div");
    message_content_date.classList.add("message-time");
    // we will leave the date hard coded for now
    message_content_date.textContent = "Now";

    message_content.appendChild(message_content_date);

    message.appendChild(message_content);

    messages.appendChild(message);

    // scrolling down to the last message
    messages.scrollTop = message.offsetTop;
});

connection.on("EnterConversation", function (conversation_id) {
    alert("conversation id is: " + conversation_id);
});

connection.on("alert", function (message) {
    alert(message);
});

connection.start().then(function () {
    // there goes the init
    var send_btn = document.getElementById("send_btn");
    if (send_btn == null) {
        alert("send button not found!");
        return;
    }

    send_btn.addEventListener("click", function (event) {

        // checking if we had typed something
        var input = document.getElementById("message_input");
        if (input == null) {
            return; // then we have nothing to send
        }

        // checking for empty message
        if (input.value.length == 0) {
            return; // then we have nothing to send
        }

        var user = document.getElementById("username").value;
        var message = input.value;

        input.value = "";

        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });

        // Search conversation part

        var search_conversation_container = document.getElementById("search-container");

        if (search_conversation_container == null) {
            alert("search conversation container not found");
            return;
        }

        var search_conversation_input = document.getElementById("search-conversation-input");

        if (search_conversation_input == null) {
            alert("search conversation input not found");
            return;
        }

    });

    // keydown events
    document.addEventListener("keydown", function (event) {
        if (event.keyCode === 13 && event.shiftKey) {
            // here we should add a new line
        } else if (event.keyCode === 13) {
            send_btn.click();
        }
    });

}).catch(function (err) {
    return console.error(err.toString());
});
