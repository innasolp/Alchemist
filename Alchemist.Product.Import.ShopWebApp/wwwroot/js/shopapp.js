// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//import * as alchemist_confirm from "alchemist_common/confirm";

$ready(
    function ()
    {
        //const shopId = $("#Id").length > 0 ? $("#Id").value() : null;

        //document.addEventListener('DOMContentLoaded', function () {
        //    const dynamicElement = document.getElementById('dynamicContent');
        //    if (dynamicElement) {
        //        dynamicElement.addEventListener('mouseover', function () {
        //            console.log('Mouse over dynamic content!');
        //        });
        //    }
        //});
        $(document).on(AppStartEvent.eventName, (event) => {
            if (!event.data.shopId) return;

            uploadShopList(event.data.shopId, loadShop);
        });        
    });