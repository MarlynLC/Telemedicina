let estructura_medicamentos = {
    medicamentos: [
        {
            nombre_medicamento: "Paracetamol",
            dosis: ["1 tableta", "2 tabletas", "3 tabletas"],
            frecuencia: [
                "Cada 8 horas",
                "Cada 6 horas",
                "Cada 4 horas",
                "Cada 12 horas",
            ],
        },
        {
            nombre_medicamento: "Ibuprofeno",
            dosis: ["1 tableta", "2 tabletas", "3 tabletas"],
            frecuencia: [
                "Cada 8 horas",
                "Cada 6 horas",
                "Cada 4 horas",
                "Cada 12 horas",
            ],
        },
        {
            nombre_medicamento: "Amoxicilina",
            dosis: ["1 tableta", "2 tabletas", "3 tabletas"],
            frecuencia: [
                "Cada 8 horas",
                "Cada 6 horas",
                "Cada 4 horas",
                "Cada 12 horas",
            ],
        },
        {
            nombre_medicamento: "Diclofenaco",
            dosis: ["1 tableta", "2 tabletas", "3 tabletas"],
            frecuencia: [
                "Cada 8 horas",
                "Cada 6 horas",
                "Cada 4 horas",
                "Cada 12 horas",
            ],
        },
        {
            nombre_medicamento: "Omeprazol",
            dosis: ["1 tableta", "2 tabletas", "3 tabletas"],
            frecuencia: [
                "Cada 8 horas",
                "Cada 6 horas",
                "Cada 4 horas",
                "Cada 12 horas",
            ],
        },
    ],
};

let btnCrearReceta = document.getElementById("btn-CrearReceta");
btnCrearReceta.addEventListener("click", mostarModal);

let btnAgregar = document.getElementById("btnAddElement");
btnAgregar.addEventListener("click", addRow);

let btnGuardar = document.getElementById("btnSaveReceta");
btnGuardar.addEventListener("click", exportarReceta);

addRow();

function mostarModal() {
    let modal = document.getElementById("modal");
    modal.style.display = "flex";
}

function ocultarModal() {
    let modal = document.getElementById("modal");
    modal.style.display = "none";
}

function addRow() {
    let row_content = `
                    <div class="modal_element_container">
                        <label for="nombre_medicamento">Medicamento</label>
                        <input type="text" name="nombre_medicamento" oninput="suggestMedicamento(event)"/>
                    </div>
                    <div class="modal_element_container">
                        <label for="dosis">Dosis</label>
                        <input type="text" name="dosis" />
                    </div>
                    <div class="modal_element_container">
                        <label for="frecuencia">Frecuencia</label>
                        <input type="text" name="frecuencia" />
                    </div>
                    <button class="remove_row_button" onclick="eliminarFila(event)">X</button>
                `;

    let row = document.createElement("div");
    row.classList.add("modal_row");
    row.innerHTML = row_content;

    let row_container = document.querySelector(".modal_row_container");
    row_container.appendChild(row);
}

function eliminarFila(e) {
    // get parent target
    let objetivo = e.target.parentNode;
    objetivo.remove()
}

function suggestMedicamento(e) {
    let valor = e.target.value;
    let medicamentos = estructura_medicamentos.medicamentos;
    let medicamentos_sugeridos = [];
    medicamentos.forEach((medicamento) => {
        // includes but add to lower
        if (
            medicamento.nombre_medicamento
                .toLowerCase()
                .includes(valor.toLowerCase())
        ) {
            medicamentos_sugeridos.push(medicamento);
        }
    });

    // Add elements to dom
    document.getElementById("sugerencias")?.remove();
    let sugerencias = document.createElement("div");
    sugerencias.id = "sugerencias";

    medicamentos_sugeridos.forEach((medicamento) => {
        let sugerencia = document.createElement("div");

        sugerencia.innerHTML = medicamento.nombre_medicamento;
        sugerencia.addEventListener("click", () => {
            e.target.value = medicamento.nombre_medicamento;
            // find target brother
            let dosis = e.target.parentNode.parentNode.querySelector(
                'input[name="dosis"]'
            );
            let frecuencia =
                e.target.parentNode.parentNode.querySelector(
                    'input[name="frecuencia"]'
                );

            dosis.value = medicamento.dosis[0];
            frecuencia.value = medicamento.frecuencia[0];

            sugerencias.remove();
        });
        sugerencias.appendChild(sugerencia);
    });

    e.target.parentNode.appendChild(sugerencias);
}

function exportarReceta() {
    let medicamentos = document.querySelectorAll(".modal_row");
    let prescripcion = document.getElementById("txtprescripcion").value;
    let receta = {
        recetas: [],
        id: ROOM_ID,
        press: prescripcion
    };
    medicamentos.forEach((medicamento) => {
        let nombre_medicamento = medicamento.querySelector(
            'input[name="nombre_medicamento"]'
        ).value;
        let dosis = medicamento.querySelector(
            'input[name="dosis"]'
        ).value;
        let frecuencia = medicamento.querySelector(
            'input[name="frecuencia"]'
        ).value;

        receta.recetas.push({
            nombre_medicamento,
            dosis,
            frecuencia,
        });
    });

    // send receta to backend using post request
    fetch("https://localhost:7051/api/Receta/", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(receta),
    })
        .then((res) => res.json())
        .then((data) => {
            //console.log(data);
            var p = document.getElementById('txtreceta');
            p.style.display = 'block';
            p.innerHTML = 'Receta Generada ✅';
        });
    //Ocultamos la receta
    ocultarModal()
}