function uploadShops(shopTabDiv, selectedShopId = null, onSuccess = null) {
    shopTabDiv.load('/Shop/ShopTab', selectedShopId, (r, status, xhr) => {
        if (status == "error") {
            console.error("/Shop/ShopTab error: " + xhr.status + ": " + xhr.statusText);
            console.trace(r);
            return;
        }
        else 
            onSuccess();        
    });
}

function setItemsPreventClick() {
    $(".shop_item").on('click', onShopItemChangePrevent);
}

function onShopItemChangePrevent(event) {

    event.preventDefault();

    var formData = new FormData($('#shopEditForm')[0]);

    var onSuccess = function (changed) {
        if (!changed) {
            shopItemAction(event.target);
        }
        else {
            confirm('Confirmation', 'Input data will be reset. Continue?',
                () => {
                    shopItemAction(event.target);
                });
        }
    };

    postFormData(url = "/Shop/IsChanged", formData = formData, onSuccess = onSuccess, onError = null);     
}

function shopItemAction(anchor) {
    window.location.href = anchor.attributes["href"].value;
}

function saveShop(shopEditForm) {
    validateForm($('#' + shopEditForm), () => {
        $('#' + shopEditForm).trigger("submit");
    }, null); 
}