// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(
    function () {
        $(document).on(AppStartEvent.eventName, (event) => {
            if (!event.detail.appName || event.detail.appName != 'importsettingsapp') return;

            if (!event.detail.shopId || !event.detail.shopSettingsType) return;

            uploadShopList(event.detail.shopId, event.detail.shopSettingsType);
        });
    });