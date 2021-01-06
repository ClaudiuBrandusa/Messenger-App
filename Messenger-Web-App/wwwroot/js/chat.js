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

function formatString(string, length_limit) {
    if (string == null) {
        return "";
    }
    if (length_limit == null) {
        return string;
    }
    return string.length > length_limit ? string.substring(0, length_limit) : string;
}

// used in the conversations list
function formatConversationTitle(string) {
    if (string == null) {
        return;
    }
    return formatString(string, 20) + (string.length > 20 ? "..." : "");
}

// used in chat room conversation
function formatConversationRoomTitle(string) {
    if (string == null) {
        return;
    }
    return formatString(string, 20) + (string.length > 20 ? "..." : "");
}

function formatConversationLastMessage(string) {
    if (string == null) {
        return;
    }
    return formatString(string, 20) + (string.length > 20 ? "..." : "");
}

// warning block

function createConversationWarningBlock(message) {
    let block = document.createElement("div");
    block.classList.add("warning");
    block.addEventListener("click", function (event) {
        hideConversationSearchResults();
        renderConversationsList();
    });

    let title_block = document.createElement("h4");
    title_block.innerText = message;

    block.appendChild(title_block);

    let description = document.createElement("h6");
    description.innerText = "Click here to go back to conversation history";

    block.appendChild(description);
    return block;
}

function createNewConversationsHeader(title) {
    let block = document.createElement("div");
    block.classList.add("warning");

    let title_block = document.createElement("h4");
    title_block.innerText = title;

    block.appendChild(title_block);
    return block;
}

// new message notification
function notifyNewMessage(conversation_Id, message) {
    if (conversations_list == null) {
        let conversations_list = [];
        return;
    }
    if (!conversations_list.includes(conversation_Id)) {
        // request the new conversation
        // then recall the function
        return;
    }

    // make the conversation highlighted with the new message
    let conversation_block = getElementById(conversation_Id);
    if (conversation_block == null) {
        alert("not found");
    }
    // ToDo

    // play the notification sound
}

// Messages
function renderMessages(messages) {
    for (var i = 0; i < messages.length; i++) {
        renderMessage(messages[i]);
    }
}

function renderMessage(message) {
    if (message == null) return;

    if (message.sender === "") { // then it's a sent message
        renderSentMessage(message.content);
    } else {

    }
}

function renderSentMessage(message) {
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
}


// SignalR Client data
let conversationId = "";
let conversations_list = [];
let contacts_search_result = [];
let conversations_search_result = [];

let hasSearchedConversation = false;
let clickedOnConversationsList = false;

function renderConversationsList() {
    var conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list container not found");
        return;
    }

    if (conversations_list == null) {
        // here we should requesting the conversations list
        alert("conversations list not found");
        return;
    }

    conversations_list_container.innerHTML = "";
    
    for (var i = 0; i < conversations_list.length; i++) {
        renderConversationInList(conversations_list[i]);
    }
}

function renderConversationInList(conversationData) {
    var conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list container not found");
        return;
    }

    var conversation_block = document.createElement("div");
    conversation_block.id = conversationData.id;
    conversation_block.classList.add("conversation");
    conversation_block.addEventListener("click", function (event) {
        requestConversation(conversationData.id);
    })
    var conversation_image = document.createElement("img");

    conversation_image.src = window.location.protocol + "//" + window.location.host + "/images/user2.png";
    conversation_image.width = "40";
    conversation_image.height = "40";

    var conversation_title = document.createElement("div");
    conversation_title.classList.add("title-text");
    conversation_title.innerHTML = formatConversationTitle(conversationData.conversationName);
    var conversation_data = document.createElement("div"); // last message data
    conversation_data.classList.add("created-date");
    conversation_data.innerHTML = "";
    var conversation_message = document.createElement("div"); // an excerpt from the last message
    conversation_message.classList.add("conversation-message");
    conversation_message.innerHTML = formatConversationLastMessage("");

    conversation_block.appendChild(conversation_image);
    conversation_block.appendChild(conversation_title);
    conversation_block.appendChild(conversation_data);
    conversation_block.appendChild(conversation_message);

    conversations_list_container.appendChild(conversation_block);
}

function hideConversationsList() {
    let conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list not found");
        return;
    }

    conversations_list_container.innerHTML = "";
}

function addConversationsList(conversationList) {
    if (conversations_list == null) {
        let conversations_list = [];
    }

    for (var i = 0; i < conversationList.length; i++) {
        if (conversationList[i] != null && !conversations_list.includes(conversationList[i])) {
            conversations_list.push(conversationList[i]);
        }
    }
}

