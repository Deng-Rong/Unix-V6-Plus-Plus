using System;

namespace Build
{
	/// <summary>
	/// commonfile 的摘要说明。
	/// </summary>
	public class CommonFile : File
	{
		/// <summary>
		/// 要写入的源文件
		/// </summary>
		protected RWFiles _origFile;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tsb"></param>
        /// <param name="tid"></param>
        /// <param name="tdb"></param>
        /// <param name="disk"></param>
        /// <param name="createFilePath"></param>
        /// <param name="originFilePath"></param>
		public CommonFile(Superblock tsb,InodeBlock tid,DataBlock tdb,Disk disk,string createFilePath,string originFilePath):base(tsb,tid,tdb,disk,createFilePath)
		{
			_origFile = new RWFiles(originFilePath);
		}

		/// <summary>
		/// 设置普通文件的inode
		/// </summary>
		protected override void SetCommonFileInode()
		{
			base.SetCommonFileInode();
			_fileInode._i_mode =(uint) (File.IALLOC | IREAD | IWRITE | IEXEC | (IREAD >> 3) | (IWRITE >> 3) | (IEXEC >> 3) | (IREAD >> 6) | (IWRITE >> 6) | (IEXEC>>6));

            if (_origFile.FileName.Length != 0)
            {
                if (!_origFile.OpenFile())
                {
                    Error.ErrorType = Error.ORIGFILEERROR;
                    return;
                }
                _fileInode._i_size = (int)(_origFile.ReturnFileLength());
                //写文件
                WriteFileItem();
                _origFile.CloseFile();
            }
		}

        /// <summary>
        /// 写入文件项
        /// </summary>
        private void WriteFileItem()
		{
			int blockNum,i;
			byte[] readFrom = new byte[512];

            int fileLength = (int)_origFile.ReturnFileLength();
		    blockNum = fileLength/512 + ((fileLength%512 == 0) ? 0 : 1);

			_origFile.SeekFilePosition(0,System.IO.SeekOrigin.Begin);
			for(i = 0;i < blockNum-1;++i)
			{
				_origFile.ReadFile(ref readFrom,0,512);
				WriteStream(readFrom,i*512,512,_fileInode);
			}
            _origFile.ReadFile(ref readFrom, 0, _fileInode._i_size%512);
            WriteStream(readFrom, i * 512, _fileInode._i_size%512 , _fileInode);
		}
	}
}
