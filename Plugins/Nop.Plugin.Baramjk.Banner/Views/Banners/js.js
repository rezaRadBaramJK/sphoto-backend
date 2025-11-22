
window.OnSelectProductOrCategory = (data, e) => {
    $('#Link').val(e.data.type + ':' + e.data.id);
    if(data.type === 'Category' || data.type === 'Product' || data.type === 'Manufacturer' || data.type === 'Vendor') {
        $('#EntityId').val(data.id)
        $('#EntityName').val(data.type)
    }
}

function selectManufacturesPopup() {
    let win = window.open('/Admin/Manufacturer/List', null, 'popup');
    win.onload = () => {
        RunScriptOnSelectProductOrCategory(win.document, 'Manufacturer')
    }
}

function selectDiscountsPopup() {
    let win = window.open('/Admin/Discount/List', null, 'popup');
    win.onload = () => {
        RunScriptOnSelectProductOrCategory(win.document, 'Discount')
    }
}

function selectVendorsPopup() {
    let win = window.open('/Admin/Vendor/List', null, 'popup');
    win.onload = () => {
        RunScriptOnSelectProductOrCategory(win.document, 'Vendor')
    }
}