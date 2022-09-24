/*
 * 机器参数设置计算
 */
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Build
{
    public class MachinePara
    {
        /// <summary>
        /// 磁盘块大小为512字节
        /// </summary>
        public const int Block_Size = 512;

        /// <summary>
        /// BOOT区大小
        /// </summary>
        public const int Boot_Size = 1;

        /// <summary>
        /// 柱面数
        /// </summary>
        public static int Cylinders;

		/// <summary>
		/// 磁头数
		/// </summary>
        public static int Heads;

        /// <summary>
        /// 每磁道扇区数
        /// </summary>
        public static int Spt;

        /// <summary>
        /// 初起块和内核区的大小，单位是块（512B）
        /// </summary>
        public static int BootAndKernelSize;

        /// <summary>
        /// 超级块区的大小，单位是块（512B）
        /// </summary>
        public static int SuperBlockSize;

        /// <summary>
        /// inode区的大小，单位是块（512B）
        /// </summary>
        public static int InodeSize;

        /// <summary>
        /// 交换区大小
        /// </summary>
        public static int SwapSize;

        /// <summary>
        /// 磁盘文件路径
        /// </summary>
        public static string DiskPath;

        /// <summary>
        /// 操作系统文件路径
        /// </summary>
        public static string OsPath;

        /// <summary>
        ///初起块路径 
        /// </summary>
        public static string BootPath;

        /// <summary>
        /// 文件夹路径
        /// </summary>
        public static string ProgramDirPath;

        /// <summary>
        /// 成员变量初始化
        /// </summary>
        /// <param name="diskPath"></param>
        /// <param name="bootPath"></param>
        /// <param name="osPath"></param>
        /// <param name="dirPath"></param>
        public static void Init(string diskPath,string bootPath,string osPath,string dirPath)
        {
            InitPath(diskPath,bootPath,osPath,dirPath);
            InitDiskInfo();
        }

        //系统路径初始化(由命令行参数指定)
        private static void InitPath(string diskPath, string bootPath, string osPath, string dirPath)
        {
            DiskPath = diskPath;
            BootPath = bootPath;
            OsPath = osPath;
            ProgramDirPath = dirPath;
        }

        //磁盘信息相关参数初始化(由配置文件disk_info.config读入)
        private static void InitDiskInfo()
        {
            Dictionary<string, int> diskInfoHash = DiskInfo.GetDiskInfo();

            Cylinders = diskInfoHash[DiskInfo.CYLINDERS];
            Heads = diskInfoHash[DiskInfo.HEADS];
            Spt = diskInfoHash[DiskInfo.SPT];
            BootAndKernelSize = diskInfoHash[DiskInfo.BOOTANDKERNELSIZE];
            SuperBlockSize = diskInfoHash[DiskInfo.SUPERBLOCKSIZE];
            InodeSize = diskInfoHash[DiskInfo.INODESIZE];
            SwapSize = diskInfoHash[DiskInfo.SWAPSIZE];
        }

        /// <summary>
        /// 得到数据区大小，以块（512B）为单位(不包括启动区，内核区,超级块区，inode区,交换区) 
        /// </summary>
        /// <returns></returns>
        public static int GetDataSize()
        {
            return Cylinders*Heads*Spt - BootAndKernelSize - SuperBlockSize - InodeSize - SwapSize;
         }

        /// <summary>
        /// 得到数据区其实地址，以块（512B）为单位
        /// </summary>
        /// <returns></returns>
        public static int GetDataStartAddr()
        {
            return BootAndKernelSize + SuperBlockSize + InodeSize;
        }
    }
}
