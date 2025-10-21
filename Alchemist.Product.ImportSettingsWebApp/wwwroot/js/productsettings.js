const categoryUrlModalSettings = new ModalForm(".rootCategoryModal", ".categoryModalBody", '#rootCategoryCloseBtn', null, '#settingsForm', new InputConfirmationSettings('#rootCategoryForm', onCategoryUrlChanged));

function onCategoryUrlChanged(formData, onChanged) {

    var data = getFormDataCopy(formData);

    postFormData('/Import/Settings/Product/CategoryUrl/IsChanged',
        data,
        (result) => onChanged(result)
    );
}

function onSetRootCategory(event) {
    var guid = event.target.hasAttribute("data-guid") ? event.target.getAttribute("data-guid") : null;
    const onShow = (response) => {
        setDivToForm($('#rootCategoryDiv'), $('#rootCategoryFormDiv'), 'rootCategoryForm');
        $("button.save-root-category").on("click", function (event){
            setCategoryUrl('#rootCategoryForm');
            });
    };
    categoryUrlModalSettings.show('/Import/Settings/Product/CategoryUrl', getCategoryUrlData(guid), onShow, null);
}

function getCategoryUrlData(guid) {
    var shopId = $("#ShopId").val();
    var data = { guid: guid, shopId: shopId };
    if (guid != null) {
        const li = $(`li[data-guid='${guid}']`);
        data.item = li.find(".table_cell.item").text();
        data.url = li.find(".table_cell.url").text();
    }
    return data;
}

function setCategoryUrl(categoryUrlForm, onSetCategory = null) {

    var onSuccess = (data) => {

        categoryUrlModalSettings.closeModal(true);
        if (data == null) return;
        onSetCategoryUrlItem(data);
        if (onSetCategory != null)
            onSetCategory(data);
    }

    validateForm($(categoryUrlForm), () => {

        var formData = new FormData($(categoryUrlForm)[0]);

        postFormData(url = "/Import/Settings/Product/CategoryUrl/Set",
            formData = formData,
            onSuccess = onSuccess,
            onError = null);

    }, null);
}

function onSetCategoryUrlItem(data) {
    var li = $(`li[data-guid='${data.guid}']`);
    if (li.length == 0)
        addCategoryUrlItem(data);
    else
        updateCategoryUrlItem(li, data);
}

function updateCategoryUrlItem(li, data) {
    li.find(".table_cell.item").text(data.item);
    li.find(".table_cell.url").text(data.url);
}

function addCategoryUrlItem(data) {
    var li = $('ul.table-ul.rootCategories > .add-btn-ul');
    postJsonData('/Import/Settings/Product/CategoryUrl/Item', JSON.stringify(data), (content) => {
        li.before(content);
        li.prev().find(".edit-root-category").on('click', onSetRootCategory);
    });
}

function initRootCategoriesEvents() {
    $(".add-root-category").on('click', onSetRootCategory);
    $(".edit-root-category").on('click', onSetRootCategory);
}