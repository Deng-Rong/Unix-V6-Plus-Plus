using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Build
{
    class Run
    {
        //磁盘文件c.img
        private Disk _disk;
        //用于写bootloader
        private Boot _boot;
        //用于写kernel
        private Kernel _kernel;
        //超级块
        private Superblock _superBlock;
        //inode区
        private InodeBlock _inodeBlock;
        //数据区
        private DataBlock _dataBlock;

        public Run()
        {
            _disk = new Disk();
            _boot = new Boot(_disk);
            _kernel = new Kernel(_disk);

            _superBlock = new Superblock(_disk);
            _inodeBlock = new InodeBlock(_superBlock,_disk);
            _dataBlock = new DataBlock(_superBlock, _disk);

        }

        public void Begin()
        {
            _disk.Init();

            _boot.WriteBootBlock();
            _kernel.WriteKernelBlock();

            _inodeBlock.initInodeManager();
            _dataBlock.InitDataBlock();

            ConstructDirectoryStruct();
        }

        /// <summary>
        /// 构建目录结构（包括根目录，设备文件目录以及其他一些目录等等）
        /// </summary>
        private void ConstructDirectoryStruct()
        { 
            DirFile rootDir = new DirFile(_superBlock, _inodeBlock, _dataBlock, _disk, "");
            rootDir.CreateDirectory();

            DirFile devDir = new DirFile (_superBlock, _inodeBlock, _dataBlock, _disk, "/dev");
            devDir.CreateDirectory();

            DevFile ttyDevFile = new DevFile(_superBlock, _inodeBlock, _dataBlock, _disk, "/dev/tty1","tty");
            ttyDevFile .CreateFile();

            WriteWholeDir();
        }

        /// <summary>
        /// 遍历MachinePara.ProgramDirPath所指向的目录，将其目录结构复制到根目录"/"下
        /// </summary>
        private void WriteWholeDir()
        {
            Queue<string> dirQueue = new Queue<string>();
            dirQueue.Enqueue("/");

            while (dirQueue.Count != 0)
            {
                //要在V6++系统中创建文件的路径
                string dirV6Path = dirQueue.Dequeue();
                //实际文件的路径
                string dirRealPath = MachinePara.ProgramDirPath + dirV6Path;
                DirectoryInfo theFolder = new DirectoryInfo(dirRealPath);

                //创建文件
                foreach (FileInfo file in theFolder.GetFiles())
                {
                    string createFilePath = dirV6Path + "/"+file.Name;
                    CommonFile commonFile = new CommonFile(_superBlock, _inodeBlock, _dataBlock, _disk, createFilePath, file.FullName);
                    commonFile.CreateFile();
                }
                //创建文件夹
                foreach (DirectoryInfo directory in theFolder.GetDirectories())
                {
                    if ((directory.Attributes & FileAttributes.Hidden) != 0)
                        continue;

                    string dirPath = dirV6Path + "/"+directory.Name;
                    DirFile dirFile = new DirFile(_superBlock, _inodeBlock, _dataBlock, _disk, dirPath);
                    dirFile.CreateDirectory();
                    dirQueue.Enqueue(dirPath);
                }
            }
        }
    }
}
