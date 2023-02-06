Public Class MainWindow

    Dim ReceiptImage As Bitmap

    ' the form loads and initialization should happen
    Private Sub MainWindow_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'TODO: This line of code loads data into the 'MyDataset.ReciptDetails' table. You can move, or remove it, as needed.
        Me.ReciptDetailsTableAdapter.Fill(Me.MyDataset.ReciptDetails)
        'TODO: This line of code loads data into the 'POSDS.ItemsTotal' table. You can move, or remove it, as needed.
        Me.ItemsTotalTableAdapter.Fill(Me.POSDS.ItemsTotal)


        Try


            ' get the password from the user
            Dim PSWWin As New PasswordPicker

            ' if the user hits the exit button then stop execution
            If PSWWin.ShowDialog <> Windows.Forms.DialogResult.OK Then
                End
            End If

            ' get the password
            Dim PSW As String = PSWWin.TextBox1.Text

            ' get the password from the database
            Dim TA As New POSDSTableAdapters.ValuesTableAdapter
            Dim TB = TA.GetDataByKey("password")
            Dim DBPSW As String = TB.Rows(0).Item(1)

            ' check that passwords match
            If PSW <> DBPSW Then
                MsgBox("invalid password", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
                End
            End If

            ' load the items information from db into the dataset
            ItemsTA.Fill(MyDataset.Items)


            ' the printer name should appear
            Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
            Dim Result = VTA.GetDataByKey("printer")
            TextBox5.Text = Result.Rows(0).Item(1) & ""

            Result = VTA.GetDataByKey("unit width")
            TextBox6.Text = Result.Rows(0).Item(1) & ""

            Result = VTA.GetDataByKey("unit height")
            TextBox7.Text = Result.Rows(0).Item(1) & ""

            Result = VTA.GetDataByKey("font size")
            TextBox8.Text = Result.Rows(0).Item(1) & ""

            ' fill the settings page with test data
            DGV3.Rows.Add("", "tea", "", 20, 30, 20 * 30)
            DGV3.Rows.Add("", "pen", "", 1, 12, 1 * 12)
            DGV3.Rows.Add("", "cup", "", 5, 7, 7 * 5)

            PB.Image = DrawReceipt(DGV3.Rows, 838, "2012/01/01 10:45:01", 123, 200, 77, TextBox6.Text, TextBox7.Text, TextBox8.Text)

            


        Catch ex As Exception

            ' handle the error
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            End
        End Try

        Me.RV.RefreshReport()
        Me.RV.RefreshReport()
    End Sub

    ' change the password
    Private Sub ChangePasswordToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChangePasswordToolStripMenuItem.Click
        Dim PSWChange As New ChangePassword
        PSWChange.ShowDialog()
    End Sub

    ' add item to the db
    Private Sub AddItemToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddItemToolStripMenuItem.Click
        Dim AddItemWindow As New AddItem
        If AddItemWindow.ShowDialog = Windows.Forms.DialogResult.OK Then
            ' load the information of items from db
            ItemsTA.Fill(MyDataset.Items)
        End If
    End Sub

    ' used to select an item
    Private Sub EditItemToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EditItemToolStripMenuItem.Click

        ' make sure an item is selected
        If DGV.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        ' get the barcode of the item
        Dim Barcode = DGV.SelectedRows(0).Cells(0).Value

        ' create the edit window
        Dim EditItemWindow As New EditItem

        ' fill the window with information
        EditItemWindow.FillItemInfo(Barcode)

        If EditItemWindow.ShowDialog = Windows.Forms.DialogResult.OK Then
            ' load the information of items from db
            ItemsTA.Fill(MyDataset.Items)
        End If
    End Sub

    ' this one is used to remove an item
    Private Sub RemoveItemToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RemoveItemToolStripMenuItem.Click

        ' make sure a single item is being selected
        If DGV.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        ' get the barcode of the item
        Dim Barcode As String = DGV.SelectedRows(0).Cells(0).Value

        ' remove the item
        Try
            ReciptDetailsTableAdapter.DeleteByBarcode2(Barcode)
            ReciptDetailsTableAdapter.Fill(MyDataset.ReciptDetails)
            ItemsTA.DeleteByBarcode(Barcode)
            ItemsTA.Fill(MyDataset.Items)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub


    ' checks if the return key is pressed
    Private Sub TextBox1_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox1.KeyPress
        If Button1.Enabled Then
            If e.KeyChar = Chr(13) Then
                Button1_Click(Nothing, Nothing)
                e.Handled = True
            End If
        End If
    End Sub


    ' this one is used to detect the barcode item when the text change and display its information
    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        Try
            ' step 01: create the table adapter
            Dim TA As New POSDSTableAdapters.ItemsTableAdapter
            Dim TB = TA.GetDataByBarcode(TextBox1.Text)

            ' step 02: check if no item is found
            If TB.Rows.Count = 0 Then
                TextBox2.Text = ""
                TextBox3.Text = ""
                Button1.Enabled = False
                Exit Sub
            End If

            ' step 03: display the information in the textboxes
            Button1.Enabled = True
            Dim R As POS.POSDS.ItemsRow = TB.Rows(0)
            TextBox2.Text = R.ItemName
            TextBox3.Text = R.SellPrice
            Button1.Tag = R
        Catch ex As Exception
            ' display error message
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub


    ' this will be used to add an item to the recipt details
    Friend Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        ' get the details of the item
        Dim R As POS.POSDS.ItemsRow = Button1.Tag

        ' next search for the barcode in the datagridview
        Dim I As Integer
        Dim ItemLoc As Integer = -1
        For I = 0 To DGV2.Rows.Count - 1
            If R.Barcode = DGV2.Rows(I).Cells(0).Value Then

                ' item found
                ItemLoc = I
                Exit For

            End If
        Next

        ' if item is not found, add it
        If ItemLoc = -1 Then
            DGV2.Rows.Add(R.Barcode, R.ItemName, R.BuyPrice, R.SellPrice, 1, R.SellPrice)
        Else
            ' if item is already there increase its Quantity
            Dim ItemCount As Long = DGV2.Rows(ItemLoc).Cells(4).Value
            ItemCount += 1
            Dim NewPrice As Decimal = R.SellPrice * ItemCount
            DGV2.Rows(ItemLoc).Cells(4).Value = ItemCount
            DGV2.Rows(ItemLoc).Cells(5).Value = NewPrice
        End If

        ' next clear textbox1 and set focus to it
        TextBox1.Text = ""
        TextBox1.Focus()

        ' compute the total for the recipt
        Dim Sum As Decimal = 0
        For I = 0 To DGV2.Rows.Count - 1
            Sum += DGV2.Rows(I).Cells(5).Value
        Next

        Label11.Text = (Sum)


    End Sub


    ' remove item from the recipt
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If DGV2.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        DGV2.Rows.Remove(DGV2.SelectedRows(0))
    End Sub


    ' save the recipt
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        Dim MyConnection As OleDb.OleDbConnection = Nothing
        Dim MyTransaction As OleDb.OleDbTransaction = Nothing

        Try

            ' create the connection and  transaction object
            MyConnection = New OleDb.OleDbConnection(My.Settings.dbConnectionString)
            MyConnection.Open()
            MyTransaction = MyConnection.BeginTransaction

            ' insert the new recipt
            Dim SQL As String = "insert into recipts (reciptdate,recipttotal) values (:0,:1)"
            Dim CMD1 As New OleDb.OleDbCommand
            CMD1.Connection = MyConnection
            CMD1.Transaction = MyTransaction
            CMD1.CommandText = SQL
            CMD1.Parameters.AddWithValue(":0", Now.Date)
            CMD1.Parameters.AddWithValue(":1", Label11.Text)
            CMD1.ExecuteNonQuery()
            CMD1.Dispose()

            ' get the id for the recipt
            SQL = "select max(reciptid) as MAXID from recipts"
            Dim CMD2 As New OleDb.OleDbCommand
            CMD2.Connection = MyConnection
            CMD2.Transaction = MyTransaction
            CMD2.CommandText = SQL
            Dim ReciptID As Long = CMD2.ExecuteScalar()
            CMD2.Dispose()

            ' insert the details of the recipt
            Dim I As Integer
            For I = 0 To DGV2.Rows.Count - 1

                ' get the values
                Dim Barcode As String = DGV2.Rows(I).Cells(0).Value
                Dim BuyPrice As Decimal = DGV2.Rows(I).Cells(2).Value
                Dim SellPrice As Decimal = DGV2.Rows(I).Cells(3).Value
                Dim ItemCount As Integer = DGV2.Rows(I).Cells(4).Value

                ' next create a command
                Dim CMD3 As New OleDb.OleDbCommand
                SQL = "insert into ReciptDetails " & _
                      "(reciptid,barcode,itemcount,itembuyprice,itemsellprice) " & _
                      "values " & _
                      "(:0      ,:1     ,:2       ,:3          ,:4       )"
                CMD3.Connection = MyConnection
                CMD3.Transaction = MyTransaction
                CMD3.CommandText = SQL
                CMD3.Parameters.AddWithValue(":0", ReciptID)
                CMD3.Parameters.AddWithValue(":1", Barcode)
                CMD3.Parameters.AddWithValue(":2", ItemCount)
                CMD3.Parameters.AddWithValue(":3", BuyPrice)
                CMD3.Parameters.AddWithValue(":4", SellPrice)

                CMD3.ExecuteNonQuery()
                CMD3.Dispose()

            Next


            ' all well save the changes
            MyTransaction.Commit()

            ' close connection
            MyTransaction.Dispose()
            MyConnection.Close()
            MyConnection.Dispose()



            ' use the printer
            If TextBox5.Text <> "" Then

                If PB.Image Is Nothing Then
                    MsgBox("The settings for the receipt size is wrong, the receipt will no be printed", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
                Else
                    ' draw the receipt
                    ReceiptImage = DrawReceipt(DGV2.Rows, ReciptID, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), Label11.Text, TextBox4.Text, Label14.Text, TextBox6.Text, TextBox7.Text, TextBox8.Text)

                    ' print the receipt
                    PrintDoc.PrinterSettings.PrinterName = TextBox5.Text
                    PrintDoc.Print()
                End If

            Else
                MsgBox("You did not set a printer to print your receipt", MsgBoxStyle.OkOnly, "Warning")
            End If


            DGV2.Rows.Clear()
            Label11.Text = "0.00"
            TextBox4.Clear()
            Label14.Text = "0.00"


        Catch ex As Exception
            If MyTransaction IsNot Nothing Then
                MyTransaction.Rollback()
            End If
            If MyConnection IsNot Nothing Then
                If MyConnection.State = ConnectionState.Open Then
                    MyConnection.Close()
                End If
            End If

            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try

    End Sub


    ' show the correct report
    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click

        ' remove previous report
        Me.Controls.Remove(RV)
        RV.Dispose()

        RV = New Microsoft.Reporting.WinForms.ReportViewer
        RV.Parent = GroupBox2
        RV.Dock = DockStyle.Fill
        RV.Visible = True


        ' check the first report
        If ComboBox1.Text = "Total profit for all time" Then

            ' fill the information for that report
            Dim TA As New POSDSTableAdapters.TotalProfitForAllTimeTableAdapter
            Dim TmpDS As New POSDS
            TA.Fill(TmpDS.TotalProfitForAllTime)

            ' clear previous DS
            RV.LocalReport.DataSources.Clear()

            ' create new DS
            Dim RDS As New Microsoft.Reporting.WinForms.ReportDataSource("POSDS_TotalProfitForAllTime", CType(TmpDS.TotalProfitForAllTime, DataTable))

            ' tell the report control to use the DS, and 
            ' use the report template created by us.
            RV.LocalReport.DataSources.Add(RDS)
            RV.LocalReport.ReportEmbeddedResource = "POS.TotalProfitForAllTime.rdlc"
            RV.RefreshReport()

        ElseIf ComboBox1.Text = "Total profit between two dates" Then

            ' fill the information for that report
            Dim TA As New POSDSTableAdapters.TotalProfitForAllTimeTableAdapter
            Dim TmpDS As New POSDS
            TA.FillByFilteringBetweenTwoDates(TmpDS.TotalProfitForAllTime, DateTimePicker1.Value, DateTimePicker2.Value)

            ' clear previous DS
            RV.LocalReport.DataSources.Clear()

            ' create new DS
            Dim RDS As New Microsoft.Reporting.WinForms.ReportDataSource("POSDS_TotalProfitForAllTime", CType(TmpDS.TotalProfitForAllTime, DataTable))

            ' tell the report control to use the DS, and 
            ' use the report template created by us.
            RV.LocalReport.DataSources.Add(RDS)
            RV.LocalReport.ReportEmbeddedResource = "POS.TotalProfitBetweenTwoDates.rdlc"
            RV.RefreshReport()

        ElseIf ComboBox1.Text = "Total profit and Quantity for items" Then

            ' fill the information for that report
            Dim TA As New POSDSTableAdapters.ItemsTotalTableAdapter
            Dim TmpDS As New POSDS
            TA.Fill(TmpDS.ItemsTotal)

            ' clear previous DS
            RV.LocalReport.DataSources.Clear()

            ' create new DS
            Dim RDS As New Microsoft.Reporting.WinForms.ReportDataSource("POSDS_ItemsTotal", CType(TmpDS.ItemsTotal, DataTable))

            ' tell the report control to use the DS, and 
            ' use the report template created by us.
            RV.LocalReport.DataSources.Add(RDS)
            RV.LocalReport.ReportEmbeddedResource = "POS.ItemsTotal.rdlc"
            RV.RefreshReport()


        ElseIf ComboBox1.Text = "Total profit and Quantity for items between two dates" Then

            ' fill the information for that report
            Dim TA As New POSDSTableAdapters.ItemsTotalTableAdapter
            Dim TmpDS As New POSDS
            TA.FillByFilteringBetweenTwoDates(TmpDS.ItemsTotal, DateTimePicker1.Value, DateTimePicker2.Value)

            ' clear previous DS
            RV.LocalReport.DataSources.Clear()

            ' create new DS
            Dim RDS As New Microsoft.Reporting.WinForms.ReportDataSource("POSDS_ItemsTotal", CType(TmpDS.ItemsTotal, DataTable))

            ' tell the report control to use the DS, and 
            ' use the report template created by us.
            RV.LocalReport.DataSources.Add(RDS)
            RV.LocalReport.ReportEmbeddedResource = "POS.ItemsTotal.rdlc"
            RV.RefreshReport()

        End If


    End Sub




    ' function Draw Receipt
    Public Function DrawReceipt(ByVal Rows As DataGridViewRowCollection, ByVal ReceiptNo As String, ByVal ReceiptDate As String, ByVal ReceiptTotal As Decimal, ByVal AmountGiven As Decimal, ByVal Change As Decimal, ByVal UnitWidth As Integer, ByVal UnitHeight As Integer, ByVal FontSize As Integer) As Bitmap



        Dim ReceiptWidth As Integer = 13 * UnitWidth
        Dim ReceiptDetailsHeight As Integer = Rows.Count * UnitHeight
        Dim ReceiptHeight As Integer = 6 * UnitWidth + ReceiptDetailsHeight

        ' create the bitmap
        Dim BMP As New Bitmap(ReceiptWidth + 1, ReceiptHeight, Imaging.PixelFormat.Format24bppRgb)

        ' create the graphics object
        Dim GR As Graphics = Graphics.FromImage(BMP)

        ' fill the image with color white
        GR.Clear(Color.White)


        ' draw the basic lines

        ' draw the headers
        Dim LNHeaderYStart = 3 * UnitHeight
        Dim LNDetailsStart = LNHeaderYStart + UnitHeight

        GR.DrawRectangle(Pens.White, UnitWidth * 0, LNHeaderYStart, UnitWidth, UnitHeight)
        GR.DrawRectangle(Pens.White, UnitWidth * 1, LNHeaderYStart, UnitWidth * 5, UnitHeight)
        GR.DrawRectangle(Pens.White, UnitWidth * 6, LNHeaderYStart, UnitWidth * 2, UnitHeight)
        GR.DrawRectangle(Pens.White, UnitWidth * 9, LNHeaderYStart, UnitWidth * 2, UnitHeight)
        GR.DrawRectangle(Pens.White, UnitWidth * 11, LNHeaderYStart, UnitWidth * 3, UnitHeight)


        ' draw the details part
        GR.DrawRectangle(Pens.White, UnitWidth * 0, LNDetailsStart, UnitWidth * 1, ReceiptDetailsHeight)
        GR.DrawRectangle(Pens.White, UnitWidth * 1, LNDetailsStart, UnitWidth * 5, ReceiptDetailsHeight)
        GR.DrawRectangle(Pens.White, UnitWidth * 6, LNDetailsStart, UnitWidth * 2, ReceiptDetailsHeight)
        GR.DrawRectangle(Pens.White, UnitWidth * 9, LNDetailsStart, UnitWidth * 2, ReceiptDetailsHeight)
        GR.DrawRectangle(Pens.White, UnitWidth * 11, LNDetailsStart, UnitWidth * 3, ReceiptDetailsHeight)

        ' fill the header with some text
        Dim FNT As New Font("Times", FontSize, FontStyle.Bold)

        GR.DrawString("No", FNT, Brushes.Black, UnitWidth * 0, LNHeaderYStart)
        GR.DrawString("Item", FNT, Brushes.Black, UnitWidth * 1, LNHeaderYStart)
        GR.DrawString("Price (GH₵)", FNT, Brushes.Black, UnitWidth * 5.5, LNHeaderYStart)
        GR.DrawString("Qty", FNT, Brushes.Black, UnitWidth * 9, LNHeaderYStart)
        GR.DrawString("Total (GH₵)", FNT, Brushes.Black, UnitWidth * 10.5, LNHeaderYStart)


        ' final part is to render the text on the recipt
        Dim I As Integer
        For I = 0 To Rows.Count - 1

            ' find the y
            Dim YLOC = UnitHeight * I + LNDetailsStart

            ' render the values
            GR.DrawString(I + 1, FNT, Brushes.Black, UnitWidth * 0, YLOC)
            GR.DrawString(Rows(I).Cells(1).Value, FNT, Brushes.Black, UnitWidth * 1, YLOC)
            GR.DrawString(Rows(I).Cells(3).Value, FNT, Brushes.Black, UnitWidth * 6, YLOC)
            GR.DrawString(Rows(I).Cells(4).Value, FNT, Brushes.Black, UnitWidth * 9, YLOC)
            GR.DrawString(Rows(I).Cells(5).Value, FNT, Brushes.Black, UnitWidth * 11, YLOC)


        Next


        ' render the total
        GR.DrawString("Subtotal: GH₵ " & ReceiptTotal, FNT, Brushes.Black, 0, LNDetailsStart + ReceiptDetailsHeight + UnitHeight)
        GR.DrawString("Amount Given: GH₵ " & AmountGiven, FNT, Brushes.Black, 0, LNDetailsStart + ReceiptDetailsHeight + (UnitHeight * 3))
        GR.DrawString("Balance Due: GH₵ " & Change, FNT, Brushes.Black, 0, LNDetailsStart + ReceiptDetailsHeight + (UnitHeight * 5))



        ' write the recipt no and the receipt date
        GR.DrawString("Receipt No:" & ReceiptNo, FNT, Brushes.Black, 0, 0)
        GR.DrawString("Receipt Date:" & ReceiptDate, FNT, Brushes.Black, 0, UnitHeight)


        ' end the drawing
        Return BMP
    End Function





    ' this one is used to print a document
    Private Sub PrintDoc_PrintPage(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs) Handles PrintDoc.PrintPage
        e.Graphics.DrawImage(ReceiptImage, 0, 0, ReceiptImage.Width, ReceiptImage.Height)
        e.HasMorePages = False
    End Sub

    ' used to select a printer
    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        If PrintDLG.ShowDialog = Windows.Forms.DialogResult.Cancel Then
            Exit Sub
        End If

        TextBox5.Text = PrintDLG.PrinterSettings.PrinterName

        ' save the printer name in the database
        Try
            Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
            VTA.UpdateDbVar(TextBox5.Text, "printer")
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub



    ' draw the receipt
    Private Sub TextBox6_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox6.TextChanged
        DrawReceiptPreview()

        If PB.Image IsNot Nothing Then
            Try
                Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
                VTA.UpdateDbVar(TextBox6.Text, "unit width")
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            End Try
        End If

    End Sub

    Public Sub DrawReceiptPreview()

        ' check the width is valid
        If Not IsNumeric(TextBox6.Text) Then
            PB.Image = Nothing
            Exit Sub
        End If

        Dim L As Double = Double.Parse(TextBox6.Text)
        If Math.Truncate(L) <> L Then
            PB.Image = Nothing
            Exit Sub
        End If

        If L <= 0 Then
            PB.Image = Nothing
            Exit Sub
        End If

        ' check the height is valid
        If Not IsNumeric(TextBox7.Text) Then
            PB.Image = Nothing
            Exit Sub
        End If

        L = Double.Parse(TextBox7.Text)
        If Math.Truncate(L) <> L Then
            PB.Image = Nothing
            Exit Sub
        End If

        If L <= 0 Then
            PB.Image = Nothing
            Exit Sub
        End If

        ' check the font size
        If Not IsNumeric(TextBox8.Text) Then
            PB.Image = Nothing
            Exit Sub
        End If

        L = Double.Parse(TextBox8.Text)
        If Math.Truncate(L) <> L Then
            PB.Image = Nothing
            Exit Sub
        End If

        If L <= 0 Then
            PB.Image = Nothing
            Exit Sub
        End If

        Try
            PB.Image = DrawReceipt(DGV3.Rows, 838, "2012/01/01 10:45:01", 123, 200, 77, TextBox6.Text, TextBox7.Text, TextBox8.Text)
        Catch ex As Exception
            PB.Image = Nothing
        End Try

    End Sub

    Private Sub TextBox7_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox7.TextChanged
        DrawReceiptPreview()
        If PB.Image IsNot Nothing Then
            Try
                Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
                VTA.UpdateDbVar(TextBox7.Text, "unit height")
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            End Try
        End If

    End Sub

    Private Sub TextBox8_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox8.TextChanged
        DrawReceiptPreview()
        If PB.Image IsNot Nothing Then
            Try
                Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
                VTA.UpdateDbVar(TextBox8.Text, "font size")
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            End Try
        End If

    End Sub

    Private Sub DGV3_CellEndEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGV3.CellEndEdit
        DrawReceiptPreview()
    End Sub




    Friend Sub TextBox4_TextChanged(sender As Object, e As EventArgs) Handles TextBox4.TextChanged
        Try
            If TextBox4.Text <> "" Then
                Dim AmountTendered As Decimal = Decimal.Parse(TextBox4.Text, Globalization.NumberStyles.Currency)
                Dim Total As Decimal = 0
                For I = 0 To DGV2.Rows.Count - 1
                    Total += DGV2.Rows(I).Cells(5).Value
                Next
                Dim Change As Decimal = AmountTendered - Total
                Label14.Text = (Change)
            End If
        Catch Ex As Exception
            MsgBox("Renter Amount Tendered", MsgBoxStyle.OkOnly Or MsgBoxStyle.Critical, "Error")
        End Try
    End Sub

    Private Sub BackupDBToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BackupDBToolStripMenuItem.Click
        Dim SFD As New SaveFileDialog
        SFD.Filter = "*.backup|*.backup"
        If SFD.ShowDialog = Windows.Forms.DialogResult.Cancel Then
            Exit Sub
        End If
        If BackupRestoreModule.SaveDB(SFD.FileName) Then
            MsgBox("Backup completed successfully", MsgBoxStyle.OkOnly Or MsgBoxStyle.Information, "OK")
        Else
            MsgBox("Unable to make backup", MsgBoxStyle.OkOnly Or MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub RestoreDBToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RestoreDBToolStripMenuItem.Click
        Dim OFD As New OpenFileDialog
        OFD.Filter = "*.backup|*.backup"
        If OFD.ShowDialog = Windows.Forms.DialogResult.Cancel Then
            Exit Sub
        End If
        If Not LoadDB(OFD.FileName) Then
            MsgBox("Error restoring the Database", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        Else
            MsgBox("Restore successful", MsgBoxStyle.Information Or MsgBoxStyle.OkOnly, "OK")
            End
        End If
    End Sub

End Class
