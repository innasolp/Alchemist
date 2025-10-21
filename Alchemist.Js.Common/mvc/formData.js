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

function postData(url, data = null, onSuccess = null, onError = null,
    contentType = "application/x-www-form-urlencoded; charset=UTF-8",
    processData = false) {
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
    postData(url, formData, onSuccess, onError, false);
}

function postJsonData(url, jsonData, onSuccess = null, onError = null) {
    postData(url, jsonData, onSuccess, onError, 'application/json');
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

    if (!formSelector.data('validator').pendingRequest) {
        if (onValidationSuccess != null) onValidationSuccess();
        return;
    }

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

function tryFillFormDataByUrlParams(formData, paramNames) {
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.size > 0) {

        for (var i = 0; i < paramNames.length; i++) {
            if (!urlParams.has(paramNames[i])) continue;

            const value = urlParams.get(paramNames[i]);
            formData.append(paramNames[i], value);
        }
        return true;
    }
    return false;
}

function tryFillFormDataByPathNameParameters(formData, paramNames) {
    const url = new URL(window.location.href);
    const pathname = url.pathname;
    const pathSegments = pathname.split('/').filter(segment => segment !== '');

    if (pathSegments.length < paramNames.length) return false;

    const shift = pathSegments.length - paramNames.length;
    for (var i = 0; i < paramNames.length; i++) {
        formData.append(paramNames[i], pathSegments[i + shift]);
    }
    return true;
}

function getFormDataCopy(formData) {

    const copiedFormData = new FormData();

    for (const [key, value] of formData.entries()) {
        // Check if the value is a File or Blob to preserve its type and filename
        if (value instanceof File) {
            copiedFormData.append(key, value, value.name);
        } else if (value instanceof Blob) {
            copiedFormData.append(key, value); // Filename might be "blob" by default
        } else {
            copiedFormData.append(key, value);
        }
    }

    return copiedFormData;
}