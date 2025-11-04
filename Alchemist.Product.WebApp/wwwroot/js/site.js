// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function init(tab) {
    
    const url = new URL(window.location.href);   
    const apiUrl = `/${tab}Api${url.pathname}`;

    postData(apiUrl, null, (response) => { $('#appDiv').html(response); });    
}
