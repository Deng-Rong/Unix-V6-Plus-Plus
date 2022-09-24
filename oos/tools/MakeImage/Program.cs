using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MakeImage
{
    class Program
    {
        static string FILE_NOT_EXISTED_PATTERN = "{0} not exist!";
        static void Main(string[] args)
        {
            /* 注释掉arg[]相关代码，为了便于F5直接调试，
             * 而不用deploy.bat。boot.bin，kernel.bin，c.img
             * 直接放在项目的Debug\目录下。
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }
            */

            string diskFilePath = args[0];
            if (!File.Exists(diskFilePath))
            {
                Console.WriteLine(string.Format(FILE_NOT_EXISTED_PATTERN, diskFilePath));
                return;
            }

            //string[] files = new string[4] { "boot.bin", "kernel.bin", "peProgram.exe", "Shell.exe" };//new string[args.Length - 1];
            string[] files = new string[args.Length - 1];
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = args[i + 1];
                if (!File.Exists(files[i]))
                {
                    Console.WriteLine(string.Format(FILE_NOT_EXISTED_PATTERN, files[i]));
                    return;
                }
            }

            Stream fs = File.OpenWrite(diskFilePath);
            Tool.imgfs = fs;

            /* 
             * only for boot.bin and kernel.bin here, not include any exe Files
             */
            for (int i = 0; i < 2 /* files.Length */; i++)
            {
                Stream fsRead = File.OpenRead(files[i]);
                byte[] buffer = new byte[fsRead.Length];
                fsRead.Read(buffer, 0, buffer.Length);

                //写入磁盘镜像
                fs.Write(buffer, 0, buffer.Length);

                fsRead.Close();
            }


            byte[] exeInfo = new byte[Constant.BLOCK_SIZE]; /*  作为19000#扇区 */

            /* 注意c.img中18000#扇区开始为交换区!
             * 
             * 将参数arg中每一个*.exe文件的起始扇区记录在19000#扇区中，
             * exe的数据从19001#开始存放!
             * 
             * 将peProgram.exe写入c.img的EXEBLOCK = 19001#及后续扇区中，
             * 用此方法将外部exe文件放入c.img!! 内核在指定扇区位置上读取peProgram.exe。
             */
            long offset = 1;
            for (int i = 2, j = 0; i < files.Length; i++ )
            {
                Stream fsPERead = File.OpenRead(files[i]);
                byte[] peBuffer = new byte[fsPERead.Length];
                fsPERead.Read(peBuffer, 0, peBuffer.Length);

                //peProgram.exe写入c.img中19000# Sector
                fs.Seek(Constant.BLOCK_SIZE * (Constant.EXEBLOCK + offset), SeekOrigin.Begin);
                fs.Write(peBuffer, 0, peBuffer.Length);

                Tool.WriteInt(Constant.EXEBLOCK + (int)offset, ref exeInfo, j);
                j += sizeof(int);
                Tool.WriteInt((int)fsPERead.Length, ref exeInfo, j);
                j += sizeof(int);

                /* 计算本次exe文件写入占用了多少整数个磁盘块 */
                offset += (fsPERead.Length + (Constant.BLOCK_SIZE - 1)) / Constant.BLOCK_SIZE;
                fsPERead.Close();
            }

            Tool.WriteSector(ref exeInfo, Constant.EXEBLOCK);
            fs.Close();


            /**********************************************/
            /*  下面进行构建Unix V6文件系统格式的C.img    */
            /**********************************************/
            Tool.Init(diskFilePath);

            SuperBlock spb = new SuperBlock();

            DataBlockManager blkMgr = new DataBlockManager(ref spb);
            /* 认为数据区中所有盘块皆为空闲，构建"栈的栈" */
            blkMgr.FreeAllBlock();

            /* 将修改后的spb更新到c.img */
            spb.Update();

            Tool.imgfs.Close();

            Console.Write("Work Done!\n");
            //while (true) ;
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: DiskWriter [DiskFilePath] [File1] ... [FileN]");
        }
    }
}
