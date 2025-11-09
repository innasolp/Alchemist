function uploadShopList(shopId, shopSettingsType) {

    const hrefFormat = '/Import/Settings/{0}/' + shopSettingsType;
    var data = { hrefFormat: hrefFormat, shopId: shopId };

    postJsonData(url = '/ShopApi/ShopList',
        data = JSON.stringify(data),
        onSuccess = (result) => {
            $("#shopListDiv").html(result);

            if (shopId == null) {
                const shopItems = $("a.shop_item");
                if (shopItems.length == 0) return;
                window.location.href = shopItems.first().attr("href");
            }
            else {
                setShopSettingsItemsPreventClick();

                loadImportSettingsTab(shopId, shopSettingsType);
            }
        },
        null);
}

function enableButton(saveButton) {
    $(saveButton).prop('disabled', false);
}

async function saveSettings(settingsForm, url, onSuccess, onError, onValidateionError) {

    validateForm($(settingsForm), () => {

        var formData = new FormData($(settingsForm)[0]);
        postFormData(url = url, formData = formData, onSuccess = onSuccess, onError = onError);

    }, onValidateionError);
}

function setShopSettingsItemsPreventClick() {
    $(".shop_item").on('click', onImportSettingsItemChangePrevent);
}

function getIsSettingsChangedUrl() {
    var shopSettingsType = $("#ShopSettingType").val();
    return `/ImportSettingsAction/${shopSettingsType}/IsChanged`;
}

function onImportSettingsItemChangePrevent(event) {
    onItemChangePrevent(event, getIsSettingsChangedUrl(), '#settingsForm');
}

function loadImportSettingsTab(shopId, shopSettingsType) {
    postJsonData(url = `/ImportSettingsAction/${shopId}/${shopSettingsType}`,
        null,
        onSuccess = (result) => {

            $("#importSettingsDiv").html(result);

            initSettingsEvents();
        },
        null
    );
}

function initSettingsEvents() {
    initImportSettingsEvents();

    initRootCategoriesEvents();

    initServiceEvents();

    setItemsPreventClick();

    initImportSettingsUpload();
}

function setItemsPreventClick() {
    $(".shop-tab").on('click', onSettingsTabChangePrevent);
}

function onSettingsTabChangePrevent(event) {

    var url = getIsSettingsChangedUrl();
    onItemChangePrevent(event, url, '#settingsForm');
}

function initImportSettingsUpload() {

    const onImportSettingsUpload = async (event) => {
        const shopId = $("#ShopId").val();
        const shopSettingsType = $("#ShopSettingType").val();
        const url = `/ImportSettingsAction/Set/${shopId}/${shopSettingsType}`;
        const fileInputName = $(event.target).find("input").attr("name");        
        await uploadImportSettingsFromJson('#loadSettingsFromJsonForm', fileInputName, url);
    };

    $("file-upload.import-settings").on('file-uploaded', onImportSettingsUpload);
}

async function uploadImportSettingsFromJson(form, fileInputName, importSettingsUrl) {

    var json = await postFormInputFileAsync('/Upload/Json/', form, fileInputName, null);
    if (json == null) return;
    console.trace(json);
    console.log('shop settings upload successfully');
    var html = await postJsonDataAsync(importSettingsUrl, json);   
    $("#importSettingsDiv").html(html);
    //todo obsolete?
    initSettingsEvents();
}

function enableSaveSettingsButton() {
    enableButton('#saveImportSettingsBtn');
} 

function initImportSettingsEvents() {
    $("#saveImportSettingsBtn").on('click', async function (event) {
        event.target.setAttribute('disabled', true);
        const shopSettingsType = $("#ShopSettingType").val();

        await saveSettings('#settingsForm',
            `/ImportSettingsAction/${shopSettingsType}/Save`,
            (result) => { enableSaveSettingsButton(); },
            (error) => { enableSaveSettingsButton(); },
            () => { enableSaveSettingsButton(); }
        );
    });
}
const showRootCategoryModal = new ShowModal(".root-category-modal", "#settingsForm");

async function isCategoryUrlChanged(formData) {
    var formDataCopy = getFormDataCopy(formData);
    const data = Object.fromEntries(formDataCopy.entries());  
    const shopId = $("#ShopId").val();    
    return await postJsonDataAsync(`/ImportSettingsAction/Product/${shopId}/CategoryUrl/IsChanged`, JSON.stringify(data));
}

async function tryRootCategoryModalClose() {

    const isChanged = await isCategoryUrlChanged(new FormData($("#rootCategoryForm")[0]));

    if (!isChanged) return true;

    return await confirmAsync('Reseting', 'Input values will be reset. Are you sure?');
}


