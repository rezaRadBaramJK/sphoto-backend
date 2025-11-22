function registerServiceWorker(callback) {
    let isActive = false;
    let registration = null;

    navigator.serviceWorker.getRegistrations().then(registrations => {
        registrations.forEach((w) => {
            if (w.active.scriptURL.indexOf('firebase-messaging-sw') > -1) {
                isActive = w.active.state === "activated";
                registration = w;
            }
        });
    });

    if (isActive) {
        callback(registration);
    } else {
        let callBack = callback;
        navigator.serviceWorker
            .register('/Plugins/Baramjk.PushNotification/Content/firebase-messaging-sw.js')
            .then((registration) => {
                callBack(registration)
            })
    }
}

function firebaseInitializeApp(registration, callBack) {
    let firebaseConfig = JSON.parse(firebaseConfigHidden.value)
    firebase.initializeApp(firebaseConfig);
    const messaging = firebase.messaging();
    messaging.useServiceWorker(registration);

    messaging.requestPermission()
        .then(function () {
            console.log('Notification permission granted.');
            callBack(messaging);
        })
        .catch(function (err) {
            console.log(err);
        });
}

function sendToken(token) {
    var data = {
        Device: navigator.userAgent,
        Token: token,
        Platform: "Web"
    }

    fetch('/PushNotification/Token', {
        method: 'post',
        body: JSON.stringify(data),
        headers: {
            'Content-Type': 'application/json'
        },
    }).then(r => {
        localStorage.setItem("fcmToken", token)
        localStorage.setItem("sendTokenTime", Date.now())
        localStorage.setItem("firebaseUserId", firebaseUserId.value);
        console.log(token)
    });
}

function init() {
    let isRegisteredUser = IsRegistered.value === 'True';
    let token = localStorage.getItem("fcmToken");
    let storedUserId = localStorage.getItem("firebaseUserId");
    let currentUserId = firebaseUserId.value;

    if (isRegisteredUser === false && (token === null || token === ""))
        return;

    registerServiceWorker((reg) => {
        firebaseInitializeApp(reg, (msg) => {

            if (isRegisteredUser === false && (token !== null && token !== "")) {
                msg.deleteToken().then(function () {
                    localStorage.setItem("fcmToken", "");
                    console.log("deleteToken");
                });
                return;
            }

            if (storedUserId != null && storedUserId !== currentUserId) {
                console.log("user changed");
                msg.deleteToken().then(function () {
                    msg.getToken().then(function (token) {
                        sendToken(token);
                    });
                });
                return;
            }

            if (Date.now() - Number(localStorage.getItem("sendTokenTime")) < 2000) {
                console.log("Not need to send again")
                return;
            }

            msg.getToken().then(function (token) {
                sendToken(token);
            });
        })
    });
}

init();
//todo: new user
 