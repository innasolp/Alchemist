function loadImportSettingsTab(shopId, shopSettingsType) {
    var data = { shopSettingsType: shopSettingsType, shopId: shopId };
    postJsonData(url = '/Import/Settings/Tab',
        data = JSON.stringify(data),
        onSuccess = (result) => {
            $("#importSettingsTabDiv").html(result);

            setItemsPreventClick();
        },
        null
    );
}

function setItemsPreventClick() {
    $(".shop-tab").on('click', onSettingsTabChangePrevent);
}

function onSettingsTabChangePrevent(event) {

    var url = getIsSettingsChangedUrl();
    onItemChangePrevent(event, url, '#settingsForm');
}

async function uploadImportSettingsFromJson(formSelector, fileInputName, importSettingsUrl) {

    var lodImportSettingsToDiv = (html) => $("#importSettingsDiv").html(html);

    postFormInputFile('/Upload/Json/',
        formSelector,
        fileInputName,
        null,
        (data) => {
            if (data == null) return;
            console.trace(data);
            console.log('shop settings upload successfully');

            postJsonData(importSettingsUrl, data, lodImportSettingsToDiv, null);
        }
    );
}

function enableSaveSettingsButton() {
    enableButton('#saveImportSettingsBtn');
}
 
async function saveImportSettings(url) {   

    await saveSettings('#settingsForm', url,
        (result) => { enableSaveSettingsButton(); },
        (error) => { enableSaveSettingsButton(); },
        () => { enableSaveSettingsButton(); }
    );
}