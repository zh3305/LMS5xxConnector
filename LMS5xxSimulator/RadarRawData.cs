using System;

public class RadarRawData
{
    public DateTime Timestamp { get; set; }
    public byte[] RawBytes { get; set; }
    public string RadarName { get; set; }
} 