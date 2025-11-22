function troubleshoot(systemName){
    let postData = {};
    addAntiForgeryToken(postData);

    $.ajax({
        cache: false,
        type: "POST",
        url: "/Admin/BackendApi/Core/Troubleshoot/" + systemName,
        data: postData,
        success: function () {
            alert("Troubleshoot done.");
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("failed:" + errorThrown);
        }
    });
}