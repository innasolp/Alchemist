function setServiceSettingsFromJson(form, fileInputName, shopGuid, shopSettingsGuid, serviceSettingsName, onSuccess = null) {
    uploadFromJson('/FileUpload/UploadServiceSettings',
        form,
        fileInputName,
        { 'shopGuid': shopGuid, 'serviceSettingsName': serviceSettingsName, 'shopSettingsGuid': shopSettingsGuid },
        onSuccess
    );
}


function onServiceSettingsChanged(data, onChanged) {
    postData('/ServiceSettings/IsChanged',
        { data: JSON.stringify(data) },
        (result) => onChanged(result)
    );
}

function showServiceSettingsModal(url, data, onHide = null) {

    const serviceModalSettings = new ModalForm(".serviceSettingsModal", ".serviceSettingsModalBody", '#serviceSettingsCloseBtn', '#settingsForm', onServiceSettingsChanged, true);

    serviceModalSettings.show(url, data, onHide);
}

function setServiceSettingsLi(service, ulServices) {

    var items = ulServices.children('.service_li');
    var item = items.filter(function (index) {
        return $(this).children("div").children("input[class='guid']").val() == service.guid;
    });

    if (item.length > 0) {
        item.find('.name').text(service.name);
        item.find('.serviceTypeName').text(service.serviceTypeName);
        item.find('.guid').val(service.guid);
    }
    else {
        if (items.length == 0) items = ulServices.children('.header');
        if (items.length == 0) return;

        var li = $("<li>", { "id": "service_li_" + service.guid, "class": "table-ul service_li" });
        var divFlex = $("<div>", { "class": "flex serviceRow" });
        var divItem = $("<div>", { "class": "table_cell name", "style": "width:200px" }).text(service.name);
        var divUrl = $("<div>", { "class": "table_cell serviceTypeName", "style": "width:200px" }).text(service.serviceTypeName);
        var hiddenGuid = $("<input>", { "type": "hidden", "class": "guid" }).val(service.guid);
        var divEdit = $("<div>");
        var btnEdit = $("<i>", { "class": "fa fa-edit editService", "style": "font-size:18px" })
            .on("click", function () {
                showServiceSettingsModal('/ServiceSettings/',
                    { 'shopGuid': service.ShopGuid, 'shopSettingsGuid': service.shopSettingsGuid, 'guid': service.guid });
            });
        divEdit.append(btnEdit);
        divFlex.append(divItem).append(divUrl).append(hiddenGuid).append(divEdit);
        li.append(divFlex);
        items.last().after(li);
    }
}

function saveServiceSettings(serviceFormSelector, serviceName, isPrimaryService) {
    var formData = getFormData(serviceFormSelector);
    save(
        serviceFormSelector,
        '/ServiceSettings/Save',
        { data: JSON.stringify(formData) },
        null,
        (data) => {
            if (data == null) return;

            if (isPrimaryService)
                setValIfValid('#' + serviceName, $('#ServiceTypeName').val());
            else
                setServiceSettingsLi(data, $('.services'));

            closeServiceSettingsModal();
        });
}