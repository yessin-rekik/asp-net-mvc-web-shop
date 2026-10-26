(function () {
    "use strict";

    var root = document.getElementById("chatbot");
    if (!root) {
        return;
    }

    var toggle = document.getElementById("chatbotToggle");
    var panel = document.getElementById("chatbotPanel");
    var closeButton = document.getElementById("chatbotClose");
    var messages = document.getElementById("chatbotMessages");
    var form = document.getElementById("chatbotForm");
    var input = document.getElementById("chatbotInput");
    var sendButton = document.getElementById("chatbotSend");
    var pending = false;

    function openChat() {
        root.classList.add("is-open");
        panel.setAttribute("aria-hidden", "false");
        toggle.setAttribute("aria-expanded", "true");
        window.setTimeout(function () {
            input.focus();
        }, 180);
    }

    function closeChat() {
        root.classList.remove("is-open");
        panel.setAttribute("aria-hidden", "true");
        toggle.setAttribute("aria-expanded", "false");
    }

    function scrollToBottom() {
        messages.scrollTop = messages.scrollHeight;
    }

    function removeSuggestions() {
        messages.querySelectorAll(".chat-suggestions").forEach(function (element) {
            element.remove();
        });
    }

    function appendMessage(text, sender) {
        var message = document.createElement("div");
        message.className = "chat-message chat-" + sender;
        var bubble = document.createElement("div");
        bubble.className = "chat-bubble";
        bubble.textContent = text;
        message.appendChild(bubble);
        messages.appendChild(message);
        scrollToBottom();
    }

    function showTyping() {
        var typing = document.createElement("div");
        typing.className = "chat-message chat-bot chat-typing";
        typing.id = "chatTyping";
        typing.innerHTML = '<div class="chat-bubble"><span></span><span></span><span></span></div>';
        messages.appendChild(typing);
        scrollToBottom();
    }

    function removeTyping() {
        var typing = document.getElementById("chatTyping");
        if (typing) {
            typing.remove();
        }
    }

    function appendProducts(products) {
        if (!products || !products.length) {
            return;
        }

        var list = document.createElement("div");
        list.className = "chat-products";

        products.forEach(function (product) {
            var link = document.createElement("a");
            link.className = "chat-product";
            link.href = "/Product/Details/" + product.productId;

            var image = document.createElement("img");
            image.alt = "";
            image.loading = "lazy";
            image.src = product.image ? "/images/" + encodeURIComponent(product.image) : "/images/placeholder.svg";
            image.addEventListener("error", function () {
                image.src = "/images/placeholder.svg";
            });

            var info = document.createElement("span");
            info.className = "chat-product-info";

            var name = document.createElement("strong");
            name.textContent = product.name;

            var meta = document.createElement("span");
            meta.textContent = (product.category || "Sans catégorie") + " · " +
                Number(product.price).toFixed(2) + " DT";

            info.appendChild(name);
            info.appendChild(meta);
            link.appendChild(image);
            link.appendChild(info);
            list.appendChild(link);
        });

        messages.appendChild(list);
        scrollToBottom();
    }

    function appendLinks(links) {
        if (!links || !links.length) {
            return;
        }

        var list = document.createElement("div");
        list.className = "chat-links";

        links.forEach(function (link) {
            var anchor = document.createElement("a");
            anchor.className = "chat-link";
            anchor.href = link.url;
            anchor.textContent = link.label;
            list.appendChild(anchor);
        });

        messages.appendChild(list);
        scrollToBottom();
    }

    function appendSuggestions(suggestions) {
        if (!suggestions || !suggestions.length) {
            return;
        }

        var list = document.createElement("div");
        list.className = "chat-suggestions";

        suggestions.forEach(function (suggestion) {
            var button = document.createElement("button");
            button.type = "button";
            button.setAttribute("data-suggestion", suggestion);
            button.textContent = suggestion;
            list.appendChild(button);
        });

        messages.appendChild(list);
        scrollToBottom();
    }

    function setPending(value) {
        pending = value;
        input.disabled = value;
        sendButton.disabled = value;
        root.classList.toggle("is-loading", value);
    }

    function sendMessage(rawText) {
        var text = (rawText || "").trim();
        if (!text || pending) {
            return;
        }

        input.value = "";
        removeSuggestions();
        appendMessage(text, "user");
        setPending(true);
        showTyping();

        var headers = { "Content-Type": "application/json" };
        var token = document.querySelector('meta[name="csrf-token"]');
        if (token) {
            headers.RequestVerificationToken = token.getAttribute("content");
        }

        fetch("/Assistant/Ask", {
            method: "POST",
            headers: headers,
            credentials: "same-origin",
            body: JSON.stringify({ message: text })
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("HTTP " + response.status);
                }
                return response.json();
            })
            .then(function (data) {
                removeTyping();
                appendMessage(data.reply || "Je n'ai pas de réponse pour le moment.", "bot");
                appendProducts(data.products);
                appendLinks(data.links);
                appendSuggestions(data.suggestions);
            })
            .catch(function () {
                removeTyping();
                appendMessage("Je n'arrive pas à joindre l'assistant pour le moment. Réessayez dans quelques instants.", "bot");
            })
            .finally(function () {
                setPending(false);
                input.focus();
            });
    }

    toggle.addEventListener("click", function () {
        if (root.classList.contains("is-open")) {
            closeChat();
        } else {
            openChat();
        }
    });

    closeButton.addEventListener("click", closeChat);

    form.addEventListener("submit", function (event) {
        event.preventDefault();
        sendMessage(input.value);
    });

    messages.addEventListener("click", function (event) {
        var trigger = event.target.closest("[data-suggestion]");
        if (trigger && !pending) {
            sendMessage(trigger.getAttribute("data-suggestion"));
        }
    });

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && root.classList.contains("is-open")) {
            closeChat();
        }
    });
})();
