function uploadShopList(shopId = null, onSuccess = null) {
    $("#shopList").load('/ShopAction/ShopList', { shopId: shopId }, (r, status, xhr) => {
        if (status == "error") {
            console.error("/ShopAction/ShopList error: " + xhr.status + ": " + xhr.statusText);
            console.trace(r);
            return;
        }
        else {
            if (shopId == null) {
                const shopItems = $("a.shop_item");
                if (shopItems.length == 0) return;
                window.location.href = shopItems.first().attr("href");
            }
            else {
                setShopItemsPreventClick();
                if (onSuccess != null && shopId != 0) onSuccess(shopId);
            }
        }
    });
}

function loadShop(shopId, onSuccess = null) {
    const shopUrl = `/ShopAction/${shopId}`;
    postData(shopUrl, null, (html) => {
        $("#shopDiv").html(html);
        if (onSuccess != null) onSuccess();
    });
}

function setShopItemsPreventClick() {
    $(".shop_item").on('click', onShopItemChangePrevent);
}

function onShopItemChangePrevent(event) {

    onItemChangePrevent(event, "/ShopAction/IsChanged", '#shopEditForm');
}

function saveShop(shopEditForm) {

    var updateShopListOnSuccess = function (shop) {
        uploadShopList(shop.id, null);
    };

    validateForm($('#' + shopEditForm), () => {
        
        var formData = new FormData($('#' + shopEditForm)[0]);
        postFormData(url = "/ShopAction/Save", formData = formData, onSuccess = updateShopListOnSuccess, onError = null);

    }, null); 
}

$(document).on(AppStartEvent.eventName, (event) => {
    if (!event.detail.appName || event.detail.appName != 'shopapp') return;

    if (!Object.hasOwn(event.detail, 'shopId')) return;

    const setEditForms = function () {
        $("#shopEditForm").addClass("edit-form");
    }

    uploadShopList(event.detail.shopId, (shopId) => {
        loadShop(shopId, setEditForms);
    });
});

$(document).on(AppClosingEvent.eventName, onShopDataChangedBeforeClosing);

async function onShopDataChangedBeforeClosing(event) {
    if (!Object.hasOwn(event.detail, 'appName') || event.detail.appName != 'shopapp' || !Object.hasOwn(event.detail, 'onSuccess')) return;
    let map = new Map();
    map.set('.edit-form', "/ShopAction/IsChanged");
    const changed = await onEditableDataChangedWithConfirmAsync(map);
    if (!changed) event.detail.onSuccess();    
}