Public Class Form2

	'Help di PaintRobot

	Private help As String = "HELP di PaintRobot v. 2.0 (Licenza MIT/Public)


1 - Cosa é PaintRobot?
PaintRobot non è AutoCAD. PaintRobot è un programma che esegue dei comandi grafici in stile AutoCAD ma senza la possibilità, attualmente, di intervenire sul disegno in modo classico col mouse o tastiera. PaintRobot disegna i comandi eseguiti da uno script nel linguaggio PaintRobotScript che è ancora in evoluzione.

2 - Come funziona PaintRobot?
PaintRobot utilizza una tecnologia definita come Timer-Driven Rendering, i comandi di disegno vengono eseguiti a gruppi in un ciclo unico in cui non è possibile intervenire direttamente se non per fermarlo. E’ in grado di eseguire migliaia di migliaia di comandi grafici in poco tempo.

3 - A chi può servire PaintRobot?
PaintRobot eseguendo uno script è assimilabile ad un CAD automatico che disegnerà sempre allo stesso modo, indipendentemente dal sistema Windows o da chi lo sta usando. La mancanza di interferenza utente garantisce lo standard.

4 - Posso modificare PaintRobot?
Si, se modifichi il codice sorgente di PaintRobot, e puoi farlo a tuo piacere.


5 - Elenco dei Comandi
#, #Commento,
LINEA, LINEA;x1,y1;x2,y2;Colore;Spessore,
CERCHIO, CERCHIO;x,y;Raggio;Colore;Spessore,
RETT, RETT;x1,y1;x2,y2;Tipo;Colore;Spessore,
TRIANG, TRIANG;x1,y1;x2,y2;x3,y3;Colore;Tipo;Spessore,
PULISCI, PULISCI;Colore,
ARCO, ARCO;x1,y1;x2,y2;Colore;Tipo;Spessore;AngoloStart;AngoloSweep,
SCACCHI, SCACCHI;x1,y1;x2,y2;Colore;Colore2,
TESTO, TESTO;x1,y1;x2,y2;Testo;Colore;Dimensione;Font;Stile,
POLIGONO, POLIGONO;xN,YN;Colore;Tipo,
GRIGLIA, GRIGLIA;Lato;Colore,
RUOTA, RUOTA;-Gradi,
INVERTI, INVERTI;Direzione;-Percentuale,
SALVA, SALVA;Percorso;Formato(PNG,BMP,JPG),
INVERTI, INVERTI;Direzione;-Percentuale,
RUOTA, RUOTA;Gradi;-Percentuale,
APPUNTI, APPUNTI,
COPIA, COPIA;Percorso,
INCOLLA, INCOLLA;x1,y1,
RIDIMENSIONA, RIDIMENSIONA;Appunti;Larghezza,Altezza,
SPLINE, SPLINE;xN,yN;Colore;Spessore,
SPLINE2, SPLINE2;xN,yN;Colore;Spessore;Tensione,
CROCE, CROCE;x1,y1;x2,y2;Colore;Spessore,
BEZIER, BEZIER;x1,y1;x2,y2;x3,y3;x4,y4;Colore;Spessore,
TEXTURE, TEXTURE;Nome;Percorso,
DRAWTEXTURE, DRAWTEXTURE;Nome;x,y,
PATTERN, PATTERN;Nome;LINEE;Angolo;Spaziatura;Colore;Spessore,
FILLPATTERN, FILLPATTERN;Nome;x1,y1;x2,y2,
INIZIO, INIZIO,
INIZIOCAD, INIZIOCAD;Passo;Colore,
INIZIOMATH, INIZIOMATH;ScalaX;ScalaY;ColoreScale,
STEP, STEP;n,
ADDLIVELLO, ADDLIVELLO;NomeLivello,
DELLIVELLO, DELLIVELLO;NomeLivello,
RENLIVELLO, RENLIVELLO;NomeLivello;NuovoNomeLivello,
GRIGLIAFULL;Lato;Colore,
FRECCIA;x1,y1;x2,y2;Colore;Spessore,
STELLA;x1,y1;NumeroPunte;Diametro;Colore;Spessore,
DEFINE;NomeMacro;Parametri;L,
FOR;NomeCiclo;Start;End;Step,
END,
INCLUDE;PercorsoFile,
MATRICE;x1,y1;x2,y2;x3,y3;...;PASSOX,PASSOY;Tipo;Colore;Spessore,
MATRICEQ;x1,y1;x2,y2;PASSOX,PASSOY;Tipo;Colore;Spessore,
SPIRALE, SPIRALE;CentroX,CentroY;RaggioIniziale;RaggioFinale;Giri;Colore;Spessore;Direzione,
SINUSOIDE, SINUSOIDE;StartX,StartY;EndX,EndY;Ampiezza;Frequenza;Colore;Spessore


