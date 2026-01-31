
Imports System.Diagnostics.Contracts
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports System.Net.Sockets
Imports System.Net.WebRequestMethods
Imports System.Runtime.InteropServices.ComTypes
Imports System.Runtime.Remoting
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Web
Imports System.Windows.Forms.AxHost
Imports Microsoft.SqlServer

Public Class Form1

    '08-01-2026
    'PaintRobot/ v 2.0
    '│
    '├─ RobotInterpreter.vb   ← parser + dispatcher
    '├─ RobotCommand.vb       ← modello comando
    '├─ RobotDrawer.vb        ← disegno vero e proprio
    '└─ Script.txt            ← linguaggio di disegno

    'Concetto finale (chiaro e robusto)
    '  ; → separa parametri (linguaggio PaintRobot)
    '  + → separa comandi UI
    '  Ogni comando dopo + è completo e indipendente

    'Regola d’oro (finale)
    ' Non disegnare mai più di quello che vedi

    'Bitmap 10K
    Private LarghezzaPaginaPixel As Integer = 10000
    Private AltezzaPaginaPixel As Integer = 10000
    Private Origine As Point = New Point(0, 0)
    Private OrigineF As PointF = New PointF(0, 0)

    'Mondo 10K, disegno e snapshot 0
    Private PaintRobotBMP As Bitmap = Nothing
    Private picbmp As Bitmap = Nothing
    Private BgSnapshot As Bitmap = Nothing

    'Abilita il Debug
    Private DEBUGOK As Boolean = False

    'Controllo
    Private PaintRobotAlted As Boolean = False
    Private redrawInProgress As Boolean = False
    Const BATCH_SIZE As Integer = 50

    'Timer-Driven Renderer
    Private WithEvents RenderTimer As New Windows.Forms.Timer With {.Interval = 16} ' ~60 FPS
    Private renderIndex As Integer = -1
    Private rendering As Boolean = False
    Private renderGraphics As Graphics = Nothing
    Private renderCtx As RobotContext = Nothing

    'Controllo Preview
    Const PREVIEW_BATCH As Integer = 100
    'Numero di comandi per tick visibile ovunque
    Public Shared commandsPerTick As Integer = 40
    'Booleana che deve essere True per continuare il disegno a STEP=0
    Public Shared waitForContinue As Boolean = False ' true se N=0 e voglio fare 1 comando e fermarmi

    'Serve per muovere la finestra senza bordi
    Private dragging As Boolean = False
    Private dragCursorPoint As Point
    Private dragFormPoint As Point

    'Mondo
    Private View As New Viewport()

    'Pannelli
    Public Const PanelLeftWidth As Integer = 60
    Public Const PanelTopHeight As Integer = 60

    'Test
    Private inTest As Boolean = False

    'Help CHM
    Private helpPathCHM As String = Path.Combine(Application.StartupPath, "PaintRobot2Help.chm")

    'Firma
    Private firmaImg As Bitmap = My.Resources.PaintRobotFirma

    'File
    Private FileScript As String = ""

    'PaintRobot
    Private Comandi As List(Of RobotCommand) 'Da script
    Private Commands As List(Of RobotCommand) 'Interfaccia
    ' History dei comandi
    Private History As New List(Of RobotCommand) 'Storico
    ' History di stringhe user-friendly
    ' Storico testuale (UI / TXT)
    Private HistoryString As New List(Of String)

    Private Execute As Boolean = False 'Sovraintende alla eseguzione dei comandi interfaccia
    Private Comando As String = ""
    Private numComandi As Integer = 0

    Private Drawer As New RobotDrawer()
    Private Interpreter As New RobotInterpreter()


    ' PaintRobot
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Finestra sempre massimizzata. 
        Dim WR As Rectangle = Screen.GetWorkingArea(New Point(0, 0))
        Me.Size = New Size(WR.Width, WR.Height - 50)
        Me.Location = New Point(0, 40)

        ButtonClose.Location = New Point(Me.Width - 30, 3)
        ButtonMinimize.Location = New Point(Me.Width - 60, 3)

        'Casting comandi a Maiuscolo
        TextBoxCommands.CharacterCasing = CharacterCasing.Upper

        'Zoom Start
        ZoomStart()

        'Help
        Me.KeyPreview = True

        'Interfaccia
        CreaInterfaccia()

        'Info Test
        Debug.WriteLine("Numero Colori Italiani: " & RobotDrawer.ColoriItaliani.Count.ToString)

        Dim ofd As OpenFileDialog = New OpenFileDialog
        ofd.FileName = ""
        ofd.Filter = "Files di Testo|*.txt"
        ofd.Title = "Apri un script testo PaintRobot..."

        If ofd.ShowDialog(Me) = DialogResult.OK Then
            If ofd.FileName <> "" Then
                ' Disegno tuti i comandi dello script
                'Comandi = Interpreter.CaricaScript(ofd.FileName)
                FileScript = ofd.FileName

                ' Supporto MACRO
                Comandi = Interpreter.CaricaScriptConMacro(ofd.FileName)

                History.Clear()
                History.AddRange(Comandi)

                ' Linee Script
                Dim ComandiStringa As List(Of String) = Interpreter.CaricaLineeScript(ofd.FileName)

                HistoryString.Clear()
                For Each cmd In Comandi
                    Dim s As String = cmd.Tipo
                    If cmd.Parametri IsNot Nothing AndAlso cmd.Parametri.Count > 0 Then
                        s &= ";" & String.Join(";", cmd.Parametri)
                    End If

                    HistoryString.Add(s)
                Next

                ' ListBox Comandi
                AggiornaListaComandiStringa()

                Dim HistoryIndex = History.Count   ' 👈 TUTTI attivi

                numComandi = Comandi.Count - 1
                ProgressBar1.Minimum = 0
                ProgressBar1.Maximum = numComandi
                ProgressBar1.Value = 0

                StartRender()
            End If
        End If
    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        'HELP CHM
        If e.KeyCode = Keys.F1 Then
            ' Controlla se esiste prima di aprirlo
            If IO.File.Exists(helpPathCHM) Then
                Process.Start(helpPathCHM)
            End If
        End If
    End Sub

    Private Sub CreaInterfaccia()
        ' Pulsante Test solo se Debug attivo. Crea uno script ultra pesante
        ' Per test con migliaia di comandi grafici casuali.
        ButtonTest.Visible = True

        ButtonTest.Location = New Point(ButtonSavePaintRobot.Right + 20, ButtonSavePaintRobot.Location.Y)
        ButtonHistory.Location = New Point(ButtonSavePaintRobot.Location.X + 1, 32)

        ' Layout principale
        PanelTop.Dock = DockStyle.Top
        PanelTop.Height = PanelTopHeight

        PanelLeft.Dock = DockStyle.Left
        PanelLeft.Width = PanelLeftWidth

        LstComandi.Dock = DockStyle.Bottom
        LstComandi.Height = 60
        LstComandi.DrawMode = DrawMode.OwnerDrawFixed
        'LstComandi.ItemHeight = 30   ' altezza riga

        PictureBox1.Dock = DockStyle.Fill

        'PictureBox sempre grande
        'PictureBox1.Size = New Size(LarghezzaPaginaPixel, AltezzaPaginaPixel)
        'PictureBox1.Location = New Point(60, 60)

        ToolTip1.IsBalloon = True
        ToolTip1.ToolTipIcon = ToolTipIcon.Info
        ToolTip1.ToolTipTitle = "PaintRobot CAD"
        ToolTip1.ShowAlways = True

        ToolTip1.SetToolTip(ButtonCmd, "Eseguire il comando CAD scritto o dettato")
        ToolTip1.SetToolTip(ButtonStop, "Interrompere il rendering in corso")
        ToolTip1.SetToolTip(TextBoxCommands, "Scrivere o dettare un comando CAD")
        ToolTip1.SetToolTip(ButtonContinueRendering, "Eseguire il comando CAD successivo")
        ToolTip1.SetToolTip(ButtonRenderRemain, "Eseguire i restanti comandi CAD")
        ToolTip1.SetToolTip(ButtonHelp, "Aprire la finestra di aiuto")
        ToolTip1.SetToolTip(ButtonScript, "Salvare lo script generato dai comandi PaintRobot")
        ToolTip1.SetToolTip(ButtonEditHistory, "Aprire la finestra della History comandi")
        ToolTip1.SetToolTip(ButtonAttivaListBox, "Attivare o disattivare la lista comandi History")
        ToolTip1.SetToolTip(ButtonHistory, "Renderizzare tutti i comandi CAD dalla History")
        ToolTip1.SetToolTip(ButtonTest, "Testare PaintRobot con comandi casuali")
        ToolTip1.SetToolTip(ButtonSavePaintRobot, "Salvare l'area di disegno in 10k")
        ToolTip1.SetToolTip(ButtonClose, "Chiudere PaintRobot")
        ToolTip1.SetToolTip(ButtonMinimize, "Ridurre a icona PaintRobot")
        ToolTip1.SetToolTip(ButtonLeft, "Spostare il disegno a Sinistra")
        ToolTip1.SetToolTip(ButtonRight, "Spostare il disegno a Destra")
        ToolTip1.SetToolTip(ButtonUp, "Spostare il disegno in Su")
        ToolTip1.SetToolTip(ButtonDown, "Spostare il disegno in Giu")
        ToolTip1.SetToolTip(ButtonOrigin, "Tonare al punto 0,0")
        ToolTip1.SetToolTip(ButtonCentroMondo, "Spostare la vista al centro")
        ToolTip1.SetToolTip(ButtonZoomIn, "Aumentare lo Zoom disegno")
        ToolTip1.SetToolTip(ButtonOut, "Diminuire lo Zoom disegno")

        'Colori
        ListView1.Visible = False

        With ListView1
            .FullRowSelect = True
            .GridLines = True
            .HideSelection = False
            .OwnerDraw = True
            .Columns.Clear()
            .Columns.Add("Colore", 200)
            .Columns.Add("RGB", 150)
        End With

        CaricaColori()
    End Sub

    ' World Offset CAD
    ' Corregge il PictrureBox1.DockStyle.Fill che manda la pic a 0,0
    Public Shared ReadOnly WorldOffset As PointF = New PointF(PanelLeftWidth, PanelTopHeight)

    Private Sub ButtonReload_Click(sender As Object, e As EventArgs) Handles ButtonReload.Click
        'Ricarica script
        If String.IsNullOrEmpty(FileScript) Then Return

        ' Ferma il rendering se in corso
        StopRender()
        RenderTimer.Stop()
        renderIndex = 0

        ' Dispone in sicurezza oggetti grafici precedenti
        If renderGraphics IsNot Nothing Then
            renderGraphics.Dispose()
            renderGraphics = Nothing
        End If

        If PaintRobotBMP IsNot Nothing Then
            PictureBox1.Image = Nothing
            PaintRobotBMP.Dispose()
            PaintRobotBMP = Nothing
        End If

        If BgSnapshot IsNot Nothing Then
            BgSnapshot.Dispose()
            BgSnapshot = Nothing
        End If

        If picbmp IsNot Nothing Then
            picbmp.Dispose()
            picbmp = Nothing
        End If

        renderCtx = Nothing
        PaintRobotAlted = False
        waitForContinue = False

        ' Carica script + macro
        Comandi = Interpreter.CaricaScriptConMacro(FileScript)

        History.Clear()
        History.AddRange(Comandi)

        HistoryString.Clear()
        For Each cmd In Comandi
            Dim s As String = cmd.Tipo
            If cmd.Parametri IsNot Nothing AndAlso cmd.Parametri.Count > 0 Then
                s &= ";" & String.Join(";", cmd.Parametri)
            End If
            HistoryString.Add(s)
        Next

        AggiornaListaComandiStringa()

        ' Reset progress bar
        numComandi = Comandi.Count - 1
        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = numComandi
        ProgressBar1.Value = 0

        ZoomStart()

        ' Avvia nuovo rendering
        StartRender()
    End Sub

    Private Sub StartRender()
        ' avvio del timer Rendering
        If rendering Then Return

        PaintRobotAlted = False
        rendering = True
        renderIndex = 0

        ' Crea bitmap nuova per il rendering
        PaintRobotBMP = New Bitmap(LarghezzaPaginaPixel, AltezzaPaginaPixel)
        picbmp = New Bitmap(PictureBox1.Width * 2, PictureBox1.Height * 2)
        PictureBox1.Image = picbmp

        ' Misura
        If inTest Then swRender.Start()

        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = Comandi.Count
        ProgressBar1.Value = 0

        ' Prepariamo bitmap e Graphics UNA SOLA VOLTA
        renderGraphics = Graphics.FromImage(PaintRobotBMP)
        renderGraphics.Clear(Color.White)
        renderGraphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        renderGraphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        renderGraphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality

        ' 🔥 Offset mondo CAD
        renderGraphics.TranslateTransform(WorldOffset.X, WorldOffset.Y)

        renderCtx = New RobotContext With {
        .Graphics = renderGraphics,
        .Bitmap = picbmp,
        .LivelloCorrente = "BackGround",
        .View = View
    }

        ' 🔴 PULIZIA STRUTTURE (fondamentale)
        renderCtx.Livelli.Clear()
        renderCtx.OrdineLivelli.Clear()

        ' ✅ Snapshot iniziale = BackGround
        BgSnapshot = CType(PaintRobotBMP.Clone(), Bitmap)

        renderCtx.Livelli.Add("BackGround", New Livello With {
        .Nome = "BackGround",
        .Bitmap = BgSnapshot
    })

        renderCtx.OrdineLivelli.Add("BackGround") 'Aggiunge all'indice OrdineLivelli

        ' Versione timer-driven Rendering
        RenderTimer.Start()
    End Sub

    Private Sub Render(Comandi As List(Of RobotCommand))
        'Renderizza un comando
        If rendering Then Return

        PaintRobotAlted = False
        rendering = True

        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = Comandi.Count
        ProgressBar1.Value = 0


        ' Se non esiste crea lo snapshot iniziale BackGround
        If BgSnapshot Is Nothing OrElse renderIndex = Nothing Then
            ' Prepariamo bitmap e Graphics UNA SOLA VOLTA
            renderGraphics = Graphics.FromImage(PaintRobotBMP)
            renderGraphics.Clear(Color.White)
            renderGraphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            renderGraphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            renderGraphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality

            ' Index = 0
            renderIndex = 0

            ProgressBar1.Minimum = 0
            ProgressBar1.Maximum = Comandi.Count
            ProgressBar1.Value = 0

            ' Inizializza a bianco
            renderGraphics.Clear(Color.White)

            renderCtx = New RobotContext With {
        .Graphics = renderGraphics,
        .Bitmap = picbmp,
        .LivelloCorrente = "BackGround",
        .View = View
    }

            ' 🔴 PULIZIA STRUTTURE (fondamentale)
            renderCtx.Livelli.Clear()
            renderCtx.OrdineLivelli.Clear()

            ' ✅ Snapshot iniziale = BackGround
            BgSnapshot = CType(PaintRobotBMP.Clone(), Bitmap)

            renderCtx.Livelli.Add("BackGround", New Livello With {
            .Nome = "BackGround",
            .Bitmap = BgSnapshot
        })

            renderCtx.OrdineLivelli.Add("BackGround") 'Aggiunge all'indice OrdineLivelli

            PictureBox1.Image = picbmp
        End If

        ' Aggiunge i comandi da eseguire
        For Each Comando As RobotCommand In Comandi
            Drawer.Esegui(Comando, renderCtx)

            If ListaComandiStringa Then EvidenziaComando(renderIndex)

            renderIndex += 1
            ProgressBar1.Value += 1
            Debug.WriteLine("RenderIndex " & renderIndex.ToString)
        Next

        ' Qui svuotiamo la lista dei comandi eseguiti in interfaccia
        Commands.Clear()

        LabelNumCmds.Text = renderIndex.ToString ' num comandi eseguiti

        ' Firma se vogliamo
        If firmaImg IsNot Nothing Then
            renderCtx.Graphics.DrawImage(firmaImg, 10, 10)
        End If

        'If DEBUGOK Then PaintRobotBMP.Save("C:\Temp\debug_master.png")

        'Comando Terminato
        Execute = False

        StopRender()
    End Sub

    Private Sub RenderTimer_Tick(sender As Object, e As EventArgs) Handles RenderTimer.Tick
        ' CUORE DEL SISTEMA
        If PaintRobotAlted OrElse renderIndex >= Comandi.Count Then
            StopRender()
            Exit Sub
        End If

        ' Una variabile per comandare anche la pausa STEP;0
        Dim commandsHERETick As Integer = Form1.commandsPerTick

        ' Inizializza l'indice principale
        If renderIndex = -1 Then renderIndex = 0

        ' ciclo esecuzione
        For i = 1 To commandsHERETick
            If renderIndex >= Comandi.Count OrElse PaintRobotAlted Then
                ButtonContinueRendering.Visible = False
                Exit For
            End If

            ' Evidenziazione mentre disegna
            If ListaComandiStringa Then EvidenziaComando(renderIndex)

            ' Comando
            Dim cmd = Comandi(renderIndex)
            If cmd.Tipo.Contains("VAI") Then
                Dim newrenderindex As Integer = GestisciVAI(cmd)
                renderIndex = newrenderindex
            End If

            Drawer.Esegui(cmd, renderCtx)
            renderIndex += 1

            ' Progresso di disegno
            ProgressBar1.Value = Math.Min(renderIndex, ProgressBar1.Maximum)

            Debug.WriteLine("RenderIndex " & renderIndex.ToString)

            ' se siamo in modalità STEP singolo (waitForContinue) fermiamo il timer subito dopo 1 comando
            If Form1.waitForContinue Then
                ' Mostra ciò che abbiamo disegnato finora
                RedrawViewport()   ' Aggiorna subito la PictureBox

                RenderTimer.Stop() ' fermo il timer
                Exit For
            End If
        Next

        ' Aggiorno visibilità pulsante dopo il ciclo
        If Form1.waitForContinue AndAlso renderIndex < Comandi.Count Then
            ButtonContinueRendering.Visible = True
            ButtonRenderRemain.Visible = True
        Else
            ButtonContinueRendering.Visible = False
            ButtonRenderRemain.Visible = False
        End If

        ' Firma se vogliamo
        If firmaImg IsNot Nothing Then
            renderCtx.Graphics.DrawImage(firmaImg, 10, 10)
        End If

        PictureBox1.Invalidate()   ' preview live
        PictureBox1.Update()
        Application.DoEvents()

        LabelNumCmds.Text = renderIndex.ToString ' num comandi eseguiti
    End Sub

    Private Sub StopRender()
        If Not rendering Then
            ButtonContinueRendering.Visible = False  ' Nascondi il pulsante se non serve
            Return
        End If

        swRender.Stop()

        If inTest Then
            Debug.WriteLine("=== TEST PaintRobot ===")
            Debug.WriteLine($"Tempo totale render: {swRender.ElapsedMilliseconds} ms")
            Debug.WriteLine($"Comandi eseguiti: {renderIndex}")
        End If

        rendering = False
        RedrawViewport()
        PictureBox1.Invalidate()
        ' STOP fluido
        RenderTimer.Stop()
    End Sub

    Private Function GestisciVAI(cmd As RobotCommand) As Integer
        If cmd.Parametri.Count = 0 Then Return renderIndex

        Dim targetStr = cmd.Parametri(0).Trim()
        Dim target As Integer

        ' 🔹 VAI relativo
        If targetStr.StartsWith("+") OrElse targetStr.StartsWith("-") Then
            target = renderIndex + Integer.Parse(targetStr)
        Else
            ' 🔹 VAI assoluto (1-based)
            target = Integer.Parse(targetStr) - 1
        End If

        ' Clamp di sicurezza
        If target < 0 Then target = 0
        If target >= Comandi.Count Then target = Comandi.Count - 1

        ' Nuovo renderindex
        Return target
    End Function

    Public Sub EvidenziaComando(index As Integer)
        'Evidenzia il comando eseguito nella Listbox
        If index < 0 OrElse index >= LstComandi.Items.Count Then Return

        LstComandi.SelectedIndex = index

        ' Centra la riga selezionata nella ListBox
        Dim visibleItems As Integer = Math.Max(1, LstComandi.ClientSize.Height \ LstComandi.ItemHeight)
        Dim top As Integer = Math.Max(0, index - (visibleItems \ 2))

        LstComandi.TopIndex = top
    End Sub

    Private Sub RedrawMaster()
        ' Copio picbmp sul Master PaintRobotBMP
        Using g As Graphics = Graphics.FromImage(PaintRobotBMP)
            g.DrawImage(picbmp, Origine)
        End Using
    End Sub

    Private Sub ButtonCmd_Click(sender As Object, e As EventArgs) Handles ButtonCmd.Click
        'Eseugue il comando
        If rendering Then
            MessageBox.Show("Rendering in corso. Puoi premere STOP.")
            Return
        End If

        If TextBoxCommands.Text = "" Then Return

        ' 🔥 1. Normalizza il testo dettato
        'Dim testoNormalizzato As String = NormalizeSpeechInput(TextBoxCommands.Text.Trim)
        ' 🔥 2. Normalizza le coordinate e rimuove gli spazi
        ' Ma puoi usare anche NormalizzaStruttura()
        'Dim SpaziNormalizzati As String = NormalizzaCoordinate(TextBoxCommands.Text.Trim)
        'Dim testoNormalizzato = SpaziNormalizzati.Replace(" ", "")

        Dim testoNormalizzato = TextBoxCommands.Text.Trim
        testoNormalizzato = testoNormalizzato.Replace(" ", "")

        Comando = testoNormalizzato
        Execute = True
        PaintRobotAlted = False

        ' Crea la lista di RobotCommands senza Marco, FOR etc.
        Commands = Interpreter.CaricaComandiMultipli(testoNormalizzato)

        ' Inizializza l'indice principale
        If renderIndex = -1 Then renderIndex = 0

        ' 🔥 UNDO: elimina il futuro (comandi + stringhe)
        If renderIndex >= 0 AndAlso renderIndex < History.Count Then
            History.RemoveRange(renderIndex, History.Count - renderIndex)
            HistoryString.RemoveRange(renderIndex, HistoryString.Count - renderIndex)
        End If

        ' Crea la lista di comandi stringa da visualizzare
        Dim CommandsStringa As List(Of String) = Interpreter.CaricaComandiStringaMultipli(testoNormalizzato)
        HistoryString.AddRange(CommandsStringa)

        ' ListBox Comandi
        AggiornaListaComandiStringa()

        ' Aggiorna la History
        History.AddRange(Commands)

        If Commands Is Nothing OrElse Commands.Count = 0 Then Return

        ' Renderizza il comando
        Render(Commands)
    End Sub

    Private Function NormalizzaCoordinate(input As String) As String
        ' 1. Rimuove spazi dopo ;
        input = Regex.Replace(input, ";\s+", ";")

        ' 2. Trasforma "numero spazio numero" in "numero,numero"
        input = Regex.Replace(input, "(?<=\d)\s+(?=\d)", ",")

        Return input
    End Function

    Private Function NormalizzaStruttura(input As String) As String
        ' Rimuove spazi prima e dopo ;
        input = Regex.Replace(input, "\s*;\s*", ";")

        ' Rimuove spazi prima e dopo ,
        input = Regex.Replace(input, "\s*,\s*", ",")

        Return input
    End Function

    Private Sub AggiornaListaComandiStringa()
        'Aggiunge i comandi stringa alla listbox
        LstComandi.BeginUpdate()
        LstComandi.Items.Clear()

        ' Popola senza Indice
        'For Each s In HistoryString
        'LstComandi.Items.Add(s)
        'Next

        ' Popola con Indice
        For i As Integer = 0 To HistoryString.Count - 1
            ' Aggiungi numero + comando
            LstComandi.Items.Add($"{i + 1}: {HistoryString(i)}")
        Next

        LstComandi.EndUpdate()
    End Sub

    Private Sub RenderAllHistory()
        'Renderizza la History 
        If rendering Then Return

        rendering = True

        ' A zero l'indice principale
        Dim HistoryIndex = 0
        Debug.WriteLine("HistoryIndex " & HistoryIndex.ToString)

        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = Comandi.Count
        ProgressBar1.Value = 0

        ' Prepariamo bitmap e Graphics UNA SOLA VOLTA
        'renderGraphics = Graphics.FromImage(PaintRobotBMP)
        renderGraphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        ' ResetContext
        renderGraphics.Clear(Color.White)

        For Each Comando As RobotCommand In History
            If HistoryIndex = renderIndex Then Exit For
            Drawer.Esegui(Comando, renderCtx)

            HistoryIndex += 1
            ProgressBar1.Value = HistoryIndex
            Debug.WriteLine("RenderIndex " & renderIndex.ToString)
        Next

        LabelNumCmds.Text = renderIndex.ToString ' num comandi eseguiti
        Debug.WriteLine("Eseguiti " & HistoryIndex.ToString & " History commands")

        StopRender()
    End Sub

    Private Sub ButtonHistory_Click(sender As Object, e As EventArgs) Handles ButtonHistory.Click
        'Renderizza tutta la History
        If History.Count = 0 Then Return

        StopRender()

        rendering = False

        RenderAllHistory()
    End Sub

    Private Sub ButtonContinueRendering_Click(sender As Object, e As EventArgs) Handles ButtonContinueRendering.Click
        'Continue Rendering
        If renderIndex >= Comandi.Count Then Return

        ' riavvia il timer per eseguire il prossimo comando
        ButtonContinueRendering.Visible = False
        Debug.WriteLine("PAUSA per STEP=0")
        RenderTimer.Start()
    End Sub

    Private Sub ButtonRenderRemain_Click(sender As Object, e As EventArgs) Handles ButtonRenderRemain.Click
        'Render Remains
        If renderIndex >= Comandi.Count Then Return

        ' riavvia il timer per eseguire il prossimo comando
        ButtonContinueRendering.Visible = False
        Debug.WriteLine("REMAIN commandsPerTick=40")

        waitForContinue = False
        commandsPerTick = 40

        RenderTimer.Start()
    End Sub

    Private Sub ButtonStop_Click(sender As Object, e As EventArgs) Handles ButtonStop.Click
        'Stoppa i comandi
        Execute = False
        PaintRobotAlted = True
    End Sub

    Private Sub PanelTop_Paint(sender As Object, e As PaintEventArgs) Handles PanelTop.Paint
        'Pannello superiore
    End Sub

    Private Sub PanelTop_MouseDown(sender As Object, e As MouseEventArgs) Handles PanelTop.MouseDown
        dragging = True
        dragCursorPoint = Cursor.Position
        dragFormPoint = Me.Location
    End Sub

    Private Sub PanelTop_MouseMove(sender As Object, e As MouseEventArgs) Handles PanelTop.MouseMove
        If dragging Then
            Dim diff As Point = Point.Subtract(Cursor.Position, New Size(dragCursorPoint))
            Me.Location = Point.Add(dragFormPoint, New Size(diff))
        End If
    End Sub

    Private Sub PanelTop_MouseUp(sender As Object, e As MouseEventArgs) Handles PanelTop.MouseUp
        dragging = False
    End Sub

    Private Sub ButtonMinimize_Click(sender As Object, e As EventArgs) Handles ButtonMinimize.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub ButtonClose_Click(sender As Object, e As EventArgs) Handles ButtonClose.Click
        PaintRobotAlted = True
        Me.Close()
    End Sub

    'View ZOOM
    Private Const ZOOM_STEP As Single = 1.1F ' 10% per step

    Private Sub RedrawViewport()
        'Ridisegna la vista
        Dim src As RectangleF = GetWorldViewRect()
        Dim dst As New Rectangle(0, 0, PictureBox1.Width, PictureBox1.Height)

        Using g As Graphics = Graphics.FromImage(picbmp)
            g.Clear(Color.White)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBilinear

            If PaintRobotBMP IsNot Nothing Then
                ' Creiamo una copia temporanea thread-safe
                Dim bmpCopy As Bitmap
                SyncLock PaintRobotBMP
                    bmpCopy = CType(PaintRobotBMP.Clone(), Bitmap)
                End SyncLock

                g.DrawImage(bmpCopy, dst, src, GraphicsUnit.Pixel)
                bmpCopy.Dispose()
            End If
        End Using

        PictureBox1.Invalidate()
    End Sub

    Private Function GetWorldViewRect() As RectangleF
        'Calcola il rettangolo visibile nel mondo
        Dim w = PictureBox1.Width / View.Zoom
        Dim h = PictureBox1.Height / View.Zoom

        Return New RectangleF(
        View.Origine.X,
        View.Origine.Y,
        w,
        h
    )
    End Function
    Private Sub ZoomStart()
        View.Zoom = 1.0F

        Origine = New Point(0, 0)
        OrigineF = New PointF(0, 0)

        View.Origine = OrigineF
    End Sub

    Private Sub CenterOnDrawing(min As PointF, max As PointF)
        ' Dimensioni del disegno
        Dim w As Single = max.X - min.X
        Dim h As Single = max.Y - min.Y

        ' Centro del disegno
        Dim cx As Single = min.X + w / 2
        Dim cy As Single = min.Y + h / 2

        ' Sposta la vista in modo che il centro del disegno sia al centro della PictureBox
        View.Origine = New PointF(
        cx - (PictureBox1.Width / 2.0F) / View.Zoom,
        cy - (PictureBox1.Height / 2.0F) / View.Zoom)

        RedrawViewport()
    End Sub

    Private Sub PictureBox1_MouseWheel(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseWheel
        If rendering Then Return   ' sicurezza
        If Execute Then Return

        Dim factor As Single = If(e.Delta > 0, 1.1F, 0.9F)
        ZoomAtPoint(e.Location, factor)
    End Sub

    Private Sub ZoomAtPoint(mouse As Point, factor As Single)
        Dim oldZoom = View.Zoom
        Dim newZoom = Math.Max(0.05F, Math.Min(20.0F, oldZoom * factor))
        If newZoom = oldZoom Then Return

        ' Punto mondo sotto il mouse
        Dim worldX = View.Origine.X + mouse.X / oldZoom
        Dim worldY = View.Origine.Y + mouse.Y / oldZoom

        View.Zoom = newZoom

        ' Mantieni fermo il punto sotto il mouse
        View.Origine = New PointF(
        worldX - mouse.X / newZoom,
        worldY - mouse.Y / newZoom
    )

        RedrawViewport()
    End Sub

    Private Sub ButtonZoomIn_Click(sender As Object, e As EventArgs) Handles ButtonZoomIn.Click
        ' Zoom centrato sul centro della PictureBox
        Dim center = New Point(PictureBox1.Width \ 2, PictureBox1.Height \ 2)
        ZoomAtPoint(center, ZOOM_STEP)
    End Sub

    Private Sub ButtonOut_Click(sender As Object, e As EventArgs) Handles ButtonOut.Click
        Dim center = New Point(PictureBox1.Width \ 2, PictureBox1.Height \ 2)
        ZoomAtPoint(center, 1.0F / ZOOM_STEP)
    End Sub

    'PAN o Spostamento
    Private Const PAN_STEP As Single = 100

    Private Sub Pan(dx As Single, dy As Single)
        View.Origine = New PointF(
            View.Origine.X + dx / View.Zoom,
            View.Origine.Y + dy / View.Zoom
        )
        RedrawViewport()
    End Sub

    Private Sub ButtonLeft_Click(sender As Object, e As EventArgs) Handles ButtonLeft.Click
        Pan(-PAN_STEP, 0)
    End Sub

    Private Sub ButtonRight_Click(sender As Object, e As EventArgs) Handles ButtonRight.Click
        Pan(PAN_STEP, 0)
    End Sub

    Private Sub ButtonUp_Click(sender As Object, e As EventArgs) Handles ButtonUp.Click
        Pan(0, -PAN_STEP)
    End Sub

    Private Sub ButtonDown_Click(sender As Object, e As EventArgs) Handles ButtonDown.Click
        Pan(0, PAN_STEP)
    End Sub

    Private Sub PictureBox1_DoubleClick(sender As Object, e As EventArgs) Handles PictureBox1.DoubleClick
        ' Apri un file script PaintRobot
        Dim ofd As New OpenFileDialog
        ofd.FileName = ""
        ofd.Filter = "Files di Testo|*.txt"
        ofd.Title = "Apri un script testo PaintRobot..."

        If ofd.ShowDialog(Me) = DialogResult.OK AndAlso ofd.FileName <> "" Then
            ' Normale
            'Dim newCommands = Interpreter.CaricaScript(ofd.FileName)
            FileScript = ofd.FileName

            ' Supporto MACRO
            Dim newCommands = Interpreter.CaricaScriptConMacro(ofd.FileName)

            If newCommands.Count = 0 Then Return

            If Comandi Is Nothing Then
                ' Disegno tuti i comandi dello script
                Comandi = Interpreter.CaricaComando("PULISCI;Bianco")
                Comandi.AddRange(newCommands)
            Else
                ' 🔹 Aggiungo i comandi alla coda senza cancellare History
                Comandi.AddRange(newCommands)
            End If

            ' Aggiunge i comandi alla History
            History.AddRange(Comandi)

            ' Linee Script
            Dim ComandiStringa As List(Of String) = Interpreter.CaricaLineeScript(ofd.FileName)
            HistoryString.Clear()
            HistoryString.AddRange(ComandiStringa)

            ' ListBox Comandi
            AggiornaListaComandiStringa()

            ' Init renderindex
            If renderIndex = -1 Then renderIndex = 0

            ' Imposta la progressBar
            numComandi = newCommands.Count - 1
            ProgressBar1.Minimum = 0
            ProgressBar1.Maximum = Comandi.Count
            ProgressBar1.Value = Math.Min(renderIndex, ProgressBar1.Maximum)

            StartRender()
        End If
    End Sub

    Private Function ZoomOrigin(zooom As Single, punto As PointF)
        View.Zoom = zooom

        Origine = New Point(punto.X, punto.Y)
        OrigineF = punto

        View.Origine = OrigineF
        Return Nothing
    End Function

    Private Sub ButtonOrigin_Click(sender As Object, e As EventArgs) Handles ButtonOrigin.Click
        'Vai a Origine
        ZoomOrigin(1.0, New PointF(0, 0))
        RedrawViewport()

        'Visibile
        ButtonOrigin.BackColor = Color.Green
        ButtonCentroMondo.BackColor = Color.White
    End Sub

    Private Sub ButtonCentroMondo_Click(sender As Object, e As EventArgs) Handles ButtonCentroMondo.Click
        'Vai al centro del mondo PaintRobot 5000,5000
        CenterViewOnPoint(New PointF(5000, 5000))
        RedrawViewport()

        'Visibile
        ButtonOrigin.BackColor = Color.White
        ButtonCentroMondo.BackColor = Color.Green
    End Sub

    Private Sub ButtonAdapt_Click(sender As Object, e As EventArgs) Handles ButtonAdapt.Click
        'Centra il disegno
        Dim minX As Single = Single.MaxValue
        Dim minY As Single = Single.MaxValue
        Dim maxX As Single = Single.MinValue
        Dim maxY As Single = Single.MinValue

        For Each cmd In Comandi
            For Each param In cmd.Parametri
                If param.Contains(",") Then
                    Dim xy = param.Split(","c)
                    Dim x = CSng(xy(0))
                    Dim y = CSng(xy(1))

                    minX = Math.Min(minX, x)
                    minY = Math.Min(minY, y)
                    maxX = Math.Max(maxX, x)
                    maxY = Math.Max(maxY, y)
                End If
            Next
        Next

        CenterOnDrawing(New PointF(minX, minY), New PointF(maxX, maxY))
    End Sub

    Private Sub CenterViewOnPoint(p As PointF)
        View.Origine = New PointF(
        p.X - (PictureBox1.Width / 2.0F) / View.Zoom,
        p.Y - (PictureBox1.Height / 2.0F) / View.Zoom
    )
        RedrawViewport()
    End Sub


    'PAN mouse
    Private isPanning As Boolean = False
    Private panStartMouse As Point
    Private panStartOrigin As PointF

    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
        If rendering Then Return 'Blocca il pan mentre disegni

        If e.Button = MouseButtons.Middle Then
            isPanning = True
            panStartMouse = e.Location
            panStartOrigin = View.Origine
            PictureBox1.Cursor = Cursors.Hand
        End If
    End Sub

    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove
        ' --- PAN solo se tasto centrale premuto ---
        If isPanning AndAlso Not rendering Then
            Dim dxScreen = e.X - panStartMouse.X
            Dim dyScreen = e.Y - panStartMouse.Y

            ' conversione schermo → mondo
            Dim dxWorld = dxScreen / View.Zoom
            Dim dyWorld = dyScreen / View.Zoom

            View.Origine = New PointF(
            panStartOrigin.X - dxWorld,
            panStartOrigin.Y - dyWorld
        )

            RedrawViewport()
        End If

        ' --- Aggiorna sempre la label ---
        Dim w = ScreenToWorld(e.Location)
        Dim LabelX As Integer = e.X + OFFSET_X
        Dim LabelY As Integer = e.Y + OFFSET_Y
        LabelCoord.Text = $"X: {w.X:0.00}   Y: {w.Y:0.00}   Zoom: {View.Zoom:0.00}x"
        If LabelX > 60 AndAlso LabelY > 60 Then
            LabelCoord.Location = New Point(LabelX, LabelY)
        Else
            'Rimane dove sta
        End If
        LabelCoord.BringToFront()
        LabelCoord.Visible = True
    End Sub

    Private Sub PictureBox1_MouseUp(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseUp
        If e.Button = MouseButtons.Middle Then
            isPanning = False
            PictureBox1.Cursor = Cursors.Default
        End If
    End Sub

    'Coordinate
    Private lastCoord As PointF

    Private Function ScreenToWorld(p As Point) As PointF
        Return New PointF(View.Origine.X + (p.X - WorldOffset.X) / View.Zoom, View.Origine.Y + (p.Y - WorldOffset.Y) / View.Zoom)
    End Function

    'Mouse Label
    Private Const OFFSET_X As Integer = 20
    Private Const OFFSET_Y As Integer = 20

    'Autocomplete Sintassi
    Private suppressTextChanged As Boolean = False

    Private Sub TextBoxCommands_TextChanged(sender As Object, e As EventArgs) Handles TextBoxCommands.TextChanged
        If suppressTextChanged Then Return

        Dim txt As String = TextBoxCommands.Text.ToUpper()

        Dim match = RobotDrawer.ComandiSintassi.Keys.FirstOrDefault(Function(k) k.StartsWith(txt))

        ' Autocompletamento o Suggerimento del comando
        If match IsNot Nothing AndAlso txt.Length > 0 Then
            Dim suggerimento = RobotDrawer.ComandiSintassi(match)

            suppressTextChanged = True
            TextBoxCommands.Text = suggerimento
            TextBoxCommands.SelectionStart = txt.Length
            TextBoxCommands.SelectionLength = suggerimento.Length - txt.Length
            suppressTextChanged = False
        End If
    End Sub


    Private Sub TextBoxCommands_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBoxCommands.KeyDown
        If e.KeyCode = Keys.Tab Then
            ' Accetta il completamento: porta il caret alla fine
            TextBoxCommands.SelectionStart = TextBoxCommands.Text.Length
            TextBoxCommands.SelectionLength = 0
            e.Handled = True
        End If
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True ' evita il beep della TextBox
            ButtonCmd.PerformClick()  ' simula click sul pulsante Esegui
        End If
    End Sub

    'Salva
    Private Sub SalvaBitmapPercorso(bmp As Bitmap, path As String, formato As String)
        Dim imgFormat As Imaging.ImageFormat

        Select Case formato.ToUpper()
            Case "PNG" : imgFormat = Imaging.ImageFormat.Png
            Case "BMP" : imgFormat = Imaging.ImageFormat.Bmp
            Case "JPG", "JPEG" : imgFormat = Imaging.ImageFormat.Jpeg
            Case Else : imgFormat = Imaging.ImageFormat.Png
        End Select

        Try
            bmp.Save(path, imgFormat)
            Debug.WriteLine("Bitmap salvata: " & path)
        Catch ex As Exception
            ' Se fallisce, salva su Desktop
            Try
                Dim desk = My.Computer.FileSystem.SpecialDirectories.Desktop
                Dim newPath = System.IO.Path.Combine(desk, System.IO.Path.GetFileName(path))
                bmp.Save(newPath, imgFormat)
                Debug.WriteLine("Bitmap salvata su Desktop: " & newPath)
            Catch innerEx As Exception
                Debug.WriteLine("Errore salvataggio: " & innerEx.Message)
            End Try
        End Try
    End Sub

    Private Sub ButtonSavePaintRobot_Click(sender As Object, e As EventArgs) Handles ButtonSavePaintRobot.Click
        ' Percorso e formato di default
        Dim sfd As New SaveFileDialog With {
    .Title = "Salva l'immagine PaintRobot...",
    .FileName = "PaintRobot.png",
    .Filter = "Files PNG|*.png|Files Bitmap|*.bmp|Files Jpeg|*.jpg"
}

        If sfd.ShowDialog(Me) = DialogResult.OK AndAlso sfd.FileName <> "" Then
            Dim percorso As String = sfd.FileName
            Dim estensione As String = System.IO.Path.GetExtension(percorso).TrimStart("."c).ToUpperInvariant()

            Select Case estensione
                Case "PNG", "JPG", "JPEG", "BMP"
                    SalvaBitmapPercorso(PaintRobotBMP, percorso, estensione)
                Case Else
                    ' Se non riconosciuta, salva come PNG
                    SalvaBitmapPercorso(PaintRobotBMP, percorso, "PNG")
            End Select

            If System.IO.File.Exists(percorso) AndAlso FileLen(percorso) > 0 Then
                MsgBox("File salvato in " & percorso,, "\\(*_*)//  PaintRobot")
            End If
        End If
    End Sub


    'TEST
    ' StopWatch misurazione precisa
    Dim swRender As New Stopwatch()

    Private Sub ButtonTest_Click(sender As Object, e As EventArgs) Handles ButtonTest.Click
        'Test - creazione di script
        Dim testScriptCommands As List(Of String) = New List(Of String)

        'Test attivo
        inTest = True

        ' Random
        Dim r As New Random

        Dim Nmax As Integer = r.Next(1, PictureBox1.Height - 10)
        'Comandi casuali
        For Linea As Integer = 0 To 1000
            testScriptCommands.Add("LINEA;" & r.Next(0, Nmax).ToString & ",10;" & r.Next(0, Nmax).ToString & "," & r.Next(0, Nmax).ToString & ";Blu;2")
        Next

        Nmax = r.Next(1, PictureBox1.Height - 10)
        'Comandi casuali
        Dim tipo = ""
        For Rett As Integer = 0 To 1000
            If Rett Mod 2 = 0 Then
                tipo = "PIENO"
            Else
                tipo = "VUOTO"
            End If
            testScriptCommands.Add("RETT;50," & r.Next(0, Nmax).ToString & ";" & r.Next(0, Nmax).ToString & "," & r.Next(0, Nmax).ToString & ";Nero;" & tipo & ";1")
        Next

        Nmax = r.Next(10, PictureBox1.Height - 10)
        'Comandi casuali
        For triang As Integer = 0 To 1000
            testScriptCommands.Add("TRIANG;" & r.Next(0, Nmax).ToString & ",50;" & r.Next(0, Nmax).ToString & "," & r.Next(0, Nmax).ToString & ";" & r.Next(0, Nmax).ToString & ",340;Rosso;VUOTO;1")
        Next

        Nmax = r.Next(60, PictureBox1.Height - 10)
        'Comandi casuali
        For splines As Integer = 0 To 1000
            testScriptCommands.Add("SPLINE;" & r.Next(0, Nmax).ToString & ",15;" & r.Next(0, Nmax).ToString & ",500;1000," & r.Next(0, Nmax).ToString & ";Giallo;1")
        Next

        ' Nome del file di test script
        Dim testFile As String = Path.Combine(Application.StartupPath, "test.txt")

        ' Scarica i comandi nel file di test script
        Dim script As String = "#Test Script PainRobot del " & Now.ToLongDateString & vbCrLf
        For i = 0 To testScriptCommands.Count - 1
            script = script & testScriptCommands(i) & vbCrLf
        Next
        script = script & "#Fine"

        ' Salva il file script
        If script <> "" Then
            System.IO.File.WriteAllText(testFile, script)
        End If

        ' Esegue lo script di comandi casuali
        If System.IO.File.Exists(testFile) Then
            If FileLen(testFile) > 0 Then
                ' Disegno tuti i comandi dello script caricando con
                ' CaricaScript perche non serve MACRO
                Comandi = Interpreter.CaricaScript(testFile)

                History.Clear()
                History.AddRange(Comandi)

                numComandi = Comandi.Count - 1
                ProgressBar1.Minimum = 0
                ProgressBar1.Maximum = numComandi
                ProgressBar1.Value = 0

                StartRender()
            End If
        End If
    End Sub

    'HELP
    Private Sub ButtonHelp_Click(sender As Object, e As EventArgs) Handles ButtonHelp.Click
        'Help di PaintRobot
        Dim FormHelp As New Form2
        FormHelp.ShowDialog()
    End Sub

    'HISTORY
    Private Sub ButtonEditHistory_Click(sender As Object, e As EventArgs) Handles ButtonEditHistory.Click
        ' Apre la finestra fi History
        If History Is Nothing OrElse History.Count = 0 Then Return

        Using historyForm As New HistoryEditorForm(History, HistoryString)
            If historyForm.ShowDialog() = DialogResult.OK Then

                History = historyForm.EditedHistory
                HistoryString = historyForm.EditedHistoryString

                'Ridisegna tutto
                RenderAllHistory()
            End If
        End Using
    End Sub


    ' Lista Comandi Stringa
    Private Sub LstComandi_DrawItem(sender As Object, e As DrawItemEventArgs) Handles LstComandi.DrawItem
        If e.Index < 0 Then Return

        Dim lb As ListBox = DirectCast(sender, ListBox)
        Dim text As String = lb.Items(e.Index).ToString()

        ' Colore riga: verde se selezionata, bianco se no
        Dim backColor As Color
        Dim foreColor As Color = Color.White

        If (e.State And DrawItemState.Selected) = DrawItemState.Selected Then
            backColor = Color.Green
        Else
            backColor = Color.Black
        End If

        ' Disegna lo sfondo
        Using b As New SolidBrush(backColor)
            e.Graphics.FillRectangle(b, e.Bounds)
        End Using

        ' Disegna il testo
        Using f As New SolidBrush(foreColor)
            e.Graphics.DrawString(text, lb.Font, f, e.Bounds.Location)
        End Using

        ' Disegna il focus rectangle (se necessario)
        e.DrawFocusRectangle()
    End Sub

    ' ATTIVA DISATTIVA LISTA COMANDI STRINGA
    Private ListaComandiStringa As Boolean = True
    Private Sub ButtonAttivaListBox_Click(sender As Object, e As EventArgs) Handles ButtonAttivaListBox.Click
        ListaComandiStringa = Not ListaComandiStringa
        If ListaComandiStringa Then
            ButtonAttivaListBox.BackColor = Color.DarkOrchid
        Else
            ButtonAttivaListBox.BackColor = Color.Black
        End If
    End Sub

    'SCRIPT
    Private Sub ButtonScript_Click(sender As Object, e As EventArgs) Handles ButtonScript.Click
        If HistoryString Is Nothing OrElse HistoryString.Count = 0 Then
            MessageBox.Show("Nessun comando da salvare.")
            Return
        End If

        ' Mostra la finestra per Titolo e Autore
        Using formInput As New FormTitoloAutore()
            If formInput.ShowDialog() <> DialogResult.OK Then
                ' Se l'utente annulla
                Return
            End If

            Dim titoloDisegno As String = formInput.Titolo
            Dim autore As String = formInput.Autore

            ' Salva file
            Using sfd As New SaveFileDialog()
                sfd.Filter = "Script PaintRobot (*.txt)|*.txt|Tutti i file (*.*)|*.*"
                sfd.Title = "Salva script"
                sfd.FileName = "PaintRobotScript.txt"

                If sfd.ShowDialog() = DialogResult.OK Then
                    Dim dataOra As String = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                    Dim scriptFinale As New List(Of String)

                    scriptFinale.Add("# ===============================")
                    scriptFinale.Add("# Titolo : " & titoloDisegno)
                    scriptFinale.Add("# Autore : " & autore)
                    scriptFinale.Add("# Data   : " & dataOra)
                    scriptFinale.Add("# PaintRobot \\(*_*)//")
                    scriptFinale.Add("# ===============================")
                    scriptFinale.Add("")

                    scriptFinale.AddRange(HistoryString)
                    IO.File.WriteAllLines(sfd.FileName, scriptFinale)
                End If
            End Using
        End Using
    End Sub

    'ACCESSO VOCALE
    Private Shared ReadOnly SymbolMap As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
    {"punto e virgola", ";"},
    {"punto virgola", ";"},
    {"virgola", ","},
    {"punto", ","},
    {"spazio", ","},
    {"due punti", ":"},
    {"duepunti", ":"},
    {"e", ","},
    {"meno", "-"},
    {"più", "+"}
}

    Public Function WordToNumber(text As String) As String
        Dim numberWords = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase) From {
        {"zero", 0}, {"uno", 1}, {"due", 2}, {"tre", 3}, {"quattro", 4},
        {"cinque", 5}, {"sei", 6}, {"sette", 7}, {"otto", 8}, {"nove", 9},
        {"dieci", 10}, {"undici", 11}, {"dodici", 12}, {"tredici", 13},
        {"quattordici", 14}, {"quindici", 15}, {"sedici", 16}, {"diciassette", 17},
        {"diciotto", 18}, {"diciannove", 19}, {"venti", 20}, {"trenta", 30},
        {"quaranta", 40}, {"cinquanta", 50}, {"sessanta", 60}, {"settanta", 70},
        {"ottanta", 80}, {"novanta", 90}, {"cento", 100}, {"mille", 1000}
    }

        If numberWords.ContainsKey(text) Then
            Return numberWords(text).ToString()
        End If

        Return text
    End Function

    Public Function ConvertItalianNumberWordsToNumber(text As String) As String
        Dim units = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase) From {
        {"zero", 0}, {"uno", 1}, {"una", 1}, {"due", 2}, {"tre", 3}, {"quattro", 4},
        {"cinque", 5}, {"sei", 6}, {"sette", 7}, {"otto", 8}, {"nove", 9}
    }

        Dim teens = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase) From {
        {"dieci", 10}, {"undici", 11}, {"dodici", 12}, {"tredici", 13}, {"quattordici", 14},
        {"quindici", 15}, {"sedici", 16}, {"diciassette", 17}, {"diciotto", 18}, {"diciannove", 19}
    }

        Dim tens = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase) From {
        {"venti", 20}, {"trenta", 30}, {"quaranta", 40}, {"cinquanta", 50},
        {"sessanta", 60}, {"settanta", 70}, {"ottanta", 80}, {"novanta", 90}
    }

        Dim hundreds = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase) From {
        {"cento", 100}
    }

        Dim thousands = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase) From {
        {"mille", 1000}, {"mila", 1000}
    }

        ' Spezza parole composte (ventitré → venti tre)
        text = Regex.Replace(text, "([a-z]+)(tre|uno|otto)", "$1 $2", RegexOptions.IgnoreCase)

        Dim words = text.Split(" "c)
        Dim total As Integer = 0
        Dim current As Integer = 0

        For Each w In words
            If units.ContainsKey(w) Then
                current += units(w)
            ElseIf teens.ContainsKey(w) Then
                current += teens(w)
            ElseIf tens.ContainsKey(w) Then
                current += tens(w)
            ElseIf w.StartsWith("cent") Then
                current += 100
            ElseIf thousands.ContainsKey(w) Then
                current = If(current = 0, 1, current)
                total += current * 1000
                current = 0
            Else
                ' Non è un numero → restituisci testo originale
                Return text
            End If
        Next

        total += current
        Return total.ToString()
    End Function

    Public Function NormalizeSpeechInput(input As String) As String
        'Normalizza le parola con i caratteri corretti.
        Dim result As String = input

        ' Sostituzione simboli
        For Each kvp In SymbolMap
            result = Regex.Replace(
        result,
        "\b" & Regex.Escape(kvp.Key) & "\b",
        kvp.Value,
        RegexOptions.IgnoreCase)
        Next


        ' Conversione numeri parola → cifra (con supporto per negativi)
        Dim words = result.Split(" "c)
        Dim outputWords As New List(Of String)
        Dim i As Integer = 0

        While i < words.Length
            If words(i).Equals("meno", StringComparison.OrdinalIgnoreCase) Then
                ' Numero negativo
                If i + 1 < words.Length Then
                    Dim converted = ConvertItalianNumberWordsToNumber(words(i + 1))
                    outputWords.Add("-" & converted)
                    i += 2
                Else
                    outputWords.Add(words(i))
                    i += 1
                End If
            Else
                ' Numero normale
                Dim converted = ConvertItalianNumberWordsToNumber(words(i))
                outputWords.Add(converted)
                i += 1
            End If
        End While

        'Aggiunge parole
        result = String.Join(" ", outputWords)

        ' 🔥 Rimuove spazi dopo simboli ; , : . -
        result = Regex.Replace(result, "([;,:.\-])\s+", "$1")

        Return result
    End Function

    'RIAVVIA
    Private Sub ButtonRiavvia_Click(sender As Object, e As EventArgs) Handles ButtonRiavvia.Click
        Application.Restart()
    End Sub

    'COLORI
    Private Sub ButtonColors_Click(sender As Object, e As EventArgs) Handles ButtonColors.Click
        Dim larghezzaTotale As Integer = 0
        For Each col As ColumnHeader In ListView1.Columns
            larghezzaTotale += col.Width
        Next
        ListView1.Size = New Size(larghezzaTotale + 40, Me.Height - 200)
        ListView1.Visible = Not ListView1.Visible
    End Sub

    Private Sub CaricaColori()
        ListView1.BeginUpdate()
        ListView1.Items.Clear()

        For Each kv In RobotDrawer.ColoriItaliani.OrderBy(Function(c) c.Key)
            Dim item As New ListViewItem(kv.Key)
            item.SubItems.Add($"{kv.Value.R}, {kv.Value.G}, {kv.Value.B}")
            item.Tag = kv.Value   ' memorizzo il colore
            ListView1.Items.Add(item)
        Next

        ' Aggiorna intestazione con count
        ListView1.Columns(0).Text = $"Colori ({ListView1.Items.Count})"

        ListView1.EndUpdate()
    End Sub

    Private Sub ListView1_DrawColumnHeader(sender As Object, e As DrawListViewColumnHeaderEventArgs) Handles ListView1.DrawColumnHeader
        e.DrawDefault = True
    End Sub

    Private Sub ListView1_DrawSubItem(sender As Object, e As DrawListViewSubItemEventArgs) Handles ListView1.DrawSubItem
        If e.ColumnIndex = 0 Then
            Dim colore As Color = CType(e.Item.Tag, Color)

            Using b As New SolidBrush(colore)
                e.Graphics.FillRectangle(b,
                    e.Bounds.Left + 3,
                    e.Bounds.Top + 3,
                    20,
                    e.Bounds.Height - 6)
            End Using

            e.Graphics.DrawString(
                e.SubItem.Text,
                ListView1.Font,
                Brushes.Black,
                e.Bounds.Left + 28,
                e.Bounds.Top + 3)
        Else
            e.DrawText()
        End If
    End Sub

    'SVG
    Private Sub ButtonSVG_Click(sender As Object, e As EventArgs) Handles ButtonSVG.Click
        Dim sfd As New SaveFileDialog With {
    .Title = "Salva SVG PaintRobot...",
    .FileName = "PaintRobot.svg",
    .Filter = "Files Disegno SVG|*.svg"
}

        If sfd.ShowDialog(Me) = DialogResult.OK AndAlso sfd.FileName <> "" Then
            Dim percorso As String = sfd.FileName
            Dim estensione As String = System.IO.Path.GetExtension(percorso).TrimStart("."c).ToUpperInvariant()

            Dim svg = New SvgDrawer().GeneraSVG(HistoryString, LarghezzaPaginaPixel, AltezzaPaginaPixel)
            System.IO.File.WriteAllText(percorso, svg)

            If System.IO.File.Exists(percorso) AndAlso FileLen(percorso) > 0 Then
                MsgBox("File salvato in " & percorso,, "\\(*_*)//  PaintRobot")
            End If
        End If
    End Sub

    'TASTO DESTRO
    ' Devi associare nella proprietà della Picturebox1 "ContextMenuStrip" il ContextMenuOptions.

    Private Sub AdattaAlSchermo()
        ' 1. Calcola bounding box
        Dim minX As Single = Single.MaxValue
        Dim minY As Single = Single.MaxValue
        Dim maxX As Single = Single.MinValue
        Dim maxY As Single = Single.MinValue

        If Comandi IsNot Nothing Then
            For Each cmd In Comandi
                For Each param In cmd.Parametri
                    If param.Contains(",") Then
                        Dim xy = param.Split(","c)
                        Dim x = CSng(xy(0))
                        Dim y = CSng(xy(1))

                        minX = Math.Min(minX, x)
                        minY = Math.Min(minY, y)
                        maxX = Math.Max(maxX, x)
                        maxY = Math.Max(maxY, y)
                    End If
                Next
            Next

            ' 2. Dimensioni del disegno
            Dim w = maxX - minX
            Dim h = maxY - minY

            ' 3. Calcola zoom per farlo entrare
            Dim zoomX = PictureBox1.Width / w
            Dim zoomY = PictureBox1.Height / h
            Dim zoomFinale = Math.Min(zoomX, zoomY) * 0.9F ' margine 10%

            View.Zoom = zoomFinale

            ' 4. Centra il disegno
            Dim cx = minX + w / 2
            Dim cy = minY + h / 2

            View.Origine = New PointF(
            cx - (PictureBox1.Width / 2.0F) / View.Zoom,
            cy - (PictureBox1.Height / 2.0F) / View.Zoom
        )

            RedrawViewport()

        End If
    End Sub

    Private Sub AdattaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AdattaToolStripMenuItem.Click
        AdattaAlSchermo()
    End Sub

    'Crea i righelli
    Private Righelli As Boolean = False
    Private HorizontalRuler As TransparentRuler
    Private VerticalRuler As TransparentRuler
    Private rulerBackColor As Color = Color.FromArgb(100, 44, 44, 250)
    ' Drag
    Private draggingRuler As Boolean = False
    Private dragStartPoint As Point
    Private draggedRuler As PictureBox = Nothing

    Private Sub ButtonRighe_Click(sender As Object, e As EventArgs) Handles ButtonRighe.Click
        If Righelli Then
            ' Rimuovi righelli
            If HorizontalRuler IsNot Nothing Then
                Me.Controls.Remove(HorizontalRuler)
                HorizontalRuler.Dispose()
            End If
            If VerticalRuler IsNot Nothing Then
                Me.Controls.Remove(VerticalRuler)
                VerticalRuler.Dispose()
            End If

            Righelli = False
            ButtonRighe.BackColor = Color.Black
        Else
            ' Aggiungi righelli
            Me.AllowTransparency = True

            HorizontalRuler = New TransparentRuler() With {
    .Width = PictureBox1.Width,
    .Height = 30,
    .Parent = PictureBox1,
    .BackColor = Color.Transparent,
    .Location = New Point(60, 60)
}
            VerticalRuler = New TransparentRuler() With {
    .Height = PictureBox1.Height,
    .Width = 30,
    .Parent = PictureBox1,
    .BackColor = Color.Transparent,
    .Location = New Point(60, 60)
}

            ' Orizzontale
            AddHandler HorizontalRuler.MouseDown, AddressOf Ruler_MouseDown
            AddHandler HorizontalRuler.MouseMove, AddressOf Ruler_MouseMove
            AddHandler HorizontalRuler.MouseUp, AddressOf Ruler_MouseUp

            ' Verticale
            AddHandler VerticalRuler.MouseDown, AddressOf Ruler_MouseDown
            AddHandler VerticalRuler.MouseMove, AddressOf Ruler_MouseMove
            AddHandler VerticalRuler.MouseUp, AddressOf Ruler_MouseUp

            Me.Controls.Add(VerticalRuler)
            Me.Controls.Add(HorizontalRuler)

            VerticalRuler.BringToFront()
            HorizontalRuler.BringToFront()

            With HorizontalRuler
                .Parent = PictureBox1
                .BackColor = rulerBackColor
            End With

            With VerticalRuler
                .Parent = PictureBox1
                .BackColor = rulerBackColor
            End With

            Righelli = True
            ButtonRighe.BackColor = Color.DarkOrchid
        End If
    End Sub

    Private Sub Ruler_MouseDown(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            draggingRuler = True
            draggedRuler = CType(sender, PictureBox)
            dragStartPoint = e.Location
            draggedRuler.BringToFront()
        End If
    End Sub

    Private Sub Ruler_MouseMove(sender As Object, e As MouseEventArgs)
        If draggingRuler AndAlso draggedRuler IsNot Nothing Then
            Dim newX As Integer = draggedRuler.Left + (e.X - dragStartPoint.X)
            Dim newY As Integer = draggedRuler.Top + (e.Y - dragStartPoint.Y)
            draggedRuler.Location = New Point(newX, newY)
            draggedRuler.Invalidate()
        End If
    End Sub

    Private Sub Ruler_MouseUp(sender As Object, e As MouseEventArgs)
        draggingRuler = False
        draggedRuler = Nothing
    End Sub


End Class

Public Class TransparentRuler
    Inherits PictureBox

    Public Sub New()
        Me.SetStyle(ControlStyles.SupportsTransparentBackColor Or ControlStyles.OptimizedDoubleBuffer, True)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        ' Disegno tacche
        If Me.Width > Me.Height Then
            ' Orizzontale
            Dim yCenter = Me.Height \ 2
            For i As Integer = 0 To Me.Width Step 10
                Dim h = If(i Mod 50 = 0, yCenter, yCenter \ 2)
                e.Graphics.DrawLine(Pens.Black, i, 0, i, h)
                If i Mod 50 = 0 Then e.Graphics.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Black, i + 2, h)
            Next
        Else
            ' Verticale
            Dim xCenter = Me.Width \ 2
            For i As Integer = 0 To Me.Height Step 10
                Dim w = If(i Mod 50 = 0, xCenter, xCenter \ 2)
                e.Graphics.DrawLine(Pens.Black, 0, i, w, i)
                If i Mod 50 = 0 Then e.Graphics.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Black, w + 2, i)
            Next
        End If

        MyBase.OnPaint(e)
    End Sub
