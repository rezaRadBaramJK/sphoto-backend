const actorsMap = new Map()

function SelectEntityPopup(eventId) {

    const features = [
        'width=900',
        'height=600',
        'left=100',
        'top=50',
        'resizable=yes',
        'scrollbars=yes'
    ].join(',');
    let win = window.open("/Admin/PhotoPlatform/Actor/List", null, features);
    win.onload = () => {
        setupSelectionScript(win.document, "Actor", eventId)
    }
}

function setupSelectionScript(document, type, eventId) {

    let styles = `
    tr {
        cursor: pointer;
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
        let result={actorId:id, eventId:${eventId}, type:'${type}',postMessageType:"selectItem"};
        window.opener.postMessage(result, '*');
    })
})
`;
    document.head.appendChild(script)
    const saveButton = document.createElement("div");

    saveButton.classList.add("d-flex");
    saveButton.classList.add("justify-content-end");

    saveButton.innerHTML = `
    <button type="submit" name="save" id="add-actors"  class="btn btn-primary">
        <i class="far fa-save"></i>Save
    </button>
`;
    document.querySelector(".content-header").appendChild(saveButton);
    let addItemsPostMessage = document.createElement("script")
    addItemsPostMessage.innerHTML = `document.getElementById("add-actors").addEventListener("click", event => {
    event.preventDefault();
    let data={postMessageType:"submitItems"};
    window.opener.postMessage(data, '*');
    window.close();
});
`
    document.body.appendChild(addItemsPostMessage);

}


window.addEventListener("message", (e) => {

    if (e.data?.postMessageType === "selectItem") {


        if (actorsMap.has(e.data.actorId)) {
            actorsMap.delete(e.data.actorId)
        } else {
            actorsMap.set(e.data.actorId, e.data.eventId);
        }

    } else if (e.data?.postMessageType === "submitItems") {
        if (actorsMap.size > 0) {
            const data = {
                actorIds: Array.from(actorsMap.keys()),
                eventId: actorsMap.values().next().value
            }


            fetch("/Admin/PhotoPlatform/ActorEvent/AddEventActorEvent", {
                method: 'POST',
                body: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: "include"
            }).then((response) => {
                actorsMap.clear()
                $('#actor-events-grid').DataTable().ajax.reload();
            }).catch((error) => {
                actorsMap.clear()
            })
        }
    }

});
 
