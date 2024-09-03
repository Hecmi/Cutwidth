//Variables para precargar las imagenes del 
//problema
let IMAGENES = {}
let NUM_IMAGENES;
let NUM_H;
let CARGADO = false;

//Datos resultado del problema
let vertices = [];
let ordenamiento = [];
let NUMERO_VERTICES;
let json_data;

//Control de gráficos
let ESPACIADO = 100;
let TAMANIO_LETRA = 12;
let TAMANIO_VERTICES = 30;
let ZOOM;

//Control de desplazamiento del canva
let OFFSET_VERTICES_X = 200;
let offset_x = 0;
let offset_y = 0;
let esta_arrastrando = false; 
let ultima_posicion_x, ultima_posicion_y;


function preload() {
  json_data = loadJSON('resolucion2.json');
  //console.log(json_data)
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
  createCanvas(2000, 1000);

  NUMERO_VERTICES = json_data.VERTICES.length;
  ZOOM = 1;
  NUM_H = 0;

  NUM_IMAGENES = max(round((ESPACIADO * NUMERO_VERTICES) / width), 1)
  let pos_y = height / 2;  

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
  preprocesar();
  console.log("fin de caraga")
}

function preprocesar() {
  for (let j = 0; j < NUM_IMAGENES; j++){
    IMAGENES[j] = {}
    for (let k = -4; k <= 4; k++){
      background(255);
      //Aplicar el desplazamiento
      push();
      translate(-(width * j), -(height * k));
      scale(ZOOM);
    
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
    
        //Dibujar las líneas de corte en el centro de los dos vértices
        fill(255);
        let x = ((ESPACIADO * (2 * i + 1))) / 2 + TAMANIO_VERTICES;
        stroke('rgba(100, 100, 100, 0.8)');
        //line(x, 100, x + OFFSET_VERTICES_X, height);
        line(x, 100, x, height);
    
        fill(0);
        //Colocar el corte en la línea
        let indice = ordenamiento[i + 1];
        textSize(TAMANIO_LETRA);
        text(`CW${i}(${vertices[indice].cw})`, x, 100);
      }
    
      IMAGENES[j][k] = get(0, 0, width, height);
      //saveCanvas('parte', 'png'); 
      pop();
    }
  }    
  cargado = true;  
}

function draw() {
  background(255);

  if (cargado) {   
    push();
    translate(-offset_x, -offset_y);
    scale(ZOOM);

    // Cálculo de índices
    let idx_x = Math.floor(offset_x / (width * ZOOM));
    let idx_y = Math.floor(offset_y / (height * ZOOM));

    // Ajustar índices para que estén dentro del rango permitido
    idx_x = constrain(idx_x, 0, Object.keys(IMAGENES).length - 1);
    idx_y = constrain(idx_y, -4, 4);

    let posiciones = [
      //Imagenes actuales
      {x: idx_x     , y: idx_y    },
      {x: idx_x     , y: idx_y + 1},
      {x: idx_x     , y: idx_y - 1},

      //Imagenes de la derecha
      {x: idx_x + 1 , y: idx_y    },
      {x: idx_x + 1 , y: idx_y + 1},
      {x: idx_x + 1 , y: idx_y - 1},

      //Imagenes de la izquierda
      {x: idx_x - 1 , y: idx_y    },
      {x: idx_x - 1 , y: idx_y + 1},
      {x: idx_x - 1 , y: idx_y - 1}
    ];

    //image(IMAGENES[0][0], width * x, height * y);
    //Mostrar las imágenes en las posiciones definidas
    for (let pos of posiciones) {
      let x = pos.x;
      let y = pos.y;

      //Verificar que la imagen existe antes de dibujarla
      if (IMAGENES[x] && IMAGENES[x][y]) {
        image(IMAGENES[x][y], width * x, height * y);
      }
    }

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
    ZOOM -= zoomAmount;
  } else {
    ZOOM += zoomAmount;
  }

  ZOOM = constrain(ZOOM, 1, 2);
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
