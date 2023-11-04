var WebSocketApi = (function () {
    var locationInput, deviceIdInput;
    var connectButton, disconnectButton, loginButton, updateCoinsButton;
    var consoleOutput;
    var webSocket;

    var isConncted = false;
    var openWebSocket = function () {

        if (isConncted) {
            writeToConsole('[-- User Already Connected --]:');
            return;
        }

        webSocket = new WebSocket(locationInput.value);
        webSocket.onopen = webSocketOnOpen;
        webSocket.onclose = webSocketOnClose;
        webSocket.onerror = webSocketOnError;
        webSocket.onmessage = webSocketOnMessage;
    };

    var closeWebSocket = function () {
        webSocket.close();
    }

    function login() {
        if (!isConncted) {
            writeToConsole('[-- Not Connected --]:');
            return;
        }

        var deviceId = deviceIdInput.value;
        writeToConsole('[-- LOGIN --]: ' + deviceId);
        webSocket.send('login/' + deviceId);
    }

    function updateCoins() {
        if (!isConncted) {
            writeToConsole('[-- Not Connected --]:');
            return;
        }

        var coins = coinsInput.value;

        writeToConsole('[-- UpdateCoins --]: ' + coins);
        webSocket.send('updateCoins/' + coins);
    }

    var webSocketOnOpen = function () {
        isConncted = true;
        writeToConsole('[-- CONNECTION ESTABLISHED --]');
    };

    var webSocketOnClose = function () {
        if (isConncted) {
            isConncted = false;
            writeToConsole('[-- CONNECTION CLOSED --]');
        }
    }

    var webSocketOnError = function (err) {
        console.log(err);
        writeToConsole('[-- ERROR OCCURRED --]');
    }

    var webSocketOnMessage = function (message) {
        writeToConsole('[-- RECEIVED --]: ' + message.data);
    };

    var clearConsole = function () {
        while (consoleOutput.childNodes.length > 0) {
            consoleOutput.removeChild(consoleOutput.lastChild);
        }
    };

    var writeToConsole = function (text) {
        var paragraph = document.createElement('p');
        paragraph.style.wordWrap = 'break-word';
        paragraph.appendChild(document.createTextNode(text));

        consoleOutput.appendChild(paragraph);
    };

    return {
        initialize: function () {
            locationInput = document.getElementById('location');
            deviceIdInput = document.getElementById('deviceId');
            coinsInput = document.getElementById('coins');

            connectButton = document.getElementById('connect');
            disconnectButton = document.getElementById('disconnect');
            loginButton = document.getElementById('login');
            updateCoinsButton = document.getElementById('updateCoins');
            consoleOutput = document.getElementById('console');

            connectButton.addEventListener('click', openWebSocket);
            disconnectButton.addEventListener('click', closeWebSocket);
            loginButton.addEventListener('click', login);
            updateCoinsButton.addEventListener('click', updateCoins);
            document.getElementById('clear').addEventListener('click', clearConsole);
        }
    };
})();

WebSocketApi.initialize();