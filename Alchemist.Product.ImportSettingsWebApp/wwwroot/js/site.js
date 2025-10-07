// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function uploadShopList(shopId, shopSettingsType) {

    const hrefFormat = '/Import/Settings/{0}/' + shopSettingsType; 
    var data = { hrefFormat: hrefFormat, shopId: shopId };

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