6 - Esempi dei Comandi (devono terminare con un ritorno a capo)

L’esempio seguente crea una linea continua dal punto (x,y) (7,10) al punto (96,92) di colore Blu e di spessore 2 pixel.

		LINEA;7,10;96,92;Blu;2

L’esempio seguente crea un rettangolo vuoto dal punto (50,50) al punto (200,150) di colore Nero e di spessore 2 pixel.

		RETT;50,50;200,150;Nero;VUOTO;2

L’esempio seguente crea un cerchio pieno dal centro (200,200) di raggio 50 e di colore Rosso e di spessore 3 pixel.

		CERCHIO;200,200;50;Rosso;PIENO;3

L’esempio seguente crea un triangolo vuoto i cui vertici sono nei punti (72,50) (440,358) (17,340) di colore Rosso e di spessore 1 pixel.

		TRIANG;72,50;440,358;17,340;Rosso;VUOTO;1

L’esempio seguente pulisce il foglio/schermo completamente con il colore Bianco.

		PULISCI;Bianco

L’esempio seguente crea un arco dal punto (50,50) al punto (150,150) di colore Rosso e pieno. Lo spessore del tratto di 2 pixel e l’arco partirà dall’angolo 0 all’angolo 180 gradi. Pieno vuol dire che viene chiuso in una specie di semiluna.

		ARCO;50,50;150,150;Rosso;PIENO;2;0;180

L’esempio seguente crea un arco dal punto (200,50) al punto (300,150) di colore Blu e vuoto. Lo spessore del tratto di 3 pixel e l’arco partirà dall’angolo 90 all’angolo 270 gradi. Vuoto vuol dire che non viene riempito e rimane un semplice arco.

		ARCO;200,50;300,150;Blu;VUOTO;3;90;270

L’esempio seguente crea una scacchiera sullo foglio/schermo di colore Blu e Rossa dal punto (100,10) al punto (400,310). La scacchiera è sempre divisa in otto per otto quadrati.

		SCACCHI;100,10;400,310;Blu;Rosso

L’esempio seguente crea un testo grafico sul foglio/schermo con scritto Ciao mondo di colore Rosso dal punto (100,200). Il font usato è Arial, normale di dimensione 20. Gli altri stili oltre il Normal sono Bold e Italic.

		TESTO;100,200;Ciao mondo;Rosso;20;Arial;Normal

L’esempio seguente crea un poligono di n lati dal punto (10,10) seguendo tutti i punti che vengono descritti e quindi (100,30) (80,120) (30,90). Il poligono è di colore Verde e verrà riempito. Utilizzando coordinate negative è possibile disegnare fiocchi, valvole, clessidre etc.

		POLIGONO;10,10;100,30;80,120;30,90;Verde;PIENO

L’esempio seguente crea una griglia sul foglio/schermo dal punto 0,0 fino al visibile di colore Grigio e passo 20 pixel.

		GRIGLIA;20;Grigio

L’esempio seguente Ruota il disegno di 45 gradi in senso Anti-Orario.

		RUOTA;-45;Grigio

