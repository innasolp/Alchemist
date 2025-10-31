function getServiceData(formData) {
    var formDataCopy = getFormDataCopy(formData);
    //todo ???
    formDataCopy.delete('uploadValueFromJson');
    formDataCopy.delete('Value');
    const data = Object.fromEntries(formDataCopy.entries());  
    return data;
}

async function isServiceSettingsChanged(formData) {

    var serviceData = getServiceData(formData);

    const shopSettingsType = $("#ShopSettingType").val();
    const shopId = $("#ShopId").val();

    return await postJsonDataAsync(`/Import/Settings/${shopId}/${shopSettingsType}/Service/IsChanged`,
        JSON.stringify(serviceData));
}

const showServiceSettingsModal = new ShowModal(".service-settings-modal", "#settingsForm");

function enableSaveServiceSettingsButton() {
    enableButton('#saveServiceSettingsBtn');
}

function saveServiceSettings(serviceForm, onServiceSet) {    

    var onSuccess = (data) => {

        showServiceSettingsModal.closeModal();

        if (data == null) return;
        onServiceSet(data);
        enableSaveServiceSettingsButton();
    };
    
    validateForm($(serviceForm),
        () => {

            var serviceData = getServiceData(new FormData($(serviceForm)[0]));

            const shopSettingsType = $("#ShopSettingType").val();
            const shopId = $("#ShopId").val();
                
            postJsonData(url = `/Import/Settings/${shopId}/${shopSettingsType}/Service/Set`,
            JSON.stringify(serviceData),
            onSuccess = onSuccess,
            onError = (error) => { enableSaveServiceSettingsButton(); });

    }, enableSaveServiceSettingsButton);     
}

function onSetServiceItem(data) {
    var li = $('li.table-ul.service_li').filter(function () {
        return $(this).find('.guid').val() === data.guid;
    });
    if (li.length == 0)
        addServiceItem(data);
    else
        updateServiceItem(li, data);
}

function updateServiceItem(li, data) {
    li.find(".table_cell.name").text(data.name);
    li.find(".table_cell.serviceTypeName").text(data.serviceTypeName);
}

function addServiceItem(data) {
    var li = $('ul.table-ul.services > .add-btn-ul');
    postJsonData('/Import/Settings/Service/Item', JSON.stringify(data), (content) => {
        li.before(content);
        li.prev().find(".edit-service").on('click', onSetSecondaryServiceClick);
    });
}

function initServiceEvents() {
    $(".set-service").on('click', onSetPrimaryServiceClick);
    $(".edit-service").on('click', onSetSecondaryServiceClick);
    $(".add-service").on('click', onSetSecondaryServiceClick);

    $("file-upload.import-service").on('file-uploaded', onSetServiceUpload);
    $("file-upload.browser-data-loader").on('file-uploaded', onSetServiceUpload);
    $("file-upload.browser-launcher").on('file-uploaded', onSetServiceUpload);
    $("file-upload.web-loader").on('file-uploaded', onSetServiceUpload);
}

function onSaveServiceSettings(event) {
    event.target.setAttribute('disabled', true);
    saveServiceSettings('#serviceSettingsForm', onServiceSet);
}

async function tryServiceSettingsModalClose() {

    const isChanged = await isServiceSettingsChanged(new FormData($("#serviceSettingsForm")[0]));

    if (!isChanged) return true;

    return await confirmAsync('Reseting', 'Input values will be reset. Are you sure?');
}

function onSetPrimaryServiceClick(event) {
    var serviceName = event.target.getAttribute("data-name");

    const onSetPrimaryService = function (result) {
        $(`#${serviceName}`).val(result.serviceTypeName);
        clearFileNameFromUploadControl(`${serviceName}Json`);
    };    

    const shopId = $("#ShopId").val();
    const shopSettingsType = $("#ShopSettingType").val();

    const onShow = function (event) {
        $("#saveServiceSettingsBtn").on('click', function (event) {
                    event.target.setAttribute('disabled', true);
                    saveServiceSettings('#serviceSettingsForm', (result) => {
                        onSetPrimaryService(result);
                    });
                });
    };

    const url = `/Import/Settings/${shopId}/${shopSettingsType}/PrimaryService/${serviceName}`;
    showServiceSettingsModal.show(url, null, onShow, tryServiceSettingsModalClose);
}

function onSetSecondaryServiceClick(event) {

    const onShow = function (event) {
        $("#saveServiceSettingsBtn").on('click', function (event) {
            event.target.setAttribute('disabled', true);
            saveServiceSettings('#serviceSettingsForm', (result) => {
                onSetServiceItem(result);
            });
        });
    };

    const guid = event.target.hasAttribute("data-guid") ? event.target.getAttribute("data-guid") : null;
    const shopId = $("#ShopId").val();
    const shopSettingsType = $("#ShopSettingType").val();
    const url = guid != null ? `/Import/Settings/${shopId}/${shopSettingsType}/Service/${guid}` : `/Import/Settings/${shopId}/${shopSettingsType}/Service`;
    showServiceSettingsModal.show(url, null, onShow, tryServiceSettingsModalClose);    
}

function onSetServiceUpload(event) {
    const shopId = $("#ShopId").val();
    const shopSettingsType = $("#ShopSettingType").val();
    const modelName = $(event.target).attr("data-name") ?? null;
    const url = `/Import/Settings/${shopId}/${shopSettingsType}/PrimaryService/Set/${modelName}`;
    const fileInputName = $(event.target).find("input").attr("name");
    const onSuccess = (data) => {       
        $(`input[data-name='${modelName}']`).val(data.serviceTypeName);
    };
    uploadServiceSettingsFromJson($('#settingsForm'), fileInputName, url, onSuccess);
}

function uploadServiceSettingsFromJson(formSelector, fileInputName, url, onSuccess) {
    postFormInputFile('/Upload/Json/',
        formSelector,
        fileInputName,
        null,
        (json) => {
            if (json == null) return;
            console.trace(json);
            console.log('shop settings upload successfully');
            postJsonData(url, json, onSuccess, null);
        }
    );
}
