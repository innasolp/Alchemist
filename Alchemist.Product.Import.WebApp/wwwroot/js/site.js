// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function selectItem(element, ulId, itemTag) {
    var classAttr = element.getAttribute("class") ?? "";
    if (!classAttr.includes("selected"))
        element.setAttribute("class", classAttr + " selected");

    var ul = document.getElementById(ulId);
    if (ul == null)
        return;

    var items = ul.getElementsByTagName(itemTag);
    for (const li of items) {
        if (li == element) continue;

        var liClassAttr = li.getAttribute("class") ?? "";
        if (liClassAttr.includes("selected"))
            li.setAttribute("class", liClassAttr.replace(" selected", ""));
    }
}

function formDataToJson(formData) {
    var object = {};
    formData.forEach(function (value, key) {
        if (!(value instanceof File))
            object[key] = value;
    });
    var json = JSON.stringify(object);
    return json;
}

function getFormAsJson(form) {
    let obj = {};
    let formData = form.serialize();
    let formArray = formData.split("&");

    for (inputData of formArray) {
        let dataTmp = inputData.split('=');
        obj[dataTmp[0]] = dataTmp[1];
    }
    return JSON.stringify(obj);
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

function getFormDataWithPrefix(formData, prefix) {

    var newFormData = new FormData();
    for (const key of formData.keys()) {
        var value = formData.get(key);
        if (value instanceof File)
            continue;
        var keyWithPrefix = prefix != null ? prefix + '.' + key : key;
        newFormData.append(keyWithPrefix, value);
    }   
    return newFormData;
}

function createInput(name, value) {
    var i = document.createElement("input");
    i.setAttribute('type', "hidden");
    i.setAttribute('name', name);
    i.setAttribute('value', value);
    return i;
}

function appendFormData(form, formData) {

    for (const key of formData.keys())
    {
        var input = createInput(key, formData.get(key));
        form.appendChild(input);     
    }
}

function addFormDataJson(formData) {
    var json = formDataToJson(formData);
    formData.append('json', json);
}

async function fetchFormData(formData, action, method = 'post', callback = null)
{
    const request = new Request(action, {
            method: method,
            body: formData,
        });

    await fetch(request).then((r) => {
        if (r.ok) { 
            callback();
            console.log(r);
        }
        else 
            console.error(r);
    });    
}

async function fetchForm(formSelector, action, method = 'post', callback = null) {
    var formData = getFormData(formSelector);
    await fetchFormData(formData, action, method, callback);
}

async function fetchRedirect(action, callback = null) {
    const request = new Request(action, {
        method: 'GET'
    });

    await fetch(request).then((r) => {
        if (r.ok) {
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

function display(source, displayId) {
    document.getElementById(displayId).value = source.value;
}


function setVal(selectorId, value) {
    $(selectorId).val(value);

    var form = $(selectorId).closest("form");
    $.validator.unobtrusive.parse(form);

    return form.valid();
}
