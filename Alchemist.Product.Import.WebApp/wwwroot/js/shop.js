const shopModalSettings = new ModalForm(".shopModal", ".shopModalBody", '#shopCloseBtn', null, null, null, 'shopModalResult' );

function showShopModal(url, data, onHide = null) {   

    shopModalSettings.show(url, data, onHide);
}

function saveShop(newShopGuidSelector) {
    save($('#shopForm'),
        '/Shop/Save',
        { 'shop': JSON.stringify(getFormData($('#shopForm'))) },
        null,
        (data) => {
            if ($('#Id').val() == 0) {
                newShopGuidSelector.val(data.guid);
            }
            else {
                $('#shop_li_' + data.guid.toString()).find("a").text(data.name);
            }
            shopModalSettings.closeModal(true);
        },
        null);
}

async function updateShops(divPartialShops, tabData = null, settingsPartialDiv = null) {
    postData(
        '/Home/UpdateShops',
        null,
        (result) => {
            divPartialShops.load('/Home/ShopList', { 'shops': JSON.stringify(result) },
                function (response, status, xhr) {
                    if (status == "error") {
                        console.error("/Home/ShopList error: " + xhr.status + ": " + xhr.statusText);
                        console.trace(response);
                        return;
                    }

                    if (tabData == null) return;

                    if (tabData.shopGuid == '' && result.length > 0)
                        tabData.shopGuid = result[0].guid.toString();

                    if (tabData.shopGuid != null) {

                        var shopLi = $('#shop_li_' + tabData.shopGuid);
                        shopLi.addClass('selected');

                        var editButton = $("<i class='fa fa-edit' style='font-size:24px'></i>");
                        var editData = { "shopGuid": tabData.shopGuid.toString() };
                        editButton.on("click", function () { showShopModal('/Shop/Edit', editData); });

                        shopLi.children('div').children('form').append(editButton);
                    }

                    $('.menuDiv').load('/Home/TabsMenu', tabData,
                        function (response, status, xhr) {
                            settingsPartialDiv.load('/Home/LoadTab', tabData);
                        });
                }
            );
        }    );
}