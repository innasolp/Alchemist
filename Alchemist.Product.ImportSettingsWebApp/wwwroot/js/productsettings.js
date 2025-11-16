const showRootCategoryModal = new ShowModal(".root-category-modal", "#settingsForm");

async function isCategoryUrlChanged(formData) {
    var formDataCopy = getFormDataCopy(formData);
    const data = Object.fromEntries(formDataCopy.entries());  
    const shopId = $("#ShopId").val();    
    return await postJsonDataAsync(`/ImportSettingsAction/Product/${shopId}/CategoryUrl/IsChanged`, JSON.stringify(data));
}

async function tryRootCategoryModalClose() {

    const isChanged = await isCategoryUrlChanged(new FormData($("#rootCategoryForm")[0]));

    if (!isChanged) return true;

    return await confirmAsync('Reseting', 'Input values will be reset. Are you sure?');
}


function onSetRootCategory(event) {
    var guid = event.target.hasAttribute("data-guid") ? event.target.getAttribute("data-guid") : null;
    const onShow = () => {
        setDivToForm($('#rootCategoryDiv'), $('#rootCategoryFormDiv'), 'rootCategoryForm');
        $("button.save-root-category").on("click", function (event){
            setCategoryUrl('#rootCategoryForm');
            });
    };
    showRootCategoryModal.show('/ImportSettingsAction/Product/CategoryUrl', getCategoryUrlData(guid), onShow, tryRootCategoryModalClose);
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

        showRootCategoryModal.closeModal();
        if (data == null) return;
        onSetCategoryUrlItem(data);
        if (onSetCategory != null)
            onSetCategory(data);
    }

    validateForm($(categoryUrlForm), () => {

        var formData = new FormData($(categoryUrlForm)[0]);
        const shopId = $("#ShopId").val();
        postFormData(url = `/ImportSettingsAction/Product/${shopId}/CategoryUrl/Set`,
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
    postJsonData('/ImportSettingsAction/Product/CategoryUrl/Item', JSON.stringify(data), (content) => {
        li.before(content);
        li.prev().find(".edit-root-category").on('click', onSetRootCategory);
    });
}

function initRootCategoriesEvents() {
    $(".add-root-category").on('click', onSetRootCategory);
    $(".edit-root-category").on('click', onSetRootCategory);
}