function addConversationInList(conversationData) {
    if (conversations_list == null) {
        let conversations_list = [];
    }

    let containsObject = false;

    for (var i = 0; i < conversations_list.length; i++) {
        if (conversations_list[i].id === conversationData.id) {
            containsObject = true;
            break;
        }
    }

    if (containsObject) {
        //alert("conversation already there");
        return;
    }

    conversations_list.push(conversationData);

    renderConversationsList();
}

// conversation search results

function showConversationSearchResults(no_clear) {
    let conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list container not found");
        return;
    }

    if (conversations_search_result == null) {
        let conversations_search_result = []; // then we create another list
        return;
    }
    alert("before");
    if (no_clear == null) conversations_list_container.innerHTML = "";
    alert("after");
    let backButton = createConversationWarningBlock("");

    conversations_list_container.appendChild(backButton);

    let conversationsListHeader = createNewConversationsHeader("Conversations");

    conversations_list_container.appendChild(conversationsListHeader);

    for (let i = 0; i < conversations_search_result.length; i++) {
        renderConversationSearchResult(conversations_search_result[i]);
    }
}

function addConversationsToSearchResults(conversations) {
    if (conversations_search_result == null) {
        let conversations_search_result = [];
    } else {
        conversations_search_result = [];
    }

    if (conversations == null || conversations.length < 1) return null;

    for (var i = 0; i < conversations.length; i++) {
        addConversationToSearchResults(conversations[i]);
    }

    return conversations.length;
}

function addConversationToSearchResults(conversation) {
    if (conversations_search_result == null) {
        let conversations_search_result = [];
        return;
    }

    if (conversation != null && !conversations_search_result.includes(conversation)) {
        conversations_search_result.push(conversation);
    }
}

function renderConversationSearchResult(result) {
    var conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list container not found");
        return;
    }

    var conversation_block = document.createElement("div");
    conversation_block.classList.add("conversation");
    conversation_block.addEventListener("click", function (event) {
        requestConversation(result.id);
    })
    var conversation_image = document.createElement("img");

    conversation_image.src = window.location.protocol + "//" + window.location.host + "/images/user2.png";
    conversation_image.width = "40";
    conversation_image.height = "40";

    var conversation_title = document.createElement("div");
    conversation_title.classList.add("title-text");
    conversation_title.innerHTML = formatConversationTitle(result.conversationName);
    var conversation_data = document.createElement("div"); // last message data
    conversation_data.classList.add("created-date");
    conversation_data.innerHTML = "";
    var conversation_message = document.createElement("div"); // an excerpt from the last message
    conversation_message.classList.add("conversation-message");
    conversation_message.innerHTML = formatConversationLastMessage("");

    conversation_block.appendChild(conversation_image);
    conversation_block.appendChild(conversation_title);
    conversation_block.appendChild(conversation_data);
    conversation_block.appendChild(conversation_message);

    conversations_list_container.appendChild(conversation_block);
}

function hideConversationSearchResults() {
    let conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list not found");
        return;
    }

    let search_conversation_input = document.getElementById("search-conversation-input");

    if (search_conversation_input != null) {
        search_conversation_input.value = "";
    }

    conversations_list_container.innerHTML = "";
}

function showLoadingConversationsList() {
    let conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list not found");
        return;
    }

    let conversations_loading_container = document.getElementById("conversations-loading-container");
    if (conversations_loading_container == null) {
        conversations_loading_container = document.createElement("div");
        conversations_loading_container.id = "conversations-loading-container";
    }

    let conversations_loading = document.getElementById("conversations-loading");
    if (conversations_loading == null) {
        conversations_loading = document.createElement("i");
        conversations_loading.classList.add("fas");
        conversations_loading.classList.add("fa-spinner");
        conversations_loading.classList.add("fa-pulse");
        conversations_loading.id = "conversations-loading";
    }

    if (conversations_loading.parentNode != conversations_loading_container) {
        conversations_loading_container.appendChild(conversations_loading);
    }

    let conversations_loading_message = document.getElementById("conversations-loading-message");
    if (conversations_loading_message == null) {
        conversations_loading_message = document.createElement("conversations-loading-message");
        conversations_loading_message.id = "conversations-loading-message";
        conversations_loading_message.innerText = "Loading results";
    }

    if (conversations_loading_message.parentNode != conversations_loading_container) {
        conversations_loading_container.appendChild(conversations_loading_message);
    }

    if (conversations_loading_container.parentNode != conversations_list) {
        conversations_list_container.appendChild(conversations_loading_container);
    }
}

