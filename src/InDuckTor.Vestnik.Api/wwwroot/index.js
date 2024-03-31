const connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/v1/ws/vestnik/account-events", {
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

// Start the connection.
start();

