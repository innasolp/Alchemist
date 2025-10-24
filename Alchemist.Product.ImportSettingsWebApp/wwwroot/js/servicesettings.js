function getServiceData(formData) {
    var formDataCopy = getFormDataCopy(formData);
    //todo ???
    formDataCopy.delete('uploadValueFromJson');
    formDataCopy.delete('Value');
    const data = Object.fromEntries(formDataCopy.entries());  
    return data;
}

function onServiceSettingsChanged(formData, onChanged) {

    var serviceData = getServiceData(formData);

    const shopSettingsType = $("#ShopSettingType").val();
    const shopId = $("#ShopId").val();

    postJsonData(`/Import/Settings/${shopId}/${shopSettingsType}/Service/IsChanged`,
        JSON.stringify(serviceData),
        (result) => onChanged(result)
    );
}

const serviceModalSettings = new ModalForm(".serviceSettingsModal", ".serviceSettingsModalBody", '#serviceSettingsCloseBtn', null, '#settingsForm', new InputConfirmationSettings('#serviceSettingsForm', onServiceSettingsChanged));
function showServiceSettingsModal(url, data, onServiceSet, onHide = null) {
    serviceModalSettings.show(url,
        data,
        function (response)
        {
            $("#saveServiceSettingsBtn").on('click', function (event) {
                event.target.setAttribute('disabled', true);
                saveServiceSettings('#serviceSettingsForm', onServiceSet);
            });
        },
        onHide);
}

function enableSaveServiceSettingsButton() {
    enableButton('#saveServiceSettingsBtn');
}

function saveServiceSettings(serviceForm, onServiceSet) {    

    var onSuccess = (data) => {

        serviceModalSettings.closeModal(true);
        if (data == null) return;
        onServiceSet(data);
        enableSaveServiceSettingsButton();
    }
    
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

function onSetPrimaryServiceClick(event) {
    var serviceName = event.target.getAttribute("data-name");

    var shopId = $("#ShopId").val();
    var shopSettingsType = $("#ShopSettingType").val();

    showServiceSettingsModal(`/Import/Settings/${shopId}/${shopSettingsType}/PrimaryService/${serviceName}`,
        null,
        (result) => {
            $(`#${serviceName}`).val(result.serviceTypeName);
        },
        (result) => {
            if (result) clearFileNameFromUploadControl(`${serviceName}Json`);
        });
}

function onSetSecondaryServiceClick(event) {
    var guid = event.target.hasAttribute("data-guid") ? event.target.getAttribute("data-guid") : null;
    var shopId = $("#ShopId").val();
    var shopSettingsType = $("#ShopSettingType").val();
    var url = guid != null ? `/Import/Settings/${shopId}/${shopSettingsType}/Service/${guid}` : `/Import/Settings/${shopId}/${shopSettingsType}/Service`;
    showServiceSettingsModal(url,
        null,
        onSetServiceItem
        //todo
        //,(result) => {
        //    if (result) clearFileNameFromUploadControl(`${serviceName}Json`);
        //}
    );
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

async function uploadServiceSettingsFromJson(formSelector, fileInputName, url, onSuccess) {
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
