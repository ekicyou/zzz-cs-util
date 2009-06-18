Public Class Form1

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.ComboBox1.Items.Add("System.Random")
        Me.ComboBox1.Items.Add("SFMT607")
        Me.ComboBox1.Items.Add("SFMT1279")
        Me.ComboBox1.Items.Add("SFMT2281")
        Me.ComboBox1.Items.Add("SFMT4253")
        Me.ComboBox1.Items.Add("SFMT11213")
        Me.ComboBox1.Items.Add("SFMT19937")
        Me.ComboBox1.Items.Add("SFMT44497")
        Me.ComboBox1.Items.Add("SFMT86243")
        Me.ComboBox1.Items.Add("SFMT132049")
        Me.ComboBox1.Items.Add("SFMT216091")
        Me.ComboBox1.Items.Add("MotherOfAll")
        Me.ComboBox1.Items.Add("LCG")
        Me.ComboBox1.Items.Add("MersenneTwister")
        Me.ComboBox1.Items.Add("Xorshift")
        Me.ComboBox1.Items.Add("Well")
        Me.ComboBox1.Items.Add("Ranrot-B")
        Me.ComboBox1.SelectedIndex = 6
    End Sub

    Public ReadOnly Property Random() As Object
        Get
            Select Case Me.ComboBox1.SelectedIndex
                Case 0
                    Return New System.Random(1234)
                Case 1
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT607)
                Case 2
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT1279)
                Case 3
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT2281)
                Case 4
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT4253)
                Case 5
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT11213)
                Case 6
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT19937)
                Case 7
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT44497)
                Case 8
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT86243)
                Case 9
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT132049)
                Case 10
                    Return New Rei.Random.SFMT(1234, Rei.Random.MTPeriodType.MT216091)
                Case 11
                    Return New Rei.Random.MotherOfAll(1234)
                Case 12
                    Return New Rei.Random.LCG(1234)
                Case 13
                    Return New Rei.Random.MersenneTwister(1234)
                Case 14
                    Return New Rei.Random.Xorshift(1234)
                Case 15
                    Return New Rei.Random.Well(1234)
                Case 16
                    Return New Rei.Random.RanrotB(1234)
            End Select
            Throw New ApplicationException
        End Get
    End Property

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim rand As Object = Me.Random
        Dim rand1 As Rei.Random.RandomBase = TryCast(rand, Rei.Random.RandomBase)
        Dim rand2 As System.Random = TryCast(rand, System.Random)
        Dim s As String = rand.ToString() & ControlChars.CrLf
        If rand1 IsNot Nothing Then
            For i As Integer = 0 To 1000 \ 5 - 1
                s &= String.Format("{0,10} {1,10} {2,10} {3,10} {4,10} ", rand1.NextUInt32(), rand1.NextUInt32(), rand1.NextUInt32(), rand1.NextUInt32(), rand1.NextUInt32()) & ControlChars.CrLf
            Next
        ElseIf rand2 IsNot Nothing Then
            For i As Integer = 0 To 1000 \ 5 - 1
                s &= String.Format("{0,10} {1,10} {2,10} {3,10} {4,10} ", rand2.Next(), rand2.Next(), rand2.Next(), rand2.Next(), rand2.Next()) & ControlChars.CrLf
            Next
        End If

        Me.TextBox.Text = s
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim rand As Object = Me.Random
        Dim rand1 As Rei.Random.RandomBase = TryCast(rand, Rei.Random.RandomBase)
        Dim rand2 As System.Random = TryCast(rand, System.Random)
        Dim MaxCount As Integer = 100000000
        Dim i As Integer
        Dim starttime As Integer
        Dim endtime As Integer
        Dim buf(2) As Byte

        If rand1 IsNot Nothing Then
            starttime = Environment.TickCount
            For i = 1 To MaxCount
                rand1.NextUInt32()
            Next
            endtime = Environment.TickCount
        ElseIf rand2 IsNot Nothing Then
            starttime = Environment.TickCount
            For i = 1 To MaxCount
                rand2.Next()
            Next
            endtime = Environment.TickCount
        End If

        Me.TextBox.Text = rand.ToString & ": " & (endtime - starttime).ToString() & "msec (" & MaxCount.ToString & "th generation)"
    End Sub

End Class
