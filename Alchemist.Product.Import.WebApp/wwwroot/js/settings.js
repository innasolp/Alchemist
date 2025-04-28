function setServiceSettingsFromJson(form, fileInputName, shopGuid, shopSettingsGuid, serviceSettingsName, onSuccess = null) {
    uploadFromJson('/FileUpload/UploadServiceSettings',
        form,
        fileInputName,
        { 'shopGuid': shopGuid, 'serviceSettingsName': serviceSettingsName, 'shopSettingsGuid': shopSettingsGuid },
        onSuccess
    );
}

function onCloseWithConfirm(event) {
    event.preventDefault();

    var data = getFormData($('#serviceSettingsForm'));

    postData('/ShopSettings/IsServiceSettingsChanged', data,
        (result) => {

            if (!result) {
                closeServiceSettingsModal(false);
                return;
            }

            confirm('Reseting', 'Input values will be reset. Are you sure?',
                function () {
                    closeServiceSettingsModal(false);
                });
        }
    );    
}

function showServiceSettingsModal(url, data, onHide = null) {
    
    $('#settingsForm').on('submit', submitPreventDefault);

    $('#serviceSettingsCloseBtn').on('click', onCloseWithConfirm);

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

function closeServiceSettingsModal(success = true) {
    if(success)
        $("#divModal").append("<input type='hidden' id='modalResult' value='success'/>");
    $("#divModal").modal("hide");
    $('#serviceSettingsCloseBtn').off('click', onCloseWithConfirm);
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
