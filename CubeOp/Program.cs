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
            if (true==options.listcubes)
            {
                IPAddress startIp = IPAddress.Parse("192.168.100.0");
                Console.WriteLine($"Searching for cubes, starting from {startIp}");
                List<IPAddress> l = CubeSeeker.FindCubesInRange( startIp, 256);
                if (0 == l.Count)
                {
                    Console.WriteLine("No connected cubes found");
                }
                else
                {
                    foreach(var ip in l)
                    {
                        Console.WriteLine($"Found: {ip}");
                    }
                }
                goto exit;
            }

            if (true== options.pingcube)
            {
                if (null!=options.cube_id)
                {
                    Uri resutlUri;
                    bool ok1 = Uri.TryCreate(options.cube_id, UriKind.RelativeOrAbsolute, out resutlUri);
                    if (ok1)
                    {
                        IPAddress cubeip;
                        if (IPAddress.TryParse(resutlUri.Host, out cubeip))
                        {
                            UeiCube ucube = new UeiCube(cubeip);
                            //if (null != CubeSeeker.TryIP(cubeip))
                            if (ucube.IsCubeConnected())
                            {
                                Console.WriteLine($"Cube {cubeip} connected");
                            }
                            else
                            {
                                Console.WriteLine($"Cube {cubeip} can't be reached");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Can't parse cube ip. {resutlUri.Host}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Can't parse cube Uri. {resutlUri.Host}");
                    }
                }
                goto exit;
            }

            if (true==options.resetcube)
            {
                if (null!=options.cube_id)
                {
                    UeiCube ucube = new UeiCube(options.cube_id);
                    if (ucube.IsValidCube)
                    { 
                        if (ucube.IsCubeConnected())
                        {
                            ucube.CubeReset();
                            
                        }
                    }
                }
            }

            if (true==options.dohelp)
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
            foreach(var e in errs)
            {
                var t = e.Tag;
                
                Console.WriteLine(e);
            }
            HelpText htext = HelpText.AutoBuild(pa, x => x, x => x);
            //Console.WriteLine(htext);
        }

    }
}