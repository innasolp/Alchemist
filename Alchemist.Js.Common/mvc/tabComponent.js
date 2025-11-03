class TabSelectEvent extends Event {
    static eventName = 'tab-select';

    constructor() {
        super(TabSelectEvent.eventName, { bubbles: true, composed: true, cancelable: true });
    }
}

class TabElement extends HTMLElement {
    constructor() {
        super();
    }

    get name() {
        return this.getAttribute('name');
    }

    set name(value) {
        this.setAttribute('name', value);
    }

    #selected = false;

    get selected() {
        return this.#selected;
    }

    set selected(value) {
        this.#selected = value;

        if (this.#selected == true && !this.#a.hasClass("selected"))
            this.#a.addClass("selected");
        else if (this.#selected == false && this.#a.hasClass("selected"))
            this.#a.removeClass("selected");
    }

    get #a() {
        return $(this).find('a');
    }

    get action() {
        return this.#a.attr('href');
    }

    set action(value) {
        this.#a.attr('href', value);
    }    

    get method() {
        return this.getAttribute('method');
    }

    set method(value) {
        this.setAttribute('method', value);
    }

    get content() {
        return this.getAttribute('content');
    }

    set content(value) {
        if (value) {
            this.setAttribute('content', value);
        } else {
            this.removeAttribute('content');
        }
    }

    static get observedAttributes() {
        return ['content']; 
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (name === 'content') {
            this.#a.text(newValue);
        }
    }

    connectedCallback() {
        const fullDivClassName = this.className != null ? `tab-item ${this.className}` : 'tab-item';
        const tabDiv = $(`<div class='${fullDivClassName}'></div>`);
        tabDiv.on('click', this, this.#onClick);

        var fullAnchorClassName = `display-8 menu_header ${this.selected == true ? "selected" : "disabled-link"}`;
        if(this.className != null) fullAnchorClassName = fullAnchorClassName + " " + this.className;
        const anchor = $(`<a class='${fullAnchorClassName}'></a>`);
        anchor.text(this.content);
        tabDiv.append(anchor);        

        $(this).append(tabDiv);
    }

    #onClick(event) {
        const tabSelectEvent = new TabSelectEvent();
        if (this.dispatchEvent(tabSelectEvent)) {
            if (event.data instanceof TabElement)
                event.data.selected = true;
        }
        else
            event.preventDefault();
    }
}


class TabPanelElement extends HTMLElement {
    constructor() {
        super();
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

    get #tabs() {
        return $(this).children('tab-item');
    }

    connectedCallback() {
        const fullClassName = this.className != null ? `tabs ${this.className}` : 'tabs';
        const tabsDiv = $(`<div class='${fullClassName}'></div>`);

        this.#tabs.each((index,child) => {
            $(child).on('tab-select', this, this.#onTabSelect);
        });

        $(this).append(tabsDiv); 
    }

    #onTabSelect(event) {
        if (!(event.data instanceof TabPanelElement)) return;

        const childElements = event.data.querySelectorAll('tab-item');
        childElements.forEach(child => {
            if (child != event.target)
                child.selected = false;
        });
    }
}

customElements.define("tab-item", TabElement);
customElements.define("tab-panel", TabPanelElement);
