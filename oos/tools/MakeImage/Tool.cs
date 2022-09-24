using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MakeImage
{
    class Tool
    {
        public static Stream imgfs;     /* C.img file stream */

        public static void Init(string diskFilePath)
        {
            if(!File.Exists(diskFilePath))
            {
                Console.WriteLine("{0} Not Exist!", diskFilePath);
                return;
            }

            imgfs = File.Open(diskFilePath, FileMode.Open);
        }
        
        public static byte[] ReadSector(int blkno)
        {
            byte[] buffer = new byte[Constant.BLOCK_SIZE];
            
            imgfs.Seek(Constant.BLOCK_SIZE * blkno, SeekOrigin.Begin);
            imgfs.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public static void WriteSector(ref byte[] buffer, int blkno)
        {
            imgfs.Seek(Constant.BLOCK_SIZE * blkno, SeekOrigin.Begin);
            imgfs.Write(buffer, 0, buffer.Length);

            //imgfs.Flush();
            return;
        }

        public static void WriteInt(int value, ref byte[] buffer, int offset)
        {
            /* 手动强转value到byte[4] */
            byte[] b = new byte[4];
            
            b[0] = (byte)value;
            b[1] = (byte)(value >> 8);
            b[2] = (byte)(value >> 16);
            b[3] = (byte)(value >> 24);

            b.CopyTo(buffer, offset);
        }

        public static int ReadInt(ref byte[] buffer, int offset)
        {
            byte[] b = new byte[4];

            b[0] = buffer[offset];
            b[1] = buffer[offset + 1];
            b[2] = buffer[offset + 2];
            b[3] = buffer[offset + 3];

            int ans = (b[0] | b[1] << 8 | b[2] << 16 | b[3] << 24);

            return ans;
        }

        public static void ClearBlock(int blkno)
        {
            byte[] tmp = new byte[Constant.BLOCK_SIZE];

            tmp = Tool.ReadSector(blkno);
            
            /* Clear the block */
            for( int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = 0;
            }

            Tool.WriteSector(ref tmp, blkno);
        }
    }
}
