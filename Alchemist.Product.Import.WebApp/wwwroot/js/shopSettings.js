function ShowItemModal() {
    var url = "/Home/ServiceSettings";
    $("#modalBodyDiv").load(url, function (data) {
        $("#divModal").modal("show");
    })
}

function save() {
    var objdata =
    {
        vender_Name: $('#ServiceName').val()
    };
    $.validator.unobtrusive.parse($("#modalForm"));
    if ($("#modalForm").valid()) {
        $.ajax({
            type: 'POST',
            url: '/Home/UpdateSettings',
            data: objdata,
            dataType: 'json',
            dataType: 'json',
            success: function () {
                console.log('saved');
            },
            error: function () {
                console.error("Not Saved");
            }
        });
    }
}