End Class


Public Class RobotCommand
    Public Property Tipo As String
    Public Property Parametri As List(Of String)

    Public Sub New(tipo As String, parametri As List(Of String))
        Me.Tipo = tipo
        Me.Parametri = parametri
    End Sub
End Class

Public Class Viewport
    Public Property Origine As PointF = New PointF(0, 0)
    Public Property Zoom As Single = 1.0F
End Class

Public Class RobotInterpreter
    'Responsabile di : 
    '✔️ leggere il TXT
    '✔️ ignorare commenti
    '✔️ creare i comandi

    Public Function CaricaScript(percorso As String) As List(Of RobotCommand)
        ' Versione 2.0 senza le macro
        Dim comandi As New List(Of RobotCommand)

        For Each riga In IO.File.ReadAllLines(percorso)
            Dim line = riga.Trim()

            ' Commento
            If line = "" OrElse line.StartsWith("#") Then Continue For

            If line = "" OrElse line.StartsWith("[") Then
                'Aggiungi livello [+NOMELIVELLO]
                'Rimuovi livello  [-NOMELIVELLO]
                'Rinomina livello [NOMELIVELLO;NUOVONOMELIVELLO]
            End If

            Dim parti = line.Split(";"c)
            Dim tipo = parti(0).ToUpper()
            Dim parametri = parti.Skip(1).ToList()

            comandi.Add(New RobotCommand(tipo, parametri))
        Next

        Return comandi
    End Function

    Public Function CaricaLineeScript(percorso As String) As List(Of String)
        Dim comandiStringa As New List(Of String)

        For Each riga In IO.File.ReadAllLines(percorso)
            Dim line = riga.Trim()

            ' # Commento, [ Aggiunge livello
            If line = "" OrElse line.StartsWith("#") Then Continue For

            comandiStringa.Add(line)
        Next

        Return comandiStringa
    End Function

    Public Function CaricaComando(comando As String) As List(Of RobotCommand)
        Dim comandi As New List(Of RobotCommand)

        Dim line = comando.Trim()

        ' Commento
        If line = "" OrElse line.StartsWith("#") Then
            Return Nothing
        End If

        Dim parti = line.Split(";"c)
        Dim tipo = parti(0).ToUpper()
        Dim parametri = parti.Skip(1).ToList()

        comandi.Add(New RobotCommand(tipo, parametri))

        Return comandi
    End Function

    Public Function CaricaComandiMultipli(input As String) As List(Of RobotCommand)
        ' Per comandi multipli da linea singola
        Dim lista As New List(Of RobotCommand)

        If String.IsNullOrWhiteSpace(input) Then Return lista

        ' separa per +
        Dim blocchi = input.Split("+"c)

        For Each blocco In blocchi

            Dim line = blocco.Trim()

            ' Commento
            If line = "" OrElse line.StartsWith("#") Then Continue For

            Dim parti = line.Split(";"c)
            Dim tipo = parti(0).ToUpper()
            Dim parametri = parti.Skip(1).ToList()

            lista.Add(New RobotCommand(tipo, parametri))
        Next

        Return lista
    End Function

    Public Function CaricaComandiStringaMultipli(input As String) As List(Of String)
        ' Per comandi multipli da linea singola
        Dim lista As New List(Of String)

        If String.IsNullOrWhiteSpace(input) Then Return lista

        ' separa per +
        Dim blocchi = input.Split("+"c)

        For Each blocco In blocchi

            Dim line = blocco.Trim()

            ' Commento
            If line = "" OrElse line.StartsWith("#") Then Continue For

            lista.Add(line)
        Next

        Return lista
    End Function

    'MACRO
    Private Macros As New Dictionary(Of String, MacroDef)

    Public Function CaricaScriptConMacro(percorso As String) As List(Of RobotCommand)
        ' Carica lo script
        Dim righe = IO.File.ReadAllLines(percorso).
                Select(Function(r) r.Trim()).
                Where(Function(r) r <> "" AndAlso Not r.StartsWith("#")).
                ToList()

        Dim righeEspanse = EspandiMacro(righe)

        Dim comandi As New List(Of RobotCommand)
        For Each line In righeEspanse
            Dim parti = line.Split(";"c)
            Dim tipo = parti(0).ToUpper()
            Dim parametri = parti.Skip(1).ToList()
            comandi.Add(New RobotCommand(tipo, parametri))
        Next

        Return comandi
    End Function

    Private Function EspandiMacro(righe As List(Of String)) As List(Of String)
        ' Espande il contenuto dello script con macro, FOR e INCLUDE
        Dim output As New List(Of String)
        Dim i As Integer = 0

        While i < righe.Count
            Dim line = righe(i).Trim()

            If line = "" OrElse line.StartsWith("#") Then
                i += 1
                Continue While
            End If

            ' --- INCLUDE ---
            If line.StartsWith("INCLUDE;", StringComparison.OrdinalIgnoreCase) Then
                Dim percorsoIncluso = line.Split(";"c)(1).Trim()
                If IO.File.Exists(percorsoIncluso) Then
                    Dim righeInclude = IO.File.ReadAllLines(percorsoIncluso).
                        Select(Function(r) r.Trim()).
                        Where(Function(r) r <> "" AndAlso Not r.StartsWith("#")).
                        ToList()
                    ' Espandi anche eventuali macro/FOR nel file incluso
                    output.AddRange(EspandiMacro(righeInclude))
                Else
                    Debug.WriteLine($"INCLUDE file non trovato: {percorsoIncluso}")
                End If

                i += 1
                Continue While
            End If

            ' --- DEFINE Macro ---
            If line.StartsWith("DEFINE;", StringComparison.OrdinalIgnoreCase) Then
                Dim parti = line.Split(";"c)
                Dim nomeMacro = parti(1).ToUpper()
                Dim parametri = parti.Skip(2).Select(Function(p) p.Trim()).ToList()

                If Macros.ContainsKey(nomeMacro) Then
                    'Aggiunge un numero casuale alla marco per distinguerla e non perderla
                    If Macros.ContainsKey(nomeMacro) Then
                        Dim r As New Random
                        Dim newNomeMacro As String = ""
                        Do
                            newNomeMacro = nomeMacro & r.Next(1, 9999).ToString
                        Loop Until nomeMacro <> newNomeMacro
                        ' Assegna il nuovo nome alla macro
                        nomeMacro = newNomeMacro
                    End If
                End If

                Dim corpo As New List(Of String)
                i += 1

                While i < righe.Count AndAlso righe(i).ToUpper() <> "END"
                    corpo.Add(righe(i))
                    i += 1
                End While

                Macros(nomeMacro) = New MacroDef With {
                .Nome = nomeMacro,
                .Parametri = parametri,
                .Corpo = corpo
            }

                i += 1 ' Salta l'END
                Continue While
            End If

            ' --- FOR ---
            If line.StartsWith("FOR;", StringComparison.OrdinalIgnoreCase) Then
                output.AddRange(EspandiFor(righe, i))
                ' EspandiFor aggiorna i fino all'END del FOR
                Continue While
            End If

            ' --- Riga normale o chiamata macro ---
            output.AddRange(EspandiRiga(line))

            i += 1
        End While

        Return output
    End Function

    Private Function EspandiRiga(line As String) As List(Of String)
        ' Espande il contenuto della riga
        Dim parti = line.Split(";"c)
        Dim nome = parti(0).ToUpper()

        If Not Macros.ContainsKey(nome) Then
            Return New List(Of String) From {line}
        End If

        Dim macro = Macros(nome)
        Dim valori = parti.Skip(1).ToList()

        If valori.Count <> macro.Parametri.Count Then
            Throw New Exception($"Macro {nome}: numero parametri errato")
        End If

        Dim map As New Dictionary(Of String, String)
        For i = 0 To macro.Parametri.Count - 1
            map(macro.Parametri(i)) = valori(i)
        Next

        Dim espanse As New List(Of String)

        For Each riga In macro.Corpo
            espanse.Add(SostituisciParametriMACRO(riga, map))
        Next

        Return espanse
    End Function

    Private Function SostituisciParametriMACRO(riga As String, vars As Dictionary(Of String, String)) As String

        Dim risultato = riga

        For Each kv In vars
            risultato = risultato.Replace("{" & kv.Key & "}", kv.Value)
        Next

        ' Espressioni tipo {X+10}
        Dim rx As New Regex("\{([^}]+)\}")
        risultato = rx.Replace(risultato, Function(m)
                                              Return EvalExpr(m.Groups(1).Value, vars)
                                          End Function)

        Return risultato
    End Function

    Private Function EvalExpr(expr As String, vars As Dictionary(Of String, String)) As String

        Dim e = expr
        For Each kv In vars
            e = e.Replace(kv.Key, kv.Value)
        Next

        Dim dt As New DataTable()
        Dim v = dt.Compute(e, "")
        Return Convert.ToString(v, Globalization.CultureInfo.InvariantCulture)
    End Function

    'FOR
    Private Function EspandiFor(righe As List(Of String), ByRef i As Integer) As List(Of String)

        Dim parti = righe(i).Split(";"c)
        Dim varName = parti(1)
        Dim startVal = Double.Parse(parti(2), CultureInfo.InvariantCulture)
        Dim endVal = Double.Parse(parti(3), CultureInfo.InvariantCulture)
        Dim stepVal = Double.Parse(parti(4), CultureInfo.InvariantCulture)

        If stepVal = 0 Then
            Debug.WriteLine($"FOR {varName} saltato: step = 0")
            Return New List(Of String) 'ritorna vuoto e salta il ciclo
        End If


        Dim corpo As New List(Of String)
        i += 1

        While i < righe.Count AndAlso righe(i).ToUpper() <> "END"
            corpo.Add(righe(i))
            i += 1
        End While

        Dim espanse As New List(Of String)

        Dim v = startVal
        If stepVal > 0 Then
            While v <= endVal
                espanse.AddRange(EspandiCorpoFor(corpo, varName, v))
                v += stepVal
            End While
        Else
            While v >= endVal
                espanse.AddRange(EspandiCorpoFor(corpo, varName, v))
                v += stepVal
            End While
        End If

        Return espanse
    End Function

    ' Espande il corpo di un FOR
    Private Function EspandiCorpoFor(corpo As List(Of String), varName As String, value As Double) As List(Of String)

        Dim vars As New Dictionary(Of String, String) From {
        {varName, value.ToString(CultureInfo.InvariantCulture)}
    }

        Dim righeEspanse As New List(Of String)

        For Each r In corpo
            ' 1️⃣ Sostituisco le variabili FOR
            Dim rSostituita = SostituisciParametriFOR(r, vars)

            ' 2️⃣ Controllo se è una macro da espandere
            Dim righeMacro As List(Of String) = Nothing
            If rSostituita.StartsWith("DEFINE;", StringComparison.OrdinalIgnoreCase) OrElse
           Macros.ContainsKey(EstraiNomeMacro(rSostituita)) Then
                ' Espande la macro con i parametri già sostituiti
                righeMacro = EspandiMacroSingolaRiga(rSostituita)
            End If

            ' 3️⃣ Aggiungo righe espanse
            If righeMacro IsNot Nothing Then
                righeEspanse.AddRange(righeMacro)
            Else
                righeEspanse.Add(rSostituita)
            End If
        Next

        Return righeEspanse
    End Function

    ' Sostituisce {VAR} con i valori passati
    Private Function SostituisciParametriFOR(riga As String, vars As Dictionary(Of String, String)) As String
        Dim result = riga
        For Each kvp In vars
            result = result.Replace("{" & kvp.Key.ToUpper() & "}", kvp.Value)
        Next
        Return result
    End Function

    ' Espande una singola riga di macro
    Private Function EspandiMacroSingolaRiga(riga As String) As List(Of String)
        Dim nomeMacro = EstraiNomeMacro(riga)
        If Not Macros.ContainsKey(nomeMacro) Then Return New List(Of String) From {riga}

        Dim parametriRiga = riga.Split(";"c).Skip(1).ToList()
        Dim macro = Macros(nomeMacro)

        ' Sostituisci i parametri della macro
        Dim sostDict As New Dictionary(Of String, String)
        For i = 0 To macro.Parametri.Count - 1
            If i < parametriRiga.Count Then
                sostDict(macro.Parametri(i).ToUpper()) = parametriRiga(i)
            End If
        Next

        Dim righeFinali As New List(Of String)
        For Each r In macro.Corpo
            righeFinali.Add(SostituisciParametriMACRO(r, sostDict))
        Next

        Return righeFinali
    End Function

    ' Estrae il nome della macro da una riga
    Private Function EstraiNomeMacro(riga As String) As String
        Return riga.Split(";"c)(0).ToUpper()
    End Function
