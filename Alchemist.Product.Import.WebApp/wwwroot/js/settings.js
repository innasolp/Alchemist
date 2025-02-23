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

async function saveTab(formSelector, callback = null) {
    var formData = new FormData(formSelector[0]);

    var json = formDataToJson(formData);
    formData.append('json', json);

    await fetchFormData(formData, '/Home/SaveTabSettings', 'post', callback);    
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