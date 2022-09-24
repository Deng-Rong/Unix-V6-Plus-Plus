using System;

namespace Build
{
	/// <summary>
	/// dir 的摘要说明。
	/// </summary>
	public class DirFile : File
	{
        //规定根目录只能创建一次
        private static bool RootCreated = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tsb"></param>
        /// <param name="tid"></param>
        /// <param name="tdb"></param>
        /// <param name="disk"></param>
        /// <param name="createDirPath"></param>
		public DirFile(Superblock tsb,InodeBlock tid,DataBlock tdb,Disk disk,string createDirPath):base(tsb,tid,tdb,disk,createDirPath)
		{}

        /// <summary>
        /// 设置文件Inode
        /// </summary>
		protected override void SetCommonFileInode()
		{
			base.SetCommonFileInode ();
			_fileInode._i_mode = (uint)(File.IALLOC | IFDIR | IREAD | IWRITE | IEXEC | (IREAD >> 3) | (IWRITE >> 3) | (IEXEC >> 3) | (IREAD >> 6) | (IWRITE >> 6) | (IEXEC>>6));
		}

        /// <summary>
        /// 创建目录
        /// </summary>
        public void CreateDirectory()
        {
            if (_createFilePath != "")
                CreateFile();
            //根目录创建
            else if (!RootCreated)
            {
                /*由于根目录总是第一个被创建的目录，
                 *而受管理的inode数组在初始化的时候第一个成员被设置为了0，
                  所以等于直接就将0号inode分给了根目录*/
                _fileInodeNo = _inodeblock.FetchFreeInode();
                //写新建的inode，并写入磁盘
                SetCommonFileInode();
                _inodeblock.CleanInodeAddr(_fileInode);
                _inodeblock.UpdateInodeToDisk(_fileInode, _fileInodeNo);
                RootCreated = true;
            }
            else
            {
                Console.WriteLine("Root Created!");
            }
        }
	}
}
