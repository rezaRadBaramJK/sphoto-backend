importScripts("https://www.gstatic.com/firebasejs/8.6.1/firebase-app.js");
importScripts("https://www.gstatic.com/firebasejs/8.6.1/firebase-messaging.js");
importScripts("https://www.gstatic.com/firebasejs/8.6.1/firebase-analytics.js");

function firebaseInitializeApp(config) {

    firebase.initializeApp(config);

    const messaging = firebase.messaging();

    messaging.setBackgroundMessageHandler(function (payload) {
        console.log("sw Received background message ", payload);
        // Customize notification here
        const notificationTitle = "Background Message Title";
        const notificationOptions = {
            body: "Background Message body.",
            icon: "/itwonders-web-logo.png",
        };

        return self.registration.showNotification(
            notificationTitle,
            notificationOptions,
        );
    });
}

fetch('/PushNotification/FirebaseConfig', {
    method: 'get',
    headers: {
        'Content-Type': 'application/json'
    },
}).then(response => response.json())
    .then(data => {
        var config = JSON.parse(data.Data)
        firebaseInitializeApp(config)
    });