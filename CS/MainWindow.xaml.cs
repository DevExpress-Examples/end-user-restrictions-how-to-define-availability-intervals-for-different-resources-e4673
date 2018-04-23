using System;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using DevExpress.Xpf.Scheduler;
using DevExpress.XtraScheduler;

namespace SchedulerResourcesAvailabilitiesWpf {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            schedulerControl1.ApplyTemplate();
            schedulerControl1.Storage.AppointmentChanging += Storage_AppointmentChanging;
            schedulerControl1.AllowAppointmentCreate += schedulerControl1_AllowAppointmentCreate;

            ResourceFiller.FillResources(schedulerControl1.Storage, 3);
            schedulerControl1.Start = new DateTime(2012, 1, 1);
        }

        private void schedulerControl1_AllowAppointmentCreate(object sender, AppointmentOperationEventArgs e) {
            bool available = ResourcesAvailabilities.IsIntervalAvailableForResource(
                schedulerControl1.SelectedResource.Id.ToString(),
                schedulerControl1.SelectedInterval);

            errorInfo.Content = (available ? null : ResourcesAvailabilities.WarningMessage);

            e.Allow = available;
        }

        private void Storage_AppointmentChanging(object sender, PersistentObjectCancelEventArgs e) {
            Appointment apt = (Appointment)e.Object;

            bool available = ResourcesAvailabilities.IsIntervalAvailableForResource(
                apt.ResourceId.ToString(),
                new TimeInterval(apt.Start, apt.End));

            errorInfo.Content = (available ? null : ResourcesAvailabilities.WarningMessage);

            e.Cancel = !available;
        }
    }

    public static class ResourceFiller {
        public static string[] Users = new string[] { "Sarah Brighton", "Ryan Fischer", "Andrew Miller" };
        public static string[] Usernames = new string[] { "sbrighton", "rfischer", "amiller" };

        public static void FillResources(SchedulerStorage storage, int count) {
            ResourceStorage resources = storage.ResourceStorage;
            storage.BeginUpdate();
            try {
                int cnt = Math.Min(count, Users.Length);
                for (int i = 1; i <= cnt; i++) {
                    resources.Add(new Resource(Usernames[i - 1], Users[i - 1]));
                }
            }
            finally {
                storage.EndUpdate();
            }
        }
    }

    public static class ResourcesAvailabilities {
        private static DataTable availabilitiesTable = null;

        public static string WarningMessage { get { return "This time interval is not available for this resource."; } }

        public static bool IsIntervalAvailableForResource(string resourceId, TimeInterval timeInterval) {
            TimeIntervalCollection availabilities = GetAvailabilitiesForResource(resourceId.ToString());
            bool result = false;

            for (int i = 0; i < availabilities.Count; i++) {
                if (availabilities[i].Contains(timeInterval)) {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private static TimeIntervalCollection GetAvailabilitiesForResource(string resourceId) {
            DataTable table = GetAvailabilitiesTable();
            DataView view = new DataView(table, string.Format("ResourceId = '{0}'", resourceId), string.Empty, DataViewRowState.CurrentRows);
            TimeIntervalCollection result = new TimeIntervalCollection();

            for (int i = 0; i < view.Count; i++) {
                result.Add(new TimeInterval(Convert.ToDateTime(view[i]["StartTime"]), Convert.ToDateTime(view[i]["EndTime"])));
            }

            return result;
        }

        private static DataTable GetAvailabilitiesTable() {
            if (availabilitiesTable == null) {
                using (OleDbConnection connection = new OleDbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ResourcesAvailabilitiesConnectionString"].ConnectionString)) {
                    OleDbCommand selectCommand = new OleDbCommand("SELECT * FROM ResourcesAvailabilities", connection);
                    OleDbDataAdapter dataAdapter = new OleDbDataAdapter(selectCommand);
                    availabilitiesTable = new DataTable("AvailabilitiesTable");

                    dataAdapter.Fill(availabilitiesTable);
                    availabilitiesTable.Constraints.Add("IDPK", availabilitiesTable.Columns["Id"], true);

                    connection.Close();
                }
            }

            return availabilitiesTable;
        }
    }
}