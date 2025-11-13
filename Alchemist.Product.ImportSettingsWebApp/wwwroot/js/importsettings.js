function uploadShopList(shopId, shopSettingsType, onLoadSuccess = loadImportSettingsTab) {

    const hrefFormat = `/Import/Settings/{0}/${shopSettingsType ?? 'Product'}`;
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

                onLoadSuccess(shopId, shopSettingsType);
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