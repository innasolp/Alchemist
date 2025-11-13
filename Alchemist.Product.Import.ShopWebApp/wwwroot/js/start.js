
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