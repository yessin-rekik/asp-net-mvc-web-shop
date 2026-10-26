// NovaShop client bootstrap: navigation, sidebar, toasts and AJAX security.
(function () {
    "use strict";

    var body = document.body;
    var sidebar = document.getElementById("sidebar");
    var mainContent = document.getElementById("mainContent");
    var toggleButton = document.getElementById("toggleMenu");
    var closeButton = document.getElementById("closeSidebar");
    var backdrop = document.getElementById("sidebarBackdrop");
    var desktopQuery = window.matchMedia("(min-width: 992px)");
    var storageKey = "nova.sidebar.collapsed";

    function isMobile() {
        return !desktopQuery.matches;
    }

    function setAriaExpanded() {
        if (!toggleButton) {
            return;
        }
        var expanded = isMobile()
            ? body.classList.contains("sidebar-open")
            : !body.classList.contains("sidebar-collapsed");
        toggleButton.setAttribute("aria-expanded", expanded ? "true" : "false");
    }

    function closeMobileSidebar() {
        body.classList.remove("sidebar-open");
        setAriaExpanded();
    }

    function initSidebar() {
        if (!sidebar || !toggleButton) {
            return;
        }

        if (!isMobile() && localStorage.getItem(storageKey) === "1") {
            body.classList.add("sidebar-collapsed");
        }
        setAriaExpanded();

        toggleButton.addEventListener("click", function (event) {
            event.preventDefault();

            if (isMobile()) {
                body.classList.toggle("sidebar-open");
            } else {
                var collapsed = body.classList.toggle("sidebar-collapsed");
                localStorage.setItem(storageKey, collapsed ? "1" : "0");
            }
            setAriaExpanded();
        });

        if (closeButton) {
            closeButton.addEventListener("click", closeMobileSidebar);
        }

        if (backdrop) {
            backdrop.addEventListener("click", closeMobileSidebar);
        }

        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape") {
                closeMobileSidebar();
            }
        });

        desktopQuery.addEventListener("change", function () {
            closeMobileSidebar();
            setAriaExpanded();
        });
    }

    function initToasts() {
        if (typeof bootstrap === "undefined") {
            return;
        }

        document.querySelectorAll(".app-toast").forEach(function (toastElement) {
            new bootstrap.Toast(toastElement).show();
        });
    }

    window.showAppToast = function (message, type) {
        if (typeof bootstrap === "undefined") {
            return;
        }

        var container = document.getElementById("toast-container");
        if (!container) {
            return;
        }

        var toastElement = document.createElement("div");
        toastElement.className = "toast align-items-center border-0 app-toast " +
            (type === "error" ? "toast-error" : "toast-success");
        toastElement.setAttribute("role", "alert");
        toastElement.setAttribute("aria-live", "assertive");
        toastElement.setAttribute("aria-atomic", "true");
        toastElement.innerHTML =
            '<div class="d-flex">' +
            '<div class="toast-body"></div>' +
            '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Fermer"></button>' +
            '</div>';
        toastElement.querySelector(".toast-body").textContent = message;
        container.appendChild(toastElement);

        var toast = new bootstrap.Toast(toastElement, { delay: 4200 });
        toastElement.addEventListener("hidden.bs.toast", function () {
            toastElement.remove();
        });
        toast.show();
    };

    function initAjaxSecurity() {
        if (typeof window.jQuery === "undefined") {
            return;
        }

        var token = document.querySelector('meta[name="csrf-token"]');
        window.jQuery.ajaxSetup({
            headers: token ? { RequestVerificationToken: token.getAttribute("content") } : {}
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        initSidebar();
        initToasts();
        initAjaxSecurity();
    });
})();
