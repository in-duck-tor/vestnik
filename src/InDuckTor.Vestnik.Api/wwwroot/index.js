const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5057/api/v1/ws/vestnik/account-events", {
        accessTokenFactory: () => document.getElementById("accessTokenArea").value

    })
    .configureLogging(signalR.LogLevel.Information)
    .build();

// connection.on("method", (data) => {
//     console.log(data);
// });

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

connection.onclose(async () => {
    await start();
});

connection.on("ReceivePleasure", async () => {
    console.log("Pleasure received");
})

let isConnected;

// Start the connection.
start().then(async r => {
    isConnected = true
}, r => {
    console.log(`What the fuck??? ${r}`)
});

async function subscribeSosok() {
    let sosochki = ["small", "medium", "large", "big"];
    await connection.invoke("SubscribeSosok", sosochki);
}

