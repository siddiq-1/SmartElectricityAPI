﻿namespace SmartElectricityAPI.Models;

public class MqttServerSettings
{
    public string Server { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
