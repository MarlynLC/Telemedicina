let btnGuardarDiag = document.getElementById("btnSaveDiagnostico");
btnGuardarDiag.addEventListener("click", exportarDiagnostico);

function exportarDiagnostico() {

    let diagtext = document.getElementById("txtdiagnostico").value;
    let temp = document.getElementById("temp").textContent;
    let presion = document.getElementById("presion").textContent;
    let peso = document.getElementById("peso").textContent;
    let ritmo = document.getElementById("ritmo").textContent;

    let diag = {
        diagnostico: diagtext,
        id: ROOM_ID,
        temperatura: temp,
        presionarterial: presion,
        pesocorporal: peso,
        ritmocardiaco: ritmo
    };

    // send receta to backend using post request
    fetch("https://localhost:7051/api/receta/diag", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(diag),
    })
        .then((res) => res.json())
        .then((data) => {
            //console.log(data);
            var p = document.getElementById('txtadig');
            p.style.display = 'block';
            p.innerHTML = 'Diagnostico Guardado ✅';
        });

    //console.log(diag);
}