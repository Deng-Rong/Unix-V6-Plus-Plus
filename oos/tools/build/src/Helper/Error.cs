using System;

namespace Build
{
	/// <summary>
	/// error 的摘要说明。
	/// </summary>
	public class Error
	{
		public static int ErrorType = 0;

		public const int SUCCESS = 0;
		/// <summary>
		/// 空闲块消耗完
		/// </summary>
		public const int DATABLOCKOVERFLOW = 1;
		/// <summary>
		/// inode消耗完
		/// </summary>
		public const int INODEOVERFLOW = 2;
		/// <summary>
		/// 写入文件时，打开源文件错误
		/// </summary>
		public const int ORIGFILEERROR = 3;
		/// <summary>
		/// 搜索目录时，没有搜索到目录项
		/// </summary>
		public const int SEARCHDIR = 4;
		/// <summary>
		/// 同一目录下的文件名相同
		/// </summary>
		public const int SAMEFILENAME = 5;

		public Error()
		{

		}
	}
}
