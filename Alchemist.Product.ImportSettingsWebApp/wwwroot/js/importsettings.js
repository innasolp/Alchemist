function uploadImportSettings(shopId, shopSettingsType) {
    var data = { shopSettingsType: shopSettingsType, shopId: shopId };
    postData(url = '/Import/SettingsTab',
        data = JSON.stringify(data),
        onSuccess = (result) => {
            $("#importSettingsDiv").html(result);
        },
        null,
        contentType = 'application/json; charset=utf-8'
    );
}