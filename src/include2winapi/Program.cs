﻿namespace include2winapi
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    class Program
    {
        static void Main(String[] args)
        {
            if (0 == args.Length)
            {
                Console.WriteLine(@"Usage: include2winapi <path to Windows SDK include directory>");
                Console.WriteLine(@"Example: include2winapi C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Include");
                return;
            }

            String directoryName = args[0];

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(
@"namespace Vurdalakov.UsbDevicesDotNet
{
    using System;

    public static partial class UsbDeviceWinApi
    {

        // devpkey.h

        public static class DevicePropertyKeys
        {
");

            using (StreamReader streamReader = new StreamReader(Path.Combine(directoryName, "devpkey.h")))
            {
                while (true)
                {
                    String line = streamReader.ReadLine();
                    if (null == line)
                    {
                        break;
                    }

/*
DEFINE_DEVPROPKEY(DEVPKEY_Device_DeviceDesc,             0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 2);     // DEVPROP_TYPE_STRING
*/
                    Match match = Regex.Match(line, @"DEFINE_DEVPROPKEY\(([a-zA-Z0-9_]+),\s+(.*), ((?:0x)?[0-9a-fA-F]+)\);");
                    if (match.Groups.Count != 4)
                    {
                        if (line.Contains("DEFINE_DEVPROPKEY("))
                        {
                            throw new Exception(String.Format("Line not handled:\n{0}", line));
                        }
                        continue;
                    }
/*
            public static DEVPROPKEY DEVPKEY_NAME = new DEVPROPKEY() { Fmtid = new Guid(0xb725f130, 0x47ef, 0x101a, 0xa5, 0xf1, 0x02, 0x60, 0x8c, 0x9e, 0xeb, 0xac), Pid = 10 };
*/
                    stringBuilder.AppendFormat("            public static DEVPROPKEY {0} = new DEVPROPKEY() {{ Fmtid = new Guid({1}), Pid = {2} }};", match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                    stringBuilder.AppendLine();
                }
            }

            stringBuilder.AppendLine(
@"        }
    }
}
");

            using (StreamWriter streamWriter = new StreamWriter("UsbDeviceWinApi.DevicePropertyKeys.cs"))
            {
                streamWriter.Write(stringBuilder);
            }
        }
    }
}
