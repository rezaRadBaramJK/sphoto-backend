console.log("Common Run");


function RunScriptOnSelectProductOrCategory(document, type) {
    let styles = `tr {
    cursor: pointer;
}
tr a,tr input {
    display: none!important;
}
.content-wrapper{
margin-left:  0px!important;
}

.main-sidebar,.main-header,.float-right{
display: none;
}

tr:hover{
    background-color: #c5f0ff!important;
    }

`;

    let styleSheet = document.createElement("style")
    styleSheet.innerHTML = styles
    document.head.appendChild(styleSheet)

    let script = document.createElement("script")
    script.innerHTML = `document.querySelectorAll('body').forEach(item => {
    item.addEventListener('click', event => {
        let tr=event.target.closest('tr');
        if(tr==null)
        return;
        let editLink=tr.querySelector('[href^="Edit"]');
        let id=parseInt( editLink.href.split('/').slice(-1)[0]);
        let result={id:id,type:'${type}',SelectProductOrCategory:true};
        window.opener.postMessage(result, '*');
        console.log(id)
    })
})
        console.log('run')
`;

    document.head.appendChild(script)
}

window.OnSelectProductOrCategory = (data, e) => {
    console.log(data)
}

window.addEventListener("message", function (e) {
    if (e.data.SelectProductOrCategory === true) {
        window.OnSelectProductOrCategory(e.data, e)
        e.source.close()
    }
    console.log(e)
});


function selectProductPopup() {
    let win = window.open('/Admin/Product/List', null, 'popup');
    win.onload = () => {
        RunScriptOnSelectProductOrCategory(win.document, 'Product')
    }
}

function selectCategoryPopup() {
    let win = window.open('/Admin/Category/List', null, 'popup');
    win.onload = () => {
        RunScriptOnSelectProductOrCategory(win.document, 'Category')
    }
}

