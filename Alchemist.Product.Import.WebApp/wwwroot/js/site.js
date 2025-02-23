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

function ShowItemModal(divModelSelector, modalBodyDivSelector, url, data, onHide = null)
{

    if (onHide != null)
        divModelSelector.on('hide.bs.modal', function () {
            onHide();
        });

    modalBodyDivSelector.load(url, data, function (obj) {
        console.log('url ' + url + ' load');
        divModelSelector.modal("show");
    })
}

function postData(url, jsonData, onSuccess = null) {
    $.ajax({
        type: 'POST',
        url: url,
        data: jsonData,
        success: function (data) {
            console.log('saved');
            if (onSuccess != null)
                onSuccess(data);
        },
        error: function (err) {
            console.error("Not Saved");
            console.trace(err);
        }
    });
}

function save(selectorId, url, jsonData, onSuccess=null) {

    $.validator.unobtrusive.parse($(selectorId));

    if (!$(selectorId).valid()) return;

    var pendingRequest = $(selectorId).data('validator').pendingRequest;
    if (pendingRequest == 0)
        postData(url, jsonData, onSuccess);
    else
        setTimeout(() =>
        {
            if ($(selectorId).valid())
                postData(url, jsonData, onSuccess);
        }
        ,500);
}

function setValIfValid(selectorId, value) {
    $(selectorId).val(value);

    var form = $(selectorId).closest("form");
    $.validator.unobtrusive.parse(form);

    return form.valid();
}
