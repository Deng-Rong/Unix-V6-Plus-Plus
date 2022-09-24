using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Build
{
    public class Helper
    {
        //DiskInfo配置文件的路径
        public static string DiskInfoConfigFileName = @"disk_info.config";

         /// <summary>
        /// 将byte流转换成值类型
        /// </summary>
        /// <param name="byt">byte流</param>
        /// <returns>转换后的变量</returns>
        public static unsafe object Bytes2Struct(byte[] byt, Type t)
        {
            fixed (byte* pb = &byt[0])
            {
                return Marshal.PtrToStructure(new IntPtr(pb), t);
            }
        }

        /// <summary>
        /// 将值类型强转成byte流
        /// </summary>
        /// <param name="obj">需强转的变量</param>
        /// <returns>字节流</returns>
        public static unsafe byte[] Struct2Bytes(object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] bytes = new byte[size];
            fixed (byte* pb = &bytes[0])
            {
                Marshal.StructureToPtr(obj, new IntPtr(pb), true);
            }
            return bytes;
        }

        /// <summary>
        /// 得到距离1970年一月一日零时零分零秒的秒数
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentTime()
        {
            DateTime given = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan ts = DateTime.Now - given;
            return Convert.ToInt32(ts.TotalSeconds);
        }
    }
}



