// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function formDataToJson(formData) {
    var object = {};  

    for (const key of formData.keys()) {
        if (!(formData.get(key) instanceof File))
            object[key] = formData.get(key);
    }

    var json = JSON.stringify(object);
    return json;
}

function getFormData(formSelector) {
    var formData = new FormData(formSelector[0]);
    var object = {};

    for (const key of formData.keys()) {
        if (!(formData.get(key) instanceof File))
            object[key] = formData.get(key);
    }

    return object;
}

async function fetchFormData(formData, action, method = 'post', onSuccess = null, onError=null)
{
    const request = new Request(action, {
            method: method,
            body: formData,
        });

    await fetch(request).then((r) => {
        if (r.ok) {
            if (onSuccess != null)
                onSuccess(r.body);

            console.log(r);
        }
        else {
            if (onError != null)
                onError(r.body);
            console.error(r);
        }
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

function save(formSelector, url, data, onValidationError = null, onSuccess=null, onError=null) {

    $.validator.unobtrusive.parse(formSelector);

    if (!formSelector.valid()) {
        if (onValidationError != null)
            onValidationError();
        return;
    }

    var pendingRequest = formSelector.data('validator').pendingRequest;
    if (pendingRequest == 0)
        postData(url, data, onSuccess, onError);
    else
        setTimeout(() =>
        {
            if (formSelector.valid())
                postData(url, data, onSuccess, onError);
            else if (onValidationError != null)
                onValidationError();                
        }
        ,500);
}

function setValIfValid(selectorId, value) {
    $(selectorId).val(value);

    var form = $(selectorId).closest("form");
    $.validator.unobtrusive.parse(form);

    return form.valid();
}

function confirm(title, message, onSuccess = null, onCancel = null) {
    $.confirm({
        title: title,
        content: message,
        buttons: {
            'confirm':               
            { text: 'Yes',
                action: function () {
                    if (onSuccess != null)
                        onSuccess();
                }
            }                ,
            cancel: function () {
                if (onCancel != null)
                    onCancel();
            }            
        }
    });   
}

async function tabChanged(formSelector, callback = null) {

    var formData = getFormData(formSelector);
    var json = formDataToJson(new FormData(formSelector[0]));
    formData['json'] =  json;

    postData('/Home/IsTabChanged', formData, (result) => {

        if (!result) {
            if (callback != null) callback();
            return;
        }

        confirm('Confirmation', 'Input data will be reset. Continue?',
            () => {
                if (callback != null) callback();
            }
        );
    });
}
function setDivToForm(formDiv, div, formId) {
    var form = $("<form id='" + formId + "'></form>");
    formDiv.append(form);
    var newDiv = $(div[0].outerHTML);
    form.append(newDiv);
    div.remove();
}


