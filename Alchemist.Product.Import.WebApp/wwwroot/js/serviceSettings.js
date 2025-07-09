function setServiceSettingsFromJson(form, fileInputName, shopGuid, shopSettingsGuid, serviceSettingsName, onSuccess = null) {
    uploadFromJson('/FileUpload/UploadServiceSettings',
        form,
        fileInputName,
        { 'shopGuid': shopGuid, 'serviceSettingsName': serviceSettingsName, 'shopSettingsGuid': shopSettingsGuid },
        onSuccess
    );
}

function onCloseServiceSettingsWithConfirm(event) {
    event.preventDefault();

    var data = getFormData($('#serviceSettingsForm'));

    postData('/ServiceSettings/IsChanged',
        { data: JSON.stringify(data) },
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

    $('#serviceSettingsCloseBtn').on('click', onCloseServiceSettingsWithConfirm);

    $(".serviceSettingsModalBody").on('load', function (event) {
        console.log(event);
        console.trace(event);
    });

    showItemModal($(".serviceSettingsModal"), $(".serviceSettingsModalBody"), url, data, () => { if (onHide != null) onSaveServiceSettings(onHide); })
}

function onSaveServiceSettings(onHide) {
    if ($('#modalResult').val() == 'success' || $('#modalResult').val() == 1 || $('#modalResult').val() == true) {
        $('#modalResult').remove();
        onHide(true);
    }
    else
        onHide(false);
}

function closeServiceSettingsModal(success = true) {
    if (success)
        $(".serviceSettingsModal").append("<input type='hidden' id='modalResult' value='success'/>");
    $(".serviceSettingsModal").modal("hide");
    $('#serviceSettingsCloseBtn').off('click', onCloseServiceSettingsWithConfirm);
    $('#settingsForm').off('submit', submitPreventDefault);
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