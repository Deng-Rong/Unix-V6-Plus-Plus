/*
 * 文件读写
 */
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Build
{
    /// <summary>
    /// RWFiles对特定的一个文件进行操作
    /// </summary>
    public class RWFiles
    {
        /// <summary>
        /// 打开的文件名,包括路径
        /// </summary>
        protected string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// 文件流
        /// </summary>
        private FileStream _streamFile;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileName"></param>
        public RWFiles(string fileName)
        {
            this._fileName = fileName;
        }

        /// 打开或者创建要操作的文件
        /// </summary>
        /// <param name="tfileName">要操作的文件路径</param>
        /// <returns>是否成功</returns>
        public bool OpenFile()
        {
            try
            {
                _streamFile = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 关闭所操作的文件
        /// </summary>
        /// <returns>关闭是否成功</returns>
        public bool CloseFile()
        {
            try
            {
                _streamFile.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="tarray">要写入的字节流</param>
        /// <param name="offset">tarray的起写位置</param>
        /// <param name="count">最多写入多少字节</param>
        /// <returns>是否写入流成功</returns>
        public bool WriteFile(ref byte[] tarray, int offset, int count)
        {
            try
            {
                _streamFile.Write(tarray, offset, count);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="tarray">读到的字节流</param>
        /// <param name="offset">tarray的起存位置</param>
        /// <param name="count">最多读取多少字节</param>
        /// <returns>实际读取多少字节</returns>
        public int ReadFile(ref byte[] tarray, int offset, int count)
        {
            try
            {
                return _streamFile.Read(tarray, offset, count);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 文件定位
        /// </summary>
        /// <param name="toffset">定位位置</param>
        /// <param name="seekOrigin">参考位置</param>
        public long SeekFilePosition(long toffset, SeekOrigin seekOrigin)
        {
            try
            {
                return _streamFile.Seek(toffset, seekOrigin);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 装换块和块内地址，转换成流中位置
        /// </summary>
        /// <param name="blocknum"></param>
        /// <param name="blkoffset"></param>
        /// <returns></returns>
        public long ConvertPosition(int blocknum, int blkoffset)
        {
            return Convert.ToInt64(512 * blocknum + blkoffset);
        }

        /// <summary>
        /// 返回文件长度
        /// </summary>
        /// <returns></returns>
        public long ReturnFileLength()
        {
            return _streamFile.Length;
        }

        /// <summary>
        /// 设定流的长度为虚拟磁盘的大小
        /// </summary>
        public void SetDiskLength(int length)
        {
            _streamFile.SetLength(length);
        }

        /// <summary>
        /// 使数据清零
        /// </summary>
        public void CleanDiskFile()
        {
            byte[] b = new byte[_streamFile.Length];
            this.WriteFile(ref b, 0, (int)_streamFile.Length);
        }
    }
}
