import * as signalR from "@microsoft/signalr";
import "./css/main.css";

const divMessages: HTMLDivElement = document.querySelector("#divMessages");
const uploadForm: HTMLFormElement = document.querySelector("#batchUploadForm")!;

uploadForm.addEventListener("submit", (event) => {
    event.preventDefault();
    uploadBatch(event.target as HTMLFormElement);
});

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    .build();

connection.on("statusChanged", (id: string, status: string) => {
  const m = document.createElement("div");

  m.innerHTML = `<div class="message-author">${id}</div><div>${status}</div>`;

  divMessages.appendChild(m);
  divMessages.scrollTop = divMessages.scrollHeight;
});

async function uploadBatch(target: HTMLFormElement) {
    var data = new FormData(target);

    const busy = document.querySelector("#lblBusy")!;

    busy.setAttribute("style", "");

    return await fetch(target.action, {
        method: target.method,
        body: data,
    })
    .then(r => r.json())
    .then(r => {
        busy.setAttribute("style", "visibility: hidden;");
        
        r.results.forEach(m => {
            const msg = document.createElement("div");
            msg.attributes["name"] = m.messageId;
            msg.innerHTML = `<div class="message-author"><span>${m.name}</span><span style="float: right">${m.queueDateTime}</span></div>`;

            divMessages.appendChild(msg);
            divMessages.scrollTop = divMessages.scrollHeight;
        });

        connection.start()
            .then(v => {
                connection.send("subscribe", r.batchId);
            });
    })
    .catch(err => console.log(err));
}
