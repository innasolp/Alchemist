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

function getFormData(form) {
    var formData = new FormData(form);
    var object = {};
    formData.forEach(function (value, key) {
        object[key] = value;
    });
    return object;
}

function getFormDataWithPrefix(formData, prefix) {

    var newFormData = new FormData();
    for (const key of formData.keys()) {
        var value = formData.get(key);
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

async function sendFormData(formData, action, method='post', callback=null) {
    try {

        const request = new Request(action, {
            method: method,
            body: formData,
        });

        await fetch(request).then(r => { callback(); console.log(r); }); 

    } catch (e) {
        console.error(e);
    }
}

function postXmlHttpFormData(form, formData) {
    //var verificationToken = getRequestVerificationToken();

    var body = JSON.stringify(formData);
    //console.trace(body);

    var xhr = new XMLHttpRequest();
    xhr.open(form.method, form.action, true);
    xhr.setRequestHeader("Content-Type", "application/json");//; charset=UTF-8");
    xhr.setRequestHeader("Accept", "application/json");//; charset=UTF-8");
   // xhr.setRequestHeader("RequestVerificationToken", verificationToken);//; charset=UTF-8");

    xhr.onload = () => {
        if (xhr.readyState == 4 && xhr.status == 201) {
            console.log(JSON.parse(xhr.responseText));
            if (onSuccess != null)
                onSuccess(xhr);
        }
        else {
            console.trace(xhr);
            if (onFail != null)
                onFail(xhr);
        }
    };

    xhr.onerror = (e) => {
        console.error(`Error: ${e}`);
    }

    xhr.send(body);
}
function ShowItemModal(url) {
    $("#modalBodyDiv").load(url, function (data) {
        $("#divModal").modal("show");
    })
}

function ShowItemModal(url, data) {
    $("#modalBodyDiv").load(url, data, function (obj) {
        $("#divModal").modal("show");
    })
}

function closeItemModal() {
    $("#divModal").modal("hide");
}

function postData(url, jsonData, onSuccess = null) {
    $.ajax({
        type: 'POST',
        url: url,
        data: jsonData,
        success: function () {
            console.log('saved');
            onSuccess();
        },
        error: function (err) {
            console.error("Not Saved");
            console.trace(err);
        }
    });
}

function save(url, jsonData, onSuccess=null) {

    $.validator.unobtrusive.parse($("#modalForm"));

    if(!$("#modalForm").valid()) return;

    var pendingRequest = $("#modalForm").data('validator').pendingRequest;
    if (pendingRequest == 0)
        postData(url, jsonData, onSuccess);
    else
        setTimeout(() =>
        {
            if ($("#modalForm").valid())
                postData(url, jsonData, onSuccess);
        }
        ,500);
}
