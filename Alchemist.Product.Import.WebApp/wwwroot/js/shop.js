function showShopModal(url, data, onHide = null) {   

    $("#modalBodyDivShop").on('load', function (event) {
        console.log(event);
        console.trace(event);
    });    

    showItemModal($("#divModalShop"), $("#modalBodyDivShop"), url, data, () => { if (onHide != null) onSaveShop(onHide); })
}

function onSaveShop(onHide = null) {
    if ($('#shopModalResult').val() == 'success' || $('#shopModalResult').val() == 1 || $('#shopModalResult').val() == true) {
        $('#shopModalResult').remove();
        onHide(true);
    }
    else
        onHide(false);
}

function closeShopModal() {
    $("#divModalShop").append("<input type='hidden' id='shopModalResult' value='success'/>");
    $("#divModalShop").modal("hide");    
}

function saveShop(newShopGuidSelector) {
    save('#shopForm',
        '/Shop/Save',
        getFormData($('#shopForm')), (data) =>    {
        if ($('#Id').val() == 0) {
            newShopGuidSelector.val(data);
            };
            closeShopModal();
    })
}

async function updateShops(divPartialShops, tabData = null, settingsPartialDiv = null) {
    postData('/Home/UpdateShops', null,
        (result) => {
            divPartialShops.load('/Home/ShopList', { 'shops': result },
                function (response, status, xhr)
                {
                    if (tabData == null) return;                     

                    if (tabData.shopGuid == '' && result.length > 0)
                        tabData.shopGuid = result[0].guid.toString();

                    if (tabData.shopGuid != null) {
                        $('#shop_li_' + tabData.shopGuid).addClass('selected');                        

                        var editData = `{ &#39;shopGuid&#39;: &#39;${tabData.shopGuid.toString()}&#39; }`;
                        var editHtml = `<i class='fa fa-edit' style='font-size:24px'  onclick='showShopModal(&#39;\/Shop\/Edit&#39;, ${editData} )' ><\/i>`;
                        
                        $('#shop_li_' + tabData.shopGuid).children('div').children('form').children('div').append(editHtml);
                    }                  

                    settingsPartialDiv.load('/Home/LoadTab', tabData);
                }
            );
        });
}