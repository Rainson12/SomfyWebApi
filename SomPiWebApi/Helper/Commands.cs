using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unosquare.PiGpio;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;
using Unosquare.PiGpio.NativeTypes;

namespace SomPiWebApi.Helper
{
    public enum SendAction
    {
        Down = 0x4,
        Up = 0x2,
        Stop = 0x1,
        Prog = 0x8
    }
    internal static class Constants
    {
        internal const string PiGpioLibrary = "libpigpio.so";
    }
    public struct PulseNew { public BitMask GpioOn; public BitMask GpioOff; public uint DurationMicroSecs; }

    public class Commands
    {
        SystemGpio TXGPIO = SystemGpio.Bcm04; // 433.42 MHz emitter on GPIO 4


        [DllImport(Constants.PiGpioLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "gpioWaveAddGeneric")]
        public static extern int GpioWaveAddGeneric(uint numPulses, [In, MarshalAs(UnmanagedType.LPArray)] PulseNew[] pulses);


       
        public void Send(int remote, int counter, SendAction action)
        {
            int line = 0;
            try
            {
                var checksum = 0;
                Setup.GpioInitialise();

                IO.GpioSetMode(SystemGpio.Bcm04, PinMode.Output);

                Waves.GpioWaveAddNew();
                byte[] frame = new byte[7];
                line = 4;


                frame[0] = 0xA7;                   // Encryption key. Doesn't matter much
                frame[1] = Convert.ToByte((int)action << 4);             // Which action did you chose? The 4 LSB will be the checksum
                frame[2] = Convert.ToByte(counter >> 8);               // Rolling code (big endian)
                frame[3] = Convert.ToByte((counter & 0xFF));           // Rolling code
                frame[4] = Convert.ToByte(remote >> 16);            // Remote address
                frame[5] = Convert.ToByte(((remote >> 8) & 0xFF)); //Remote address
                frame[6] = Convert.ToByte((remote & 0xFF));         // Remote address
                line = 5;

                foreach (var f in frame)
                {
                    checksum = checksum ^ f ^ (f >> 4);
                }
                line = 6;
                checksum &= 0b1111; // We keep the last 4 bits only

                line = 7;
                frame[1] |= Convert.ToByte(checksum);
                line = 8;

                for (int i = 1; i < frame.Length; i++) // obfuscate
                {
                    frame[i] ^= frame[i - 1];
                }
                line = 9;
                var wf = new List<PulseNew>();
                BitMask pinBitMask = (BitMask)Enum.Parse(typeof(BitMask), (1 << (int)TXGPIO).ToString());
                wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 9415 });
                wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 89565 });
                for (int i = 0; i < 2; i++)
                {
                    wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 2560 });
                    wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 2560 });
                }

                wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 4550 });
                wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 640 });

                for (int i = 0; i < 56; i++)
                {
                    var check = (frame[i / 8] >> (7 - (i % 8))) & 1;
                    if (check == 1)
                    {
                        wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 640 });
                        wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 640 });
                    }
                    else
                    {
                        wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 640 });
                        wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 640 });
                    }
                }
                wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 30415 });






                //1
                for (int i = 0; i < 7; i++)
                {
                    wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 2560 });
                    wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 2560 });
                }

                wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 4550 });
                wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 640 });

                for (int i = 0; i < 56; i++)
                {
                    var check = (frame[i / 8] >> (7 - (i % 8))) & 1;
                    if (check == 1)
                    {
                        wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 640 });
                        wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 640 });
                    }
                    else
                    {
                        wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 640 });
                        wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 640 });
                    }
                }
                wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 30415 });







                //2
                for (int i = 0; i < 7; i++)
                {
                    wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 2560 });
                    wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 2560 });
                }
                wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 4550 });
                wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 640 });

                for (int i = 0; i < 56; i++)
                {
                    var check = (frame[i / 8] >> (7 - (i % 8))) & 1;
                    if (check == 1)
                    {
                        wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 640 });
                        wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 640 });
                    }
                    else
                    {
                        wf.Add(new PulseNew() { GpioOn = pinBitMask, GpioOff = BitMask.None, DurationMicroSecs = 640 });
                        wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 640 });
                    }
                }
                wf.Add(new PulseNew() { GpioOn = BitMask.None, GpioOff = pinBitMask, DurationMicroSecs = 30415 });

                line = 10;
                //var wid = (uint)Waves.GpioWaveAddGeneric((uint)wf.Count, wf.ToArray());

                GpioWaveAddGeneric(Convert.ToUInt32(wf.Count), wf.ToArray());
                var wid = (uint)Waves.GpioWaveCreate();
                line = 11;
                Waves.GpioWaveTxSend(wid, WaveMode.OneShot);
                line = 12;
                while (Waves.GpioWaveTxBusy() == 1)
                {

                }
                line = 13;
                Waves.GpioWaveDelete(wid);
                line = 14;
                Setup.GpioTerminate();
                line = 15;
            }
            catch (Exception ex)
            {
                throw new Exception("line=" + line + "message2" + ex.Message);
            }
        }
    }
}
