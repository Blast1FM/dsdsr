using System;

namespace Server;

public class PTPMessage
{
    public DateTime CreatedAt {get;set;} = DateTime.Now;
    public string Type { get; set; }
    public string Data { get; set; }
    public string Status { get; set; }
}
