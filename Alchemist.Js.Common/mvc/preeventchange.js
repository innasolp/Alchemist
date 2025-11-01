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

function anchorAction(anchor) {
    window.location.href = anchor.attributes["href"].value;
}