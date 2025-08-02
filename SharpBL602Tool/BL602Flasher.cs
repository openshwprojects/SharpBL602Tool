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
        for (int i = 0; i < 1000; i++)
        {
            Console.WriteLine("Sync attempt " + i + "... please ground BOOT and reset...");
            if (internalSync())
            {
                Console.WriteLine("Sync OK!");
                return true;
            }
            Console.WriteLine("Sync failed, will retry!");
            Thread.Sleep(500);
        }
        return false;
    }
    bool internalSync() { 
        // Flush input buffer
        while (_port.BytesToRead > 0)
        {
            _port.ReadByte();
        }

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
                return true;
            }
        }
        else
        {
            byte[] leftovers = new byte[Math.Min(_port.BytesToRead, 1024)];
            _port.Read(leftovers, 0, leftovers.Length);
        }

        return false;
    }

    internal void getInfo()
    {
        executeCommand(0x10, null);
    }
    void executeCommand(int type, byte [] parms)
    {
        int len = 0;
        if (parms != null)
        {
            len = parms.Length;
        }
        byte[] raw = new byte[] { (byte)type, 1, (byte)(len & 0xFF), (byte)(len >> 8) };
        _port.Write(raw,0,raw.Length);
        if (parms != null)
        {
            _port.Write(parms, 0, parms.Length);
        }
        Thread.Sleep(100);
        if(_port.BytesToRead >= 2)
        {
            byte[] rep = new byte[2];
            _port.Read(rep, 0, 2);
            if(rep[0] == 'O' && rep[1] == 'K')
            {

            }
            else if (rep[0] == 'F' && rep[1] == 'L')
            {

            }
        }
    }
}

