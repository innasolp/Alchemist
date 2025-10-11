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

            loadImportSettings(result.shopId, shopSettingsType);
        },
         null);
}

function enableSaveButton(saveButton) {
    $(saveButton).prop('disabled', false);
}

async function saveSettings(settingsForm, url, onSuccess, onError, onValidateionError) {

    validateForm($(settingsForm), () => {

        var formData = new FormData($(settingsForm)[0]);
        postFormData(url = url, formData = formData, onSuccess = onSuccess, onError = onError);

    }, onValidateionError);
}


