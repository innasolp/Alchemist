export function confirm(title, message, onSuccess = null, onCancel = null) {
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