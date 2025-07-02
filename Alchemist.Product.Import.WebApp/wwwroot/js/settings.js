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

    $('#serviceSettingsCloseBtn').on('click', onCloseWithConfirm);

    $("#modalBodyDiv").on('load', function (event) {
        console.log(event);
        console.trace(event);
    });

    showItemModal($("#divModal"), $("#modalBodyDiv"), url, data, () => { if (onHide != null) onSaveServiceSettings(onHide); })
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
    if(success)
        $("#divModal").append("<input type='hidden' id='modalResult' value='success'/>");
    $("#divModal").modal("hide");
    $('#serviceSettingsCloseBtn').off('click', onCloseWithConfirm);
    $('#settingsForm').off('submit', submitPreventDefault);
}

async function setShopSettings(formSelector, callback = null) {
    var formData = new FormData(formSelector[0]);

    var json = formDataToJson(formData);
    formData.append('json', json);

    await fetchFormData(formData, '/ShopSettings/Save', 'post', callback);
}

async function redirectToShopSettings(data, currentTab) {
    await setShopSettings($('#settingsForm'),
        (result) => {
            $('#shopSettingsPartialDiv').load(
                '/ShopSettings/',
                data,
                function (response, status, xhr) { setTabsSelection($('.shopTab'), currentTab, "selected-a"); })
    });
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

function showRootCategory(data) {   

    $("#categoryModalBodyDiv").on('load', function (event) {
        console.log(event);
        console.trace(event);
    });    

    $('#divRootCategoryModal').on("show.bs.modal", function () {
        if ($('#rootCategoryForm').length == 0)
            setDivToForm($('#rootCategoryDiv'), $('#rootCategoryFormDiv'), 'rootCategoryForm');
    });

    if (data.hasOwnProperty('guid'))
        showItemModal($("#divRootCategoryModal"), $("#categoryModalBodyDiv"), '/ShopSettings/RootCategory/Edit', { data: JSON.stringify(data) });
    else
        showItemModal($("#divRootCategoryModal"), $("#categoryModalBodyDiv"), '/ShopSettings/RootCategory/New', data);
}

function setRootCategoryLi(rootCategory, ulRootCategories) {

    var item = ulRootCategories.find("#rootcategory_li_" + (rootCategory.guid));
    if (item.length > 0) {
        item.find('.item').text(rootCategory.item);
        item.find('.url').text(rootCategory.url);
        item.find('.guid').val(rootCategory.guid);
    }
    else {
        var items = ulRootCategories.children('.rootcategory_li');
        if (items.length == 0) items = ulRootCategories.children('.header');
        if (items.length == 0) return;

        var li = $("<li>", { "id": "rootcategory_li_" + rootCategory.guid, "class": "table-ul rootcategory_li" });
        var divFlex = $("<div>", { "class": "flex rootCategoryRow" });
        var divItem = $("<div>", { "class": "table_cell item" }).text(rootCategory.item);
        var divUrl = $("<div>", { "class": "table_cell url" }).text(rootCategory.url);
        var hiddenGuid = $("<input>", { "type": "hidden", "class": "guid" }).val(rootCategory.guid);
        var divEdit = $("<div>");
        var btnEdit = $("<i>", { "class": "fa fa-edit editRootCategory", "style": "font-size:18px" })
            .on("click", function () { showRootCategory(rootCategory); });
        divEdit.append(btnEdit);
        divFlex.append(divItem).append(divUrl).append(hiddenGuid).append(divEdit);
        li.append(divFlex);
        items.last().after(li);
    }
}

function setServiceSettingsLi(service, ulServices) {

    var item = ulServices.find("#service_li_" + (service.guid));
    if (item.length > 0) {
        item.find('.name').text(service.name);
        item.find('.serviceTypeName').text(service.serviceTypeName);
        item.find('.guid').val(service.guid);
    }
    else {
        var items = ulServices.children('.service_li');
        if (items.length == 0) items = ulServices.children('.header');
        if (items.length == 0) return;

        var li = $("<li>", { "id": "service_li_" + service.guid, "class": "table-ul service_li" });
        var divFlex = $("<div>", { "class": "flex serviceRow" });
        var divItem = $("<div>", { "class": "table_cell name", "style":"width:200px" }).text(service.name);
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
                setServiceSettingsLi(data, $('#servicesUl'));

            closeServiceSettingsModal();
        });
}

function saveShopSettings(shopSettingsFromSelector, type, submitter) {
    var url = `/ShopSettings/Save/${type}`;
    var data = JSON.stringify(getFormData(shopSettingsFromSelector));
    save(shopSettingsFromSelector,
        url,
        { data: data } ,
        () => { submitter.disabled = false; },
        (result) => { submitter.disabled = false; },
        (error) => { submitter.disabled = false; });
}

function saveRootCategory(formSelector, divModal, categoriesUL) {
    var data = getFormData(formSelector);
    save(formSelector,
        '/ShopSettings/RootCategory/Set',
        { data: JSON.stringify(data) },
        null,
        (data) =>
        {
            if (data == null) return;
            divModal.modal('hide');
            setRootCategoryLi(data, categoriesUL);
        });
}