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

    get selected() {
        return this.getAttribute('selected');
    }

    set selected(value) {
        this.setAttribute('selected', value);
        this.#setAnchorSelected(this.#a, value);
    }

    #setAnchorSelected(anchor, value) {
        if (value == true || value == 'selected') {
            if (!anchor.hasClass("selected")) anchor.addClass("selected");
            if (!anchor.hasClass("disabled-link")) anchor.addClass("disabled-link");
        }
        else {
            anchor.removeClass("selected");
            anchor.removeClass("disabled-link");
        }
    }

    get #a() {
        return $(this).find('a');
    }

    get action() {
        return this.getAttribute('action');
    }

    set action(value) {
        if (value) {
            this.setAttribute('action', value);
        } else {
            this.removeAttribute('action');
        }
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
        return ['content', 'action']; 
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (name === 'content') {
            this.#a.text(newValue);
        }
        else if (name === 'action') {
            this.#a.attr('href', newValue);
        }
    }

    connectedCallback() {
        const tabDiv = $("<div class='menu_item-a tab-item-div'></div>");
        tabDiv.on('click', this, this.#onClick);

        const anchor = $("<a class='display-8 menu_header'></a>");
        this.#setAnchor(anchor);
        tabDiv.append(anchor);        

        $(this).append(tabDiv);
    }

    #setAnchor(anchor) {
        if (this.className) anchor.addClass(this.className);
        this.#setAnchorSelected(anchor, this.selected);
        if (this.content) anchor.text(this.content);
        if (this.action) anchor.attr('href', this.action);
    }

    #onClick(event) {
        const tabSelectEvent = new TabSelectEvent();
        if (!(event.data instanceof TabElement)) return;

        if (event.data.dispatchEvent(tabSelectEvent)) {           
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
        const tabsDiv = $("<div class='tabs'></div>");
        if (this.className) tabsDiv.addClass(this.className);

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
