class ShowModalCompleteEvent extends Event {
    static eventName = 'show-modal-complete';    

    constructor() {
        super(ShowModalCompleteEvent.eventName, { bubbles: true, composed: true });            
    }
}

class ModalCloseEvent extends Event {
    static eventName = 'modal-close'; 

    constructor() {
        super(ModalCloseEvent.eventName, { bubbles: true, composed: true, cancelable : true });        
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

    async #onClose(event) {
        event.preventDefault();

        const onModalClose = event.data.onClose;

        var result = true;

        if (typeof onModalClose == "function") {
            if (onModalClose.constructor.name == "AsyncFunction") 
                result = await onModalClose();            
            else 
                result = onModalClose();            
        }

        if (result == true)
            event.data.target.closeModal();
    }    

    #onShow(event) {
        const onShow = event.data;
        if (onShow != null)
            onShow();
    }

    #onHide(event) {
        //todo
    }

    get #modalDiv() {
        return $(this).find('div.modal.fade');
    }

    get #modalCloseBtn() {
        return $(this).find('a.close');
    }

    get #modalBodyDiv() {
        return $(this).find('div.modal-body');
    }    

    #onLoad(event) {
        console.log(event);
        console.trace(event);
    }

    showModal(url, data, onShow = null, onClose = null) {
        this.#modalCloseBtn.on('click', { target : this, onClose : onClose} , this.#onClose);

        this.#modalBodyDiv.on('load', this.#onLoad);

        this.#modalDiv.on('hide.bs.modal', this.#onHide);

        this.#modalDiv.on('show.bs.modal', onShow, this.#onShow);

        const onComplete = (response, success) => {
            if (success) {
                this.#modalDiv.modal("show");
            }
        };

        const onLoadCallback = function (url, response, status, xhr, onComplete) {
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
        };

        if (data != null)
            this.#modalBodyDiv.load(url, data, function (response, status, xhr) {
                onLoadCallback(url, response, status, xhr, onComplete)
            });
        else
            this.#modalBodyDiv.load(url, function (response, status, xhr) {
                onLoadCallback(url, response, status, xhr, onComplete)
            });
    }

    closeModal() {
        this.#modalDiv.modal("hide");
        this.#modalCloseBtn.off('click', this.#onClose);
        this.#modalBodyDiv.off('load', this.#onLoad);
        this.#modalDiv.off('hide.bs.modal', this.#onHide);
        this.#modalDiv.off('show.bs.modal', this.#onShow);
    }
}

customElements.define("modal-div", ModalElement);

class ShowModal
{
    #modalElement;

    #parentForm;

    constructor(modalElement, parentForm = null) {       
        this.#modalElement = modalElement;
        this.#parentForm = parentForm;
    }

    #submitPreventDefault(event) {
        event.preventDefault();
    }    

    show(url, data, onShow = null, onClose = null) {
        if (this.#parentForm != null)
            $(this.#parentForm).on('submit', this.#submitPreventDefault);
        
        $(this.#modalElement)[0].showModal(url, data, onShow, onClose);
    }    

    closeModal() {
        if (this.#parentForm != null)
            $(this.#parentForm).off('submit', this.#submitPreventDefault);        

        $(this.#modalElement)[0].closeModal();
    }
}