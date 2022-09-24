using System;


namespace Build
{
	/// <summary>
	/// file 的摘要说明。
	/// </summary>
	public class File
    {
        #region 常量
        public static  int CREATFILE = 1;
		public static  int DELETEFILE = 2;
        
		/*******i_mode********/
		public static  uint IALLOC = 0x8000;
		public static  uint IFMT = 0x6000;
		public static  uint IFDIR = 0x4000;
		public static  uint IFCHR = 0x2000;
		public static  uint IFBLK = 0x6000;
		public static  uint IREAD = 0x100;
		public static  uint IWRITE = 0x80;
		public static  uint IEXEC = 0x40;

        #endregion

        /// <summary>
        /// 要在V6中创建的文件路径
        /// </summary>
        protected string _createFilePath;
        /// <summary>
		/// 目录操作
		/// </summary>
		protected Directary _dire;
		/// <summary>
		/// 目录的inode号
		/// </summary>
		protected int _dirInodeNo;
		/// <summary>
		/// 父目录的inode项
		/// </summary>
		protected InodeStr _dirInode;
		/// <summary>
		/// 文件的inode项
		/// </summary>
		protected InodeStr _fileInode;
		/// <summary>
		/// 文件的inode号
		/// </summary>
		protected int _fileInodeNo;
		/// <summary>
		/// 空闲目录项的位置,虚地址
		/// </summary>
		protected int _blankDirItem;
		/// <summary>
		/// 超级块管理
		/// </summary>
		protected Superblock _superBlock;
		/// <summary>
		/// inode管理
		/// </summary>
		protected InodeBlock _inodeblock;
		/// <summary>
		/// 空闲数据块管理
		/// </summary>
		protected DataBlock _dataBlock;
        /// <summary>
        /// 磁盘文件
        /// </summary>
        protected Disk _diskFile;
		/// <summary>
		/// 用于存放由namei解析后的文件名(绝对名字，不含路径，作为ref 参数传给namei)
		/// </summary>
		protected char[] _fileName = new char[28];

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tsb"></param>
        /// <param name="tid"></param>
        /// <param name="tdb"></param>
        /// <param name="diskInfo"></param>
        /// <param name="createFilePath"></param>
		public File(Superblock tsb,InodeBlock tid,DataBlock tdb,Disk diskInfo,string createFilePath)
		{
			_superBlock = tsb;
			_inodeblock = tid;
			_dataBlock = tdb;
            _diskFile = diskInfo;
			_dire = new Directary(tsb,tid,tdb,diskInfo);
			_fileInode = new InodeStr();
			_dirInode = new InodeStr();
            _createFilePath = createFilePath;
		}

        /// <summary>
        /// 根据路径名，将文件以及相关信息写入
        /// </summary>
		public virtual void CreateFile()
		{
            //获取目录inode
            SearchFatherDir(_createFilePath);
            if (Error.ErrorType != Error.SUCCESS)
            {
                return;
            }
            //填写写目录项
            WriteDirItem(_dirInode);
            //写新建的inode，并写入磁盘
            //_inodeblock.CleanInodeAddr(_fileInode);
            SetCommonFileInode();
            _inodeblock.UpdateInodeToDisk(_fileInode, _fileInodeNo);
            //更新目录inode并写回磁盘
            UpdateFatherDirInode();
            _inodeblock.UpdateInodeToDisk(_dirInode, _dirInodeNo);
        }

        /// <summary>
        /// 根据路径名在文件系统中进行搜索
        /// </summary>
        /// <param name="path"></param>
		protected void SearchFatherDir(string path)
		{
			_blankDirItem = _dire.NameI(path,ref _dirInodeNo,ref _dirInode,ref _fileInodeNo,ref _fileInode ,ref _fileName,CREATFILE);
   		}

		/// <summary>
		/// 写入目录项
		/// </summary>
		protected void WriteDirItem(InodeStr dirInode)
		{
			byte[] inodeNo;
			byte[] cin = new byte[32];
			inodeNo = Helper.Struct2Bytes(_fileInodeNo);

			for(int i = 0;i < 4;++i)
			{
				cin[i] = inodeNo[i];
			}
            //此处的FileName由之前的namei中得来，指的是文件的绝对名，不带路径
			for(int i = 0;i < 28;++i)
			{
				cin[i + 4] = Convert.ToByte(_fileName[i]);
			}
			WriteStream(cin,_blankDirItem,32,dirInode);
		}

		/// <summary>
		/// 流写入
		/// </summary>
		/// <param name="cin">待写入的流</param>
		/// <param name="position">写入起始位置</param>
		/// <param name="count">需写入的数量</param>
		protected void WriteStream(byte[] cin,int itemNum,int count,InodeStr dinode)
		{
            int pos = _dire.BMap(dinode, itemNum / 512);
            _diskFile.OpenFile();
			_diskFile.SeekFilePosition(pos * 512 + itemNum % 512,System.IO.SeekOrigin.Begin);
			_diskFile.WriteFile(ref cin,0,count);
            _diskFile.CloseFile();
		}

        /// <summary>
        /// 设置文件的inode内容
        /// </summary>
		protected virtual void SetCommonFileInode()
		{
			_fileInode._i_ilink = 1;
			_fileInode._i_gid = 0;
			_fileInode._i_uid = 0;
			_fileInode._i_atime = Helper.GetCurrentTime();
            _fileInode._i_mtime = Helper.GetCurrentTime();
			_fileInode._i_size = 0;
		}

        /// <summary>
        /// 更新父目录的inode信息
        /// </summary>
		public void UpdateFatherDirInode()
		{
			_dirInode._i_size += 32;
            _dirInode._i_atime = Helper.GetCurrentTime();
            _dirInode._i_mtime = Helper.GetCurrentTime();
		}
	}
}
