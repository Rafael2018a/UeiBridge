using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Configuration;
using UeiBridge.Library;

namespace CubeOp
{

    public class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run(args);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Any key to continue...");
                Console.ReadKey();
            }
        }

        ParserResult<Options> pa;
        /// <summary>
        /// --ip only: check if cube connected and list the devices (including dnrp)
        /// --reset-cube
        /// --reset-device <num>>
        /// --list-devices
        /// --list-cubes
        /// --ping-cube
        /// </summary>
        /// <param name="args"></param>
        public void Run(string[] args)
        {
            pa = CommandLine.Parser.Default.ParseArguments<Options>(args);

            var r = pa.WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts));

            pa.WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        bool IsCubeConnected { get; set; }
        int RunOptionsAndReturnExitCode(Options options)
        {
            if (true == options.listcubes)
            {
                IPAddress startIp = IPAddress.Parse("192.168.100.0");
                Console.WriteLine($"Searching for cubes, starting from {startIp}");
                List<IPAddress> l = CubeSeeker.FindCubesInRange(startIp, 256);
                if (0 == l.Count)
                {
                    Console.WriteLine("No connected cubes found");
                }
                else
                {
                    foreach (var ip in l)
                    {
                        Console.WriteLine($"Found: {ip}");
                    }
                }
                goto exit;
            }

            if (true == options.pingcube)
            {
                if (null != options.cube_id)
                {
                    UeiCube ucube = new UeiCube(options.cube_id);
                    if (ucube.IsCubeConnected())
                    {
                        Console.WriteLine($"Cube {options.cube_id} connected");
                    }
                    else
                    {
                        Console.WriteLine($"Cube {options.cube_id} can't be reached");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: Cube id not specified.");
                }
                goto exit;
            }

            // watchdog reset to given cube
            // ==========================
            if (true == options.resetcube)
            {
                if (null != options.cube_id)
                {
                    UeiCube ucube = new UeiCube(options.cube_id);
                    if (ucube.IsValidAddress)
                    {
                        if (ucube.IsSimuCube || ucube.IsCubeConnected())
                        {
                            if (true == ucube.CubeReset())
                            {
                                Console.WriteLine("Reset success");
                            }
                            else
                            {
                                Console.WriteLine("Reset failed");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Cube {options.cube_id} can't be reached");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse cube id. {options.cube_id}");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: Cube id not specified.");
                }

                goto exit;
            }

            // reset device in specifed slot
            // ============================
            if (true == options.resetdevice)
            {
                if (null != options.cube_id && options.slotnumber>=0)
                {
                    UeiCube ucube = new UeiCube(options.cube_id);
                    if (ucube.IsValidAddress)
                    {
                        if (ucube.IsSimuCube || ucube.IsCubeConnected())
                        {
                            string devuri = $"{ucube.GetCubeUri()}Dev{options.slotnumber}";
                            if (true == ucube.DeviceReset(devuri))
                            {
                                Console.WriteLine($"Device {devuri} Reset success");
                            }
                            else
                            {
                                Console.WriteLine($"Device {devuri} Reset fail");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Cube {options.cube_id} can't be reached");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse cube id. {options.cube_id}");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: Cube id or slot number not specified.");
                }

                goto exit;
            }

            // list devices in given cube
            // ==========================
            if (true == options.listdevices)
            {
                if (null != options.cube_id)
                {
                    UeiCube ucube = new UeiCube(options.cube_id);
                    if (ucube.IsValidAddress)
                    {
                        if (ucube.IsSimuCube || ucube.IsCubeConnected())
                        {
                            var ldev = ucube.GetDeviceList();
                            foreach (var dev in ldev)
                            {

                                Console.WriteLine($"{dev.GetDeviceName().PadRight(15)}; Uri:{dev.GetResourceName()}; Serial:{ dev.GetSerialNumber()}; Slot:{dev.GetSlot()}; Index:{dev.GetIndex()}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Cube {options.cube_id} can't be reached");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse cube id. {options.cube_id}");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: Cube id not specified.");
                }
                goto exit;
            }

            if (true == options.dohelp)
            {
                HelpText htext = HelpText.AutoBuild(pa, x => x, x => x);
                Console.WriteLine(htext);
            }


        //resutlUri.AbsolutePath
        //"/Dev14"
        //resutlUri.AbsoluteUri
        //"pdna://192.168.100.2/Dev14"
        //resutlUri.Host
        //"192.168.100.2"

        exit: return 0;
        }

        void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var e in errs)
            {
                var t = e.Tag;

                Console.WriteLine(e);
            }
            HelpText htext = HelpText.AutoBuild(pa, x => x, x => x);
            //Console.WriteLine(htext);
        }

    }
}