L’esempio seguente Inverte il 50% del disegno in senso Orizzontale.

		INVERTI;Orizzontale;-50
		INVERTI;X;50       → inverte metà della bitmap orizzontalmente (da sinistra)
		INVERTI;Y;-25      → inverte il 25% inferiore della bitmap verticalmente
		INVERTI;X;100      → inverte tutta la bitmap orizzontale
		INVERTI;Y;100      → inverte tutta la bitmap verticale

L’esempio seguente salva l’intero contenuto del disegno in un file JPEG nel percorso C:\Temp in un file di nome foto.jpg.

		SALVA;C:\Temp\foto.jpg;JPG

L’esempio seguente copia l’intero contenuto del disegno negli appunti di Windows.

		APPUNTI

L’esempio seguente copia il contenuto immagine  del  file  foto.png   nel   percorso   C:\Immagini   negli appunti di Windows.

		COPIA;C:\Immagini\foto.png

L’esempio seguente incolla il contenuto degli appunti di Windows alla coordinata (50,100).

		INCOLLA;50,100

L’esempio seguente ridimensiona l’immagine, se presente, negli appunti alle dimensioni di 400 x 400 pixels.

		RIDIMENSIONA;Appunti;400,400

L’esempio seguente crea una spezzata dal punto (29,15) passando dai punti (53,500) (1000,183) di colore Giallo e di spessore 1 pixel.

		SPLINE;29,15;53,500;1000,183;Giallo;1

L’esempio seguente crea una spezzata ma stavolta curva con una tensione della curva di 0.7 dal punto (10,10) passando dai punti (50,80) (100,50) (150,120) di colore Blu e spessore 2 pixel.

		SPLINE2;10,10;50,80;100,50;150,120;Blu;2;0.7

L’esempio seguente crea una spirale partendo dal punto (400,300) al punto (10,150) con 5 giri di colore Blu e spessore 2 pixel girando in senso Orario. Usando Antioraria la rotazione avverrà in senso opposto.

		SPIRALE;400,300;10;150;5;Blu;2;Oraria

L’esempio seguente crea una sinusoide partendo dal punto (600,200) fino al punto (700,200) di ampiezza 50 pixel, frequenza 25 pixel, colore Blu e spessore 2 pixel.

		SINUSOIDE;600,200;700,200;50;25;Blu;2

L’esempio seguente crea una croce nel rettangolo (50,50) (100,100) di colore Arancio e spessore 3 pixel. La croce quindi è alta e larga 50 pixel al centro del rettangolo.

		CROCE;50,50;100,100;Arancio;3

L’esempio seguente crea una linea curva di tipo Bezier, dal punto (800,320) seguendo una traiettoria morbida verso (850,340) (900,310) (950,210) di colore Viola e 2 pixel di spessore.

		BEZIER;800,320;850,340;900,310;950,210;VIOLA;2

L’esempio seguente crea in memoria una bitmap texture di nome Texture01 leggendo l’immagine dal file nel percorso C:\TEXTURE dal file immagine textures_autocad_45635.gif. Le texture non sono animate.

		TEXTURE;Texture01;C:\TEXTURE\textures_autocad_45635.gif

L’esempio seguente incolla/disegna sul foglio/schermo una texture precedentemente creata di nome Texture01 iniziando dal punto (950,400).

		DRAWTEXTURE;Texture01;950,400

L’esempio seguente crea un pattern, una sequenza di dot chiamata PUNTI. L’angolo in gradi sarà di 10 con un colore Nero e grandezza di 1 pixel.

		PATTERN;PUNTI;DOT;1000;10;Nero;1

L’esempio seguente disegna un pattern precedentemente creato di nome PUNTI, iniziando il disegno dal punto (1000,100) fino al punto (400,300).

		FILLPATTERN;PUNTI;1000,100;400,300

L’esempio seguente pulisce e inizializza il foglio schermo di colore Bianco.

		INIZIO

