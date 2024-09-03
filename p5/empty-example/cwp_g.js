class Vertice {
    constructor(v, conexiones, h, w, x, y) {
        //Datos del vértice
        this.indice = v.indice_ordenamiento;
        this.v = v.vertice;
        this.cw = v.ancho_corte;

        //Vértices adyacentes, con su peso asociado
        this.conexiones = conexiones;

        //Tamaño del vértice
        this.h = h;
        this.w = w;

        //Coordenadas en el canva
        this.x = x;
        this.y = y;

        this.dir_sup = random() > 0.5;

        //Posiciones del vértice
        this.r = createVector(this.x + this.h / 2, this.y             );
        this.t = createVector(this.x             , this.y + this.w / 2);
        this.l = createVector(this.x - this.h / 2, this.y             );
        this.d = createVector(this.x             , this.y - this.w / 2);
    }

    mostrar(){
        fill(255);
        stroke(0);
        ellipse(this.x, this.y, this.h, this.w);
        
        fill(0);
        textAlign(CENTER, CENTER);
        textSize(TAMANIO_LETRA);
        text(this.v, this.x, this.y);
    }

    conectar(otro, peso){
        //Verificar si un vértice está después de otro
        if (this.indice + 1 == otro.indice) {
            line(this.r.x, this.r.y, otro.l.x, otro.l.y);
            fill(0);
            textAlign(CENTER, CENTER);
            textSize(TAMANIO_LETRA);
            text(peso, (this.x + otro.x) / 2, (this.y + otro.y) / 2 - 10);
        } else if(this.indice - 1 == otro.indice) {
            line(this.l.x, this.l.y, otro.r.x, otro.r.y);
            fill(0);
            textAlign(CENTER, CENTER);
            textSize(TAMANIO_LETRA);
            text(peso, (this.x + otro.x) / 2, (this.y + otro.y) / 2 - 10);
        } else {
            let u = this;
            let v = otro;

            //Cálculo de la distancia entre los vértices
            let d = dist(u.x, u.y, v.x, v.y);
            let a = map(d, 0, 10000, 2, 7);
            
            //Ancho y alto del óvalo
            let w = d;
            let h = d / a;

            if (h > NUM_H) NUM_H = h;
            
            //Centro del óvalo a dibujar
            let cx = (u.x + v.x) / 2;
            let cy = (u.y + v.y) / 2;
            
            //Cálculo del ángulo del arco            
            let a1 = atan2(u.y - cy, u.x - cx);
            let a2 = atan2(v.y - cy, v.x - cx);

            noFill();        
            arc(cx, cy, w, h, a1, a2, OPEN);

            //Calcular el punto medio del arco para colocar el peso
            let angulo = (a1 + a2) / 2;
            let offset = h / 2;

            //En caso que el arco se haya dibujado en la parte superior
            //invertir el ángulo
            angulo = (a1 > a2) ? -angulo : angulo;

            let textX = cx + cos(angulo) * offset;
            let textY = cy + sin(angulo) * offset + ((a1 > a2) ? 10 : -10);

            fill(0);
            textAlign(CENTER, CENTER);
            textSize(10);
            text(peso, textX, textY);

            push();
            strokeWeight(1);
            v.mostrar();
            pop();
        }
    }
}