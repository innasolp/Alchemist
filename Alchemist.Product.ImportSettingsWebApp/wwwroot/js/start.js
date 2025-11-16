$(document).on(AppStartEvent.eventName, (event) => {
    if (!event.detail.appName || event.detail.appName != 'importsettingsapp') return;

    if (!Object.hasOwn(event.detail, 'shopId') || !Object.hasOwn(event.detail, 'shopSettingsType')) return;   

    uploadShopList(event.detail.shopId, event.detail.shopSettingsType);
});

$(document).on(AppClosingEvent.eventName, onEditDataChanged);

async function onEditDataChanged(event) {
    if (!Object.hasOwn(event.detail, 'appName') || event.detail.appName != 'importsettingsapp' || !Object.hasOwn(event.detail, 'onSuccess')
        || !Object.hasOwn(event.detail, 'shopId') || !Object.hasOwn(event.detail, 'shopSettingsType')) return;
    let map = new Map();
    map.set('#settingsForm', `/ImportSettingsAction/${event.detail.shopSettingsType}/IsChanged`);
    map.set('#serviceSettingsForm', `/ImportSettingsAction/${event.detail.shopId}/${event.detail.shopSettingsType}/Service/IsChanged`);
    const changed = await onEditableDataChangedWithConfirmAsync(map);
    if (!changed) event.detail.onSuccess();
}
