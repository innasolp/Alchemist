$(
    function ()
    {
        $(document).on(AppStartEvent.eventName, (event) => {
            if (!event.detail.appName || event.detail.appName != 'shopapp') return;

            if (!Object.hasOwn(event.detail, 'shopId')) return;

            uploadShopList(event.detail.shopId, loadShop);
        });        
    });