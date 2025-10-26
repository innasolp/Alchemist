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

class ModalCloseEvent extends Event {
    static eventName = 'modal-close';   

   

    constructor() {
        super(ModalCloseEvent.eventName, { bubbles: true, composed: true, cancelable : true });
        //this.#parentForm = parentForm;
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

class ModalHideEvent extends Event {
    static eventName = 'modal-hide';

    constructor() {
        super(ModalErrorEvent.eventName, { bubbles: true, composed: true });
    }
}

class ModalElement extends HTMLElement {
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
}

customElements.define("modal-div", ModalElement);

class ShowModal extends EventTarget {

    #modalElement;

    #parentForm;

    constructor(modalElement, parentForm = null) {
        super();
        this.#modalElement = modalElement;
        this.#parentForm = parentForm;
    }

    #submitPreventDefault(event) {
        event.preventDefault();
    }

    get #modalCloseBtn() {
        return $(this.#modalElement).find('a.close');
    }

    get #modalBodyDiv() {
        return $(this.#modalElement).find('div.modal-body');
    }

    get #modalDiv() {
        return $(this.#modalElement).find('div.modal.fade');
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

    #showItemModal(modalDiv, modalBodyDiv, url, data, onLoadCallback, onShow = null, onHide = null) {
        if (onHide != null)
            modalDiv.on('hide.bs.modal',onHide);

        if (onShow != null)
            modalDiv.on('show.bs.modal', onShow);

        const onComplete = (response, success) => {
            if (success) {
                modalDiv.modal("show");                
            }
        };

        if (data != null)
            modalBodyDiv.load(url, data, function (response, status, xhr) {
                onLoadCallback(url, response, status, xhr, onComplete)
            });
        else
            modalBodyDiv.load(url, function (response, status, xhr) {
                onLoadCallback(url, response, status, xhr, onComplete)
            });
    }

    #onShow(event) {
        const showModalCompleteEvent = new ShowModalCompleteEvent(response);
        this.dispatchEvent(showModalCompleteEvent);
    }

    #onHide(response) {
        const hideModalEvent = new ModalHideEvent(response);
        this.dispatchEvent(hideModalEvent);
    }

    show(url, data) {

        if (this.#parentForm != null)
            $(this.#parentForm).on('submit', this.#submitPreventDefault);

        this.#modalCloseBtn.on('click', this.#onModalClose);

        $(this.#modalBodyDiv).on('load', function (event) {
            console.log(event);
            console.trace(event);
        });


        this.#showItemModal($(this.#modalDiv), $(this.#modalBodyDiv), url, data,
            this.#onLoadCallback,
            this.#onShow,
            this.#onHide);
    }

    #onModalClose(event) {
        event.preventDefault();

        const modalClosedEvent = new ModalCloseEvent();

        if (this.dispatchEvent(modalClosedEvent)) return;

        closeModal();
    }

    closeModal() {
        $(this.#modalDiv).modal("hide");
        $(this.#modalCloseBtn).off('click', this.#onModalClose);
        if(this.#parentForm != null)
            $(this.#parentForm).off('submit', this.#submitPreventDefault);

        if (this.#onShow != null)
            $(this.#modalDiv).off("show.bs.modal", this.#onShow);
    }
}