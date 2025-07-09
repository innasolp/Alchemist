function showRootCategory(data) {

    $(".categoryModalBody").on('load', function (event) {
        console.log(event);
        console.trace(event);
    });

    $('.rootCategoryModal').on("show.bs.modal", function () {
        if ($('#rootCategoryForm').length == 0)
            setDivToForm($('#rootCategoryDiv'), $('#rootCategoryFormDiv'), 'rootCategoryForm');
    });

    if (data.hasOwnProperty('guid'))
        showItemModal($(".rootCategoryModal"), $(".categoryModalBody"), '/ShopSettings/RootCategory/Edit', { data: JSON.stringify(data) });
    else
        showItemModal($(".rootCategoryModal"), $(".categoryModalBody"), '/ShopSettings/RootCategory/New', data);
}
function setRootCategoryLi(rootCategory, ulRootCategories) {

    var items = ulRootCategories.children('.rootcategory_li');
    var item = items.filter(function (index) {
        return $(this).children("div").children("input[class='guid']").val() == rootCategory.guid;
    });

    if (item.length > 0) {
        item.find('.item').text(rootCategory.item);
        item.find('.url').text(rootCategory.url);
        item.find('.guid').val(rootCategory.guid);
    }
    else {

        if (items.length == 0) items = ulRootCategories.children('.header');
        if (items.length == 0) return;

        var li = $("<li>", { "class": "table-ul rootcategory_li" });
        var divFlex = $("<div>", { "class": "flex rootCategoryRow" });
        var divItem = $("<div>", { "class": "table_cell item" }).text(rootCategory.item);
        var divUrl = $("<div>", { "class": "table_cell url" }).text(rootCategory.url);
        var hiddenGuid = $("<input>", { "type": "hidden", "class": "guid" }).val(rootCategory.guid);
        var divEdit = $("<div>");
        var btnEdit = $("<i>", { "class": "fa fa-edit editRootCategory", "style": "font-size:18px" })
            .on("click", function () { showRootCategory(rootCategory); });
        divEdit.append(btnEdit);
        divFlex.append(divItem).append(divUrl).append(hiddenGuid).append(divEdit);
        li.append(divFlex);
        items.last().after(li);
    }
}

function saveRootCategory(formSelector, divModal, categoriesUL) {
    var data = getFormData(formSelector);
    save(formSelector,
        '/ShopSettings/RootCategory/Set',
        { data: JSON.stringify(data) },
        null,
        (data) => {
            if (data == null) return;
            divModal.modal('hide');
            setRootCategoryLi(data, categoriesUL);
        });
}