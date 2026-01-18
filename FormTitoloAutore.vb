Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class FormTitoloAutore
    ' Proprietà per leggere i valori inseriti dall'utente
    Public Property Titolo As String
    Public Property Autore As String

    Private Sub ButtonSalva_Click(sender As Object, e As EventArgs) Handles ButtonSalva.Click
        ' Controllo che i campi non siano vuoti
        If String.IsNullOrWhiteSpace(TextBox1.Text) Then
            MessageBox.Show("Inserisci un titolo valido.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If String.IsNullOrWhiteSpace(TextBox2.Text) Then
            MessageBox.Show("Inserisci un autore valido.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Salvo i valori nelle proprietà
        Titolo = TextBox1.Text.Trim()
        Autore = TextBox2.Text.Trim()

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub ButtonAnnulla_Click(sender As Object, e As EventArgs) Handles ButtonAnnulla.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub FormTitoloAutore_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Location = New Point(200, 200)
    End Sub
End Class
