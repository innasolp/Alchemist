// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function formDataToJson(formData) {
    var object = {};
    formData.forEach(function (value, key) {
        if (!(value instanceof File))
            object[key] = value;
    });
    var json = JSON.stringify(object);
    return json;
}

function getFormData(formSelector) {
    var formData = new FormData(formSelector[0]);
    var object = {};
    formData.forEach(function (value, key) {
        if (!(value instanceof File))
         object[key] = value;
    });
    return object;
}

async function fetchFormData(formData, action, method = 'post', callback = null)
{
    const request = new Request(action, {
            method: method,
            body: formData,
        });

    await fetch(request).then((r) => {
        if (r.ok) {

            if (callback != null)
                callback();

            console.log(r);
        }
        else 
            console.error(r);
    });    
}

function sendFormData(url, data, onSuccess = null) {
    $.ajax({
        method:'POST',
        url: url,
        data: data,
        processData: false,
        contentType: false,
        success: function (data) {
            console.log('success');
            if (onSuccess != null) onSuccess(data);
        },
        error: function (err) {
            console.error("Failed");
            console.trace(err);
        }
    });
}

function showItemModal(divModelSelector, modalBodyDivSelector, url, data, onHide = null)
{

    if (onHide != null)
        divModelSelector.on('hide.bs.modal', function () {
            onHide();
        });

    if (data != null)
        modalBodyDivSelector.load(url, data, function (response, status, xhr) {
            onLoadCallback(url, response, status, xhr, () => { divModelSelector.modal("show"); })
        });
    else
        modalBodyDivSelector.load(url, function (response, status, xhr) {
            onLoadCallback(url, response, status, xhr, () => { divModelSelector.modal("show"); })
        }); 
}

function onLoadCallback(url, response, status, xhr, onSuccess) {
    if (status == "error") {
        console.error(xhr);
        if (response)
            console.trace(response);
    }
    else {
        console.log('url ' + url + ' load');
        onSuccess();
    }
}

function submitPreventDefault(event) {
    event.preventDefault();
}

function postData(url, data, onSuccess = null, onError = null) {
    $.ajax({
        type: 'POST',
        url: url,
        data: data,
        success: function (result) {
            console.log('saved');
            if (onSuccess != null)
                onSuccess(result);
        },
        error: function (err) {
            if (onError != null)
                onError(err);
            console.error("Not Saved");
            console.trace(err);
        }
    });
}

function save(selectorId, url, data, onSuccess=null, onError=null) {

    $.validator.unobtrusive.parse($(selectorId));

    if (!$(selectorId).valid()) return;

    var pendingRequest = $(selectorId).data('validator').pendingRequest;
    if (pendingRequest == 0)
        postData(url, data, onSuccess);
    else
        setTimeout(() =>
        {
            if ($(selectorId).valid())
                postData(url, data, onSuccess, onError);
        }
        ,500);
}

function setValIfValid(selectorId, value) {
    $(selectorId).val(value);

    var form = $(selectorId).closest("form");
    $.validator.unobtrusive.parse(form);

    return form.valid();
}
