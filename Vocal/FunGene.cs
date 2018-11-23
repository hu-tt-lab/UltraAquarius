﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Ivi.Visa.Interop;
using System.Text.RegularExpressions;
using System.Windows;
using Codeplex.Data;
using CsvHelper;
using CsvHelper.Configuration;

namespace Vocal
{

    public class FunGene
    {
        public List<Ultrasound> Parameters { get; set; }
        public ResourceManager RM { get; set; }
        public FormattedIO488 DMM { get; set; }
        private string _fungenename;
        private string _strtmp;

        public string Indentifer
        {
            get
            {
                DMM.WriteString("*IDN?");
                _fungenename = Regex.Match(DMM.ReadString(), "[A-Z]{2}[0-9]{4}").ToString();
                return DMM.ReadString();
            }
        }

        public FunGene()
        {
            _fungenename = "none";
            RM = new ResourceManager();
            DMM = new FormattedIO488();
        }

        public string[] GetResourse()
        {
            return RM.FindRsrc("?*::INSTR");
        }

        public string Open(string Address)
        {
            DMM.IO = (IMessage)RM.Open(Address);
            DMM.WriteString("*IDN?");
            _strtmp = DMM.ReadString();
            _fungenename = Regex.Match(_strtmp, "[A-Z]{2}[0-9]{4}").ToString();
            return _fungenename;
        }

        public void Trigger()
        {
            try
            {
                switch (_fungenename)
                {
                    case "DF1906":
                        DMM.WriteString(":TRIG 1");
                        break;
                    case "WF1947":
                        DMM.WriteString("*TRG");
                        break;
                    default: throw new InvalidOperationException();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }

        }

        public void Parameter(Ultrasound parameter)
        {
            try
            {
                switch (parameter.Waveform)
                {
                    case UltrasoundWaveform.Sine:
                        DMM.WriteString(":SOURce1:FUNCtion:SHAPe SIN");
                        DMM.WriteString(":SOURce1:FREQuency:CW " + parameter.Frequency + "HZ");
                        if (parameter.Voltage > 1.0)
                        {
                            throw new ArgumentException("Over Applied Max Voltage !");
                        }
                        DMM.WriteString(":SOURce1:VOLTage:LEVel:IMMediate:AMPLitude " + parameter.Voltage + " VPP");
                        DMM.WriteString(":SOURce1:BURSt:TRIGger:NCYCles " + parameter.Waves);
                        break;
                    case UltrasoundWaveform.Square:
                        DMM.WriteString(":SOURce1:FUNCtion:SHAPe SQUare");
                        DMM.WriteString(":SOURce1:FUNCtion:SQUare:DCYCle " + parameter.Duty + "PCT");
                        DMM.WriteString(":SOURce1:FREQuency:CW " + parameter.Frequency + "HZ");
                        DMM.WriteString(":SOURce1:VOLTage:LEVel:IMMediate:AMPLitude " + parameter.Voltage + " VPP");
                        DMM.WriteString(":SOURce1:BURSt:TRIGger:NCYCles " + parameter.Waves);
                        break;
                    default: throw new InvalidOperationException();
                }              
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
        


        public string OnOff()
        {
            try
            {
                DMM.WriteString(":OUTPut:STATe?");
                string strtmp = DMM.ReadString();
                switch (strtmp)
                {
                    case "0\n":
                        return "OFF";
                    case "0":
                        return "OFF";
                    case "1\n":
                        return "ON";
                    case "1":
                        return "ON";
                    default: throw new InvalidOperationException();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
                return error.Message;
            }
        }

        public string OnOff(string output)
        {
            try
            {
                int n = 0;
                switch (output)
                {
                    case "OFF":
                        n = 0;
                        break;
                    case "ON":
                        n = 1;
                        break;
                    default: throw new InvalidOperationException();
                }
                DMM.WriteString(":OUTPut:STATe " + n.ToString());
                return OnOff();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
                return error.Message;
            }

        }


        public void Oscillation(string mode)
        {
            try
            {
                switch (_fungenename)
                {
                    case "DF1906":
                        switch (mode)
                        {
                            case "BURST":
                                DMM.WriteString(":SOURce:MODE BRST");
                                break;
                            case "TRIGGER":
                                DMM.WriteString(":SOURce:MODE TRIG");
                                break;
                            default: throw new InvalidOperationException();
                        }
                        break;
                    case "WF1947":
                        switch (mode)
                        {
                            case "TRIGGER":
                                DMM.WriteString(":SOURce:BURSt:STATe ON");
                                DMM.WriteString(":SOURce1:BURSt:MODE TRIG");
                                break;
                            default: throw new InvalidOperationException();
                        }
                        break;
                    default: throw new InvalidOperationException();
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }


        public void BurstSyncType(string mode)
        {
            try
            {
                switch (_fungenename)
                {
                    case "DF1906":
                        switch (mode)
                        {
                            case "BURSTSYNC":
                                DMM.WriteString(":OUTPut:SYNC:TYPE STATe");
                                break;
                            case "SYNC":
                                DMM.WriteString(":OUTPut:SYNC:TYPE PHASe");
                                break;
                            default: throw new InvalidOperationException();
                        }
                        break;
                    case "WF1947":
                        switch (mode)
                        {
                            case "BURSTSYNC":
                                DMM.WriteString(":OUTPut1:SYNC:BURSt:TYPE BSYNc");
                                break;
                            case "SYNC":
                                DMM.WriteString(":OUTPut1:SYNC:BURSt:TYPE SYNC");
                                break;
                            default: throw new InvalidOperationException();
                        }
                        break;
                    default: throw new InvalidOperationException();
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
    }
}