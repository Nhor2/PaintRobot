<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class HistoryEditorForm
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
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.LabelClose = New System.Windows.Forms.Label()
        Me.LabelTitle = New System.Windows.Forms.Label()
        Me.LabelCopia = New System.Windows.Forms.Label()
        Me.PictureBoxHistory = New System.Windows.Forms.PictureBox()
        Me.LabelSave = New System.Windows.Forms.Label()
        CType(Me.PictureBoxHistory, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ListBox1
        '
        Me.ListBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.ItemHeight = 24
        Me.ListBox1.Location = New System.Drawing.Point(87, 50)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(672, 292)
        Me.ListBox1.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Black
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(12, 57)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(53, 47)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "SU"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label2
        '
        Me.Label2.BackColor = System.Drawing.Color.Black
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.White
        Me.Label2.Location = New System.Drawing.Point(12, 114)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(53, 47)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "GIU"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label3
        '
        Me.Label3.BackColor = System.Drawing.Color.Black
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.Red
        Me.Label3.Location = New System.Drawing.Point(12, 172)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(53, 47)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "X"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LabelClose
        '
        Me.LabelClose.AutoSize = True
        Me.LabelClose.BackColor = System.Drawing.Color.Red
        Me.LabelClose.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelClose.Location = New System.Drawing.Point(765, 9)
        Me.LabelClose.Name = "LabelClose"
        Me.LabelClose.Size = New System.Drawing.Size(21, 20)
        Me.LabelClose.TabIndex = 4
        Me.LabelClose.Text = "X"
        '
        'LabelTitle
        '
        Me.LabelTitle.AutoSize = True
        Me.LabelTitle.Font = New System.Drawing.Font("Consolas", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelTitle.ForeColor = System.Drawing.Color.White
        Me.LabelTitle.Location = New System.Drawing.Point(12, 9)
        Me.LabelTitle.Name = "LabelTitle"
        Me.LabelTitle.Size = New System.Drawing.Size(418, 24)
        Me.LabelTitle.TabIndex = 7
        Me.LabelTitle.Text = "Storico Comandi - History Commands"
        '
        'LabelCopia
        '
        Me.LabelCopia.BackColor = System.Drawing.Color.White
        Me.LabelCopia.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelCopia.ForeColor = System.Drawing.Color.White
        Me.LabelCopia.Image = Global.PaintRobot.My.Resources.Resources.CopiaRTF
        Me.LabelCopia.Location = New System.Drawing.Point(12, 283)
        Me.LabelCopia.Name = "LabelCopia"
        Me.LabelCopia.Size = New System.Drawing.Size(53, 47)
        Me.LabelCopia.TabIndex = 8
        Me.LabelCopia.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'PictureBoxHistory
        '
        Me.PictureBoxHistory.BackColor = System.Drawing.Color.White
        Me.PictureBoxHistory.Location = New System.Drawing.Point(87, 363)
        Me.PictureBoxHistory.Name = "PictureBoxHistory"
        Me.PictureBoxHistory.Size = New System.Drawing.Size(672, 286)
        Me.PictureBoxHistory.TabIndex = 6
        Me.PictureBoxHistory.TabStop = False
        '
        'LabelSave
        '
        Me.LabelSave.BackColor = System.Drawing.Color.White
        Me.LabelSave.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelSave.ForeColor = System.Drawing.Color.White
        Me.LabelSave.Image = Global.PaintRobot.My.Resources.Resources.FloppyS
        Me.LabelSave.Location = New System.Drawing.Point(12, 423)
        Me.LabelSave.Name = "LabelSave"
        Me.LabelSave.Size = New System.Drawing.Size(53, 47)
        Me.LabelSave.TabIndex = 5
        Me.LabelSave.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'HistoryEditorForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(798, 661)
        Me.Controls.Add(Me.LabelCopia)
        Me.Controls.Add(Me.LabelTitle)
        Me.Controls.Add(Me.PictureBoxHistory)
        Me.Controls.Add(Me.LabelSave)
        Me.Controls.Add(Me.LabelClose)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ListBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "HistoryEditorForm"
        Me.Text = "HistoryEditorForm"
        CType(Me.PictureBoxHistory, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents LabelClose As Label
    Friend WithEvents LabelSave As Label
    Friend WithEvents PictureBoxHistory As PictureBox
    Friend WithEvents LabelTitle As Label
    Friend WithEvents LabelCopia As Label
End Class
