function confirm(title, message, onSuccess = null, onCancel = null) {
    $.confirm({
        title: title,
        content: message,
        buttons: {
            'confirm':
            {
                text: 'Yes',
                action: function () {
                    if (onSuccess != null)
                        onSuccess();
                }
            },
            cancel: function () {
                if (onCancel != null)
                    onCancel();
            }
        }
    });
}

async function confirmAsync(title, message, onSuccess = null, onCancel = null) {

    const showConfirmation = function () {
        return new Promise((resolve) => {
            $.confirm({
                title: title,
                content: message,
                buttons: {
                    'confirm':
                    {
                        text: 'Yes',
                        action: function () {
                            if (onSuccess != null)
                                onSuccess();
                            resolve(true);
                        }
                    },
                    cancel: function () {
                        if (onCancel != null)
                            onCancel();
                        resolve(false);
                    }
                }
            });
        });
    };

    return await showConfirmation();
}
