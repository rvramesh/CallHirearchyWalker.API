/************************************************************************************************
 * DICLAIMER : The below code is hacked over a weekend. This is not of a production quality
 * and it is proved to be right and not wrong. Please use this code AS/IS and I am not 
 * responsible for damages caused.  Software distributed under the License is distributed 
 * on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied.
 * 
 * Written by Ramesh Vijayaraghavan, <contact@rvramesh.com>
 *
 *
 * You are free to use this in any way you want, in case you find this useful or working for you.
 * ***********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CallHirearchyWalker.API
{
    public static class Log
    {

        public static void Exception(Exception ex, string location)
        {
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("ERROR:");
            Console.WriteLine(location);
            Console.WriteLine(ex.ToString());
            Console.WriteLine("-------------------------------------------------------------------------");
        }

        public static void Info(string format, params object[] objects)
        {
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("INFO:");
            Console.WriteLine(string.Format(format, objects));
            Console.WriteLine("-------------------------------------------------------------------------");
        }

    }
}
