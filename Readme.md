<!-- default file list -->
*Files to look at*:

* [Helpers.cs](./CS/Helpers.cs) (VB: [Helpers.vb](./VB/Helpers.vb))
* [MainWindow.xaml](./CS/MainWindow.xaml) (VB: [MainWindow.xaml.vb](./VB/MainWindow.xaml.vb))
* [MainWindow.xaml.cs](./CS/MainWindow.xaml.cs) (VB: [MainWindow.xaml.vb](./VB/MainWindow.xaml.vb))
<!-- default file list end -->
# End-User Restrictions - How to define "availability" intervals for different resources


<p>This example illustrates how to define different time intervals that are available for scheduling. This information is stored in the <strong>ResourcesAvailabilities.mdb</strong> database. It has the <strong>ResourcesAvailabilities </strong>table with the following schema:</p><p></p><p><strong>Id: Integer, </strong></p><p><strong>ResourceId: String, </strong></p><p><strong>StartTime: DateTime, </strong></p><p><strong>EndTime: DateTime</strong></p><p></p><p>So, generally, each resource can have independent availability intervals. This information is loaded and exposed via the <strong>ResourcesAvailabilities </strong>class.</p><p></p><p>First of all, we use this information to modify the default appearance of the <a href="http://documentation.devexpress.com/#WPF/CustomDocument8725">Time Cells</a> by overriding the default cell style (see <a href="http://documentation.devexpress.com/#WPF/CustomDocument8923">DXScheduler Styles</a>) for <strong>Day </strong>and <strong>WorkWeek </strong>views. The actual cell color is calculated and passed to the cell in the <strong>CellColorConverter</strong> class. This converter is used in XAML as follows:</p><p></p>

```xaml
    ...
    <Grid.Background>
        <MultiBinding Converter="{local:CellColorConverter}">
            <Binding Path="IntervalStart" />
            <Binding Path="IntervalEnd" />
            <Binding Path="ResourceId" />
            <Binding Path="Brushes.Cell" />
        </MultiBinding>
    </Grid.Background>
    ...
```

<p></p><p>Then, we disallow appointment creation and modification actions if their execution is initiated outside available intervals. For this, we handle the following events:</p><p></p><p><a href="http://documentation.devexpress.com/#WPF/DevExpressXpfSchedulerSchedulerControl_AllowAppointmentCreatetopic">SchedulerControl.AllowAppointmentCreate Event</a></p><p><a href="http://documentation.devexpress.com/#WPF/DevExpressXpfSchedulerSchedulerStorage_AppointmentChangingtopic">SchedulerStorage.AppointmentChanging Event</a></p><p></p><p>Here is a screenshot that illustrates a sample application in action:</p><p></p><p><img src="https://raw.githubusercontent.com/DevExpress-Examples/end-user-restrictions-how-to-define-availability-intervals-for-different-resources-e4673/15.2.4+/media/2e6b306c-fedf-4b06-afdd-305ca92d1422.png"></p>

<br/>


