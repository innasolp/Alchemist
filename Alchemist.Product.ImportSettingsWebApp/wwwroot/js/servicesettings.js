function uploadServiceSettingsValueFromJson(fileInputName, onSuccess) {
    postFormInputFile('/ServiceSettings/UploadFromFile', $('#serviceSettingsForm'), fileInputName, null, onSuccess);
}

function tryFillFromDataByUrlParams(formData) {
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.size > 0) {
        if (urlParams.has("shopId")) {
            const shopId = urlParams.get('shopId');
            formD.append("shopId", shopId);
        }
        if (urlParams.has("shopSettingsType")) {
            const shopSettingsType = urlParams.get('shopSettingsType');
            formData.append("ParentShopSettingsType", shopSettingsType);
        }
        return true;
    }
    return false;
}

function tryFillFormDataByPathNameParameters(formData, paramNames) {
    const url = new URL(window.location.href);
    const pathname = url.pathname;
    const pathSegments = pathname.split('/').filter(segment => segment !== '');

    if (pathSegments.length < paramNames.length) return false;

    const shift = pathSegments.length - paramNames.length;
    for (var i = 0; i < paramNames.length; i++) {
        formData.append(paramNames[i], pathSegments[i + shift]);
    }
    return true;
}

function getFormDataCopy(formData) {

    const copiedFormData = new FormData();

    for (const [key, value] of formData.entries()) {
        // Check if the value is a File or Blob to preserve its type and filename
        if (value instanceof File) {
            copiedFormData.append(key, value, value.name);
        } else if (value instanceof Blob) {
            copiedFormData.append(key, value); // Filename might be "blob" by default
        } else {
            copiedFormData.append(key, value);
        }
    }

    return copiedFormData;
}

function onServiceSettingsChanged(formData, onChanged) {

    var data = getFormDataCopy(formData);
    
    postFormData('/Import/Settings/Service/IsChanged',
        data,
        (result) => onChanged(result)
    );
}

const serviceModalSettings = new ModalForm(".serviceSettingsModal", ".serviceSettingsModalBody", '#serviceSettingsCloseBtn', null, '#settingsForm', new InputConfirmationSettings('#serviceSettingsForm', onServiceSettingsChanged));
function showServiceSettingsModal(url, data, onHide = null) {
    serviceModalSettings.show(url, data, onHide);
}

function enableSaveServiceSettingsButton() {
    enableButton('#saveServiceSettingsBtn');
}

function saveServiceSettings(serviceForm, setServiceInfo) {    

    var onSuccess = (data) => {

        serviceModalSettings.closeModal(true);
        if (data == null) return;
        setServiceInfo(data);
        enableSaveServiceSettingsButton();
    }
    
    validateForm($(serviceForm), () => {

        var formData = new FormData($(serviceForm)[0]);
                
        postFormData(url = "/Import/Settings/Service/Set",
            formData = formData,
            onSuccess = onSuccess,
            onError = (error) => { enableSaveServiceSettingsButton(); });

    }, enableSaveServiceSettingsButton);     
}
