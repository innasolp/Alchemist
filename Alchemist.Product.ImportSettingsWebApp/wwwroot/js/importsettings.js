function loadImportSettingsTab(shopId, shopSettingsType) {
    postJsonData(url = `/Import/Settings/Tab/${shopId}/${shopSettingsType}`,
        null,
        onSuccess = (result) => {

            $("#importSettingsTabDiv").html(result);

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
        const url = `/Import/Settings/${shopSettingsType}/${shopId}`;
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
            `/Import/Settings/${shopSettingsType}/Save`,
            (result) => { enableSaveSettingsButton(); },
            (error) => { enableSaveSettingsButton(); },
            () => { enableSaveSettingsButton(); }
        );
    });
}