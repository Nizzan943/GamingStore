

function disableCartButtonCheckOut() {
    if (!flag) {
        document.getElementById('cart_button_checkout').addClass("disabled", "true");
    } else {
        document.getElementById('cart_button_checkout').removeAttribute("disabled");
        document.getElementById('cart_button_checkout').focus();
    }
}

$("cart_button_checkout").toggleClass("disabled", true);

