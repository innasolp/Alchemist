function postFile(url, file, onSuccess = null) {
    $.ajax({
        type: 'POST',
        url: url,
        data: file,
        contentType: false,
        processData: false,
        success: function (data) {
            console.log('saved');
            onSuccess(data);
        },
        error: function (err) {
            console.error("Not Saved");
            console.trace(err);
        }
    });
}

async function postFileAsync(url, file) {
    try {
        var result = await $.ajax({
            method: 'POST',
            url: url,
            data: file,
            contentType: false,
            processData: false
        });
        console.log(`file loaded by action ${url} success.`);   
        return result;
    }
    catch (error) {
        console.error(`file loading by action ${url}  failed.`);
        console.trace(error);
    }        
}

function postFormInputFile(url, form, fileInputName, data=null, onSuccess = null) {
    var formData = new FormData($(form)[0]);
    var postData = new FormData();
    postData.append('file', formData.get(fileInputName), formData.get(fileInputName).name);

    if (data != null) {
        for (var key in data) {
            postData.append(key, data[key]);
        }
    }

    postFile(url, postData, onSuccess);
}

async function postFormInputFileAsync(url, form, fileInputName, data = null) {
    var formData = new FormData($(form)[0]);
    var postData = new FormData();
    postData.append('file', formData.get(fileInputName), formData.get(fileInputName).name);

    if (data != null) {
        for (var key in data) {
            postData.append(key, data[key]);
        }
    }

    return await postFileAsync(url, postData);
}

function uploadFromJson(uploadUrl,form, fileInputName,  data, onSuccess = null) {
    postFormInputFile(uploadUrl,
        form,
        fileInputName,
        data,
        (result) => {
            if (result == null) return;
            onSuccess(result);
        });
}

async function uploadFromJsonAsync(uploadUrl, form, fileInputName, data) {
    return await postFormInputFileAsync(uploadUrl,
        form,
        fileInputName,
        data);
}

