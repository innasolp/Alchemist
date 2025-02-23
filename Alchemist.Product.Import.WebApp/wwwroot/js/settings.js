function setServiceSettingsFromJson(form, fileInputName, shopId, serviceSettingsName, onSuccess = null) {
    uploadFromJson('/FileUpload/UploadServiceSettings',
        form,
        fileInputName,
        { 'shopId': shopId, 'serviceSettingsName': serviceSettingsName },
        onSuccess
    );
}

function submitPreventDefault(event) {
event.preventDefault();
}

function showServiceSettingsModel(url, data, onHide = null) {
    
    $('#settingsForm').on('submit', submitPreventDefault);

    $("#modalBodyDiv").on('load', function (event) {
        console.log(event);
        console.trace(event);
    });

    ShowItemModal($("#divModal"), $("#modalBodyDiv"), url, data, onHide)
}

function closeServiceSettingsModal() {
    $("#divModal").modal("hide");
    $('#settingsForm').off('submit', submitPreventDefault);
}

async function saveSettings(settingsFormSelector, formForSubmit) {
    var settingsFormData = new FormData(settingsFormSelector[0]);
    addFormDataJson(settingsFormData);
    var newFormData = getFormDataWithPrefix(settingsFormData, 'prevSettings');

    await fetchFormData(newFormData, '/', 'post', () => formForSubmit.submit());
}

async function setShopSettingsFromJson(formSelector, fileInputName, shopId) {

    uploadFromJson('/FileUpload/UploadShopSettings',
        formSelector,
        fileInputName,
        { 'shopId': shopId },
        (data) => {
            if (data == null) return;
            console.trace(data);
            console.log('shop settings upload successfully');

            location.reload();           
        }        
    );
}