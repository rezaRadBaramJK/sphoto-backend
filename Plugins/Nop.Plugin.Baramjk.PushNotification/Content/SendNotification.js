var comboAttrTree;

jQuery(document).ready(function ($) {
    comboAttrTree = $('#AttrTree').comboTree({
        isMultiple: true,
        cascadeSelect: true,
        collapse: false,
        selectableLastNode: false
    });
    comboAttrTree.setSource(treeItems);
});

function SendNotif() {
    var data = $(SendNotificationForm).serializeArray().reduce(function (json, {name, value}) {
        json[name] = value;
        return json;
    }, {});

    data.CustomerIds = comboAttrTree.getSelectedIds();

    $.ajax({
        url: '/FrontendApi/PushNotification/send',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            alert("Notification sent successfully.");
        },
        error: function (xhr) {
            var errorMessage = "An error occurred while sending the notification.";
            if (xhr.responseJSON && xhr.responseJSON.Message) {
                errorMessage = xhr.responseJSON.Message;
            }
            alert(errorMessage);
        }
    });
}
