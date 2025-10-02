function uploadShopTab(shopTabDiv, shopId = null, onSuccess = null) {
    shopTabDiv.load('/Shop/ShopTab', { shopId: shopId }, (r, status, xhr) => {
        if (status == "error") {
            console.error("/Shop/ShopTab error: " + xhr.status + ": " + xhr.statusText);
            console.trace(r);
            return;
        }
        else 
            onSuccess();        
    });
}

function uploadShopList(shopListDiv, shopId = null, onSuccess = null) {
    shopListDiv.load('/Shop/ShopList', { shopId: shopId }, (r, status, xhr) => {
        if (status == "error") {
            console.error("/Shop/ShopList error: " + xhr.status + ": " + xhr.statusText);
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

function saveShop(shopEditForm, shopListDiv) {

    var updateShopListOnSuccess = function (shop) {
        uploadShopList($(`#${shopListDiv}`), shop.id, null);
    };

    validateForm($('#' + shopEditForm), () => {
        
        var formData = new FormData($('#' + shopEditForm)[0]);
        postFormData(url = "/Shop/Save", formData = formData, onSuccess = updateShopListOnSuccess, onError = null);

    }, null); 
}