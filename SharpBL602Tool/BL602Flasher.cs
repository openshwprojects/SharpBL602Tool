using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

class BL602Flasher
{
    private SerialPort _port;
    int baudRate;

    public bool openPort(string portName, int baudRate)
    {
        try
        {
            Console.WriteLine("Opening port " + portName + "...");
            _port = new SerialPort(portName, 115200);
            this.baudRate = baudRate;
            _port.Open();
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();
            Console.WriteLine("Port " + portName + " open!");
        }
        catch (Exception)
        {
            Console.WriteLine("Error: Open {0}, {1} baud!", portName, baudRate);
            Environment.Exit(-1);
            return true;
        }
        return false;
    }
    public bool Sync()
    {
        // Flush input buffer
        while (_port.BytesToRead > 0)
        {
            _port.ReadByte();
        }

        Console.WriteLine("Starting sync...");
        // Write initialization sequence
        byte[] syncRequest = new byte[70];
        for (int i = 0; i < syncRequest.Length; i++) syncRequest[i] = (byte)'U';
        _port.Write(syncRequest, 0, syncRequest.Length);

        Thread.Sleep(100);

        // Check for 2-byte response
        if (_port.BytesToRead >= 2)
        {
            byte[] response = new byte[2];
            _port.Read(response, 0, 2);
            if (response[0] == 'O' && response[1] == 'K')
            {
                Console.WriteLine("SYNC < OK");
                return true;
            }
            else
            {
                Console.WriteLine("SYNC < Invalid response (type 2): {0:X2} {1:X2}", response[0], response[1]);
            }
        }
        else
        {
            byte[] leftovers = new byte[Math.Min(_port.BytesToRead, 1024)];
            _port.Read(leftovers, 0, leftovers.Length);
            Console.WriteLine("SYNC < Invalid response (type 1): " + BitConverter.ToString(leftovers));
        }

        return false;
    }

}

