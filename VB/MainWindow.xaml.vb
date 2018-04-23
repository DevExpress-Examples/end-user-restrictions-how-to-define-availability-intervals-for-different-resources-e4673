Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Windows
Imports DevExpress.Xpf.Scheduler
Imports DevExpress.XtraScheduler

Namespace SchedulerResourcesAvailabilitiesWpf
    Partial Public Class MainWindow
        Inherits Window

        Public Sub New()
            InitializeComponent()

            schedulerControl1.ApplyTemplate()
            AddHandler schedulerControl1.Storage.AppointmentChanging, AddressOf Storage_AppointmentChanging
            AddHandler schedulerControl1.AllowAppointmentCreate, AddressOf schedulerControl1_AllowAppointmentCreate

            ResourceFiller.FillResources(schedulerControl1.Storage, 3)
            schedulerControl1.Start = New Date(2012, 1, 1)
        End Sub

        Private Sub schedulerControl1_AllowAppointmentCreate(ByVal sender As Object, ByVal e As AppointmentOperationEventArgs)
            Dim available As Boolean = ResourcesAvailabilities.IsIntervalAvailableForResource(schedulerControl1.SelectedResource.Id.ToString(), schedulerControl1.SelectedInterval)

            errorInfo.Content = (If(available, Nothing, ResourcesAvailabilities.WarningMessage))

            e.Allow = available
        End Sub

        Private Sub Storage_AppointmentChanging(ByVal sender As Object, ByVal e As PersistentObjectCancelEventArgs)
            Dim apt As Appointment = CType(e.Object, Appointment)

            Dim available As Boolean = ResourcesAvailabilities.IsIntervalAvailableForResource(apt.ResourceId.ToString(), New TimeInterval(apt.Start, apt.End))

            errorInfo.Content = (If(available, Nothing, ResourcesAvailabilities.WarningMessage))

            e.Cancel = Not available
        End Sub
    End Class

    Public NotInheritable Class ResourceFiller

        Private Sub New()
        End Sub

        Public Shared Users() As String = { "Sarah Brighton", "Ryan Fischer", "Andrew Miller" }
        Public Shared Usernames() As String = { "sbrighton", "rfischer", "amiller" }

        Public Shared Sub FillResources(ByVal storage As SchedulerStorage, ByVal count As Integer)
            Dim resources As ResourceStorage = storage.ResourceStorage
            storage.BeginUpdate()
            Try
                Dim cnt As Integer = Math.Min(count, Users.Length)
                For i As Integer = 1 To cnt
                    resources.Add(New Resource(Usernames(i - 1), Users(i - 1)))
                Next i
            Finally
                storage.EndUpdate()
            End Try
        End Sub
    End Class

    Public NotInheritable Class ResourcesAvailabilities

        Private Sub New()
        End Sub
        Private Shared availabilitiesTable As DataTable = Nothing

        Public Shared ReadOnly Property WarningMessage() As String
            Get
                Return "This time interval is not available for this resource."
            End Get
        End Property

        Public Shared Function IsIntervalAvailableForResource(ByVal resourceId As String, ByVal timeInterval As TimeInterval) As Boolean
            Dim availabilities As TimeIntervalCollection = GetAvailabilitiesForResource(resourceId.ToString())
            Dim result As Boolean = False

            For i As Integer = 0 To availabilities.Count - 1
                If availabilities(i).Contains(timeInterval) Then
                    result = True
                    Exit For
                End If
            Next i

            Return result
        End Function

        Private Shared Function GetAvailabilitiesForResource(ByVal resourceId As String) As TimeIntervalCollection
            Dim table As DataTable = GetAvailabilitiesTable()
            Dim view As New DataView(table, String.Format("ResourceId = '{0}'", resourceId), String.Empty, DataViewRowState.CurrentRows)
            Dim result As New TimeIntervalCollection()

            For i As Integer = 0 To view.Count - 1
                result.Add(New TimeInterval(Convert.ToDateTime(view(i)("StartTime")), Convert.ToDateTime(view(i)("EndTime"))))
            Next i

            Return result
        End Function

        Private Shared Function GetAvailabilitiesTable() As DataTable
            If availabilitiesTable Is Nothing Then
                Using connection As New OleDbConnection(System.Configuration.ConfigurationManager.ConnectionStrings("ResourcesAvailabilitiesConnectionString").ConnectionString)
                    Dim selectCommand As New OleDbCommand("SELECT * FROM ResourcesAvailabilities", connection)
                    Dim dataAdapter As New OleDbDataAdapter(selectCommand)
                    availabilitiesTable = New DataTable("AvailabilitiesTable")

                    dataAdapter.Fill(availabilitiesTable)
                    availabilitiesTable.Constraints.Add("IDPK", availabilitiesTable.Columns("Id"), True)

                    connection.Close()
                End Using
            End If

            Return availabilitiesTable
        End Function
    End Class
End Namespace