function setTabsSelection(tabsSelector, currentTab, selectedClass) {
    tabsSelector.each(function () {
        let tab = $(this);

        if (tab[0] == currentTab)
            tab.addClass(selectedClass);
        else
            tab.removeClass(selectedClass);
    });
}