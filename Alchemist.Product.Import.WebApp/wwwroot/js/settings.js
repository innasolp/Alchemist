function setServiceSettingsFromJson(form, fileInputName, shopGuid, shopSettingsType, serviceSettingsName, onSuccess = null) {
    uploadFromJson('/FileUpload/UploadServiceSettings',
        form,
        fileInputName,
        { 'shopGuid': shopGuid, 'serviceSettingsName': serviceSettingsName, 'shopSettingsType': shopSettingsType },
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

    await fetchFormData(formData, '/Home/SaveShopSettings', 'post', callback);
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