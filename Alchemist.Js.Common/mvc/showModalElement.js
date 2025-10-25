class ShowModalCompleteEvent extends Event {
    static eventName = 'show-modal-complete';

    #response = "";   

    get response() {
        return this.#response;
    }

    set response(value) {
        this.#response = value;
    }    

    constructor(response) {
        super(ShowModalCompleteEvent.eventName, { bubbles: true, composed: true });
        this.#response = response;        
    }
}

class ModalClosedEvent extends Event {
    static eventName = 'modal-closed';

    #response = "";    

    get response() {
        return this.#response;
    }

    set response(value) {
        this.#response = value;
    }

    constructor(response) {
        super(ModalClosedEvent.eventName, { bubbles: true, composed: true });
        this.#response = response;
    }
}

class ModalErrorEvent extends Event {
    static eventName = 'modal-error';

    #error = "";    

    get error() {
        return this.#error;
    }

    set error(value) {
        this.#error = value;
    }

    #action = "";    

    get action() {
        return this.#action;
    }

    set action(value) {
        this.#action = value;
    }

    constructor(error, action) {
        super(ModalErrorEvent.eventName, { bubbles: true, composed: true });
        this.#error = error;
        this.#action = action;
    }
}


class ShowModalComponent extends HTMLElement {
    constructor() {
        super();
    }

    static get observedAttributes() {
        return ['name'];
    }

    get name() {
        return this.getAttribute('name');
    }

    set name(value) {
        this.setAttribute('name', value);
    }    

    attributeChangedCallback(name, oldValue, newValue) {
        //todo
    }

    connectedCallback() {
        const modalDiv = $("<div class='modal fade'></div>");

        const modalDialogDiv = $("<div class='modal-dialog'></div>");
        modalDiv.append(modalDialogDiv);

        const modalContentDiv = $("<div class='modal-content'></div>");
        modalDialogDiv.append(modalContentDiv);

        const modalHeaderDiv = $("<div class='modal-header'></div>");
        modalContentDiv.append(modalHeaderDiv);

        const closeBtn = $("<a href='#' class='close'>&times;</a>");
        modalHeaderDiv.append(closeBtn);

        const modalBodyDiv = $("<div class='modal-body'></div>");
        modalContentDiv.append(modalBodyDiv);

        $(this).append(modalDiv);
    }

    #submitPreventDefault(event) {
        event.preventDefault();
    }

    get #modalCloseBtn() {
        return $(this).find('a.close');
    }

    get #modalBodyDiv() {
        return $(this).find('div.modal-body');
    }

    get #modalDiv() {
        return $(this).find('div.modal.fade');
    }

    #onLoadCallback(url, response, status, xhr, onComplete) {
        if (status == "error") {
            console.error(xhr);
            if (response)
                console.trace(response);
            onComplete(response, false);
        }
        else {
            console.log('url ' + url + ' load');
            onComplete(response, true);
        }
    }

    #showItemModal(modalDiv, modalBodyDiv, url, data, onShow = null, onHide = null) {
        if (onHide != null)
            modalDiv.on('hide.bs.modal', function () {
                onHide(true);
            });

        const onComplete = (response, success) => {
            if (success) {
                modalDiv.modal("show");
                if (onShow != null)
                    onShow(response);
            }
        };

        if (data != null)
            modalBodyDiv.load(url, data, function (response, status, xhr) {
                this.#onLoadCallback(url, response, status, xhr, onComplete)
            });
        else
            modalBodyDiv.load(url, function (response, status, xhr) {
                this.#onLoadCallback(url, response, status, xhr, onComplete)
            });
    }

    show(url, data, parentForm) {

        if (parentForm != null)
            $(parentForm).on('submit', this.#submitPreventDefault);

        this.#modalCloseBtn().on('click', (event) => {
            //todo
        });

        $(this.#modalBodyDiv).on('load', function (event) {
            console.log(event);
            console.trace(event);
        });

        if (this.OnShow != null)
            $(this.#modalDiv).on("show.bs.modal", (event) => {
                //todo
            });

        this.#showItemModal($(this.#modalDiv), $(this.#modalBodyDiv), url, data, (event) => {
            //todo
        },
            () => { if (onHide != null) this.hide(onHide); })
    }

    #onModalClose(event) {

        event.preventDefault();

        const modalForm = event.data;

        if (modalForm.InputConfirmationSettings == null) {
            modalForm.closeModal();
            return;
        }

        var formData = new FormData($(modalForm.InputConfirmationSettings.Form)[0]);

        modalForm.InputConfirmationSettings.OnInputDataChanged(formData, changed => {
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

    closeModal(setSuccess = false) {
        if (this.ModalResultInput != null && setSuccess == true)
            $(this.ModalDiv).append(`<input type='hidden' id='${this.ModalResultInput}' value='success'/>`);

        $(this.ModalDiv).modal("hide");
        $(this.ModalCloseBtn).off('click', this.#onModalClose);
        if (parentForm != null)
            $(parentForm).off('submit', this.submitPreventDefault);

        if (this.OnShow != null)
            $(this.ModalDiv).off("show.bs.modal", this.OnShow);
    }    
}

customElements.define("show-modal", ShowModalComponent);