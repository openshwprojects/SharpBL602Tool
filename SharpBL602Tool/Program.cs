
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using static BL602Flasher;

namespace BL602Tool
{

    class Program
    {
        static void Main(string[] args)
        {
            string port = "COM3";
            string toWrite = "";
            string toRead = "";
            string toInfo = "";
            toInfo = "Axus_eWeLink_3G_Switch_SDV-002_V1.2_(FWSW-HSBL602-SWITCH-BL602L_v1.3.3).bin";
            int testLen = 12345;
            bool bErase = false;
            bool bInfo = false;
            bool bTest = false;
            int baud = 115200;
            int readSize = 2097152;

            // Erase: SharpBL602Tool.exe -p COM3 -ef
            // Read: SharpBL602Tool.exe -p COM3 -rf 2097152 dump_2mb.bin
            // Write: SharpBL602Tool.exe -p COM3 -wf obk.bin
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-p" && i + 1 < args.Length)
                {
                    port = args[++i];
                }
                if (args[i] == "-wf" && i + 1 < args.Length)
                {
                    toWrite = args[++i];
                }
                if (args[i] == "-b" && i + 1 < args.Length)
                {
                    baud = int.Parse(args[++i]);
                }
                if (args[i] == "-ef")
                {
                    bErase = true;
                }
                if (args[i] == "-t")
                {
                    bTest = true;
                }
                if (args[i] == "-tl" && i + 1 < args.Length)
                {
                    bTest = true;
                    testLen = int.Parse(args[++i]);
                }
                if (args[i] == "-i" && i + 1 < args.Length)
                {
                    toInfo = args[++i];
                }
                if (args[i] == "-rf" && i + 2 < args.Length)
                {
                    i++;
                    string input = args[i];
					readSize = int.Parse(input);
                    i++;
                    toRead = args[i];
                }
            }
            BL602Flasher f = new BL602Flasher();
            f.openPort(port, baud);
            f.Sync();
            if(f.getAndPrintInfo() == null)
            {
                Console.WriteLine("Initial get info failed.");
                Console.WriteLine("This may happen if you don't reset between flash operations");
                return;
            }
            byte[] loaderBinary = File.ReadAllBytes("eflash_loader_rc32m.bin");
            f.loadAndRunPreprocessedImage(loaderBinary);
            //resync in eflash
            f.Sync();
            f.readFlashID();
            // toWrite = "Axus_eWeLink_3G_Switch_SDV-002_V1.2_(FWSW-HSBL602-SWITCH-BL602L_v1.3.3).bin";
            //toRead = "full_binary_with_app_OpenBL602_1.18.57.bin";
            //readSize = 1024;
            if (toRead.Length>0)
            {
                Console.WriteLine("Will do dump " + readSize);
                byte[] res = f.readFlash(0, readSize);
                if(res != null)
                {
                    File.WriteAllBytes(toRead, res);
                    Console.WriteLine("Dump done!");
                }
                else
                    Console.WriteLine("Dump failed!");
            }
            if (toInfo.Length > 0)
            {
                BLHeaderReader.Read(toInfo);
            }
            if(bErase)
            {
                Console.WriteLine("Will do flash erase all...");
                f.eraseFlash();
                Console.WriteLine("Erase done!");
            }
            if (toWrite.Length > 0)
            {
                Console.WriteLine("Will do flash erase all...");
                f.eraseFlash();
                Console.WriteLine("Erase done!");
                Console.WriteLine("Will do flash " + toWrite + "...");
                if(File.Exists(toWrite) == false)
                {
                    Console.WriteLine("File " + toWrite + " does not exist.");
                }
                else
                {
                    byte[] x = File.ReadAllBytes(toWrite);
                    f.writeFlash(x, 0);
                    Console.WriteLine("Flash done!");
                }
            }
            if(bTest)
            {
                Console.WriteLine("Will do erase/write/read test, erasing...");
                f.eraseFlash();
                byte[] x = new byte[testLen];
                for (int i = 0; i < x.Length; i++)
                {
                    x[i] = (byte)(i % 256);
                }
                Console.WriteLine("Writing...");
                f.writeFlash(x, 0);
                Console.WriteLine("Reading...");
                byte[] res = f.readFlash(0, x.Length);
                Console.WriteLine("Checking...");
                int iFail = 0;
                for(int i = 0; i < x.Length; i++)
                {
                    if(res[i] != x[i])
                    {
                        iFail++;
                    }
                }
                if(iFail>0)
                {
                    Console.WriteLine("Test erase/write/read failed with " + iFail + " out of " + x.Length);
                }
                else
                {
                    Console.WriteLine("Test erase/write/read OK for " + x.Length);
                }
            }
            Console.WriteLine("All done");
        }
    }
}