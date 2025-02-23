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

function postFormInputFile(url, formSelector, fileInputName, data=null, onSuccess = null) {
    var formData = new FormData(formSelector[0]);
    var postData = new FormData();
    postData.append('file', formData.get(fileInputName), formData.get(fileInputName));

    if (data != null) {
        for (var key in data) {
            postData.append(key, data[key]);
        }
    }

    postFile(url, postData, onSuccess);
}

function uploadFromJson(uploadUrl,formSelector, fileInputName,  data, onSuccess = null) {
    postFormInputFile(uploadUrl,
        formSelector,
        fileInputName,
        data,
        (result) => {
            if (result == null) return;
            onSuccess(result);
        });
}

