// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function uploadShopSettingsTab(shopId, shopSettingsType) {

    var data = { hrefFormat: '/Import/Settings/{0}/' + shopSettingsType, shopId: shopId };

    postData(url = '/ShopApi/ShopList',
        data = JSON.stringify(data),
        onSuccess = (result) => {

            $("#shopListDiv").html(result.content);

            var selectedShopId = result.shopId;
            uploadImportSettings(selectedShopId, shopSettingsType);
        },
         null,
         contentType = 'application/json; charset=utf-8'         
        );
}

function uploadImportSettings(shopId, shopSettingsType) {
    var data = { shopSettingsType: shopSettingsType, shopId: shopId };
    postData(url = '/Import/Settings',
        data = JSON.stringify(data),
        onSuccess = (result) => {
            $("#importSettingsDiv").html(result);
        },
        null,
        contentType = 'application/json; charset=utf-8'
    );
}