End Class

Public Class RobotContext 'Contesto g e bitmap
    Public Property Graphics As Graphics
    Public Property Bitmap As Bitmap

    Public Property View As Viewport

    Public Property Textures As New Dictionary(Of String, Bitmap)

    Public Property Patterns As New Dictionary(Of String, PatternDef)

    Public Property Livelli As New Dictionary(Of String, Livello)

    Public Property OrdineLivelli As New List(Of String)

    Public Property LivelloCorrente As String = "BackGround"

    ' Stato della "penna"
    Public Property ColoreCorrente As Color = Color.Black
    Public Property SpessoreCorrente As Integer = 1

    ' Ultimo punto disegnato
    Public LastPoint As PointF? = Nothing

    ' Primo punto disegnato
    Public StartPoint As PointF? = Nothing
End Class

Public Class Livello
    Public Property Nome As String
    Public Property Bitmap As Bitmap
    Public Property Visibile As Boolean = True
End Class

Public Class PatternDef
    'DashStyle.Dot, Dash, DashDot, Solid
    '📌 Angolo → rotazione delle linee
    '📌 Spaziatura → distanza tra le linee
    Public Property Dash As Drawing2D.DashStyle
    Public Property Colore As Color
    Public Property Spessore As Single
    Public Property Angolo As Single
    Public Property Spaziatura As Single