function onSetRootCategory(event) {
    var guid = event.target.hasAttribute("data-guid") ? event.target.getAttribute("data-guid") : null;
    const onShow = () => {
        setDivToForm($('#rootCategoryDiv'), $('#rootCategoryFormDiv'), 'rootCategoryForm');
        $("button.save-root-category").on("click", function (event){
            setCategoryUrl('#rootCategoryForm');
            });
    };
    showRootCategoryModal.show('/ImportSettingsAction/Product/CategoryUrl', getCategoryUrlData(guid), onShow, tryRootCategoryModalClose);
}

function getCategoryUrlData(guid) {
    var shopId = $("#ShopId").val();
    var data = { guid: guid, shopId: shopId };
    if (guid != null) {
        const li = $(`li[data-guid='${guid}']`);
        data.item = li.find(".table_cell.item").text();
        data.url = li.find(".table_cell.url").text();
    }
    return data;
}

function setCategoryUrl(categoryUrlForm, onSetCategory = null) {

    var onSuccess = (data) => {

        showRootCategoryModal.closeModal();
        if (data == null) return;
        onSetCategoryUrlItem(data);
        if (onSetCategory != null)
            onSetCategory(data);
    }

    validateForm($(categoryUrlForm), () => {

        var formData = new FormData($(categoryUrlForm)[0]);
        const shopId = $("#ShopId").val();
        postFormData(url = `/ImportSettingsAction/Product/${shopId}/CategoryUrl/Set`,
            formData = formData,
            onSuccess = onSuccess,
            onError = null);

    }, null);
}

function onSetCategoryUrlItem(data) {
    var li = $(`li[data-guid='${data.guid}']`);
    if (li.length == 0)
        addCategoryUrlItem(data);
    else
        updateCategoryUrlItem(li, data);
}

function updateCategoryUrlItem(li, data) {
    li.find(".table_cell.item").text(data.item);
    li.find(".table_cell.url").text(data.url);
}

function addCategoryUrlItem(data) {
    var li = $('ul.table-ul.rootCategories > .add-btn-ul');
    postJsonData('/ImportSettingsAction/Product/CategoryUrl/Item', JSON.stringify(data), (content) => {
        li.before(content);
        li.prev().find(".edit-root-category").on('click', onSetRootCategory);
    });
}

function initRootCategoriesEvents() {
    $(".add-root-category").on('click', onSetRootCategory);
    $(".edit-root-category").on('click', onSetRootCategory);
}
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

    return await postJsonDataAsync(`/ImportSettingsAction/${shopId}/${shopSettingsType}/Service/IsChanged`,
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
                
            postJsonData(url = `/ImportSettingsAction/${shopId}/${shopSettingsType}/Service/Set`,
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
    postJsonData('/ImportSettingsAction/Service/Item', JSON.stringify(data), (content) => {
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
    $("file-upload.request-headers").on('file-uploaded', onSetServiceUpload);
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
        $(`input[data-name='${serviceName}']`).val(result.serviceTypeName);
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

    const url = `/ImportSettingsAction/${shopId}/${shopSettingsType}/PrimaryService/${serviceName}`;
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
    const url = guid != null ? `/ImportSettingsAction/${shopId}/${shopSettingsType}/Service/${guid}` : `/ImportSettingsAction/${shopId}/${shopSettingsType}/Service`;
    showServiceSettingsModal.show(url, null, onShow, tryServiceSettingsModalClose);    
}

async function onSetServiceUpload(event) {
    const shopId = $("#ShopId").val();
    const shopSettingsType = $("#ShopSettingType").val();
    const modelName = $(event.target).attr("data-name") ?? null;
    const url = `/ImportSettingsAction/${shopId}/${shopSettingsType}/PrimaryService/Set/${modelName}`;
    const fileInputName = $(event.target).find("input").attr("name");
   
    var data = await uploadServiceSettingsFromJson('#settingsForm', fileInputName, url);
    $(`input[data-name='${modelName}']`).val(data.serviceTypeName);
    console.log(`service settings ${data.serviceTypeName} set from file successfully`);
}

async function uploadServiceSettingsFromJson(form, fileInputName, url) {
    var json = await postFormInputFileAsync('/Upload/Json/',
        form,
        fileInputName,
        null
    );
    if (json == null) return;
    console.trace(json);
    console.log('service settings file upload successfully');
    return await postJsonDataAsync(url, json);
}

// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(
    function () {
        $(document).on(AppStartEvent.eventName, (event) => {
            if (!event.detail.appName || event.detail.appName != 'importsettingsapp') return;

            if (!event.detail.shopId || !event.detail.shopSettingsType) return;

            uploadShopList(event.detail.shopId, event.detail.shopSettingsType);
        });
    });