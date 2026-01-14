# PaintRobot 🖌️🤖
![License](https://img.shields.io/badge/License-MIT-green.svg)
![VB.NET](https://img.shields.io/badge/VB.NET-blue.svg)
![WinForms](https://img.shields.io/badge/WinForms-.NET-lightblue.svg)
![.NET Framework](https://img.shields.io/badge/.NET_Framework-4.8.1-purple.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-informational.svg)

**PaintRobot** è una applicazione **WinForms VB.NET (.NET Framework 4.8.1)** che esegue comandi grafici tramite script, ispirandosi allo stile di AutoCAD ma con un approccio completamente automatico.

Non è un CAD interattivo: PaintRobot disegna **solo** ciò che viene descritto in uno script, garantendo risultati sempre identici e riproducibili.

![PaintRobot Screenshot](Screenshot.png)

![PaintRobot Screenshot2](ScreenShot2.png)

---

## 📌 Caratteristiche principali

- Rendering grafico **timer-driven**
- Esecuzione rapida di **migliaia di comandi grafici**
- Esecuzione di comandi STEP/CONTINUE
- Esecuzione di comandi da History Commands
- Linguaggio proprietario **PaintRobotScript**
- Nessuna interferenza dell’utente durante il disegno
- Output coerente su qualsiasi sistema Windows
- Ideale per disegno automatico, generativo o standardizzato

---

## ❓ Cos’è PaintRobot

PaintRobot **non è AutoCAD**.  
È un motore grafico che interpreta uno script e disegna automaticamente forme, pattern, testi e immagini.

Attualmente **non è possibile modificare il disegno con mouse o tastiera**: tutto passa dallo script.

---

## ⚙️ Come funziona

PaintRobot utilizza una tecnologia chiamata **Timer-Driven Rendering**:

- I comandi vengono eseguiti a blocchi
- Un singolo ciclo di rendering
- L’esecuzione può solo essere avviata o fermata
- Massima velocità e coerenza del risultato

---

## 📋 Esempi dei Comandi

Alcuni Esempi dei comandi grafici presenti in questa versione:

```text
#Un comando alla volta
STEP;1
LINEA;7,10;96,92;Blu;2
RETT;50,50;200,150;Nero;VUOTO;2
CERCHIO;200,200;50;Rosso;PIENO;3
TRIANG;72,50;440,358;17,340;Rosso;VUOTO;1
ADDLIVELLO;Uno
PULISCI;Bianco
ARCO;50,50;150,150;Rosso;PIENO;2;0;180
ARCO;200,50;300,150;Blu;VUOTO;3;90;270
SCACCHI;100,10;400,310;Blu;Rosso
TESTO;100,200;Ciao mondo;Rosso;20;Arial;Normal
POLIGONO;10,10;100,30;80,120;30,90;Verde;PIENO
GRIGLIA;20;Grigio
TRASLA;50;100
SALVA;C:\Temp\foto.jpg;JPG
ADDLIVELLO;Due
APPUNTI
COPIA;C:\Immagini\foto.png
INCOLLA;50,100
RIDIMENSIONA;Appunti;400,400
SPLINE;29,15;53,500;1000,183;Giallo;1
SPLINE2;10,10;50,80;100,50;150,120;Blu;2;0.7
SPIRALE;400,300;10;150;5;Blu;2;Oraria
SINUSOIDE;600,200;700,200;50;25;Blu;2
CROCE;50,50;100,100;Arancio;3
BEZIER;800,320;850,340;900,310;950,210;VIOLA;2
TEXTURE;Texture01;C:\TEXTURE\textures_autocad_45635.gif
DRAWTEXTURE;Texture01;950,400
PATTERN;PUNTI;DOT;1000;10;Nero;1
FILLPATTERN;PUNTI;1000,100;400,300
INIZIO
INIZIOMATH;20;10;Blu
INIZIOCAD;10;Lime
```

## 👤 A chi è utile

PaintRobot è utile se ti serve:

- Un **CAD automatico**
- Disegni sempre identici e standardizzati
- Nessuna variabilità dovuta all’operatore
- Generazione grafica tramite script

---

## 🛠️ Modifica del codice

Il progetto è **open source**.  
Puoi modificare liberamente il codice sorgente secondo i termini della licenza.

---

## 📜 Licenza

**MIT License / Public**  
Usalo, modificalo e ridistribuiscilo liberamente.

---


## 📜 Sintassi Comandi Linguaggio PaintRobotScript

Ecco la sintassi di tutti i comandi disponibili:

```text
LINEA       ; LINEA;x1,y1;x2,y2;Colore;Spessore
CERCHIO     ; CERCHIO;x,y;Raggio;Colore;Spessore
RETT        ; RETT;x1,y1;x2,y2;Tipo;Colore;Spessore
TRIANG      ; TRIANG;x1,y1;x2,y2;x3,y3;Colore;Tipo;Spessore
PULISCI     ; PULISCI;Colore
ARCO        ; ARCO;x1,y1;x2,y2;Colore;Tipo;Spessore;AngoloStart;AngoloSweep
SCACCHI     ; SCACCHI;x1,y1;x2,y2;Colore;Colore2
TESTO       ; TESTO;x1,y1;x2,y2;Testo;Colore;Dimensione;Font;Stile
POLIGONO    ; POLIGONO;xN,yN;Colore;Tipo
GRIGLIA     ; GRIGLIA;Lato;Colore
TRASLA      ; TRASLA;x1,y1
SALVA       ; SALVA;Percorso;Formato(PNG,BMP,JPG)
INVERTI     ; INVERTI;Direzione;-Percentuale
RUOTA       ; RUOTA;Gradi;-Percentuale
APPUNTI     ; APPUNTI
COPIA       ; COPIA;Percorso
INCOLLA     ; INCOLLA;x1,y1
RIDIMENSIONA; RIDIMENSIONA;Appunti;Larghezza,Altezza
SPLINE      ; SPLINE;xN,yN;Colore;Spessore
SPLINE2     ; SPLINE2;xN,yN;Colore;Spessore;Tensione
#           ; #Commento
CROCE       ; CROCE;x1,y1;x2,y2;Colore;Spessore
BEZIER      ; BEZIER;x1,y1;x2,y2;x3,y3;x4,y4;Colore;Spessore
TEXTURE     ; TEXTURE;Nome;Percorso
DRAWTEXTURE ; DRAWTEXTURE;Nome;x,y
PATTERN     ; PATTERN;Nome;LINEE;Angolo;Spaziatura;Colore;Spessore
FILLPATTERN ; FILLPATTERN;Nome;x1,y1;x2,y2
INIZIO      ; INIZIO
INIZIOCAD   ; INIZIOCAD;Passo;Colore
INIZIOMATH  ; INIZIOMATH;ScalaX;ScalaY;ColoreScale
ADDLIVELLO  ; ADDLIVELLO;NomeLivello
DELLIVELLO  ; DELLIVELLO;NomeLivello
RENLIVELLO  ; RENLIVELLO;NomeLivello;NuovoNomeLivello
STEP        ; STEP;Numero
SPIRALE     ; SPIRALE;CentroX,CentroY;RaggioIniziale;RaggioFinale;Giri;Colore;Spessore;Direzione
SINUSOIDE   ; SINUSOIDE;StartX,StartY;EndX,EndY;Ampiezza;Frequenza;Colore;Spessore
```

## ⬇️ Download

Puoi scaricare il codice sorgente direttamente da GitHub:

- **Code → Download ZIP**
- oppure clona il repository:
- git clone https://github.com/Nhor2/PaintRobot.git

## 🏷️ Versioni

- **v2.0** – Prima release pubblica (codice sorgente)
 
>>>>>>> 8c2d1e810aa72bf446efdd9937d874e6edf7013c