function hideLoadingConversationsList() {
    let conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list not found");
        return;
    }

    let conversations_loading_container = document.getElementById("conversations-loading-container");
    if (conversations_loading_container == null) {
        return;
    }

    conversations_loading_container.parentNode.removeChild(conversations_loading_container);
}

// contacts found
function addContactsToSearchResults(found_contacts_list) {
    if (contacts_search_result == null) {
        let contacts_search_result = [];
    } else {
        contacts_search_result = [];
    }

    if (found_contacts_list == null || found_contacts_list.length < 1) return null;
    
    for (var i = 0; i < found_contacts_list.length; i++) {
        addContactToSearchResults(found_contacts_list[i]);
    }
}

function addContactToSearchResults(contact) {
    if (contacts_search_result == null) {
        let contacts_search_result = [];
    }

    if (contact != null && !contacts_search_result.includes(contact)) {
        contacts_search_result.push(contact);
    }
}

function showContactsSearchResults() {
    let conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list container not found");
        return;
    }

    if (contacts_search_result == null) {
        let contacts_search_result = []; // then we create another list
        return;
    }

    if (no_clear == null) {
        conversations_list_container.innerHTML = "";
    }

    let backButton = createConversationWarningBlock("");

    conversations_list_container.appendChild(backButton);

    let contactsListHeader = createNewConversationsHeader("Contacts");

    conversations_list_container.appendChild(contactsListHeader);

    for (let i = 0; i < contacts_search_result.length; i++) {
        showContactsSearchResults(contacts_search_result[i]);
    }
}

function showContactsSearchResults(contact) {
    var conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list container not found");
        return;
    }

    var conversation_block = document.createElement("div");
    conversation_block.classList.add("conversation");
    conversation_block.addEventListener("click", function (event) {
        requestConversation(contact.id);
    })
    var conversation_image = document.createElement("img");

    conversation_image.src = window.location.protocol + "//" + window.location.host + "/images/user2.png";
    conversation_image.width = "40";
    conversation_image.height = "40";

    var conversation_title = document.createElement("div");
    conversation_title.classList.add("title-text");
    conversation_title.innerHTML = formatConversationTitle(contact.conversationName);
    var conversation_data = document.createElement("div"); // last message data
    conversation_data.classList.add("created-date");
    conversation_data.innerHTML = "";
    var conversation_message = document.createElement("div"); // an excerpt from the last message
    conversation_message.classList.add("conversation-message");
    conversation_message.innerHTML = formatConversationLastMessage("");

    conversation_block.appendChild(conversation_image);
    conversation_block.appendChild(conversation_title);
    conversation_block.appendChild(conversation_data);
    conversation_block.appendChild(conversation_message);

    conversations_list_container.appendChild(conversation_block);
}

// chat room conversations
function requestConversation(id) {
    connection.invoke("OpenConversation", id);
}

function enterConversation(data) {
    if (conversationId == null) {
        let conversationId = data.id;
    }
    
    if (conversationId === data.id) {
        /*alert(conversationId + " " + data.conversationName);*/
        // then we are already there
        return;
    }

    conversationId = data.id;
    
    // set conversation title
    var conversation_title = document.getElementById("chat-title");
    if (conversation_title == null) {
        alert("no conversation title found");
        return;
    }
    conversation_title.innerText = formatConversationRoomTitle(data.conversationName);

    // clear conversation message history
    var chat_message_list = document.getElementById("chat-message-list");
    if (chat_message_list == null) {
        alert("chat message list not found");
        return;
    }

    chat_message_list.innerHTML = "";

    if (data.messages != null) {
        renderMessages(data.messages);
    }
}

// keep in mind that the connection url is hard coded for now
var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:49499/messagehub", {
    accessTokenFactory: () => getCookie("access_token")
}).build();


