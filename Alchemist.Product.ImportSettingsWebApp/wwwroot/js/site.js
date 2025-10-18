// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function uploadShopList(shopId, shopSettingsType) {

    const hrefFormat = '/Import/Settings/{0}/' + shopSettingsType; 
    var data = { hrefFormat: hrefFormat, shopId: shopId };

    postJsonData(url = '/ShopApi/ShopList',
        data = JSON.stringify(data),
        onSuccess = (result) => {

            $("#shopListDiv").html(result.content);

            setShopSettingsItemsPreventClick();

            loadImportSettingsTab(result.shopId, shopSettingsType);
        },
         null);
}

function enableButton(saveButton) {
    $(saveButton).prop('disabled', false);
}

async function saveSettings(settingsForm, url, onSuccess, onError, onValidateionError) {

    validateForm($(settingsForm), () => {

        var formData = new FormData($(settingsForm)[0]);
        postFormData(url = url, formData = formData, onSuccess = onSuccess, onError = onError);

    }, onValidateionError);
}

function setShopSettingsItemsPreventClick() {
    $(".shop_item").on('click', onImportSettingsItemChangePrevent);
}

function getIsSettingsChangedUrl() {

    return $('#settingsIsChangedAction').val();
}

function onImportSettingsItemChangePrevent(event) {

    onItemChangePrevent(event, getIsSettingsChangedUrl(), '#settingsForm');
}