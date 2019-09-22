using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;

namespace TiTsEd.Common
{
    public static class Logger
    {
        public static void Error(Exception e)
        {
            Error(e.ToString());
        }

        public static void Error(string msg)
        {
            try
            {
                string dataVersion = TiTsEd.ViewModel.VM.Instance != null ? TiTsEd.ViewModel.VM.Instance.FileVersion : "";
                if (!String.IsNullOrEmpty(dataVersion))
                {
                    dataVersion = String.Format(", TiTs Data: {0}", dataVersion);
                }

                // if possible, make TiTsEd's and TiTs' versions an integral part of the exception message,
                // so we don't have to rely on users' claims of being up to date anymore
                msg = String.Format("[{0}: {1}{2}]\n{3}",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    dataVersion,
                    msg);

                File.WriteAllText("TiTsEd.log", msg);
                //Console.WriteLine(msg);
            }
            catch(IOException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (NotSupportedException)
            {
            }
        }
    }
}
