function anchorAction(anchor) {
    window.location.href = anchor.attributes["href"].value;
}

function inputDataWillBeResetedDefaultConfirmation(event, actionContinue) {
    confirm('Confirmation', 'Input data will be reset. Continue?',
        () => {
            actionContinue(event.target);
        });
}

function onItemChangePrevent(event, isChangedActionUrl, form, actionContinue = anchorAction,
    confirmEventItemAction = (event, actionContinue) => inputDataWillBeResetedDefaultConfirmation(event, actionContinue)) {

    event.preventDefault();

    var formData = new FormData($(form)[0]);

    var onSuccess = function (changed) {
        if (!changed) {
            actionContinue(event.target);
        }
        else {
            confirmEventItemAction(event, actionContinue);
        }
    };

    postFormData(url = isChangedActionUrl, formData = formData, onSuccess = onSuccess, onError = null);
}

async function inputDataWillBeResetedDefaultConfirmationAsync() {
    return await confirmAsync('Confirmation', 'Input data will be reset. Continue?');
}

async function onEditableDataChangedWithConfirmAsync(formCheckChangedUrlsMap,
    confirmEventItemActionAsync = async function () { return await inputDataWillBeResetedDefaultConfirmationAsync(); }) {

    if (!(formCheckChangedUrlsMap instanceof Map)) return;

    for (const element of formCheckChangedUrlsMap.keys()) {
        if ($(element).length == 0) continue;
        const editFormData = new FormData($(element)[0]);
        const changed = await postFormDataAsync(formCheckChangedUrlsMap.get(element), editFormData);
        if (!changed) continue;        
        else {
            const result = await confirmEventItemActionAsync();
            return !result;           
        }
    }

    return false;
}