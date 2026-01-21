<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Richiesto da Progettazione Windows Form
    Private components As System.ComponentModel.IContainer

    'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
    'Può essere modificata in Progettazione Windows Form.  
    'Non modificarla mediante l'editor del codice.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.PanelTop = New System.Windows.Forms.Panel()
        Me.ButtonCentroMondo = New System.Windows.Forms.Button()
        Me.ButtonRenderRemain = New System.Windows.Forms.Button()
        Me.ButtonHistory = New System.Windows.Forms.Button()
        Me.ButtonContinueRendering = New System.Windows.Forms.Button()
        Me.ButtonTest = New System.Windows.Forms.Button()
        Me.ButtonSavePaintRobot = New System.Windows.Forms.Button()
        Me.LabelEseguiti = New System.Windows.Forms.Label()
        Me.LabelNumCmds = New System.Windows.Forms.Label()
        Me.LabelCommand = New System.Windows.Forms.Label()
        Me.LabelOrigine = New System.Windows.Forms.Label()
        Me.ButtonOut = New System.Windows.Forms.Button()
        Me.ButtonZoomIn = New System.Windows.Forms.Button()
        Me.ButtonOrigin = New System.Windows.Forms.Button()
        Me.ButtonRight = New System.Windows.Forms.Button()
        Me.ButtonDown = New System.Windows.Forms.Button()
        Me.ButtonUp = New System.Windows.Forms.Button()
        Me.ButtonLeft = New System.Windows.Forms.Button()
        Me.ButtonStop = New System.Windows.Forms.Button()
        Me.LabelCommands = New System.Windows.Forms.Label()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.TextBoxCommands = New System.Windows.Forms.TextBox()
        Me.ButtonCmd = New System.Windows.Forms.Button()
        Me.ButtonMinimize = New System.Windows.Forms.Button()
        Me.ButtonClose = New System.Windows.Forms.Button()
        Me.PictureBoxPR = New System.Windows.Forms.PictureBox()
        Me.PanelLeft = New System.Windows.Forms.Panel()
        Me.ButtonColors = New System.Windows.Forms.Button()
        Me.ButtonRiavvia = New System.Windows.Forms.Button()
        Me.ButtonAttivaListBox = New System.Windows.Forms.Button()
        Me.ButtonScript = New System.Windows.Forms.Button()
        Me.ButtonEditHistory = New System.Windows.Forms.Button()
        Me.ButtonHelp = New System.Windows.Forms.Button()
        Me.LabelCoord = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.LstComandi = New System.Windows.Forms.ListBox()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.PanelTop.SuspendLayout()
        CType(Me.PictureBoxPR, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelLeft.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PanelTop
        '
        Me.PanelTop.BackColor = System.Drawing.Color.Black
        Me.PanelTop.Controls.Add(Me.ButtonCentroMondo)
        Me.PanelTop.Controls.Add(Me.ButtonRenderRemain)
        Me.PanelTop.Controls.Add(Me.ButtonHistory)
        Me.PanelTop.Controls.Add(Me.ButtonContinueRendering)
        Me.PanelTop.Controls.Add(Me.ButtonTest)
        Me.PanelTop.Controls.Add(Me.ButtonSavePaintRobot)
        Me.PanelTop.Controls.Add(Me.LabelEseguiti)
        Me.PanelTop.Controls.Add(Me.LabelNumCmds)
        Me.PanelTop.Controls.Add(Me.LabelCommand)
        Me.PanelTop.Controls.Add(Me.LabelOrigine)
        Me.PanelTop.Controls.Add(Me.ButtonOut)
        Me.PanelTop.Controls.Add(Me.ButtonZoomIn)
        Me.PanelTop.Controls.Add(Me.ButtonOrigin)
        Me.PanelTop.Controls.Add(Me.ButtonRight)
        Me.PanelTop.Controls.Add(Me.ButtonDown)
        Me.PanelTop.Controls.Add(Me.ButtonUp)
        Me.PanelTop.Controls.Add(Me.ButtonLeft)
        Me.PanelTop.Controls.Add(Me.ButtonStop)
        Me.PanelTop.Controls.Add(Me.LabelCommands)
        Me.PanelTop.Controls.Add(Me.ProgressBar1)
        Me.PanelTop.Controls.Add(Me.TextBoxCommands)
        Me.PanelTop.Controls.Add(Me.ButtonCmd)
        Me.PanelTop.Controls.Add(Me.ButtonMinimize)
        Me.PanelTop.Controls.Add(Me.ButtonClose)
        Me.PanelTop.Controls.Add(Me.PictureBoxPR)
        Me.PanelTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.PanelTop.Location = New System.Drawing.Point(0, 0)
        Me.PanelTop.Name = "PanelTop"
        Me.PanelTop.Size = New System.Drawing.Size(1548, 60)
        Me.PanelTop.TabIndex = 1
        '
        'ButtonCentroMondo
        '
        Me.ButtonCentroMondo.BackColor = System.Drawing.Color.White
        Me.ButtonCentroMondo.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonCentroMondo.ForeColor = System.Drawing.Color.Black
        Me.ButtonCentroMondo.Location = New System.Drawing.Point(995, 5)
        Me.ButtonCentroMondo.Name = "ButtonCentroMondo"
        Me.ButtonCentroMondo.Size = New System.Drawing.Size(25, 25)
        Me.ButtonCentroMondo.TabIndex = 23
        Me.ButtonCentroMondo.Text = "C"
        Me.ButtonCentroMondo.UseVisualStyleBackColor = False
        '
        'ButtonRenderRemain
        '
        Me.ButtonRenderRemain.BackColor = System.Drawing.Color.Green
        Me.ButtonRenderRemain.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonRenderRemain.ForeColor = System.Drawing.Color.White
        Me.ButtonRenderRemain.Location = New System.Drawing.Point(720, 32)
        Me.ButtonRenderRemain.Name = "ButtonRenderRemain"
        Me.ButtonRenderRemain.Size = New System.Drawing.Size(80, 25)
        Me.ButtonRenderRemain.TabIndex = 22
        Me.ButtonRenderRemain.Text = "Restanti"
        Me.ButtonRenderRemain.UseVisualStyleBackColor = False
        Me.ButtonRenderRemain.Visible = False
        '
        'ButtonHistory
        '
        Me.ButtonHistory.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.ButtonHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonHistory.ForeColor = System.Drawing.Color.White
        Me.ButtonHistory.Location = New System.Drawing.Point(1094, 32)
        Me.ButtonHistory.Name = "ButtonHistory"
        Me.ButtonHistory.Size = New System.Drawing.Size(153, 25)
        Me.ButtonHistory.TabIndex = 21
        Me.ButtonHistory.Text = "Render History"
        Me.ButtonHistory.UseVisualStyleBackColor = False
        '
        'ButtonContinueRendering
        '
        Me.ButtonContinueRendering.BackColor = System.Drawing.Color.Green
        Me.ButtonContinueRendering.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonContinueRendering.ForeColor = System.Drawing.Color.White
        Me.ButtonContinueRendering.Location = New System.Drawing.Point(634, 32)
        Me.ButtonContinueRendering.Name = "ButtonContinueRendering"
        Me.ButtonContinueRendering.Size = New System.Drawing.Size(80, 25)
        Me.ButtonContinueRendering.TabIndex = 20
        Me.ButtonContinueRendering.Text = "Continue"
        Me.ButtonContinueRendering.UseVisualStyleBackColor = False
        Me.ButtonContinueRendering.Visible = False
        '
        'ButtonTest
        '
        Me.ButtonTest.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.ButtonTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonTest.ForeColor = System.Drawing.Color.White
        Me.ButtonTest.Location = New System.Drawing.Point(1191, 5)
        Me.ButtonTest.Name = "ButtonTest"
        Me.ButtonTest.Size = New System.Drawing.Size(56, 25)
        Me.ButtonTest.TabIndex = 18
        Me.ButtonTest.Text = "Test"
        Me.ButtonTest.UseVisualStyleBackColor = False
        Me.ButtonTest.Visible = False
        '
        'ButtonSavePaintRobot
        '
        Me.ButtonSavePaintRobot.BackColor = System.Drawing.Color.White
        Me.ButtonSavePaintRobot.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonSavePaintRobot.ForeColor = System.Drawing.Color.Black
        Me.ButtonSavePaintRobot.Location = New System.Drawing.Point(1094, 5)
        Me.ButtonSavePaintRobot.Name = "ButtonSavePaintRobot"
        Me.ButtonSavePaintRobot.Size = New System.Drawing.Size(80, 25)
        Me.ButtonSavePaintRobot.TabIndex = 17
        Me.ButtonSavePaintRobot.Text = "= SALVA ="
        Me.ButtonSavePaintRobot.UseVisualStyleBackColor = False
        '
        'LabelEseguiti
        '
        Me.LabelEseguiti.AutoSize = True
        Me.LabelEseguiti.ForeColor = System.Drawing.Color.White
        Me.LabelEseguiti.Location = New System.Drawing.Point(975, 38)
        Me.LabelEseguiti.Name = "LabelEseguiti"
        Me.LabelEseguiti.Size = New System.Drawing.Size(88, 13)
        Me.LabelEseguiti.TabIndex = 16
        Me.LabelEseguiti.Text = "Comandi Eseguiti"
        '
        'LabelNumCmds
        '
        Me.LabelNumCmds.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelNumCmds.ForeColor = System.Drawing.Color.White
        Me.LabelNumCmds.Location = New System.Drawing.Point(866, 32)
        Me.LabelNumCmds.Name = "LabelNumCmds"
        Me.LabelNumCmds.Size = New System.Drawing.Size(103, 25)
        Me.LabelNumCmds.TabIndex = 15
        Me.LabelNumCmds.Text = "00"
        Me.LabelNumCmds.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'LabelCommand
        '
        Me.LabelCommand.AutoSize = True
        Me.LabelCommand.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelCommand.ForeColor = System.Drawing.Color.White
        Me.LabelCommand.Location = New System.Drawing.Point(264, 39)
        Me.LabelCommand.Name = "LabelCommand"
        Me.LabelCommand.Size = New System.Drawing.Size(246, 16)
        Me.LabelCommand.TabIndex = 14
        Me.LabelCommand.Text = "Comandi       + per aggiungere Comando"
        '
        'LabelOrigine
        '
        Me.LabelOrigine.AutoSize = True
        Me.LabelOrigine.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelOrigine.ForeColor = System.Drawing.Color.White
        Me.LabelOrigine.Location = New System.Drawing.Point(48, 41)
        Me.LabelOrigine.Name = "LabelOrigine"
        Me.LabelOrigine.Size = New System.Drawing.Size(24, 16)
        Me.LabelOrigine.TabIndex = 4
        Me.LabelOrigine.Text = "0,0"
        '
        'ButtonOut
        '
        Me.ButtonOut.BackColor = System.Drawing.Color.White
        Me.ButtonOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonOut.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonOut.ForeColor = System.Drawing.Color.Black
        Me.ButtonOut.Location = New System.Drawing.Point(1058, 5)
        Me.ButtonOut.Name = "ButtonOut"
        Me.ButtonOut.Size = New System.Drawing.Size(25, 25)
        Me.ButtonOut.TabIndex = 13
        Me.ButtonOut.Text = "-"
        Me.ButtonOut.UseVisualStyleBackColor = False
        '
        'ButtonZoomIn
        '
        Me.ButtonZoomIn.BackColor = System.Drawing.Color.White
        Me.ButtonZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonZoomIn.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonZoomIn.ForeColor = System.Drawing.Color.Black
        Me.ButtonZoomIn.Location = New System.Drawing.Point(1033, 5)
        Me.ButtonZoomIn.Name = "ButtonZoomIn"
        Me.ButtonZoomIn.Size = New System.Drawing.Size(25, 25)
        Me.ButtonZoomIn.TabIndex = 12
        Me.ButtonZoomIn.Text = "+"
        Me.ButtonZoomIn.UseVisualStyleBackColor = False
        '
        'ButtonOrigin
        '
        Me.ButtonOrigin.BackColor = System.Drawing.Color.White
        Me.ButtonOrigin.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonOrigin.ForeColor = System.Drawing.Color.Black
        Me.ButtonOrigin.Location = New System.Drawing.Point(970, 5)
        Me.ButtonOrigin.Name = "ButtonOrigin"
        Me.ButtonOrigin.Size = New System.Drawing.Size(25, 25)
        Me.ButtonOrigin.TabIndex = 11
        Me.ButtonOrigin.Text = "O"
        Me.ButtonOrigin.UseVisualStyleBackColor = False
        '
        'ButtonRight
        '
        Me.ButtonRight.BackColor = System.Drawing.Color.White
        Me.ButtonRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonRight.ForeColor = System.Drawing.Color.Black
        Me.ButtonRight.Location = New System.Drawing.Point(929, 4)
        Me.ButtonRight.Name = "ButtonRight"
        Me.ButtonRight.Size = New System.Drawing.Size(25, 25)
        Me.ButtonRight.TabIndex = 10
        Me.ButtonRight.Text = ">"
        Me.ButtonRight.UseVisualStyleBackColor = False
        '
        'ButtonDown
        '
        Me.ButtonDown.BackColor = System.Drawing.Color.White
        Me.ButtonDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonDown.ForeColor = System.Drawing.Color.Black
        Me.ButtonDown.Location = New System.Drawing.Point(902, 4)
        Me.ButtonDown.Name = "ButtonDown"
        Me.ButtonDown.Size = New System.Drawing.Size(25, 25)
        Me.ButtonDown.TabIndex = 9
        Me.ButtonDown.Text = "G"
        Me.ButtonDown.UseVisualStyleBackColor = False
        '
        'ButtonUp
        '
        Me.ButtonUp.BackColor = System.Drawing.Color.White
        Me.ButtonUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonUp.ForeColor = System.Drawing.Color.Black
        Me.ButtonUp.Location = New System.Drawing.Point(875, 4)
        Me.ButtonUp.Name = "ButtonUp"
        Me.ButtonUp.Size = New System.Drawing.Size(25, 25)
        Me.ButtonUp.TabIndex = 8
        Me.ButtonUp.Text = "S"
        Me.ButtonUp.UseVisualStyleBackColor = False
        '
        'ButtonLeft
        '
        Me.ButtonLeft.BackColor = System.Drawing.Color.White
        Me.ButtonLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonLeft.ForeColor = System.Drawing.Color.Black
        Me.ButtonLeft.Location = New System.Drawing.Point(848, 4)
        Me.ButtonLeft.Name = "ButtonLeft"
        Me.ButtonLeft.Size = New System.Drawing.Size(25, 25)
        Me.ButtonLeft.TabIndex = 7
        Me.ButtonLeft.Text = "<"
        Me.ButtonLeft.UseVisualStyleBackColor = False
        '
        'ButtonStop
        '
        Me.ButtonStop.BackColor = System.Drawing.Color.Maroon
        Me.ButtonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonStop.ForeColor = System.Drawing.Color.White
        Me.ButtonStop.Location = New System.Drawing.Point(535, 32)
        Me.ButtonStop.Name = "ButtonStop"
        Me.ButtonStop.Size = New System.Drawing.Size(80, 25)
        Me.ButtonStop.TabIndex = 6
        Me.ButtonStop.Text = "Stop"
        Me.ButtonStop.UseVisualStyleBackColor = False
        '
        'LabelCommands
        '
        Me.LabelCommands.AutoSize = True
        Me.LabelCommands.ForeColor = System.Drawing.Color.White
        Me.LabelCommands.Location = New System.Drawing.Point(740, 10)
        Me.LabelCommands.Name = "LabelCommands"
        Me.LabelCommands.Size = New System.Drawing.Size(89, 13)
        Me.LabelCommands.TabIndex = 5
        Me.LabelCommands.Text = "% Comandi Script"
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(634, 12)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(100, 10)
        Me.ProgressBar1.TabIndex = 4
        '
        'TextBoxCommands
        '
        Me.TextBoxCommands.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBoxCommands.Location = New System.Drawing.Point(39, 5)
        Me.TextBoxCommands.Name = "TextBoxCommands"
        Me.TextBoxCommands.Size = New System.Drawing.Size(480, 31)
        Me.TextBoxCommands.TabIndex = 3
        '
        'ButtonCmd
        '
        Me.ButtonCmd.BackColor = System.Drawing.Color.Green
        Me.ButtonCmd.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonCmd.ForeColor = System.Drawing.Color.White
        Me.ButtonCmd.Location = New System.Drawing.Point(535, 4)
        Me.ButtonCmd.Name = "ButtonCmd"
        Me.ButtonCmd.Size = New System.Drawing.Size(80, 25)
        Me.ButtonCmd.TabIndex = 2
        Me.ButtonCmd.Text = "Esegui"
        Me.ButtonCmd.UseVisualStyleBackColor = False
        '
        'ButtonMinimize
        '
        Me.ButtonMinimize.BackColor = System.Drawing.Color.Blue
        Me.ButtonMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonMinimize.ForeColor = System.Drawing.Color.White
        Me.ButtonMinimize.Location = New System.Drawing.Point(1480, 5)
        Me.ButtonMinimize.Name = "ButtonMinimize"
        Me.ButtonMinimize.Size = New System.Drawing.Size(25, 25)
        Me.ButtonMinimize.TabIndex = 1
        Me.ButtonMinimize.Text = "--"
        Me.ButtonMinimize.UseVisualStyleBackColor = False
        '
        'ButtonClose
        '
        Me.ButtonClose.BackColor = System.Drawing.Color.Red
        Me.ButtonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonClose.ForeColor = System.Drawing.Color.White
        Me.ButtonClose.Location = New System.Drawing.Point(1511, 5)
        Me.ButtonClose.Name = "ButtonClose"
        Me.ButtonClose.Size = New System.Drawing.Size(25, 25)
        Me.ButtonClose.TabIndex = 0
        Me.ButtonClose.Text = "X"
        Me.ButtonClose.UseVisualStyleBackColor = False
        '
        'PictureBoxPR
        '
        Me.PictureBoxPR.Image = Global.PaintRobot.My.Resources.Resources.PaintRobot
        Me.PictureBoxPR.Location = New System.Drawing.Point(1259, -10)
        Me.PictureBoxPR.Name = "PictureBoxPR"
        Me.PictureBoxPR.Size = New System.Drawing.Size(670, 86)
        Me.PictureBoxPR.TabIndex = 19
        Me.PictureBoxPR.TabStop = False
        '
        'PanelLeft
        '
        Me.PanelLeft.BackColor = System.Drawing.Color.Gray
        Me.PanelLeft.Controls.Add(Me.ButtonColors)
        Me.PanelLeft.Controls.Add(Me.ButtonRiavvia)
        Me.PanelLeft.Controls.Add(Me.ButtonAttivaListBox)
        Me.PanelLeft.Controls.Add(Me.ButtonScript)
        Me.PanelLeft.Controls.Add(Me.ButtonEditHistory)
        Me.PanelLeft.Controls.Add(Me.ButtonHelp)
        Me.PanelLeft.Dock = System.Windows.Forms.DockStyle.Left
        Me.PanelLeft.Location = New System.Drawing.Point(0, 60)
        Me.PanelLeft.Name = "PanelLeft"
        Me.PanelLeft.Size = New System.Drawing.Size(60, 570)
        Me.PanelLeft.TabIndex = 2
        '
        'ButtonColors
        '
        Me.ButtonColors.BackColor = System.Drawing.Color.Black
        Me.ButtonColors.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonColors.ForeColor = System.Drawing.Color.White
        Me.ButtonColors.Location = New System.Drawing.Point(2, 190)
        Me.ButtonColors.Name = "ButtonColors"
        Me.ButtonColors.Size = New System.Drawing.Size(56, 25)
        Me.ButtonColors.TabIndex = 24
        Me.ButtonColors.Text = "Colori"
        Me.ButtonColors.UseVisualStyleBackColor = False
        '
        'ButtonRiavvia
        '
        Me.ButtonRiavvia.BackColor = System.Drawing.Color.Black
        Me.ButtonRiavvia.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonRiavvia.ForeColor = System.Drawing.Color.White
        Me.ButtonRiavvia.Location = New System.Drawing.Point(2, 155)
        Me.ButtonRiavvia.Name = "ButtonRiavvia"
        Me.ButtonRiavvia.Size = New System.Drawing.Size(56, 25)
        Me.ButtonRiavvia.TabIndex = 23
        Me.ButtonRiavvia.Text = "Reset"
        Me.ButtonRiavvia.UseVisualStyleBackColor = False
        '
        'ButtonAttivaListBox
        '
        Me.ButtonAttivaListBox.BackColor = System.Drawing.Color.DarkOrchid
        Me.ButtonAttivaListBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonAttivaListBox.ForeColor = System.Drawing.Color.White
        Me.ButtonAttivaListBox.Location = New System.Drawing.Point(2, 120)
        Me.ButtonAttivaListBox.Name = "ButtonAttivaListBox"
        Me.ButtonAttivaListBox.Size = New System.Drawing.Size(56, 25)
        Me.ButtonAttivaListBox.TabIndex = 22
        Me.ButtonAttivaListBox.Text = "Lista"
        Me.ButtonAttivaListBox.UseVisualStyleBackColor = False
        '
        'ButtonScript
        '
        Me.ButtonScript.BackColor = System.Drawing.Color.Black
        Me.ButtonScript.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonScript.ForeColor = System.Drawing.Color.White
        Me.ButtonScript.Location = New System.Drawing.Point(2, 85)
        Me.ButtonScript.Name = "ButtonScript"
        Me.ButtonScript.Size = New System.Drawing.Size(56, 25)
        Me.ButtonScript.TabIndex = 21
        Me.ButtonScript.Text = "Script"
        Me.ButtonScript.UseVisualStyleBackColor = False
        '
        'ButtonEditHistory
        '
        Me.ButtonEditHistory.BackColor = System.Drawing.Color.Black
        Me.ButtonEditHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonEditHistory.ForeColor = System.Drawing.Color.White
        Me.ButtonEditHistory.Location = New System.Drawing.Point(2, 50)
        Me.ButtonEditHistory.Name = "ButtonEditHistory"
        Me.ButtonEditHistory.Size = New System.Drawing.Size(56, 25)
        Me.ButtonEditHistory.TabIndex = 20
        Me.ButtonEditHistory.Text = "History"
        Me.ButtonEditHistory.UseVisualStyleBackColor = False
        '
        'ButtonHelp
        '
        Me.ButtonHelp.BackColor = System.Drawing.Color.Black
        Me.ButtonHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ButtonHelp.ForeColor = System.Drawing.Color.White
        Me.ButtonHelp.Location = New System.Drawing.Point(3, 16)
        Me.ButtonHelp.Name = "ButtonHelp"
        Me.ButtonHelp.Size = New System.Drawing.Size(56, 25)
        Me.ButtonHelp.TabIndex = 19
        Me.ButtonHelp.Text = "Help"
        Me.ButtonHelp.UseVisualStyleBackColor = False
        '
        'LabelCoord
        '
        Me.LabelCoord.AutoSize = True
        Me.LabelCoord.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelCoord.Location = New System.Drawing.Point(598, 312)
        Me.LabelCoord.Name = "LabelCoord"
        Me.LabelCoord.Size = New System.Drawing.Size(27, 16)
        Me.LabelCoord.TabIndex = 3
        Me.LabelCoord.Text = "X,Y"
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        Me.PictureBox1.Location = New System.Drawing.Point(60, 60)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(1214, 570)
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'LstComandi
        '
        Me.LstComandi.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LstComandi.FormattingEnabled = True
        Me.LstComandi.ItemHeight = 24
        Me.LstComandi.Location = New System.Drawing.Point(66, 507)
        Me.LstComandi.Name = "LstComandi"
        Me.LstComandi.Size = New System.Drawing.Size(549, 28)
        Me.LstComandi.TabIndex = 4
        '
        'ListView1
        '
        Me.ListView1.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.ListView1.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ListView1.HideSelection = False
        Me.ListView1.Location = New System.Drawing.Point(100, 100)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(462, 382)
        Me.ListView1.TabIndex = 25
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1548, 630)
        Me.Controls.Add(Me.ListView1)
        Me.Controls.Add(Me.LstComandi)
        Me.Controls.Add(Me.LabelCoord)
        Me.Controls.Add(Me.PanelLeft)
        Me.Controls.Add(Me.PanelTop)
        Me.Controls.Add(Me.PictureBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form1"
        Me.Text = "PaintRobot"
        Me.PanelTop.ResumeLayout(False)
        Me.PanelTop.PerformLayout()
        CType(Me.PictureBoxPR, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelLeft.ResumeLayout(False)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents PanelTop As Panel
    Friend WithEvents PanelLeft As Panel
    Friend WithEvents ButtonMinimize As Button
    Friend WithEvents ButtonClose As Button
    Friend WithEvents TextBoxCommands As TextBox
    Friend WithEvents ButtonCmd As Button
    Friend WithEvents LabelCommands As Label
    Friend WithEvents ProgressBar1 As ProgressBar
    Friend WithEvents ButtonStop As Button
    Friend WithEvents ButtonRight As Button
    Friend WithEvents ButtonDown As Button
    Friend WithEvents ButtonUp As Button
    Friend WithEvents ButtonLeft As Button
    Friend WithEvents ButtonOrigin As Button
    Friend WithEvents LabelCoord As Label
    Friend WithEvents ButtonOut As Button
    Friend WithEvents ButtonZoomIn As Button
    Friend WithEvents LabelOrigine As Label
    Friend WithEvents LabelCommand As Label
    Friend WithEvents LabelEseguiti As Label
    Friend WithEvents LabelNumCmds As Label
    Friend WithEvents ButtonSavePaintRobot As Button
    Friend WithEvents ButtonTest As Button
    Friend WithEvents ButtonHelp As Button
    Friend WithEvents PictureBoxPR As PictureBox
    Friend WithEvents ButtonContinueRendering As Button
    Friend WithEvents ButtonHistory As Button
    Friend WithEvents ButtonRenderRemain As Button
    Friend WithEvents ButtonCentroMondo As Button
    Friend WithEvents ButtonEditHistory As Button
    Friend WithEvents LstComandi As ListBox
    Friend WithEvents ButtonScript As Button
    Friend WithEvents ButtonAttivaListBox As Button
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents ButtonRiavvia As Button
    Friend WithEvents ButtonColors As Button
    Friend WithEvents ListView1 As ListView
End Class
