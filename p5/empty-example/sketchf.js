let IMAGENES = []
let NUM_IMAGENES;

//Datos resultado del problema
let vertices = [];
let ordenamiento = [];
let NUMERO_VERTICES;
let json_data;

//Control de gráficos
let ESPACIADO = 100;
let TAMANIO_LETRA = 12;
let TAMANIO_VERTICES = 30;
let zoom;

//Control de desplazamiento del canva
let offset_x = 0;
let offset_y = 0;
let esta_arrastrando = false; 
let ultima_posicion_x, ultima_posicion_y;


function preload() {
  json_data = loadJSON('prueba.json');
}

function eliminar_duplicados(matriz) {
  for (let vertice in matriz) {
    let conexiones = matriz[vertice];
    for (let destino in conexiones) {

      //Eliminar conexiones duplicadas
      if (matriz[destino] && matriz[destino][vertice] !== undefined) {
        delete conexiones[destino];
      }
    }
  }
}

function setup() {
  createCanvas(2000, 1500);

  NUMERO_VERTICES = json_data.VERTICES.length;
  zoom = 1;

  NUM_IMAGENES = round((ESPACIADO * NUMERO_VERTICES + 200) / width)
  
  let pos_y = height / 2;
  let indice_cw = -1;
  let max_cw = 0;

  eliminar_duplicados(json_data.MA);

  for (let i = 0; i < NUMERO_VERTICES; i++) {
    let v = json_data.VERTICES[i];
    let pos_x = v.indice_ordenamiento * ESPACIADO;
    
    //Crear los objetos de tipo vértice
    //let u = new Vertice(v.vertice, v.indice_ordenamiento, json_data.MA[v.vertice], v.ancho_corte, 50, 50,pos_x, pos_y);
    let u = new Vertice(v, json_data.MA[v.vertice], TAMANIO_VERTICES, TAMANIO_VERTICES, pos_x + TAMANIO_VERTICES, pos_y);
   
    vertices.push(u);
    /*if (u.cw > max_cw) {
      max_cw = u.cw;
      indice_cw = u.v;
    }*/

    ordenamiento.push(json_data.ORDENAMIENTO[i]);
  }

  console.log(json_data.ORDENAMIENTO)
}

let cargado = false;
function draw() {
  background(255);
  
  if (!cargado) {
    for (let j = 0; j < NUM_IMAGENES; j++){ 
      background(255);
      //Aplicar el desplazamiento
      push();
      translate(-(width * j), -0);
      scale(zoom);
    
      //Dibujar las líneas de conexión
      for (let i = 0; i < NUMERO_VERTICES; i++) {    
        let u = vertices[ordenamiento[i]];
        let ady_u = Object.keys(u.conexiones)
        
        for (let j = 0; j < ady_u.length; j++){
          let idx_v = ady_u[j];
          let v = vertices[idx_v];
          
          u.conectar(v, u.conexiones[idx_v]);      
        }
    
        u.mostrar();
        if (i + 1 == NUMERO_VERTICES) break;
    
        fill(255);    
        //Dibujar las líneas de corte
        let x = ((ESPACIADO * (i + i + 1))) / 2;
        stroke('rgba(100, 100, 100, 0.8)');
        line(x, 100, x, height);
    
        fill(0);
        //Colocar el corte en la línea
        let indice = ordenamiento[i + 1];
        textSize(TAMANIO_LETRA);
        text(`CW${i}(${vertices[indice].cw})`, x, 100);
      }
    
      IMAGENES.push(get(0, 0, width, height));
      pop();
    }    
    cargado = true;
    
  } else {
    push();
    translate(-offset_x, -offset_y);
    scale(zoom);

    let idx = Math.floor(offset_x / (width * zoom));
    idx = constrain(idx, 0, IMAGENES.length - 1);

    // Mostrar la imagen correspondiente    
    image(IMAGENES[idx], width * idx, 0);
    
    if (idx > 1) image(IMAGENES[idx - 1], width * (idx - 1), 0);
    if (idx < IMAGENES.length - 1) image(IMAGENES[idx + 1], width * (idx + 1), 0);
    pop(); 
  }
}

function mousePressed() {
  if (mouseButton === LEFT) {
    esta_arrastrando = true;
    ultima_posicion_x = mouseX;
    ultima_posicion_y = mouseY;
   
  }
}

function mouseReleased() {
  if (mouseButton === LEFT) {
    esta_arrastrando = false;
  }
}

function mouseWheel(event) {
    let zoomAmount = 0.1;
  if (event.delta > 0) {
    zoom -= zoomAmount;
  } else {
    zoom += zoomAmount;
  }

  zoom = constrain(zoom, 0.1, 5);
  return false;
}

function mouseDragged() {
  if (esta_arrastrando) {
    let dx = mouseX - ultima_posicion_x;
    let dy = mouseY - ultima_posicion_y;

    //Mover fondo en dirección opuesta al arrastre
    offset_x -= dx; 
    offset_y -= dy;

    //Guardar la posición x e y del mouse anterior
    ultima_posicion_x = mouseX;
    ultima_posicion_y = mouseY;
  }
}
