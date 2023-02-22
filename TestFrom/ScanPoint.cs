using System.Drawing;

namespace DistanceSensorAppDemo;

public record ScanPoint(double X, double Y, float Dim, Color Color)
{
    public override string ToString()
    {
        return $"{{ xPos = {X}, yPos = {Y}, Dim = {Dim}, Color = {Color} }}";
    }
}