L’esempio seguente pulisce e inizializza il foglio schermo di colore Bianco e disegna gli assi Cartesiani al centro con una tacchettatura per la X di 20 pixels e per la Y di 10 pixels. I tacchetti sugli assi di colore Blu.

		INIZIOMATH;20;10;Blu

L’esempio seguente pulisce e inizializza il foglio schermo di colore Nero e disegna una griglia di punti in stile AutoCAD di colore Lime con passo 10 pixel.

		INIZIOCAD;10;Lime

L’esempio seguente inizializza PaintRobot ad eseguire i comandi uno (1) alla volta e a fermarsi mostrando il pulsante Continua.

		STEP;0

L’esempio seguente inizializza PaintRobot ad eseguire i comandi uno (10) alla volta senza fermarsi.

		STEP;10

L’esempio seguente aggiunge un livello di disegno chiamato LIVELLO1 sul quale verranno disegnati i successivi comandi. Senza nessun comando di livello il disegno viene creato su BackGround.

		ADDLIVELLO;LIVELLO1

L’esempio seguente rimuove il livello LIVELLO1 dal disegno e i successivi comandi verrano disegnati sul livello precedente/sottostante.

		DELLIVELLO;LIVELLO1

L’esempio seguente rinomina il livello LIVELLO1 in LIVELLO2. Nessuna modifica su dove viene disegnato il comando successivo.

		RENLIVELLO;LIVELLO1;LIVELLO2

L’esempio seguente crea una griglia su tutto il disegno con passo 20 e colore Grigio.

		GRIGLIAFULL;20;Grigio

L’esempio seguente disegna una Freccia dal punto (0,0) al punto (100,100) di colore Nero e spessore 3 pixel.

		FRECCIA;0,0;100,100;Nero;3

L’esempio seguente disegna una Stella a 5 punte dal punto centrale (150,150) di diametro 100 di colore Nero e spessore 2 pixel.

		STELLA;150,150;5;100;Nero;2

L’esempio seguente include un altro file di script con nome ScriptINCLUDE.txt.

		#Include un altro script da qui in poi
		INCLUDE;C:\Users\user\Desktop\ScriptINCLUDE.txt

L'esempio seguente definisce una Marco FRECCIA1 per creare freccie.

		DEFINE;FRECCIA1;X;Y;L
		# Crea una freccia con coordinate relative
		LINEA;{X},{Y};{X+L},{Y};Nero;3
		LINEA;{X+L},{Y};{X+L-10},{Y-5};Rosso;2
		LINEA;{X+L},{Y};{X+L-10},{Y+5};Verde;2
		END

L’esempio seguente disegna due freccie con una macro FRECCIA1.

		FRECCIA1;1000;200;80
		FRECCIA1;1000;250;120

L'esempio seguente utilizza il comano For per disegnare delle linee.

		FOR;X;200;300;25
		LINEA;{X},100;{X},200;Blu;1
		END

L'esempio seguente utilizza il comando Matrice per disegnare un Trapezio colore Malva e riempirlo di Punti (Punti,Quadrati,Corci,X) con passo 20, spessore 2 pixel. 

		MATRICE;100,200;300,200;250,350;150,350;20,20;Punti;Malva;2

L'esempio seguente utilizza il comando MatriceQ per disegnare un Rettangolo Verde e riempirlo di Quadrati (Punti,Quadrati,Croci,X) passo 10, spessore 2 pixel.

		MATRICEQ;800,400;1000,700;10,10;Quadrati;Verde;2

L'esempio seguente crea un quadrato usando i comandi MUOVI e linea con le clausole LAST e FROM. Due lati Rosso e due Blu.

		#Poligono manuale
		MUOVI;100,100
		Linea;FROM;200,100;Blu;2
		Linea;LAST;200,200;Blu;2
		Linea;LAST;100,200;Rosso;2
		Linea;LAST;FROM;Rosso;2

