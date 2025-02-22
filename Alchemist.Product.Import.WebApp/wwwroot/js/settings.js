function setServiceSettingsFromJson(form, fileInputName, shopId, serviceSettingsName, onSuccess = null) {
    uploadFromJson(form,
        fileInputName,
        '/FileUpload/UploadJson',
        { 'shopId': shopId, 'serviceSettingsName': serviceSettingsName },
        '/Home/SetServiceSettings',
        onSuccess
    );
}

function showServiceSettingsModel(url, data, onHide = null) {
    ShowItemModal($("#divModal"), $("#modalBodyDiv"), url, data, onHide)
}

function closeServiceSettingsModal() {
    $("#divModal").modal("hide");
}

async function saveSettings(settingsFormSelector, formForSubmit) {
    var settingsFormData = new FormData(settingsFormSelector[0]);
    addFormDataJson(settingsFormData);
    var newFormData = getFormDataWithPrefix(settingsFormData, 'prevSettings');

    await fetchFormData(newFormData, '/', 'post', () => formForSubmit.submit());
}

function setShopSettingsFromJson(formSelector, fileInputName, shopId) {
    uploadFromJson(formSelector,
        fileInputName,
        '/FileUpload/UploadJson',
        { 'shopId': shopId },
        '/Home/SetShopSettings',       
        (data) => {
            if (data == null) return;
            formSelector[0].reset();
            //fetchForm(formSelector, '/Home/Index/shopId=' + shopId + '&tab=Settings', 'POST', (data) => { location.reload(); });
            formSelector.submit();
        }        
    );
}