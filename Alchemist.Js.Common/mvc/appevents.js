class AppStartEvent extends CustomEvent {
    static eventName = 'app-start';

    #appName = "";

    get appName() {
        return this.#appName;
    }

    set appName(value) {
        this.#appName = value;
    }

    #startAction = "";

    get startAction() {
        return this.#startAction;
    }

    set startAction(value) {
        this.#startAction = value;
    }   

    constructor(appName, startAction, detail) {
        super(AppStartEvent.eventName, { bubbles: true, composed: true, detail: detail });
        this.#appName = appName;
        this.#startAction = startAction;
    }
}

class AppClosingEvent extends CustomEvent {
    static eventName = 'app-closing';

    #appName = "";

    get appName() {
        return this.#appName;
    }

    set appName(value) {
        this.#appName = value;
    }    

    constructor(appName, detail) {
        super(AppClosingEvent.eventName, { bubbles: true, composed: true, cancelable: true, detail: detail });
        this.#appName = appName;
    }
}