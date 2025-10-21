function uploadServiceSettingsValueFromJson(fileInputName, onSuccess) {
    postFormInputFile('/ServiceSettings/UploadFromFile', $('#serviceSettingsForm'), fileInputName, null, onSuccess);
}

function getServiceData(formData) {
    var formDataCopy = getFormDataCopy(formData);
    //todo ???
    formDataCopy.delete('uploadValueFromJson');
    formDataCopy.delete('Value');
    const data = Object.fromEntries(formDataCopy.entries());      
    return { ServiceSettings: data, ShopSettingsType: $("#ShopSettingType").val(), ShopId: $("#ShopId").val() };
}

function onServiceSettingsChanged(formData, onChanged) {

    var serviceData = getServiceData(formData);
    postJsonData('/Import/Settings/Service/IsChanged',
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
                
        postJsonData(url = "/Import/Settings/Service/Set",
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
}

function onSetPrimaryServiceClick(event) {
    var serviceName = event.target.getAttribute("data-name");

    var shopId = $("#ShopId").val();
    var shopSettingsType = $("#ShopSettingType").val();

    var data = { shopId: shopId, shopSettingsType: shopSettingsType, serviceName: serviceName };

    showServiceSettingsModal(`/Import/Settings/${serviceName}`,
        data,
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
    var data = { shopId: shopId, shopSettingsType: shopSettingsType, guid: guid };
    showServiceSettingsModal('/Import/Settings/Service',
        data,
        onSetServiceItem
        //todo
        //,(result) => {
        //    if (result) clearFileNameFromUploadControl(`${serviceName}Json`);
        //}
    );
}
