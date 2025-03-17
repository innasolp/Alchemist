function showShopModal(url, data, onHide = null) {   

    $("#modalBodyDivShop").on('load', function (event) {
        console.log(event);
        console.trace(event);
    });    

    showItemModal($("#divModalShop"), $("#modalBodyDivShop"), url, data, () => { onSaveShop(onHide); })
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
        }; closeShopModal();
    })
}