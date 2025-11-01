class FileUploadedEvent extends Event {
    static eventName = 'file-uploaded';

    _fileName = "";

    get fileName() {
        return this._fileName;
    }

    set fileName(value) {
        this._fileName = value;
    }

    constructor(fileName) {
        super(FileUploadedEvent.eventName, { bubbles: true, composed: true });
        this._fileName = fileName;
    }
}

class FileUploadComponent extends HTMLElement { 
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
        if (name === 'name') {
            $(this).find("input").attr("name", newValue);
        }
    }

    connectedCallback() {
       
        var div = $("<div></div>");

        const fileInfoDiv = $("<div style='height:10px' class='file-upload file-info'></div>");

       
        var fileInput = $("<input type='file' style='display:none' class='file-upload' />");
        if (this.name != null) fileInput.attr("name", this.name);
        fileInput.on('change', (event) => {

            const fileName = event.target.files[0].name;

            $(event.target).parent().children("div.file-upload.file-info").html(`<i>${fileName}</i>`);

            const fileUploadedEvent = new FileUploadedEvent(fileName);            
            this.dispatchEvent(fileUploadedEvent);
        });

        var button = $("<button type='button'  class='btn btn-primary form-button file-upload'>From file</button>");        
        button.on("click", (event) => {
            $(event.target).parent().children("input.file-upload").click();
        });        

        div.append(fileInput); 
        div.append(button); 
        div.append(fileInfoDiv); 

        $(this).append(div);
    }
}

customElements.define("file-upload", FileUploadComponent);