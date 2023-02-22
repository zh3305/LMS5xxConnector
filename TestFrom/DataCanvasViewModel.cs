using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DistanceSensorAppDemo;

public class DataCanvasViewModel : INotifyPropertyChanged
{
    private ObservableCollection<ScanPoint> _points;

    public DataCanvasViewModel()
    {
        _points = new ObservableCollection<ScanPoint>();
    }

    public ObservableCollection<ScanPoint> Points
    {
        get { return _points; }
        set
        {
            _points = value;
            OnPropertyChanged("Points");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ClearData()
    {
        Points.Clear();
    }

    public void UpdateData(IEnumerable<ScanPoint> scanPoints)
    {        

        // var points = scanPoints
        //     // .Append(new Point(1, 1))
        //     // .Append(new Point(5, 5))
        //     // .Append(new Point(10, 010))
        //     
        //     .ToArray();
        
        // Console.WriteLine(JsonConvert.SerializeObject(points));
        
        Points = new ObservableCollection<ScanPoint>(scanPoints);
        
    }
}