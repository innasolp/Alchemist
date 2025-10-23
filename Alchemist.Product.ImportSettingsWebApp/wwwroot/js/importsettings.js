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
}

function setItemsPreventClick() {
    $(".shop-tab").on('click', onSettingsTabChangePrevent);
}

function onSettingsTabChangePrevent(event) {

    var url = getIsSettingsChangedUrl();
    onItemChangePrevent(event, url, '#settingsForm');
}

async function uploadImportSettingsFromJson(formSelector, fileInputName, importSettingsUrl) {

    var lodImportSettingsToDiv = function (html) {
        $("#importSettingsDiv").html(html);
        initSettingsEvents();
    }

    postFormInputFile('/Upload/Json/',
        formSelector,
        fileInputName,
        null,
        (json) => {
            if (json == null) return;
            console.trace(json);
            console.log('shop settings upload successfully');
            postJsonData(importSettingsUrl, json, lodImportSettingsToDiv, null);
        }
    );
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