L'esempio seguente identico al precedente crea un quadrato usando i comandi Linea E CHIUDI con le clausole LAST e FFROM. Due lati Rosso e due Blu.

		#Poligono manuale
		Muovi;100,100
		Linea;FROM;200,100;Blu;2
		Linea;LAST;200,200;Blu;2
		Linea;LAST;100,200;Rosso;2
		CHIUDI;100,100;Rosso;2





Colori Italiani Supportati

Il dizionario ColoriItaliani espone una raccolta di colori accessibili tramite nomi in italiano (in maiuscolo), mappati ai corrispondenti valori Color.

[Colori base]
	NERO → Black
	BIANCO → White
	ROSSO → Red
	VERDE → Green
	BLU → Blue
	GIALLO → Yellow
	CIANO → Cyan
	MAGENTA → Magenta
	GRIGIO → Gray
	ARGENTO → Silver

[Colori caldi e naturali]
	MARRONE → Brown
	MARRONECHIARO → Peru
	MARRONESCURO → Sienna
	ARANCIONE → Orange
	ROSSOSCURO → DarkRed
	ROSSOCHIARO → IndianRed
	SALMONE → Salmon
	CORALLO → Coral
	TERRA → SaddleBrown
	SABBIA → SandyBrown
	SENAPE → Goldenrod
	OCRA → RGB(204, 119, 34)
	RUGGINE → RGB(183, 65, 14)
	RAME → RGB(184, 115, 51)
	BRONZO → RGB(205, 127, 50)
	MOGANO → RGB(192, 64, 0)
	CASTAGNO → RGB(205, 92, 92)
	CARBONE → RGB(5, 4, 2)

[Colori freddi]
	AZZURRO → LightBlue
	BLUSCURO → DarkBlue
	BLUCHIARO → LightSkyBlue
	CIELO → SkyBlue
	ACQUA → Aqua
	ACQUAMARINA → RGB(0, 255, 128)
	TURCHESE → Teal
	CERULEO → RGB(0, 123, 167)
	COBALTO → RGB(0, 71, 171)
	INDACO → Indigo
	GHIACCIO → LightCyan
	ARDESIA → RGB(119, 136, 153)

[Verdi e derivati]
	VERDECHIARO → LightGreen
	VERDESCURO → DarkGreen
	LIME → Lime
	OLIVA → Olive
	GIADA → RGB(0, 168, 107)
	CHARTREUSE → RGB(128, 255, 0)

[Viola, rosa e affini]
	VIOLA → Purple
	VIOLACHIARO → Lavender
	VIOLASCURO → MediumPurple
	MELANZANA → DarkViolet
	BORDEAUX → DarkRed
	BORGOGNA → RGB(128, 0, 32)
	PRUGNA → Plum
	FUXIA → Fuchsia
	ROSA → Pink
	MALVA → RGB(224, 176, 255)
	LILLA → RGB(200, 162, 200)
	CILIEGIA → RGB(222, 49, 99)
	CARMINIO → RGB(150, 0, 24)
	GRANATA → RGB(128, 0, 0)

[Toni chiari e neutri]
	BEIGE → Beige
	AVORIO → Ivory
	ZINCO → LightGray
	CACHI → RGB(195, 176, 145)
	CAMOSCIO → RGB(240, 220, 130)
	SANDALO → RGB(209, 182, 113)
	NAPOLI → RGB(247, 232, 159)
	SEPPIA → RGB(62, 48, 35)

[Altri colori]
	LAVANDA → RGB(181, 126, 220)
	MAGGESE → RGB(193, 154, 107)
	POMORODO → RGB(255, 99, 71)
	LIMONE → RGB(253, 255, 0)



"

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'A tutto schermo
        Me.WindowState = FormWindowState.Maximized

        RichTextBox1.Size = Me.Size
		RichTextBox1.Location = New Point(0, 0)

		RichTextBox1.Text = help
	End Sub
End Class