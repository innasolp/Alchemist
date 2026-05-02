function init(tab, appName, data) {
    
    const url = new URL(window.location.href);   
    const apiUrl = `/${tab}Api${url.pathname}`;
    data.appName = appName;

    postData(apiUrl, null, (response) => {
        $('#appDiv').html(response);
        document.dispatchEvent(new AppStartEvent(appName, "", data));
    });

    $("tab-item.app-item").on(TabSelectEvent.eventName, (event) => {
        event.preventDefault();
        const href = event.target.getAttribute('data-href');
        var eventDetails = { ...data };
        eventDetails.onSuccess = () => { window.location.href = href;  };
        document.dispatchEvent(new AppClosingEvent(appName, eventDetails));
    });
}
