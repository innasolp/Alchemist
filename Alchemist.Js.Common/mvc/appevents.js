class AppStartEvent extends Event {
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

    constructor(appName, startAction) {
        super(AppStartEvent.eventName, { bubbles: true, composed: true });
        this.#appName = appName;
        this.#startAction = startAction;
    }
}

class AppClosingEvent extends Event {
    static eventName = 'app-closing';

    #appName = "";

    get appName() {
        return this.#appName;
    }

    set appName(value) {
        this.#appName = value;
    }    

    constructor(appName) {
        super(AppClosingEvent.eventName, { bubbles: true, composed: true, cancelable : true });
        this.#appName = appName;
    }
}