End Class

Public Class MacroDef
    'Le macro vengono espulse in comandi normali (LINEA, CERCHIO, …)

    Public Property Nome As String
    Public Property Parametri As List(Of String)
    Public Property Corpo As List(Of String)
End Class


Public Class RobotDrawer

    ' Lista dei comandi e descrizione
    Public Shared ComandiSintassi As New Dictionary(Of String, String) From {
    {"LINEA", "LINEA;x1,y1;x2,y2;Colore;Spessore"},
    {"CERCHIO", "CERCHIO;x,y;Raggio;Colore;Spessore"},
    {"RETT", "RETT;x1,y1;x2,y2;Tipo;Colore;Spessore"},
    {"TRIANG", "TRIANG;x1,y1;x2,y2;x3,y3;Colore;Tipo;Spessore"},
    {"PULISCI", "PULISCI;Colore"},
    {"ARCO", "ARCO;x1,y1;x2,y2;Colore;Tipo;Spessore;AngoloStart;AngoloSweep"},
    {"SCACCHI", "SCACCHI;x1,y1;x2,y2;Colore;Colore2"},
    {"TESTO", "TESTO;x1,y1;x2,y2;Testo;Colore;Dimensione;Font;Stile"},
    {"POLIGONO", "POLIGONO;xN,YN;Colore;Tipo"},
    {"GRIGLIA", "GRIGLIA;Lato;Colore"},
    {"SALVA", "SALVA;Percorso;Formato(PNG,BMP,JPG)"},
    {"INVERTI", "INVERTI;Direzione;-Percentuale"},
    {"RUOTA", "RUOTA;Gradi"},
    {"APPUNTI", "APPUNTI"},
    {"COPIA", "COPIA;Percorso"},
    {"INCOLLA", "INCOLLA;x1,y1"},
    {"RIDIMENSIONA", "RIDIMENSIONA;Appunti;Larghezza,Altezza"},
    {"SPLINE", "SPLINE;xN,yN;Colore;Spessore"},
    {"SPLINE2", "SPLINE2;xN,yN;Colore;Spessore;Tensione"},
    {"#", "#Commento"},
    {"CROCE", "CROCE;x1,y1;x2,y2;Colore;Spessore"},
    {"BEZIER", "BEZIER;x1,y1;x2,y2;x3,y3;x4,y4;Colore;Spessore"},
    {"TEXTURE", "TEXTURE;Nome;Percorso"},
    {"DRAWTEXTURE", "DRAWTEXTURE;Nome;x,y"},
    {"PATTERN", "PATTERN;Nome;LINEE;Angolo;Spaziatura;Colore;Spessore"},
    {"FILLPATTERN", "FILLPATTERN;Nome;x1,y1;x2,y2"},
    {"INIZIO", "INIZIO"},
    {"INIZIOCAD", "INIZIOCAD;Passo;Colore"},
    {"INIZIOMATH", "INIZIOMATH;ScalaX;ScalaY;ColoreScale"},
    {"ADDLIVELLO", "ADDLIVELLO;NomeLivello"},
    {"DELLIVELLO", "DELLIVELLO;NomeLivello"},
    {"RENLIVELLO", "RENLIVELLO;NomeLivello;NuovoNomeLivello"},
    {"STEP", "STEP;Numero"},
    {"GRIGLIAFULL", "GRIGLIAFULL;Lato;Colore"},
    {"FRECCIA", "FRECCIA;x1,y1;x2,y2;Colore;Spessore"},
    {"STELLA", "STELLA;x1,y1;NumeroPunte;Diametro;Colore;Spessore"},
    {"DEFINE", "DEFINE;NomeMacro;Parametri;L"},
    {"FOR", "FOR;NomeCiclo;Start;End;Step"},
    {"END", "END"},
    {"INCLUDE", "INCLUDE;PercorsoFile"},
    {"MATRICE", "MATRICE;x1,y1;x2,y2;xN,yN;PassoX,PassoY;Tipo;Colore;Spessore"},
    {"MATRICEQ", "MATRICEQ;x1,y1;x2,y2;PassoX,PassoY;Tipo;Colore;Spessore"},
    {"MUOVI", "MUOVI;x,y"},
    {"CHIUDI", "CHIUDI;x,y;Colore;Spessore"},
    {"SPIRALE", "SPIRALE;CentroX,CentroY;RaggioIniziale;RaggioFinale;Giri;Colore;Spessore;Direzione"},
    {"SINUSOIDE", "SINUSOIDE;StartX,StartY;EndX,EndY;Ampiezza;Frequenza;Colore;Spessore"}
}

    Public Sub Esegui(comando As RobotCommand, ctx As RobotContext)

        Dim g = ctx.Graphics

        Console.WriteLine("Eseguo comando: " & comando.Tipo & " con " & comando.Parametri.Count & " parametri")

        Select Case comando.Tipo
            Case "LINEA"
                'LINEA;10,10;200,50;Blu;2
                'Linea Accetta anche la clausola LAST come ultimo punto e FROM come start point
                Linea(comando.Parametri, g, ctx)
            Case "MUOVI"
                'MUOVI;x,y
                '#Poligono “manuale”
                'MUOVI;100,100
                'Linea;FROM;200,100;Blu;2
                'Linea;LAST;200,200;Blu;2
                'Linea;LAST;100,200;Rosso;2
                'Linea;LAST;FROM;Rosso;2
                MoveCommand(comando.Parametri, ctx)
            Case "CHIUDI"
                'CHIUDI;x,y;Colore;Spessore
                '#Poligono “manuale”
                'MUOVI;100,100
                'Linea;FROM;200,100;Blu;2
                'Linea;LAST;200,200;Blu;2
                'Linea;LAST;100,200;Rosso;2
                'Chiudi;100,100;Rosso;2
                CloseCommand(comando.Parametri, g, ctx)
            Case "RETT"
                'Comando Rettangolo è:
                'RETT;x1,y1;x2,y2;Colore;Tipo;Spessore con Tipo = PIENO o VUOTO
                Rettangolo(comando.Parametri, g, ctx)
            Case "CERCHIO"
                'CERCHIO;centroX,centroY;raggio;Colore;PIENO/VUOTO;spessore
                Cerchio(comando.Parametri, g, ctx)
            Case "TRIANG"
                'TRIANG;x1,y1;x2,y2;x3,y3;Red;PIENO/VUOTO;3
                Triangolo(comando.Parametri, g, ctx)
            Case "PULISCI"
                'PULISCI;Colore
                Debug.WriteLine($"Pulisci colore: {If(comando.Parametri.Count > 0, comando.Parametri(0), "nessuno")}")
                Pulisci(comando.Parametri, g)
            Case "ARCO"
                ''ARCO;x1,y1;x2,y2;Colore;PIENO/VUOTO;Spessore;StartAngle;SweepAngle
                'x1, y1 → angolo in alto a sinistra del rettangolo
                'x2, y2 → angolo in basso a destra del rettangolo
                'Colore → colore dell'arco
                'Tipo PIENO/VUOTO
                'Spessore → opzionale, Default 1
                'StartAngle → angolo iniziale (gradi)
                'SweepAngle → ampiezza dell'arco (gradi)
                Arco(comando.Parametri, g, ctx)
            Case "SCACCHI"
                'SCACCHI;x,y;x2,y2;Colore1;Colore2
                'SCACCHI;100,10;400,310;Blu;Rosso
                If comando.Parametri.Count >= 4 Then
                    Debug.WriteLine($"Scacchiera: ({comando.Parametri(0)}) → ({comando.Parametri(1)}), colori: {comando.Parametri(2)}/{comando.Parametri(3)}")
                End If
                Scacchiera(comando.Parametri, g, ctx)
            Case "TESTO"
                'TESTO;100,200;Ciao mondo;Rosso;20;Arial;Normal
                Testo(comando.Parametri, g, ctx)
            Case "POLIGONO"
                'POLIGONO;10,10;100,30;80,120;30,90;Verde;PIENO
                Poligono(comando.Parametri, g, ctx)
            Case "GRIGLIA"
                'GRIGLIA;20;Grigio
                Griglia(comando.Parametri, g, ctx)
            Case "SALVA"
                'SALVA;C:\Temp\immagine.png;PNG
                'SALVA;C:\Temp\immagine.bmp;BMP
                'SALVA;C:\Temp\foto.jpg;JPG
                SalvaBitmap(comando.Parametri, ctx.Bitmap)
            Case "APPUNTI"
                'APPUNTI                     # copia la bitmap corrente negli appunti
                AppuntiBitmap(ctx.Bitmap)

            Case "COPIA"
                'COPIA;C:\Immagini\foto.png  # carica un'immagine dal disco negli appunti
                If comando.Parametri.Count >= 1 Then
                    CopiaFileInAppunti(comando.Parametri(0))
                End If
            Case "INCOLLA"
                'INCOLLA;10,10               # incolla immagine dagli appunti alle coordinate 10,10
                'INCOLLA;50,100              # incolla ad altre coordinate
                If comando.Parametri.Count >= 1 Then
                    IncollaAppunti(ctx.Bitmap, comando.Parametri(0), ctx)
                End If
            Case "RIDIMENSIONA"
                'RIDIMENSIONA;Appunti;400,400       # ridimensiona immagine negli appunti a 400x400
                'RIDIMENSIONA;Appunti;800,600       # ridimensiona a 800x600
                If comando.Parametri.Count >= 2 Then
                    RidimensionaAppunti(comando.Parametri(0), comando.Parametri(1))
                End If
            Case "SPLINE"
                'SPLINE;x1,y1;x2,y2;x3,y3;...;Colore;Spessore
                Spline(comando.Parametri, g, ctx)
            Case "SPLINE2"
                'SPLINE2;x1,y1;x2,y2;...;Colore;Spessore;Tensione
                'SPLINE2;10,10;50,80;100,50;150,120;Blu;2;0.7
                SplineAvanzata(comando.Parametri, g, ctx)
            Case "SPIRALE"
                'SPIRALE;CentroX,CentroY;RaggioIniziale;RaggioFinale;Giri;Colore;Spessore;Direzione
                'SPIRALE;400,300;10;150;5;Blu;2;Oraria → spirale che parte da raggio 10 fino a 150, 5 giri, colore blu, spessore 2, direzione oraria
                'SPIRALE;400,300;150;10;5;Rosso;1;Antioraria → spirale inversa (si avvita verso l'interno)
                Spirale(comando.Parametri, g, ctx)

            Case "SINUSOIDE"
                'SINUSOIDE;StartX,StartY;EndX,EndY;Ampiezza;Frequenza;Colore;Spessore
                'SINUSOIDE;600,200;700,200;50;25;Blu;2
                Sinusoide(comando.Parametri, g, ctx)

            Case "CROCE"
                'CROCE;x1,y1;x2,y2;Colore;Spessore
                Croce(comando.Parametri, g, ctx)

            Case "BEZIER"
                'BEZIER;x1,y1;x2,y2;x3,y3;x4,y4;Colore;Spessore
                Bezier(comando.Parametri, g, ctx)

            Case "TEXTURE"
                'TEXTURE;Nome;Percorso
                TextureLoad(comando.Parametri, ctx)

            Case "DRAWTEXTURE"
                'DRAWTEX;Nome;x,y
                TextureDraw(comando.Parametri, g, ctx)

            Case "PATTERN"
                'PATTERN;Nome;LINEE;Angolo;Spaziatura;Colore;Spessore
                PatternCreate(comando.Parametri, ctx)

            Case "FILLPATTERN"
                'FILLPATTERN;Nome;x1,y1;x2,y2
                PatternFill(comando.Parametri, g, ctx)

            Case "INIZIO"
                'INIZIO
                Inizio(comando.Parametri, g, ctx)

            Case "INIZIOCAD"
                'sfondo nero, Rettangolo “foglio CAD” lime, look da workspace tecnico
                'INIZIOCAD;Passo;Colore
                InizioCad(comando.Parametri, g, ctx)

            Case "INIZIOMATH"
                'sfondo bianco, Griglia 10×10 grigio chiaro, asse X e Y centrati,origine visibile
                'INIZIOMATH;ScalaX;ScalaY;ColoreScale
                InizioMath(comando.Parametri, g, ctx)

            Case "ADDLIVELLO"
                'ADDLIVELLO;NomeLivello
                AddLivello(comando.Parametri, g, ctx)

            Case "DELLIVELLO"
                'DELLIVELLO;NomeLivello
                DelLivello(comando.Parametri, g, ctx)

            Case "RENLIVELLO"
                'RENLIVELLO;NomeLivello;NuovoNomeLivello
                RenLivello(comando.Parametri, g, ctx)

            Case "STEP"
                StepComandi(comando.Parametri)

            Case "GRIGLIAFULL"
                'GRIGLIAFULL;Lato;Colore
                GrigliaFull(comando.Parametri, g, ctx)

            Case "FRECCIA"
                'FRECCIA;x1,y1;x2,y2;Colore;Spessore
                Freccia(comando.Parametri, g, ctx)

            Case "STELLA"
                'STELLA;x1,y1;NumeroPunte;Diametro;Colore;Spessore
                Stella(comando.Parametri, g, ctx)

            Case "RUOTA"
                'RUOTA;Gradi
                RuotaBitmap(comando.Parametri, g, ctx.Bitmap)

            Case "INVERTI"
                'INVERTI;Direzione;-Percentuale
                InvertiBitmap(comando.Parametri, g, ctx.Bitmap)

            Case "MATRICE"
                'Trapezio
                'MATRICE;100,200;300,200;250,350;150,350;20,20;Punti;Malva;2
                'MATRICE;x1,y1;x2,y2;x3,y3;...;PASSOX,PASSOY;Tipo;Colore;Spessore
                ' Tipo = Punti, Quadrati, Croci, X
                MatricePoligono(comando.Parametri, g, ctx)

            Case "MATRICEQ"
                'Quadrato
                'MATRICEQ;400,400;700,700;20,20;Punti;Beige;2
                'MATRICEQ;x1,y1;x2,y2;PASSOX,PASSOY;Tipo;Colore;Spessore
                ' Tipo = Punti, Quadrati, Croci, X
                MatriceQuadrata(comando.Parametri, g, ctx)

            Case Else
                Debug.WriteLine($"Comando sconosciuto: {comando.Tipo}")

        End Select
    End Sub

    Public Shared ReadOnly ColoriItaliani As New Dictionary(Of String, Color) From {
    {"NERO", Color.Black},
    {"BIANCO", Color.White},
    {"ROSSO", Color.Red},
    {"VERDE", Color.Green},
    {"BLU", Color.Blue},
    {"GIALLO", Color.Yellow},
    {"CIANO", Color.Cyan},
    {"MAGENTA", Color.Magenta},
    {"GRIGIO", Color.Gray},
    {"ARGENTO", Color.Silver},
    {"MARRONE", Color.Brown},
    {"ARANCIONE", Color.Orange},
    {"VIOLA", Color.Purple},
    {"LIME", Color.Lime},
    {"TURCHESE", Color.Teal},
    {"CIOCCOLATO", Color.Chocolate},
    {"ROSA", Color.Pink},
    {"BEIGE", Color.Beige},
    {"SALMONE", Color.Salmon},
    {"CORALLO", Color.Coral},
    {"ACQUA", Color.Aqua},
    {"INDACO", Color.Indigo},
    {"OLIVA", Color.Olive},
    {"MELANZANA", Color.DarkViolet},
    {"BORDEAUX", Color.DarkRed},
    {"AZZURRO", Color.LightBlue},
    {"TERRA", Color.SaddleBrown},
    {"ZINCO", Color.LightGray},
    {"AVORIO", Color.Ivory},
    {"PRUGNA", Color.Plum},
    {"CIELO", Color.SkyBlue},
    {"FUXIA", Color.Fuchsia},
    {"GHIACCIO", Color.LightCyan},
    {"SABBIA", Color.SandyBrown},
    {"SENAPE", Color.Goldenrod},
    {"VERDESCURO", Color.DarkGreen},
    {"VERDECHIARO", Color.LightGreen},
    {"BLUSCURO", Color.DarkBlue},
    {"BLUCHIARO", Color.LightSkyBlue},
    {"ROSSOSCURO", Color.DarkRed},
    {"ROSSOCHIARO", Color.IndianRed},
    {"MARRONECHIARO", Color.Peru},
    {"MARRONESCURO", Color.Sienna},
    {"VIOLACHIARO", Color.Lavender},
    {"VIOLASCURO", Color.MediumPurple},
    {"SANDALO", Color.FromArgb(209, 182, 113)},
    {"CAMOSCIO", Color.FromArgb(240, 220, 130)},
    {"CARBONE", Color.FromArgb(5, 4, 2)},
    {"CARMINIO", Color.FromArgb(150, 0, 24)},
    {"CASTAGNO", Color.FromArgb(205, 92, 92)},
    {"CELESTE", Color.FromArgb(178, 255, 255)},
    {"CERULEO", Color.FromArgb(0, 123, 167)},
    {"CHARTREUSE", Color.FromArgb(128, 255, 0)},
    {"CILIEGIA", Color.FromArgb(222, 49, 99)},
    {"COBALTO", Color.FromArgb(0, 71, 171)},
    {"GRANATA", Color.FromArgb(128, 0, 0)},
    {"ARDESIA", Color.FromArgb(119, 136, 153)},
    {"NAPOLI", Color.FromArgb(247, 232, 159)},
    {"SEPPIA", Color.FromArgb(62, 48, 35)},
    {"LAVANDA", Color.FromArgb(181, 126, 220)},
    {"ACQUAMARINA", Color.FromArgb(0, 255, 128)},
    {"POMORODO", Color.FromArgb(255, 99, 71)},
    {"BRONZO", Color.FromArgb(205, 127, 50)},
    {"CACHI", Color.FromArgb(195, 176, 145)},
    {"LIMONE", Color.FromArgb(253, 255, 0)},
    {"GIADA", Color.FromArgb(0, 168, 107)},
    {"MAGGESE", Color.FromArgb(193, 154, 107)},
    {"RUGGINE", Color.FromArgb(183, 65, 14)},
    {"RAME", Color.FromArgb(184, 115, 51)},
    {"MOGANO", Color.FromArgb(192, 64, 0)},
    {"BORGOGNA", Color.FromArgb(128, 0, 32)},
    {"MALVA", Color.FromArgb(224, 176, 255)},
    {"LILLA", Color.FromArgb(200, 162, 200)},
    {"OCRA", Color.FromArgb(204, 119, 34)}
}

    Private Function ColorConv(nome As String) As Color
        'Conversione del colore
        Dim key = nome.ToUpper().Trim()

        ' Prima controllo nel dizionario italiano
        If ColoriItaliani.ContainsKey(key) Then
            Return ColoriItaliani(key)
        End If

        ' Se non lo trovi, prova con Color.FromName (inglese)
        Try
            Return Color.FromName(nome)
        Catch
            ' Default se non riconosciuto
            Return Color.Black
        End Try
    End Function

    Private Sub StepComandi(p As List(Of String))
        'Step;0 un comando alla volta e pausa. Step;n N comandi alla volta senza pausa.
        If p.Count < 1 Then Return

        Dim n As Integer
        If Not Integer.TryParse(p(0), n) Then Return

        Debug.WriteLine("Comando STEP: " & n.ToString)

        If n > 0 Then
            Form1.commandsPerTick = n
            Form1.waitForContinue = False
        Else
            ' N = 0 → eseguo 1 comando alla volta e mi fermo
            Form1.commandsPerTick = 1
            Form1.waitForContinue = True
        End If
    End Sub





    'TRASFORMAZIONE MONDO SCHERMO
    Private Function ToScreen(p As PointF, ctx As RobotContext) As PointF
        Return New PointF(
        (p.X - ctx.View.Origine.X) * ctx.View.Zoom,
        (p.Y - ctx.View.Origine.Y) * ctx.View.Zoom
    )
    End Function

    Private Function ToWorld(p As PointF, ctx As RobotContext) As PointF
        Return New PointF(
        p.X / ctx.View.Zoom + ctx.View.Origine.X,
        p.Y / ctx.View.Zoom + ctx.View.Origine.Y
    )
    End Function


    'Disegno
    Private Function Punto(s As String) As PointF
        ' fallback legacy: mondo puro
        If String.IsNullOrWhiteSpace(s) Then Return New PointF(0, 0)

        'Se nel punto ci sono coordinate separate da virgola, oppure punto o infine spazio
        Dim xy = s.Split(","c)

        Return New PointF(CInt(xy(0)), CInt(xy(1)))
    End Function

    ' Punto con LAST e @
    Private Function Punto(s As String, ctx As RobotContext) As PointF
        If String.IsNullOrWhiteSpace(s) Then Return New PointF(0, 0)

        s = s.Trim().ToUpper()

        ' LAST / @
        If s = "LAST" Or s = "@" Then
            If ctx.LastPoint.HasValue Then
                Return ctx.LastPoint.Value
            Else
                Throw New Exception("LASTPOINT non definito")
            End If
        End If

        ' FROM / START / ^
        If s = "FROM" Or s = "START" Or s = "^" Then
            If ctx.StartPoint.HasValue Then
                Return ctx.StartPoint.Value
            Else
                Throw New Exception("STARTPOINT non definito")
            End If
        End If

        Dim xy = s.Split(","c)
        Return New PointF(CSng(xy(0)), CSng(xy(1)))
    End Function

    Private Sub Linea(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 3 Then Return

        Dim w1 = Punto(p(0), ctx)
        Dim w2 = Punto(p(1), ctx)

        Dim colore = If(p.Count > 2, ColorConv(p(2)), ctx.ColoreCorrente)
        Dim spessore = If(p.Count >= 4, Integer.Parse(p(3)), ctx.SpessoreCorrente)

        Using pen As New Pen(colore, spessore)
            g.DrawLine(pen, ToScreen(w1, ctx), ToScreen(w2, ctx))
        End Using

        ' se è la prima linea del percorso
        If Not ctx.StartPoint.HasValue Then
            ctx.StartPoint = w1
        End If

        ctx.LastPoint = w2
    End Sub

    Private Sub MoveCommand(p As List(Of String), ctx As RobotContext)
        'MUOVI;100,100
        Dim pt = Punto(p(0), ctx)
        ctx.StartPoint = pt
        ctx.LastPoint = pt
    End Sub

    Private Sub CloseCommand(p As List(Of String), g As Graphics, ctx As RobotContext)
        'CHIUDI chiude il cerchio, poligono, struttura
        'CHIUDI;x,y;Colore;Spessore
        If p.Count < 3 Then Return

        Dim colore = If(p.Count > 1, ColorConv(p(1)), ctx.ColoreCorrente)
        Dim spessore = If(p.Count >= 2, Integer.Parse(p(2)), ctx.SpessoreCorrente)

        'Debug.WriteLine("CLOSE " & p(1) & " " & p(2))

        If ctx.LastPoint.HasValue AndAlso ctx.StartPoint.HasValue Then
            Using pen As New Pen(colore, spessore)
                g.DrawLine(pen,
                ToScreen(ctx.LastPoint.Value, ctx),
                ToScreen(ctx.StartPoint.Value, ctx))
            End Using
            ctx.LastPoint = ctx.StartPoint
        End If
    End Sub

    Private Sub Rettangolo(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 4 Then Return
        Dim p1 = Punto(p(0))
        Dim p2 = Punto(p(1))
        Dim colore = ColorConv(p(2))
        Dim pieno As Boolean = p(3).ToUpper() = "PIENO"
        Dim spessore = If(p.Count > 4, Integer.Parse(p(4)), 1)

        Dim a = ToScreen(p1, ctx)
        Dim b = ToScreen(p2, ctx)

        Dim rect As New Rectangle(
        Math.Min(a.X, b.X),
        Math.Min(a.Y, b.Y),
        Math.Abs(b.X - a.X),
        Math.Abs(b.Y - a.Y)
    )
        'Debug.WriteLine("p1" & p1.ToString & " p2" & p2.ToString & " Colore " & p(2) & " Tipo " & p(3) & " Spessore " & spessore.ToString)
        If pieno Then
            g.FillRectangle(New SolidBrush(colore), rect)
        Else
            Using pen As New Pen(colore, spessore)
                g.DrawRectangle(pen, rect)
            End Using
        End If
    End Sub

    Private Sub Cerchio(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 4 Then Return
        Dim centro = Punto(p(0))
        Dim raggio = Integer.Parse(p(1))
        Dim colore = ColorConv(p(2))
        Dim pieno As Boolean = p(3).ToUpper() = "PIENO"
        Dim spessore = If(p.Count > 4, Integer.Parse(p(4)), 1)

        Dim a = ToScreen(centro, ctx)

        Dim rect As New Rectangle(a.X - raggio, a.Y - raggio, raggio * 2, raggio * 2)

        If pieno Then
            g.FillEllipse(New SolidBrush(colore), rect)
        Else
            Using pen As New Pen(colore, spessore)
                g.DrawEllipse(pen, rect)
            End Using
        End If
    End Sub

    Private Sub Triangolo(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 5 Then Return
        Dim p1 = Punto(p(0))
        Dim p2 = Punto(p(1))
        Dim p3 = Punto(p(2))
        Dim colore = ColorConv(p(3))
        Dim pieno As Boolean = p(4).ToUpper() = "PIENO"
        Dim spessore = If(p.Count > 5, Integer.Parse(p(5)), 1)

        Dim a = ToScreen(p1, ctx)
        Dim b = ToScreen(p2, ctx)
        Dim c = ToScreen(p3, ctx)

        Dim punti() As PointF = {a, b, c}

        If pieno Then
            g.FillPolygon(New SolidBrush(colore), punti)
        Else
            Using pen As New Pen(colore, spessore)
                g.DrawPolygon(pen, punti)
            End Using
        End If
    End Sub

    Private Sub Pulisci(p As List(Of String), g As Graphics)
        If p.Count < 1 Then Return
        Dim colore As Color = ColorConv(p(0))
        g.Clear(colore)
    End Sub

    Private Sub Arco(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 7 Then Return

        Dim p1 = Punto(p(0))   ' angolo alto a sinistra
        Dim p2 = Punto(p(1))   ' angolo basso a destra
        Dim colore = ColorConv(p(2))
        Dim pieno As Boolean = p(3).ToUpper() = "PIENO"
        Dim spessore As Integer = Integer.Parse(p(4))
        Dim startAngle = Integer.Parse(p(5))
        Dim sweepAngle = Integer.Parse(p(6))

        Dim a = ToScreen(p1, ctx)
        Dim b = ToScreen(p2, ctx)

        Dim rect As New Rectangle(
            Math.Min(a.X, b.X),
            Math.Min(a.Y, b.Y),
            Math.Abs(b.X - a.X),
            Math.Abs(b.Y - a.Y)
        )

        If pieno Then
            g.FillPie(New SolidBrush(colore), rect, startAngle, sweepAngle)
        Else
            Using pen As New Pen(colore, spessore)
                g.DrawArc(pen, rect, startAngle, sweepAngle)
            End Using
        End If
    End Sub

    Private Sub Scacchiera(p As List(Of String), g As Graphics, ctx As RobotContext)
        ' Controllo parametri
        If p.Count < 4 Then Return

        ' Angoli
        Dim angolo1 = Punto(p(0))
        Dim angolo2 = Punto(p(1))

        Dim a = ToScreen(angolo1, ctx)
        Dim b = ToScreen(angolo2, ctx)

        ' Colori
        Dim colore1 = ColorConv(p(2))
        Dim colore2 = ColorConv(p(3))

        ' Calcolo angoli reali e dimensioni
        Dim xMin = Math.Min(a.X, b.X)
        Dim yMin = Math.Min(a.Y, b.Y)
        Dim larghezzaTot = Math.Abs(b.X - a.X)
        Dim altezzaTot = Math.Abs(b.Y - a.Y)

        Dim largCasella = larghezzaTot / 8
        Dim altCasella = altezzaTot / 8

        For riga = 0 To 7
            For col = 0 To 7
                Dim coloreCorrente = If((riga + col) Mod 2 = 0, colore1, colore2)
                Dim rect As New Rectangle(
                xMin + CInt(col * largCasella),
                yMin + CInt(riga * altCasella),
                CInt(largCasella),
                CInt(altCasella)
            )
                g.FillRectangle(New SolidBrush(coloreCorrente), rect)
            Next
        Next
    End Sub

    Private Sub Testo(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 4 Then Return

        Dim pos = Punto(p(0))
        Dim testo = p(1)
        Dim colore = ColorConv(p(2))
        Dim size As Single = CSng(p(3))

        Dim a = ToScreen(pos, ctx)

        Dim fontName As String = If(p.Count > 4, p(4), "Arial")
        Dim stile As FontStyle = If(p.Count > 5, ParseFontStyle(p(5)), FontStyle.Regular)

        Using f As Font = FontSicuro(fontName, size, stile)
            Using b As New SolidBrush(colore)
                g.DrawString(testo, f, b, a)
            End Using
        End Using
    End Sub


    Private Function FontSicuro(nomeFont As String, size As Single, stile As FontStyle) As Font
        'Utilizzato da TESTO per recuperare il font anche quando non esiste nel sistema
        Try
            For Each fam As FontFamily In FontFamily.Families
                If fam.Name.Equals(nomeFont, StringComparison.OrdinalIgnoreCase) Then

                    ' Verifica che lo stile sia supportato
                    If fam.IsStyleAvailable(stile) Then
                        Return New Font(fam, size, stile)
                    ElseIf fam.IsStyleAvailable(FontStyle.Regular) Then
                        Return New Font(fam, size, FontStyle.Regular)
                    End If

                End If
            Next
        Catch
        End Try

        ' Fallback finale
        Return New Font("Arial", size, stile)
    End Function

    Private Function ParseFontStyle(s As String) As FontStyle
        Select Case s.ToUpper().Trim()
            Case "BOLD"
                Return FontStyle.Bold
            Case "ITALIC"
                Return FontStyle.Italic
            Case "NORMAL", "REGULAR"
                Return FontStyle.Regular
            Case Else
                Return FontStyle.Regular
        End Select
    End Function

    Private Sub Poligono(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 4 Then Return

        Dim punti As New List(Of PointF)

        Dim i As Integer = 0
        While i < p.Count AndAlso p(i).Contains(",")
            Dim p1 = Punto(p(i))
            Dim a = ToScreen(p1, ctx)

            punti.Add(a)
            i += 1
        End While

        Dim colore = ColorConv(p(i))
        Dim pieno = (p(i + 1).ToUpper() = "PIENO")
        Dim spessore = If(p.Count > i + 2, Integer.Parse(p(i + 2)), 1)

        If pieno Then
            g.FillPolygon(New SolidBrush(colore), punti.ToArray())
        Else
            Using pen As New Pen(colore, spessore)
                g.DrawPolygon(pen, punti.ToArray())
            End Using
        End If
    End Sub

    Private Sub Griglia(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 2 Then Return

        Dim passo = Integer.Parse(p(0))
        Dim colore = ColorConv(p(1))

        Using pen As New Pen(colore, 1)
            For x = 0 To g.VisibleClipBounds.Width Step passo
                g.DrawLine(pen, x, 0, x, g.VisibleClipBounds.Height)
            Next
            For y = 0 To g.VisibleClipBounds.Height Step passo
                g.DrawLine(pen, 0, y, g.VisibleClipBounds.Width, y)
            Next
        End Using
    End Sub

    Private Sub SalvaBitmap(p As List(Of String), bmp As Bitmap)
        If p.Count < 2 Then Return

        Dim path = p(0)
        Dim formato = p(1).ToUpper()

        Dim imgFormat As Imaging.ImageFormat
        Select Case formato
            Case "PNG"
                imgFormat = Imaging.ImageFormat.Png
            Case "BMP"
                imgFormat = Imaging.ImageFormat.Bmp
            Case "JPG", "JPEG"
                imgFormat = Imaging.ImageFormat.Jpeg
            Case Else
                imgFormat = Imaging.ImageFormat.Png
        End Select

        Try
            ' Crea cartella se non esiste
            Dim dir = System.IO.Path.GetDirectoryName(path)
            If Not System.IO.Directory.Exists(dir) Then
                System.IO.Directory.CreateDirectory(dir)
            End If

            ' Salva
            bmp.Save(path, imgFormat)
            Debug.WriteLine("Bitmap salvata: " & path)
        Catch ex As Exception
            ' Fallback negli appunti
            Try
                Clipboard.SetImage(bmp)
                Debug.WriteLine("Bitmap salvata negli appunti")
                MsgBox("Impossibile salvare. L'immagine é stata copiata negli Appunti di Windows", MsgBoxStyle.Information, "PaintRobot")
            Catch innerEx As Exception
                Debug.WriteLine("Errore appunti: " & innerEx.Message)
            End Try
        End Try
    End Sub

    Private Sub InvertiBitmap(p As List(Of String), g As Graphics, bmp As Bitmap)
        If p.Count < 1 Then Return

        Dim direzione As String = p(0).ToUpper().Trim()   ' "H" / "V" o "ORIZZONTALE" / "VERTICALE"
        Dim inverso As Boolean = False
        Dim perc As Single = 1.0F

        If p.Count > 1 Then
            perc = ParsePercentualeConSegno(p(1), inverso) / 100.0F
        End If

        If direzione = "V" OrElse direzione = "VERTICALE" Then
            InvertiVerticale(bmp, perc, inverso)
        ElseIf direzione = "H" OrElse direzione = "ORIZZONTALE" Then
            InvertiOrizzontale(bmp, perc, inverso)
        Else
            ' default verticale
            InvertiVerticale(bmp, perc, inverso)
        End If

        ' Aggiorna render target
        g.DrawImageUnscaled(bmp, 0, 0)
    End Sub


    Private Function ParsePercentuale(s As String) As Single
        'Usato da INVERTI
        s = s.Replace("%", "").Trim()

        Dim val As Single
        If Single.TryParse(s, Globalization.NumberStyles.Float,
                       Globalization.CultureInfo.InvariantCulture, val) Then
            Return Math.Max(0, Math.Min(100, val))
        End If

        Return 100
    End Function

    Private Function ParsePercentualeConSegno(s As String, ByRef inverso As Boolean) As Single
        'Usato da INVERTI
        s = s.Trim()
        inverso = False

        If s.StartsWith("-") Then
            inverso = True
            s = s.Substring(1)
        End If

        s = s.Replace("%", "").Trim()

        Dim val As Single
        If Single.TryParse(s, Globalization.NumberStyles.Float,
                       Globalization.CultureInfo.InvariantCulture, val) Then
            Return Math.Max(0, Math.Min(100, val))
        End If

        Return 100
    End Function

    Private Sub InvertiVerticale(bmp As Bitmap, perc As Single, inverso As Boolean)
        Dim h = bmp.Height
        Dim area = CInt(h * perc)

        Dim startY As Integer
        Dim endY As Integer

        If Not inverso Then
            startY = 0
            endY = area - 1
        Else
            startY = h - area
            endY = h - 1
        End If

        Dim half = (endY - startY + 1) \ 2


        For i = 0 To half - 1
            Dim y1 = startY + i
            Dim y2 = endY - i

            For x = 0 To bmp.Width - 1
                Dim c1 = bmp.GetPixel(x, y1)
                Dim c2 = bmp.GetPixel(x, y2)

                bmp.SetPixel(x, y1, c2)
                bmp.SetPixel(x, y2, c1)
            Next
        Next
    End Sub

    Private Sub InvertiOrizzontale(bmp As Bitmap, perc As Single, inverso As Boolean)
        Dim w = bmp.Width
        Dim area = CInt(w * perc)

        Dim startX As Integer
        Dim endX As Integer

        If Not inverso Then
            startX = 0
            endX = area - 1
        Else
            startX = w - area
            endX = w - 1
        End If

        Dim half = (endX - startX + 1) \ 2

        For i = 0 To half - 1
            Dim x1 = startX + i
            Dim x2 = endX - i

            For y = 0 To bmp.Height - 1
                Dim c1 = bmp.GetPixel(x1, y)
                Dim c2 = bmp.GetPixel(x2, y)

                bmp.SetPixel(x1, y, c2)
                bmp.SetPixel(x2, y, c1)
            Next
        Next
    End Sub

    Private Sub RuotaBitmap(p As List(Of String), g As Graphics, bmp As Bitmap)

        If p.Count < 1 Then Return

        Dim angolo As Single
        If Not Single.TryParse(p(0), Globalization.NumberStyles.Float,
                           Globalization.CultureInfo.InvariantCulture, angolo) Then Return

        Dim w = bmp.Width
        Dim h = bmp.Height

        Using src As Bitmap = CType(bmp.Clone(), Bitmap)

            g.ResetTransform()
            g.Clear(Color.White)

            g.TranslateTransform(w / 2.0F, h / 2.0F)
            g.RotateTransform(angolo)      ' ← 45.5 OK
            g.TranslateTransform(-w / 2.0F, -h / 2.0F)

            g.DrawImage(src, 0, 0)
            g.ResetTransform()
        End Using
    End Sub

    Private Sub SfumaBitmap(p As List(Of String), bmp As Bitmap)
        If p.Count < 3 Then Return

        Dim tipo = p(0).ToUpper()
        Dim col1 = ColorConv(p(1))
        Dim col2 = ColorConv(p(2))

        Dim inverso As Boolean = False
        Dim perc As Single = 1.0F

        If p.Count > 3 Then
            perc = ParsePercentualeConSegno(p(3), inverso) / 100.0F
        End If

        Dim rect As Rectangle = If(tipo = "VERTICALE",
            AreaVerticale(bmp, perc, inverso),
            AreaOrizzontale(bmp, perc, inverso))

        Using g = Graphics.FromImage(bmp)
            Dim mode = If(tipo = "VERTICALE",
                           Drawing2D.LinearGradientMode.Vertical,
                           Drawing2D.LinearGradientMode.Horizontal)

            Using br As New Drawing2D.LinearGradientBrush(rect, col1, col2, mode)
                g.FillRectangle(br, rect)
            End Using
        End Using
    End Sub

    Private Function AreaVerticale(bmp As Bitmap, perc As Single, inverso As Boolean) As Rectangle
        Dim h = CInt(bmp.Height * perc)
        Dim y = If(inverso, bmp.Height - h, 0)
        Return New Rectangle(0, y, bmp.Width, h)
    End Function

    Private Function AreaOrizzontale(bmp As Bitmap, perc As Single, inverso As Boolean) As Rectangle
        Dim w = CInt(bmp.Width * perc)
        Dim x = If(inverso, bmp.Width - w, 0)
        Return New Rectangle(x, 0, w, bmp.Height)
    End Function

    Private Sub ContrastoBitmap(p As List(Of String), bmp As Bitmap)
        If p.Count < 1 Then Return

        Dim c As Integer = ParseContrasto(p(0))
        If c = 0 Then Return

        Dim factor As Double =
            (259.0 * (c + 255)) / (255.0 * (259 - c))

        For y = 0 To bmp.Height - 1
            For x = 0 To bmp.Width - 1
                Dim col = bmp.GetPixel(x, y)

                Dim r = Clamp(factor * (col.R - 128) + 128)
                Dim g = Clamp(factor * (col.G - 128) + 128)
                Dim b = Clamp(factor * (col.B - 128) + 128)

                bmp.SetPixel(x, y, Color.FromArgb(col.A, r, g, b))
            Next
        Next
    End Sub

    Private Function Clamp(v As Double) As Integer
        Return CInt(Math.Max(0, Math.Min(255, v)))
    End Function

    Private Function ParseContrasto(s As String) As Integer
        s = s.Replace("%", "").Trim()

        Dim val As Single
        If Single.TryParse(s, Globalization.NumberStyles.Float,
                       Globalization.CultureInfo.InvariantCulture, val) Then

            val = Math.Max(-100, Math.Min(100, val))
            Return CInt(val * 2.55F) ' -255 → +255
        End If

        Return 0
    End Function

    Private Sub LuminositaBitmap(p As List(Of String), bmp As Bitmap)
        If p.Count < 1 Then Return

        Dim val = ParsePercentuale(p(0))  ' valore -100..100
        Dim delta = CInt(255 * val / 100.0)

        For y = 0 To bmp.Height - 1
            For x = 0 To bmp.Width - 1
                Dim c = bmp.GetPixel(x, y)
                Dim r = Clamp(c.R + delta)
                Dim g = Clamp(c.G + delta)
                Dim b = Clamp(c.B + delta)
                bmp.SetPixel(x, y, Color.FromArgb(c.A, r, g, b))
            Next
        Next
    End Sub

    Private Sub SaturazioneBitmap(p As List(Of String), bmp As Bitmap)
        If p.Count < 1 Then Return

        Dim val = ParsePercentuale(p(0))  ' -100..100

        For y = 0 To bmp.Height - 1
            For x = 0 To bmp.Width - 1
                Dim c = bmp.GetPixel(x, y)
                Dim h, s, l As Double
                RgbToHsl(c, h, s, l)

                s += val / 100.0
                s = Math.Max(0, Math.Min(1, s))

                Dim newC = HslToRgb(h, s, l, c.A)
                bmp.SetPixel(x, y, newC)
            Next
        Next
    End Sub

    Private Sub RgbToHsl(c As Color, ByRef h As Double, ByRef s As Double, ByRef l As Double)
        Dim r = c.R / 255.0
        Dim g = c.G / 255.0
        Dim b = c.B / 255.0

        Dim max = Math.Max(r, Math.Max(g, b))
        Dim min = Math.Min(r, Math.Min(g, b))
        l = (max + min) / 2.0

        If max = min Then
            s = 0
            h = 0
        Else
            Dim d = max - min
            s = If(l > 0.5, d / (2 - max - min), d / (max + min))

            Select Case max
                Case r
                    h = (g - b) / d + If(g < b, 6, 0)
                Case g
                    h = (b - r) / d + 2
                Case b
                    h = (r - g) / d + 4
            End Select
            h /= 6
        End If
    End Sub

    Private Function HslToRgb(h As Double, s As Double, l As Double, alpha As Integer) As Color
        Dim r, g, b As Double

        If s = 0 Then
            r = l
            g = l
            b = l
        Else
            Dim q = If(l < 0.5, l * (1 + s), l + s - l * s)
            Dim p = 2 * l - q
            r = Hue2Rgb(p, q, h + 1.0 / 3.0)
            g = Hue2Rgb(p, q, h)
            b = Hue2Rgb(p, q, h - 1.0 / 3.0)
        End If

        Return Color.FromArgb(alpha, Clamp(r * 255), Clamp(g * 255), Clamp(b * 255))
    End Function

    Private Function Hue2Rgb(p As Double, q As Double, t As Double) As Double
        If t < 0 Then t += 1
        If t > 1 Then t -= 1
        If t < 1 / 6.0 Then Return p + (q - p) * 6 * t
        If t < 1 / 2.0 Then Return q
        If t < 2 / 3.0 Then Return p + (q - p) * (2 / 3.0 - t) * 6
        Return p
    End Function

    Private Sub AppuntiBitmap(bmp As Bitmap)
        Try
            My.Computer.Clipboard.SetImage(bmp)
            Debug.WriteLine("Bitmap copiata negli appunti.")
        Catch ex As Exception
            Debug.WriteLine("Errore copia appunti: " & ex.Message)
        End Try
    End Sub

    Private Sub CopiaFileInAppunti(percorso As String)
        Try
            If System.IO.File.Exists(percorso) Then
                ' Apri il file con FileStream e carica l'immagine
                Using fs As New System.IO.FileStream(percorso, System.IO.FileMode.Open, System.IO.FileAccess.Read)
                    Using img As Image = Image.FromStream(fs)
                        ' Copia negli appunti
                        My.Computer.Clipboard.SetImage(New Bitmap(img))
                    End Using
                End Using

                Debug.WriteLine("Immagine caricata negli appunti da: " & percorso)
            Else
                Debug.WriteLine("File non trovato: " & percorso)
            End If
        Catch ex As Exception
            Debug.WriteLine("Errore caricamento appunti: " & ex.Message)
        End Try
    End Sub

    Private Sub IncollaAppunti(bmp As Bitmap, coord As String, ctx As RobotContext)
        Try
            If My.Computer.Clipboard.ContainsImage Then
                Dim img As Image = My.Computer.Clipboard.GetImage()
                Dim p As PointF = Punto(coord)

                Dim a = ToScreen(p, ctx)

                Using g As Graphics = Graphics.FromImage(bmp)
                    g.DrawImage(img, a.X, a.Y, img.Width, img.Height)
                End Using

                Debug.WriteLine($"Immagine incollata agli appunti a: {a.X},{a.Y}")
            Else
                Debug.WriteLine("Nessuna immagine negli appunti.")
            End If
        Catch ex As Exception
            Debug.WriteLine("Errore incolla appunti: " & ex.Message)
        End Try
    End Sub

    Private Sub RidimensionaAppunti(source As String, sizeStr As String)
        Try
            ' Controlla se c'è un'immagine negli appunti
            If source.ToUpper() = "APPUNTI" AndAlso My.Computer.Clipboard.ContainsImage Then
                Dim img As Image = My.Computer.Clipboard.GetImage()

                ' Leggi le dimensioni
                Dim parts = sizeStr.Split(","c)
                If parts.Length < 2 Then Return

                Dim newWidth As Integer = CInt(parts(0).Trim())
                Dim newHeight As Integer = CInt(parts(1).Trim())

                ' Crea bitmap ridimensionata
                Dim bmpNew As New Bitmap(newWidth, newHeight)
                Using g As Graphics = Graphics.FromImage(bmpNew)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    g.DrawImage(img, 0, 0, newWidth, newHeight)
                End Using

                ' Aggiorna gli appunti
                My.Computer.Clipboard.SetImage(bmpNew)
                Debug.WriteLine($"Appunti ridimensionati a {newWidth}x{newHeight}")
            Else
                Debug.WriteLine("Nessuna immagine negli appunti o sorgente non riconosciuta.")
            End If
        Catch ex As Exception
            Debug.WriteLine("Errore ridimensiona appunti: " & ex.Message)
        End Try
    End Sub

    Private Sub RidimensionaBitmap(bmp As Bitmap, g As Graphics, nuovaLarghezza As Integer, nuovaAltezza As Integer)
        If bmp Is Nothing Then Return
        If nuovaLarghezza <= 0 OrElse nuovaAltezza <= 0 Then Return

        ' Creiamo una nuova bitmap ridimensionata
        Using temp As New Bitmap(nuovaLarghezza, nuovaAltezza)
            Using gTemp As Graphics = Graphics.FromImage(temp)
                gTemp.Clear(Color.White) ' sfondo CAD
                gTemp.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                gTemp.DrawImage(bmp, 0, 0, nuovaLarghezza, nuovaAltezza)
            End Using

            ' Copia finale sulla bitmap originale
            Using gDst = Graphics.FromImage(bmp)
                gDst.Clear(Color.White)
                gDst.DrawImageUnscaled(temp, 0, 0)
            End Using
        End Using

        ' Aggiorna canvas visibile
        g.DrawImageUnscaled(bmp, 0, 0)
    End Sub


    Private Sub Spline(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 4 Then Return ' almeno 3 punti + colore

        ' Leggi colore
        Dim colore = ColorConv(p(p.Count - 2))

        ' Leggi spessore se presente
        Dim spessore As Integer = 1
        If p.Count >= 5 Then
            Dim lastParam = p.Last()
            If Integer.TryParse(lastParam, spessore) Then
                ' ok, spessore letto
            End If
        End If

        ' Leggi punti
        Dim punti As New List(Of PointF)
        For i = 0 To p.Count - 3
            Dim p1 = Punto(p(i))
            Dim a = ToScreen(p1, ctx)

            punti.Add(a)
        Next

        ' Se meno di 3 punti non possiamo fare spline
        If punti.Count < 3 Then Return

        Using pen As New Pen(colore, spessore)
            ' Disegna curva Catmull-Rom
            g.DrawCurve(pen, punti.ToArray())
        End Using
    End Sub

    Private Sub SplineAvanzata(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 4 Then Return ' almeno 3 punti + colore

        ' Leggi colore
        Dim colore = ColorConv(p(p.Count - 3))

        ' Leggi spessore
        Dim spessore As Integer = 1
        Integer.TryParse(p(p.Count - 2), spessore)

        ' Leggi tensione
        Dim tension As Single = 0.5F
        Single.TryParse(p(p.Count - 1), tension)
        tension = Math.Max(0F, Math.Min(1.0F, tension))

        ' Leggi punti: tutti tranne gli ultimi 3 parametri
        Dim punti As New List(Of PointF)
        For i = 0 To p.Count - 4
            Try
                Dim p1 = Punto(p(i))
                Dim a = ToScreen(p1, ctx)

                punti.Add(a)
            Catch ex As Exception
                Debug.WriteLine($"Errore parsing punto: {p(i)} - {ex.Message}")
            End Try
        Next

        ' Almeno 3 punti
        If punti.Count < 3 Then Return

        Using pen As New Pen(colore, spessore)
            g.DrawCurve(pen, punti.ToArray(), tension)
        End Using
    End Sub

    Private Sub RemoveColore(coloreDaRimuovere As String, coloreSfondo As String, bmp As Bitmap)
        Try
            Dim target As Color = ColorConv(coloreDaRimuovere)
            Dim sfondo As Color = ColorConv(coloreSfondo)
            Debug.WriteLine("Colore RGB Da rimuovere: " & target.R.ToString & "," & target.G.ToString & "," & target.B.ToString)
            Debug.WriteLine("Colore RGB Di sfondo: " & sfondo.R.ToString & "," & sfondo.G.ToString & "," & sfondo.B.ToString)

            For x = 0 To bmp.Width - 1
                For y = 0 To bmp.Height - 1
                    Dim px = bmp.GetPixel(x, y)

                    ' Controlla componenti RGB singolarmente
                    If px.R = target.R AndAlso px.G = target.G AndAlso px.B = target.B Then
                        bmp.SetPixel(x, y, sfondo)
                    End If
                Next
            Next

            Debug.WriteLine($"Colore {coloreDaRimuovere} sostituito con {coloreSfondo}.")
        Catch ex As Exception
            Debug.WriteLine("Errore REMOVE: " & ex.Message)
        End Try
    End Sub

    Private Sub RemoveColore2(coloreDaRimuovere As String, coloreSfondo As String, bmp As Bitmap)
        Dim rgb1 = coloreDaRimuovere.Split(","c)
        Dim rgb2 = coloreSfondo.Split(","c)
        If rgb1.Length <> 3 OrElse rgb2.Length <> 3 Then Return

        Dim target As Color = Color.FromArgb(CInt(rgb1(0)), CInt(rgb1(1)), CInt(rgb1(2)))
        Dim sfondo As Color = Color.FromArgb(CInt(rgb2(0)), CInt(rgb2(1)), CInt(rgb2(2)))

        ' Blocca i pixel
        Dim rect As New Rectangle(0, 0, bmp.Width, bmp.Height)
        Dim bmpData = bmp.LockBits(rect, Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)

        Dim bytesPerPixel As Integer = Image.GetPixelFormatSize(bmp.PixelFormat) \ 8
        Dim ptr As IntPtr = bmpData.Scan0
        Dim byteCount As Integer = bmpData.Stride * bmp.Height
        Dim pixels(byteCount - 1) As Byte
        System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, byteCount)

        For y As Integer = 0 To bmp.Height - 1
            For x As Integer = 0 To bmp.Width - 1
                Dim idx As Integer = y * bmpData.Stride + x * bytesPerPixel
                Dim b As Byte = pixels(idx + 0)
                Dim g As Byte = pixels(idx + 1)
                Dim r As Byte = pixels(idx + 2)

                If r = target.R AndAlso g = target.G AndAlso b = target.B Then
                    pixels(idx + 0) = sfondo.B
                    pixels(idx + 1) = sfondo.G
                    pixels(idx + 2) = sfondo.R
                End If
            Next
        Next

        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, byteCount)
        bmp.UnlockBits(bmpData)
    End Sub

    Private Sub Sfumatura(p As List(Of String), bmp As Bitmap)
        If p.Count < 4 Then Return

        Dim colore1 As Color = ColorConv(p(0))
        Dim colore2 As Color = ColorConv(p(1))
        Dim tipo As String = p(2).ToUpper().Trim()
        Dim direzione As String = p(3).ToUpper().Trim()

        ' Creiamo bitmap temporanea
        Using gradBmp As New Bitmap(bmp.Width, bmp.Height)
            Using g As Graphics = Graphics.FromImage(gradBmp)
                Dim brushMode As Drawing.Drawing2D.LinearGradientMode

                Select Case tipo
                    Case "VERTICALE"
                        brushMode = Drawing.Drawing2D.LinearGradientMode.Vertical
                        If direzione = "BASSO" Then
                            Dim tmp = colore1
                            colore1 = colore2
                            colore2 = tmp
                        End If
                    Case "ORIZZONTALE"
                        brushMode = Drawing.Drawing2D.LinearGradientMode.Horizontal
                        If direzione = "DESTRA" Then
                            Dim tmp = colore1
                            colore1 = colore2
                            colore2 = tmp
                        End If
                    Case Else
                        Debug.WriteLine("SFUMATURA: Tipo non riconosciuto")
                        Return
                End Select

                Using lgBrush As New Drawing.Drawing2D.LinearGradientBrush(New Rectangle(0, 0, gradBmp.Width, gradBmp.Height), colore1, colore2, brushMode)
                    g.FillRectangle(lgBrush, 0, 0, gradBmp.Width, gradBmp.Height)
                End Using
            End Using

            Using gMain As Graphics = Graphics.FromImage(bmp)
                gMain.DrawImageUnscaled(gradBmp, 0, 0)
            End Using
        End Using

        Debug.WriteLine($"Sfumatura {tipo} da {p(0)} a {p(1)} direzione {p(3)} applicata.")
    End Sub

    Private Sub SfumaturaP(p As List(Of String), bmp As Bitmap)
        If p.Count < 4 Then Return

        Dim colore1 As Color = ColorConv(p(0))
        Dim colore2 As Color = ColorConv(p(1))
        Dim tipo As String = p(2).ToUpper().Trim()
        Dim direzione As String = p(3).ToUpper().Trim()
        Dim percentuale As Double = 100

        If p.Count >= 5 Then
            Dim percStr = p(4).Replace("%", "")
            If Double.TryParse(percStr, percentuale) = False Then percentuale = 100
            percentuale = Math.Min(Math.Max(percentuale, 0), 100)
        End If

        Using gradBmp As New Bitmap(bmp.Width, bmp.Height)
            Using g As Graphics = Graphics.FromImage(gradBmp)
                Dim rect As Rectangle
                Dim startColore As Color = colore1
                Dim endColore As Color = colore2

                Select Case tipo
                    Case "VERTICALE"
                        Dim altezzaFino As Integer = CInt(bmp.Height * percentuale / 100)
                        If direzione = "BASSO" Then
                            rect = New Rectangle(0, bmp.Height - altezzaFino, bmp.Width, altezzaFino)
                            Dim tmp = startColore
                            startColore = endColore
                            endColore = tmp
                        Else
                            rect = New Rectangle(0, 0, bmp.Width, altezzaFino)
                        End If
                        Using lgBrush As New Drawing.Drawing2D.LinearGradientBrush(rect, startColore, endColore, Drawing.Drawing2D.LinearGradientMode.Vertical)
                            g.FillRectangle(lgBrush, rect)
                        End Using

                    Case "ORIZZONTALE"
                        Dim larghezzaFino As Integer = CInt(bmp.Width * percentuale / 100)
                        If direzione = "DESTRA" Then
                            rect = New Rectangle(bmp.Width - larghezzaFino, 0, larghezzaFino, bmp.Height)
                            Dim tmp = startColore
                            startColore = endColore
                            endColore = tmp
                        Else
                            rect = New Rectangle(0, 0, larghezzaFino, bmp.Height)
                        End If
                        Using lgBrush As New Drawing.Drawing2D.LinearGradientBrush(rect, startColore, endColore, Drawing.Drawing2D.LinearGradientMode.Horizontal)
                            g.FillRectangle(lgBrush, rect)
                        End Using

                    Case Else
                        Debug.WriteLine("SFUMATURA: Tipo non riconosciuto")
                        Return
                End Select
            End Using

            Using gMain As Graphics = Graphics.FromImage(bmp)
                gMain.DrawImageUnscaled(gradBmp, 0, 0)
            End Using
        End Using

        Debug.WriteLine($"Sfumatura {tipo} da {p(0)} a {p(1)} direzione {p(3)} applicata su {percentuale}% della bitmap.")
    End Sub

    Private Sub Spirale(param As List(Of String), g As Graphics, ctx As RobotContext)
        Try
            If param.Count < 7 Then
                Debug.WriteLine("SPIRALE: parametri insufficienti")
                Return
            End If

            'Leggo i parametri
            Dim centro = Punto(param(0)) 'X,Y
            Dim raggioIniziale = CDbl(param(1))
            Dim raggioFinale = CDbl(param(2))
            Dim giri = CDbl(param(3))
            Dim colore = ColorConv(param(4))
            Dim spessore = CInt(param(5))
            Dim direzione = param(6).ToUpper()

            Dim a = ToScreen(centro, ctx)

            'Calcolo numero totale di punti
            Dim puntiTotali = CInt(360 * giri)
            Dim penna As New Pen(colore, spessore)

            'Costante per la crescita del raggio ad ogni punto
            Dim deltaRaggio = (raggioFinale - raggioIniziale) / puntiTotali

            Dim angoloStep = 2 * Math.PI / 360 '1 grado in radianti
            Dim theta = 0.0
            Dim raggio = raggioIniziale
            Dim x0 = a.X + raggio * Math.Cos(theta)
            Dim y0 = a.Y + raggio * Math.Sin(theta)

            For i = 1 To puntiTotali
                'Aggiorno l'angolo
                theta += If(direzione = "ORARIA", angoloStep, -angoloStep)
                'Aggiorno il raggio
                raggio += deltaRaggio

                'Nuovo punto
                Dim x1 = a.X + raggio * Math.Cos(theta)
                Dim y1 = a.Y + raggio * Math.Sin(theta)

                'Disegno linea tra punti
                g.DrawLine(penna, CSng(x0), CSng(y0), CSng(x1), CSng(y1))

                'Aggiorno punto precedente
                x0 = x1
                y0 = y1
            Next

            penna.Dispose()
            Debug.WriteLine($"SPIRALE disegnata: centro({a.X},{a.Y}), raggio {raggioIniziale}->{raggioFinale}, giri {giri}, colore {colore.Name}")
        Catch ex As Exception
            Debug.WriteLine("Errore SPIRALE: " & ex.Message)
        End Try
    End Sub

    Private Sub Sinusoide(param As List(Of String), g As Graphics, ctx As RobotContext)
        Try
            If param.Count < 6 Then
                Debug.WriteLine("SINUSOIDE: parametri insufficienti")
                Return
            End If

            Dim startP = Punto(param(0)) 'StartX,StartY
            Dim endP = Punto(param(1))   'EndX,EndY
            Dim amp = CDbl(param(2))     'Ampiezza in pixel
            Dim freq = CDbl(param(3))    'Frequenza in pixel
            Dim colore = ColorConv(param(4))
            Dim spessore = CInt(param(5))

            Dim a = ToScreen(startP, ctx)
            Dim b = ToScreen(endP, ctx)

            Dim penna As New Pen(colore, spessore)

            'Calcolo lunghezza totale
            Dim dx = b.X - a.X
            Dim dy = b.Y - a.Y
            Dim lunghezza = Math.Sqrt(dx * dx + dy * dy)

            'Numero di punti (un punto per pixel in X)
            Dim puntiTotali = CInt(Math.Abs(dx))

            'Direzione normalizzata
            Dim angle = Math.Atan2(dy, dx)
            Dim cosA = Math.Cos(angle)
            Dim sinA = Math.Sin(angle)

            'Punto precedente
            Dim x0 = a.X
            Dim y0 = a.Y

            For i = 1 To puntiTotali
                Dim t = i / puntiTotali 'progressione 0→1
                Dim x = a.X + t * dx
                'y = sin(2π * (x / freq)) * amp
                Dim y = a.Y + Math.Sin(2 * Math.PI * (i / freq)) * amp

                'Ruotiamo la curva se dy <> 0
                Dim xr = a.X + t * dx
                Dim yr = a.Y + Math.Sin(2 * Math.PI * (i / freq)) * amp

                'Linee tra punti
                g.DrawLine(penna, CSng(x0), CSng(y0), CSng(xr), CSng(yr))

                x0 = xr
                y0 = yr
            Next

            penna.Dispose()
            Debug.WriteLine($"SINUSOIDE disegnata: {a.X},{a.Y} → {b.X},{b.Y}, ampiezza {amp}, freq {freq}, colore {colore.Name}")
        Catch ex As Exception
            Debug.WriteLine("Errore SINUSOIDE: " & ex.Message)
        End Try
    End Sub

    Private Sub Croce(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 2 Then Return

        Dim p1 = Punto(p(0))
        Dim p2 = Punto(p(1))

        Dim a = ToScreen(p1, ctx)
        Dim b = ToScreen(p2, ctx)

        Dim colore As Color = If(p.Count > 2, ColorConv(p(2)), Color.Black)
        Dim spessore As Single = If(p.Count > 3, CSng(p(3)), 1)

        Dim minX = Math.Min(a.X, b.X)
        Dim maxX = Math.Max(a.X, b.X)
        Dim minY = Math.Min(a.Y, b.Y)
        Dim maxY = Math.Max(a.Y, b.Y)

        Dim cx As Single = (minX + maxX) / 2.0F
        Dim cy As Single = (minY + maxY) / 2.0F

        Using pen As New Pen(colore, spessore)
            g.DrawLine(pen, New PointF(cx, minY), New PointF(cx, maxY))
            g.DrawLine(pen, New PointF(minX, cy), New PointF(maxX, cy))
        End Using
    End Sub

    Private Sub Bezier(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 4 Then Return

        Dim p1 = Punto(p(0))
        Dim p2 = Punto(p(1))
        Dim p3 = Punto(p(2))
        Dim p4 = Punto(p(3))

        Dim a = ToScreen(p1, ctx)
        Dim b = ToScreen(p2, ctx)
        Dim c = ToScreen(p3, ctx)
        Dim d = ToScreen(p4, ctx)

        Dim colore As Color = If(p.Count > 4, ColorConv(p(4)), Color.Black)
        Dim spessore As Single = If(p.Count > 5, CSng(p(5)), 1)

        Using pen As New Pen(colore, spessore)
            g.DrawBezier(pen, a, b, c, d)
        End Using
    End Sub

    Private Sub TextureLoad(p As List(Of String), ctx As RobotContext)
        If p.Count < 2 Then Return

        Dim nome As String = p(0)
        Dim path As String = p(1)

        If Not IO.File.Exists(path) Then Return

        ' Se esiste già, libera la vecchia texture
        If ctx.Textures.ContainsKey(nome) Then
            ctx.Textures(nome).Dispose()
            ctx.Textures.Remove(nome)
        End If

        ' Caricamento sicuro (file NON bloccato)
        Using fs As New IO.FileStream(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
            Using tmpBmp As New Bitmap(fs)
                ' Clona per svincolarsi completamente dal FileStream
                ctx.Textures(nome) = New Bitmap(tmpBmp)
            End Using
        End Using
    End Sub

    Private Sub TextureDraw(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 2 Then Return

        Dim nome = p(0)
        If Not ctx.Textures.ContainsKey(nome) Then Return

        Dim pos = Punto(p(1))
        Dim a = ToScreen(pos, ctx)

        Dim scala As Single = If(p.Count > 2, CSng(p(2)), 1.0F)

        Dim bmp = ctx.Textures(nome)
        Dim w = bmp.Width * scala
        Dim h = bmp.Height * scala

        g.DrawImage(bmp, a.X, a.Y, w, h)
    End Sub

    Private Sub PatternCreate(p As List(Of String), ctx As RobotContext)
        If p.Count < 6 Then Return

        Dim nome = p(0)
        Dim dashStr = p(1).ToUpper()

        Dim dash As DashStyle = DashStyle.Solid
        Select Case dashStr
            Case "DOT" : dash = DashStyle.Dot
            Case "DASH" : dash = DashStyle.Dash
            Case "DASHDOT" : dash = DashStyle.DashDot
            Case "DASHDOTDOT" : dash = DashStyle.DashDotDot
        End Select

        ctx.Patterns(nome) = New PatternDef With {
        .Dash = dash,
        .Angolo = CSng(p(2)),
        .Spaziatura = CSng(p(3)),
        .Colore = ColorConv(p(4)),
        .Spessore = CSng(p(5))
    }
    End Sub

    Private Function CreaPatternLinee(angolo As Single, spacing As Single, colore As Color, spessore As Single) As Brush
        'Questo usa DrawLine per costruire la texture
        Dim size As Integer = CInt(spacing * 4)
        Dim bmp As New Bitmap(size, size)

        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

            Using pen As New Pen(colore, spessore)
                g.TranslateTransform(size / 2, size / 2)
                g.RotateTransform(angolo)
                g.DrawLine(pen, -size, 0, size, 0)
                g.ResetTransform()
            End Using
        End Using

        Return New TextureBrush(bmp, Drawing2D.WrapMode.Tile)
    End Function

    Private Sub PatternFill(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 3 Then Return

        Dim nome = p(0)
        If Not ctx.Patterns.ContainsKey(nome) Then Return

        Dim pat = ctx.Patterns(nome)

        Dim p1 = Punto(p(1))
        Dim p2 = Punto(p(2))

        Dim a = ToScreen(p1, ctx)
        Dim b = ToScreen(p2, ctx)

        Dim rect As New RectangleF(
        Math.Min(a.X, b.X),
        Math.Min(a.Y, b.Y),
        Math.Abs(b.X - a.X),
        Math.Abs(b.Y - a.Y)
    )

        Using pen As New Pen(pat.Colore, pat.Spessore)
            pen.DashStyle = pat.Dash

            Dim state As Drawing2D.GraphicsState = g.Save()

            g.TranslateTransform(rect.X + rect.Width / 2, rect.Y + rect.Height / 2)
            g.RotateTransform(pat.Angolo)

            Dim maxLen As Single = Math.Max(rect.Width, rect.Height) * 2
            Dim y As Single = -maxLen

            While y <= maxLen
                g.DrawLine(pen, -maxLen, y, maxLen, y)
                y += pat.Spaziatura
            End While

            g.Restore(state)
        End Using
    End Sub

    Private Sub Inizio(p As List(Of String), g As Graphics, ctx As RobotContext)
        g.Clear(Color.White)

        Dim w = ctx.Bitmap.Width
        Dim h = ctx.Bitmap.Height
        Dim bordo As Integer = 5

        Using pen As New Pen(Color.Black, 1)
            g.DrawRectangle(pen, bordo, bordo, w - bordo * 2, h - bordo * 2)
        End Using
    End Sub


    Private Sub InizioCad(p As List(Of String), g As Graphics, ctx As RobotContext)
        ' Parametri
        Dim passo As Integer = 10
        Dim colorePunti As Color = Color.Lime

        If p.Count > 0 Then Integer.TryParse(p(0), passo)
        If p.Count > 1 Then
            Try
                colorePunti = ColorConv(p(1))
            Catch
            End Try
        End If

        g.Clear(Color.Black)

        Dim w = ctx.Bitmap.Width
        Dim h = ctx.Bitmap.Height
        Dim bordo As Integer = 5

        ' Rettangolo esterno
        Using pen As New Pen(Color.Lime, 1)
            g.DrawRectangle(pen, bordo, bordo, w - bordo * 2, h - bordo * 2)
        End Using

        ' Puntini griglia stile CAD
        Using brush As New SolidBrush(colorePunti)
            For x = bordo To w - bordo Step passo
                For y = bordo To h - bordo Step passo
                    g.FillEllipse(brush, x - 1, y - 1, 2, 2)
                Next
            Next
        End Using
    End Sub

    Private Sub InizioMath(p As List(Of String), g As Graphics, ctx As RobotContext)
        ' Parametri
        Dim stepX As Integer = 10
        Dim stepY As Integer = 10
        Dim coloreScale As Color = Color.Gray

        If p.Count > 0 Then Integer.TryParse(p(0), stepX)
        If p.Count > 1 Then Integer.TryParse(p(1), stepY)
        If p.Count > 2 Then
            Try
                coloreScale = ColorConv(p(2))
            Catch
            End Try
        End If

        g.Clear(Color.White)
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Dim w = ctx.Bitmap.Width
        Dim h = ctx.Bitmap.Height
        Dim cx = w \ 2
        Dim cy = h \ 2

        ' Origine nota
        ctx.StartPoint = New PointF(0, 0)
        ctx.LastPoint = New PointF(0, 0)

        ' Griglia
        Using penGrid As New Pen(Color.LightGray, 1)
            For x = 0 To w Step stepX
                g.DrawLine(penGrid, x, 0, x, h)
            Next
            For y = 0 To h Step stepY
                g.DrawLine(penGrid, 0, y, w, y)
            Next
        End Using

        ' Assi cartesiani
        Using penAxis As New Pen(Color.Black, 2)
            g.DrawLine(penAxis, cx, 0, cx, h)
            g.DrawLine(penAxis, 0, cy, w, cy)
        End Using

        ' Tacche sull’asse X
        Using penScale As New Pen(coloreScale, 1)
            For x = cx To w Step stepX
                g.DrawLine(penScale, x, cy - 5, x, cy + 5)
            Next
            For x = cx To 0 Step -stepX
                g.DrawLine(penScale, x, cy - 5, x, cy + 5)
            Next
        End Using

        ' Tacche sull’asse Y
        Using penScale As New Pen(coloreScale, 1)
            For y = cy To h Step stepY
                g.DrawLine(penScale, cx - 5, y, cx + 5, y)
            Next
            For y = cy To 0 Step -stepY
                g.DrawLine(penScale, cx - 5, y, cx + 5, y)
            Next
        End Using

        ' Origine
        Using brush As New SolidBrush(Color.Red)
            g.FillEllipse(brush, cx - 3, cy - 3, 6, 6)
        End Using
    End Sub

    Private Sub AddLivello(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 1 Then Return

        Dim nome = p(0).Trim()
        If ctx.Livelli.ContainsKey(nome) Then Return

        Dim bmp As Bitmap = CType(ctx.Bitmap.Clone(), Bitmap)

        ctx.Livelli.Add(nome, New Livello With {
        .Nome = nome,
        .Bitmap = bmp
    })

        ctx.OrdineLivelli.Add(nome)
        ctx.LivelloCorrente = nome

        ' Concettualmente dovresti cancellare in TRASPARENTE
        'g.Clear(Color.White)
    End Sub

    Private Sub DelLivello(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 1 Then Return

        Dim nome = p(0).Trim()
        If nome = "BackGround" Then Return
        If Not ctx.Livelli.ContainsKey(nome) Then Return

        Dim idx = ctx.OrdineLivelli.IndexOf(nome)
        If idx <= 0 Then Return

        Dim nomePrecedente = ctx.OrdineLivelli(idx - 1)
        Dim bmpPrev = ctx.Livelli(nomePrecedente).Bitmap

        ' Ripristina snapshot precedente
        g.Clear(Color.White)
        g.DrawImageUnscaled(bmpPrev, 0, 0)

        ' Rimuove livello
        ctx.Livelli(nome).Bitmap.Dispose()
        ctx.Livelli.Remove(nome)
        ctx.OrdineLivelli.RemoveAt(idx)

        ctx.LivelloCorrente = nomePrecedente
    End Sub

    Private Sub RenLivello(p As List(Of String), g As Graphics, ctx As RobotContext)
        'Rinomina solo il dizionario piu lista
        If p.Count < 2 Then Return

        Dim oldName = p(0).Trim()
        Dim newName = p(1).Trim()

        If Not ctx.Livelli.ContainsKey(oldName) Then Return
        If ctx.Livelli.ContainsKey(newName) Then Return

        Dim liv = ctx.Livelli(oldName)
        liv.Nome = newName

        ctx.Livelli.Remove(oldName)
        ctx.Livelli.Add(newName, liv)

        Dim idx = ctx.OrdineLivelli.IndexOf(oldName)
        ctx.OrdineLivelli(idx) = newName

        If ctx.LivelloCorrente = oldName Then
            ctx.LivelloCorrente = newName
        End If
    End Sub

    Private Sub GrigliaFull(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 2 Then Return

        Dim lato = Integer.Parse(p(0))
        Dim colore = ColorConv(p(1))

        Using pen As New Pen(colore, 1)
            ' Verticali
            For x = 0 To 9999 Step lato
                g.DrawLine(pen, x, 0, x, 9999)
            Next

            ' Orizzontali
            For y = 0 To 9999 Step lato
                g.DrawLine(pen, 0, y, 9999, y)
            Next
        End Using
    End Sub

    Private Sub Freccia(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 3 Then Return

        Dim p1 = Punto(p(0))
        Dim p2 = Punto(p(1))
        Dim colore = ColorConv(p(2))
        Dim spessore = Integer.Parse(p(3))

        Dim a = ToScreen(p1, ctx)
        Dim b = ToScreen(p2, ctx)

        Using pen As New Pen(colore, spessore)
            g.DrawLine(pen, a, b)

            Dim angolo = Math.Atan2(a.Y - b.Y, a.X - b.X)
            Dim lung = 10 + spessore * 2

            Dim a1 = angolo + Math.PI / 6
            Dim a2 = angolo - Math.PI / 6

            Dim pA As New PointF(
            p2.X + lung * Math.Cos(a1),
            p2.Y + lung * Math.Sin(a1))

            Dim pB As New PointF(
            p2.X + lung * Math.Cos(a2),
            p2.Y + lung * Math.Sin(a2))

            g.DrawLine(pen, b, pA)
            g.DrawLine(pen, b, pB)
        End Using
    End Sub

    Private Sub Stella(p As List(Of String), g As Graphics, ctx As RobotContext)
        If p.Count < 4 Then Return

        Dim centro = Punto(p(0))
        Dim punte = Integer.Parse(p(1))
        Dim diametro = Single.Parse(p(2))
        ' Colori
        Dim colore1 = ColorConv(p(3))
        Dim spessore = Integer.Parse(p(4))

        Dim a = ToScreen(centro, ctx)

        Dim rEst = diametro / 2
        Dim rInt = rEst / 2

        Dim pts As New List(Of PointF)
        Dim stepAng = Math.PI / punte

        For i = 0 To punte * 2 - 1
            Dim r = If(i Mod 2 = 0, rEst, rInt)
            Dim ang = i * stepAng - Math.PI / 2

            pts.Add(New PointF(
            a.X + r * Math.Cos(ang),
            a.Y + r * Math.Sin(ang)))
        Next

        g.DrawPolygon(New Pen(colore1, spessore), pts.ToArray())
    End Sub

    Private Sub MatriceQuadrata(p As List(Of String), g As Graphics, ctx As RobotContext)
        ' Metrice Quadrata
        'Significato
        'x1, y1 → punto iniziale
        'x2, y2 → passo (distanza tra elementi)
        'xN, yN → dimensioni totali (oppure punto finale)
        'tipo → come disegnare ogni cella
        'colore
        'spessore → dimensione del simbolo

        If p.Count < 5 Then Return

        Dim start = Punto(p(0))
        Dim fine = Punto(p(1))
        Dim passo = Punto(p(2))
        Dim tipo As String = p(3).ToUpper()
        Dim colore As Color = ColorConv(p(4).ToUpper())
        Dim s As Integer = Integer.Parse(p(5)) 'spessore

        Dim a = ToScreen(start, ctx)
        Dim b = ToScreen(fine, ctx)
        Dim c = ToScreen(passo, ctx)

        'Debug.WriteLine("MATRICE QUADRATA " & start.ToString & fine.ToString & passo.ToString & tipo.ToString & s.ToString)

        For x = a.X To b.X Step c.X
            For y = a.Y To b.Y Step c.Y
                Select Case tipo
                    Case "PUNTI"
                        g.FillEllipse(New SolidBrush(colore), x - s, y - s, s * 2, s * 2)

                    Case "QUADRATI"
                        g.DrawRectangle(New Pen(colore, 1), x - s, y - s, s * 2, s * 2)

                    Case "CROCI"
                        g.DrawLine(New Pen(colore, 1), x - s, y, x + s, y)
                        g.DrawLine(New Pen(colore, 1), x, y - s, x, y + s)

                    Case "X"
                        g.DrawLine(New Pen(colore, 1), x - s, y - s, x + s, y + s)
                        g.DrawLine(New Pen(colore, 1), x - s, y + s, x + s, y - s)
                End Select
            Next
        Next
    End Sub

    Private Sub MatricePoligono(p As List(Of String), g As Graphics, ctx As RobotContext)
        'MATRICE;x1,y1;x2,y2;x3,y3;...;PASSOX,PASSOY;Tipo;Colore;Spessore
        'esempio TRAPEZIO
        'MATRICE;100,200;300,200;250,350;150,350;20,20;punti;Malva;2

        ' Servono almeno:
        ' 3 punti + passo + tipo + colore + spessore = 7
        If p.Count < 7 Then Return

        ' --- 1. Leggi SOLO i punti del poligono ---
        Dim punti As New List(Of PointF)
        Dim numPunti As Integer = p.Count - 4   ' <-- FONDAMENTALE

        For i As Integer = 0 To numPunti - 1
            Dim p1 = Punto(p(i))
            Dim a = ToScreen(p1, ctx)
            punti.Add(a)
        Next

        ' --- 2. Parametri ---
        Dim passo As PointF = Punto(p(numPunti))
        Dim b = ToScreen(passo, ctx)

        Dim tipo As String = p(numPunti + 1).ToUpper()
        Dim colore As Color = ColorConv(p(numPunti + 2).ToUpper())
        Dim spessore As Integer = Integer.Parse(p(numPunti + 3))

        ' --- 3. Bounding box ---
        Dim minX, minY, maxX, maxY As Single
        BoundingBox(punti, minX, minY, maxX, maxY)

        ' --- 4. Clip ---
        Dim oldClip = g.Clip
        Using path = CreaClipPoligono(punti)
            g.SetClip(path)

            ' --- 5. Disegno matrice ---
            For x = minX To maxX Step b.X
                For y = minY To maxY Step b.Y
                    DisegnaSimbolo(g, tipo, x, y, colore, spessore)
                Next
            Next

            ' --- 6. Ripristino ---
            g.Clip = oldClip
        End Using
    End Sub

    Private Function CreaClipPoligono(punti As List(Of PointF)) As Drawing2D.GraphicsPath
        Dim path As New Drawing2D.GraphicsPath()
        path.AddPolygon(punti.ToArray())
        Return path
    End Function

    Private Sub BoundingBox(punti As List(Of PointF),
                        ByRef minX As Single, ByRef minY As Single,
                        ByRef maxX As Single, ByRef maxY As Single)

        minX = punti.Min(Function(p) p.X)
        minY = punti.Min(Function(p) p.Y)
        maxX = punti.Max(Function(p) p.X)
        maxY = punti.Max(Function(p) p.Y)
    End Sub

    Private Sub DisegnaSimbolo(g As Graphics, tipo As String, x As Single, y As Single, colore As Color, s As Integer)
        'Solo per il disegno della matrice poligono
        Select Case tipo
            Case "PUNTI"
                g.FillEllipse(New SolidBrush(colore), x - s, y - s, s * 2, s * 2)

            Case "QUADRATI"
                g.DrawRectangle(New Pen(colore, 1), x - s, y - s, s * 2, s * 2)

            Case "CROCI"
                g.DrawLine(New Pen(colore, 1), x - s, y, x + s, y)
                g.DrawLine(New Pen(colore, 1), x, y - s, x, y + s)

            Case "X"
                g.DrawLine(New Pen(colore, 1), x - s, y - s, x + s, y + s)
                g.DrawLine(New Pen(colore, 1), x - s, y + s, x + s, y - s)
        End Select
    End Sub
End Class




Public Class SvgDrawer

    ' Tiene traccia del livello corrente, gruppo
    Private livelloCorrente As String = Nothing


    ' Mappa colori italiani → CSS
    Private Shared Function ColorToCss(nome As String) As String
        Dim key = nome.ToUpper().Trim()

        If RobotDrawer.ColoriItaliani.ContainsKey(key) Then
            Dim c = RobotDrawer.ColoriItaliani(key)
            Return $"rgb({c.R},{c.G},{c.B})"
        End If

        ' fallback: prova nome inglese
        Return nome
    End Function


    ' Funzione principale
    Public Function GeneraSVG(history As List(Of String),
                              width As Integer,
                              height As Integer) As String

        Dim sb As New Text.StringBuilder()

        ' Header SVG
        sb.AppendLine($"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{width}"" height=""{height}"" viewBox=""0 0 {width} {height}"">")

        ' Corpo
        For Each line In history
            Dim parti = line.Split(";"c)
            Dim tipo = parti(0).ToUpper()

            ' --- Gestione livelli ---
            If tipo = "ADDLIVELLO" Then
                ' Chiudi gruppo precedente
                If livelloCorrente IsNot Nothing Then
                    sb.AppendLine("  </g>")
                End If

                ' Apri nuovo gruppo
                livelloCorrente = parti(1)
                sb.AppendLine($"  <g id=""{livelloCorrente}"">")
                Continue For
            End If

            ' --- Comandi normali ---
            Dim svg = ConvertiRiga(line)
            If svg IsNot Nothing Then
                sb.AppendLine("    " & svg)
            End If
        Next

        ' Chiudi ultimo gruppo
        If livelloCorrente IsNot Nothing Then
            sb.AppendLine("  </g>")
        End If

        ' Footer
        sb.AppendLine("</svg>")

        Return sb.ToString()
    End Function



    ' Converte una singola riga CAD in SVG
    Private Function ConvertiRiga(riga As String) As String
        If String.IsNullOrWhiteSpace(riga) Then Return Nothing
        If riga.StartsWith("#") Then Return Nothing

        Dim parti = riga.Split(";"c)
        Dim tipo = parti(0).ToUpper()

        ' Esce se trova ADDLIVELLO
        If tipo = "ADDLIVELLO" Then Return Nothing

        Select Case tipo

            Case "LINEA"
                Return SvgLinea(parti)

            Case "CERCHIO"
                Return SvgCerchio(parti)

            Case "RETT"
                Return SvgRett(parti)

            Case "POLIGONO"
                Return SvgPoligono(parti)

            Case "TESTO"
                Return SvgTesto(parti)

            Case "ARCO"
                Return SvgArco(parti)

            Case "BEZIER"
                Return SvgBezier(parti)

            Case "SPLINE", "SPLINE2"
                Return SvgSpline(parti)

            Case "GRIGLIA"
                Return SvgGriglia(parti)

            Case "GRIGLIAFULL"
                Return SvgGrigliaFull(parti)


            Case Else
                ' Comandi non vettoriali → ignorati
                Return Nothing
        End Select
    End Function



    ' -------------------------
    '   SVG PRIMITIVE
    ' -------------------------

    Private Function Punto(s As String) As PointF
        Dim xy = s.Split(","c)
        Return New PointF(CSng(xy(0)), CSng(xy(1)))
    End Function


    ' LINEA;x1,y1;x2,y2;Colore;Spessore
    Private Function SvgLinea(p() As String) As String
        If p.Length < 4 Then Return Nothing

        Dim a = Punto(p(1))
        Dim b = Punto(p(2))
        Dim colore = ColorToCss(p(3))
        Dim spessore = If(p.Length > 4, p(4), "1")

        Return $"<line x1=""{a.X}"" y1=""{a.Y}"" x2=""{b.X}"" y2=""{b.Y}"" stroke=""{colore}"" stroke-width=""{spessore}"" />"
    End Function


    ' CERCHIO;x,y;raggio;Colore;PIENO/VUOTO;spessore
    Private Function SvgCerchio(p() As String) As String
        If p.Length < 4 Then Return Nothing

        Dim c = Punto(p(1))
        Dim r = p(2)
        Dim colore = ColorToCss(p(3))
        Dim pieno = If(p.Length > 4 AndAlso p(4).ToUpper() = "PIENO", colore, "none")
        Dim bordo = If(p.Length > 4 AndAlso p(4).ToUpper() = "PIENO", "none", colore)
        Dim spessore = If(p.Length > 5, p(5), "1")

        Return $"<circle cx=""{c.X}"" cy=""{c.Y}"" r=""{r}"" fill=""{pieno}"" stroke=""{bordo}"" stroke-width=""{spessore}"" />"
    End Function


    ' RETT;x1,y1;x2,y2;Colore;PIENO/VUOTO;Spessore
    Private Function SvgRett(p() As String) As String
        If p.Length < 4 Then Return Nothing

        Dim a = Punto(p(1))
        Dim b = Punto(p(2))
        Dim colore = ColorToCss(p(3))
        Dim pieno = If(p.Length > 4 AndAlso p(4).ToUpper() = "PIENO", colore, "none")
        Dim bordo = If(p.Length > 4 AndAlso p(4).ToUpper() = "PIENO", "none", colore)
        Dim spessore = If(p.Length > 5, p(5), "1")

        Dim x = Math.Min(a.X, b.X)
        Dim y = Math.Min(a.Y, b.Y)
        Dim w = Math.Abs(b.X - a.X)
        Dim h = Math.Abs(b.Y - a.Y)

        Return $"<rect x=""{x}"" y=""{y}"" width=""{w}"" height=""{h}"" fill=""{pieno}"" stroke=""{bordo}"" stroke-width=""{spessore}"" />"
    End Function


    ' POLIGONO;...;Colore;PIENO/VUOTO;Spessore
    Private Function SvgPoligono(p() As String) As String
        If p.Length < 4 Then Return Nothing

        Dim punti As New List(Of String)
        Dim i As Integer = 1

        While i < p.Length AndAlso p(i).Contains(",")
            Dim pt = Punto(p(i))
            punti.Add($"{pt.X},{pt.Y}")
            i += 1
        End While

        Dim colore = ColorToCss(p(i))
        Dim pieno = If(p(i + 1).ToUpper() = "PIENO", colore, "none")
        Dim bordo = If(p(i + 1).ToUpper() = "PIENO", "none", colore)
        Dim spessore = If(p.Length > i + 2, p(i + 2), "1")

        Return $"<polygon points=""{String.Join(" ", punti)}"" fill=""{pieno}"" stroke=""{bordo}"" stroke-width=""{spessore}"" />"
    End Function


    ' TESTO;x,y;Testo;Colore;Size;Font;Stile
    Private Function SvgTesto(p() As String) As String
        If p.Length < 4 Then Return Nothing

        Dim pos = Punto(p(1))
        Dim testo = p(2)
        Dim colore = ColorToCss(p(3))
        Dim size = If(p.Length > 4, p(4), "16")
        Dim font = If(p.Length > 5, p(5), "Arial")

        Return $"<text x=""{pos.X}"" y=""{pos.Y}"" fill=""{colore}"" font-size=""{size}"" font-family=""{font}"">{testo}</text>"
    End Function


    ' ARCO → convertito in path SVG
    Private Function SvgArco(p() As String) As String
        If p.Length < 7 Then Return Nothing

        Dim a = Punto(p(1))
        Dim b = Punto(p(2))
        Dim colore = ColorToCss(p(3))
        Dim spessore = If(p.Length > 5, p(5), "1")
        Dim startAngle = CSng(p(5))
        Dim sweepAngle = CSng(p(6))

        ' bounding box
        Dim x = Math.Min(a.X, b.X)
        Dim y = Math.Min(a.Y, b.Y)
        Dim w = Math.Abs(b.X - a.X)
        Dim h = Math.Abs(b.Y - a.Y)

        ' SVG arc → path
        Return $"<path d=""M {x},{y} A {w / 2},{h / 2} 0 0,1 {x + w},{y + h}"" stroke=""{colore}"" fill=""none"" stroke-width=""{spessore}"" />"
    End Function


    ' BEZIER;x1,y1;x2,y2;x3,y3;x4,y4;Colore;Spessore
    Private Function SvgBezier(p() As String) As String
        If p.Length < 6 Then Return Nothing

        Dim p1 = Punto(p(1))
        Dim p2 = Punto(p(2))
        Dim p3 = Punto(p(3))
        Dim p4 = Punto(p(4))
        Dim colore = ColorToCss(p(5))
        Dim spessore = If(p.Length > 6, p(6), "1")

        Return $"<path d=""M {p1.X},{p1.Y} C {p2.X},{p2.Y} {p3.X},{p3.Y} {p4.X},{p4.Y}"" stroke=""{colore}"" fill=""none"" stroke-width=""{spessore}"" />"
    End Function


    ' SPLINE → convertita in polyline
    Private Function SvgSpline(p() As String) As String
        Dim punti As New List(Of String)
        Dim i As Integer = 1

        While i < p.Length AndAlso p(i).Contains(",")
            Dim pt = Punto(p(i))
            punti.Add($"{pt.X},{pt.Y}")
            i += 1
        End While

        If punti.Count < 2 Then Return Nothing

        Dim colore = ColorToCss(p(i))
        Dim spessore = If(p.Length > i + 1, p(i + 1), "1")

        Return $"<polyline points=""{String.Join(" ", punti)}"" fill=""none"" stroke=""{colore}"" stroke-width=""{spessore}"" />"
    End Function

    ' GRIGLIA → convertita in polyline
    Private Function SvgGriglia(p() As String) As String
        If p.Length < 3 Then Return Nothing

        Dim passo As Integer = Integer.Parse(p(1))
        Dim colore = ColorToCss(p(2))

        Dim sb As New Text.StringBuilder()

        ' Linee verticali
        For x = 0 To 10000 Step passo
            sb.AppendLine($"<line x1=""{x}"" y1=""0"" x2=""{x}"" y2=""10000"" stroke=""{colore}"" stroke-width=""1"" />")
        Next

        ' Linee orizzontali
        For y = 0 To 10000 Step passo
            sb.AppendLine($"<line x1=""0"" y1=""{y}"" x2=""10000"" y2=""{y}"" stroke=""{colore}"" stroke-width=""1"" />")
        Next

        Return sb.ToString()
    End Function

    ' GRIGLIAFULL → convertita in polyline
    Private Function SvgGrigliaFull(p() As String) As String
        ' Per ora identica a GRIGLIA
        Return SvgGriglia(p)
    End Function

End Class
