const remote = "http://89.19.214.8/api/v1/ws/vestnik/account-events"
const local = "http://localhost:5057/api/v1/ws/vestnik/account-events"

const connection = new signalR.HubConnectionBuilder()
    .withUrl(local, {
        accessTokenFactory: () => {
            // document.getElementById("accessTokenArea").value
            return "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3MTIwNzc0MjMsImV4cCI6MTcxNTY3NzQyMywiaXNzIjoiaW4tZHVjay10b3IiLCJjbGllbnRfaWQiOiJhbmd1bGFyX3NwYSIsInN1YiI6IjAiLCJhdXRoX3RpbWUiOjE3MTIwNzc0MjMsImlkcCI6ImxvY2FsIiwibG9naW4iOiJpbi1kdWNrLXRvciIsImFjY291bnRfdHlwZSI6InN5c3RlbSIsImp0aSI6IjYxQjRERUE1NzA1MDQ0NjkxRTVGMjVEQUI5NTg4ODlCIiwic2lkIjoiQkFCNzQxQUIyMjZGMUJEMzZFRTA0MkYwRjY4RTFEMTIiLCJpYXQiOjE3MTIwNzc0MjMsInNjb3BlIjpbIm9wZW5pZCIsInByb2ZpbGUiLCJlbWFpbCJdLCJhbXIiOlsicHdkIl19.LsWmh_muuzuHA7fUI_7rgVCfDcC4pgrczV_LPL1U29rbTz2A6TCykVv70_gD73kas6a3bkPdu5uX0Q3oWpC4MjdYIODXOY8Er6TWULBH7ny38huTKJQy9O_VTl-A4wcG-KcCLilG_l4UDjchHV1__Ku0eD-ZtSUXb3fv7JjEfTGHz7oXPbmEEDzoVef3qd_jCqCvJ9O2qEFF5r8nIC-Q9wM5mpMDtQcnxtEi8iTDSjxKno-BIpLsY8TnYOOQ1Ci2WnEbU0uRmPwufDR2lZg8WKkvh1Ydh55DD_I82nI9a0KlPLExKTExlB0wUIvOE6Uhokq5-dKjmX_LJwXQhB0wyA"
        }
        // headers: {
        //     "Authorization": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3MTIwNzc0MjMsImV4cCI6MTcxNTY3NzQyMywiaXNzIjoiaW4tZHVjay10b3IiLCJjbGllbnRfaWQiOiJhbmd1bGFyX3NwYSIsInN1YiI6IjAiLCJhdXRoX3RpbWUiOjE3MTIwNzc0MjMsImlkcCI6ImxvY2FsIiwibG9naW4iOiJpbi1kdWNrLXRvciIsImFjY291bnRfdHlwZSI6InN5c3RlbSIsImp0aSI6IjYxQjRERUE1NzA1MDQ0NjkxRTVGMjVEQUI5NTg4ODlCIiwic2lkIjoiQkFCNzQxQUIyMjZGMUJEMzZFRTA0MkYwRjY4RTFEMTIiLCJpYXQiOjE3MTIwNzc0MjMsInNjb3BlIjpbIm9wZW5pZCIsInByb2ZpbGUiLCJlbWFpbCJdLCJhbXIiOlsicHdkIl19.LsWmh_muuzuHA7fUI_7rgVCfDcC4pgrczV_LPL1U29rbTz2A6TCykVv70_gD73kas6a3bkPdu5uX0Q3oWpC4MjdYIODXOY8Er6TWULBH7ny38huTKJQy9O_VTl-A4wcG-KcCLilG_l4UDjchHV1__Ku0eD-ZtSUXb3fv7JjEfTGHz7oXPbmEEDzoVef3qd_jCqCvJ9O2qEFF5r8nIC-Q9wM5mpMDtQcnxtEi8iTDSjxKno-BIpLsY8TnYOOQ1Ci2WnEbU0uRmPwufDR2lZg8WKkvh1Ydh55DD_I82nI9a0KlPLExKTExlB0wUIvOE6Uhokq5-dKjmX_LJwXQhB0wyA"
        // }
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

connection.on("TransactionCreated", async (event) => {
    console.log(`Transaction Created: ${event}`);
});

connection.on("TransactionUpdated", async (event) => {
    console.log(`Transaction Updated: ${event}`);
});

connection.on("Hueta", async (event) => {
    console.log(`Hueta: ${JSON.stringify(event)}`);
});

let isConnected;

// Start the connection.
start().then(async r => {
    isConnected = true
    // await subscribeToMyAccounts()
}, r => {
    console.log(`What the fuck??? ${r}`)
});

async function subscribeToMyAccounts() {
    await connection.invoke("SubscribeToMyAccounts");
}

