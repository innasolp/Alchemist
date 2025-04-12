function setServiceSettingsFromJson(form, fileInputName, shopGuid, shopSettingsGuid, serviceSettingsName, onSuccess = null) {
    uploadFromJson('/FileUpload/UploadServiceSettings',
        form,
        fileInputName,
        { 'shopGuid': shopGuid, 'serviceSettingsName': serviceSettingsName, 'shopSettingsGuid': shopSettingsGuid },
        onSuccess
    );
}

function showServiceSettingsModal(url, data, onHide = null) {
    
    $('#settingsForm').on('submit', submitPreventDefault);

    $("#modalBodyDiv").on('load', function (event) {
        console.log(event);
        console.trace(event);
    });

    showItemModal($("#divModal"), $("#modalBodyDiv"), url, data, () => { onSaveServiceSettings(onHide); })
}

function onSaveServiceSettings(onHide = null) {
    if ($('#modalResult').val() == 'success' || $('#modalResult').val() == 1 || $('#modalResult').val() == true) {
        $('#modalResult').remove();
        onHide(true);
    }
    else
        onHide(false);
}

function closeServiceSettingsModal() {
    $("#divModal").append("<input type='hidden' id='modalResult' value='success'/>");
    $("#divModal").modal("hide");
    $('#settingsForm').off('submit', submitPreventDefault);
}

async function saveTab(formSelector, callback = null) {
    var formData = new FormData(formSelector[0]);

    var json = formDataToJson(formData);
    formData.append('json', json);

    await fetchFormData(formData, '/Home/SaveTabSettings', 'post', callback);    
}

async function saveShopSettings(formSelector, callback = null) {
    var formData = new FormData(formSelector[0]);

    var json = formDataToJson(formData);
    formData.append('json', json);

    await fetchFormData(formData, '/ShopSettings/SaveShopSettings', 'post', callback);
}

async function setShopSettingsFromJson(formSelector, fileInputName, shopGuid, shopSettingsType) {

    uploadFromJson('/FileUpload/UploadShopSettings',
        formSelector,
        fileInputName,
        { 'shopGuid': shopGuid, 'shopSettingsType': shopSettingsType },
        (data) => {
            if (data == null) return;
            console.trace(data);
            console.log('shop settings upload successfully');

            location.reload();           
        }        
    );
}
