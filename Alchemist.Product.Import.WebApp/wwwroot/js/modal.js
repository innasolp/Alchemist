class ModalForm {

    #ModalCloseBtn = '#modalCloseBtn';
    get ModalCloseBtn() {
        return this.#ModalCloseBtn;
    }

    #ModalDiv;
    get ModalDiv() {
        return this.#ModalDiv;
    }

    #ModalBodyDiv;
    get ModalBodyDiv() {
        return this.#ModalBodyDiv;
    }

    #ParentForm;
    get ParentForm() {
        return this.#ParentForm;
    }

    #ModalResultInput = 'modalResult';
    get ModalResultInput() {
        return this.#ModalResultInput;
    }

    #OnInputDataChanged = null;
    get OnInputDataChanged() {
        return this.#OnInputDataChanged;
    }

    #SetSuccessResult = false;
    get SetSuccessResult() {
        return this.#SetSuccessResult;
    }

    constructor(modalDiv, modalBodyDiv, modalCloseBtn, parentForm, onInputDataChanged = null, setSuccessResult = false, modalResultInput = 'modalResult') {
        this.#ModalCloseBtn = modalCloseBtn;
        this.#ModalDiv = modalDiv;
        this.#ModalBodyDiv = modalBodyDiv;
        this.#ParentForm = parentForm;
        this.#OnInputDataChanged = onInputDataChanged;
        this.#SetSuccessResult = setSuccessResult;
        this.#ModalResultInput = modalResultInput;
    }

    submitPreventDefault(event) {
        event.preventDefault();
    }

    close() {
        if (this.SetSuccessResult)
            $(this.ModalDiv).append(`<input type='hidden' id='${this.ModalResultInput}' value='success'/>`);
        $(this.ModalDiv).modal("hide");
        $(this.ModalCloseBtn).off('click', this.onModalClose);
        $(this.ParentForm).off('submit', this.submitPreventDefault);
    }

    onModalClose(event) {
        event.preventDefault();

        if (event.data.OnInputDataChanged == null) return;

        var data = getFormData($(event.data.ParentForm));

        event.data.OnInputDataChanged(data, changed => {
            if (!changed) {
                close(event.data.SetSuccessResult);
                return;
            }

            confirm('Reseting', 'Input values will be reset. Are you sure?',
                function () {
                    close(event.data.SetSuccessResult);
                });
        });
    }

    hide(onHide) {
        var result = $(this.ModalResultInput).val();
        if (result == 'success' || result == 1 || result == true) {
            $(this.ModalResultInput).remove();
            onHide(true);
        }
        else
            onHide(false);
    }

    show(url, data, onHide = null) {

        $(this.ParentForm).on('submit', this.submitPreventDefault);

        $(this.ModalCloseBtn).on('click', this, this.onModalClose);

        $(this.ModalBodyDiv).on('load', function (event) {
            console.log(event);
            console.trace(event);
        });

        showItemModal($(this.ModalDiv), $(this.ModalBodyDiv), url, data, () => { if (onHide != null) hide(onHide); })
    }
}

function showItemModal(modalDiv, modalBodyDiv, url, data, onHide = null) {
    if (onHide != null)
        modalDiv.on('hide.bs.modal', function () {
            onHide();
        });

    if (data != null)
        modalBodyDiv.load(url, data, function (response, status, xhr) {
            onLoadCallback(url, response, status, xhr, () => { modalDiv.modal("show"); })
        });
    else
        modalBodyDiv.load(url, function (response, status, xhr) {
            onLoadCallback(url, response, status, xhr, () => { modalDiv.modal("show"); })
        });
}