connection.on("ReceiveMessage", function (conversation_Id, message) {
    // checking the conversation's id
    if (conversationId == null) {
        let conversationId = "";
        return;
    }

    if (conversationId !== conversation_Id) {
        // send notification
        notifyNewMessage(conversation_Id, formatConversationLastMessage(message));
        return;
    } // else we are on the current conversation

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

connection.on("SendMessage", function (conversation_Id, message) {
    // checking the conversation's id
    if (conversationId == null) {
        let conversationId = "";
        return;
    }

    if (conversationId !== conversation_Id) {
        return; // then we are not in the current conversation room, might happen if we are connected on multiple accounts
    }

    renderSentMessage(message);
});

connection.on("EnterConversation", function (conversation_data) {
    enterConversation(conversation_data);
    
    addConversationInList(conversation_data);
});

connection.on("ListConversations", function (new_conversations_list) {
    var conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversation list container not found");
        return;
    }

    addConversationsList(new_conversations_list);

    renderConversationsList();
});

// list all of the conversations found after the search conversation request
connection.on("ListFoundConversations", function (found_conversations_list, found_contacts_list) {
    hideLoadingConversationsList();
    if (addConversationsToSearchResults(found_conversations_list) == null && (found_contacts_list == null || found_contacts_list.length == 0)) {
        var conversations_list_container = document.getElementById("conversation-list");
        if (conversations_list_container == null) {
            alert("conversations list container not found");
            return;
        }

        let warning_block = createConversationWarningBlock("No conversation found");
        
        conversations_list_container.appendChild(warning_block);

        return;
    }
    //alert(found_conversations_list);
    //alert(found_contacts_list);
    addContactsToSearchResults(found_contacts_list);

    showContactsSearchResults();

    //showConversationSearchResults(true);
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

        if (conversationId == null) { let conversationId = ""; }

        var message = input.value;

        input.value = "";

        connection.invoke("SendMessage", conversationId, message).catch(function (err) {
            return console.error(err.toString());
        });
    });

    // Search contact & conversation part

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

    search_conversation_input.addEventListener("click", function (event) {
        
    });

    search_conversation_input.addEventListener("keydown", function (event) {
        if (event.keyCode === 13 && search_conversation_button != null) {
            search_conversation_button.click();
        }
    });

    var search_conversation_button = document.getElementById("conversation-search-button");
    if (search_conversation_button == null) {
        alert("conversation search button not found");
        return;
    }

    search_conversation_button.addEventListener("click", function (event) {
        if (conversations_list_container == null) {
            alert("conversations list container not found");
            return;
        }

        if (search_conversation_input == null) {
            alert("search conversation input not found");
            return;
        }

        var input = search_conversation_input.value;

        if (input.length < 1) {
            if (hasSearchedConversation != null && hasSearchedConversation) {
                hasSearchedConversation = false;
                renderConversationsList();
            }
            return; // nothing to search
        }
        hideConversationsList();
        hasSearchedConversation = true;
        showLoadingConversationsList();
        connection.invoke("FindConversation", input);
    });

    // New conversation part

    var new_conversation_container = document.getElementById("new-conversation-container");
    if (new_conversation_container == null) {
        alert("new conversation container not found");
        return;
    }

    var new_conversation_button = document.getElementById("new-conversation-button");
    if (new_conversation_button == null) {
        alert("new conversation button not found");
        return;
    }

    new_conversation_button.style.cursor = "pointer";

    new_conversation_button.addEventListener("click", function (event) {
        // open the new conversation menu
        connection.invoke("CreateNewConversation").catch(function (err) {
            return console.error(err.toString());
        });
    });

    // new contact part
        // ToDo
    // user settings part
        // ToDo
    // status part
        // ToDo

    // conversations list
    var conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversation list container not found");
        return;
    }

    conversations_list_container.innerHTML = "";

    if (conversations_list == null) {
        let conversations_list = [];
    }

    connection.invoke("ListConversations");

    // keydown events
    document.addEventListener("keydown", function (event) {
        if (event.keyCode === 13 && event.shiftKey) {
            // here we should add a new line
        } else if (event.keyCode === 13) {
            send_btn.click();
        }
    });

    /*document.getElementById("conversation-list").addEventListener("click", function (event) {
        if (clickedOnConversationsList == null) {
            let clickedOnConversationsList = false;
        }
        clickedOnConversationsList = true;
    });*/

    /*search_conversation_input.addEventListener("blur", function (event) {
        if (clickedOnConversationsList == null) {
            let clickedOnConversationsList = false;
        } else if (clickedOnConversationsList) {
            clickedOnConversationsList = false;
            return;
        }
        if (hasSearchedConversation == null) {
            let hasSearchedConversation = false;
        }
        if (hasSearchedConversation) {
            renderConversationsList();
            hasSearchedConversation = false;
            if (search_conversation_input != null) {
                search_conversation_input.value = "";
            }
        }
    });*/

}).catch(function (err) {
    return console.error(err.toString());
});
