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

function getMonthName(id) {
    let  monthNames = ["January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];
    return monthNames[id];
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

function formatConversationLastMessageData(data) {
    let date = new Date(Date.parse(data));

    let result = "";

    //if (Date().getDay() - date.getDay() > 7) {
        result += date.getDate() + " ";
    //}

    //if (date.getMonth() != new Date().getMonth()) {
        result += getMonthName(date.getMonth()) + " ";
    //}

    //if (date.getFullYear() != new Date().getFullYear()) {
        result += date.getFullYear() + " ";
    //}

    result += date.getHours();

    let minutes = date.getMinutes();

    if (minutes < 10) minutes = "0" + minutes;
    minutes = ":" + minutes;
    result += minutes + " ";

    return result;
}

function getPacketNumberFromString(string) {
    if (string.includes("#")) {
        let list = string.split("#");
        if (list.length == 2) return list[1];
    }
    return "";
}

function getPacketNumberFromClass(class_list) {
    if (class_list != null) {
        let res = "";
        for (let i = 0; i < class_list.length; i++) {
            res = getPacketNumberFromString(class_list[i]);
            if (res !== "") return res;
        }
    }
}

function getPacketNumberFromObject(object) {
    if (object != null) {
        return getPacketNumberFromClass(object.classList);
    }
    return "";
}

// overlay layer

function hideOverlayLayer() {
    let layer = document.getElementById("overlay-layer");
    if (layer != null) {
        layer.parentNode.removeChild(layer);
        return true;
    }
    return false;
}

function showOverlayLayer() {
    let layer = document.getElementById("overlay-layer");
    if (layer == null) {
        layer = document.createElement("div");
        layer.id = "overlay-layer";
        document.body.appendChild(layer);
        return true;
    }
    return false;
}

// settings menu

function requestConversationDataForSettings(conversationId) {
    connection.invoke("RefreshSettingsData", conversationId);
}

function showConversationSettingsMenu(data) {
    if (showOverlayLayer()) {
        let settings_container = document.getElementById("conversation-settings-container");
        if (settings_container == null) {
            settings_container = document.createElement("div");
            settings_container.id = "conversation-settings-container";
            settings_container.classList.add("settings-container");

            // title

            let settings_container_title = document.createElement("h1");
            settings_container_title.innerText = "Conversation Settings";

            settings_container.appendChild(settings_container_title);

            // button list

            let button_list = document.createElement("div");
            button_list.classList.add("btn-list");

            // buttons
            if (data.isGroup) {
                // groups

                // change conversation's name button 

                let change_conversation_name_button = document.createElement("div");
                change_conversation_name_button.innerText = "Change Conversation Name";

                button_list.appendChild(change_conversation_name_button);

                // list members button 

                let list_members_button = document.createElement("div");
                list_members_button.innerText = "See Members";

                button_list.appendChild(list_members_button);

                // add member button 

                let add_member_button = document.createElement("div");
                add_member_button.innerText = "Add Member";

                button_list.appendChild(add_member_button);

                // remove member button 

                let remove_member_button = document.createElement("div");
                remove_member_button.innerText = "Remove Member";

                button_list.appendChild(remove_member_button);

            } else {
                // contact conversation
                if (data.members.length != 1) {
                    let block_contact = document.createElement("div");
                    block_contact.id = "block-contact-button";

                    if (data.status == 0) {

                        block_contact.innerText = "Block Contact";

                        block_contact.addEventListener("click", function (e) {
                            connection.invoke("BlockContact", data.id);
                        });
                    } else {
                        block_contact.innerText = "Unblock Contact";

                        block_contact.addEventListener("click", function (e) {
                            connection.invoke("UnblockContact", data.id);
                        });
                    }
                    

                    button_list.appendChild(block_contact);
                }
            }

            

            // back button
            let back_button = document.createElement("div");
            back_button.innerText = "Back";
            back_button.addEventListener("click", function (e) {
                hideConversationSettingsMenu();
            });

            button_list.appendChild(back_button);

            settings_container.appendChild(button_list);

            let layer = document.getElementById("overlay-layer");
            layer.appendChild(settings_container);
        }
    } // else something went wrong
}

function hideConversationSettingsMenu() {
    if (hideOverlayLayer()) {

    } // else something went wrong
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

function formatConversationLastMessage(string, sender="") {
    if (string == null) {
        return;
    }
    return (sender !== ""?sender+": ":"")+formatString(string, 20) + (string.length > 20 ? "..." : "");
}

// warning block

function createConversationWarningBlock(message) {
    let block = document.createElement("div");
    block.id = "conversation-list-back-button";
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

// if we aren't searching something then the following function will return true
function isConversationListFree() {
    let back_button = document.getElementById("conversation-list-back-button");

    return back_button == null;
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
function renderMessages(messages, packetNumber) {
    for (var i = 0; i < messages.length; i++) {
        renderMessage(messages[i], packetNumber);
    }
}

function renderMessage(message, packetNumber) {
    if (message == null) return;
    
    if (message.sender === "") { // then it's a sent message
        renderSentMessage(message, packetNumber);
    } else {
        renderReceivedMessage(message, packetNumber);
    }
}

function renderReceivedMessage(messageData, packetNumber) {
    // finding the message place
    var messages = document.getElementById("chat-message-list");
    if (messages == null) {
        alert("null");
        return;
    }

    if (messageData.content == null) return;

    var msg = messageData.content.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    // we are not using the user variable for now

    // creating the message html object
    var message = document.createElement("div");
    message.classList.add("message-row");
    message.classList.add("other-message");

    // packet number
    message.classList.add("packet_number#" + packetNumber);

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
    message_content_date.textContent = formatConversationLastMessageData(messageData.sentData);

    message_content.appendChild(message_content_date);

    message.appendChild(message_content);

    messages.appendChild(message);

    // scrolling down to the last message
    messages.scrollTop = message.offsetTop;
}

function renderSentMessage(messageData, packetNumber) {
    // finding the message place
    var messages = document.getElementById("chat-message-list");
    if (messages == null) {
        alert("null");
        return;
    }

    if (messageData.content == null) return;

    var msg = messageData.content.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

    // creating the message html object
    var message = document.createElement("div");
    message.classList.add("message-row");
    message.classList.add("you-message");

    // packet number
    message.classList.add("packet_number#" + packetNumber);

    var message_content = document.createElement("div");
    message_content.classList.add("message-content");
    var message_content_text = document.createElement("div");
    message_content_text.classList.add("message-text");

    message_content_text.textContent = msg;

    message_content.appendChild(message_content_text);

    var message_content_date = document.createElement("div");
    message_content_date.classList.add("message-time");
    // we will leave the date hard coded for now
    message_content_date.textContent = formatConversationLastMessageData(messageData.sentData);

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
    conversation_data.innerHTML = formatConversationLastMessageData(conversationData.lastMessage.sentData);
    var conversation_message = document.createElement("div"); // an excerpt from the last message
    conversation_message.classList.add("conversation-message");
    conversation_message.innerHTML = formatConversationLastMessage(conversationData.lastMessage.content, conversationData.lastMessage.sender);
    
    conversation_block.appendChild(conversation_image);
    conversation_block.appendChild(conversation_title);
    conversation_block.appendChild(conversation_message);
    conversation_block.appendChild(conversation_data);

    if (conversations_list_container.firstChild == null) {
        conversations_list_container.appendChild(conversation_block);
    } else {
        conversations_list_container.insertBefore(conversation_block, conversations_list_container.firstChild);
    }
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

function addConversationInList(conversationData, render = true) {
    
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
    
    if (render && isConversationListFree()) {
        renderConversationsList();
    }
    
}

// conversation search results

function showConversationSearchResults(no_clear=false, no_back_button) {
    let conversations_list_container = document.getElementById("conversation-list");
    if (conversations_list_container == null) {
        alert("conversations list container not found");
        return;
    }

    if (conversations_search_result == null) {
        let conversations_search_result = []; // then we create another list
        return;
    }

    if (no_clear == null || no_clear == false) conversations_list_container.innerHTML = "";

    if (!no_back_button) {
        let backButton = createConversationWarningBlock("");

        conversations_list_container.appendChild(backButton);
    }

    if (conversations_search_result.length == 0) return;

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
    if (result.lastMessage != null) {
        conversation_data.innerHTML = formatConversationLastMessageData(result.lastMessage.sentData);
    } else {
        conversation_data.innerHTML = "";
    }
    conversation_data.classList.add("created-date");
    var conversation_message = document.createElement("div"); // an excerpt from the last message
    conversation_message.classList.add("conversation-message");
    if (result.lastMessage != null) {
        conversation_message.innerHTML = formatConversationLastMessage(result.lastMessage.content, result.lastMessage.sender);
    } else {
        conversation_message.innerHTML = "";
    }
    
    conversation_block.appendChild(conversation_image);
    conversation_block.appendChild(conversation_title);
    conversation_block.appendChild(conversation_message);
    conversation_block.appendChild(conversation_data);

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
    let backButton = createConversationWarningBlock("");

    conversations_list_container.appendChild(backButton);

    if (contacts_search_result.length == 0) return;
    
    let contactsListHeader = createNewConversationsHeader("Contacts");

    conversations_list_container.appendChild(contactsListHeader);

    for (let i = 0; i < contacts_search_result.length; i++) {
        showContactSearchResult(contacts_search_result[i]);
    }
}

function showContactSearchResult(contact) {
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

    let chat_title_container = document.getElementById("chat-title-container");

    if (chat_title_container == null) {
        alert("chat title container not found");
        return;
    }

    // set conversation title
    var conversation_title = document.getElementById("chat-title");
    if (conversation_title == null) {
        alert("no conversation title found");
        return;
    }
    conversation_title.innerText = formatConversationRoomTitle(data.conversationName);

    // conversation settings part

    let conversation_settings_button = document.getElementById("conversation-settings-button");
    if (conversation_settings_button == null) {
        conversation_settings_button = document.createElement("i");
        conversation_settings_button.classList.add("fas");
        conversation_settings_button.classList.add("fa-cog");
        conversation_settings_button.id = "conversation-settings-button";
        chat_title_container.appendChild(conversation_settings_button);
    } else {
        let c_s_b_clone = conversation_settings_button.cloneNode(true);
        conversation_settings_button.parentNode.replaceChild(c_s_b_clone, conversation_settings_button);
        conversation_settings_button = c_s_b_clone;
    }

    if (data.isGroup != null) {
        conversation_settings_button.addEventListener("click", function (e) {
            requestConversationDataForSettings(data.id);
        });
    }

    // send message part

    let chat_form = document.getElementById("chat-form");

    if (chat_form == null) {
        alert("chat form not found");
        return;
    }

    // checking if the conversation is blocked

    if (data.status != 0) {
        //alert("conversation blocked");

        chat_form.innerHTML = "";

        let block_status_warning = document.createElement("div");
        block_status_warning.id = "block-status-warning";
        // if conversation is blocked by us
        if (data.status == 2) {
            block_status_warning.innerText = "This conversation has been blocked by you.";

            let unblock_button = document.createElement("div");
            unblock_button.id = "block-status-warning-unblock-btn";
            unblock_button.innerText = "Unblock conversation";

            unblock_button.addEventListener("click", function (e) {
                connection.invoke("UnblockContact", data.id);
            });

            block_status_warning.appendChild(unblock_button);
        // or it's blocked by others
        } else if (data.status == 1) {
            block_status_warning.innerText = "This conversation has been blocked.";
        }

        chat_form.appendChild(block_status_warning);
    } else {

        let warning_element = document.getElementById("block-status-warning");

        if (warning_element != null) {
            warning_element.parentNode.removeChild(warning_element);
        }

        // input

        let input = document.getElementById("message_input");
        if (input == null) {
            // then we generate it
            input = document.createElement("input");
            input.id = "message_input";
            input.classList.add("col-md-10");
            input.type = "text";
            input.placeholder = "type a message";
            chat_form.appendChild(input);
        }

        // button container

        let send_btn_container = document.getElementById("send-btn-container");
        if (send_btn_container == null) {
            send_btn_container = document.createElement("div");
            send_btn_container.classList.add("container_send-btn");
            send_btn_container.id = "send-btn-container";
            chat_form.appendChild(send_btn_container);
        }

        // button

        let send_btn = document.getElementById("send_btn");
        if (send_btn == null) {
            send_btn = document.createElement("button");
            send_btn.id = "send_btn";
            send_btn.classList.add("btn-send");
            send_btn.type = "button";
            send_btn.innerText = "Send";
            send_btn_container.appendChild(send_btn);
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
    }

    // clear conversation message history
    var chat_message_list = document.getElementById("chat-message-list");
    if (chat_message_list == null) {
        alert("chat message list not found");
        return;
    }

    chat_message_list.innerHTML = "";

    chat_message_list.addEventListener("scroll", function (e) {
        if (chat_message_list.scrollTop == 0 && chat_message_list.offsetHeight <= chat_message_list.scrollHeight) {
            // we reached the top of the conversation's messages list
            //requestNextPacket(data.);
            alert(getPacketNumberFromObject(chat_message_list.children[0]));
        }
    });

    if (data.packets != null) {
        for (let i = data.packets.length-1; i >= 0; i--) {
            renderMessages(data.packets[i].messages, data.packets[i].packetNumber);
        }
    }

    // keydown events
    document.addEventListener("keydown", function (event) {
        if (event.keyCode === 13 && event.shiftKey) {
            // here we should add a new line
        } else if (event.keyCode === 13) {
            send_btn.click();
        }
    });
}

// keep in mind that the connection url is hard coded for now
var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:49499/messagehub", {
    accessTokenFactory: () => getCookie("access_token")
}).build();


connection.on("ReceiveMessage", function (conversation_Id, message, packetNumber) {
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

    renderReceivedMessage(message, packetNumber);
});

connection.on("SendMessage", function (conversation_Id, message, packetNumber) {
    // checking the conversation's id
    if (conversationId == null) {
        let conversationId = "";
        return;
    }

    if (conversationId !== conversation_Id) {
        return; // then we are not in the current conversation room, might happen if we are connected on multiple accounts
    }

    renderSentMessage(message, packetNumber);
});

connection.on("AddConversationInList", function (conversation_data) {
    addConversationInList(conversation_data, true);
});

connection.on("EnterConversation", function (conversation_data, enlist=false) {
    enterConversation(conversation_data);
    if (conversation_data.lastMessage != null) {
        addConversationInList(conversation_data, enlist);
    } // otherwise the conversation is empty
    
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

    addContactsToSearchResults(found_contacts_list);
    
    showContactsSearchResults();
    
    showConversationSearchResults(true, true);
});

connection.on("BlockConversation", function (conversationId) {
    let block_contact_button = document.getElementById("block-contact-button");
    if (block_contact_button != null) {
        block_contact_button.innerText = "Unblock Contact";
        let b_c_b_clone = block_contact_button.cloneNode(true);
        block_contact_button.parentNode.replaceChild(b_c_b_clone, block_contact_button);
        block_contact_button = b_c_b_clone;
        block_contact_button.addEventListener("click", function (e) {
            connection.invoke("UnblockContact", conversationId);
        });
    }

    let chat_form = document.getElementById("chat-form");

    if (chat_form != null) {
        let warning = document.getElementById("block-status-warning");
        if (warning == null) {
            chat_form.innerHTML = "";

            let block_status_warning = document.createElement("div");
            block_status_warning.id = "block-status-warning";
            block_status_warning.innerText = "This conversation has been blocked by you.";

            let unblock_button = document.createElement("div");
            unblock_button.id = "block-status-warning-unblock-btn";
            unblock_button.innerText = "Unblock conversation";

            unblock_button.addEventListener("click", function (e) {
                connection.invoke("UnblockContact", conversationId);
            });

            block_status_warning.appendChild(unblock_button);
            chat_form.appendChild(block_status_warning);
        }
    }
    //alert("You have blocked the conversation with id " + conversationId);
});

connection.on("NoticeConversationBlocked", function (conversationId) {
    //alert("The conversation with id " + conversationId + " has been blocked");
});

connection.on("UnblockConversation", function (conversationId) {
    let block_contact_button = document.getElementById("block-contact-button");
    if (block_contact_button != null) {
        block_contact_button.innerText = "Block Contact";
        let b_c_b_clone = block_contact_button.cloneNode(true);
        block_contact_button.parentNode.replaceChild(b_c_b_clone, block_contact_button);
        block_contact_button = b_c_b_clone;
        block_contact_button.addEventListener("click", function (e) {
            connection.invoke("BlockContact", conversationId);
        });
    }

    let chat_form = document.getElementById("chat-form");

    if (chat_form != null) {
        let warning = document.getElementById("block-status-warning");
        if (warning != null) {
            warning.parentNode.removeChild(warning);

            let warning_element = document.getElementById("block-status-warning");

            if (warning_element != null) {
                warning_element.parentNode.removeChild(warning_element);
            }

            // input

            let input = document.getElementById("message_input");
            if (input == null) {
                // then we generate it
                input = document.createElement("input");
                input.id = "message_input";
                input.classList.add("col-md-10");
                input.type = "text";
                input.placeholder = "type a message";
                chat_form.appendChild(input);
            }

            // button container

            let send_btn_container = document.getElementById("send-btn-container");
            if (send_btn_container == null) {
                send_btn_container = document.createElement("div");
                send_btn_container.classList.add("container_send-btn");
                send_btn_container.id = "send-btn-container";
                chat_form.appendChild(send_btn_container);
            }

            // button

            let send_btn = document.getElementById("send_btn");
            if (send_btn == null) {
                send_btn = document.createElement("button");
                send_btn.id = "send_btn";
                send_btn.classList.add("btn-send");
                send_btn.type = "button";
                send_btn.innerText = "Send";
                send_btn_container.appendChild(send_btn);
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
        }
    }

    //alert("You have unblocked the conversation with id " + conversationId);
});

connection.on("NoticeConversationUnblocked", function (conversationId) {
    //alert("The conversation with id " + conversationId + " has been unblocked");
});

connection.on("OpenConversationSettings", function (data) {
    showConversationSettingsMenu(data);
});

connection.on("alert", function (message) {
    alert(message);
});

connection.start().then(function () {
    // there goes the init
    var chat_form = document.getElementById("chat-form");
    if (chat_form != null) {
        chat_form.innerHTML = "";
    }

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

    let conversation_settings_button = document.getElementById("conversation-settings-button");
    if (conversation_settings_button != null);
    conversation_settings_button.parentNode.removeChild(conversation_settings_button);

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

}).catch(function (err) {
    return console.error(err.toString());
});
