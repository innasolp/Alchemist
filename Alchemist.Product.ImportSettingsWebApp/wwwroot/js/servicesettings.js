function uploadServiceSettingsValueFromJson(fileInputName, onSuccess) {
    postFormInputFile('/ServiceSettings/UploadFromFile', $('#serviceSettingsForm'), fileInputName, null, onSuccess);
}

function onServiceSettingsChanged(data, onChanged) {
    postFormData('/Import/Settings/Service/IsChanged',
        data,
        (result) => onChanged(result)
    );
}

const serviceModalSettings = new ModalForm(".serviceSettingsModal", ".serviceSettingsModalBody", '#serviceSettingsCloseBtn', null, '#settingsForm', new InputConfirmationSettings('#serviceSettingsForm', onServiceSettingsChanged));
function showServiceSettingsModal(url, data, onHide = null) {
    serviceModalSettings.show(url, data, onHide);
}

function saveServiceSettings(serviceForm, setServiceInfo) {    

    var onSuccess = (data) => {

        serviceModalSettings.closeModal(true);
        if (data == null) return;
        setServiceInfo(data);
    }
    
    validateForm($(serviceForm), () => {

        var formData = new FormData($(serviceForm)[0]);
        postFormData(url = "/Import/Settings/Service/Save", formData = formData, onSuccess = onSuccess, onError = null);

    }, null);     
}
