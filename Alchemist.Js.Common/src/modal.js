export class InputConfirmationSettings {
    #Form;
    get Form() {
        return this.#Form;
    }

    #OnInputDataChanged = null;
    get OnInputDataChanged() {
        return this.#OnInputDataChanged;
    }

    constructor(form, onInputDataChanged) {
        this.#Form = form;
        this.#OnInputDataChanged = onInputDataChanged;
    }
}

export class ModalForm {

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

    #InputConfirmationSettings = null;
    get InputConfirmationSettings() {
        return this.#InputConfirmationSettings;
    }    

    #OnShow = null;
    get OnShow() {
        return this.#OnShow;
    }

    constructor(modalDiv, modalBodyDiv, modalCloseBtn, onShow = null, parentForm = null, inputConfirmationSettings = null, modalResultInput = 'modalResult') {
        this.#ModalCloseBtn = modalCloseBtn;
        this.#ModalDiv = modalDiv;
        this.#ModalBodyDiv = modalBodyDiv;
        this.#OnShow = onShow;
        this.#ParentForm = parentForm;
        this.#InputConfirmationSettings = inputConfirmationSettings;
        this.#ModalResultInput = modalResultInput;
    }

    submitPreventDefault(event) {
        event.preventDefault();
    }

    closeModal(setSuccess = false) {
        if (this.ModalResultInput != null && setSuccess == true)
            $(this.ModalDiv).append(`<input type='hidden' id='${this.ModalResultInput}' value='success'/>`);

        $(this.ModalDiv).modal("hide");
        $(this.ModalCloseBtn).off('click', this.onModalClose);
        if (this.ParentForm != null)
            $(this.ParentForm).off('submit', this.submitPreventDefault);

        if (this.OnShow != null)
            $(this.ModalDiv).off("show.bs.modal", this.OnShow);
    }

    onModalClose(event) {

        event.preventDefault();

        const modalForm = event.data;

        if (modalForm.InputConfirmationSettings == null) {
            modalForm.closeModal();
            return;
        }

        var data = getFormData($(modalForm.InputConfirmationSettings.Form));

        modalForm.InputConfirmationSettings.OnInputDataChanged(data, changed => {
            if (!changed) {
                modalForm.closeModal(true);
                return;
            }

            confirm('Reseting', 'Input values will be reset. Are you sure?',
                function () {
                    modalForm.closeModal(false);
                });
        });
    }

    hide(onHide) {
        var result = $(`#${this.ModalResultInput}`).val();
        if (result == 'success' || result == 1 || result == true) {
            $(`#${this.ModalResultInput}`).remove();
            onHide(true);
        }
        else
            onHide(false);
    }

    show(url, data, onHide = null) {

        if (this.ParentForm != null)
            $(this.ParentForm).on('submit', this.submitPreventDefault);

        $(this.ModalCloseBtn).on('click', this, this.onModalClose);

        $(this.ModalBodyDiv).on('load', function (event) {
            console.log(event);
            console.trace(event);
        });

        if (this.OnShow != null)
            $(this.ModalDiv).on("show.bs.modal", this.OnShow);

        showItemModal($(this.ModalDiv), $(this.ModalBodyDiv), url, data, () => { if (onHide != null) this.hide(onHide); })
    }
}

export function showItemModal(modalDiv, modalBodyDiv, url, data, onHide = null) {
    if (onHide != null)
        modalDiv.on('hide.bs.modal', function () {
            onHide(true);
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
