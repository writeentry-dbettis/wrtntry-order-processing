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

    busy.innerHTML = "Uploading...";

    return await fetch(target.action, {
        method: target.method,
        body: data,
    })
    .then(r => r.json())
    .then(async r => {
        connection.start()
            .then(v => {
                connection.send("subscribe", r.batchId);
            });
        
        const entryCount = r.results.length;

        busy.innerHTML = `Uploaded ${entryCount} entries. Queueing...`;

        var queueUrl = `/api/batch/${r.batchId}/queue`;

        return await fetch(queueUrl, {
            method: "POST",
            body: JSON.stringify(r.results)
        });
    })
    .then(q => q.json())
    .then(q => {
        busy.innerHTML = `Queued ${q} entries.`;
    })
    .catch(err => console.log(err));
}
