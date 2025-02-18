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

function postFormInputFile(url, form, name, onSuccess = null) {
    const formData = new FormData(form);
    var fileData = new FormData();
    fileData.append('file', formData.get(name), formData.get(name));
    postFile(url, fileData, onSuccess);
}
 
