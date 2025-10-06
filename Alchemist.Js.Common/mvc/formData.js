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

function fetchData(data,
    action,
    method = 'post',
    contentType = 'application/x-www-form-urlencoded; charset=UTF-8',
    onSuccess = null,
    onError = null)
{
    try {

        fetch(action, {
            method: method,
            body: data,
            headers: {
                'Content-Type': contentType,
            }
        })
            .then(response => {

                var r = response.clone();                

                if (r.ok) {
                    if (onSuccess != null)
                        onSuccess(r);

                    console.log(r);
                }
                else {
                    if (onError != null)
                        onError(r);
                    console.error(r);
                }                
            });
    }
    catch (error) {
        console.error(error);
        console.trace(error);
    }
}

function postData(url, data = null, onSuccess = null, onError = null, contentType = "application/x-www-form-urlencoded; charset=UTF-8", processData = false) {
    $.ajax({
        method: 'POST',
        url: url,
        data: data,
        contentType: contentType,
        processData: processData,
        success: function (result) {
            console.log(`post ${url} successed`);
            if (onSuccess != null)
                onSuccess(result);
        },
        error: function (err) {
            if (onError != null)
                onError(err);
            console.error(`post ${url} failed`);
            console.trace(err);
        }
    });
}

function postFormData(url, formData, onSuccess = null, onError = null) {
    postData(url, formData, onSuccess, onError, false, false);
}

function save(formSelector, url, data, onValidationError = null, onSuccess = null, onError = null) {

    $.validator.unobtrusive.parse(formSelector);

    if (!formSelector.valid()) {
        if (onValidationError != null)
            onValidationError();
        return;
    }

    var pendingRequest = formSelector.data('validator').pendingRequest;
    if (pendingRequest == 0)
        postData(url = url, data = data, onSuccess = onSuccess, onError = onError);
    else
        setTimeout(() => {
            if (formSelector.valid())
                postData(url = url, data = data, onSuccess = onSuccess, onError = onError);
            else if (onValidationError != null)
                onValidationError();
        }
            , 500);
}

function validateForm(formSelector, onValidationSuccess = null, onValidationError = null) {

    $.validator.unobtrusive.parse(formSelector);

    if (!formSelector.valid()) {
        if (onValidationError != null)
            onValidationError();
        return;
    }

    var pendingRequest = formSelector.data('validator').pendingRequest;
    if (pendingRequest == 0)
        if(onValidationSuccess != null) onValidationSuccess();
    else
        setTimeout(() => {
            if (formSelector.valid()) {
                if (onValidationSuccess != null) onValidationSuccess();
            }
            else if (onValidationError != null)
                onValidationError();
        }
            , 500);
}

function setValIfValid(selectorId, value) {
    $(selectorId).val(value);

    var form = $(selectorId).closest("form");
    $.validator.unobtrusive.parse(form);

    return form.valid();
}

function setDivToForm(formDiv, div, formId) {
    var form = $("<form id='" + formId + "'></form>");
    formDiv.append(form);
    var newDiv = $(div[0].outerHTML);
    form.append(newDiv);
    div.remove();
}