async function setShopSettings(formSelector, callback = null) {
    var formData = new FormData(formSelector[0]);

    var json = formDataToJson(formData);
    formData.append('json', json);

    await fetchFormData(formData, '/ShopSettings/Save', 'post', callback);
}

async function redirectToShopSettings(data, currentTab) {
    await setShopSettings($('#settingsForm'),
        (result) => {
            $('.shopSettingsPartialDiv').load(
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