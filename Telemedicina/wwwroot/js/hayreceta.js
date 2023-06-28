function mostrarpdf() {

    let diag = {
        id: ROOM_ID,
    };

    // send receta to backend using post request
    fetch("https://localhost:7051/api/receta/hayrece", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(diag),
    })
        .then((res) => res.json())
        .then((data) => {
            //Tiene informacion
            if (data.length != 0) {
                //console.log(data);
                //Eliminamos lo que ya existia
                var div = document.getElementById('recetabox');
                while (div.firstChild) {
                    div.removeChild(div.firstChild);
                }
                //Creamos el objeto
                for (let i = 0; i < data.length; i++) {
                    let row_content = `
                            <div class="recetaboxreceta">
                                <a href="`+ data[i] + `" class="recetaboxitem">
                                <img class="recetaboxqr" id="codigo`+ i + `" alt="Recetaqr" src="" style="width:100px; height:100px;" />
                                <label for="recetaboxtexto">Receta Generada</label>
                                </a>
                            </div>
                        `;

                    let row = document.createElement("div");
                    row.classList.add("recetaboxitem");
                    row.innerHTML = row_content;

                    let row_container = document.querySelector(".recetabox");
                    row_container.appendChild(row);

                    new QRious({
                        element: document.querySelector("#codigo" + i),
                        value: data[i], // La URL o el texto
                        size: 100,
                        backgroundAlpha: 0, // 0 para fondo transparente
                        foreground: "#000", // Color del QR
                        level: "H", // Puede ser L,M,Q y H (L es el de menor nivel, H el mayor)
                    });
                }

            }
        });

}

setInterval(mostrarpdf, 10000);
