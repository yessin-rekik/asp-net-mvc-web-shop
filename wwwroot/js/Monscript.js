// Cart interactions for the Panier pages (AJAX, progressive enhancement).
(function () {
    "use strict";

    function money(value) {
        var number = Number(value) || 0;
        return new Intl.NumberFormat("fr-TN", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(number) + " DT";
    }

    function updateBadge(count) {
        var badge = document.getElementById("cart-count");
        if (!badge) {
            return;
        }
        badge.textContent = count;
        badge.classList.remove("badge-pop");
        void badge.offsetWidth;
        badge.classList.add("badge-pop");
    }

    function updateTotal(total) {
        var target = document.getElementById("totalapayer");
        if (target) {
            target.textContent = money(total);
        }
    }

    function notify(message, type) {
        if (window.showAppToast) {
            window.showAppToast(message, type);
        }
    }

    function removeRow(id) {
        var row = document.getElementById("row-" + id);
        if (!row) {
            return;
        }
        row.classList.add("row-removing");
        window.setTimeout(function () {
            row.remove();
            if (document.querySelectorAll("[id^='row-']").length === 0) {
                window.location.reload();
            }
        }, 220);
    }

    $(document).on("click", ".PlusProducts", function (event) {
        event.preventDefault();
        var id = String($(this).data("id"));
        if (!id) {
            return;
        }

        $.post("/Panier/PlusProduct", { id: id }, function (data) {
            if (!data || !data.success) {
                if (data && data.message) {
                    notify(data.message, "error");
                }
                return;
            }

            $("#quantite_" + id).text(data.quantity);
            $("#rquantite_" + id).text(data.quantity);
            $("#total_" + id).text(money(data.totalRow));
            updateTotal(data.total);
            updateBadge(data.cartCount);
        }).fail(function () {
            notify("Une erreur est survenue. Veuillez réessayer.", "error");
        });
    });

    $(document).on("click", ".MinProducts", function (event) {
        event.preventDefault();
        var id = String($(this).data("id"));
        if (!id) {
            return;
        }

        $.post("/Panier/MinusProduct", { id: id }, function (data) {
            if (!data || !data.success) {
                if (data && data.message) {
                    notify(data.message, "error");
                }
                return;
            }

            if (data.removed) {
                removeRow(id);
            } else {
                $("#quantite_" + id).text(data.quantity);
                $("#rquantite_" + id).text(data.quantity);
                $("#total_" + id).text(money(data.totalRow));
            }
            updateTotal(data.total);
            updateBadge(data.cartCount);
        }).fail(function () {
            notify("Une erreur est survenue. Veuillez réessayer.", "error");
        });
    });

    $(document).on("click", ".RemoveLink", function (event) {
        event.preventDefault();
        var id = String($(this).data("id"));
        if (!id) {
            return;
        }

        $.post("/Panier/RemoveProduct", { id: id }, function (data) {
            if (!data || !data.success) {
                if (data && data.message) {
                    notify(data.message, "error");
                }
                return;
            }

            removeRow(id);
            updateTotal(data.total);
            updateBadge(data.cartCount);
        }).fail(function () {
            notify("Une erreur est survenue. Veuillez réessayer.", "error");
        });
    });
})();
