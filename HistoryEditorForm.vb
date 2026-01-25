Public Class HistoryEditorForm

    Public Property EditedHistory As List(Of RobotCommand)
    Public Property EditedHistoryString As List(Of String)


    Private PaintRobotHistory As Bitmap
    Private Const HistoryPreviewWidth As Integer = 300
    Private Const HistoryPreviewHeight As Integer = 300

    Private Drawer As New RobotDrawer()
    Private Interpreter As New RobotInterpreter()
    Private ctxHistory As RobotContext
    Private renderGraphicsHistory As Graphics
    Private ViewHistory As New Viewport()

    Private Sub HistoryEditorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Location = New Point(200, 200)
        LabelTitle.Text = "Storico Comandi - History Commands"

        ListBox1.Font = New Font("Consolas", 12)
        ListBox1.SelectionMode = SelectionMode.One
        ListBox1.DrawMode = DrawMode.OwnerDrawFixed
        ListBox1.ItemHeight = TextRenderer.MeasureText("X", ListBox1.Font).Height + 2

        InitRenderHisotry()

        RefreshList()
    End Sub

    Public Sub New(history As List(Of RobotCommand), historyString As List(Of String))

        InitializeComponent()

        EditedHistory = history.ToList()
        EditedHistoryString = historyString.ToList()
        Me.Location = New Point(200, 200)
        LabelTitle.Text = "Storico Comandi - History Commands"


        ListBox1.Font = New Font("Consolas", 12)
        ListBox1.SelectionMode = SelectionMode.One
        ListBox1.DrawMode = DrawMode.OwnerDrawFixed
        ListBox1.ItemHeight = TextRenderer.MeasureText("X", ListBox1.Font).Height + 2

        InitRenderHisotry()

        RefreshList()
        AutoSizeListBoxToContent(ListBox1)
    End Sub

    Private Sub InitRenderHisotry()
        ' Crea la bitmap vuota
        PaintRobotHistory = New Bitmap(HistoryPreviewWidth, HistoryPreviewHeight)

        ' Prepariamo bitmap e Graphics UNA SOLA VOLTA
        renderGraphicsHistory = Graphics.FromImage(PaintRobotHistory)
        renderGraphicsHistory.Clear(Color.White)
        renderGraphicsHistory.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        renderGraphicsHistory.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        renderGraphicsHistory.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality

        ctxHistory = New RobotContext With {
        .Graphics = renderGraphicsHistory,
        .Bitmap = PaintRobotHistory,
        .LivelloCorrente = "BackGround",
        .View = ViewHistory
    }
    End Sub

    Private Sub RefreshList()
        ListBox1.Items.Clear()

        For i = 0 To EditedHistoryString.Count - 1
            ' Mostriamo i numeri da 1 invece che da 0
            ListBox1.Items.Add($"{i + 1:00000}  {EditedHistoryString(i)}")
        Next

        ' Aggiorna le dimensioni ListBox1
        AutoSizeListBoxToContent(ListBox1)
    End Sub

    Private Sub AutoSizeListBoxToContent(lb As ListBox)
        ' Aggiorna le dimensioni ListBox1
        Dim maxWidth As Integer = 0

        Using g As Graphics = lb.CreateGraphics()
            For Each item As String In lb.Items
                Dim size = TextRenderer.MeasureText(item, lb.Font)
                maxWidth = Math.Max(maxWidth, size.Width)
            Next
        End Using

        ' margine testo + scrollbar
        maxWidth += SystemInformation.VerticalScrollBarWidth + 10

        lb.Width = maxWidth

        ResizeFormToFitListBox()
    End Sub

    Private Sub ResizeFormToFitListBox()
        ' Aggiorna le dimensioni Finestra
        Dim rightMargin As Integer = 40   ' spazio per la X
        Dim leftMargin As Integer = 10

        Me.ClientSize = New Size(ListBox1.Width + leftMargin + rightMargin + 50, Me.ClientSize.Height)

        ' Posiziona la X manualmente UNA volta
        LabelClose.Location = New Point(Me.ClientSize.Width - LabelClose.Width - 10, 6)

        ' Ora l'Anchor funziona
        LabelClose.Anchor = AnchorStyles.Top Or AnchorStyles.Right
    End Sub


    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click
        'Su
        Dim i = ListBox1.SelectedIndex
        If i <= 0 Then Exit Sub

        Swap(i, i - 1)
        ListBox1.SelectedIndex = i - 1
    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click
        'Giu
        Dim i = ListBox1.SelectedIndex
        If i < 0 OrElse i >= EditedHistory.Count - 1 Then Exit Sub

        Swap(i, i + 1)
        ListBox1.SelectedIndex = i + 1
    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click
        'Cancella
        Dim i = ListBox1.SelectedIndex
        If i < 0 Then Exit Sub

        EditedHistory.RemoveAt(i)
        EditedHistoryString.RemoveAt(i)

        RefreshList()
        If i < ListBox1.Items.Count Then
            ListBox1.SelectedIndex = i
        End If
    End Sub

    Private Sub Swap(i As Integer, j As Integer)
        ' RobotCommand
        Dim cmd = EditedHistory(i)
        EditedHistory(i) = EditedHistory(j)
        EditedHistory(j) = cmd

        ' Stringa
        Dim s = EditedHistoryString(i)
        EditedHistoryString(i) = EditedHistoryString(j)
        EditedHistoryString(j) = s

        RefreshList()
    End Sub

    Private Sub LabelCopia_Click(sender As Object, e As EventArgs) Handles LabelCopia.Click
        'Copia gli elementi in HistoryString
        Dim copyString As String = ""
        For i = 0 To EditedHistoryString.Count - 1
            ' Non numerata
            copyString &= ($"{EditedHistoryString(i)}") & vbCrLf
        Next

        If copyString IsNot Nothing Then
            Clipboard.SetText(copyString)
            MsgBox("History Copiata in Appunti.", MsgBoxStyle.Information, "\\(*_*//  PaintRobot")
        End If
    End Sub

    Private Sub LabelClose_Click(sender As Object, e As EventArgs) Handles LabelClose.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub LabelSave_Click(sender As Object, e As EventArgs) Handles LabelSave.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        Dim i = ListBox1.SelectedIndex
        If i < 0 Then Return

        DrawHistoryPreview(i)
    End Sub

    Private Sub ListBox1_DrawItem(sender As Object, e As DrawItemEventArgs) Handles ListBox1.DrawItem
        ' colore verde per riga selezionata
        If e.Index < 0 Then Return

        Dim lb = CType(sender, ListBox)
        Dim text As String = lb.Items(e.Index).ToString()

        Dim isSelected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected

        ' Colori
        Dim backColor As Color = If(isSelected, Color.Green, lb.BackColor)
        Dim foreColor As Color = If(isSelected, Color.White, lb.ForeColor)

        Using backBrush As New SolidBrush(backColor),
              foreBrush As New SolidBrush(foreColor)

            e.Graphics.FillRectangle(backBrush, e.Bounds)
            e.Graphics.DrawString(text, e.Font, foreBrush, e.Bounds.X, e.Bounds.Y)
        End Using

        e.DrawFocusRectangle()
    End Sub

    Private Sub DrawHistoryPreview(upToIndex As Integer)
        If PaintRobotHistory Is Nothing OrElse renderGraphicsHistory Is Nothing Then Return

        ' Pulisci la bitmap esistente
        renderGraphicsHistory.Clear(Color.White)

        ' Trasformazione per scalare tutto nella miniatura
        Dim scaleX As Single = HistoryPreviewWidth / Form1.Width ' o LarghezzaPaginaPixel 10K
        Dim scaleY As Single = HistoryPreviewHeight / Form1.Height ' o AltezzaPaginaPixel 10k
        renderGraphicsHistory.ResetTransform()
        renderGraphicsHistory.ScaleTransform(scaleX, scaleY)

        ' Disegna i comandi fino a quello selezionato
        For j = 0 To upToIndex
            Dim cmd = EditedHistory(j)
            Drawer.Esegui(cmd, ctxHistory)  ' metodo da creare per miniatura
        Next
        PictureBoxHistory.Image = PaintRobotHistory
        PictureBoxHistory.Invalidate()
    End Sub
End Class